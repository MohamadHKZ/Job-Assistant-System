import { useState, useEffect } from 'react';
import { getRecommendedJobs } from '../../api/jobs';
import Alert from '../../components/Alert';
import LoadingSpinner from '../../components/Loadingspinner';
import EmptyState from '../../components/Emptystate';
import JobCard from '../../components/JobCard';

const JobsView = ({ token, user }) => {
  const [jobs, setJobs] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchJobs = async () => {
      if (!user?.jobSeekerId) {
        setError('Please log in to view jobs');
        return;
      }

      setLoading(true);
      setError('');
      
      try {
        const profileId = localStorage.getItem('profileId');
        const data = await getRecommendedJobs(token, profileId);

        // API returns an array (not { jobs: [...] })
        setJobs(Array.isArray(data) ? data : []);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchJobs();
  }, [token, user]);

  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-2xl font-bold mb-6">Jobs</h1>

      <Alert message={error} type="warning" />

      {loading ? (
        <LoadingSpinner />
      ) : jobs.length === 0 ? (
        <EmptyState message="No jobs found" />
      ) : (
        <div className="space-y-4">
          {jobs.map((job, index) => (
            <JobCard key={job.jobId || index} job={job} index={index} />
          ))}
        </div>
      )}
    </div>
  );
};

export default JobsView;