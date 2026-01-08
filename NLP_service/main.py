import numpy as np


# ---------------------------------------------------------
# Cosine similarity
# ---------------------------------------------------------
def cosine_similarity(a, b):
    return np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b))


# ---------------------------------------------------------
# Generic N×M category matcher
# ---------------------------------------------------------
def array_match(cv_vecs, job_vecs, weight,cv_path,job_path):
    if job_vecs.shape[0] == 0:
      return weight
    n = len(job_vecs)
    per_job_item_weight = weight / n
    total = 0.0

    for job_vec in job_vecs:
        best_similarity = 0.0

        for cv_vec in cv_vecs:
            sim = cosine_similarity(cv_vec, job_vec)
            best_similarity = max(best_similarity, sim)

        # Decision based on best similarity
        if best_similarity >= 0.65:
            # Reward
            total += per_job_item_weight*best_similarity


    # Clamp score to [0, weight]
    total = max(0, min(total, weight))
    return total


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
# FINAL CALCULATION
# ---------------------------------------------------------
print("\n=====================================")
print("         FINAL MATCH RESULTS         ")
print("=====================================\n")

final_score = 0.0

for category, weight in weights.items():
    contribution = array_match(
        cv_data[category],
        job_data[category],
        weight,
        f"/content/drive/MyDrive/embeddings/cv_{category}.npy",
        f"/content/drive/MyDrive/embeddings/job_{category}.npy"
    )

    print(f"Category: {category}")
    print(f"  Contribution: {contribution:.4f}  ({contribution*100:.2f}%)")

    final_score += contribution


final_percentage = final_score * 100


