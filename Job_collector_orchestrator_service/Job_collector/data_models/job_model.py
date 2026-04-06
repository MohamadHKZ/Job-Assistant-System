from datetime import datetime

from pydantic import BaseModel


class Job(BaseModel):
    job_post_id: int
    job_title: str
    job_description: str
    job_type: str
    location: str
    experience_level: str
    posted_date: datetime
    url: str
    source_name: str
    company_name: str