from sentence_transformers import SentenceTransformer
from pathlib import Path

MODEL_PATH = Path("/app/models/BAAI/bge-large-en-v1.5")

# create parent directories
MODEL_PATH.parent.mkdir(parents=True, exist_ok=True)

# download model
model = SentenceTransformer("BAAI/bge-large-en-v1.5")

# explicitly save it to the required path
model.save(str(MODEL_PATH))

print(f"Model saved to {MODEL_PATH}")