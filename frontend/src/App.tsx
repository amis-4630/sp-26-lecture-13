import { BrowserRouter, Routes, Route } from "react-router-dom";
import { LoanProvider } from "./contexts/LoanContext";
import Dashboard from "./components/Dashboard";
import LoanForm from "./components/LoanForm";
import "./App.css";

// ─── App ─────────────────────────────────────────────────────────────────────
// BrowserRouter enables client-side navigation.
// LoanProvider owns the reducer + fetch side-effect.
// All descendants access state via useLoanContext().
function App() {
  return (
    <BrowserRouter>
      <LoanProvider>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/apply" element={<LoanForm />} />
        </Routes>
      </LoanProvider>
    </BrowserRouter>
  );
}

export default App;
