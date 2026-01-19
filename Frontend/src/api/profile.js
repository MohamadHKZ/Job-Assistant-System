const API_URL = import.meta.env.VITE_BACKEND_API_BASE_URL || 'https://localhost:5000';
import snakecaseKeys from "snakecase-keys";
// Endpoint: POST api/{jobseeker_id}/profile or PUT api/{jobseeker_id}/{profile_id}/profile
export const updateProfile = async (formData, token, jobSeekerId, profileId = null) => {
  const seekedJobTitle = formData.get('seekedJobTitle');
  const experience = formData.get('experience');
  const receiveNotificationsRaw = formData.get('receiveNotifications');
  const notifications =
    receiveNotificationsRaw === 'on' ||
    receiveNotificationsRaw === 'true' ||
    receiveNotificationsRaw === true;
  const technicalSkills = JSON.parse(formData.get('technicalSkills') || '[]');
  const jobPositionSkills = JSON.parse(formData.get('jobPositionSkills') || '[]');
  const fieldSkills = JSON.parse(formData.get('fieldSkills') || '[]');
  const softSkills = JSON.parse(formData.get('softSkills') || '[]');

  const profileData = {
    profileName: 'Default Profile',
    jobTitle: [seekedJobTitle],
    technicalSkills: technicalSkills.filter(s => s.trim()),
    jobPositionSkills: jobPositionSkills.filter(s => s.trim()),
    fieldSkills: fieldSkills.filter(s => s.trim()),
    softSkills: softSkills.filter(s => s.trim()),
    experience: experience,
    education: '',
    receiveNotifications: notifications,
    customRules: ''
  };

  const url =profileId 
    ? `${API_URL}/api/profile/${profileId}/update` 
    : `${API_URL}/api/profile/${jobSeekerId}/save`; 
  
  console.log(url);
  const method = profileId ? 'PUT' : 'POST';
  const snakedProfileData = snakecaseKeys(profileData);
  console.log(snakedProfileData);
  const response = await fetch(url, {
    method: method,
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(snakedProfileData)
  });
  console.log(snakedProfileData);
  if (!response.ok) {
    const data = await response.json();
    throw new Error(data.message || 'Failed to update profile');
  }
  
  return response.json();
};

 // Endpoint: POST api/{jobseeker_id}/{profile_id}/upload_cv
export const uploadCV = async (cvFile, token) => {
  // Reject FormData (it will be multipart/form-data, not application/pdf)
  if (cvFile instanceof FormData) {
    throw new Error('uploadCV expects a PDF File/Blob, not FormData');
  }

  // Basic client-side validation
  if (!cvFile) throw new Error('No CV file provided');
  if (typeof cvFile.type === 'string' && cvFile.type !== 'application/pdf') {
    throw new Error('CV must be a PDF (application/pdf)');
  }

  const response = await fetch(`${API_URL}/api/profile/extract-info`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/pdf',
      'Accept': 'application/json',
    },
    body: cvFile, // send raw PDF bytes
  });

  if (!response.ok) {
    let msg = 'Failed to upload CV';
    try {
      const data = await response.json();
      msg = data.message || msg;
    } catch {
      // ignore json parse errors
    }
    throw new Error(msg);
  }
  const res = await response.json();
  console.log(res); 
  return res;
};


 // Endpoint: GET api/{jobseeker_id}/{profile_id}/profile

export const getProfile = async (token, profileId) => {
  const response = await fetch(`${API_URL}/api/profile/${profileId}`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  
  if (!response.ok) {
    const data = await response.json();
    throw new Error(data.message || 'Failed to fetch profile');
  }
  
  return response.json();
};

// Endpoint: GET api/{jobseeker_id}/profile_id
export const getProfileIdForUser = async (token, jobSeekerId) => {
  const response = await fetch(`${API_URL}/api/profile/${jobSeekerId}/profile_id`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  if (!response.ok) {
    const data = await response.json();
    throw new Error(data.message || 'Failed to fetch profile ID');
  }
  return response.json();
}