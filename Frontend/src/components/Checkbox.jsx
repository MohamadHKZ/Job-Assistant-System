import { motion, AnimatePresence } from 'framer-motion';
import { Check } from 'lucide-react';

const Checkbox = ({ name, label, className = '', checked, onChange }) => {
  return (
    <label
      className={`group inline-flex items-center gap-2.5 cursor-pointer select-none text-sm text-slate-700 dark:text-slate-200 ${className}`}
    >
      <span className="relative inline-flex">
        <input
          type="checkbox"
          name={name}
          checked={checked}
          onChange={onChange}
          className="sr-only"
        />
        <span
          className={`grid place-items-center w-5 h-5 rounded-md border transition-colors ${
            checked
              ? 'bg-emerald-500 border-emerald-500 dark:bg-emerald-400 dark:border-emerald-400'
              : 'bg-white dark:bg-slate-900/60 border-slate-300 dark:border-slate-600 group-hover:border-emerald-400/70'
          }`}
        >
          <AnimatePresence>
            {checked && (
              <motion.span
                initial={{ scale: 0.4, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                exit={{ scale: 0.4, opacity: 0 }}
                transition={{ duration: 0.15 }}
              >
                <Check size={14} className="text-white dark:text-slate-900" strokeWidth={3} />
              </motion.span>
            )}
          </AnimatePresence>
        </span>
      </span>
      <span>{label}</span>
    </label>
  );
};

export default Checkbox;
