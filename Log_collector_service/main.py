"""Poll Docker container logs every 30s and write tail to shared volume files."""

from __future__ import annotations

import logging
import os
import time

import docker

LOG_DIR = os.environ.get("LOG_COLLECTOR_OUTPUT_DIR", "/logs")
INTERVAL_SEC = int(os.environ.get("LOG_COLLECTOR_INTERVAL_SEC", "30"))
TAIL_LINES = int(os.environ.get("LOG_COLLECTOR_TAIL", "1000"))

# Compose service names (must match docker-compose `services:` keys)
SERVICES = [
    "backend",
    "nlp_service",
    "matching_service",
    "embedding_service",
    "nlp_embedding_service",
    "job_collector_orchestrator",
]

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s %(levelname)s %(message)s",
)
log = logging.getLogger("log_collector")


def write_service_logs(client: docker.DockerClient, service: str) -> None:
    """Find a running container for this compose service and append snapshots to file."""
    try:
        containers = client.containers.list(
            filters={"label": f"com.docker.compose.service={service}"},
        )
    except docker.errors.DockerException as e:
        log.warning("Docker list failed for %s: %s", service, e)
        return

    if not containers:
        log.debug("No running container for service=%s", service)
        return

    # Prefer running container if multiple (e.g. scaling); take first.
    container = containers[0]
    try:
        raw = container.logs(tail=TAIL_LINES, timestamps=True)
        text = raw.decode("utf-8", errors="replace")
    except docker.errors.DockerException as e:
        log.warning("logs() failed for %s (%s): %s", service, container.short_id, e)
        return

    os.makedirs(LOG_DIR, exist_ok=True)
    path = os.path.join(LOG_DIR, f"{service}.log")
    try:
        with open(path, "w", encoding="utf-8") as f:
            f.write(text)
    except OSError as e:
        log.error("Write failed %s: %s", path, e)


def main() -> None:
    log.info(
        "Starting log collector: interval=%ss tail=%s services=%s dir=%s",
        INTERVAL_SEC,
        TAIL_LINES,
        SERVICES,
        LOG_DIR,
    )
    client = docker.from_env()

    while True:
        try:
            for svc in SERVICES:
                write_service_logs(client, svc)
            write_collector_heartbeat()
        except Exception:
            log.exception("Unexpected error in poll loop")
        time.sleep(INTERVAL_SEC)


def write_collector_heartbeat() -> None:
    os.makedirs(LOG_DIR, exist_ok=True)
    path = os.path.join(LOG_DIR, "log_collector.log")
    try:
        ts = time.strftime("%Y-%m-%d %H:%M:%SZ", time.gmtime())
        with open(path, "w", encoding="utf-8") as f:
            f.write(f"log_collector heartbeat {ts}\n")
            f.write(
                "Collecting docker logs for: "
                + ", ".join(SERVICES)
                + "\n"
            )
    except OSError as e:
        log.error("Heartbeat write failed %s: %s", path, e)


if __name__ == "__main__":
    main()
