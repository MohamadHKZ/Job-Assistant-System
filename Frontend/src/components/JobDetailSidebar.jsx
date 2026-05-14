import { useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import {
  Building2,
  MapPin,
  Briefcase,
  ExternalLink,
  X,
  GraduationCap,
} from 'lucide-react';
import { getMatchQuality } from '../utils/matchQuality';

const JobDetailSidebar = ({ job, onClose }) => {
  useEffect(() => {
    if (!job) return undefined;
    const onKey = (e) => {
      if (e.key === 'Escape') onClose?.();
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [job, onClose]);

  const open = Boolean(job);
  const skills = Array.isArray(job?.technicalSkills) ? job.technicalSkills : [];
  const badge = getMatchQuality(job?.score);

  return (
    <AnimatePresence>
      {open && (
        <motion.div
          key="job-detail-overlay"
          className="fixed inset-0 z-50 flex justify-end"
          role="dialog"
          aria-modal="true"
          aria-labelledby="job-detail-title"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.2 }}
        >
          <button
            type="button"
            className="absolute inset-0 bg-slate-900/50 dark:bg-black/60"
            aria-label="Close panel"
            onClick={onClose}
          />
          <motion.aside
            initial={{ x: '100%' }}
            animate={{ x: 0 }}
            exit={{ x: '100%' }}
            transition={{ type: 'tween', duration: 0.25, ease: [0.32, 0.72, 0, 1] }}
            className="relative flex h-full w-full max-w-[420px] flex-col border-l border-slate-200 bg-white shadow-2xl dark:border-slate-700 dark:bg-slate-900"
          >
            <div className="flex shrink-0 items-start justify-between gap-3 border-b border-slate-200 p-4 dark:border-slate-700">
              <div className="min-w-0">
                <h2
                  id="job-detail-title"
                  className="text-lg font-semibold leading-snug text-slate-900 dark:text-slate-50"
                >
                  {job.jobTitle}
                </h2>
                {job.companyName && (
                  <p className="mt-1 flex items-center gap-1.5 text-sm text-slate-600 dark:text-slate-400">
                    <Building2 size={14} className="shrink-0" />
                    {job.companyName}
                  </p>
                )}
                <span
                  className={`mt-2 inline-flex rounded-lg border px-2 py-0.5 text-xs font-semibold ${badge.className}`}
                >
                  {badge.label}
                  {typeof job.score === 'number' && (
                    <span className="ml-1.5 font-mono opacity-80">({Math.round(job.score)})</span>
                  )}
                </span>
              </div>
              <button
                type="button"
                onClick={onClose}
                className="rounded-lg border border-slate-200 p-2 text-slate-600 hover:bg-slate-50 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-800"
                aria-label="Close"
              >
                <X size={18} />
              </button>
            </div>

            <div className="min-h-0 flex-1 space-y-4 overflow-y-auto p-4">
              <div className="flex flex-wrap gap-x-3 gap-y-1 text-xs text-slate-600 dark:text-slate-400">
                {job.location && (
                  <span className="inline-flex items-center gap-1">
                    <MapPin size={12} />
                    {job.location}
                  </span>
                )}
                {job.jobType && (
                  <span className="inline-flex items-center gap-1">
                    <Briefcase size={12} />
                    {job.jobType}
                  </span>
                )}
                {job.experienceLevel && (
                  <span className="inline-flex items-center gap-1">
                    <GraduationCap size={12} />
                    {job.experienceLevel}
                  </span>
                )}
              </div>

              {job.jobDescription && (
                <div>
                  <h3 className="mb-1.5 text-xs font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-500">
                    Description
                  </h3>
                  <p className="whitespace-pre-wrap text-sm leading-relaxed text-slate-700 dark:text-slate-300">
                    {job.jobDescription}
                  </p>
                </div>
              )}

              {skills.length > 0 && (
                <div>
                  <h3 className="mb-2 text-xs font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-500">
                    Technical skills
                  </h3>
                  <div className="flex flex-wrap gap-1.5">
                    {skills.map((skill, i) => (
                      <span
                        key={`${skill}-${i}`}
                        className="rounded-md border border-slate-200 bg-slate-50 px-2 py-1 text-[11px] font-medium text-slate-700 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-300"
                      >
                        {skill}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>

            {job.url && (
              <div className="shrink-0 border-t border-slate-200 p-4 dark:border-slate-700">
                <a
                  href={job.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex w-full items-center justify-center gap-2 rounded-xl bg-gradient-to-r from-emerald-500 to-emerald-400 py-3 text-sm font-semibold text-slate-900 hover:shadow-lg hover:shadow-emerald-500/25"
                >
                  Apply
                  <ExternalLink size={16} />
                </a>
              </div>
            )}
          </motion.aside>
        </motion.div>
      )}
    </AnimatePresence>
  );
};

export default JobDetailSidebar;
