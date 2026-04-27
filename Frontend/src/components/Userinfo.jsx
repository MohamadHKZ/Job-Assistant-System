import { Mail, BadgeCheck } from 'lucide-react';

const UserInfo = ({ user }) => {
  const email = user?.email || '';
  const initial = (user?.name?.[0] || email[0] || 'U').toUpperCase();

  return (
    <div className="flex items-center gap-3 mb-6">
      <div className="grid place-items-center w-12 h-12 rounded-2xl bg-gradient-to-br from-emerald-400 to-cyan-400 text-slate-900 font-bold text-lg shadow-lg shadow-emerald-500/25">
        {initial}
      </div>
      <div className="min-w-0">
        <div className="flex items-center gap-1.5">
          <span className="text-base font-semibold text-slate-900 dark:text-slate-50 truncate">
            {user?.name || 'Welcome back'}
          </span>
          <BadgeCheck size={14} className="text-emerald-500 dark:text-emerald-400" />
        </div>
        <div className="flex items-center gap-1 text-xs text-slate-500 dark:text-slate-400">
          <Mail size={12} />
          <span className="truncate">{email || 'Not signed in'}</span>
        </div>
      </div>
    </div>
  );
};

export default UserInfo;
