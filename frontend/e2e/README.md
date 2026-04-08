# Playwright E2E Tests

## Prerequisites

- .NET SDK 10+ and Node.js 22+ installed
- Playwright browsers: `npx playwright install chromium`
- JWT user-secrets configured (see `backend/Buckeye.Lending.Api/README.md`)

## Running

From the `frontend/` directory:

```bash
npx playwright test
```

Playwright will automatically start the API and Vite dev server if they aren't already running (configured via `webServer` in `playwright.config.ts`).

To run with the UI:

```bash
npx playwright test --ui
```

To see a headed browser:

```bash
npx playwright test --headed
```

## Test structure

- `e2e/auth-and-loans.spec.ts` — Happy-path test: register → login → view dashboard → logout.

## Locators used

Tests use Playwright's recommended semantic locators:

- `getByRole` — buttons, headings
- `getByLabel` — form inputs via their `<label>` text
- `getByText` — visible text content

No raw CSS selectors. Minimal `data-testid` attributes:

_(None added yet — semantic locators are sufficient for the current UI.)_

## Known flake sources

1. **Cold start**: The API takes a few seconds to build on first run. The `webServer` timeout is set to 30s.
2. **Port conflicts**: If port 5000 or 5173 is already in use by another process, Playwright will reuse that server (via `reuseExistingServer: true`).
