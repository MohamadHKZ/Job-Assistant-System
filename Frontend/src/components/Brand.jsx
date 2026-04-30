import { Briefcase } from 'lucide-react';
import {
  BRAND_NAME_PREFIX,
  BRAND_NAME_SUFFIX,
  BRAND_TAGLINE,
} from '../constants/brand';

const SIZES = {
  sm: {
    box: 'w-9 h-9 rounded-xl',
    icon: 16,
    name: 'font-bold tracking-tight',
    tagline: 'text-[10px] uppercase tracking-[0.18em]',
    gap: 'gap-2.5',
  },
  md: {
    box: 'w-9 h-9 rounded-xl shadow-lg shadow-emerald-500/20',
    icon: 18,
    name: 'font-bold tracking-tight',
    tagline: 'text-[10px] uppercase tracking-[0.18em]',
    gap: 'gap-2.5',
  },
  lg: {
    box: 'w-11 h-11 rounded-2xl shadow-xl shadow-emerald-500/30',
    icon: 20,
    name: 'text-xl font-bold tracking-tight',
    tagline: 'text-[11px] uppercase tracking-[0.2em]',
    gap: 'gap-3',
  },
};

const Brand = ({
  size = 'md',
  showTagline = true,
  nameClassName = '',
  taglineClassName = '',
  className = '',
}) => {
  const s = SIZES[size] ?? SIZES.md;

  return (
    <div className={`flex items-center ${s.gap} ${className}`}>
      <div
        className={`${s.box} bg-gradient-to-br from-emerald-400 to-cyan-400 grid place-items-center`}
      >
        <Briefcase size={s.icon} className="text-slate-900" />
      </div>
      <div className="leading-tight">
        <div className={`${s.name} ${nameClassName}`}>
          {BRAND_NAME_PREFIX}
          <span className="text-gradient-brand">{BRAND_NAME_SUFFIX}</span>
        </div>
        {showTagline && (
          <div
            className={`${s.tagline} text-slate-500 dark:text-slate-400 ${taglineClassName}`}
          >
            {BRAND_TAGLINE}
          </div>
        )}
      </div>
    </div>
  );
};

export default Brand;
