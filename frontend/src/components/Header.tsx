import { useAuth } from "../contexts/AuthContext";

export default function Header() {
  const { user, logout } = useAuth();

  return (
    <header>
      <div className="header-top">
        <div>
          <h1>Buckeye Lending</h1>
          <p>Loan Application Dashboard</p>
        </div>
        <div className="header-actions">
          {user && <span className="user-email">{user.email}</span>}
          {user && (
            <button className="logout-btn" onClick={logout} type="button">
              Logout
            </button>
          )}
        </div>
      </div>
    </header>
  );
}
