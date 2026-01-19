import { useState, useEffect } from 'react';
import { updateProfile, getProfile, getProfileIdForUser, uploadCV } from '../../api/profile';
import Input from '../../components/Input';
import Button from '../../components/Button';
import Alert from '../../components/Alert';
import Checkbox from '../../components/Checkbox';
import SkillsInput from '../../components/Skillsinput';
import UserInfo from '../../components/Userinfo';
import CVUpload from '../../components/Cvupload';

const ProfileUser = ({ user, token }) => {
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState({ text: '', type: '' });
  const [, setCvFile] = useState(null);
  const [currentCV, setCurrentCV] = useState('');
  const [jobTitle, setJobTitle] = useState('');
  const [technicalSkills, setTechnicalSkills] = useState(['']);
  const [jobPositionSkills, setJobPositionSkills] = useState(['']);
  const [fieldSkills, setFieldSkills] = useState(['']);
  const [softSkills, setSoftSkills] = useState(['']);
  const [profileId, setProfileId] = useState(null);
  const [experience, setExperience] = useState('');
  const [receiveNotifications, setReceiveNotifications] = useState(false);

  const bindProfileDtoToForm = (dto) => {
    if (!dto) return;

    const profileIdFromDto = dto.profileId ?? null;

    const seekedJobTitleList = dto.seekedJobTitle ?? dto.jobTitle ?? [];

    const technical = dto.technicalSkills ?? [];
    const jobPos = dto.jobPositionSkills ?? [];
    const field = dto.fieldSkills ?? [];
    const soft = dto.softSkills ?? [];

    const exp = dto.experience ?? '';
    const notif = dto.receiveNotifications ?? false;

    if (profileIdFromDto) {
      setProfileId(profileIdFromDto);
      localStorage.setItem('profileId', String(profileIdFromDto));
    }

    const title = Array.isArray(seekedJobTitleList)
      ? (seekedJobTitleList[0] ?? '')
      : (seekedJobTitleList ?? '');
    setJobTitle(title);

    setTechnicalSkills(Array.isArray(technical) && technical.length ? technical : ['']);
    setJobPositionSkills(Array.isArray(jobPos) && jobPos.length ? jobPos : ['']);
    setFieldSkills(Array.isArray(field) && field.length ? field : ['']);
    setSoftSkills(Array.isArray(soft) && soft.length ? soft : ['']);

    setExperience(exp);
    setReceiveNotifications(Boolean(notif));
  };

  useEffect(() => {
    const loadProfile = async () => {
      try {
        if(!localStorage.getItem('profileId')){
          const profileIdForCurrentUser = await getProfileIdForUser(token, user.jobSeekerId);
          if(profileIdForCurrentUser){
            localStorage.setItem('profileId', profileIdForCurrentUser);
          }
        }
        const storedProfileId = localStorage.getItem('profileId');        
        if (storedProfileId && user?.jobSeekerId) {
          const profile = await getProfile(token, storedProfileId);

          // Bind server profile into form (handles list/string shapes)
          bindProfileDtoToForm(profile);

          if (profile.cvFileName) {
            setCurrentCV(profile.cvFileName);
          }
        }
      // eslint-disable-next-line no-unused-vars
      } catch (err) {
        console.log('No existing profile found');
      }
    };

    if (user?.jobSeekerId && token) {
      loadProfile();
    }
  }, [token, user]);

  const handleCVSelect = async (file) => {
    setCvFile(file);
    if (!file) return;

    if (file.type !== 'application/pdf') {
      setMessage({ text: 'Please upload a PDF file only.', type: 'error' });
      return;
    }

    setLoading(true);
    setMessage({ text: '', type: '' });

    try {
      const dto = await uploadCV(file, token);

      // Bind extracted ProfileDTO to fields
      bindProfileDtoToForm(dto);

      setMessage({ text: 'CV processed and fields filled.', type: 'success' });
    } catch (err) {
      setMessage({ text: err.message || 'Failed to upload CV', type: 'error' });
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage({ text: '', type: '' });

    const formData = new FormData(e.target);
    // CV is now uploaded separately, so we don't add it here
    formData.append('seekedJobTitle', jobTitle);
    formData.append('technicalSkills', JSON.stringify(technicalSkills.filter(s => s.trim())));
    formData.append('jobPositionSkills', JSON.stringify(jobPositionSkills.filter(s => s.trim())));
    formData.append('fieldSkills', JSON.stringify(fieldSkills.filter(s => s.trim())));
    formData.append('softSkills', JSON.stringify(softSkills.filter(s => s.trim())));
    formData.append('experience', experience);
    formData.append('receiveNotifications', receiveNotifications);

    try {
      const result = await updateProfile(formData, token, user.jobSeekerId, profileId);

      if (!profileId && result.profileId) {
        setProfileId(result.profileId);
        localStorage.setItem('profileId', result.profileId);
      }
      
      setMessage({ text: 'Profile updated', type: 'success' });
    } catch (err) {
      setMessage({ text: err.message, type: 'error' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6 max-w-2xl mx-auto">
      <UserInfo user={user} />

      <h1 className="text-xl font-bold mb-4">Profile</h1>

      <Alert message={message.text} type={message.type} />

      <form onSubmit={handleSubmit} className="space-y-4">
        <CVUpload onFileSelect={handleCVSelect} currentCV={currentCV} />

        <Input
          type="text"
          name="seekedJobTitle"
          required
          placeholder="Seeked Job Title"
          value={jobTitle}
          onChange={(e) => setJobTitle(e.target.value)}
        />

        <SkillsInput 
          label="Technical Skills" 
          skills={technicalSkills} 
          setSkills={setTechnicalSkills} 
        />
        
        <SkillsInput 
          label="Job Position Skills" 
          skills={jobPositionSkills} 
          setSkills={setJobPositionSkills} 
        />
        
        <SkillsInput 
          label="Field Skills" 
          skills={fieldSkills} 
          setSkills={setFieldSkills} 
        />
        
        <SkillsInput 
          label="Soft Skills" 
          skills={softSkills} 
          setSkills={setSoftSkills} 
        />

        <Input
          type="text"
          name="experience"
          placeholder="Experience"
          value={experience}
          onChange={(e) => setExperience(e.target.value)}
        />

        <Checkbox 
          name="receiveNotifications" 
          label="Receive job notifications"
          checked={receiveNotifications}
          onChange={(e) => setReceiveNotifications(e.target.checked)} 
        />

        <Button
          type="submit"
          disabled={loading}
          variant="primary"
        >
          {loading ? '...' : 'Save'}
        </Button>
      </form>
    </div>
  );
};

export default ProfileUser;