import { useEffect, useMemo, useState, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import {
  TrendingUp,
  Briefcase,
  Layers,
  Crown,
  RefreshCw,
  Search,
  ChevronRight,
} from 'lucide-react';
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
} from 'recharts';
import { getTrends } from '../../api/trends';
import { useTheme } from '../../theme/ThemeContext';
import Alert from '../../components/Alert';
import EmptyState from '../../components/Emptystate';
import Skeleton from '../../components/Skeleton';

const PERIODS = [
  { id: 'week', label: 'Week' },
  { id: 'month', label: 'Month' },
  { id: 'threeMonths', label: '3 Months' },
];

const ACCENT_CLASSES = {
  emerald:
    'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/20',
  cyan: 'bg-cyan-500/10 text-cyan-600 dark:text-cyan-400 border-cyan-500/20',
  indigo:
    'bg-indigo-500/10 text-indigo-600 dark:text-indigo-400 border-indigo-500/20',
};

const KpiCard = ({ icon: Icon, label, value, accent = 'emerald' }) => (
  <motion.div
    initial={{ opacity: 0, y: 8 }}
    animate={{ opacity: 1, y: 0 }}
    transition={{ duration: 0.3 }}
    className="surface rounded-2xl p-5 flex items-center gap-4"
  >
    <div
      className={`grid place-items-center w-12 h-12 rounded-2xl border ${
        ACCENT_CLASSES[accent] || ACCENT_CLASSES.emerald
      }`}
    >
      <Icon size={20} />
    </div>
    <div>
      <div className="text-[11px] uppercase tracking-wider text-slate-500 dark:text-slate-400">
        {label}
      </div>
      <div className="text-xl font-bold tracking-tight text-slate-900 dark:text-slate-50 mt-0.5 truncate max-w-[260px]">
        {value}
      </div>
    </div>
  </motion.div>
);

const formatPercent = (ratio) => {
  if (typeof ratio !== 'number' || Number.isNaN(ratio)) return '0%';
  return `${(ratio * 100).toFixed(1)}%`;
};

const ChartTooltip = ({ active, payload, isDark }) => {
  if (!active || !payload || payload.length === 0) return null;
  const item = payload[0]?.payload;
  return (
    <div
      className={`rounded-xl px-3 py-2 text-xs shadow-lg ${
        isDark
          ? 'bg-slate-900/95 border border-slate-700 text-slate-100'
          : 'bg-white/95 border border-slate-200 text-slate-800'
      }`}
    >
      <div className="font-semibold">{item?.skill}</div>
      <div className="text-slate-500 dark:text-slate-400">
        {item?.count} jobs &middot; {formatPercent(item?.ratio)}
      </div>
    </div>
  );
};

const TrendsView = ({ token }) => {
  const { theme } = useTheme();
  const isDark = theme === 'dark';

  const [trends, setTrends] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [period, setPeriod] = useState('month');
  const [selectedTitle, setSelectedTitle] = useState(null);
  const [search, setSearch] = useState('');

  const fetchTrends = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await getTrends(token);
      const arr = Array.isArray(data) ? data : [];
      setTrends(arr);
      if (arr.length && !selectedTitle) {
        setSelectedTitle(arr[0].jobTitle);
      }
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
    // selectedTitle intentionally omitted - we only auto-select once on first load
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  useEffect(() => {
    fetchTrends();
  }, [fetchTrends]);

  const filteredTrends = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return trends;
    return trends.filter((t) => t.jobTitle?.toLowerCase().includes(q));
  }, [trends, search]);

  const selected = useMemo(
    () => trends.find((t) => t.jobTitle === selectedTitle) || trends[0],
    [trends, selectedTitle],
  );

  const periodData = useMemo(
    () =>
      selected?.topTechnicalSkills?.[period] || {
        totalSkills: 0,
        topSkills: [],
      },
    [selected, period],
  );

  const chartData = useMemo(() => {
    return [...(periodData.topSkills || [])]
      .slice(0, 10)
      .reverse(); // reverse so largest renders at top in horizontal bar chart
  }, [periodData]);

  const totalJobs = useMemo(
    () => trends.reduce((sum, t) => sum + (t.jobsCount || 0), 0),
    [trends],
  );

  const topJob = trends[0]?.jobTitle || 'N/A';

  if (loading) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 py-8">
        <div className="grid gap-4 sm:grid-cols-3 mb-6">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-24" rounded="rounded-2xl" />
          ))}
        </div>
        <div className="grid gap-4 lg:grid-cols-[320px_1fr]">
          <Skeleton className="h-[500px]" rounded="rounded-2xl" />
          <Skeleton className="h-[500px]" rounded="rounded-2xl" />
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-3xl mx-auto px-4 sm:px-6 py-8">
        <Alert message={error} type="error" />
      </div>
    );
  }

  if (!trends.length) {
    return (
      <div className="max-w-3xl mx-auto px-4 sm:px-6 py-8">
        <EmptyState
          icon={TrendingUp}
          message="No trend data yet"
          description="Trends will populate as new jobs are collected and analyzed."
        />
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 py-8">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-3 mb-5">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-slate-50">
            Trending skills
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400 mt-0.5">
            What employers are asking for, broken down by job title and time window.
          </p>
        </div>
        <button
          type="button"
          onClick={fetchTrends}
          className="inline-flex items-center gap-2 px-3.5 py-2 rounded-xl border border-slate-200 dark:border-slate-700 bg-white/60 dark:bg-slate-800/60 text-sm font-medium text-slate-700 dark:text-slate-200 hover:border-emerald-400/40 hover:text-emerald-600 dark:hover:text-emerald-400 transition-colors focus-ring"
        >
          <RefreshCw size={14} />
          Refresh
        </button>
      </div>

      {/* KPIs */}
      <div className="grid gap-4 sm:grid-cols-3 mb-6">
        <KpiCard icon={Briefcase} label="Tracked job titles" value={trends.length} />
        <KpiCard
          icon={Layers}
          label="Open jobs (sum)"
          value={totalJobs.toLocaleString()}
          accent="cyan"
        />
        <KpiCard icon={Crown} label="Top job title" value={topJob} accent="emerald" />
      </div>

      {/* Period toggle */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mb-5">
        <div className="inline-flex p-1 rounded-2xl border border-slate-200 dark:border-slate-700 bg-white/60 dark:bg-slate-900/40 self-start">
          {PERIODS.map((p) => {
            const active = period === p.id;
            return (
              <button
                key={p.id}
                type="button"
                onClick={() => setPeriod(p.id)}
                className={`relative px-4 py-2 rounded-xl text-sm font-medium transition-colors focus-ring ${
                  active
                    ? 'text-slate-900 dark:text-white'
                    : 'text-slate-600 dark:text-slate-400'
                }`}
              >
                {active && (
                  <motion.span
                    layoutId="periodPill"
                    className="absolute inset-0 rounded-xl bg-gradient-to-r from-emerald-400/25 to-cyan-400/25 border border-emerald-400/40"
                    transition={{ type: 'spring', stiffness: 350, damping: 30 }}
                  />
                )}
                <span className="relative z-10">{p.label}</span>
              </button>
            );
          })}
        </div>

        <div className="text-xs text-slate-500 dark:text-slate-400 sm:text-right">
          Showing skills for{' '}
          <span className="font-medium text-slate-700 dark:text-slate-200">
            {selected?.jobTitle || '-'}
          </span>{' '}
          &middot; total skills tracked: {periodData.totalSkills?.toLocaleString() || 0}
        </div>
      </div>

      {/* Body */}
      <div className="grid gap-4 lg:grid-cols-[340px_1fr]">
        {/* Left: job titles list */}
        <div className="surface rounded-2xl p-3 flex flex-col">
          <div className="relative mb-2">
            <Search
              size={14}
              className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
            />
            <input
              type="search"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Filter job titles..."
              className="w-full h-9 pl-8 pr-3 rounded-lg border bg-white dark:bg-slate-900/60 text-slate-900 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 border-slate-200 dark:border-slate-700 focus:border-emerald-400 focus:ring-2 focus:ring-emerald-400/30 focus:outline-none text-xs"
            />
          </div>
          <div className="overflow-y-auto max-h-[560px] pr-1 -mr-1 space-y-1">
            {filteredTrends.map((t) => {
              const active = t.jobTitle === selected?.jobTitle;
              return (
                <button
                  key={t.jobTitle}
                  type="button"
                  onClick={() => setSelectedTitle(t.jobTitle)}
                  className={`group w-full text-left px-3 py-2.5 rounded-xl border transition-all focus-ring ${
                    active
                      ? 'bg-emerald-500/10 border-emerald-500/40'
                      : 'bg-transparent border-transparent hover:bg-slate-100 dark:hover:bg-slate-800/60 hover:border-slate-200 dark:hover:border-slate-700'
                  }`}
                >
                  <div className="flex items-center justify-between gap-2">
                    <div
                      className={`text-sm font-medium truncate ${
                        active
                          ? 'text-emerald-700 dark:text-emerald-300'
                          : 'text-slate-800 dark:text-slate-200'
                      }`}
                    >
                      {t.jobTitle}
                    </div>
                    <div className="flex items-center gap-1 shrink-0">
                      <span className="text-xs font-semibold text-slate-700 dark:text-slate-300 tabular-nums">
                        {t.jobsCount}
                      </span>
                      <ChevronRight
                        size={14}
                        className={`text-slate-400 transition-transform ${
                          active ? 'translate-x-0.5 text-emerald-500' : ''
                        }`}
                      />
                    </div>
                  </div>
                  <div className="mt-1.5 h-1 rounded-full bg-slate-200/70 dark:bg-slate-800 overflow-hidden">
                    <motion.div
                      initial={{ width: 0 }}
                      animate={{ width: `${(t.jobRatio || 0) * 100}%` }}
                      transition={{ duration: 0.6, ease: 'easeOut' }}
                      className="h-full rounded-full bg-gradient-to-r from-emerald-400 to-cyan-400"
                    />
                  </div>
                  <div className="mt-1 text-[10px] text-slate-500 dark:text-slate-500 tabular-nums">
                    {formatPercent(t.jobRatio)} of all jobs
                  </div>
                </button>
              );
            })}
            {filteredTrends.length === 0 && (
              <div className="text-center text-xs text-slate-500 dark:text-slate-400 py-6">
                No matching titles.
              </div>
            )}
          </div>
        </div>

        {/* Right: detail */}
        <AnimatePresence mode="wait">
          <motion.div
            key={`${selected?.jobTitle}-${period}`}
            initial={{ opacity: 0, y: 6 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -6 }}
            transition={{ duration: 0.2 }}
            className="surface rounded-2xl p-5"
          >
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 mb-4">
              <div>
                <div className="text-xs uppercase tracking-wider text-slate-500 dark:text-slate-400">
                  Top technical skills
                </div>
                <div className="text-lg font-semibold tracking-tight text-slate-900 dark:text-slate-50">
                  {selected?.jobTitle}
                </div>
              </div>
              <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-lg bg-emerald-500/10 text-emerald-700 dark:text-emerald-300 border border-emerald-500/30 text-xs font-medium self-start">
                <TrendingUp size={12} />
                {PERIODS.find((p) => p.id === period)?.label}
                {periodData.totalSkills
                  ? ` · ${periodData.totalSkills.toLocaleString()} total`
                  : ''}
              </div>
            </div>

            {chartData.length === 0 ? (
              <EmptyState
                icon={TrendingUp}
                message="No skill data for this period"
                description="Try a different period or another job title."
              />
            ) : (
              <>
                <div className="w-full h-[320px]">
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart
                      data={chartData}
                      layout="vertical"
                      margin={{ top: 4, right: 16, bottom: 4, left: 8 }}
                    >
                      <defs>
                        <linearGradient id="barGrad" x1="0" y1="0" x2="1" y2="0">
                          <stop offset="0%" stopColor="#34d399" />
                          <stop offset="100%" stopColor="#22d3ee" />
                        </linearGradient>
                      </defs>
                      <CartesianGrid
                        strokeDasharray="3 3"
                        stroke={isDark ? '#1e293b' : '#e2e8f0'}
                        horizontal={false}
                      />
                      <XAxis
                        type="number"
                        tick={{
                          fill: isDark ? '#94a3b8' : '#64748b',
                          fontSize: 11,
                        }}
                        axisLine={{ stroke: isDark ? '#1e293b' : '#e2e8f0' }}
                        tickLine={false}
                        allowDecimals={false}
                      />
                      <YAxis
                        type="category"
                        dataKey="skill"
                        tick={{
                          fill: isDark ? '#cbd5e1' : '#334155',
                          fontSize: 12,
                        }}
                        axisLine={false}
                        tickLine={false}
                        width={120}
                      />
                      <Tooltip
                        cursor={{
                          fill: isDark
                            ? 'rgba(16,185,129,0.08)'
                            : 'rgba(16,185,129,0.10)',
                        }}
                        content={<ChartTooltip isDark={isDark} />}
                      />
                      <Bar
                        dataKey="count"
                        fill="url(#barGrad)"
                        radius={[6, 6, 6, 6]}
                        barSize={18}
                      />
                    </BarChart>
                  </ResponsiveContainer>
                </div>

                {/* Skill ratio list */}
                <div className="mt-5 grid gap-2">
                  {periodData.topSkills.slice(0, 10).map((s, idx) => (
                    <motion.div
                      key={`${s.skill}-${idx}`}
                      initial={{ opacity: 0, x: -6 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ duration: 0.25, delay: idx * 0.02 }}
                      className="flex items-center gap-3"
                    >
                      <span className="w-6 text-xs font-semibold text-slate-400 tabular-nums">
                        #{idx + 1}
                      </span>
                      <span className="flex-1 text-sm text-slate-800 dark:text-slate-200 truncate">
                        {s.skill}
                      </span>
                      <div className="flex-1 max-w-[260px] h-1.5 rounded-full bg-slate-200/70 dark:bg-slate-800 overflow-hidden">
                        <motion.div
                          initial={{ width: 0 }}
                          animate={{ width: `${(s.ratio || 0) * 100}%` }}
                          transition={{ duration: 0.5, ease: 'easeOut' }}
                          className="h-full rounded-full bg-gradient-to-r from-emerald-400 to-cyan-400"
                        />
                      </div>
                      <span className="w-14 text-right text-xs text-slate-500 dark:text-slate-400 tabular-nums">
                        {formatPercent(s.ratio)}
                      </span>
                      <span className="w-12 text-right text-xs font-semibold text-slate-700 dark:text-slate-200 tabular-nums">
                        {s.count}
                      </span>
                    </motion.div>
                  ))}
                </div>
              </>
            )}
          </motion.div>
        </AnimatePresence>
      </div>
    </div>
  );
};

export default TrendsView;
