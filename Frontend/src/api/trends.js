import { parseApiError } from './apiError';

const API_URL =
  import.meta.env.VITE_BACKEND_API_BASE_URL || 'https://localhost:5000';

// Endpoint: GET /api/trends
export const getTrends = async (token) => {
  const headers = { 'Content-Type': 'application/json' };
  if (token) headers['Authorization'] = `Bearer ${token}`;

  const response = await fetch(`${API_URL}/api/trends`, {
    method: 'GET',
    headers,
  });

  if (!response.ok) throw await parseApiError(response);
  return response.json();
};
