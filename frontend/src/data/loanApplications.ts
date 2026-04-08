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

import client from "../api/client";

/**
 * Fetch all loan applications from the API.
 * The axios client automatically attaches the JWT via the request interceptor.
 */
export async function fetchLoanApplications(): Promise<LoanApplication[]> {
    const { data } = await client.get<LoanApplication[]>("/api/loanapplications");
    return data;
}

/**
 * Fetch all loan types for populating the loan-type dropdown.
 */
export async function fetchLoanTypes(): Promise<LoanType[]> {
    const { data } = await client.get<LoanType[]>("/api/loantypes");
    return data;
}

/**
 * Create a new applicant. Returns the server-created Applicant (with id).
 */
export async function createApplicant(
    payload: CreateApplicantPayload,
): Promise<Applicant> {
    const { data } = await client.post<Applicant>("/api/applicants", payload);
    return data;
}

/**
 * Create a new loan application. Returns the server-created application.
 * The server sets Status → "Pending Review" and SubmittedDate → now.
 */
export async function createLoanApplication(
    payload: CreateLoanApplicationPayload,
): Promise<LoanApplication> {
    const { data } = await client.post<LoanApplication>("/api/loanapplications", payload);
    return data;
}
