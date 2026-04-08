import { useState, useEffect } from "react";
import { useNavigate, Link } from "react-router-dom";
import {
  fetchLoanTypes,
  createApplicant,
  createLoanApplication,
  type LoanType,
} from "../data/loanApplications";

// ─── Form data shape ─────────────────────────────────────────────────────────
type LoanFormData = {
  applicantName: string;
  email: string;
  loanAmount: number | "";
  annualIncome: number | "";
  loanTypeId: number | "";
  notes: string;
};

// ─── LoanForm ────────────────────────────────────────────────────────────────
// Controlled inputs + onBlur validation following the lecture pattern.
export default function LoanForm() {
  const navigate = useNavigate();

  const [loanTypes, setLoanTypes] = useState<LoanType[]>([]);

  // Fetch loan types from the API on mount
  useEffect(() => {
    fetchLoanTypes()
      .then(setLoanTypes)
      .catch((err) => console.error("Failed to load loan types:", err));
  }, []);

  // ── State ────────────────────────────────────────────────────────────────
  const [formData, setFormData] = useState<LoanFormData>({
    applicantName: "",
    email: "",
    loanAmount: "",
    annualIncome: "",
    loanTypeId: "",
    notes: "",
  });

  const [errors, setErrors] = useState<Record<string, string | undefined>>({});
  const [touched, setTouched] = useState<Set<string>>(new Set());
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  // ── Generic change handler ───────────────────────────────────────────────
  // Uses the input's `name` attribute + computed property [name]: value
  const handleChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement
    >,
  ) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === "number" ? (value === "" ? "" : Number(value)) : value,
    }));

    // Clear the error while the user is actively editing
    if (touched.has(name)) {
      const coerced =
        type === "number" ? (value === "" ? "" : Number(value)) : value;
      const error = validateField(name, coerced);
      setErrors((prev) => ({ ...prev, [name]: error }));
    }
  };

  // ── Field-level validation ───────────────────────────────────────────────
  const validateField = (
    name: string,
    value: string | number | "",
  ): string | undefined => {
    switch (name) {
      case "applicantName":
        if (typeof value === "string" && value.trim().length < 2)
          return "Name must be at least 2 characters";
        break;
      case "email":
        if (
          typeof value === "string" &&
          value.length > 0 &&
          !value.includes("@")
        )
          return "Enter a valid email";
        if (typeof value === "string" && value.trim().length === 0)
          return "Email is required";
        break;
      case "loanAmount":
        if (value === "") return "Loan amount is required";
        if (typeof value === "number" && value < 1000) return "Minimum $1,000";
        if (typeof value === "number" && value > 500000)
          return "Maximum $500,000";
        break;
      case "loanTypeId":
        if (!value) return "Select a loan type";
        break;
      case "annualIncome":
        if (value === "") return "Annual income is required";
        if (typeof value === "number" && value <= 0)
          return "Income must be positive";
        break;
    }
    return undefined;
  };

  // ── Blur handler — validate on field exit ────────────────────────────────
  const handleBlur = (
    e: React.FocusEvent<
      HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement
    >,
  ) => {
    const { name } = e.target;
    setTouched((prev) => new Set(prev).add(name));
    const error = validateField(name, formData[name as keyof LoanFormData]);
    setErrors((prev) => ({ ...prev, [name]: error }));
  };

  // ── Submit handler ───────────────────────────────────────────────────────
  // preventDefault → validate all → submit or show errors
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError(null);

    // Validate every field
    const newErrors: Record<string, string | undefined> = {};
    for (const [key, value] of Object.entries(formData)) {
      newErrors[key] = validateField(key, value);
    }

    const hasErrors = Object.values(newErrors).some(Boolean);
    setErrors(newErrors);
    setTouched(new Set(Object.keys(formData)));

    if (hasErrors) return;

    // All validations passed — submit to API
    setSubmitting(true);
    try {
      // Step 1: Create the applicant
      const applicant = await createApplicant({
        name: formData.applicantName,
        email: formData.email,
      });

      // Step 2: Create the loan application using the new applicant's id
      await createLoanApplication({
        applicantName: formData.applicantName,
        loanAmount: formData.loanAmount as number,
        annualIncome: (formData.annualIncome as number) || 0,
        notes: formData.notes,
        applicantId: applicant.id,
        loanTypeId: formData.loanTypeId as number,
      });

      // Success — navigate back to dashboard
      navigate("/");
    } catch (err) {
      setSubmitError(
        err instanceof Error ? err.message : "An unexpected error occurred",
      );
    } finally {
      setSubmitting(false);
    }
  };

  // ── Render ───────────────────────────────────────────────────────────────
  return (
    <div className="app">
      <header>
        <h1>Buckeye Lending</h1>
        <p>New Loan Application</p>
        <Link to="/" className="back-link">
          ← Back to Dashboard
        </Link>
      </header>

      <form className="loan-form" onSubmit={handleSubmit} noValidate>
        {/* Applicant Name */}
        <label>
          Applicant Name
          <input
            type="text"
            name="applicantName"
            value={formData.applicantName}
            onChange={handleChange}
            onBlur={handleBlur}
            {...(touched.has("applicantName") && errors.applicantName
              ? { "aria-invalid": "true" as const }
              : {})}
          />
          {touched.has("applicantName") && errors.applicantName && (
            <span className="field-error" role="alert">
              {errors.applicantName}
            </span>
          )}
        </label>

        {/* Email */}
        <label>
          Email
          <input
            type="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
            onBlur={handleBlur}
            {...(touched.has("email") && errors.email
              ? { "aria-invalid": "true" as const }
              : {})}
          />
          {touched.has("email") && errors.email && (
            <span className="field-error" role="alert">
              {errors.email}
            </span>
          )}
        </label>

        {/* Loan Amount */}
        <label>
          Loan Amount ($)
          <input
            type="number"
            name="loanAmount"
            value={formData.loanAmount}
            onChange={handleChange}
            onBlur={handleBlur}
            min={1000}
            max={500000}
            {...(touched.has("loanAmount") && errors.loanAmount
              ? { "aria-invalid": "true" as const }
              : {})}
          />
          {touched.has("loanAmount") && errors.loanAmount && (
            <span className="field-error" role="alert">
              {errors.loanAmount}
            </span>
          )}
        </label>

        {/* Annual Income */}
        <label>
          Annual Income ($)
          <input
            type="number"
            name="annualIncome"
            value={formData.annualIncome}
            onChange={handleChange}
            onBlur={handleBlur}
            min={0}
            {...(touched.has("annualIncome") && errors.annualIncome
              ? { "aria-invalid": "true" as const }
              : {})}
          />
          {touched.has("annualIncome") && errors.annualIncome && (
            <span className="field-error" role="alert">
              {errors.annualIncome}
            </span>
          )}
        </label>

        {/* Loan Type */}
        <label>
          Loan Type
          <select
            name="loanTypeId"
            value={formData.loanTypeId}
            onChange={handleChange}
            onBlur={handleBlur}
            {...(touched.has("loanTypeId") && errors.loanTypeId
              ? { "aria-invalid": "true" as const }
              : {})}
          >
            <option value="">Select a type…</option>
            {loanTypes.map((lt) => (
              <option key={lt.id} value={lt.id}>
                {lt.name}
              </option>
            ))}
          </select>
          {touched.has("loanTypeId") && errors.loanTypeId && (
            <span className="field-error" role="alert">
              {errors.loanTypeId}
            </span>
          )}
        </label>

        {/* Notes */}
        <label>
          Notes (optional)
          <textarea
            name="notes"
            value={formData.notes}
            onChange={handleChange}
            maxLength={500}
            rows={3}
          />
        </label>

        {/* Submit error */}
        {submitError && (
          <p className="error" role="alert">
            {submitError}
          </p>
        )}

        {/* Submit */}
        <button type="submit" className="submit-btn" disabled={submitting}>
          {submitting ? "Submitting…" : "Submit Application"}
        </button>
      </form>
    </div>
  );
}
