const Input = ({ 
  type = 'text', 
  name, 
  placeholder, 
  required = false,
  value,
  onChange,
  className = '' 
}) => {
  return (
    <input
      type={type}
      name={name}
      placeholder={placeholder}
      required={required}
      value={value}
      onChange={onChange}
      className={`w-full p-2 border ${className}`}
    />
  );
};

export default Input;