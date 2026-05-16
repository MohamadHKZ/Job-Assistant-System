import http from 'k6/http';
import { check, sleep } from 'k6';
import { defaultThresholds } from '../thresholds.js';

export const options = {
  stages: [
    { duration: '5m', target: 200 },
    { duration: '5m', target: 200 },
    { duration: '2m', target: 0 },
  ],
  thresholds: Object.assign(
    {},
    defaultThresholds,
    {
      http_req_failed: ['rate<0.05'],
      checks: ['rate>0.90'],
    },
  ),
};

const base = () => __ENV.BASE_URL || 'http://localhost:5000';

export function setup() {
  const b = base();
  const email = __ENV.LOAD_TEST_EMAIL;
  const password = __ENV.LOAD_TEST_PASSWORD;
  let token = null;
  if (email && password) {
    const res = http.post(
      `${b}/api/Accounts/login`,
      JSON.stringify({ email, password }),
      { headers: { 'Content-Type': 'application/json' } },
    );
    if (res.status === 200) {
      token = JSON.parse(res.body).token;
    }
  }
  return { token, profileId: __ENV.LOAD_TEST_PROFILE_ID || null };
}

export default function (data) {
  const b = base();
  const auth = data.token ? { Authorization: `Bearer ${data.token}` } : {};
  const roll = Math.random();

  if (roll < 0.5 && data.token && data.profileId) {
    const res = http.get(`${b}/api/Jobs/${data.profileId}`, { headers: auth });
    check(res, { 'jobs': (r) => r.status === 200 });
  } else if (roll < 0.75 && data.token && data.profileId) {
    const res = http.get(`${b}/api/Profile/${data.profileId}`, { headers: auth });
    check(res, { 'profile': (r) => r.status === 200 });
  } else {
    const res = http.get(`${b}/api/Trends`);
    check(res, { 'trends': (r) => r.status === 200 });
  }

  sleep(0.2);
}
