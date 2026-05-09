import { useCallback, useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { Sliders, Save } from 'lucide-react';
import { getSettings, updateSettings, SCRAPE_INTERVAL_OPTIONS } from '../../api/admin';
import Alert from '../../components/Alert';
import Button from '../../components/Button';
import LoadingSpinner from '../../components/Loadingspinner';

const AdminSettings = ({ token }) => {
  const [scrapingInterval, setScrapingInterval] = useState('LAST_DAY');
  const [minSimilarity, setMinSimilarity] = useState('0.6');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [savedOk, setSavedOk] = useState(false);

  const mapRowsToState = (rows) => {
    const arr = Array.isArray(rows) ? rows : [];
    for (const row of arr) {
      if (row.key === 'ScrapingInterval') setScrapingInterval(row.value || 'LAST_DAY');
      if (row.key === 'MinSimilarityThreshold') setMinSimilarity(String(row.value ?? '0.6'));
    }
  };

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    setSavedOk(false);
    try {
      const data = await getSettings(token);
      mapRowsToState(data);
    } catch (e) {
      setError(e);
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    load();
  }, [load]);

  const save = async () => {
    setSaving(true);
    setError(null);
    setSavedOk(false);
    try {
      await updateSettings(token, [
        { key: 'ScrapingInterval', value: scrapingInterval },
        { key: 'MinSimilarityThreshold', value: String(minSimilarity) },
      ]);
      setSavedOk(true);
      await load();
    } catch (e) {
      setError(e);
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center py-20">
        <LoadingSpinner />
      </div>
    );
  }

  return (
    <div className="max-w-lg mx-auto px-4 py-8 space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-slate-900 dark:text-slate-50 flex items-center gap-2">
          <Sliders className="text-indigo-500" size={26} />
          System settings
        </h2>
        <p className="text-slate-600 dark:text-slate-400 mt-1 text-sm">
          Stored in <code className="text-emerald-600 dark:text-emerald-400">SystemSettings</code>.
        </p>
      </div>

      {error && <Alert type="error" message={error} />}
      {savedOk && <Alert type="success" message="Settings saved." />}

      <motion.div
        initial={{ opacity: 0, y: 8 }}
        animate={{ opacity: 1, y: 0 }}
        className="surface rounded-2xl p-6 space-y-5 border border-slate-200 dark:border-slate-700"
      >
        <div>
          <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-2">
            Scraping interval
          </label>
          <select
            value={scrapingInterval}
            onChange={(e) => setScrapingInterval(e.target.value)}
            className="w-full h-11 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 px-3 text-sm focus-ring"
          >
            {SCRAPE_INTERVAL_OPTIONS.map((opt) => (
              <option key={opt} value={opt}>
                {opt}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-2">
            Minimum similarity threshold (0–1)
          </label>
          <input
            type="number"
            min={0}
            max={1}
            step={0.05}
            value={minSimilarity}
            onChange={(e) => setMinSimilarity(e.target.value)}
            className="w-full h-11 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 px-3 text-sm focus-ring"
          />
        </div>
        <Button fullWidth icon={Save} loading={saving} onClick={save}>
          Save changes
        </Button>
      </motion.div>
    </div>
  );
};

export default AdminSettings;
