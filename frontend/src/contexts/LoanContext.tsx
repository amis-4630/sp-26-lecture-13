import {
  createContext,
  useContext,
  useReducer,
  useEffect,
  type ReactNode,
} from "react";
import { loanReducer } from "../reducers/loanReducer";
import type { LoanDashboardState, LoanAction } from "../types/loanActions";
import type { LoanApplication } from "../data/loanApplications";
import { fetchLoanApplications } from "../data/loanApplications";

type LoanContextType = {
  state: LoanDashboardState;
  dispatch: React.Dispatch<LoanAction>;
  filteredLoans: LoanApplication[];
  loanTypes: string[];
};

const LoanContext = createContext<LoanContextType | null>(null);

const initialState: LoanDashboardState = {
  loans: [],
  filter: "All",
  loading: true,
  error: null,
  notificationCount: 0,
};

export function LoanProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(loanReducer, initialState);

  useEffect(() => {
    dispatch({ type: "FETCH_START" });
    fetchLoanApplications()
      .then((loans) => dispatch({ type: "FETCH_SUCCESS", loans }))
      .catch((err) => dispatch({ type: "FETCH_ERROR", message: err.message }));
  }, []);

  const loanTypes = [
    "All",
    ...new Set(state.loans.map((loan) => loan.loanType.name)),
  ];

  const filteredLoans =
    state.filter === "All"
      ? state.loans
      : state.loans.filter((loan) => loan.loanType.name === state.filter);

  return (
    <LoanContext.Provider value={{ state, dispatch, filteredLoans, loanTypes }}>
      {children}
    </LoanContext.Provider>
  );
}

export function useLoanContext() {
  const context = useContext(LoanContext);
  if (!context) {
    throw new Error("useLoanContext must be used within a LoanProvider");
  }
  return context;
}
