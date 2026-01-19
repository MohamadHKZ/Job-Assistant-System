const Navbar = ({ currentView, setCurrentView, onLogout }) => {
  return (
    <nav className="border-b p-4">
      <div className="max-w-2xl mx-auto flex justify-between items-center">
        <div className="flex gap-4">
          <button
            onClick={() => setCurrentView('profile')}
            className={`text-sm ${currentView === 'profile' ? 'underline' : ''}`}
          >
            Profile
          </button>
          <button
            onClick={() => setCurrentView('jobs')}
            className={`text-sm ${currentView === 'jobs' ? 'underline' : ''}`}
          >
            Jobs
          </button>
        </div>
        <button
          onClick={onLogout}
          className="text-sm"
        >
          Logout
        </button>
      </div>
    </nav>
  );
};

export default Navbar;