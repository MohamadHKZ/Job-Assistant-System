import { useCallback, useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { Database, ToggleLeft, ToggleRight, RefreshCw } from 'lucide-react';
import { getJobSources, patchJobSource } from '../../api/admin';
import Alert from '../../components/Alert';
import Button from '../../components/Button';
import LoadingSpinner from '../../components/Loadingspinner';

const AdminJobSources = ({ token }) => {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [busy, setBusy] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getJobSources(token);
      setRows(Array.isArray(data) ? data : []);
    } catch (e) {
      setError(e);
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    load();
  }, [load]);

  const toggle = async (sourceName, nextActive) => {
    setBusy(sourceName);
    setError(null);
    try {
      await patchJobSource(token, sourceName, nextActive);
      await load();
    } catch (e) {
      setError(e);
    } finally {
      setBusy(null);
    }
  };

  if (loading && rows.length === 0) {
    return (
      <div className="flex justify-center py-20">
        <LoadingSpinner />
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto px-4 py-8 space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h2 className="text-2xl font-bold text-slate-900 dark:text-slate-50 flex items-center gap-2">
            <Database className="text-emerald-500" size={26} />
            Job sources
          </h2>
          <p className="text-slate-600 dark:text-slate-400 mt-1 text-sm">
            Enable or disable scraping sources used by the pipeline.
          </p>
        </div>
        <Button variant="secondary" icon={RefreshCw} onClick={() => load()} loading={loading}>
          Refresh
        </Button>
      </div>

      {error && <Alert type="error" message={error.message || String(error)} />}

      <div className="surface rounded-2xl overflow-hidden border border-slate-200 dark:border-slate-700">
        <table className="w-full text-sm">
          <thead className="bg-slate-50 dark:bg-slate-900/80 text-left text-slate-500 dark:text-slate-400 uppercase tracking-wider text-[11px]">
            <tr>
              <th className="px-4 py-3 font-semibold">Source</th>
              <th className="px-4 py-3 font-semibold">Status</th>
              <th className="px-4 py-3 font-semibold text-right">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
            {rows.map((r) => (
              <motion.tr
                key={r.sourceName}
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                className="hover:bg-slate-50/80 dark:hover:bg-slate-800/40"
              >
                <td className="px-4 py-3 font-medium text-slate-900 dark:text-slate-100">
                  {r.sourceName}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-medium ${
                      r.isActive
                        ? 'bg-emerald-500/15 text-emerald-700 dark:text-emerald-400'
                        : 'bg-slate-500/15 text-slate-600 dark:text-slate-400'
                    }`}
                  >
                    {r.isActive ? 'Active' : 'Disabled'}
                  </span>
                </td>
                <td className="px-4 py-3 text-right">
                  <Button
                    size="sm"
                    variant={r.isActive ? 'danger' : 'primary'}
                    loading={busy === r.sourceName}
                    icon={r.isActive ? ToggleLeft : ToggleRight}
                    onClick={() => toggle(r.sourceName, !r.isActive)}
                  >
                    {r.isActive ? 'Disable' : 'Enable'}
                  </Button>
                </td>
              </motion.tr>
            ))}
          </tbody>
        </table>
        {rows.length === 0 && !loading && (
          <div className="px-4 py-12 text-center text-slate-500">No job sources found.</div>
        )}
      </div>
    </div>
  );
};

export default AdminJobSources;
