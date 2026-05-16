import http from 'k6/http';
import { check, sleep } from 'k6';
import { defaultThresholds } from '../thresholds.js';

export const options = {
  vus: 1,
  duration: '1m',
  thresholds: defaultThresholds,
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
  return {
    token,
    profileId: __ENV.LOAD_TEST_PROFILE_ID || null,
  };
}

export default function (data) {
  const b = base();
  const auth = data.token ? { Authorization: `Bearer ${data.token}` } : {};

  const trends = http.get(`${b}/api/Trends`);
  check(trends, { 'trends 200': (r) => r.status === 200 });

  if (data.token && data.profileId) {
    const prof = http.get(`${b}/api/Profile/${data.profileId}`, { headers: auth });
    check(prof, { 'profile 200': (r) => r.status === 200 });
    const jobs = http.get(`${b}/api/Jobs/${data.profileId}`, { headers: auth });
    check(jobs, { 'jobs 200': (r) => r.status === 200 });
  }

  sleep(1);
}
