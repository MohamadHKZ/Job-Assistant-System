const LoadingSpinner = ({ text = 'Loading...' }) => {
  return (
    <div className="text-center py-8">{text}</div>
  );
};

export default LoadingSpinner;