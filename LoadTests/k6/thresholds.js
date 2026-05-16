/**
 * Shared SLO-style thresholds for k6 scenarios.
 * Import into scenario files: import { defaultThresholds } from '../thresholds.js';
 */
export const defaultThresholds = {
  http_req_duration: ['p(95)<2000'],
  http_req_failed: ['rate<0.01'],
  checks: ['rate>0.99'],
};
