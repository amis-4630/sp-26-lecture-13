import { describe, it, expect } from "vitest";
import { validateEmail } from "../../utils/validateEmail";

describe("validateEmail", () => {
    it("returns undefined for a valid email", () => {
        expect(validateEmail("user@buckeye.edu")).toBeUndefined();
    });

    it("returns an error for an empty string", () => {
        expect(validateEmail("")).toBe("Email is required");
    });

    it("returns an error for a string without @", () => {
        expect(validateEmail("no-at-sign.com")).toBe("Enter a valid email address");
    });

    it("returns an error for a string without a domain dot", () => {
        expect(validateEmail("user@nodot")).toBe("Enter a valid email address");
    });
});
