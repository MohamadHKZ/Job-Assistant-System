const UserInfo = ({ user }) => {
  return (
    <div className="mb-6">
      <p className="text-sm text-gray-600">Name: {user?.name || 'Not set'}</p>
      <p className="text-sm text-gray-600">Email: {user?.email}</p>
    </div>
  );
};

export default UserInfo;