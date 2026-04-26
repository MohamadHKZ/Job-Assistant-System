"""
Trend analyzer
==============

Refreshes the aggregated analytics stored in the `public."Trends"` table.

For every row of the `Trends` table it computes:

1. `TopTechnicalSkills` (jsonb) - the top 10 technical skills for three
   rolling periods (week, month, 3 months) together with the total number of
   skill occurrences observed in each period. Source:
   `public."TechnicalSkillsRecorded"` filtered by `JobTitle ~* <pattern>`.

2. `JobsCount` (integer) - the number of normalized job posts whose refined
   job title matches the trend's pattern. Source:
   `public."NormalizedJobPosts"`.

Each trend row is associated with a POSIX regular expression defined in
`job_title_patterns.JOB_TITLE_PATTERNS`. The analyzer persists the active
pattern into the `Trends."JobTitlePattern"` column (adding it if missing)
so that every service in the system can see the exact expression that
was used to compute the trends.
"""

from __future__ import annotations

import logging
from datetime import date, timedelta
from typing import Iterable

import psycopg2
from psycopg2.extras import Json

from Trend_analyzer.job_title_patterns import JOB_TITLE_PATTERNS


PERIODS: dict[str, int] = {
    "week": 7,
    "month": 30,
    "3_months": 90,
}

TOP_SKILLS_LIMIT = 10


def _ensure_pattern_column(cur) -> None:
    cur.execute(
        'ALTER TABLE public."Trends" '
        'ADD COLUMN IF NOT EXISTS "JobTitlePattern" TEXT'
    )


def _sync_patterns(cur, trends_rows: Iterable[tuple]) -> dict[int, tuple[str, str]]:
    """Persist the current regex patterns into the `Trends` table.

    Returns a mapping `{TrendId: (JobTitle, Pattern)}` limited to rows that
    have a pattern available in `JOB_TITLE_PATTERNS`.
    """
    id_to_pattern: dict[int, tuple[str, str]] = {}
    for trend_id, job_title, existing_pattern in trends_rows:
        pattern = JOB_TITLE_PATTERNS.get(job_title)
        if not pattern:
            logging.warning(
                "No regex pattern defined for trend job title %r (Id=%s); "
                "this trend row will be skipped.",
                job_title, trend_id,
            )
            continue

        if pattern != existing_pattern:
            cur.execute(
                'UPDATE public."Trends" '
                'SET "JobTitlePattern" = %s '
                'WHERE "Id" = %s',
                (pattern, trend_id),
            )

        id_to_pattern[trend_id] = (job_title, pattern)

    return id_to_pattern


def _fetch_top_skills(cur, pattern: str, since: date) -> tuple[list[dict], int]:
    cur.execute(
        'SELECT "SkillName", COUNT(*) AS cnt '
        'FROM public."TechnicalSkillsRecorded" '
        'WHERE "JobTitle" IS NOT NULL '
        '  AND "JobTitle" ~* %s '
        '  AND "DateRecorded" >= %s '
        'GROUP BY "SkillName" '
        'ORDER BY cnt DESC, "SkillName" ASC '
        'LIMIT %s',
        (pattern, since, TOP_SKILLS_LIMIT),
    )
    top_skills = [
        {"skill": name, "count": int(count)} for name, count in cur.fetchall()
    ]

    cur.execute(
        'SELECT COUNT(*) '
        'FROM public."TechnicalSkillsRecorded" '
        'WHERE "JobTitle" IS NOT NULL '
        '  AND "JobTitle" ~* %s '
        '  AND "DateRecorded" >= %s',
        (pattern, since),
    )
    (total_skills,) = cur.fetchone()

    return top_skills, int(total_skills)


def _fetch_job_count(cur, pattern: str) -> int:
    cur.execute(
        'SELECT COUNT(*) '
        'FROM public."NormalizedJobPosts" njp '
        'WHERE EXISTS ('
        '    SELECT 1 '
        '    FROM jsonb_array_elements_text(njp."JobTitleRefined") AS t(title) '
        '    WHERE t.title ~* %s'
        ')',
        (pattern,),
    )
    (jobs_count,) = cur.fetchone()
    return int(jobs_count)


def analyze_trends(database_url: str) -> None:
    """Recompute every row of `public."Trends"` from the latest raw data."""
    if not database_url:
        raise ValueError("database_url must be provided")

    conn = None
    cur = None
    try:
        conn = psycopg2.connect(database_url)
        cur = conn.cursor()

        _ensure_pattern_column(cur)
        conn.commit()

        cur.execute(
            'SELECT "Id", "JobTitle", "JobTitlePattern" '
            'FROM public."Trends" '
            'ORDER BY "Id"'
        )
        trends_rows = cur.fetchall()

        id_to_pattern = _sync_patterns(cur, trends_rows)
        conn.commit()

        if not id_to_pattern:
            logging.warning("No trend rows have a matching pattern; nothing to analyze.")
            return

        today = date.today()

        for trend_id, (job_title, pattern) in id_to_pattern.items():
            top_technical_skills: dict[str, dict] = {}
            for period_label, days in PERIODS.items():
                since = today - timedelta(days=days)
                top_skills, total_skills = _fetch_top_skills(cur, pattern, since)
                top_technical_skills[period_label] = {
                    "total_skills": total_skills,
                    "top_skills": top_skills,
                }

            jobs_count = _fetch_job_count(cur, pattern)

            cur.execute(
                'UPDATE public."Trends" '
                'SET "TopTechnicalSkills" = %s, "JobsCount" = %s '
                'WHERE "Id" = %s',
                (Json(top_technical_skills), jobs_count, trend_id),
            )

            logging.info(
                "Trend updated %r: JobsCount=%d; "
                "skills recorded week=%d / month=%d / 3m=%d",
                job_title, jobs_count,
                top_technical_skills["week"]["total_skills"],
                top_technical_skills["month"]["total_skills"],
                top_technical_skills["3_months"]["total_skills"],
            )

        conn.commit()

    except Exception:
        if conn is not None:
            conn.rollback()
        logging.exception("Trend analyzer run failed")
        raise
    finally:
        if cur is not None:
            cur.close()
        if conn is not None:
            conn.close()


if __name__ == "__main__":
    from os import getenv

    logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
    analyze_trends(getenv("DATABASE_URL"))
