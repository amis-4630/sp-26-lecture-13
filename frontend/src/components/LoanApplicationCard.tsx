import type { LoanApplication } from "../data/loanApplications";
import ActionButtons from "./ActionButtons";
import styles from "./LoanApplicationCard.module.css";

type LoanApplicationCardProps = {
  loan: LoanApplication;
};

function getStatusClass(status: string): string {
  switch (status) {
    case "Approved":
      return styles.statusApproved;
    case "Denied":
      return styles.statusDenied;
    case "Pending":
      return styles.statusPending;
    case "Under Review":
      return styles.statusReview;
    case "Flagged":
      return styles.statusFlagged;
    default:
      return "";
  }
}

function LoanApplicationCard({ loan }: LoanApplicationCardProps) {
  return (
    <div
      className={`${styles.card} ${loan.status === "Denied" ? styles.denied : ""}`}
    >
      <div className={styles.header}>
        <span className={styles.loanType}>{loan.loanType.name}</span>
        <span className={`${styles.status} ${getStatusClass(loan.status)}`}>
          {loan.status}
        </span>
      </div>
      <div className={styles.info}>
        <h3 className={styles.applicant}>{loan.applicantName}</h3>
        <p className={styles.amount}>${loan.loanAmount.toLocaleString()}</p>
        <p className={styles.income}>
          Income: ${loan.annualIncome.toLocaleString()}/yr
        </p>
        {loan.riskRating >= 4 && (
          <span className={styles.highRiskBadge}>⚠ High Risk</span>
        )}
        {loan.loanAmount > 200000 && (
          <span className={styles.largeLoanBadge}>Large Loan</span>
        )}
        <p className={styles.date}>
          Submitted: {new Date(loan.submittedDate).toLocaleDateString()}
        </p>
        {/* ActionButtons reads dispatch directly from context — no prop drilling */}
        <ActionButtons loanId={loan.id} currentStatus={loan.status} />
      </div>
    </div>
  );
}

export default LoanApplicationCard;
