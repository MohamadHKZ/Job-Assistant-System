import { Inbox } from 'lucide-react';

const EmptyState = ({
  message = 'Nothing here yet',
  description,
  icon: Icon = Inbox,
  action,
}) => {
  return (
    <div className="surface rounded-2xl text-center px-6 py-14">
      <div className="mx-auto mb-4 grid place-items-center w-14 h-14 rounded-2xl bg-emerald-500/10 text-emerald-500 dark:text-emerald-400 border border-emerald-500/20">
        <Icon size={26} />
      </div>
      <h3 className="text-base font-semibold text-slate-900 dark:text-slate-50">
        {message}
      </h3>
      {description && (
        <p className="mt-1.5 text-sm text-slate-500 dark:text-slate-400 max-w-md mx-auto">
          {description}
        </p>
      )}
      {action && <div className="mt-5">{action}</div>}
    </div>
  );
};

export default EmptyState;
