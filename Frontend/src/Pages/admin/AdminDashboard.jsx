import { useState } from 'react';
import { motion } from 'framer-motion';
import {
  LayoutDashboard,
  Database,
  FileText,
  Sliders,
  BarChart3,
  LogOut,
} from 'lucide-react';
import Brand from '../../components/Brand';
import ThemeToggle from '../../components/ThemeToggle';
import Button from '../../components/Button';
import AdminJobSources from './AdminJobSources';
import AdminLogs from './AdminLogs';
import AdminSettings from './AdminSettings';
import AdminAnalytics from './AdminAnalytics';

const NAV = [
  { id: 'sources', label: 'Job sources', icon: Database },
  { id: 'logs', label: 'Logs', icon: FileText },
  { id: 'settings', label: 'Settings', icon: Sliders },
  { id: 'analytics', label: 'Analytics', icon: BarChart3 },
];

const AdminDashboard = ({ token, user, onLogout }) => {
  const [tab, setTab] = useState('sources');

  return (
    <div className="min-h-screen flex flex-col md:flex-row bg-slate-50 dark:bg-slate-950">
      <aside className="md:w-64 shrink-0 border-b md:border-b-0 md:border-r border-slate-200 dark:border-slate-800 bg-white/90 dark:bg-slate-900/90 backdrop-blur-md">
        <div className="p-5 flex flex-row md:flex-col gap-4 md:gap-8 items-center md:items-stretch justify-between md:justify-start">
          <Brand size="sm" />
          <nav className="hidden md:flex flex-col gap-1">
            {NAV.map((item) => {
              const Icon = item.icon;
              const active = tab === item.id;
              return (
                <button
                  key={item.id}
                  type="button"
                  onClick={() => setTab(item.id)}
                  className={`flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-colors focus-ring ${
                    active
                      ? 'bg-emerald-500/15 text-emerald-700 dark:text-emerald-300'
                      : 'text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800'
                  }`}
                >
                  <Icon size={18} />
                  {item.label}
                </button>
              );
            })}
          </nav>
          <div className="flex items-center gap-2 md:flex-col md:items-stretch md:mt-auto">
            <ThemeToggle />
            <Button variant="danger" size="sm" icon={LogOut} onClick={onLogout} fullWidth>
              Sign out
            </Button>
          </div>
        </div>
        {/* Mobile tabs */}
        <div className="md:hidden flex overflow-x-auto gap-1 px-3 pb-3 border-t border-slate-200 dark:border-slate-800">
          {NAV.map((item) => {
            const Icon = item.icon;
            const active = tab === item.id;
            return (
              <button
                key={item.id}
                type="button"
                onClick={() => setTab(item.id)}
                className={`shrink-0 flex items-center gap-1.5 rounded-lg px-3 py-2 text-xs font-medium ${
                  active
                    ? 'bg-emerald-500/15 text-emerald-700 dark:text-emerald-300'
                    : 'text-slate-600 dark:text-slate-400'
                }`}
              >
                <Icon size={14} />
                {item.label}
              </button>
            );
          })}
        </div>
      </aside>

      <div className="flex-1 flex flex-col min-w-0">
        <header className="sticky top-0 z-10 border-b border-slate-200 dark:border-slate-800 bg-white/80 dark:bg-slate-900/80 backdrop-blur-md px-4 py-3 flex items-center justify-between gap-3">
          <div className="flex items-center gap-2 text-slate-800 dark:text-slate-100">
            <LayoutDashboard size={20} className="text-emerald-500" />
            <span className="font-semibold">Admin</span>
            <span className="text-slate-400 hidden sm:inline">·</span>
            <span className="text-sm text-slate-500 dark:text-slate-400 truncate hidden sm:inline">
              {user?.email}
            </span>
          </div>
        </header>

        <motion.div
          key={tab}
          initial={{ opacity: 0, y: 6 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.2 }}
          className="flex-1 overflow-auto"
        >
          {tab === 'sources' && <AdminJobSources token={token} />}
          {tab === 'logs' && <AdminLogs token={token} />}
          {tab === 'settings' && <AdminSettings token={token} />}
          {tab === 'analytics' && <AdminAnalytics token={token} />}
        </motion.div>
      </div>
    </div>
  );
};

export default AdminDashboard;
