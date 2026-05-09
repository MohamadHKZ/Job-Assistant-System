import { useCallback, useState } from 'react';
import { motion } from 'framer-motion';
import { FileText, RefreshCw } from 'lucide-react';
import { getAdminLogs, LOG_CONTAINERS } from '../../api/admin';
import Alert from '../../components/Alert';
import Button from '../../components/Button';

/** Sentinel so the select never defaults to a real container name. */
const NO_SELECTION = '__select__';

const AdminLogs = ({ token }) => {
  const [container, setContainer] = useState(NO_SELECTION);
  const [text, setText] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const load = useCallback(async () => {
    if (container === NO_SELECTION) return;
    setLoading(true);
    setError(null);
    try {
      const body = await getAdminLogs(token, container);
      setText(body);
    } catch (e) {
      setError(e);
      setText('');
    } finally {
      setLoading(false);
    }
  }, [token, container]);

  return (
    <div className="max-w-5xl mx-auto px-4 py-8 space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h2 className="text-2xl font-bold text-slate-900 dark:text-slate-50 flex items-center gap-2">
            <FileText className="text-cyan-500" size={26} />
            Logs
          </h2>
          <p className="text-slate-600 dark:text-slate-400 mt-1 text-sm">
            Logs are refreshed every 30s by the log collector service into shared volume files.
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <select
            value={container}
            onChange={(e) => setContainer(e.target.value)}
            className="h-11 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 px-3 text-sm text-slate-900 dark:text-slate-100 focus-ring"
          >
            <option value={NO_SELECTION}>Select container…</option>
            {LOG_CONTAINERS.map((c) => (
              <option key={c.id} value={c.id}>
                {c.label}
              </option>
            ))}
          </select>
          <Button
            variant="primary"
            icon={RefreshCw}
            onClick={load}
            loading={loading}
            disabled={container === NO_SELECTION}
          >
            Load
          </Button>
        </div>
      </div>

      {error && <Alert type="error" message={error} />}

      <motion.div
        initial={{ opacity: 0, y: 6 }}
        animate={{ opacity: 1, y: 0 }}
        className="surface rounded-2xl border border-slate-200 dark:border-slate-700 overflow-hidden"
      >
        <pre className="text-xs leading-relaxed p-4 max-h-[520px] overflow-auto whitespace-pre-wrap font-mono text-slate-800 dark:text-slate-200 bg-slate-50/80 dark:bg-slate-950/40">
          {text ||
            (loading ? 'Loading…' : 'Choose a container from the list, then click Load.')}
        </pre>
      </motion.div>
    </div>
  );
};

export default AdminLogs;
