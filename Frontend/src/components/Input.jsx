const Input = ({
  type = 'text',
  name,
  placeholder,
  required = false,
  value,
  onChange,
  icon: Icon,
  rightSlot,
  className = '',
  label,
  ...rest
}) => {
  return (
    <label className="block w-full">
      {label && (
        <span className="block text-xs font-medium text-slate-600 dark:text-slate-400 mb-1.5 ml-1">
          {label}
        </span>
      )}
      <span className="relative block">
        {Icon && (
          <Icon
            size={16}
            className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400 dark:text-slate-500"
          />
        )}
        <input
          type={type}
          name={name}
          placeholder={placeholder}
          required={required}
          value={value}
          onChange={onChange}
          className={`w-full h-11 ${Icon ? 'pl-10' : 'pl-4'} ${
            rightSlot ? 'pr-12' : 'pr-4'
          } rounded-xl border bg-white dark:bg-slate-900/60 text-slate-900 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 border-slate-200 dark:border-slate-700 hover:border-slate-300 dark:hover:border-slate-600 focus:border-emerald-400 dark:focus:border-emerald-400 focus:ring-2 focus:ring-emerald-400/30 focus:outline-none transition-colors text-sm ${className}`}
          {...rest}
        />
        {rightSlot && (
          <span className="absolute right-2 top-1/2 -translate-y-1/2">{rightSlot}</span>
        )}
      </span>
    </label>
  );
};

export default Input;
