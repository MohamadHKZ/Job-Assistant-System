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

  if (!response.ok) {
    let msg = 'Failed to fetch trends';
    try {
      const data = await response.json();
      msg = data.message || msg;
    } catch {
      // ignore
    }
    throw new Error(msg);
  }

  return response.json();
};
