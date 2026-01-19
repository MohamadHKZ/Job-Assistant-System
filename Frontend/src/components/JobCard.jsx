const JobCard = ({ job, index }) => {
  return (
    <div className="bg-white border rounded p-4">
      <h3 className="font-bold text-lg mb-2">{job.jobTitle}</h3>
      
      <p className="text-gray-600 text-sm mb-2">{job.companyName}</p>
      
      {job.jobDescription && (
        <p className="text-gray-700 text-sm mb-3">{job.jobDescription}</p>
      )}

      <div className="flex gap-4 text-sm text-gray-600 mb-3">
        {job.location && <span>{job.location}</span>}
        {job.jobType && <span>{job.jobType}</span>}
      </div>

      {job.technicalSkills && job.technicalSkills.length > 0 && (
        <div className="flex flex-wrap gap-2 mb-3">
          {job.technicalSkills.map((skill, i) => (
            <span key={i} className="px-2 py-1 bg-gray-100 text-gray-700 text-xs rounded">
              {skill}
            </span>
          ))}
        </div>
      )}

      {job.url && (
        <a
          href={job.url}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-block px-4 py-2 bg-black text-white text-sm rounded hover:bg-gray-800"
        >
          Apply
        </a>
      )}
    </div>
  );
};

export default JobCard;