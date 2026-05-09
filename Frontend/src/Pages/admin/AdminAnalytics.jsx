import { useCallback, useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { BarChart3, Users, Briefcase, RefreshCw } from 'lucide-react';
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
} from 'recharts';
import { getAnalytics } from '../../api/admin';
import { useTheme } from '../../theme/ThemeContext';
import Alert from '../../components/Alert';
import Button from '../../components/Button';
import LoadingSpinner from '../../components/Loadingspinner';

const Kpi = ({ icon: Icon, label, value, accent = 'emerald' }) => {
  const accents = {
    emerald: 'text-emerald-600 dark:text-emerald-400 bg-emerald-500/10 border-emerald-500/20',
    cyan: 'text-cyan-600 dark:text-cyan-400 bg-cyan-500/10 border-cyan-500/20',
  };
  return (
    <motion.div
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      className="surface rounded-2xl p-5 flex items-center gap-4 border border-slate-200 dark:border-slate-700"
    >
      <div
        className={`grid place-items-center w-12 h-12 rounded-2xl border ${accents[accent] || accents.emerald}`}
      >
        <Icon size={22} />
      </div>
      <div>
        <div className="text-[11px] uppercase tracking-wider text-slate-500 dark:text-slate-400">
          {label}
        </div>
        <div className="text-2xl font-bold text-slate-900 dark:text-slate-50">{value}</div>
      </div>
    </motion.div>
  );
};

const AdminAnalytics = ({ token }) => {
  const { theme } = useTheme();
  const isDark = theme === 'dark';
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await getAnalytics(token);
      setData(res);
    } catch (e) {
      setError(e);
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    load();
  }, [load]);

  const chartData =
    data?.trendingSkills?.map((t, index) => {
      const raw = String(t.skill ?? '').trim();
      const fullSkill = raw.length > 0 ? raw : `(unnamed skill ${index + 1})`;
      const short =
        fullSkill.length > 28 ? `${fullSkill.slice(0, 28)}…` : fullSkill;
      return {
        skill: short,
        fullSkill,
        count: t.count,
      };
    }) ?? [];

  if (loading && !data) {
    return (
      <div className="flex justify-center py-20">
        <LoadingSpinner />
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto px-4 py-8 space-y-8">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h2 className="text-2xl font-bold text-slate-900 dark:text-slate-50 flex items-center gap-2">
            <BarChart3 className="text-violet-500" size={26} />
            Analytics
          </h2>
          <p className="text-slate-600 dark:text-slate-400 mt-1 text-sm">
            Users, jobs in catalog, and trending technical skills (month window).
          </p>
        </div>
        <Button variant="secondary" icon={RefreshCw} onClick={load} loading={loading}>
          Refresh
        </Button>
      </div>

      {error && <Alert type="error" message={error} />}

      <div className="grid sm:grid-cols-2 gap-4">
        <Kpi icon={Users} label="Total users" value={data?.totalUsers ?? '—'} accent="emerald" />
        <Kpi
          icon={Briefcase}
          label="Total job posts"
          value={data?.totalMatchedJobs ?? '—'}
          accent="cyan"
        />
      </div>

      <motion.div
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        className="surface rounded-2xl p-4 md:p-6 border border-slate-200 dark:border-slate-700"
      >
        <h3 className="text-sm font-semibold text-slate-700 dark:text-slate-300 mb-4">
          Trending skills (aggregated)
        </h3>
        <div className="h-[320px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart
              data={chartData}
              layout="vertical"
              margin={{ left: 16, right: 16, top: 8, bottom: 8 }}
            >
              <CartesianGrid strokeDasharray="3 3" stroke={isDark ? '#334155' : '#e2e8f0'} />
              <XAxis type="number" stroke={isDark ? '#94a3b8' : '#64748b'} />
              <YAxis
                type="category"
                dataKey="skill"
                width={168}
                interval={0}
                stroke={isDark ? '#94a3b8' : '#64748b'}
                tick={{ fontSize: 11 }}
                tickFormatter={(v) => (v == null || v === '' ? '—' : String(v))}
              />
              <Tooltip
                contentStyle={{
                  background: isDark ? '#0f172a' : '#fff',
                  border: isDark ? '1px solid #334155' : '1px solid #e2e8f0',
                  borderRadius: 12,
                  fontSize: 12,
                }}
                labelFormatter={(_, payload) => payload?.[0]?.payload?.fullSkill ?? ''}
              />
              <Bar dataKey="count" fill="#34d399" radius={[0, 8, 8, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </motion.div>
    </div>
  );
};

export default AdminAnalytics;
