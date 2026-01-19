const API_URL = import.meta.env.VITE_BACKEND_API_BASE_URL || 'https://localhost:5000';

// Endpoint: GET api/{jobseeker_id}/jobs
export const getRecommendedJobs = async (token,profileId) => {
  let url = `${API_URL}/api/jobs/${profileId}`;

  const response = await fetch(url, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  
  if (!response.ok) {
    const data = await response.json();
    throw new Error(data.message || 'Failed to fetch recommended jobs');
  }
  const res = await response.json();
  console.log(res);
  return res;
};