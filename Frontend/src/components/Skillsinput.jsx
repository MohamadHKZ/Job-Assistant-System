const SkillsInput = ({ label, skills, setSkills }) => {
  const addSkill = () => {
    setSkills([...skills, '']);
  };

  const removeSkill = (index) => {
    setSkills(skills.filter((_, i) => i !== index));
  };

  const updateSkill = (index, value) => {
    const updated = [...skills];
    updated[index] = value;
    setSkills(updated);
  };

  return (
    <div>
      <div className="flex justify-between items-center mb-2">
        <label className="text-sm font-medium">{label}</label>
        <button type="button" onClick={addSkill} className="text-xs px-2 py-1 bg-gray-200">
          + Add
        </button>
      </div>
      <div className="max-h-40 overflow-y-auto border p-2 space-y-2">
        {skills.map((skill, index) => (
          <div key={index} className="flex gap-2">
            <input
              type="text"
              value={skill}
              onChange={(e) => updateSkill(index, e.target.value)}
              className="flex-1 p-2 border text-sm"
              placeholder={`${label.split(' ')[0]} skill`}
            />
            <button
              type="button"
              onClick={() => removeSkill(index)}
              className="px-3 py-2 bg-red-100 text-red-800 text-sm"
            >
              ×
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};

export default SkillsInput;