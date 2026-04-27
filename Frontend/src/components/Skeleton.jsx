const Skeleton = ({ className = '', rounded = 'rounded-xl' }) => (
  <div className={`shimmer ${rounded} ${className}`} aria-hidden="true" />
);

export default Skeleton;
