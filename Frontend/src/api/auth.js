const API_URL = import.meta.env.VITE_BACKEND_API_BASE_URL || 'https://localhost:5000';

// Endpoint: POST api/jobseeker/login
export const login = async (email, password) => {
  const response = await fetch(`${API_URL}/api/accounts/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });

  if (!response.ok) {
    const data = await response.json();
    throw new Error(data.message || 'Login failed');
  }
  const res = await response.json();

  return res;
};

// endpoint: POST api/jobseeker/register
export const register = async (email, password) => {
  const response = await fetch(`${API_URL}/api/accounts/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });
  if (!response.ok) {
    const data = await response.json();
    throw new Error(data.message || 'Registration failed');
  }
  return response.json();
};