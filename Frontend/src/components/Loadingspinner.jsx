const LoadingSpinner = ({ text = 'Loading...', size = 28 }) => {
  return (
    <div className="flex flex-col items-center justify-center gap-3 py-10">
      <span
        className="inline-block rounded-full border-2 border-slate-200 dark:border-slate-700 border-t-emerald-400 dark:border-t-emerald-400 animate-spin"
        style={{ width: size, height: size }}
        aria-hidden="true"
      />
      {text && (
        <span className="text-sm text-slate-500 dark:text-slate-400">{text}</span>
      )}
    </div>
  );
};

export default LoadingSpinner;
