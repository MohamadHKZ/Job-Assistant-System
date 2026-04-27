import { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X, Plus } from 'lucide-react';

const SkillsInput = ({ label, skills, setSkills, placeholder }) => {
  const [draft, setDraft] = useState('');

  const chips = (skills || []).filter((s) => s && s.trim());

  const commit = (newChips) => {
    setSkills(newChips.length ? newChips : ['']);
  };

  const addChip = (raw) => {
    if (!raw) return;
    raw
      .split(',')
      .map((p) => p.trim())
      .filter(Boolean)
      .forEach((value) => {
        if (!chips.some((c) => c.toLowerCase() === value.toLowerCase())) {
          chips.push(value);
        }
      });
    commit([...chips]);
    setDraft('');
  };

  const removeChip = (index) => {
    const next = [...chips];
    next.splice(index, 1);
    commit(next);
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter' || e.key === ',') {
      e.preventDefault();
      addChip(draft);
    } else if (e.key === 'Backspace' && draft === '' && chips.length > 0) {
      e.preventDefault();
      removeChip(chips.length - 1);
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-2">
        <label className="text-sm font-medium text-slate-700 dark:text-slate-200">
          {label}
        </label>
        <span className="text-xs text-slate-500 dark:text-slate-400">
          {chips.length} {chips.length === 1 ? 'item' : 'items'}
        </span>
      </div>

      <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900/40 p-2.5 focus-within:border-emerald-400 dark:focus-within:border-emerald-400 focus-within:ring-2 focus-within:ring-emerald-400/30 transition-colors">
        <div className="flex flex-wrap gap-1.5">
          <AnimatePresence initial={false}>
            {chips.map((skill, index) => (
              <motion.span
                key={`${skill}-${index}`}
                layout
                initial={{ opacity: 0, scale: 0.8, y: -4 }}
                animate={{ opacity: 1, scale: 1, y: 0 }}
                exit={{ opacity: 0, scale: 0.8 }}
                transition={{ duration: 0.15 }}
                className="group inline-flex items-center gap-1 px-2.5 py-1 rounded-lg bg-emerald-500/10 text-emerald-700 dark:text-emerald-300 border border-emerald-500/30 text-xs font-medium"
              >
                {skill}
                <button
                  type="button"
                  onClick={() => removeChip(index)}
                  aria-label={`Remove ${skill}`}
                  className="grid place-items-center w-4 h-4 rounded-md hover:bg-emerald-500/20 transition-colors"
                >
                  <X size={12} />
                </button>
              </motion.span>
            ))}
          </AnimatePresence>

          <input
            type="text"
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
            onKeyDown={handleKeyDown}
            onBlur={() => draft && addChip(draft)}
            placeholder={
              chips.length === 0
                ? placeholder || `Type a ${label.toLowerCase()} and press Enter`
                : 'Add another...'
            }
            className="flex-1 min-w-[140px] bg-transparent text-sm text-slate-900 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 outline-none px-1.5 py-1"
          />
        </div>
      </div>

      <div className="mt-1.5 flex items-center gap-1.5 text-[11px] text-slate-500 dark:text-slate-500">
        <Plus size={11} />
        <span>Press Enter or comma to add. Backspace removes the last.</span>
      </div>
    </div>
  );
};

export default SkillsInput;
