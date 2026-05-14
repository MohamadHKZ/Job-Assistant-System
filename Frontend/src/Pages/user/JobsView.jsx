import { useState, useEffect, useMemo, useCallback, useRef } from 'react';
import { motion } from 'framer-motion';
import { Search, RefreshCw, Briefcase, ChevronLeft, ChevronRight } from 'lucide-react';
import { getRecommendedJobs } from '../../api/jobs';
import Alert from '../../components/Alert';
import EmptyState from '../../components/Emptystate';
import JobCard from '../../components/JobCard';
import JobDetailSidebar from '../../components/JobDetailSidebar';
import Skeleton from '../../components/Skeleton';

const JobSkeleton = () => (
  <div className="surface rounded-2xl p-5 space-y-3">
    <div className="flex gap-3">
      <Skeleton className="w-11 h-11" />
      <div className="flex-1 space-y-2">
        <Skeleton className="h-4 w-3/5" />
        <Skeleton className="h-3 w-2/5" />
      </div>
    </div>
    <Skeleton className="h-3 w-full" />
    <Skeleton className="h-3 w-4/5" />
    <div className="flex gap-1.5 pt-1">
      <Skeleton className="h-5 w-14" rounded="rounded-md" />
      <Skeleton className="h-5 w-20" rounded="rounded-md" />
      <Skeleton className="h-5 w-12" rounded="rounded-md" />
    </div>
  </div>
);

const JobsView = ({ token, user }) => {
  const [jobs, setJobs] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [query, setQuery] = useState('');
  const [activeType, setActiveType] = useState('All');

  const [cursors, setCursors] = useState([{ score: null, id: null }]);
  const [page, setPage] = useState(0);
  const [hasNextPage, setHasNextPage] = useState(false);
  const [nextCursor, setNextCursor] = useState({ score: null, id: null });
  const [selectedJob, setSelectedJob] = useState(null);

  const listTopRef = useRef(null);

  const fetchJobs = useCallback(async () => {
    if (!user?.jobSeekerId) {
      setError('Please log in to view jobs');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const profileId = localStorage.getItem('profileId');
      const cur = cursors[page] ?? { score: null, id: null };
      const data = await getRecommendedJobs(
        token,
        profileId,
        cur.score,
        cur.id,
      );
      setJobs(Array.isArray(data.jobs) ? data.jobs : []);
      setHasNextPage(Boolean(data.hasNextPage));
      setNextCursor({
        score: data.nextCursorScore ?? null,
        id: data.nextCursorId ?? null,
      });
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }, [token, user, page, cursors]);

  useEffect(() => {
    fetchJobs();
  }, [fetchJobs]);

  useEffect(() => {
    listTopRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }, [page]);

  const handleRefresh = () => {
    setLoading(true);
    setError(null);
    setCursors(() => [{ score: null, id: null }]);
    setPage(0);
    setSelectedJob(null);
  };

  const goNext = () => {
    if (
      !hasNextPage ||
      nextCursor.score == null ||
      nextCursor.id == null ||
      nextCursor.id === ''
    )
      return;
    setCursors((prev) => [
      ...prev.slice(0, page + 1),
      { score: nextCursor.score, id: nextCursor.id },
    ]);
    setPage((p) => p + 1);
    setSelectedJob(null);
  };

  const goPrev = () => {
    if (page <= 0) return;
    setPage((p) => p - 1);
    setSelectedJob(null);
  };

  const jobTypes = useMemo(() => {
    const set = new Set();
    jobs.forEach((j) => j.jobType && set.add(j.jobType));
    return ['All', ...Array.from(set)];
  }, [jobs]);

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    return jobs.filter((j) => {
      if (activeType !== 'All' && j.jobType !== activeType) return false;
      if (!q) return true;
      const haystack = [
        j.jobTitle,
        j.companyName,
        j.location,
        j.jobDescription,
        ...(Array.isArray(j.technicalSkills) ? j.technicalSkills : []),
      ]
        .filter(Boolean)
        .join(' ')
        .toLowerCase();
      return haystack.includes(q);
    });
  }, [jobs, query, activeType]);

  return (
    <div ref={listTopRef} className="max-w-6xl mx-auto px-4 sm:px-6 py-8">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mb-5">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-slate-50">
            Recommended jobs
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400 mt-0.5">
            {loading
              ? 'Finding the best matches...'
              : `Page ${page + 1} · ${filtered.length} of ${jobs.length} on this page match filters`}
          </p>
        </div>
        <button
          type="button"
          onClick={handleRefresh}
          disabled={loading}
          className="inline-flex items-center gap-2 px-3.5 py-2 rounded-xl border border-slate-200 dark:border-slate-700 bg-white/60 dark:bg-slate-800/60 text-sm font-medium text-slate-700 dark:text-slate-200 hover:border-emerald-400/40 hover:text-emerald-600 dark:hover:text-emerald-400 disabled:opacity-60 transition-colors focus-ring"
        >
          <RefreshCw size={14} className={loading ? 'animate-spin' : ''} />
          Refresh
        </button>
      </div>

      {/* Search + filter */}
      <div className="flex flex-col md:flex-row gap-3 mb-6">
        <div className="relative flex-1">
          <Search
            size={16}
            className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
          />
          <input
            type="search"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Search jobs by title, company, skill..."
            className="w-full h-11 pl-10 pr-4 rounded-xl border bg-white dark:bg-slate-900/60 text-slate-900 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 border-slate-200 dark:border-slate-700 hover:border-slate-300 dark:hover:border-slate-600 focus:border-emerald-400 focus:ring-2 focus:ring-emerald-400/30 focus:outline-none transition-colors text-sm"
          />
        </div>
        {jobTypes.length > 1 && (
          <div className="flex gap-1.5 flex-wrap">
            {jobTypes.map((type) => {
              const active = activeType === type;
              return (
                <button
                  key={type}
                  type="button"
                  onClick={() => setActiveType(type)}
                  className={`px-3.5 py-2 rounded-xl text-sm font-medium border transition-colors focus-ring ${
                    active
                      ? 'bg-emerald-500/15 border-emerald-500/40 text-emerald-700 dark:text-emerald-300'
                      : 'bg-white/60 dark:bg-slate-900/40 border-slate-200 dark:border-slate-700 text-slate-600 dark:text-slate-300 hover:border-emerald-400/40'
                  }`}
                >
                  {type}
                </button>
              );
            })}
          </div>
        )}
      </div>

      <Alert message={error} type="warning" />

      {loading ? (
        <div className="grid gap-4 md:grid-cols-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <JobSkeleton key={i} />
          ))}
        </div>
      ) : filtered.length === 0 ? (
        <EmptyState
          icon={Briefcase}
          message={jobs.length === 0 ? 'No jobs yet' : 'No jobs match your filters'}
          description={
            jobs.length === 0
              ? 'Update your profile or upload your CV so we can find roles that fit you.'
              : 'Try clearing the search or switching the job type filter.'
          }
        />
      ) : (
        <>
          <motion.div layout className="grid gap-4 md:grid-cols-2">
            {filtered.map((job, index) => (
              <JobCard
                key={job.id ?? index}
                job={job}
                index={index}
                onViewDetails={setSelectedJob}
              />
            ))}
          </motion.div>

          <div className="mt-8 flex flex-col items-center gap-3 sm:flex-row sm:justify-between">
            <button
              type="button"
              onClick={goPrev}
              disabled={page === 0 || loading}
              className="inline-flex w-full items-center justify-center gap-2 rounded-xl border border-slate-200 bg-white/60 px-4 py-2.5 text-sm font-medium text-slate-700 transition-colors hover:border-emerald-400/40 hover:text-emerald-600 disabled:cursor-not-allowed disabled:opacity-50 dark:border-slate-700 dark:bg-slate-800/60 dark:text-slate-200 dark:hover:text-emerald-400 sm:w-auto"
            >
              <ChevronLeft size={18} />
              Previous
            </button>
            <span className="text-sm font-medium text-slate-600 dark:text-slate-400">
              Page {page + 1}
            </span>
            <button
              type="button"
              onClick={goNext}
              disabled={!hasNextPage || loading}
              className="inline-flex w-full items-center justify-center gap-2 rounded-xl border border-slate-200 bg-white/60 px-4 py-2.5 text-sm font-medium text-slate-700 transition-colors hover:border-emerald-400/40 hover:text-emerald-600 disabled:cursor-not-allowed disabled:opacity-50 dark:border-slate-700 dark:bg-slate-800/60 dark:text-slate-200 dark:hover:text-emerald-400 sm:w-auto"
            >
              Next
              <ChevronRight size={18} />
            </button>
          </div>
        </>
      )}

      <JobDetailSidebar job={selectedJob} onClose={() => setSelectedJob(null)} />
    </div>
  );
};

export default JobsView;
