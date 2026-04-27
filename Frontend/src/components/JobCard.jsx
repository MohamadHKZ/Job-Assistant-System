import { useState } from 'react';
import { motion } from 'framer-motion';
import {
  Building2,
  MapPin,
  Briefcase,
  ExternalLink,
  ChevronDown,
} from 'lucide-react';

const VISIBLE_SKILLS = 6;

const JobCard = ({ job, index = 0 }) => {
  const [expanded, setExpanded] = useState(false);

  const skills = Array.isArray(job.technicalSkills) ? job.technicalSkills : [];
  const visibleSkills = skills.slice(0, VISIBLE_SKILLS);
  const overflow = skills.length - visibleSkills.length;

  const initials = (job.companyName || '?')
    .split(' ')
    .map((w) => w[0])
    .filter(Boolean)
    .slice(0, 2)
    .join('')
    .toUpperCase();

  return (
    <motion.article
      layout
      initial={{ opacity: 0, y: 12 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3, delay: index * 0.04 }}
      whileHover={{ y: -2 }}
      className="surface rounded-2xl p-5 hover:border-emerald-400/40 hover:shadow-lg hover:shadow-emerald-500/10 transition-all"
    >
      <div className="flex items-start gap-3">
        <div className="grid place-items-center w-11 h-11 rounded-xl bg-gradient-to-br from-emerald-400/30 to-cyan-400/30 text-emerald-700 dark:text-emerald-300 font-bold text-sm border border-emerald-500/20 shrink-0">
          {initials || <Building2 size={18} />}
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="font-semibold text-base text-slate-900 dark:text-slate-50 leading-snug">
            {job.jobTitle}
          </h3>
          <div className="mt-0.5 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-slate-600 dark:text-slate-400">
            {job.companyName && (
              <span className="inline-flex items-center gap-1">
                <Building2 size={12} />
                {job.companyName}
              </span>
            )}
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
          </div>
        </div>
      </div>

      {job.jobDescription && (
        <div className="mt-4">
          <p
            className={`text-sm text-slate-600 dark:text-slate-300 leading-relaxed ${
              expanded ? '' : 'line-clamp-3'
            }`}
          >
            {job.jobDescription}
          </p>
          {job.jobDescription.length > 180 && (
            <button
              type="button"
              onClick={() => setExpanded((v) => !v)}
              className="mt-1.5 inline-flex items-center gap-1 text-xs font-medium text-emerald-600 dark:text-emerald-400 hover:text-emerald-500"
            >
              {expanded ? 'Show less' : 'Read more'}
              <ChevronDown
                size={12}
                className={`transition-transform ${expanded ? 'rotate-180' : ''}`}
              />
            </button>
          )}
        </div>
      )}

      {visibleSkills.length > 0 && (
        <div className="mt-4 flex flex-wrap gap-1.5">
          {visibleSkills.map((skill, i) => (
            <span
              key={`${skill}-${i}`}
              className="px-2 py-1 rounded-md bg-slate-100 dark:bg-slate-800/80 text-slate-700 dark:text-slate-300 text-[11px] font-medium border border-slate-200 dark:border-slate-700"
            >
              {skill}
            </span>
          ))}
          {overflow > 0 && (
            <span className="px-2 py-1 rounded-md bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 text-[11px] font-medium border border-emerald-500/30">
              +{overflow} more
            </span>
          )}
        </div>
      )}

      {job.url && (
        <div className="mt-5 flex items-center justify-between">
          <span className="text-[11px] uppercase tracking-wider text-slate-500 dark:text-slate-500">
            Recommended for you
          </span>
          <a
            href={job.url}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center gap-1.5 px-4 py-2 rounded-xl bg-gradient-to-r from-emerald-500 to-emerald-400 text-slate-900 text-sm font-semibold hover:shadow-lg hover:shadow-emerald-500/25 transition-shadow"
          >
            Apply
            <ExternalLink size={14} />
          </a>
        </div>
      )}
    </motion.article>
  );
};

export default JobCard;
