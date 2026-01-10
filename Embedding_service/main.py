from fastapi import FastAPI
from pydantic import BaseModel
from typing import List, Dict
from sentence_transformers import SentenceTransformer

# ---------------------------------------------------------
# Load embedding model (load once)
# ---------------------------------------------------------
MODEL_PATH = "/app/models/BAAI/bge-large-en-v1.5"
model = SentenceTransformer(MODEL_PATH)

app = FastAPI(title="Job Embedding Service")


# ---------------------------------------------------------
# Input schema (validation)
# ---------------------------------------------------------
class Embedding_Entity(BaseModel):
    id: int
    technical_skills: List[str] = []
    job_position_skills: List[str] = []
    field_skills: List[str] = []
    job_title: List[str] = []
    soft_skills: List[str] = []


# ---------------------------------------------------------
# Embedding logic (unchanged)
# ---------------------------------------------------------
def embed_jobs(jobs):
    skill_fields = [
        "technical_skills",
        "job_position_skills",
        "field_skills",
        "job_title",
        "soft_skills"
    ]

    texts = []
    index_map = []

    for job_idx, job in enumerate(jobs):
        for field in skill_fields:
            for skill in getattr(job, field):
                texts.append(skill)
                index_map.append((job_idx, field, skill))

    vectors = model.encode(
        texts,
        batch_size=128,
        show_progress_bar=False
    )

    result = [
        {
            "id": job.id,
            "embeddings": {field: [] for field in skill_fields}
        }
        for job in jobs
    ]

    for (job_idx, field, skill), vector in zip(index_map, vectors):
        result[job_idx]["embeddings"][field].append(
            {"skill": skill, "vector": vector.tolist()}
        )

    return result


# ---------------------------------------------------------
# API endpoint
# ---------------------------------------------------------
@app.post("/embed/jobs")
def embed_jobs_endpoint(jobs: List[Embedding_Entity]):
    return embed_jobs(jobs)
