import enum
import json
import logging
from datetime import datetime
from enum import Enum
from pydantic import BaseModel
from typing import List
import httpx
from  Job_collector.Collector import JobCollector
from Job_collector.interfaces.provider import Provider
from Job_collector.data_models.job_model import Job
from psycopg2 import sql
from psycopg2.extras import Json

class ResponseEntity(BaseModel):
    id: int
    technical_skills: List[str] = []
    technologies: List[str] = []
    job_position_skills: List[str] = []
    field_skills: List[str] = []
    job_title: List[str] = []
    soft_skills: List[str] = []


class SkillEmbedding(BaseModel):
    skill: str
    vector: List[float]


class EmbeddingCategories(BaseModel):
    technical_skills: List[SkillEmbedding] = []
    job_position_skills: List[SkillEmbedding] = []
    field_skills: List[SkillEmbedding] = []
    job_title: List[SkillEmbedding] = []
    soft_skills: List[SkillEmbedding] = []


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
    jobs : list[Job] = []
    published_at = PublishedAt[published_at.upper()].value
    for provider in providers:
        try:
            collector = JobCollector(provider=provider)
            jobs.extend(collector.collect(title=title, location=location, rows=rows, published_at=published_at))
        except Exception as e:
            logging.error(f"Error occurred while collecting jobs from {provider.name}: {e}")
    logging.info(f"Total jobs collected: {len(jobs)}")
    return jobs

async def call_nlp_embedding_orchestrator(payload: Request,url: str) -> Response:
    async with httpx.AsyncClient() as client:
        res = await client.post(
            url,
            json=payload.model_dump(),
            timeout=httpx.Timeout(3600.0)
        )
        res.raise_for_status()

        return Response(**res.json())


def shutdown(signum, frame):
    global running
    logging.info(f"Received signal {signum}, shutting down...")
    running["value"] = False

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
        logging.error(
            f"Error occurred while inserting rows into {table_name}: {e}\n"
            f"Rows: {json.dumps(rows, default=str, indent=2)}"
        )
        return False

def prepare_data(jobs: list[Job], refined_data: Response):
    jobs_data = []
    refined_job_posts = []
    embeddings = []
    technologies = []

    for idx, item in enumerate(refined_data.response):
        job = jobs[idx]
        entity = item.entity
        emb = item.embeddings.embeddings

        jobs_data.append({
            "JobPostId": job.job_post_id,
            "JobTitle": job.job_title,
            "JobDescription": job.job_description,
            "JobType": job.job_type,
            "Location": job.location,
            "ExperienceLevel": job.experience_level,
            "PostedDate": job.posted_date,
            "Url": job.url,
            "SourceName": job.source_name,
            "CompanyName": job.company_name,
        })

        refined_job_posts.append({
            "JobPostId": entity.id,
            "SourceName": job.source_name,
            "ExperienceLevelRefined": job.experience_level,
            "JobTitleRefined": Json(entity.job_title),
            "RequiredFieldSkills": Json(entity.field_skills),
            "RequiredJobPositionSkills": Json(entity.job_position_skills),
            "RequiredTechnicalSkills": Json(entity.technical_skills),
            "RequiredSoftSkills": Json(entity.soft_skills),
        })

        embeddings.append({
            "JobPostId": entity.id,
            "SourceName": job.source_name,
            "EmbeddedTechnicalSkills": Json([skill.model_dump() for skill in emb.technical_skills]),
            "EmbeddedJobPositionSkills": Json([skill.model_dump() for skill in emb.job_position_skills]),
            "EmbeddedJobTitle": Json([skill.model_dump() for skill in emb.job_title]),
            "EmbeddedFieldSkills": Json([skill.model_dump() for skill in emb.field_skills]),
            "EmbeddedSoftSkills": Json([skill.model_dump() for skill in emb.soft_skills]),
        })

        for tech in entity.technologies:
            technologies.append({
                "SkillName": tech,
                "DateRecorded": job.posted_date,
            })

    return jobs_data, refined_job_posts, embeddings, technologies

