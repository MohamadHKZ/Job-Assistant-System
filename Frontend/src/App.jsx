import { useState } from 'react';
import { AnimatePresence } from 'framer-motion';
import TopNav from './components/TopNav';
import PageTransition from './components/PageTransition';
import ProfileAuth from './Pages/user/ProfileAuth';
import ProfileUser from './Pages/user/ProfileUser';
import JobsView from './Pages/user/JobsView';
import TrendsView from './Pages/user/TrendsView';

const readInitialAuth = () => {
  if (typeof window === 'undefined') {
    return { token: null, user: null, isAuthenticated: false };
  }
  try {
    const storedToken = window.localStorage.getItem('token');
    const storedEmail = window.localStorage.getItem('email');
    const storedJobSeekerId = window.localStorage.getItem('jobSeekerId');
    if (storedToken && storedEmail && storedJobSeekerId) {
      return {
        token: storedToken,
        user: { email: storedEmail, jobSeekerId: parseInt(storedJobSeekerId) },
        isAuthenticated: true,
      };
    }
  } catch {
    // ignore storage access errors
  }
  return { token: null, user: null, isAuthenticated: false };
};

function App() {
  const [auth, setAuth] = useState(readInitialAuth);
  const [currentView, setCurrentView] = useState('profile');

  const handleAuthSuccess = (newToken, userData) => {
    setAuth({ token: newToken, user: userData, isAuthenticated: true });
    setCurrentView('profile');
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('email');
    localStorage.removeItem('jobSeekerId');
    localStorage.removeItem('profileId');
    setAuth({ token: null, user: null, isAuthenticated: false });
    setCurrentView('profile');
  };

  const { token, user, isAuthenticated } = auth;

  if (!isAuthenticated) {
    return <ProfileAuth onAuthSuccess={handleAuthSuccess} />;
  }

  return (
    <div className="min-h-screen flex flex-col">
      <TopNav
        currentView={currentView}
        setCurrentView={setCurrentView}
        onLogout={handleLogout}
        user={user}
      />

      <main className="flex-1">
        <AnimatePresence mode="wait">
          {currentView === 'profile' && (
            <PageTransition key="profile">
              <ProfileUser user={user} token={token} onLogout={handleLogout} />
            </PageTransition>
          )}
          {currentView === 'jobs' && (
            <PageTransition key="jobs">
              <JobsView token={token} user={user} />
            </PageTransition>
          )}
          {currentView === 'trends' && (
            <PageTransition key="trends">
              <TrendsView token={token} />
            </PageTransition>
          )}
        </AnimatePresence>
      </main>
    </div>
  );
}

export default App;
