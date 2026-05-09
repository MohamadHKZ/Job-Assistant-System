import { parseApiError } from './apiError';

const API_URL = import.meta.env.VITE_BACKEND_API_BASE_URL || 'https://localhost:5000';

const authHeaders = (token, json = true) => {
  const h = { Authorization: `Bearer ${token}` };
  if (json) h['Content-Type'] = 'application/json';
  return h;
};

export async function getJobSources(token) {
  const res = await fetch(`${API_URL}/api/admin/job-sources`, {
    headers: authHeaders(token),
  });
  if (!res.ok) throw await parseApiError(res);
  return res.json();
}

export async function patchJobSource(token, sourceName, isActive) {
  const encoded = encodeURIComponent(sourceName);
  const res = await fetch(`${API_URL}/api/admin/job-sources/${encoded}`, {
    method: 'PATCH',
    headers: authHeaders(token),
    body: JSON.stringify({ isActive }),
  });
  if (!res.ok) throw await parseApiError(res);
  return res.json();
}

export async function getAdminLogs(token, container) {
  const q = new URLSearchParams({ container });
  const res = await fetch(`${API_URL}/api/admin/logs?${q}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw await parseApiError(res);
  return res.text();
}

export async function getSettings(token) {
  const res = await fetch(`${API_URL}/api/admin/settings`, {
    headers: authHeaders(token),
  });
  if (!res.ok) throw await parseApiError(res);
  return res.json();
}

export async function updateSettings(token, settings) {
  const res = await fetch(`${API_URL}/api/admin/settings`, {
    method: 'PUT',
    headers: authHeaders(token),
    body: JSON.stringify({ settings }),
  });
  if (!res.ok) throw await parseApiError(res);
  return res.json();
}

export async function getAnalytics(token) {
  const res = await fetch(`${API_URL}/api/admin/analytics`, {
    headers: authHeaders(token),
  });
  if (!res.ok) throw await parseApiError(res);
  return res.json();
}

export const LOG_CONTAINERS = [
  { id: 'backend', label: 'backend' },
  { id: 'nlp_service', label: 'nlp_service' },
  { id: 'matching_service', label: 'matching_service' },
  { id: 'embedding_service', label: 'embedding_service' },
  { id: 'nlp_embedding_service', label: 'nlp_embedding_service' },
  { id: 'job_collector_orchestrator', label: 'job_collector_orchestrator' },
  { id: 'log_collector', label: 'log_collector' },
];

export const SCRAPE_INTERVAL_OPTIONS = ['LAST_DAY', 'LAST_WEEK', 'LAST_MONTH'];
