from __future__ import annotations

from apify_client import ApifyClient

from Job_collector.data_models.job_model import Job


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

        run = self._client.actor(self._actor_id).call(run_input=run_input)

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

        return jobs
