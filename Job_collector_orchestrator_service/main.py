import json
import logging
import os
import signal
import uuid
from pathlib import Path

import asyncio
import psycopg2

import helpers as hp
from Job_collector import LinkedInProvider, Provider
from Trend_analyzer import analyze_trends

LOG_LEVEL = os.getenv("LOG_LEVEL", "INFO").upper()
RUN_ID = uuid.uuid4().hex[:8]
logging.basicConfig(
    level=LOG_LEVEL,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
    datefmt="%Y-%m-%dT%H:%M:%S",
)
logger = logging.getLogger(__name__)


def _log(level: int, msg: str, *args):
    logger.log(level, "[run_id=%s] " + msg, RUN_ID, *args)


NLP_EMBEDDING_ORCHESTRATOR_URL = os.getenv("NLP_EMBEDDING_ORCHESTRATOR_URL") or "http://localhost:5003/nlp-embed"

apify_token = os.getenv("APIFY_API_TOKEN")
linkedin_actor_id = os.getenv("LINKEDIN_ACTOR_ID") or "bebity/linkedin-jobs-scraper"
linkedin_active = os.getenv("LINKEDIN_ACTIVE", "True") == "True"

SCRAPE_TITLE = os.getenv("SCRAPE_TITLE", "Software Engineer")
SCRAPE_LOCATION = os.getenv("SCRAPE_LOCATION", "Jordan")
SCRAPE_ROWS = int(os.getenv("SCRAPE_ROWS", "20"))
SCRAPE_PUBLISHED_AT = os.getenv("SCRAPE_PUBLISHED_AT", "r2592000")

providers: list[Provider] = [
    LinkedInProvider(apify_token=apify_token, actor_id=linkedin_actor_id)
    for active in [linkedin_active]
    if active
]

signal.signal(signal.SIGTERM, hp.shutdown)
signal.signal(signal.SIGINT, hp.shutdown)

_log(logging.INFO, "Starting job collection title=%r location=%r", SCRAPE_TITLE, SCRAPE_LOCATION)
jobs = hp.collect_jobs(
    providers,
    title=SCRAPE_TITLE,
    location=SCRAPE_LOCATION,
    rows=SCRAPE_ROWS,
    published_at=SCRAPE_PUBLISHED_AT,
)

matching_prompt_path = Path(__file__).parent / "prompts" / "matching_object_prompt.txt"
with matching_prompt_path.open("r", encoding="utf-8") as matching_prompt_file:
    matching_prompt_string = matching_prompt_file.read()

refined_jobs_list = []
BATCH_SIZE = 5
total_jobs = len(jobs)
_log(logging.INFO, "Refining collected jobs in batches of %s (total_jobs=%d)", BATCH_SIZE, total_jobs)

for i in range(0, total_jobs, BATCH_SIZE):
    jobs_batch = jobs[i : i + BATCH_SIZE]
    jobs_json_string_batch = json.dumps(
        jobs_batch,
        ensure_ascii=False,
        default=hp._json_serializer,
        indent=2,
    )
    prompt = f"{matching_prompt_string}\n\n{jobs_json_string_batch}"
    payload = hp.Request(prompt=prompt)
    refined_job_batch = asyncio.run(
        hp.call_nlp_embedding_orchestrator(payload=payload, url=NLP_EMBEDDING_ORCHESTRATOR_URL)
    )
    if hasattr(refined_job_batch, "response"):
        refined_jobs_list.extend(refined_job_batch.response)
    else:
        refined_jobs_list.extend(refined_job_batch)

refined_jobs = hp.Response(response=refined_jobs_list)

jobs_data, refined_job_posts, embeddings, technologies = hp.prepare_data(jobs, refined_jobs)

conn = None
cur = None
try:
    conn = psycopg2.connect(os.getenv("DATABASE_URL"))

    cur = conn.cursor()

    insert_specs = [
        ("raw jobs", "JobPosts", jobs_data),
        ("normalized", "NormalizedJobPosts", refined_job_posts),
        ("embeddings", "EmbeddedJobPosts", embeddings),
        ("technologies", "TechnicalSkillsRecorded", technologies),
    ]
    for label, table_name, rows in insert_specs:
        ok = hp.insert_rows(cur, conn, table_name, rows)
        if ok:
            _log(logging.INFO, "Insert %s -> %s rows=%d ok=True", label, table_name, len(rows))
        else:
            _log(logging.ERROR, "Insert %s -> %s rows=%d ok=False", label, table_name, len(rows))

except Exception:
    logger.exception("[run_id=%s] Database phase failed", RUN_ID)
finally:
    if cur is not None:
        cur.close()
    if conn is not None:
        conn.close()
    _log(logging.INFO, "inserting jobs finished")

try:
    analyze_trends(os.getenv("DATABASE_URL"))
    _log(logging.INFO, "Trends table refreshed successfully")
except Exception:
    logger.exception("[run_id=%s] Trends refresh failed", RUN_ID)
