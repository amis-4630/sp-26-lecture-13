import { useLoanContext } from "../contexts/LoanContext";
import LoanApplicationCard from "./LoanApplicationCard";

// No props — reads filtered loans directly from context.
// Prop drilling eliminated: the parent no longer computes or passes the list.
function LoanApplicationList() {
  const { filteredLoans } = useLoanContext();

  return (
    <div className="loan-grid">
      {filteredLoans.map((loan) => (
        <LoanApplicationCard key={loan.id} loan={loan} />
      ))}
    </div>
  );
}

export default LoanApplicationList;
