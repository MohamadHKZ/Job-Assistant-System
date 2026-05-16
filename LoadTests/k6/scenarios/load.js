import http from "k6/http";
import { check, sleep } from "k6";
import { defaultThresholds } from "../thresholds.js";

export const options = {
  stages: [
    { duration: "2m", target: 5 },
    { duration: "5m", target: 10 },
    { duration: "1m", target: 0 },
  ],
  thresholds: defaultThresholds,
};

const base = () => __ENV.BASE_URL || "http://localhost:5000";

export function setup() {
  const b = base();
  const email = __ENV.LOAD_TEST_EMAIL;
  const password = __ENV.LOAD_TEST_PASSWORD;
  let token = null;
  if (email && password) {
    const res = http.post(
      `${b}/api/Accounts/login`,
      JSON.stringify({ email, password }),
      { headers: { "Content-Type": "application/json" } },
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

  if (roll < 0.15) {
    const res = http.post(
      `${b}/api/Accounts/login`,
      JSON.stringify({
        email: __ENV.LOAD_TEST_EMAIL || "missing@example.com",
        password: __ENV.LOAD_TEST_PASSWORD || "x",
      }),
      { headers: { "Content-Type": "application/json" } },
    );
    check(res, { "login ok": (r) => r.status === 200 || r.status === 401 });
  } else if (roll < 0.65 && data.token && data.profileId) {
    const res = http.get(`${b}/api/Jobs/${data.profileId}`, { headers: auth });
    check(res, { "jobs 200": (r) => r.status === 200 });
  } else if (roll < 0.9 && data.token && data.profileId) {
    const res = http.get(`${b}/api/Profile/${data.profileId}`, {
      headers: auth,
    });
    check(res, { "profile 200": (r) => r.status === 200 });
  } else {
    const res = http.get(`${b}/api/Trends`);
    check(res, { "trends 200": (r) => r.status === 200 });
  }

  sleep(0.3);
}
