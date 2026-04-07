#!/bin/sh
# ---------------------------------------------------------------------------
# Interval (in seconds) between job runs.
# Change this single value to adjust how often the job executes.
#   1 hour  = 3600
#   6 hours = 21600
#   12 hours = 43200
# ---------------------------------------------------------------------------
INTERVAL=600

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
timestamp() {
    date '+%Y-%m-%d %H:%M:%S'
}

log() {
    echo "[$(timestamp)] $*"
}

# Convert raw seconds into a "Xh Ym Zs" human-readable string.
human_duration() {
    _secs="$1"
    _h=$(( _secs / 3600 ))
    _m=$(( (_secs % 3600) / 60 ))
    _s=$(( _secs % 60 ))
    if [ "$_h" -gt 0 ]; then
        echo "${_h}h ${_m}m ${_s}s"
    elif [ "$_m" -gt 0 ]; then
        echo "${_m}m ${_s}s"
    else
        echo "${_s}s"
    fi
}

# ---------------------------------------------------------------------------
# Main loop
# ---------------------------------------------------------------------------
log "============================================================"
log "Container started. Job interval: $(human_duration "$INTERVAL")"
log "============================================================"

while true; do
    # ---- Run job ----
    log "--- Job starting ---"
    JOB_START=$(date +%s)

    python main.py
    EXIT_CODE=$?

    JOB_END=$(date +%s)
    JOB_DURATION=$(( JOB_END - JOB_START ))

    if [ "$EXIT_CODE" -eq 0 ]; then
        log "Job finished successfully (exit code: $EXIT_CODE) | duration: $(human_duration "$JOB_DURATION")"
    else
        log "Job FAILED (exit code: $EXIT_CODE) | duration: $(human_duration "$JOB_DURATION") — continuing loop"
    fi

    # ---- Calculate sleep time ----
    SLEEP_TIME=$(( INTERVAL - JOB_DURATION ))

    if [ "$SLEEP_TIME" -le 0 ]; then
        log "WARNING: Job duration ($(human_duration "$JOB_DURATION")) exceeded the interval ($(human_duration "$INTERVAL")). Skipping sleep and running immediately."
    else
        log "Sleep starting. Next run in: $(human_duration "$SLEEP_TIME")"
        sleep "$SLEEP_TIME"
        log "Sleep ended. Starting next job run now."
    fi
done
