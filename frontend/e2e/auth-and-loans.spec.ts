import { test, expect } from "@playwright/test";

test("register → login → view dashboard → logout", async ({ page }) => {
    const timestamp = Date.now();
    const email = `e2e-${timestamp}@buckeye.edu`;
    const password = "TestPass123";
    const fullName = "E2E Test User";

    // ── Register ──────────────────────────────────────────────────────────
    await page.goto("/register");
    await page.getByLabel("Full Name").fill(fullName);
    await page.getByLabel("Email").fill(email);
    await page.getByLabel("Password").fill(password);
    await page.getByRole("button", { name: /register/i }).click();

    // Should redirect to the dashboard after register + auto-login
    await expect(page).toHaveURL("/", { timeout: 10_000 });
    await expect(page.getByText(email)).toBeVisible();

    // ── Logout ────────────────────────────────────────────────────────────
    await page.getByRole("button", { name: /logout/i }).click();
    await expect(page).toHaveURL("/login");

    // ── Login ─────────────────────────────────────────────────────────────
    await page.getByLabel("Email").fill(email);
    await page.getByLabel("Password").fill(password);
    await page.getByRole("button", { name: /sign in/i }).click();

    // Should redirect back to dashboard
    await expect(page).toHaveURL("/", { timeout: 10_000 });
    await expect(
        page.getByRole("heading", { name: /buckeye lending/i }),
    ).toBeVisible();

    // ── Verify dashboard loaded ───────────────────────────────────────────
    // New user has no loans so we expect the "0 applications" count
    await expect(page.getByText(/\d+ application/)).toBeVisible({
        timeout: 5_000,
    });

    // ── Logout again and verify redirect ──────────────────────────────────
    await page.getByRole("button", { name: /logout/i }).click();
    await expect(page).toHaveURL("/login");
});
