---
applyTo: "frontend/e2e/**/*.spec.ts"
description: "Playwright e2e test conventions for the Buckeye Lending app"
---

# Playwright E2E Conventions

## Locators (in order of preference)

1. `getByRole` — buttons, headings, links, textboxes
2. `getByLabel` — form inputs via their `<label>`
3. `getByText` — visible text content
4. `getByTestId` — last resort only

Never use raw CSS selectors (`page.locator('.btn-primary')`).

## Assertions

- Use `await expect(locator).toBeVisible()` instead of `waitForSelector`
- Add explicit timeouts on assertions that wait for server responses: `{ timeout: 10_000 }`
- Prefer `toHaveURL` for navigation checks

## Test Data

- Generate unique emails with `Date.now()`: `` `e2e-${Date.now()}@buckeye.edu` ``
- Use deterministic passwords that meet complexity rules: `"TestPass123"`
- Never rely on pre-existing database state

## Structure

- One spec file per feature area (e.g., `auth.spec.ts`, `loans.spec.ts`)
- Use `test.describe` to group related scenarios
- Descriptive test names: `test("action → expected result", ...)`

## Servers

Do not start servers manually. `playwright.config.ts` manages both the .NET API and Vite dev server via `webServer`.

## Debugging

- Run single test: `npx playwright test -g "test name"`
- See browser: `npx playwright test --headed`
- Inspect trace: `npx playwright show-trace test-results/*/trace.zip`
