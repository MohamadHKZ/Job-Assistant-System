import { useState, useEffect } from 'react';
import Navbar from './components/Navbar';
import ProfileAuth from './Pages/user/ProfileAuth';
import ProfileUser from './Pages/user/ProfileUser';
import JobsView from './Pages/user/JobsView';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [token, setToken] = useState(null);
  const [user, setUser] = useState(null);
  const [currentView, setCurrentView] = useState('profile');

  useEffect(() => {
    const storedToken = localStorage.getItem('token');
    const storedEmail = localStorage.getItem('email');
    const storedJobSeekerId = localStorage.getItem('jobSeekerId');

    if (storedToken && storedEmail && storedJobSeekerId) {
      setToken(storedToken);
      setUser({
        email: storedEmail,
        jobSeekerId: parseInt(storedJobSeekerId)
      });
      setIsAuthenticated(true);
    }
  }, []);

  const handleAuthSuccess = (newToken, userData) => {
    setToken(newToken);
    setUser(userData);
    setIsAuthenticated(true);
    setCurrentView('profile');
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('email');
    localStorage.removeItem('jobSeekerId');
    localStorage.removeItem('profileId');
    setToken(null);
    setUser(null);
    setIsAuthenticated(false);
    setCurrentView('profile');
  };

  if (!isAuthenticated) {
    return <ProfileAuth onAuthSuccess={handleAuthSuccess} />;
  }

  return (
    <div>
      <Navbar 
        currentView={currentView}
        setCurrentView={setCurrentView}
        onLogout={handleLogout}
      />
      
      {currentView === 'profile' ? (
        <ProfileUser user={user} token={token} onLogout={handleLogout} />
      ) : (
        <JobsView token={token} user={user} />
      )}
    </div>
  );
}

export default App;