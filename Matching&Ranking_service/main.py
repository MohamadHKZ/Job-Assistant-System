from fastapi import FastAPI
from pydantic import BaseModel
from typing import List, Optional, Tuple
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
    ("mid",    "mid"):    1.00,
    ("associate", "associate"): 1.00,
    ("senior", "senior"): 1.00,

    ("junior", "mid"):    0.65,
    ("junior", "associate"): 0.65,
    ("mid",    "junior"): 0.60,
    ("associate",    "junior"): 0.65,

    ("mid",    "senior"): 0.70,
    ("associate", "senior"): 0.70,
    ("senior", "mid"):    0.80,
    ("senior", "associate"): 0.80,

    ("junior", "senior"): 0.40,
    ("senior", "junior"): 0.70,
}


# ---------------------------------------------------------
# Weights
# ---------------------------------------------------------
weights = {
    "technologies":        0.50,
    "job_position_skills": 0.25,
    "experience":          0.25,
}

MATCH_THRESHOLD = 0.65
TECHNOLOGY_COSINE_MIN_SIMILARITY = 0.80


# ---------------------------------------------------------
# Cosine similarity
# ---------------------------------------------------------
def cosine_similarity(a, b):
    return np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b))


def clamp(value: float, minimum: float, maximum: float) -> float:
    return max(minimum, min(value, maximum))


def normalize_experience_level(level: str) -> str:
    return level.strip().lower()


def print_matching_header(title: str, weight: float) -> None:
    print("\n=====================================")
    print(f" {title} (weight = {weight})")
    print("=====================================")


def find_exact_substring_match(cv_items: List[SkillEmbedding], job_skill_lower: str) -> Optional[str]:
    for cv_item in cv_items:
        cv_tech_lower = cv_item.skill.lower()
        print(f"   Checking exact match: CV Tech '{cv_item.skill}' vs Job Tech '{job_skill_lower}'")
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
        print(f"   {cv_item_label}: '{cv_item.skill}' → similarity = {sim:.4f}")
        if sim > best_similarity:
            best_similarity = sim
            best_cv_skill = cv_item.skill

    return best_similarity, best_cv_skill


# ---------------------------------------------------------
# Experience matcher
# ---------------------------------------------------------
def match_experience(cv_level: str, job_level: str, weight: float) -> float:
    cv_level  = normalize_experience_level(cv_level)
    job_level = normalize_experience_level(job_level)
    ratio     = EXPERIENCE_SCORES.get((cv_level, job_level), 0.0)
    score     = ratio * weight

    print_matching_header("EXPERIENCE MATCHING", weight)
    print(f"   CV  level  : {cv_level}")
    print(f"   Job level  : {job_level}")
    print(f"   Match ratio: {ratio:.2f}")
    print(f"   Score      : {score:.4f}")

    return score


# ---------------------------------------------------------
# Technologies matcher (exact-match-first, cosine fallback)
# ---------------------------------------------------------
def array_match_technologies(
    cv_items: List[SkillEmbedding],
    job_items: List[SkillEmbedding],
    weight: float,
) -> float:
    if len(job_items) == 0:
        return weight

    n                   = len(job_items)
    per_job_item_weight = weight / n
    total               = 0.0

    print_matching_header("TECHNOLOGIES MATCHING", weight)

    for job_item in job_items:
        job_skill      = job_item.skill
        job_tech_lower = job_skill.lower()
        job_vec        = np.array(job_item.vector)

        print(f"\n▶ Job Tech: '{job_skill}'")

        exact_match = find_exact_substring_match(cv_items, job_tech_lower)

        if exact_match:
            best_similarity = 1.0
            best_cv_tech = exact_match
            print(f"   ✅ Exact match → '{exact_match}' → similarity = 1.0000")
        else:
            print(f"   (No exact match — falling back to cosine similarity)")

            best_similarity, best_cv_tech = best_cosine_similarity_for_job_item(
                cv_items,
                job_vec,
                "CV Tech",
            )

            if best_similarity < TECHNOLOGY_COSINE_MIN_SIMILARITY:
                best_similarity = 0.0

            print(f"   ✔ Best result: '{best_cv_tech}' ({best_similarity:.4f})")

        if best_similarity >= MATCH_THRESHOLD:
            total += per_job_item_weight * best_similarity

    total = clamp(total, 0, weight)
    return total


# ---------------------------------------------------------
# Generic N×M matcher (job_position_skills, field_skills, etc.)
# ---------------------------------------------------------
def array_match(
    cv_items: List[SkillEmbedding],
    job_items: List[SkillEmbedding],
    weight: float,
) -> float:
    if len(job_items) == 0:
        return weight

    n                   = len(job_items)
    per_job_item_weight = weight / n
    total               = 0.0

    print_matching_header("CATEGORY MATCHING", weight)

    for job_item in job_items:
        job_skill       = job_item.skill
        job_vec         = np.array(job_item.vector)

        print(f"\n▶ Job Item: '{job_skill}'")

        best_similarity, best_cv_skill = best_cosine_similarity_for_job_item(
            cv_items,
            job_vec,
            "CV Item",
        )

        print(f"   ✔ Best match: '{best_cv_skill}' ({best_similarity:.4f})")

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


# ---------------------------------------------------------
# Label helper
# ---------------------------------------------------------
def match_label(score: float) -> str:
    if score >= 80:
        return "Strong Candidate"
    elif score >= 60:
        return "Good Match"
    elif score >= 40:
        return "Moderate Match"
    else:
        return "Weak Match"


# ---------------------------------------------------------
# Score a single job against the profile
# ---------------------------------------------------------
def score_job(profile: MatchingEntity, job: MatchingEntity, job_index: int) -> dict:
    print(f"\n\n{'#'*50}")
    print(f"#  EVALUATING JOB #{job_index + 1}: {job.title}")
    print(f"{'#'*50}")

    final_score = 0.0
    breakdown   = {}

    for category, weight in weights.items():
        contribution = score_category(profile, job, category, weight)

        print(f"\nCategory : {category}")
        print(f"  Weight       : {weight * 100:.0f}%")
        print(f"  Contribution : {contribution:.4f}  ({contribution * 100:.2f}%)")

        final_score         += contribution
        breakdown[category]  = round(contribution * 100, 2)

    final_pct = round(final_score * 100, 2)

    print(f"\n{'=' * 50}")
    print(f"  FINAL SCORE: {final_pct:.2f}%  →  {match_label(final_pct)}")
    print(f"{'=' * 50}")

    return {
        "id":               job.id,
        "title":            job.title,
        "match_percentage": final_pct,
        "breakdown":        breakdown,
        "label":            match_label(final_pct),
    }


def entity_without_embeddings(entity: MatchingEntity) -> dict:
    if hasattr(entity, "model_dump"):
        return entity.model_dump(exclude={"embeddings"})
    return entity.dict(exclude={"embeddings"})


# ---------------------------------------------------------
# Match profile against all jobs
# ---------------------------------------------------------
def match_profile_to_jobs(jobs: List[MatchingEntity], profile: MatchingEntity) -> List[dict]:
    results = []

    for i, job in enumerate(jobs):
        print(f"\n\n{'~' * 65}")
        print(f" MATCH ITERATION #{i + 1}")
        print(f"{'~' * 65}")
        print("Profile object (without embeddings):")
        print(entity_without_embeddings(profile))
        print("Job object (without embeddings):")
        print(entity_without_embeddings(job))

        result = score_job(profile, job, i)
        results.append(result)

    results_sorted = sorted(results, key=lambda x: x["match_percentage"], reverse=True)

    print("\n\n")
    print("=" * 65)
    print("              MATCH SUMMARY  (sorted best → worst)         ")
    print("=" * 65)
    print(f"{'Rank':<5} {'Job Title':<30} {'Score':>7}  Label")
    print("-" * 65)

    for rank, r in enumerate(results_sorted, start=1):
        print(
            f"{rank:<5} {r['title']:<30} {r['match_percentage']:>6.2f}%"
            f"  {r['label']}"
        )

    print("=" * 65)
    if results_sorted:
        best = results_sorted[0]
        print(f"\nBest match: {best['title']} ({best['match_percentage']:.2f}%)")

    return results_sorted


# ---------------------------------------------------------
# API ENDPOINT
# ---------------------------------------------------------
@app.post("/match/jobs")
def match_jobs_endpoint(payload: MatchRequest):
    return match_profile_to_jobs(payload.jobs, payload.profile)
