from typing import Any, List
from fastapi import HTTPException
import os
from pyparsing import Dict, Union
import requests
import json
from fastapi import FastAPI
from pydantic import BaseModel

# ---------------------------------------------------------
# CONFIG (FROM ENV)
# ---------------------------------------------------------
API_KEY = os.getenv("OPENROUTER_API_KEY")
MODEL_NAME = os.getenv("OPENROUTER_MODEL")
OPENROUTER_URL = "https://openrouter.ai/api/v1/chat/completions"

if not API_KEY:
    raise RuntimeError("OPENROUTER_API_KEY environment variable is not set")

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
# LLM CALL FUNCTION
# ---------------------------------------------------------
def call_llm(prompt: str) -> PromptResponse:
    headers = {
        "Authorization": f"Bearer {API_KEY}",
        "X-Title": "Job Assistant System NLP Service",
        "Content-Type": "application/json",
    }

    payload = {
        "model": MODEL_NAME,
        "messages": [
            {"role": "user", "content": prompt}
        ]
    }

    response = requests.post(
        OPENROUTER_URL,
        headers=headers,
        data=json.dumps(payload),
        timeout=30
    )
    response.raise_for_status()

    result = response.json()
    raw_content = result["choices"][0]["message"]["content"]
    raw_content = strip_json_fences(raw_content)
    # Parse model output into JSON
    try:
        return json.loads(raw_content)
    except json.JSONDecodeError:
        raise HTTPException(
            status_code=500,
            detail=f"LLM did not return valid JSON\n\n{raw_content}"
        )


# ---------------------------------------------------------
# API ENDPOINT
# ---------------------------------------------------------
@app.post("/llm/ask", response_model=PromptResponse)
def ask_llm(payload: PromptRequest):
    answer = call_llm(payload.prompt)
    return {"response": answer}
