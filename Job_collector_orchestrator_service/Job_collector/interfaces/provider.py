from typing import Protocol
from Job_collector.data_models.job_model import Job
class Provider(Protocol):
    name: str
    def get_jobs(
        self,
        *,
        location: str = "Jordan",
        title: str = "Software developer",
        rows: int = 50,
        published_at: str = "r604800",
    ) -> list[Job]: ...