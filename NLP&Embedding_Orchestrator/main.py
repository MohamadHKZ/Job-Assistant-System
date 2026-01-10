from fastapi import FastAPI
from pydantic import BaseModel
from typing import List
import httpx
import os

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
    id: int
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
    id: int
    embeddings: EmbeddingCategories


# ---------------------------------------------------------
# Orchestrator request / response
# ---------------------------------------------------------
class Request(BaseModel):
    prompt: str


class UnifiedResponseItem(BaseModel):
    entity: Embedding_Entity
    embeddings: ObjectEmbeddings


class Response(BaseModel):
    response: List[UnifiedResponseItem]


# ---------------------------------------------------------
# Orchestration logic
# ---------------------------------------------------------
async def call_nlp_service(prompt: str) -> List[Embedding_Entity]:
    async with httpx.AsyncClient() as client:
        res = await client.post(
            NLP_SERVICE_URL,
            json={"prompt": prompt},
            timeout=httpx.Timeout(300.0)
        )
        res.raise_for_status()

        return [
            Embedding_Entity(**item)
            for item in res.json()["response"]
        ]



async def call_embedding_service(
    entities: List[Embedding_Entity]
) -> List[ObjectEmbeddings]:
    async with httpx.AsyncClient() as client:
        res = await client.post(
            EMBEDDING_SERVICE_URL,
            json=[entity.model_dump() for entity in entities],
            timeout=httpx.Timeout(300.0)
        )
        res.raise_for_status()

        return [
            ObjectEmbeddings(**item)
            for item in res.json()
        ]



# ---------------------------------------------------------
# API endpoint
# ---------------------------------------------------------
@app.post("/nlp-embed", response_model=Response)
async def nlp_and_embed(payload: Request):
    nlp_entities = await call_nlp_service(payload.prompt)
    embeddings = await call_embedding_service(nlp_entities)

    unified = [
        UnifiedResponseItem(entity=e, embeddings=emb)
        for e, emb in zip(nlp_entities, embeddings)
    ]

    return Response(response=unified)
