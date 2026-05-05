import logging
import time

from apify_client import ApifyClient

from Job_collector.data_models.job_model import Job

logger = logging.getLogger(__name__)


class LinkedInProvider:
    name: str = "LinkedIn"
    def __init__(
        self,
        apify_token: str,
        actor_id: str
    ) -> None:
        self._client = ApifyClient(apify_token)
        self._actor_id = actor_id

    def get_jobs(
        self,
        *,
        location: str = "Jordan",
        title: str = "Software developer",
        rows: int = 15,
        published_at: str = "r86400",
    ) -> list[Job]:
        run_input = {
            "location": location,
          "proxy": {
        "useApifyProxy": True,
        "apifyProxyGroups": [
            "RESIDENTIAL"
        ],
        "apifyProxyCountry": "JO"
    },
            "publishedAt": published_at,
            "rows": rows,
            "title": title,
        }

        logger.info(
            "Apify actor=%s starting rows=%d title=%r location=%r",
            self._actor_id,
            rows,
            title,
            location,
        )
        t0 = time.perf_counter()
        run_id = "?"
        try:
            run = self._client.actor(self._actor_id).call(run_input=run_input)
            run_id = str(run.get("id", "?"))
            jobs: list[Job] = []
            for item in self._client.dataset(run["defaultDatasetId"]).iterate_items():
                jobs.append(
                    Job(
                        job_post_id=item.get("id"),
                        job_title=item.get("title"),
                        job_description=item.get("description"),
                        job_type=item.get("contractType"),
                        location=item.get("location"),
                        experience_level=item.get("experienceLevel"),
                        posted_date=item.get("publishedAt"),
                        url=item.get("jobUrl"),
                        source_name="Linkedin",
                        company_name=item.get("companyName"),
                    )
                )
        except Exception:
            logger.exception("Apify actor=%s failed", self._actor_id)
            raise

        elapsed_ms = int((time.perf_counter() - t0) * 1000)
        logger.info(
            "Apify actor=%s run_id=%s items=%d elapsed_ms=%d",
            self._actor_id,
            run_id,
            len(jobs),
            elapsed_ms,
        )

        return jobs
