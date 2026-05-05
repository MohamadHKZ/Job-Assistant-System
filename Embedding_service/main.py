import contextvars
import logging
import os
import time
import uuid
from contextvars import ContextVar
from typing import Dict, List

from fastapi import FastAPI
from pydantic import BaseModel
from sentence_transformers import SentenceTransformer
from starlette.middleware.base import BaseHTTPMiddleware
from starlette.requests import Request

# ---------------------------------------------------------------------------
trace_id_ctx: ContextVar[str] = ContextVar("trace_id", default="-")


class _TraceIdFilter(logging.Filter):
    def filter(self, record: logging.LogRecord) -> bool:        
        record.trace_id = trace_id_ctx.get("-")
        return True


class TraceIdMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next):
        tid = request.headers.get("x-trace-id") or uuid.uuid4().hex[:16]
        token = trace_id_ctx.set(tid)
        try:
            response = await call_next(request)
        finally:
            trace_id_ctx.reset(token)
        response.headers["X-Trace-Id"] = tid
        return response


LOG_LEVEL = os.getenv("LOG_LEVEL", "INFO").upper()
logging.basicConfig(
    level=LOG_LEVEL,
    format="%(asctime)s [%(levelname)s] [trace=%(trace_id)s] %(name)s: %(message)s",
    datefmt="%Y-%m-%dT%H:%M:%S",
)
# Child loggers do not run the root logger's filters; attach to handlers so format sees trace_id.
_trace_id_filter = _TraceIdFilter()
for _handler in logging.root.handlers:
    _handler.addFilter(_trace_id_filter)
logger = logging.getLogger(__name__)

# ---------------------------------------------------------
MODEL_PATH = "/app/models/BAAI/bge-large-en-v1.5"

logger.info("Loading embedding model from %s", MODEL_PATH)
_load_t0 = time.perf_counter()
model = SentenceTransformer(MODEL_PATH)
logger.info(
    "Model loaded: dim=%s in %.2fs",
    model.get_sentence_embedding_dimension(),
    time.perf_counter() - _load_t0,
)

app = FastAPI(title="Job Embedding Service")
app.add_middleware(TraceIdMiddleware)


class Embedding_Entity(BaseModel):
    id: int
    technical_skills: List[str] = []
    job_position_skills: List[str] = []
    field_skills: List[str] = []
    job_title: List[str] = []
    soft_skills: List[str] = []
    technologies: List[str] = []


SKILL_FIELDS = [
    "technical_skills",
    "job_position_skills",
    "field_skills",
    "job_title",
    "soft_skills",
    "technologies",
]


def _count_skills(jobs: List[Embedding_Entity]) -> int:
    total = 0
    for job in jobs:
        for field in SKILL_FIELDS:
            total += len(getattr(job, field))
    return total


def embed_jobs(jobs):
    texts = []
    index_map = []

    for job_idx, job in enumerate(jobs):
        for field in SKILL_FIELDS:
            for skill in getattr(job, field):
                texts.append(skill)
                index_map.append((job_idx, field, skill))

    vectors = model.encode(
        texts,
        batch_size=128,
        show_progress_bar=False,
    )

    result = [
        {
            "id": job.id,
            "embeddings": {field: [] for field in SKILL_FIELDS},
        }
        for job in jobs
    ]

    for (job_idx, field, skill), vector in zip(index_map, vectors):
        result[job_idx]["embeddings"][field].append(
            {"skill": skill, "vector": vector.tolist()},
        )

    return result


@app.post("/embed/jobs")
def embed_jobs_endpoint(jobs: List[Embedding_Entity]):
    n_skills = _count_skills(jobs)
    t0 = time.perf_counter()
    try:
        out = embed_jobs(jobs)
    except Exception:
        logger.exception("embed_jobs failed n_jobs=%d", len(jobs))
        raise
    elapsed_ms = int((time.perf_counter() - t0) * 1000)
    logger.info(
        "embed_jobs n_jobs=%d n_skills=%d elapsed_ms=%d",
        len(jobs),
        n_skills,
        elapsed_ms,
    )
    return out
