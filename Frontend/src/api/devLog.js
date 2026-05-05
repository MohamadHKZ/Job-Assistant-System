/** Dev-only console logging; stripped from production bundles by Vite. */
export function devLog(...args) {
  if (import.meta.env.DEV) {
    // eslint-disable-next-line no-console -- intentional dev-only tracing
    console.log(...args);
  }
}
