import json
import logging
from datetime import datetime
from enum import Enum
from typing import List

import httpx
import uuid_utils
from Job_collector import JobCollector, Provider, Job
from pydantic import BaseModel, field_validator
from psycopg2 import sql
from psycopg2.extras import Json


class ResponseEntity(BaseModel):
    id: int
    technical_skills: List[str] = []
    technologies: List[str] = []
    experience_level: str
    job_position_skills: List[str] = []
    field_skills: List[str] = []
    job_title: str = ""
    soft_skills: List[str] = []

    @field_validator("job_title", mode="before")
    @classmethod
    def _coerce_job_title(cls, v):
        if v is None:
            return ""
        if isinstance(v, list):
            return str(v[0]).strip() if v else ""
        return str(v).strip()


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


class ObjectEmbeddings(BaseModel):
    id: int
    embeddings: EmbeddingCategories


class Request(BaseModel):
    prompt: str


class UnifiedResponseItem(BaseModel):
    entity: ResponseEntity
    embeddings: ObjectEmbeddings


class Response(BaseModel):
    response: List[UnifiedResponseItem]


logger = logging.getLogger(__name__)


class PublishedAt(Enum):
    LAST_DAY = "r86400"
    LAST_WEEK = "r604800"
    LAST_MONTH = "r2592000"


def collect_jobs(
    providers: list[Provider],
    *,
    title: str = "Software Engineer",
    location: str = "Jordan",
    rows: int = 20,
    published_at: str = "LAST_MONTH",
):
    jobs: list[Job] = []
    published_at = PublishedAt[published_at.upper()].value
    for provider in providers:
        try:
            collector = JobCollector(provider=provider)
            jobs.extend(collector.collect(title=title, location=location, rows=rows, published_at=published_at))
        except Exception:
            logger.exception("Error occurred while collecting jobs from %s", provider.name)
    logger.info("Total jobs collected: %d", len(jobs))
    return jobs


async def call_nlp_embedding_orchestrator(payload: Request, url: str) -> Response:
    async with httpx.AsyncClient() as client:
        res = await client.post(
            url,
            json=payload.model_dump(),
            timeout=httpx.Timeout(3600.0),
        )
        res.raise_for_status()

        return Response(**res.json())


def shutdown(signum, frame):
    logger.info("Received signal %s, shutting down...", signum)


def _json_serializer(value):
    if isinstance(value, datetime):
        return value.isoformat()
    return str(value)


def build_insert_query(schema_name: str, table_name: str, row: dict):
    if not row:
        raise ValueError("row must contain at least one column")

    columns = list(row.keys())
    columns_sql = sql.SQL(", ").join(sql.Identifier(column) for column in columns)
    placeholders_sql = sql.SQL(", ").join(sql.Placeholder(column) for column in columns)

    return sql.SQL("INSERT INTO {}.{} ({}) VALUES ({}) ON CONFLICT DO NOTHING").format(
        sql.Identifier(schema_name),
        sql.Identifier(table_name),
        columns_sql,
        placeholders_sql,
    )


def insert_rows(cur, conn, table_name: str, rows: list[dict]) -> bool:
    if not rows:
        return True

    try:
        query = build_insert_query("public", table_name, rows[0])
        cur.executemany(query, rows)
        conn.commit()
        return True
    except Exception as e:
        conn.rollback()
        logger.error(
            "Insert failed table=%s rows=%d sample_keys=%s: %s",
            table_name,
            len(rows),
            list(rows[0].keys()) if rows else [],
            e,
        )
        if logger.isEnabledFor(logging.DEBUG):
            logger.debug(
                "Insert failed payload table=%s rows=%s",
                table_name,
                json.dumps(rows, default=str),
            )
        return False


_JOB_TITLE_QUOTE_CHARS = "\"'“”‘’`"


def _strip_job_title_quotes(value) -> str:
    if value is None:
        return ""
    text = str(value).strip()
    for ch in _JOB_TITLE_QUOTE_CHARS:
        text = text.replace(ch, "")
    return text.strip()


def prepare_data(jobs: list[Job], refined_data: Response):
    jobs_data = []
    refined_job_posts = []
    embeddings = []
    technologies = []

    for idx, item in enumerate(refined_data.response):
        job = jobs[idx]
        entity = item.entity
        emb = item.embeddings.embeddings
        job_id = str(uuid_utils.uuid7())

        job_title = _strip_job_title_quotes(job.job_title)
        refined_job_title = _strip_job_title_quotes(entity.job_title)

        jobs_data.append(
            {
                "Id": job_id,
                "JobPostId": job.job_post_id,
                "JobTitle": job_title,
                "JobDescription": job.job_description,
                "JobType": job.job_type,
                "Location": job.location,
                "ExperienceLevel": job.experience_level,
                "PostedDate": job.posted_date,
                "Url": job.url,
                "SourceName": job.source_name,
                "CompanyName": job.company_name,
            }
        )

        refined_job_posts.append(
            {
                "Id": job_id,
                "JobPostId": entity.id,
                "SourceName": job.source_name,
                "ExperienceLevelRefined": Json(entity.experience_level),
                "JobTitleRefined": refined_job_title,
                "RequiredFieldSkills": Json(entity.field_skills),
                "RequiredJobPositionSkills": Json(entity.job_position_skills),
                "RequiredTechnicalSkills": Json(entity.technical_skills),
                "RequiredSoftSkills": Json(entity.soft_skills),
                "Technologies": Json(entity.technologies),
            }
        )

        embeddings.append(
            {
                "Id": job_id,
                "JobPostId": entity.id,
                "SourceName": job.source_name,
                "EmbeddedTechnicalSkills": Json([skill.model_dump() for skill in emb.technical_skills]),
                "EmbeddedJobPositionSkills": Json([skill.model_dump() for skill in emb.job_position_skills]),
                "EmbeddedJobTitle": Json([skill.model_dump() for skill in emb.job_title]),
                "EmbeddedFieldSkills": Json([skill.model_dump() for skill in emb.field_skills]),
                "EmbeddedSoftSkills": Json([skill.model_dump() for skill in emb.soft_skills]),
                "EmbeddedTechnologies": Json([skill.model_dump() for skill in emb.technologies]),
            }
        )

        primary_job_title = refined_job_title or job_title
        for tech in entity.technologies:
            technologies.append(
                {
                    "SkillName": tech,
                    "DateRecorded": job.posted_date,
                    "JobTitle": primary_job_title,
                }
            )

    return jobs_data, refined_job_posts, embeddings, technologies
