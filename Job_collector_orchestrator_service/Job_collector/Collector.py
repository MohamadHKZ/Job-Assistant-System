import logging
from Job_collector.data_models.job_model import Job
from Job_collector.interfaces.provider import Provider
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s"
)

class JobCollector:
    def __init__(self, provider: Provider | None = None) -> None:
        self.provider = provider
        self.logger = logging.getLogger(self.__class__.__name__)

    def collect(
        self,
        *,
        location: str = "Jordan",
        title: str = "Software developer",
        rows: int = 50,
        published_at: str = "r604800",
    ) -> list[Job]:
        self.logger.info(f"Collecting data from {self.provider.name}...")

        jobs = self.provider.get_jobs(
            location=location,
            title=title,
            rows=rows,
            published_at=published_at,
        )

        self.logger.info("Collection finished. Total jobs collected: %s", len(jobs))
        return jobs