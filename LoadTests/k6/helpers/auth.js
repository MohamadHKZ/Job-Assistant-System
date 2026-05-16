import http from 'k6/http';
import { check } from 'k6';

const BASE = () => __ENV.BASE_URL || 'http://localhost:5000';

/**
 * POST /api/Accounts/login — caches bearer token on the vu object.
 */
export function login(vu, email, password) {
  const res = http.post(
    `${BASE()}/api/Accounts/login`,
    JSON.stringify({ email, password }),
    { headers: { 'Content-Type': 'application/json' } },
  );
  check(res, { 'login 200': (r) => r.status === 200 });
  if (res.status === 200) {
    const body = JSON.parse(res.body);
    vu.token = body.token;
    vu.userId = body.jobSeekerId;
  }
  return vu.token;
}

export function authHeaders(vu) {
  if (!vu.token) return {};
  return { Authorization: `Bearer ${vu.token}` };
}
