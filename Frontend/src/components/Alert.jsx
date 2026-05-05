import { useState } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import {
  CheckCircle2,
  AlertTriangle,
  XCircle,
  Info,
  ChevronDown,
} from 'lucide-react';

const styles = {
  error: {
    Icon: XCircle,
    cls: 'bg-rose-500/10 text-rose-600 dark:text-rose-300 border-rose-500/30',
    accent: 'text-rose-600 dark:text-rose-300',
    muted: 'text-rose-500/80 dark:text-rose-300/70',
  },
  success: {
    Icon: CheckCircle2,
    cls: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-300 border-emerald-500/30',
    accent: 'text-emerald-700 dark:text-emerald-300',
    muted: 'text-emerald-600/80 dark:text-emerald-300/70',
  },
  warning: {
    Icon: AlertTriangle,
    cls: 'bg-amber-500/10 text-amber-600 dark:text-amber-300 border-amber-500/30',
    accent: 'text-amber-700 dark:text-amber-300',
    muted: 'text-amber-600/80 dark:text-amber-300/70',
  },
  info: {
    Icon: Info,
    cls: 'bg-cyan-500/10 text-cyan-600 dark:text-cyan-300 border-cyan-500/30',
    accent: 'text-cyan-700 dark:text-cyan-300',
    muted: 'text-cyan-600/80 dark:text-cyan-300/70',
  },
};

// Pulls a `title` + optional details out of either a plain string or an
// ApiError-shaped object. Anything truthy renders.
function normalizeMessage(message) {
  if (!message) return null;

  if (typeof message === 'string') {
    return { title: message, detail: null, fieldErrors: null, traceId: null };
  }

  // ApiError instance OR a plain object with the same fields.
  return {
    title: message.title || message.message || 'Something went wrong',
    detail: message.detail || null,
    fieldErrors: message.fieldErrors || null,
    traceId: message.traceId || null,
  };
}

function flattenFieldErrors(fieldErrors) {
  if (!fieldErrors) return [];
  return Object.entries(fieldErrors).flatMap(([field, msgs]) => {
    const list = Array.isArray(msgs) ? msgs : [String(msgs)];
    return list.map((m) => ({ field, message: m }));
  });
}

const Alert = ({ message, type = 'error' }) => {
  const conf = styles[type] || styles.error;
  const { Icon } = conf;
  const [expanded, setExpanded] = useState(false);

  const data = normalizeMessage(message);
  if (!data) return <AnimatePresence />;

  const fieldList = flattenFieldErrors(data.fieldErrors);
  const hasMore = !!(data.detail || fieldList.length > 0);

  return (
    <AnimatePresence>
      <motion.div
        key="alert"
        initial={{ opacity: 0, y: -6, scale: 0.98 }}
        animate={{ opacity: 1, y: 0, scale: 1 }}
        exit={{ opacity: 0, y: -6, scale: 0.98 }}
        transition={{ duration: 0.2 }}
        className={`mb-4 rounded-xl border px-4 py-3 text-sm ${conf.cls}`}
      >
        <div className="flex items-start gap-2.5">
          <Icon size={18} className="mt-0.5 shrink-0" />
          <div className="flex-1 min-w-0">
            <div className={`font-medium leading-relaxed ${conf.accent}`}>
              {data.title}
            </div>

            {data.traceId && (
              <p
                className={`mt-1.5 text-[11px] font-mono select-all ${conf.muted}`}
                title="Quote this id when contacting support"
              >
                Reference id: {data.traceId}
              </p>
            )}

            {hasMore && (
              <button
                type="button"
                onClick={() => setExpanded((v) => !v)}
                className={`mt-1 inline-flex items-center gap-1 text-xs font-medium ${conf.muted} hover:underline focus-ring rounded`}
                aria-expanded={expanded}
              >
                <motion.span
                  animate={{ rotate: expanded ? 180 : 0 }}
                  transition={{ duration: 0.15 }}
                  className="inline-flex"
                >
                  <ChevronDown size={14} />
                </motion.span>
                {expanded ? 'Hide details' : 'Show details'}
              </button>
            )}
          </div>
        </div>

        <AnimatePresence initial={false}>
          {hasMore && expanded && (
            <motion.div
              key="details"
              initial={{ opacity: 0, height: 0 }}
              animate={{ opacity: 1, height: 'auto' }}
              exit={{ opacity: 0, height: 0 }}
              transition={{ duration: 0.18 }}
              className="overflow-hidden"
            >
              <div className="pt-2 pl-7 space-y-2">
                {data.detail && (
                  <p className={`text-xs leading-relaxed ${conf.muted}`}>
                    {data.detail}
                  </p>
                )}

                {fieldList.length > 0 && (
                  <ul className={`text-xs space-y-0.5 list-disc pl-4 ${conf.muted}`}>
                    {fieldList.map((f, i) => (
                      <li key={`${f.field}-${i}`}>
                        <span className="font-medium">{f.field}:</span> {f.message}
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </motion.div>
    </AnimatePresence>
  );
};

export default Alert;
