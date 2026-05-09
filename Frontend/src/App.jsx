import { useState } from 'react';
import { AnimatePresence } from 'framer-motion';
import TopNav from './components/TopNav';
import PageTransition from './components/PageTransition';
import ProfileAuth from './Pages/user/ProfileAuth';
import ProfileUser from './Pages/user/ProfileUser';
import JobsView from './Pages/user/JobsView';
import TrendsView from './Pages/user/TrendsView';
import AdminDashboard from './Pages/admin/AdminDashboard';
import { parsePrimaryRoleFromToken } from './utils/jwtRole';

const readInitialAuth = () => {
  if (typeof window === 'undefined') {
    return { token: null, user: null, isAuthenticated: false, role: 'User' };
  }
  try {
    const storedToken = window.localStorage.getItem('token');
    const storedEmail = window.localStorage.getItem('email');
    const storedJobSeekerId = window.localStorage.getItem('jobSeekerId');
    const storedRole = window.localStorage.getItem('role');
    if (storedToken && storedEmail && storedJobSeekerId) {
      const role = storedRole || parsePrimaryRoleFromToken(storedToken);
      return {
        token: storedToken,
        user: { email: storedEmail, jobSeekerId: parseInt(storedJobSeekerId, 10), role },
        isAuthenticated: true,
        role,
      };
    }
  } catch {
    // ignore storage access errors
  }
  return { token: null, user: null, isAuthenticated: false, role: 'User' };
};

function App() {
  const [auth, setAuth] = useState(readInitialAuth);
  const [currentView, setCurrentView] = useState('profile');

  const handleAuthSuccess = (newToken, userData) => {
    const role = userData.role ?? parsePrimaryRoleFromToken(newToken);
    setAuth({ token: newToken, user: { ...userData, role }, isAuthenticated: true, role });
    setCurrentView('profile');
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('email');
    localStorage.removeItem('jobSeekerId');
    localStorage.removeItem('profileId');
    localStorage.removeItem('role');
    setAuth({ token: null, user: null, isAuthenticated: false, role: 'User' });
    setCurrentView('profile');
  };

  const { token, user, isAuthenticated, role } = auth;

  if (!isAuthenticated) {
    return <ProfileAuth onAuthSuccess={handleAuthSuccess} />;
  }

  if (role === 'Admin') {
    return <AdminDashboard token={token} user={user} onLogout={handleLogout} />;
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
