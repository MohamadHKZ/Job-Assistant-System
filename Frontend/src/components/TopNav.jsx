import { motion } from 'framer-motion';
import { UserRound, Sparkles, TrendingUp, LogOut } from 'lucide-react';
import ThemeToggle from './ThemeToggle';
import Brand from './Brand';

const NAV_ITEMS = [
  { id: 'profile', label: 'Profile', icon: UserRound },
  { id: 'jobs', label: 'Jobs', icon: Sparkles },
  { id: 'trends', label: 'Trends', icon: TrendingUp },
];

const TopNav = ({ currentView, setCurrentView, onLogout, user }) => {
  return (
    <header className="sticky top-0 z-40">
      <div className="surface border-b">
        <div className="max-w-7xl mx-auto flex items-center justify-between gap-4 px-4 sm:px-6 py-3">
          {/* Brand */}
          <Brand
            size="md"
            nameClassName="text-slate-900 dark:text-slate-50"
          />

          {/* Nav items */}
          <nav className="hidden md:flex items-center gap-1 p-1 rounded-2xl border border-slate-200 dark:border-slate-800 bg-white/60 dark:bg-slate-900/40 backdrop-blur">
            {NAV_ITEMS.map(({ id, label, icon: Icon }) => {
              const active = currentView === id;
              return (
                <button
                  key={id}
                  type="button"
                  onClick={() => setCurrentView(id)}
                  className={`relative inline-flex items-center gap-2 px-3.5 py-2 rounded-xl text-sm font-medium transition-colors focus-ring ${
                    active
                      ? 'text-slate-900 dark:text-white'
                      : 'text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-slate-100'
                  }`}
                >
                  {active && (
                    <motion.span
                      layoutId="navPill"
                      className="absolute inset-0 rounded-xl bg-gradient-to-r from-emerald-400/20 to-cyan-400/20 border border-emerald-400/30 shadow-inner"
                      transition={{ type: 'spring', stiffness: 350, damping: 30 }}
                    />
                  )}
                  <Icon size={16} className="relative z-10" />
                  <span className="relative z-10">{label}</span>
                </button>
              );
            })}
          </nav>

          {/* Right cluster */}
          <div className="flex items-center gap-2">
            <ThemeToggle />
            <div className="hidden sm:flex items-center gap-2 px-3 py-1.5 rounded-xl bg-slate-100 dark:bg-slate-800/70 border border-slate-200 dark:border-slate-700 max-w-[200px]">
              <div className="w-6 h-6 rounded-lg bg-gradient-to-br from-emerald-400 to-cyan-400 grid place-items-center text-[11px] font-bold text-slate-900">
                {(user?.email?.[0] || 'U').toUpperCase()}
              </div>
              <span className="text-xs text-slate-700 dark:text-slate-200 truncate">
                {user?.email || 'Account'}
              </span>
            </div>
            <button
              type="button"
              onClick={onLogout}
              aria-label="Log out"
              title="Log out"
              className="inline-flex items-center justify-center w-10 h-10 rounded-xl border border-slate-200 dark:border-slate-700 bg-white/60 dark:bg-slate-800/60 text-slate-600 dark:text-slate-300 hover:text-rose-500 dark:hover:text-rose-400 hover:border-rose-400/50 transition-colors focus-ring"
            >
              <LogOut size={18} />
            </button>
          </div>
        </div>

        {/* Mobile nav */}
        <div className="md:hidden px-4 pb-3">
          <div className="flex items-center gap-1 p-1 rounded-2xl border border-slate-200 dark:border-slate-800 bg-white/60 dark:bg-slate-900/40 backdrop-blur">
            {NAV_ITEMS.map(({ id, label, icon: Icon }) => {
              const active = currentView === id;
              return (
                <button
                  key={id}
                  type="button"
                  onClick={() => setCurrentView(id)}
                  className={`relative flex-1 inline-flex items-center justify-center gap-2 px-3 py-2 rounded-xl text-sm font-medium transition-colors focus-ring ${
                    active
                      ? 'text-slate-900 dark:text-white'
                      : 'text-slate-600 dark:text-slate-400'
                  }`}
                >
                  {active && (
                    <motion.span
                      layoutId="navPillMobile"
                      className="absolute inset-0 rounded-xl bg-gradient-to-r from-emerald-400/20 to-cyan-400/20 border border-emerald-400/30"
                      transition={{ type: 'spring', stiffness: 350, damping: 30 }}
                    />
                  )}
                  <Icon size={16} className="relative z-10" />
                  <span className="relative z-10">{label}</span>
                </button>
              );
            })}
          </div>
        </div>
      </div>
    </header>
  );
};

export default TopNav;
