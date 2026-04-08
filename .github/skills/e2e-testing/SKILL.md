---
name: e2e-testing
description: "Write, debug, and maintain Playwright end-to-end tests for the Buckeye Lending app. Use when: writing e2e tests, adding new test scenarios, debugging failing specs, fixing flaky tests, improving e2e coverage, generating page helpers, verifying user flows end-to-end."
argument-hint: "Describe the user flow or scenario to test"
---

# Playwright E2E Testing

Write, debug, and improve Playwright e2e tests for a full-stack app (React/TypeScript frontend + .NET API backend).

## When to Use

- Writing new e2e test scenarios for user flows
- Debugging a failing or flaky e2e spec
- Adding coverage for untested features
- Refactoring tests for better reliability

## Project Layout

| Item               | Path                                              |
| ------------------ | ------------------------------------------------- |
| Test files         | `frontend/e2e/*.spec.ts`                          |
| Playwright config  | `frontend/playwright.config.ts`                   |
| E2E flow scenarios | `docs/e2e-testing-flow.md`                        |
| Conventions        | `.github/instructions/playwright.instructions.md` |
| Frontend source    | `frontend/src/`                                   |
| Backend API        | `backend/Buckeye.Lending.Api/`                    |

## Constraints

- **Only modify** files in `frontend/e2e/` or `frontend/playwright.config.ts`
- **Never modify** backend C# code or frontend application source
- **Never add** `data-testid` attributes unless semantic locators are impossible
- **Never start** servers manually — `playwright.config.ts` manages both via `webServer`

## Procedure

### 1. Gather Context

Read existing tests and the scenario doc to understand current coverage:

```
frontend/e2e/*.spec.ts
docs/e2e-testing-flow.md
```

If testing a specific UI flow, also read the relevant frontend page components under `frontend/src/pages/` and `frontend/src/components/` to identify available labels, roles, and text.

### 2. Plan the Test

Before writing code, outline:

- **Preconditions**: What state is needed? (e.g., registered user, existing loan)
- **Steps**: User actions in sequence
- **Assertions**: What to verify at each checkpoint
- **Isolation**: Each test must create its own state — never depend on other tests

### 3. Write the Test

Follow these locator preferences (strict order):

1. `getByRole` — buttons, headings, links, textboxes
2. `getByLabel` — form inputs via `<label>`
3. `getByText` — visible text content
4. `getByTestId` — last resort only

Key patterns:

```typescript
// Unique test data — always use timestamps
const email = `e2e-${Date.now()}@buckeye.edu`;
const password = "TestPass123";

// Navigation — use relative URLs (baseURL configured)
await page.goto("/register");

// Assertions — never use waitForSelector
await expect(page).toHaveURL("/", { timeout: 10_000 });
await expect(locator).toBeVisible();

// Server-dependent assertions — add explicit timeout
await expect(page.getByText(/\d+ application/)).toBeVisible({ timeout: 5_000 });
```

### 4. Structure the File

- **One spec file per feature area**: `auth.spec.ts`, `loans.spec.ts`, `payments.spec.ts`
- **Use `test.describe`** to group related scenarios
- **Descriptive names**: `test("action → expected result", ...)`

```typescript
import { test, expect } from "@playwright/test";

test.describe("Loan Applications", () => {
  test("submit new loan → appears in dashboard", async ({ page }) => {
    // ...
  });

  test("view loan details → shows correct amounts", async ({ page }) => {
    // ...
  });
});
```

### 5. Run and Verify

```bash
# Run all e2e tests
cd frontend && npx playwright test

# Run a single test by name
npx playwright test -g "test name"

# Run headed (see the browser)
npx playwright test --headed

# Debug with trace
npx playwright show-trace test-results/*/trace.zip
```

### 6. Debug Failures

When a test fails:

1. **Read the error output** — Playwright shows the failing locator and expected vs. actual
2. **Check if the locator is stale** — UI text or roles may have changed; re-read the component source
3. **Check timing** — If an assertion times out, the server may be slow; increase the assertion timeout (not the global timeout)
4. **Check isolation** — If the test depends on state from another test, refactor to create its own state
5. **Inspect the trace** — `npx playwright show-trace` gives a step-by-step visual replay

Common flake sources:

- **Cold start**: .NET API takes a few seconds on first run (webServer timeout is 30s)
- **Port conflicts**: Ports 5000/5173 may be held by another process (`reuseExistingServer: true`)

## Shared Helper: Fresh User Registration

Many tests need a logged-in user. Extract this into a helper when multiple specs need it:

```typescript
async function registerAndLogin(page, overrides?: { email?: string }) {
  const email = overrides?.email ?? `e2e-${Date.now()}@buckeye.edu`;
  const password = "TestPass123";

  await page.goto("/register");
  await page.getByLabel("Full Name").fill("E2E Test User");
  await page.getByLabel("Email").fill(email);
  await page.getByLabel("Password").fill(password);
  await page.getByRole("button", { name: /register/i }).click();
  await expect(page).toHaveURL("/", { timeout: 10_000 });

  return { email, password };
}
```

## Output Format

- **New test**: Provide the complete spec file
- **Fix**: Show the failing assertion, explain root cause, provide the corrected code
- **Coverage gap**: List untested flows from `docs/e2e-testing-flow.md` and propose specs
