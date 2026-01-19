const Checkbox = ({ name, label, className = '' ,checked,onChange}) => {
  return (
    <label className={`flex items-center text-sm ${className}`}>
      <input type="checkbox" name={name} className="mr-2" checked={checked} onChange={onChange} />
      {label}
    </label>
  );
};

export default Checkbox;