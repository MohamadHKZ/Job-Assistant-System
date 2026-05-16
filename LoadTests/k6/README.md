# Load tests (k6)

Scripts exercise the HTTP API against a **running** stack (e.g. `docker compose up`).

## Prerequisites

- [k6](https://k6.io/docs/get-started/installation/)
- API reachable at `BASE_URL` (default `http://localhost:5000`)

## Optional authenticated scenarios

Set these when you want VUs to hit `/api/Jobs` and `/api/Profile`:

- `LOAD_TEST_EMAIL` — existing user email
- `LOAD_TEST_PASSWORD` — password
- `LOAD_TEST_PROFILE_ID` — numeric profile id for that user

## Commands

From the `Job-Assistant-System` repository root:

```bash
k6 run LoadTests/k6/scenarios/smoke.js --env BASE_URL=http://localhost:5000
k6 run LoadTests/k6/scenarios/load.js --env BASE_URL=http://localhost:5000 --env LOAD_TEST_EMAIL=... --env LOAD_TEST_PASSWORD=... --env LOAD_TEST_PROFILE_ID=1
k6 run LoadTests/k6/scenarios/stress.js --env BASE_URL=http://localhost:5000
k6 run LoadTests/k6/scenarios/spike.js --env BASE_URL=http://localhost:5000
```

## Files

| File | Purpose |
|------|---------|
| `thresholds.js` | Shared latency / error-rate thresholds |
| `helpers/auth.js` | Login helper (optional; scenarios mostly use `setup()`) |
| `scenarios/smoke.js` | 1 VU, 1 minute sanity |
| `scenarios/load.js` | Ramp to 50 VUs, weighted mix |
| `scenarios/stress.js` | Ramp to 200 VUs |
| `scenarios/spike.js` | Sudden burst to 300 VUs |
