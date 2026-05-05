import { parseApiError } from './apiError';
import { devLog } from './devLog';

const API_URL = import.meta.env.VITE_BACKEND_API_BASE_URL || 'https://localhost:5000';

// Endpoint: GET api/jobs/{profileId}
export const getRecommendedJobs = async (token, profileId) => {
  const url = `${API_URL}/api/jobs/${profileId}`;

  const response = await fetch(url, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) throw await parseApiError(response);
  const res = await response.json();
  devLog('[jobs] getRecommendedJobs', res);
  return res;
};
