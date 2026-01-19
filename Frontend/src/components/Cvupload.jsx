import { useState, useRef } from 'react';
import Button from './Button';

const CVUpload = ({ onFileSelect, currentCV }) => {
  const [fileName, setFileName] = useState(currentCV || '');
  const fileInputRef = useRef(null);

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      if (file.type === 'application/pdf' || file.name.endsWith('.pdf')) {
        setFileName(file.name);
        onFileSelect(file);
      } else {
        alert('Please upload a PDF file');
        e.target.value = '';
      }
    }
  };

  const handleRemove = () => {
    setFileName('');
    onFileSelect(null);
  };

  const handleButtonClick = () => {
    fileInputRef.current?.click();
  };

  return (
    <div className="mb-4">
      <label className="block text-sm mb-2">Upload CV</label>
      
      <div className="flex items-center gap-3">
        {fileName ? (
          <>
            <span className="text-sm flex-1 truncate">{fileName}</span>
            <button
              type="button"
              onClick={handleRemove}
              className="text-sm underline hover:no-underline"
            >
              Remove
            </button>
          </>
        ) : (
          <>
            <div className="w-32">
              <Button type="button" onClick={handleButtonClick} variant="primary">
                Choose File
              </Button>
            </div>
            <span className="text-sm text-gray-600">PDF only</span>
          </>
        )}
        <input
          ref={fileInputRef}
          id="cv-upload"
          name="cv"
          type="file"
          accept=".pdf"
          className="hidden"
          onChange={handleFileChange}
        />
      </div>
    </div>
  );
};

export default CVUpload;