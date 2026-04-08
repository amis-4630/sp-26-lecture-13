// Types that match the shape returned by our ASP.NET Core API

export type LoanType = {
    id: number;
    name: string;
    description: string;
    maxTermMonths: number;
};

export type LoanApplication = {
    id: number;
    applicantName: string;
    loanAmount: number;
    annualIncome: number;
    status: string;
    riskRating: number;
    submittedDate: string;
    notes: string;
    applicantId: number;
    loanTypeId: number;
    loanType: LoanType;
};

// Payload shape for creating a new loan application via POST
export type CreateLoanApplicationPayload = {
    applicantName: string;
    loanAmount: number;
    annualIncome: number;
    notes: string;
    applicantId: number;
    loanTypeId: number;
};

// Payload shape for creating a new applicant via POST
export type CreateApplicantPayload = {
    name: string;
    email: string;
};

export type Applicant = {
    id: number;
    name: string;
    email: string;
    phone: string;
    createdDate: string;
};

const API_BASE = "http://localhost:5000";

/**
 * Fetch all loan applications from the API.
 *
 * Concepts demonstrated:
 *   - Fetch API:   browser-native way to make HTTP requests
 *   - Async/Await: makes asynchronous code read like synchronous code
 */
export async function fetchLoanApplications(): Promise<LoanApplication[]> {
    const response = await fetch(`${API_BASE}/api/loanapplications`);

    // fetch does NOT throw on 4xx/5xx — we must check manually
    if (!response.ok) {
        throw new Error(`API error: ${response.status} ${response.statusText}`);
    }

    const data: LoanApplication[] = await response.json();
    return data;
}

/**
 * Fetch all loan types for populating the loan-type dropdown.
 */
export async function fetchLoanTypes(): Promise<LoanType[]> {
    const response = await fetch(`${API_BASE}/api/loantypes`);
    if (!response.ok) {
        throw new Error(`API error: ${response.status} ${response.statusText}`);
    }
    return response.json();
}

/**
 * Create a new applicant. Returns the server-created Applicant (with id).
 */
export async function createApplicant(
    payload: CreateApplicantPayload,
): Promise<Applicant> {
    const response = await fetch(`${API_BASE}/api/applicants`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
    });
    if (!response.ok) {
        throw new Error(`API error: ${response.status} ${response.statusText}`);
    }
    return response.json();
}

/**
 * Create a new loan application. Returns the server-created application.
 * The server sets Status → "Pending Review" and SubmittedDate → now.
 */
export async function createLoanApplication(
    payload: CreateLoanApplicationPayload,
): Promise<LoanApplication> {
    const response = await fetch(`${API_BASE}/api/loanapplications`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
    });
    if (!response.ok) {
        throw new Error(`API error: ${response.status} ${response.statusText}`);
    }
    return response.json();
}
