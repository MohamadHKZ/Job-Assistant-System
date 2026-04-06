from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List
import httpx
import logging
import os

logging.basicConfig(level=logging.INFO)

# ---------------------------------------------------------
# App
# ---------------------------------------------------------
app = FastAPI(title="NLP & Embedding Orchestrator")

NLP_SERVICE_URL = os.getenv("NLP_SERVICE_URL")
EMBEDDING_SERVICE_URL = os.getenv("EMBEDDING_SERVICE_URL")


# ---------------------------------------------------------
# NLP output schema
# ---------------------------------------------------------
class Embedding_Entity(BaseModel):
    id: int = 0
    technical_skills: List[str] = []
    job_position_skills: List[str] = []
    field_skills: List[str] = []
    job_title: List[str] = []
    soft_skills: List[str] = []


# ---------------------------------------------------------
# Embedding output schema
# ---------------------------------------------------------
class SkillEmbedding(BaseModel):
    skill: str
    vector: List[float]


class EmbeddingCategories(BaseModel):
    technical_skills: List[SkillEmbedding] = []
    job_position_skills: List[SkillEmbedding] = []
    field_skills: List[SkillEmbedding] = []
    job_title: List[SkillEmbedding] = []
    soft_skills: List[SkillEmbedding] = []


class ObjectEmbeddings(BaseModel):
    id: int = 0
    embeddings: EmbeddingCategories


# ---------------------------------------------------------
# Orchestrator request / response
# ---------------------------------------------------------
class Request(BaseModel):
    prompt: str


class UnifiedResponseItem(BaseModel):
    entity: object
    embeddings: ObjectEmbeddings


class Response(BaseModel):
    response: List[UnifiedResponseItem]


# ---------------------------------------------------------
# Orchestration logic
# ---------------------------------------------------------
async def call_nlp_service(prompt: str) -> object:
    try:
        async with httpx.AsyncClient() as client:
            res = await client.post(
                NLP_SERVICE_URL,
                json={"prompt": prompt},
                timeout=httpx.Timeout(3600.0)
            )
            res.raise_for_status()
            return res.json()
    except httpx.HTTPStatusError as e:
        logging.error(f"NLP service returned an error: {e.response.status_code} - {e.response.text}")
        raise HTTPException(status_code=502, detail="NLP service returned an error")
    except httpx.RequestError as e:
        logging.error(f"Failed to reach NLP service: {e}")
        raise HTTPException(status_code=503, detail="NLP service is unreachable")


async def call_embedding_service(
    entities: List[Embedding_Entity]
) -> List[ObjectEmbeddings]:
    try:
        async with httpx.AsyncClient() as client:
            res = await client.post(
                EMBEDDING_SERVICE_URL,
                json=[entity.model_dump() for entity in entities],
                timeout=httpx.Timeout(3600.0)
            )
            res.raise_for_status()

            return [
                ObjectEmbeddings(**item)
                for item in res.json()
            ]
    except httpx.HTTPStatusError as e:
        logging.error(f"Embedding service returned an error: {e.response.status_code} - {e.response.text}")
        raise HTTPException(status_code=502, detail="Embedding service returned an error")
    except httpx.RequestError as e:
        logging.error(f"Failed to reach embedding service: {e}")
        raise HTTPException(status_code=503, detail="Embedding service is unreachable")


# ---------------------------------------------------------
# API endpoint
# ---------------------------------------------------------
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
    except Exception as e:
        logging.error(f"Unexpected error in /nlp-embed: {e}")
        raise HTTPException(status_code=500, detail="Internal server error")
