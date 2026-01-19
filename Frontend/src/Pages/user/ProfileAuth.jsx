import { useState } from 'react';
import { login, register } from '../../api/auth';
import Input from '../../components/Input';
import Button from '../../components/Button';
import Alert from '../../components/Alert';

const ProfileAuth = ({ onAuthSuccess }) => {
  const [isLogin, setIsLogin] = useState(true);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    const formData = new FormData(e.target);
    const email = formData.get('email');
    const password = formData.get('password');

    try {
      const data = isLogin 
        ? await login(email, password) 
        : await register(email, password);
      
      // Store authentication data
      localStorage.setItem('token', data.token);
      localStorage.setItem('jobSeekerId', data.jobSeekerId);
      localStorage.setItem('email', data.email);
      
      onAuthSuccess(data.token, { 
        email: data.email, 
        jobSeekerId: data.jobSeekerId 
      });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6 max-w-sm mx-auto mt-20">
      <h1 className="text-xl font-bold mb-4">
        {isLogin ? 'Login' : 'Sign Up'}
      </h1>

      <Alert message={error} type="error" />

      <form onSubmit={handleSubmit} className="space-y-3">
        <Input
          type="email"
          name="email"
          required
          placeholder="Email"
        />

        <Input
          type="password"
          name="password"
          required
          placeholder="Password"
        />

        <Button
          type="submit"
          disabled={loading}
          variant="primary"
        >
          {loading ? '...' : (isLogin ? 'Login' : 'Sign Up')}
        </Button>
      </form>

      <p className="text-sm text-gray-600 mt-4">
        {isLogin ? "No account? " : "Have account? "}
        <button
          onClick={() => setIsLogin(!isLogin)}
          className="underline"
        >
          {isLogin ? 'Sign up' : 'Login'}
        </button>
      </p>
    </div>
  );
};

export default ProfileAuth;