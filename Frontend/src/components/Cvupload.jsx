import { useRef, useState } from 'react';
import { motion } from 'framer-motion';
import { UploadCloud, FileText, X, CheckCircle2 } from 'lucide-react';

const CVUpload = ({ onFileSelect, currentCV, isProcessing = false }) => {
  const [fileName, setFileName] = useState(currentCV || '');
  const [isDragging, setIsDragging] = useState(false);
  const fileInputRef = useRef(null);

  const handleFile = (file) => {
    if (!file) return;
    if (file.type === 'application/pdf' || file.name.endsWith('.pdf')) {
      setFileName(file.name);
      onFileSelect(file);
    } else {
      alert('Please upload a PDF file');
    }
  };

  const handleFileChange = (e) => handleFile(e.target.files?.[0]);

  const handleDrop = (e) => {
    e.preventDefault();
    setIsDragging(false);
    handleFile(e.dataTransfer.files?.[0]);
  };

  const handleRemove = () => {
    setFileName('');
    onFileSelect(null);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  const openPicker = () => fileInputRef.current?.click();

  return (
    <div>
      <input
        ref={fileInputRef}
        id="cv-upload"
        name="cv"
        type="file"
        accept="application/pdf,.pdf"
        className="hidden"
        onChange={handleFileChange}
      />

      {fileName ? (
        <motion.div
          initial={{ opacity: 0, y: 6 }}
          animate={{ opacity: 1, y: 0 }}
          className="flex items-center gap-3 rounded-2xl border border-emerald-500/30 bg-emerald-500/5 px-4 py-3"
        >
          <div className="grid place-items-center w-10 h-10 rounded-xl bg-emerald-500/15 text-emerald-600 dark:text-emerald-400">
            <FileText size={18} />
          </div>
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-1.5 text-sm font-medium text-slate-900 dark:text-slate-100 truncate">
              <span className="truncate">{fileName}</span>
              {!isProcessing && (
                <CheckCircle2 size={14} className="text-emerald-500 shrink-0" />
              )}
            </div>
            <div className="text-xs text-slate-500 dark:text-slate-400">
              {isProcessing ? 'Processing CV...' : 'Ready'}
            </div>
          </div>
          <div className="flex items-center gap-1.5">
            <button
              type="button"
              onClick={openPicker}
              className="text-xs font-medium px-3 py-1.5 rounded-lg border border-slate-200 dark:border-slate-700 text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
            >
              Replace
            </button>
            <button
              type="button"
              onClick={handleRemove}
              aria-label="Remove CV"
              className="grid place-items-center w-8 h-8 rounded-lg text-slate-500 hover:text-rose-500 hover:bg-rose-500/10 transition-colors"
            >
              <X size={16} />
            </button>
          </div>
        </motion.div>
      ) : (
        <motion.button
          type="button"
          onClick={openPicker}
          onDragOver={(e) => {
            e.preventDefault();
            setIsDragging(true);
          }}
          onDragLeave={() => setIsDragging(false)}
          onDrop={handleDrop}
          whileHover={{ y: -1 }}
          className={`group w-full flex flex-col items-center justify-center gap-3 rounded-2xl border-2 border-dashed px-6 py-10 transition-colors text-center ${
            isDragging
              ? 'border-emerald-400 bg-emerald-500/10'
              : 'border-slate-200 dark:border-slate-700 hover:border-emerald-400/70 bg-white/60 dark:bg-slate-900/40'
          }`}
        >
          <div className="grid place-items-center w-12 h-12 rounded-2xl bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 group-hover:scale-105 transition-transform">
            <UploadCloud size={22} />
          </div>
          <div>
            <div className="text-sm font-semibold text-slate-900 dark:text-slate-100">
              Drop your CV here, or click to browse
            </div>
            <div className="mt-1 text-xs text-slate-500 dark:text-slate-400">
              PDF only. We'll auto-fill your skills and experience.
            </div>
          </div>
        </motion.button>
      )}
    </div>
  );
};

export default CVUpload;
