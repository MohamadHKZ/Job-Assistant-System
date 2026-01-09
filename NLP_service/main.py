import os
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
app = FastAPI(title="LLM Prompt Service")

# ---------------------------------------------------------
# REQUEST / RESPONSE MODELS
# ---------------------------------------------------------
class PromptRequest(BaseModel):
    prompt: str


class PromptResponse(BaseModel):
    response: str


# ---------------------------------------------------------
# LLM CALL FUNCTION
# ---------------------------------------------------------
def call_llm(prompt: str) -> str:
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
    return result["choices"][0]["message"]["content"]


# ---------------------------------------------------------
# API ENDPOINT
# ---------------------------------------------------------
@app.post("/llm/ask", response_model=PromptResponse)
def ask_llm(payload: PromptRequest):
    answer = call_llm(payload.prompt)
    return {"response": answer}
