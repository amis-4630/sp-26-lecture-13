/**
 * Basic email validation — checks for a non-empty value with @ and a dot in the domain.
 */
export function validateEmail(email: string): string | undefined {
    if (!email || email.trim().length === 0) {
        return "Email is required";
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        return "Enter a valid email address";
    }
    return undefined;
}
