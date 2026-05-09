import { jwtDecode } from 'jwt-decode';

/**
 * Primary role for UI routing. Reads JWT `role` claim(s); backend issues ClaimTypes.Role → `"role"` in payload.
 * @param {string | null | undefined} token
 * @returns {string}
 */
export function parsePrimaryRoleFromToken(token) {
  if (!token || typeof token !== 'string') return 'User';
  try {
    const payload = jwtDecode(token);
    const r = payload.role;
    if (Array.isArray(r)) return String(r[0] ?? 'User');
    if (r != null && r !== '') return String(r);
    return 'User';
  } catch {
    return 'User';
  }
}
