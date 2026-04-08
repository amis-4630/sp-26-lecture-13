import { Link } from "react-router-dom";
import { useLoanContext } from "../contexts/LoanContext";
import { useAuth } from "../contexts/AuthContext";
import LoanApplicationList from "./LoanApplicationList";

// ─── Dashboard ───────────────────────────────────────────────────────────────
// Reads state and dispatch directly from context — no useState, no prop drilling.
export default function Dashboard() {
  const { state, dispatch, filteredLoans, loanTypes } = useLoanContext();
  const { user, logout } = useAuth();

  return (
    <div className="app">
      <header>
        <div className="header-top">
          <div>
            <h1>Buckeye Lending</h1>
            <p>Loan Application Dashboard</p>
          </div>
          <div className="header-actions">
            {user && <span className="user-email">{user.email}</span>}
            <Link to="/apply" className="apply-link">
              Apply
            </Link>
            <button className="logout-btn" onClick={logout} type="button">
              Logout
            </button>
          </div>
        </div>
        {state.notificationCount > 0 && (
          <button
            className="notification-badge"
            onClick={() => dispatch({ type: "CLEAR_NOTIFICATIONS" })}
          >
            {state.notificationCount} notification
            {state.notificationCount !== 1 ? "s" : ""} — Clear
          </button>
        )}
      </header>
      <main>
        {state.loading && <p className="loading">Loading loan applications…</p>}
        {state.error && (
          <p className="error">Failed to load applications: {state.error}</p>
        )}
        {!state.loading && !state.error && (
          <>
            <div className="type-filter">
              {loanTypes.map((type) => (
                <button
                  key={type}
                  className={type === state.filter ? "active" : ""}
                  onClick={() => dispatch({ type: "SET_FILTER", filter: type })}
                  type="button"
                >
                  {type}
                </button>
              ))}
            </div>
            <LoanApplicationList />
            <p className="loan-count">{filteredLoans.length} applications</p>
          </>
        )}
      </main>
    </div>
  );
}
