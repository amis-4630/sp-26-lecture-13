import type { LoanApplication } from "../data/loanApplications";

export type LoanDashboardState = {
    loans: LoanApplication[];
    filter: string;
    loading: boolean;
    error: string | null;
    notificationCount: number;
};

export type LoanAction =
    | { type: "FETCH_START" }
    | { type: "FETCH_SUCCESS"; loans: LoanApplication[] }
    | { type: "FETCH_ERROR"; message: string }
    | { type: "APPROVE_LOAN"; loanId: number }
    | { type: "DENY_LOAN"; loanId: number }
    | { type: "FLAG_LOAN"; loanId: number }
    | { type: "SET_FILTER"; filter: string }
    | { type: "CLEAR_NOTIFICATIONS" };
