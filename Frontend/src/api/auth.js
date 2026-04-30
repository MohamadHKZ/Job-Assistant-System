import { parseApiError } from './apiError';

const API_URL = import.meta.env.VITE_BACKEND_API_BASE_URL || 'https://localhost:5000';

// Endpoint: POST api/accounts/login
export const login = async (email, password) => {
  const response = await fetch(`${API_URL}/api/accounts/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });

  if (!response.ok) throw await parseApiError(response);
  return response.json();
};

// Endpoint: POST api/accounts/register
export const register = async (email, password) => {
  const response = await fetch(`${API_URL}/api/accounts/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });

  if (!response.ok) throw await parseApiError(response);
  return response.json();
};
