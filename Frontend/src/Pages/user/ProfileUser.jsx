import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import {
  FileText,
  Target,
  Wrench,
  GraduationCap,
  Save,
} from 'lucide-react';
import {
  updateProfile,
  getProfile,
  getProfileIdForUser,
  uploadCV,
} from '../../api/profile';
import Input from '../../components/Input';
import Button from '../../components/Button';
import Alert from '../../components/Alert';
import Checkbox from '../../components/Checkbox';
import SkillsInput from '../../components/Skillsinput';
import UserInfo from '../../components/Userinfo';
import CVUpload from '../../components/Cvupload';

const Section = ({ icon: Icon, title, description, children }) => (
  <motion.section
    initial={{ opacity: 0, y: 8 }}
    animate={{ opacity: 1, y: 0 }}
    transition={{ duration: 0.3 }}
    className="surface rounded-2xl p-5 sm:p-6"
  >
    <header className="flex items-start gap-3 mb-5">
      <div className="grid place-items-center w-10 h-10 rounded-xl bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border border-emerald-500/20 shrink-0">
        <Icon size={18} />
      </div>
      <div>
        <h2 className="text-base font-semibold text-slate-900 dark:text-slate-50">
          {title}
        </h2>
        {description && (
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
            {description}
          </p>
        )}
      </div>
    </header>
    <div className="space-y-4">{children}</div>
  </motion.section>
);

const ProfileUser = ({ user, token }) => {
  const [loading, setLoading] = useState(false);
  const [cvLoading, setCvLoading] = useState(false);
  const [message, setMessage] = useState({ text: '', type: '' });
  const [, setCvFile] = useState(null);
  const [currentCV, setCurrentCV] = useState('');
  const [jobTitle, setJobTitle] = useState('');
  const [technicalSkills, setTechnicalSkills] = useState(['']);
  const [technologies, setTechnologies] = useState(['']);
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
    const tech = dto.technologies ?? [];
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
      ? seekedJobTitleList[0] ?? ''
      : seekedJobTitleList ?? '';
    setJobTitle(title);

    setTechnicalSkills(Array.isArray(technical) && technical.length ? technical : ['']);
    setTechnologies(Array.isArray(tech) && tech.length ? tech : ['']);
    setJobPositionSkills(Array.isArray(jobPos) && jobPos.length ? jobPos : ['']);
    setFieldSkills(Array.isArray(field) && field.length ? field : ['']);
    setSoftSkills(Array.isArray(soft) && soft.length ? soft : ['']);

    setExperience(exp);
    setReceiveNotifications(Boolean(notif));
  };

  useEffect(() => {
    const loadProfile = async () => {
      try {
        if (!localStorage.getItem('profileId')) {
          const profileIdForCurrentUser = await getProfileIdForUser(
            token,
            user.jobSeekerId,
          );
          if (profileIdForCurrentUser) {
            localStorage.setItem('profileId', profileIdForCurrentUser);
          }
        }
        const storedProfileId = localStorage.getItem('profileId');
        if (storedProfileId && user?.jobSeekerId) {
          const profile = await getProfile(token, storedProfileId);
          bindProfileDtoToForm(profile);
          if (profile.cvFileName) {
            setCurrentCV(profile.cvFileName);
          }
        }
      } catch (err) {
        if (err?.status === 404) {
          // No existing profile yet — expected on first visit
          return;
        }
        console.warn(
          'Failed to load profile:',
          err?.title || err?.message || err,
          err?.traceId != null ? `(traceId: ${err.traceId})` : '',
        );
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

    setCvLoading(true);
    setMessage({ text: '', type: '' });

    try {
      const dto = await uploadCV(file, token);
      bindProfileDtoToForm(dto);
      setMessage({
        text: 'CV processed and your profile fields have been filled in.',
        type: 'success',
      });
    } catch (err) {
      setMessage({
        text: err || 'Failed to upload CV',
        type: 'error',
      });
    } finally {
      setCvLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage({ text: '', type: '' });

    const formData = new FormData(e.target);
    formData.append('seekedJobTitle', jobTitle);
    formData.append(
      'technicalSkills',
      JSON.stringify(technicalSkills.filter((s) => s.trim())),
    );
    formData.append(
      'technologies',
      JSON.stringify(technologies.filter((s) => s.trim())),
    );
    formData.append(
      'jobPositionSkills',
      JSON.stringify(jobPositionSkills.filter((s) => s.trim())),
    );
    formData.append(
      'fieldSkills',
      JSON.stringify(fieldSkills.filter((s) => s.trim())),
    );
    formData.append('softSkills', JSON.stringify(softSkills.filter((s) => s.trim())));
    formData.append('experience', experience);
    formData.append('receiveNotifications', receiveNotifications);

    try {
      const result = await updateProfile(formData, token, user.jobSeekerId, profileId);
      if (!profileId && result.profileId) {
        setProfileId(result.profileId);
        localStorage.setItem('profileId', result.profileId);
      }
      setMessage({ text: 'Profile saved successfully.', type: 'success' });
    } catch (err) {
      setMessage({ text: err, type: 'error' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="max-w-5xl mx-auto px-4 sm:px-6 py-8">
      <UserInfo user={user} />

      <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-3 mb-6">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-slate-50">
            Your profile
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400 mt-0.5">
            Keep your skills up to date for the most relevant matches.
          </p>
        </div>
      </div>

      <Alert message={message.text} type={message.type || 'info'} />

      <div className="grid gap-5">
        <Section
          icon={FileText}
          title="Your CV"
          description="Upload a PDF and we'll extract your skills and experience automatically."
        >
          <CVUpload
            onFileSelect={handleCVSelect}
            currentCV={currentCV}
            isProcessing={cvLoading}
          />
        </Section>

        <Section
          icon={Target}
          title="Job preference"
          description="What role are you looking for right now?"
        >
          <Input
            type="text"
            name="seekedJobTitle"
            required
            placeholder="e.g. Frontend Engineer, Data Analyst..."
            label="Seeked job title"
            value={jobTitle}
            onChange={(e) => setJobTitle(e.target.value)}
          />
        </Section>

        <Section
          icon={Wrench}
          title="Skills"
          description="Add your skills as chips. We'll use them to find your best matches."
        >
          <div className="grid gap-5 md:grid-cols-2">
            <SkillsInput
              label="Technical Skills"
              skills={technicalSkills}
              setSkills={setTechnicalSkills}
              placeholder="e.g. React, TypeScript, SQL..."
            />
            <SkillsInput
              label="Job Position Skills"
              skills={jobPositionSkills}
              setSkills={setJobPositionSkills}
              placeholder="e.g. Code review, API design..."
            />
            <SkillsInput
              label="Field Skills"
              skills={fieldSkills}
              setSkills={setFieldSkills}
              placeholder="e.g. Fintech, Healthcare..."
            />
            <SkillsInput
              label="Soft Skills"
              skills={softSkills}
              setSkills={setSoftSkills}
              placeholder="e.g. Leadership, Communication..."
            />
          </div>
        </Section>

        <Section
          icon={GraduationCap}
          title="Experience & alerts"
          description="A short summary of your experience and your notification preference."
        >
          <Input
            type="text"
            name="experience"
            placeholder="e.g. 3 years building web apps with React"
            label="Experience"
            value={experience}
            onChange={(e) => setExperience(e.target.value)}
          />
          <Checkbox
            name="receiveNotifications"
            label="Receive job notifications by email"
            checked={receiveNotifications}
            onChange={(e) => setReceiveNotifications(e.target.checked)}
          />
        </Section>
      </div>

      {/* Sticky save bar */}
      <div className="sticky bottom-4 mt-6 z-30">
        <div className="surface rounded-2xl px-4 py-3 flex items-center justify-between shadow-xl shadow-slate-900/10 dark:shadow-black/40">
          <div className="text-xs text-slate-500 dark:text-slate-400">
            Changes save to your profile and update your matches.
          </div>
          <Button type="submit" loading={loading} icon={Save}>
            Save profile
          </Button>
        </div>
      </div>
    </form>
  );
};

export default ProfileUser;
