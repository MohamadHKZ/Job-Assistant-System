// Centralised API error type + response parser.
//
// The backend emits RFC 7807 ProblemDetails for every non-2xx response:
//   { type, title, status, detail?, instance?, traceId, errors? }
//
// `parseApiError(response)` reads that body safely (falling back to text or a
// generic message when the body isn't JSON) and returns an `ApiError` you can
// either throw or pass directly to the <Alert /> component.

const FALLBACK_TITLES = {
  400: "Bad request",
  401: "You're not signed in",
  403: "You don't have access to that",
  404: "Not found",
  409: "Conflict",
  422: "We couldn't process that",
  429: "Too many requests",
  500: "Something went wrong",
  502: "An upstream service failed",
  503: "Service unavailable",
};

export class ApiError extends Error {
  constructor({
    title,
    detail = null,
    status = 0,
    type = null,
    instance = null,
    traceId = null,
    fieldErrors = null,
    raw = null,
  }) {
    super(title);
    this.name = "ApiError";
    this.title = title;
    this.detail = detail;
    this.status = status;
    this.type = type;
    this.instance = instance;
    this.traceId = traceId;
    // Map<string, string[]>-shaped object, or null
    this.fieldErrors = fieldErrors;
    // The original parsed body (or text fallback) for debugging
    this.raw = raw;
  }

  /** True if the API returned per-field validation errors. */
  get hasFieldErrors() {
    return this.fieldErrors && Object.keys(this.fieldErrors).length > 0;
  }
}

/**
 * Read a fetch Response that came back with !ok and turn it into an ApiError.
 * Never throws.
 */
export async function parseApiError(response) {
  const status = response.status;
  const fallbackTitle =
    FALLBACK_TITLES[status] || `Request failed (${status || "network error"})`;

  // Prefer JSON when the server says it's JSON or problem+json.
  const contentType = (response.headers.get("content-type") || "").toLowerCase();
  const looksJson =
    contentType.includes("application/json") ||
    contentType.includes("application/problem+json");

  if (looksJson) {
    try {
      const body = await response.json();
      return new ApiError({
        title: body.title || fallbackTitle,
        detail: body.detail || null,
        status: body.status || status,
        type: body.type || null,
        instance: body.instance || null,
        traceId: body.traceId || body.trace_id || null,
        fieldErrors: body.errors || null,
        raw: body,
      });
    } catch {
      // fall through to text branch
    }
  }

  // Non-JSON body (HTML error page, plain string, etc.) — keep something useful.
  let text = "";
  try {
    text = await response.text();
  } catch {
    // ignore
  }

  return new ApiError({
    title: fallbackTitle,
    detail: text && text.length < 500 ? text : null,
    status,
    raw: text || null,
  });
}
