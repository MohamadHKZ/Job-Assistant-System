import logging
import os
import time
import uuid
from contextvars import ContextVar
from typing import List, Optional, Tuple

import numpy as np
from fastapi import FastAPI
from pydantic import BaseModel
from starlette.middleware.base import BaseHTTPMiddleware
from starlette.requests import Request

# ---------------------------------------------------------------------------
# Logging (P0 + trace id in format)
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
# FastAPI app
# ---------------------------------------------------------
app = FastAPI(title="Job Matching Service")
app.add_middleware(TraceIdMiddleware)


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


class MatchingEntity(BaseModel):
    id: int
    title: str
    experience: str
    techonologies: List[str] = []
    embeddings: EmbeddingCategories


class MatchRequest(BaseModel):
    jobs: List[MatchingEntity]
    profile: MatchingEntity


# ---------------------------------------------------------
# Experience scores table
# ---------------------------------------------------------
EXPERIENCE_SCORES = {
    ("junior", "junior"): 1.00,
    ("mid", "mid"): 1.00,
    ("associate", "associate"): 1.00,
    ("senior", "senior"): 1.00,
    ("junior", "mid"): 0.65,
    ("junior", "associate"): 0.65,
    ("mid", "junior"): 0.60,
    ("associate", "junior"): 0.65,
    ("mid", "senior"): 0.70,
    ("associate", "senior"): 0.70,
    ("senior", "mid"): 0.80,
    ("senior", "associate"): 0.80,
    ("junior", "senior"): 0.40,
    ("senior", "junior"): 0.70,
}

weights = {
    "technologies": 0.50,
    "job_position_skills": 0.25,
    "experience": 0.25,
}

MATCH_THRESHOLD = 0.65
TECHNOLOGY_COSINE_MIN_SIMILARITY = 0.80


def cosine_similarity(a, b):
    return np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b))


def clamp(value: float, minimum: float, maximum: float) -> float:
    return max(minimum, min(value, maximum))


def normalize_experience_level(level: str) -> str:
    return level.strip().lower()


def log_matching_header(title: str, weight: float) -> None:
    logger.debug("===== %s (weight=%s) =====", title, weight)


def find_exact_substring_match(cv_items: List[SkillEmbedding], job_skill_lower: str) -> Optional[str]:
    for cv_item in cv_items:
        cv_tech_lower = cv_item.skill.lower()
        logger.debug(
            "Checking exact match: CV Tech %r vs Job Tech %r",
            cv_item.skill,
            job_skill_lower,
        )
        if cv_tech_lower in job_skill_lower or job_skill_lower in cv_tech_lower:
            return cv_item.skill
    return None


def best_cosine_similarity_for_job_item(
    cv_items: List[SkillEmbedding],
    job_vec: np.ndarray,
    cv_item_label: str,
) -> Tuple[float, Optional[str]]:
    best_similarity = 0.0
    best_cv_skill = None

    for cv_item in cv_items:
        cv_vec = np.array(cv_item.vector)
        sim = cosine_similarity(cv_vec, job_vec)
        logger.debug("%s: %r similarity=%.4f", cv_item_label, cv_item.skill, sim)
        if sim > best_similarity:
            best_similarity = sim
            best_cv_skill = cv_item.skill

    return best_similarity, best_cv_skill


def match_experience(cv_level: str, job_level: str, weight: float) -> float:
    cv_level = normalize_experience_level(cv_level)
    job_level = normalize_experience_level(job_level)
    ratio = EXPERIENCE_SCORES.get((cv_level, job_level), 0.0)
    score = ratio * weight

    log_matching_header("EXPERIENCE MATCHING", weight)
    logger.debug("CV level=%r job level=%r ratio=%.2f score=%.4f", cv_level, job_level, ratio, score)

    return score


def array_match_technologies(
    cv_items: List[SkillEmbedding],
    job_items: List[SkillEmbedding],
    weight: float,
) -> float:
    if len(job_items) == 0:
        return weight

    n = len(job_items)
    per_job_item_weight = weight / n
    total = 0.0

    log_matching_header("TECHNOLOGIES MATCHING", weight)

    for job_item in job_items:
        job_skill = job_item.skill
        job_tech_lower = job_skill.lower()
        job_vec = np.array(job_item.vector)

        logger.debug("Job Tech: %r", job_skill)

        exact_match = find_exact_substring_match(cv_items, job_tech_lower)

        if exact_match:
            best_similarity = 1.0
            best_cv_tech = exact_match
            logger.debug("Exact match -> %r similarity=1.0", exact_match)
        else:
            logger.debug("No exact match, falling back to cosine similarity")

            best_similarity, best_cv_tech = best_cosine_similarity_for_job_item(
                cv_items,
                job_vec,
                "CV Tech",
            )

            if best_similarity < TECHNOLOGY_COSINE_MIN_SIMILARITY:
                best_similarity = 0.0

            logger.debug("Best cosine result: %r %.4f", best_cv_tech, best_similarity)

        if best_similarity >= MATCH_THRESHOLD:
            total += per_job_item_weight * best_similarity

    total = clamp(total, 0, weight)
    return total


def array_match(
    cv_items: List[SkillEmbedding],
    job_items: List[SkillEmbedding],
    weight: float,
) -> float:
    if len(job_items) == 0:
        return weight

    n = len(job_items)
    per_job_item_weight = weight / n
    total = 0.0

    log_matching_header("CATEGORY MATCHING", weight)

    for job_item in job_items:
        job_skill = job_item.skill
        job_vec = np.array(job_item.vector)

        logger.debug("Job item: %r", job_skill)

        best_similarity, best_cv_skill = best_cosine_similarity_for_job_item(
            cv_items,
            job_vec,
            "CV Item",
        )

        logger.debug("Best match: %r %.4f", best_cv_skill, best_similarity)

        if best_similarity >= MATCH_THRESHOLD:
            total += per_job_item_weight * best_similarity

    total = clamp(total, 0, weight)
    return total


def score_category(
    profile: MatchingEntity,
    job: MatchingEntity,
    category: str,
    weight: float,
) -> float:
    if category == "experience":
        return match_experience(profile.experience, job.experience, weight)

    if category == "technologies":
        return array_match_technologies(
            getattr(profile.embeddings, category),
            getattr(job.embeddings, category),
            weight,
        )

    return array_match(
        getattr(profile.embeddings, category),
        getattr(job.embeddings, category),
        weight,
    )


def match_label(score: float) -> str:
    if score >= 80:
        return "Strong Candidate"
    if score >= 60:
        return "Good Match"
    if score >= 40:
        return "Moderate Match"
    return "Weak Match"


def score_job(profile: MatchingEntity, job: MatchingEntity, job_index: int) -> dict:
    logger.debug("##### EVALUATING JOB #%s: %s #####", job_index + 1, job.title)

    final_score = 0.0
    breakdown = {}

    for category, weight in weights.items():
        contribution = score_category(profile, job, category, weight)

        logger.debug(
            "Category=%s weight=%.0f%% contribution=%.4f (%.2f%%)",
            category,
            weight * 100,
            contribution,
            contribution * 100,
        )

        final_score += contribution
        breakdown[category] = round(contribution * 100, 2)

    final_pct = round(final_score * 100, 2)

    logger.debug("FINAL SCORE: %.2f%% -> %s", final_pct, match_label(final_pct))

    return {
        "id": job.id,
        "title": job.title,
        "match_percentage": final_pct,
        "breakdown": breakdown,
        "label": match_label(final_pct),
    }


def entity_without_embeddings(entity: MatchingEntity) -> dict:
    if hasattr(entity, "model_dump"):
        return entity.model_dump(exclude={"embeddings"})
    return entity.dict(exclude={"embeddings"})


def match_profile_to_jobs(jobs: List[MatchingEntity], profile: MatchingEntity) -> List[dict]:
    t0 = time.perf_counter()
    results = []

    for i, job in enumerate(jobs):
        logger.debug("--- MATCH ITERATION #%s ---", i + 1)
        logger.debug("Profile (no embeddings): %s", entity_without_embeddings(profile))
        logger.debug("Job (no embeddings): %s", entity_without_embeddings(job))

        result = score_job(profile, job, i)
        results.append(result)

    results_sorted = sorted(results, key=lambda x: x["match_percentage"], reverse=True)

    logger.debug("===== MATCH SUMMARY (sorted) =====")
    for rank, r in enumerate(results_sorted, start=1):
        logger.debug(
            "rank=%s title=%r score=%.2f%% label=%s",
            rank,
            r["title"],
            r["match_percentage"],
            r["label"],
        )

    if results_sorted:
        best = results_sorted[0]
        logger.debug("Best match: %s (%.2f%%)", best["title"], best["match_percentage"])

    elapsed_ms = int((time.perf_counter() - t0) * 1000)
    top = results_sorted[0] if results_sorted else None
    logger.info(
        "match_profile_to_jobs profile_id=%s jobs=%d top_score=%s top_id=%s elapsed_ms=%d",
        profile.id,
        len(jobs),
        f"{top['match_percentage']:.2f}" if top else "0",
        top["id"] if top else None,
        elapsed_ms,
    )

    return results_sorted


@app.post("/match/jobs")
def match_jobs_endpoint(payload: MatchRequest):
    return match_profile_to_jobs(payload.jobs, payload.profile)
