const Alert = ({ message, type = 'error' }) => {
  if (!message) return null;

  const styles = {
    error: 'bg-red-100 text-red-800',
    success: 'bg-green-100 text-green-800',
    warning: 'bg-red-50 text-red-700',
  };

  return (
    <div className={`mb-4 p-2 text-sm ${styles[type]}`}>
      {message}
    </div>
  );
};

export default Alert;