from typing import Any, List
from fastapi import HTTPException
import logging
import os
from pyparsing import Dict, Union
import requests
import json
from fastapi import FastAPI
from pydantic import BaseModel

logging.basicConfig(level=logging.INFO)

# ---------------------------------------------------------
# CONFIG (FROM ENV)
# ---------------------------------------------------------
# Set LLM_PROVIDER to either "openrouter" (default) or "lmstudio"
LLM_PROVIDER = os.getenv("LLM_PROVIDER", "openrouter").lower()

OPENROUTER_API_KEY = os.getenv("OPENROUTER_API_KEY")
OPENROUTER_MODEL   = os.getenv("OPENROUTER_MODEL")
OPENROUTER_URL     = os.getenv("OPENROUTER_URL", "https://openrouter.ai/api/v1/chat/completions")

LM_STUDIO_URL      = os.getenv("LM_STUDIO_URL", "http://host.docker.internal:1234/v1/chat/completions")
LM_STUDIO_MODEL    = os.getenv("LM_STUDIO_MODEL", "local-model")

if LLM_PROVIDER == "openrouter" and not OPENROUTER_API_KEY:
    raise RuntimeError("OPENROUTER_API_KEY environment variable is not set")

logging.info(f"LLM provider: {LLM_PROVIDER}")

# ---------------------------------------------------------
# FASTAPI APP
# ---------------------------------------------------------
app = FastAPI(title="NLP Service")

# ---------------------------------------------------------
# REQUEST / RESPONSE MODELS
# ---------------------------------------------------------
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


# ---------------------------------------------------------
# PROVIDER IMPLEMENTATIONS
# ---------------------------------------------------------
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


# ---------------------------------------------------------
# LLM CALL FUNCTION
# ---------------------------------------------------------
def call_llm(prompt: str):
    if LLM_PROVIDER == "lmstudio":
        url, headers, payload = _build_lmstudio_request(prompt)
        provider_label = "LM Studio"
    else:
        url, headers, payload = _build_openrouter_request(prompt)
        provider_label = "OpenRouter"

    try:
        response = requests.post(
            url,
            headers=headers,
            data=json.dumps(payload),
            timeout=3600.0,
        )
        response.raise_for_status()
    except requests.HTTPError as e:
        logging.error(f"{provider_label} returned an error: {e.response.status_code} - {e.response.text}")
        raise HTTPException(status_code=502, detail="LLM provider returned an error")
    except requests.RequestException as e:
        logging.error(f"Failed to reach {provider_label}: {e}")
        raise HTTPException(status_code=503, detail="LLM provider is unreachable")

    result = response.json()
    raw_content = result["choices"][0]["message"]["content"]
    raw_content = strip_json_fences(raw_content)

    try:
        return json.loads(raw_content)
    except json.JSONDecodeError:
        logging.error(f"LLM returned invalid JSON:\n{raw_content}")
        raise HTTPException(
            status_code=500,
            detail=f"LLM did not return valid JSON\n\n{raw_content}",
        )


# ---------------------------------------------------------
# API ENDPOINT
# ---------------------------------------------------------
@app.post("/llm/ask", response_model=PromptResponse)
def ask_llm(payload: PromptRequest):
    try:
        answer = call_llm(payload.prompt)
        return {"response": answer}
    except HTTPException:
        raise
    except Exception as e:
        logging.error(f"Unexpected error in /llm/ask: {e}")
        raise HTTPException(status_code=500, detail="Internal server error")
