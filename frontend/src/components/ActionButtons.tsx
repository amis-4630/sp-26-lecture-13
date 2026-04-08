import { useLoanContext } from "../contexts/LoanContext";
import styles from "./LoanApplicationCard.module.css";

type ActionButtonsProps = {
  loanId: number;
  currentStatus: string;
};

function ActionButtons({ loanId, currentStatus }: ActionButtonsProps) {
  const { dispatch } = useLoanContext();
  const isDenied = currentStatus === "Denied";

  return (
    <div className={styles.actionButtons}>
      <button
        className={`${styles.actionBtn} ${styles.approveBtn}`}
        onClick={() => dispatch({ type: "APPROVE_LOAN", loanId })}
        disabled={isDenied || currentStatus === "Approved"}
      >
        Approve
      </button>
      <button
        className={`${styles.actionBtn} ${styles.denyBtn}`}
        onClick={() => dispatch({ type: "DENY_LOAN", loanId })}
        disabled={isDenied}
      >
        Deny
      </button>
      <button
        className={`${styles.actionBtn} ${styles.flagBtn}`}
        onClick={() => dispatch({ type: "FLAG_LOAN", loanId })}
        disabled={isDenied || currentStatus === "Flagged"}
      >
        Flag
      </button>
    </div>
  );
}

export default ActionButtons;
