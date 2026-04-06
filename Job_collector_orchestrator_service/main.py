import logging
import json
import asyncio
import time
from os import getenv
import signal
from pathlib import Path
from Job_collector.interfaces.provider import Provider
from Job_collector.providers.linkedin_provider import LinkedInProvider
from Job_collector.data_models.job_model import Job
import psycopg2
import helpers as hp


NLP_EMBEDDING_ORCHESTRATOR_URL = getenv("NLP_EMBEDDING_ORCHESTRATOR_URL") or "http://localhost:5003/nlp-embed"

apify_token = getenv("APIFY_API_TOKEN")
linkedin_actor_id = getenv("LINKEDIN_ACTOR_ID") or "bebity/linkedin-jobs-scraper"
linkedin_active = getenv("LINKEDIN_ACTIVE", "True") == "True"

SCRAPE_TITLE    = getenv("SCRAPE_TITLE", "Software Engineer")
SCRAPE_LOCATION = getenv("SCRAPE_LOCATION", "Jordan")
SCRAPE_ROWS     = int(getenv("SCRAPE_ROWS", "20"))
SCRAPE_PUBLISHED_AT = getenv("SCRAPE_PUBLISHED_AT", "r2592000")

providers : list[Provider] = [LinkedInProvider(apify_token=apify_token, actor_id=linkedin_actor_id) for active in [linkedin_active] if active]
jobs : list[Job] = []

signal.signal(signal.SIGTERM, hp.shutdown)
signal.signal(signal.SIGINT, hp.shutdown)

jobs = hp.collect_jobs(
    providers,
    title=SCRAPE_TITLE,
    location=SCRAPE_LOCATION,
    rows=SCRAPE_ROWS,
    published_at=SCRAPE_PUBLISHED_AT,
)

jobs_json_string = json.dumps(
    jobs,
    ensure_ascii=False,
    default=hp._json_serializer,
    indent=2
)

matching_prompt_path = Path(__file__).parent / "prompts" / "matching_object_prompt.txt"
with matching_prompt_path.open("r", encoding="utf-8") as matching_prompt_file:
    matching_prompt_string = matching_prompt_file.read()

refined_jobs = []

BATCH_SIZE = 5
refined_jobs_list = []
total_jobs = len(jobs)
logging.info(f"Refining collected jobs in batches of {BATCH_SIZE}...")

for i in range(0, total_jobs, BATCH_SIZE):
    jobs_batch = jobs[i:i+BATCH_SIZE]
    jobs_json_string_batch = json.dumps(
        jobs_batch,
        ensure_ascii=False,
        default=hp._json_serializer,
        indent=2
    )
    prompt = f"{matching_prompt_string}\n\n{jobs_json_string_batch}"
    payload = hp.Request(prompt=prompt)
    refined_job_batch = asyncio.run(hp.call_nlp_embedding_orchestrator(payload=payload, url=NLP_EMBEDDING_ORCHESTRATOR_URL))
    if hasattr(refined_job_batch, "response"):
        refined_jobs_list.extend(refined_job_batch.response)
    else:
        refined_jobs_list.extend(refined_job_batch)

refined_jobs = hp.Response(response=refined_jobs_list)

jobs_data, refined_job_posts, embeddings, technologies = hp.prepare_data(jobs, refined_jobs)

try:    
    conn = psycopg2.connect(getenv("DATABASE_URL"))

    cur = conn.cursor()
    
    if hp.insert_rows(cur, conn, "JobPosts", jobs_data):
        print("Jobs inserted successfully")
    else:
        logging.error("Error occurred while inserting raw jobs into the database")

    time.sleep(0.5)

    if hp.insert_rows(cur, conn, "NormalizedJobPosts", refined_job_posts):
        print("Refined jobs inserted successfully")
    else:
        logging.error("Error occurred while inserting refined jobs into the database")

    time.sleep(0.5)

    if hp.insert_rows(cur, conn, "EmbeddedJobPosts", embeddings):
        print("Embeddings inserted successfully")
    else:
        logging.error("Error occurred while inserting job embeddings into the database")

    time.sleep(0.5)

    if hp.insert_rows(cur, conn, "TechnicalSkillsRecorded", technologies):
        print("Technologies inserted successfully")
    else:
        logging.error("Error occurred while inserting technologies into the database")

except Exception as e:
    logging.error(f"Error occurred while connecting to the database: {e}")

finally:
    if cur:
        cur.close()
    if conn:
        conn.close()
    print("inserting jobs finished")



