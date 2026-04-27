import { AnimatePresence, motion } from 'framer-motion';
import { CheckCircle2, AlertTriangle, XCircle, Info } from 'lucide-react';

const styles = {
  error: {
    Icon: XCircle,
    cls: 'bg-rose-500/10 text-rose-600 dark:text-rose-300 border-rose-500/30',
  },
  success: {
    Icon: CheckCircle2,
    cls: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-300 border-emerald-500/30',
  },
  warning: {
    Icon: AlertTriangle,
    cls: 'bg-amber-500/10 text-amber-600 dark:text-amber-300 border-amber-500/30',
  },
  info: {
    Icon: Info,
    cls: 'bg-cyan-500/10 text-cyan-600 dark:text-cyan-300 border-cyan-500/30',
  },
};

const Alert = ({ message, type = 'error' }) => {
  const conf = styles[type] || styles.error;
  const { Icon } = conf;

  return (
    <AnimatePresence>
      {message && (
        <motion.div
          initial={{ opacity: 0, y: -6, scale: 0.98 }}
          animate={{ opacity: 1, y: 0, scale: 1 }}
          exit={{ opacity: 0, y: -6, scale: 0.98 }}
          transition={{ duration: 0.2 }}
          className={`mb-4 flex items-start gap-2.5 rounded-xl border px-4 py-3 text-sm ${conf.cls}`}
        >
          <Icon size={18} className="mt-0.5 shrink-0" />
          <span className="leading-relaxed">{message}</span>
        </motion.div>
      )}
    </AnimatePresence>
  );
};

export default Alert;
