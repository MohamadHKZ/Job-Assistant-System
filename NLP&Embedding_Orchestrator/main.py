import contextvars
import logging
import os
import time
import uuid
from contextvars import ContextVar
from typing import List

import httpx
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from starlette.middleware.base import BaseHTTPMiddleware
from starlette.requests import Request

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
app = FastAPI(title="NLP & Embedding Orchestrator")
app.add_middleware(TraceIdMiddleware)

NLP_SERVICE_URL = os.getenv("NLP_SERVICE_URL")
EMBEDDING_SERVICE_URL = os.getenv("EMBEDDING_SERVICE_URL")


def _truncate500(s: str) -> str:
    if len(s) <= 500:
        return s
    return s[:500] + "..."


class Embedding_Entity(BaseModel):
    id: int = 0
    technical_skills: List[str] = []
    job_position_skills: List[str] = []
    field_skills: List[str] = []
    job_title: List[str] = []
    soft_skills: List[str] = []
    technologies: List[str] = []


class SkillEmbedding(BaseModel):
    skill: str
    vector: List[float]


class EmbeddingCategories(BaseModel):
    technical_skills: List[SkillEmbedding] = []
    job_position_skills: List[SkillEmbedding] = []
    field_skills: List[SkillEmbedding] = []
    job_title: List[SkillEmbedding] = []
    soft_skills: List[SkillEmbedding] = []
    technologies: List[SkillEmbedding] = []


class ObjectEmbeddings(BaseModel):
    id: int = 0
    embeddings: EmbeddingCategories


class Request(BaseModel):
    prompt: str


class UnifiedResponseItem(BaseModel):
    entity: object
    embeddings: ObjectEmbeddings


class Response(BaseModel):
    response: List[UnifiedResponseItem]


async def call_nlp_service(prompt: str) -> object:
    t0 = time.perf_counter()
    try:
        async with httpx.AsyncClient() as client:
            res = await client.post(
                NLP_SERVICE_URL,
                json={"prompt": prompt},
                timeout=httpx.Timeout(3600.0),
            )
            res.raise_for_status()
            body = res.json()
    except httpx.HTTPStatusError as e:
        text = e.response.text or ""
        safe = _truncate500(text)
        logger.error(
            "NLP service returned %s for prompt_len=%d: %s",
            e.response.status_code,
            len(prompt),
            safe,
        )
        if logger.isEnabledFor(logging.DEBUG) and len(text) > 500:
            logger.debug("Full NLP service error body: %s", text)
        raise HTTPException(status_code=502, detail="NLP service returned an error") from e
    except httpx.RequestError as e:
        logger.error("Failed to reach NLP service: %s", e)
        raise HTTPException(status_code=503, detail="NLP service is unreachable") from e

    elapsed_ms = int((time.perf_counter() - t0) * 1000)
    logger.info("call_nlp_service OK prompt_len=%d elapsed_ms=%d", len(prompt), elapsed_ms)
    return body


async def call_embedding_service(
    entities: List[Embedding_Entity],
) -> List[ObjectEmbeddings]:
    t0 = time.perf_counter()
    try:
        async with httpx.AsyncClient() as client:
            res = await client.post(
                EMBEDDING_SERVICE_URL,
                json=[entity.model_dump() for entity in entities],
                timeout=httpx.Timeout(3600.0),
            )
            res.raise_for_status()

            out = [ObjectEmbeddings(**item) for item in res.json()]
    except httpx.HTTPStatusError as e:
        text = e.response.text or ""
        safe = _truncate500(text)
        logger.error(
            "Embedding service returned %s for entities=%d: %s",
            e.response.status_code,
            len(entities),
            safe,
        )
        if logger.isEnabledFor(logging.DEBUG) and len(text) > 500:
            logger.debug("Full embedding service error body: %s", text)
        raise HTTPException(status_code=502, detail="Embedding service returned an error") from e
    except httpx.RequestError as e:
        logger.error("Failed to reach embedding service: %s", e)
        raise HTTPException(status_code=503, detail="Embedding service is unreachable") from e

    elapsed_ms = int((time.perf_counter() - t0) * 1000)
    logger.info(
        "call_embedding_service OK entities=%d elapsed_ms=%d",
        len(entities),
        elapsed_ms,
    )
    return out


@app.post("/nlp-embed", response_model=Response)
async def nlp_and_embed(payload: Request):
    try:
        nlp_response = await call_nlp_service(payload.prompt)
        nlp_entities = [Embedding_Entity(**item) for item in nlp_response["response"]]
        embeddings = await call_embedding_service(nlp_entities)

        unified = [
            UnifiedResponseItem(entity=e, embeddings=emb)
            for e, emb in zip(nlp_response["response"], embeddings)
        ]

        return Response(response=unified)
    except HTTPException:
        raise
    except Exception:
        logger.exception("Unexpected error in /nlp-embed")
        raise HTTPException(status_code=500, detail="Internal server error") from None
