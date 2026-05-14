/** ML match score (0–100): badge label + Tailwind classes for light/dark. */
export function getMatchQuality(score) {
  const s = typeof score === 'number' && !Number.isNaN(score) ? score : 0;
  if (s > 75) {
    return {
      label: 'Strong match',
      className:
        'bg-emerald-500/15 text-emerald-800 dark:text-emerald-300 border-emerald-500/35',
    };
  }
  if (s >= 58) {
    return {
      label: 'Moderate match',
      className:
        'bg-amber-500/15 text-amber-800 dark:text-amber-300 border-amber-500/35',
    };
  }
  return {
    label: 'Bad match',
    className:
      'bg-rose-500/15 text-rose-800 dark:text-rose-300 border-rose-500/35',
  };
}
