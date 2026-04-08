# AGENT INSTRUCTIONS: Upgrade Buckeye Lending to Week 13 Reference Architecture

**Audience:** A coding agent (Claude Code, Copilot agent mode, etc.) with read/write access to the Buckeye Lending repo.
**Goal:** Bring the existing Buckeye Lending codebase up to the Week 13 reference-architecture state so the instructor can demo every concept Milestone 5 requires: JWT auth, role-based authorization, automated tests (backend + frontend), Playwright MCP E2E, and the W13 security checklist.
**Non-goal:** Do **not** add new business features. Do not refactor the loan domain model. Leave the existing Week 5–8 feature surface intact unless a security fix requires a minimal change.

---

## Ground Rules (Read Before Doing Anything)

1. **Work in phases. Stop at each phase boundary** and print a summary of what changed, what tests now pass, and what the next phase will touch. The instructor will resume you.
2. **Never commit secrets.** JWT signing keys, connection strings with passwords, and seed user passwords go in `dotnet user-secrets` or `.env.local` (gitignored). Never `appsettings.json`, never `.env`, never a string literal in source.
3. **Follow existing conventions.** The repo has established patterns (see `resources/agents/dotnet-agent.md` and `resources/agents/react-agent.md`). Match them. File-scoped namespaces, record DTOs, CSS Modules, etc.
4. **One commit per phase.** Commit message format: `feat(w13): phase N - <short summary>`. No squashing across phases.
5. **Every phase ends with green tests.** If a phase breaks an existing test, fix the root cause — do not weaken the assertion, and do not skip the test.
6. **Ask before adding a package** that is not in the allowlist below. Stop and surface the request.
7. **Preserve the teaching narrative.** Code that will be shown in lecture should read top-to-bottom without clever tricks. Favor obvious over elegant.

### Package allowlist (no approval needed)

Backend:

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `System.IdentityModel.Tokens.Jwt`
- `Microsoft.AspNetCore.Mvc.Testing`
- `Microsoft.EntityFrameworkCore.InMemory`
- `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`
- `FluentAssertions`

Frontend:

- `vitest`, `@testing-library/react`, `@testing-library/jest-dom`, `@testing-library/user-event`, `jsdom`
- `@playwright/test`, `@playwright/mcp`
- `axios` (only if not already installed)

---

## Assumed Starting State

The repo at head of `main` is the Week 8 state of Buckeye Lending:

- **API project**: `BuckeyeLendingApi/` — namespace `Buckeye.Lending.Api`, .NET 10, EF Core InMemory, `LendingContext`, `LoanApplicationsController`, seed data via `HasData`. No auth, no tests.
- **Frontend project**: `buckeye-lending-dashboard/` — Vite + React + TypeScript, loan application dashboard with filters and forms. No auth, no tests.
- **Solution root**: contains both projects plus a `README.md` and `.gitignore`.

Before starting, run `dotnet build` and `npm run build` (in the frontend) and confirm both succeed. If they don't, **stop** and report — the baseline is broken and must be fixed by a human first.

---

## Phase 1 — Backend Authentication

**Outcome:** Registered users can log in and receive a JWT. ASP.NET Core Identity is wired up. JWT signing key lives in user secrets.

### Tasks

1. Add packages: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`.
2. Create `Models/ApplicationUser.cs` extending `IdentityUser<int>` (keep the int key convention that matches existing entities). Add a `FullName` string property.
3. Change `LendingContext` to inherit from `IdentityDbContext<ApplicationUser, IdentityRole<int>, int>`. Update `OnModelCreating` to call `base.OnModelCreating(builder)` first. Existing loan seed data must still load.
4. Register Identity and JWT bearer auth in `Program.cs`:
   - `builder.Services.AddIdentityCore<ApplicationUser>(...)` with password rules: min length 8, require digit, require uppercase, require lowercase. No require non-alphanumeric (keeps seeding simple).
   - Add `.AddRoles<IdentityRole<int>>()`, `.AddEntityFrameworkStores<LendingContext>()`, `.AddSignInManager()`, `.AddDefaultTokenProviders()`.
   - `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` reading issuer, audience, and key from `IConfiguration["Jwt:Issuer|Audience|Key"]`.
   - Register a `TokenService` class that generates JWTs (claims: `NameIdentifier`, `Email`, `Role`; 1-hour expiry).
   - Add `app.UseAuthentication()` **before** `app.UseAuthorization()`.
5. Create `Controllers/AuthController.cs` with:
   - `POST /api/auth/register` — accepts `{ email, password, fullName }`, creates user via `UserManager.CreateAsync`, returns 201 on success or a `ValidationProblemDetails` listing Identity errors.
   - `POST /api/auth/login` — accepts `{ email, password }`, uses `UserManager.CheckPasswordAsync`, returns 401 with a **generic** message for any failure (do not distinguish "no such user" from "wrong password"), returns 200 with `{ token, expiresAt, userId, email, role }` on success.
6. Configure JWT signing key via user secrets. In the instructions you print at the end of this phase, tell the human to run:

   ```
   dotnet user-secrets init --project BuckeyeLendingApi
   dotnet user-secrets set "Jwt:Key" "<at least 32 random chars>" --project BuckeyeLendingApi
   dotnet user-secrets set "Jwt:Issuer" "buckeye-lending" --project BuckeyeLendingApi
   dotnet user-secrets set "Jwt:Audience" "buckeye-lending-clients" --project BuckeyeLendingApi
   ```

7. Seed two roles (`Admin`, `User`) and two seed users on startup via a `DbSeeder` class called after `EnsureCreated()`:
   - `admin@buckeye.edu` / `AdminPass123` — role `Admin`
   - `user@buckeye.edu` / `UserPass123` — role `User`
     Document these in `BuckeyeLendingApi/README.md` under a "Test Credentials" section.

### Acceptance criteria (Phase 1)

- [ ] `dotnet build` succeeds, zero warnings introduced by your changes.
- [ ] `dotnet run` starts the API cleanly and seeds both users on a fresh run.
- [ ] `POST /api/auth/register` with a valid body returns 201; invalid body returns 400 with Identity errors.
- [ ] `POST /api/auth/login` with the seeded admin returns 200 and a JWT whose `role` claim is `Admin` (verify via jwt.io or a quick decode).
- [ ] `POST /api/auth/login` with wrong password and with unknown email both return 401 with identical bodies.
- [ ] `git grep -n "Jwt:Key"` shows matches only in `Program.cs`, `README.md`, and this instruction file — no literal keys in source.
- [ ] Commit: `feat(w13): phase 1 - backend identity and JWT auth`.

**Stop here and print the Phase 1 summary.**

---

## Phase 2 — Protected Endpoints and Authorization

**Outcome:** Loan endpoints require authentication. Admin-only endpoints require the `Admin` role. Every loan-scoped endpoint that takes an id enforces ownership via the JWT, not via a URL or body parameter (fixing BOLA).

### Tasks

1. Add a nullable `OwnerUserId` (int) foreign key to `LoanApplication`, referencing `ApplicationUser`. Update the existing seed data so the two seed users each own at least one loan (keeps existing demo content visible post-upgrade).
2. On every method of `LoanApplicationsController`:
   - Add `[Authorize]` at the controller level.
   - `GET /api/loanapplications` — returns **only the caller's loans** (filter by `OwnerUserId == currentUserId`). Admin users get all loans. Detect admin via `User.IsInRole("Admin")`.
   - `GET /api/loanapplications/{id}` — returns 404 if the loan does not exist **or** is not owned by the caller (and the caller is not Admin). Do not return 403 — do not leak existence.
   - `POST /api/loanapplications` — sets `OwnerUserId` from the JWT (`User.FindFirst(ClaimTypes.NameIdentifier).Value`). Ignore any `ownerUserId` in the request body.
   - `PUT /api/loanapplications/{id}/status` — require `[Authorize(Roles = "Admin")]` on this method only.
   - `DELETE /api/loanapplications/{id}` — require Admin or owner; return 404 for non-owners.
3. Create an extension method `User.GetUserId()` in `Extensions/ClaimsPrincipalExtensions.cs` that parses `ClaimTypes.NameIdentifier` to `int` and throws if missing. Use it consistently — do **not** repeat the parsing logic inline.
4. Add a CORS policy in `Program.cs` that allows the frontend origin (read from configuration key `Frontend:Origin`, default `http://localhost:5173` for dev).

### Acceptance criteria (Phase 2)

- [ ] Unauthenticated request to any loan endpoint returns 401.
- [ ] Logged in as `user@buckeye.edu`, `GET /api/loanapplications` returns only that user's loans.
- [ ] Logged in as `user@buckeye.edu`, `GET /api/loanapplications/{id}` where `{id}` belongs to admin returns **404** (not 403, not 200).
- [ ] Logged in as `user@buckeye.edu`, `PUT /api/loanapplications/{id}/status` returns 403.
- [ ] Logged in as `admin@buckeye.edu`, the same `PUT` returns 200.
- [ ] POSTing a new loan with a spoofed `ownerUserId` in the body still records the caller as owner.
- [ ] Commit: `feat(w13): phase 2 - authorization and BOLA hardening`.

**Stop here and print the Phase 2 summary.**

---

## Phase 3 — Frontend Authentication

**Outcome:** The React app has a working login/register flow, stores the JWT, injects it on every API call, and redirects unauthenticated users away from protected routes.

### Tasks

1. Install `axios` if not already present. Create `src/api/client.ts` exporting a configured axios instance whose base URL comes from `import.meta.env.VITE_API_BASE_URL` (default `http://localhost:5000`). Add a request interceptor that reads the JWT from the auth context and sets `Authorization: Bearer <token>`.
2. Create `src/context/AuthContext.tsx`:
   - State: `{ user: { id, email, role } | null, token: string | null, status: 'idle' | 'loading' | 'authenticated' | 'error' }`.
   - Actions: `login(email, password)`, `register(email, password, fullName)`, `logout()`.
   - Persist the token in `sessionStorage` (not `localStorage` — explain this choice in a code comment citing W13 XSS concerns).
   - Rehydrate from `sessionStorage` on mount.
3. Create pages: `src/pages/LoginPage.tsx`, `src/pages/RegisterPage.tsx`. Use the existing form patterns from Week 8 (same CSS Modules, same validation error display). Show inline field errors on submit failure. Redirect to `/dashboard` on success.
4. Create `src/components/ProtectedRoute.tsx` — a wrapper component that renders its children only when `status === 'authenticated'`, otherwise `<Navigate to="/login" replace />`.
5. Update `src/App.tsx` (or the router) so `/`, `/dashboard`, `/loans/*` are behind `ProtectedRoute`. `/login` and `/register` are public.
6. Add a header component with the logged-in user's email and a Logout button.
7. Create `buckeye-lending-dashboard/.env.local.example` listing `VITE_API_BASE_URL` with the dev default. Add `.env.local` to `.gitignore` (verify it already is).

### Acceptance criteria (Phase 3)

- [ ] `npm run dev` starts cleanly.
- [ ] Unauthenticated visit to `/dashboard` redirects to `/login`.
- [ ] Registering with a valid email/password/fullName creates an account and logs the user in.
- [ ] Logging in as `user@buckeye.edu` shows that user's loans (phase 2 wired up end to end).
- [ ] Reloading the page after login keeps the user logged in.
- [ ] Logout clears the token and redirects to `/login`.
- [ ] Network tab shows every `/api/loanapplications*` request carrying `Authorization: Bearer ...`.
- [ ] Commit: `feat(w13): phase 3 - frontend auth flow`.

**Stop here and print the Phase 3 summary.**

---

## Phase 4 — Backend Automated Tests

**Outcome:** A test project runs on every `dotnet test`, with at least 3 pure unit tests and at least 2 integration tests (one for auth, one for the BOLA fix).

### Tasks

1. Create `BuckeyeLendingApi.Tests/` as an xUnit project referencing the API project. Add `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.InMemory`, `FluentAssertions`.
2. Add a `Tests/Unit/` folder with at least three unit tests of pure logic — no HTTP, no DB. Pick real classes from the existing domain. Suggested targets (implement whichever ones match the actual code):
   - `LoanApplication_MonthlyPayment_ComputesCorrectly` (if there is any computed property — if not, add a small pure calculator class first and test it).
   - `ClaimsPrincipalExtensions_GetUserId_ThrowsWhenMissing`.
   - `PasswordPolicy_RejectsShortPasswords` (wrap the Identity options into a thin validator if needed).
3. Add a `Tests/Integration/` folder with a `CustomWebApplicationFactory : WebApplicationFactory<Program>` that swaps `LendingContext` to the in-memory provider with a unique database name per test, and seeds two users and two loans.
4. Write two integration tests:
   - `GetLoan_ReturnsNotFound_WhenLoanBelongsToAnotherUser` — logs in as user, requests admin's loan id, asserts 404.
   - `UpdateLoanStatus_Returns403_ForNonAdmin` — logs in as user, attempts the admin-only endpoint, asserts 403.
5. Mark `Program.cs` as `public partial class Program` at the end of the file so `WebApplicationFactory<Program>` can find it.
6. Add a `BuckeyeLendingApi.Tests.csproj` entry in the solution file.

### Acceptance criteria (Phase 4)

- [ ] `dotnet test` runs from the solution root and reports ≥5 tests passing, 0 failing.
- [ ] The two integration tests assert the **exact** status code expected (404 and 403). Do not weaken to `IsSuccessStatusCode == false`.
- [ ] Tests do not touch the developer's real InMemory database instance — each test gets a fresh scope.
- [ ] Commit: `feat(w13): phase 4 - backend unit and integration tests`.

**Stop here and print the Phase 4 summary.**

---

## Phase 5 — Frontend Automated Tests

**Outcome:** Vitest + React Testing Library runs with at least 3 meaningful tests covering a pure helper, a reducer/context, and a component.

### Tasks

1. Install: `vitest`, `@testing-library/react`, `@testing-library/jest-dom`, `@testing-library/user-event`, `jsdom`. Add a `test` script (`vitest run`) and a `test:watch` script to `package.json`.
2. Create `vitest.config.ts` with `environment: 'jsdom'`, `setupFiles: './src/test/setup.ts'`. Setup file imports `@testing-library/jest-dom/vitest`.
3. Write three tests:
   - `src/utils/__tests__/validateEmail.test.ts` — pure function, happy path and at least two failure cases.
   - `src/context/__tests__/authContext.test.tsx` — mount a minimal consumer, dispatch `login` with a mocked `api/client`, assert state transitions to `authenticated`.
   - `src/pages/__tests__/LoginPage.test.tsx` — render the page inside a memory router and a mocked `AuthContext`, submit with empty fields, assert an inline error message renders via `screen.findByText`.
4. Do NOT mock React Router at a level that defeats the test. Use `MemoryRouter` for routing-aware tests.

### Acceptance criteria (Phase 5)

- [ ] `npm test -- --run` exits 0 with at least 3 tests passing.
- [ ] No test uses `@ts-ignore`, `as any`, or `eslint-disable` to silence errors.
- [ ] No test relies on timing (`setTimeout`/`waitFor` with raw ms) — use Testing Library's async queries.
- [ ] Commit: `feat(w13): phase 5 - frontend vitest suite`.

**Stop here and print the Phase 5 summary.**

---

## Phase 6 — Playwright MCP End-to-End

**Outcome:** A committed Playwright spec that drives a browser through register → login → view loan → log out, runnable with `npx playwright test`. The spec was generated by driving the Playwright MCP with Copilot agent mode (or equivalent), then exported.

### Tasks

1. Install `@playwright/test` as a devDependency. Run `npx playwright install chromium`.
2. Create `buckeye-lending-dashboard/e2e/` with a `playwright.config.ts` configured to:
   - `baseURL: http://localhost:5173`
   - `webServer` entries that start both the API (`dotnet run --project ../BuckeyeLendingApi`) and the frontend (`npm run dev`) if not already running
   - `projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }]`
3. Create `e2e/auth-and-loans.spec.ts` with **one** test:
   - Register a fresh user with a timestamped email.
   - Log in with those credentials.
   - Assert the dashboard shows the empty state (or the expected default).
   - Create a loan application via the UI.
   - Assert the new loan appears in the list.
   - Log out. Assert redirect to `/login`.
4. Use Playwright's recommended locators: `getByRole`, `getByLabel`, `getByTestId`. No raw CSS selectors.
5. Where the existing UI doesn't have a good accessible name, add a `data-testid` rather than relying on text that will change.
6. Create `e2e/README.md` documenting:
   - How to run the spec locally
   - How to regenerate the spec from the Playwright MCP (prompt template included — see the [W13 Friday lab](../../content/labs/week-13/security-workshop.md) Exercise 4 for the pattern)
   - Known flake sources and how to fix them

### Acceptance criteria (Phase 6)

- [ ] `npx playwright test` from `buckeye-lending-dashboard/` runs the spec and exits 0 with both the API and frontend running.
- [ ] The spec runs in under 60 seconds on a warm machine.
- [ ] `data-testid` attributes added to components are minimal and documented in `e2e/README.md`.
- [ ] Commit: `feat(w13): phase 6 - playwright e2e happy path`.

**Stop here and print the Phase 6 summary.**

---

## Phase 7 — Security Checklist Hardening

**Outcome:** The W13 security checklist passes cleanly. This is the phase that makes Buckeye Lending a **reference** architecture rather than just a working one.

### Tasks

1. `Program.cs`: add `app.UseHttpsRedirection()` and `app.UseHsts()` (behind `if (!app.Environment.IsDevelopment())` for HSTS).
2. Add the `Microsoft.AspNetCore.HttpsPolicy` pieces needed for a clean dev cert flow. Document `dotnet dev-certs https --trust` in `BuckeyeLendingApi/README.md`.
3. Add secure response headers via a small middleware: `X-Content-Type-Options: nosniff`, `Referrer-Policy: no-referrer`, `X-Frame-Options: DENY`, `Content-Security-Policy: default-src 'self'` (Content-Security-Policy on API responses only — do not try to set it on the SPA from the API).
4. Verify and document: **all** EF queries use LINQ, not `FromSqlRaw`. If any raw SQL exists, replace it. Grep for `FromSqlRaw`, `ExecuteSqlRaw`, `DbCommand` — none should remain.
5. Verify and document: **no** `dangerouslySetInnerHTML` anywhere in the frontend. Grep for it. If it exists, replace with normal JSX.
6. Add a `SECURITY.md` at the repo root listing exactly which of the W13 checklist items Buckeye Lending satisfies and where (file + line citation). This is the artifact the instructor will point at in lecture.
7. Run `git grep -iE "password|secret|jwt.*key"` and confirm no hits outside tests, documentation, and Identity's own APIs. Record the result in `SECURITY.md`.

### Acceptance criteria (Phase 7)

- [ ] `curl -I http://localhost:5000/api/loanapplications` (unauthenticated) shows all three security headers set.
- [ ] `grep -r "FromSqlRaw\|ExecuteSqlRaw" BuckeyeLendingApi/` returns no matches in production code.
- [ ] `grep -r "dangerouslySetInnerHTML" buckeye-lending-dashboard/src` returns no matches.
- [ ] `SECURITY.md` exists at repo root with a populated checklist.
- [ ] All previous phases' tests still pass (`dotnet test` and `npm test -- --run`).
- [ ] Commit: `feat(w13): phase 7 - security checklist hardening`.

---

## Final Verification (Before Handing Back to the Instructor)

Run this sequence from a fresh clone of the repo. **If any step fails, stop and report — do not paper over.**

```bash
# Backend
cd BuckeyeLendingApi
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 32)"
dotnet user-secrets set "Jwt:Issuer" "buckeye-lending"
dotnet user-secrets set "Jwt:Audience" "buckeye-lending-clients"
dotnet build
dotnet test
dotnet run &  # leave running
cd ..

# Frontend
cd buckeye-lending-dashboard
npm ci
npm test -- --run
npm run build
npm run dev &  # leave running
cd ..

# E2E
cd buckeye-lending-dashboard
npx playwright test
```

Report at the end:

- Total files added/modified per phase
- Total tests passing (backend + frontend + e2e)
- Any package approvals requested during the run
- Any deviations from these instructions and why

---

## Deliverables Checklist

- [ ] Backend: Identity, JWT, register/login, role-based authorization, BOLA fix, secrets in user-secrets
- [ ] Backend tests: ≥3 unit + ≥2 integration, all passing
- [ ] Frontend: auth context, login/register pages, protected routes, token interceptor, logout
- [ ] Frontend tests: ≥3 Vitest tests, all passing
- [ ] Playwright E2E: one happy-path spec committed and passing
- [ ] Security: HTTPS, secure headers, no raw SQL, no `dangerouslySetInnerHTML`, `SECURITY.md` populated
- [ ] Docs: `README.md` test credentials, `.env.local.example`, `e2e/README.md`, `SECURITY.md`
- [ ] Seven commits, one per phase, all on a single branch named `w13-reference-architecture`
- [ ] No secrets in git history (`git log -p` grep clean)

When all boxes are checked, push the branch and open a PR titled `W13 reference architecture upgrade` with a body that links each phase commit to the corresponding deliverable.
