from fastapi import FastAPI
from pydantic import BaseModel
from typing import List
import numpy as np


# ---------------------------------------------------------
# FastAPI app
# ---------------------------------------------------------
app = FastAPI(title="Job Matching Service")



class SkillEmbedding(BaseModel):
    skill: str
    vector: List[float]


class EmbeddingCategories(BaseModel):
    technical_skills: List[SkillEmbedding] = []
    job_position_skills: List[SkillEmbedding] = []
    field_skills: List[SkillEmbedding] = []
    job_title: List[SkillEmbedding] = []
    soft_skills: List[SkillEmbedding] = []


class EmbeddingEntity(BaseModel):
    id: int
    embeddings: EmbeddingCategories


class MatchRequest(BaseModel):
    job_embeddings: List[EmbeddingEntity]
    profile_embedding: EmbeddingEntity


# ---------------------------------------------------------
# Cosine similarity
# ---------------------------------------------------------
def cosine_similarity(a, b):
    return np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b))


# ---------------------------------------------------------
# Category matcher
# ---------------------------------------------------------
def array_match(cv_items, job_items, weight):
    if len(job_items) == 0:
        return weight

    cv_vecs = np.array([item.vector for item in cv_items])
    job_vecs = np.array([item.vector for item in job_items])

    n = len(job_vecs)
    per_job_item_weight = weight / n
    total = 0.0

    for job_vec in job_vecs:
        best_similarity = 0.0

        for cv_vec in cv_vecs:
            sim = cosine_similarity(cv_vec, job_vec)
            best_similarity = max(best_similarity, sim)

        if best_similarity >= 0.65:
            total += per_job_item_weight * best_similarity

    return max(0, min(total, weight))


# ---------------------------------------------------------
# Weights
# ---------------------------------------------------------
weights = {
    "technical_skills": 0.35,
    "job_position_skills": 0.40,
    "field_skills": 0.15,
    "job_title": 0.05,
    "soft_skills": 0.05
}


# ---------------------------------------------------------
# FINAL MATCH (profile ↔ single job)
# ---------------------------------------------------------
def match(profile_embeddings: EmbeddingCategories, job_embeddings: EmbeddingCategories):
    final_score = 0.0

    for category, weight in weights.items():
        contribution = array_match(
            getattr(profile_embeddings, category),
            getattr(job_embeddings, category),
            weight
        )
        final_score += contribution

    return final_score * 100


# ---------------------------------------------------------
# MATCH PROFILE AGAINST ALL JOBS
# ---------------------------------------------------------
def match_profile_to_jobs(
    job_embeddings: List[EmbeddingEntity],
    profile_embedding: EmbeddingEntity
):
    results = []

    for job in job_embeddings:
        score = match(
            profile_embedding.embeddings,
            job.embeddings
        )

        results.append({
            "id": job.id,
            "match_percentage": score
        })

    return results


# ---------------------------------------------------------
# API ENDPOINT
# ---------------------------------------------------------
@app.post("/match/jobs")
def match_jobs_endpoint(payload: MatchRequest):
    return match_profile_to_jobs(
        payload.job_embeddings,
        payload.profile_embedding
    )
