import { useState } from 'react';
import { motion } from 'framer-motion';
import {
  Sparkles,
  TrendingUp,
  ShieldCheck,
  Mail,
  Lock,
  Eye,
  EyeOff,
} from 'lucide-react';
import { login, register } from '../../api/auth';
import Input from '../../components/Input';
import Button from '../../components/Button';
import Alert from '../../components/Alert';
import ThemeToggle from '../../components/ThemeToggle';
import Brand from '../../components/Brand';
import { BRAND_FOOTER } from '../../constants/brand';

const TABS = [
  { id: 'login', label: 'Sign In' },
  { id: 'signup', label: 'Create Account' },
];

const FEATURES = [
  {
    Icon: Sparkles,
    title: 'Smart job matching',
    text: 'AI parses your CV and finds roles that fit your skills and goals.',
  },
  {
    Icon: TrendingUp,
    title: 'Live market trends',
    text: 'See in‑demand skills by job title across the last week, month, and quarter.',
  },
  {
    Icon: ShieldCheck,
    title: 'Privacy first',
    text: 'Your CV stays yours. Update your profile any time, opt‑in to alerts.',
  },
];

const ProfileAuth = ({ onAuthSuccess }) => {
  const [activeTab, setActiveTab] = useState('login');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [showPassword, setShowPassword] = useState(false);

  const isLogin = activeTab === 'login';

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    const formData = new FormData(e.target);
    const email = formData.get('email');
    const password = formData.get('password');

    try {
      const data = isLogin
        ? await login(email, password)
        : await register(email, password);

      localStorage.setItem('token', data.token);
      localStorage.setItem('jobSeekerId', data.jobSeekerId);
      localStorage.setItem('email', data.email);

      onAuthSuccess(data.token, {
        email: data.email,
        jobSeekerId: data.jobSeekerId,
      });
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen grid lg:grid-cols-2">
      {/* Brand panel */}
      <div className="relative hidden lg:flex flex-col justify-between p-10 overflow-hidden bg-gradient-to-br from-slate-900 via-slate-950 to-emerald-950 text-slate-100">
        <div className="absolute -top-32 -left-32 w-[500px] h-[500px] rounded-full bg-emerald-500/20 blur-3xl" />
        <div className="absolute -bottom-32 -right-32 w-[500px] h-[500px] rounded-full bg-cyan-500/20 blur-3xl" />

        <Brand
          size="lg"
          className="relative z-10"
          taglineClassName="!text-slate-400"
        />

        <div className="relative z-10 max-w-md">
          <h1 className="text-4xl font-bold leading-tight tracking-tight">
            Find the role that <span className="text-gradient-brand">matches you</span>.
          </h1>
          <p className="mt-3 text-slate-300 text-sm leading-relaxed">
            Upload your CV once. Discover matched jobs, follow trending skills, and stay
            ahead of the market.
          </p>

          <div className="mt-8 space-y-4">
            {FEATURES.map(({ Icon, title, text }) => (
              <motion.div
                key={title}
                initial={{ opacity: 0, x: -8 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ duration: 0.4, delay: 0.1 }}
                className="flex gap-3"
              >
                <div className="grid place-items-center w-10 h-10 rounded-xl bg-emerald-500/15 text-emerald-300 border border-emerald-500/20 shrink-0">
                  <Icon size={18} />
                </div>
                <div>
                  <div className="text-sm font-semibold text-slate-100">{title}</div>
                  <div className="text-xs text-slate-400 leading-relaxed">{text}</div>
                </div>
              </motion.div>
            ))}
          </div>
        </div>

        <div className="relative z-10 text-xs text-slate-500">
          {BRAND_FOOTER}
        </div>
      </div>

      {/* Form panel */}
      <div className="flex flex-col justify-center px-6 py-10 sm:px-10">
        <div className="absolute top-4 right-4 lg:top-6 lg:right-6">
          <ThemeToggle />
        </div>

        <Brand
          size="sm"
          showTagline={false}
          className="lg:hidden mb-8"
          nameClassName="text-slate-900 dark:text-slate-50"
        />

        <div className="w-full max-w-md mx-auto">
          <h2 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-slate-50">
            {isLogin ? 'Welcome back' : 'Create your account'}
          </h2>
          <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">
            {isLogin
              ? 'Sign in to view your matched jobs and trends.'
              : 'Start matching with roles that fit you in seconds.'}
          </p>

          {/* Tabs */}
          <div className="mt-6 relative grid grid-cols-2 p-1 rounded-2xl border border-slate-200 dark:border-slate-700 bg-white/60 dark:bg-slate-900/40">
            {TABS.map((tab) => {
              const active = activeTab === tab.id;
              return (
                <button
                  key={tab.id}
                  type="button"
                  onClick={() => {
                    setActiveTab(tab.id);
                    setError(null);
                  }}
                  className={`relative py-2.5 rounded-xl text-sm font-medium transition-colors focus-ring ${
                    active
                      ? 'text-slate-900 dark:text-white'
                      : 'text-slate-600 dark:text-slate-400'
                  }`}
                >
                  {active && (
                    <motion.span
                      layoutId="authTab"
                      className="absolute inset-0 rounded-xl bg-gradient-to-r from-emerald-400/25 to-cyan-400/25 border border-emerald-400/40"
                      transition={{ type: 'spring', stiffness: 350, damping: 30 }}
                    />
                  )}
                  <span className="relative z-10">{tab.label}</span>
                </button>
              );
            })}
          </div>

          <div className="mt-5">
            <Alert message={error} type="error" />
          </div>

          <form onSubmit={handleSubmit} className="space-y-4">
            <Input
              type="email"
              name="email"
              required
              icon={Mail}
              placeholder="you@company.com"
              label="Email"
            />

            <Input
              type={showPassword ? 'text' : 'password'}
              name="password"
              required
              icon={Lock}
              placeholder="********"
              label="Password"
              rightSlot={
                <button
                  type="button"
                  onClick={() => setShowPassword((v) => !v)}
                  className="grid place-items-center w-9 h-9 rounded-lg text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 transition-colors"
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                >
                  {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              }
            />

            <Button
              type="submit"
              loading={loading}
              fullWidth
              size="lg"
              variant="primary"
            >
              {isLogin ? 'Sign in' : 'Create account'}
            </Button>
          </form>

          <p className="mt-5 text-sm text-center text-slate-500 dark:text-slate-400">
            {isLogin ? "Don't have an account? " : 'Already a member? '}
            <button
              type="button"
              onClick={() => {
                setActiveTab(isLogin ? 'signup' : 'login');
                setError(null);
              }}
              className="font-medium text-emerald-600 dark:text-emerald-400 hover:text-emerald-500"
            >
              {isLogin ? 'Sign up' : 'Sign in'}
            </button>
          </p>
        </div>
      </div>
    </div>
  );
};

export default ProfileAuth;
