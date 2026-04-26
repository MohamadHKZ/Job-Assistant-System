"""
Regex patterns (PostgreSQL POSIX flavour, used with the case-insensitive
`~*` operator) that describe how each job title listed in the
`public."Trends"` table should be matched against free-form job titles found
in the other tables (`TechnicalSkillsRecorded.JobTitle`,
`NormalizedJobPosts.JobTitleRefined`, ...).

Notes on the dialect:
- `\m` and `\M` match the start and end of a word (PostgreSQL extension).
- `[- ]?` tolerates both "back end", "back-end" and "backend" variants.
- Matching is case-insensitive because the queries use `~*` (see
  `trend_analyzer.py`).

Every key MUST equal the exact `JobTitle` value stored in the `Trends`
table. Adding a new trend row requires adding a new entry here.
"""

JOB_TITLE_PATTERNS: dict[str, str] = {
    "Software Engineer":
        r"\m(software[- ]?engineer|software[- ]?developer|swe|sde)\M",

    "Backend Developer":
        r"\m(back[- ]?end[- ]?(developer|engineer|programmer)|backend)\M",

    "Frontend Developer":
        r"\m(front[- ]?end[- ]?(developer|engineer|programmer)|frontend|ui[- ]?(developer|engineer))\M",

    "Full Stack Developer":
        r"\mfull[- ]?stack[- ]?(developer|engineer|programmer)\M",

    "Mobile Application Developer":
        r"\m(mobile[- ]?(developer|engineer|application[- ]?developer)|android[- ]?developer|ios[- ]?developer|flutter[- ]?developer|react[- ]?native[- ]?developer)\M",

    "Web Developer":
        r"\mweb[- ]?(developer|engineer|programmer)\M",

    "Database Developer":
        r"\m(database[- ]?(developer|engineer|administrator|admin)|dba)\M",

    "DevOps Engineer":
        r"\m(dev[- ]?ops[- ]?(engineer|developer|specialist)?|sre|site[- ]?reliability[- ]?engineer|platform[- ]?engineer)\M",

    "Systems Engineer":
        r"\msystems?[- ]?(engineer|administrator|admin|analyst)\M",

    "Cloud Engineer":
        r"\mcloud[- ]?(engineer|architect|developer|specialist)\M",

    "Cybersecurity Engineer":
        r"\m(cyber[- ]?security[- ]?(engineer|analyst|specialist)?|security[- ]?(engineer|analyst|specialist)|infosec[- ]?(engineer|analyst|specialist)?)\M",

    "Quality Assurance Engineer":
        r"\m(qa[- ]?(engineer|analyst|tester|automation[- ]?engineer)?|quality[- ]?assurance[- ]?(engineer|analyst)?|tester|test[- ]?(engineer|analyst|automation[- ]?engineer))\M",

    "Software Engineering Manager":
        r"\m(software[- ]?engineering[- ]?manager|engineering[- ]?manager|development[- ]?manager|technical[- ]?lead|tech[- ]?lead|engineering[- ]?lead)\M",
}
