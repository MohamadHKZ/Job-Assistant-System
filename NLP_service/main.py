import contextvars
import json
import logging
import os
import time
import uuid
from contextvars import ContextVar
from typing import Any, List

import requests
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
_trace_id_filter = _TraceIdFilter()
for _handler in logging.root.handlers:
    _handler.addFilter(_trace_id_filter)
logger = logging.getLogger(__name__)

# ---------------------------------------------------------
LLM_PROVIDER = os.getenv("LLM_PROVIDER", "openrouter").lower()

OPENROUTER_API_KEY = os.getenv("OPENROUTER_API_KEY")
OPENROUTER_MODEL = os.getenv("OPENROUTER_MODEL")
OPENROUTER_URL = os.getenv("OPENROUTER_URL", "https://openrouter.ai/api/v1/chat/completions")

LM_STUDIO_URL = os.getenv("LM_STUDIO_URL", "http://host.docker.internal:1234/v1/chat/completions")
LM_STUDIO_MODEL = os.getenv("LM_STUDIO_MODEL", "local-model")

if LLM_PROVIDER == "openrouter" and not OPENROUTER_API_KEY:
    raise RuntimeError("OPENROUTER_API_KEY environment variable is not set")

logger.info(
    "NLP service startup provider=%s model=%s",
    LLM_PROVIDER,
    OPENROUTER_MODEL if LLM_PROVIDER != "lmstudio" else LM_STUDIO_MODEL,
)

app = FastAPI(title="NLP Service")
app.add_middleware(TraceIdMiddleware)


class PromptRequest(BaseModel):
    prompt: str


class PromptResponse(BaseModel):
    response: object


def strip_json_fences(text: str) -> str:
    text = text.strip()
    if text.startswith("```"):
        text = text.split("json")[1]
        text = text.split("```")[0]
    return text.strip()


def _truncate500(s: str) -> str:
    if len(s) <= 500:
        return s
    return s[:500] + "..."


def _build_openrouter_request(prompt: str):
    url = OPENROUTER_URL
    headers = {
        "Authorization": f"Bearer {OPENROUTER_API_KEY}",
        "X-Title": "Job Assistant System NLP Service",
        "Content-Type": "application/json",
    }
    payload = {
        "model": OPENROUTER_MODEL,
        "messages": [{"role": "user", "content": prompt}],
    }
    return url, headers, payload


def _build_lmstudio_request(prompt: str):
    url = LM_STUDIO_URL
    headers = {"Content-Type": "application/json"}
    payload = {
        "model": LM_STUDIO_MODEL,
        "messages": [{"role": "user", "content": prompt}],
    }
    return url, headers, payload


def call_llm(prompt: str):
    if LLM_PROVIDER == "lmstudio":
        url, headers, payload = _build_lmstudio_request(prompt)
        provider_label = "LM Studio"
    else:
        url, headers, payload = _build_openrouter_request(prompt)
        provider_label = "OpenRouter"

    t0 = time.perf_counter()
    try:
        response = requests.post(
            url,
            headers=headers,
            data=json.dumps(payload),
            timeout=3600.0,
        )
        response.raise_for_status()
    except requests.HTTPError as e:
        body = e.response.text or ""
        safe = _truncate500(body)
        logger.error(
            "%s returned %s for prompt_len=%d: %s",
            provider_label,
            e.response.status_code,
            len(prompt),
            safe,
        )
        if logger.isEnabledFor(logging.DEBUG) and len(body) > 500:
            logger.debug("Full %s error body: %s", provider_label, body)
        raise HTTPException(status_code=502, detail="LLM provider returned an error") from e
    except requests.RequestException as e:
        logger.error("Failed to reach %s: %s", provider_label, e)
        raise HTTPException(status_code=503, detail="LLM provider is unreachable") from e

    elapsed_ms = int((time.perf_counter() - t0) * 1000)
    result = response.json()
    raw_content = result["choices"][0]["message"]["content"]
    raw_content = strip_json_fences(raw_content)

    logger.info(
        "llm_ask provider=%s prompt_len=%d response_len=%d elapsed_ms=%d",
        provider_label,
        len(prompt),
        len(raw_content),
        elapsed_ms,
    )

    try:
        return json.loads(raw_content)
    except json.JSONDecodeError:
        safe = _truncate500(raw_content)
        logger.error("LLM returned invalid JSON (len=%d): %s", len(raw_content), safe)
        logger.debug("Full LLM raw response: %s", raw_content)
        raise HTTPException(
            status_code=500,
            detail="LLM did not return valid JSON",
        ) from None


@app.post("/llm/ask", response_model=PromptResponse)
def ask_llm(payload: PromptRequest):
    try:
        answer = call_llm(payload.prompt)
        return {"response": answer}
    except HTTPException:
        raise
    except Exception:
        logger.exception("Unexpected error in /llm/ask")
        raise HTTPException(status_code=500, detail="Internal server error") from None
