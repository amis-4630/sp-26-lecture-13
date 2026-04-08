# Security Checklist — Buckeye Lending (Week 13)

This document records which W13 security checklist items Buckeye Lending satisfies and where.

## Authentication & Authorization

| Item                                                 | Status | Location                                                                                                                                       |
| ---------------------------------------------------- | ------ | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| JWT-based authentication                             | ✅     | [Program.cs](backend/Buckeye.Lending.Api/Program.cs) — JWT Bearer registration                                                                 |
| Password hashing via ASP.NET Identity                | ✅     | [Program.cs](backend/Buckeye.Lending.Api/Program.cs) — `AddIdentityCore` with password rules                                                   |
| Password policy: min 8 chars, digit, upper, lower    | ✅     | [Program.cs](backend/Buckeye.Lending.Api/Program.cs) — `options.Password.*`                                                                    |
| Generic login failure messages (no user enumeration) | ✅     | [AuthController.cs](backend/Buckeye.Lending.Api/Controllers/AuthController.cs) — single "Invalid email or password"                            |
| JWT signing key in user-secrets (not in source)      | ✅     | [appsettings.Development.json](backend/Buckeye.Lending.Api/appsettings.Development.json) — only issuer/audience, key via `dotnet user-secrets` |
| Role-based authorization (Admin/User)                | ✅     | [LoanApplicationsController.cs](backend/Buckeye.Lending.Api/Controllers/LoanApplicationsController.cs) — `[Authorize(Roles = "Admin")]` on PUT |
| `[Authorize]` on all data endpoints                  | ✅     | All controllers: Applicants, LoanApplications, LoanTypes, Notes, Payments, ReviewQueue                                                         |

## BOLA (Broken Object-Level Authorization)

| Item                                       | Status | Location                                                                                                                                     |
| ------------------------------------------ | ------ | -------------------------------------------------------------------------------------------------------------------------------------------- |
| Ownership check on loan read               | ✅     | [LoanApplicationsController.cs](backend/Buckeye.Lending.Api/Controllers/LoanApplicationsController.cs) — `GetById` returns 404 for non-owned |
| Ownership filter on loan list              | ✅     | [LoanApplicationsController.cs](backend/Buckeye.Lending.Api/Controllers/LoanApplicationsController.cs) — `GetAll` filters by `OwnerUserId`   |
| OwnerUserId set from JWT, not request body | ✅     | [LoanApplicationsController.cs](backend/Buckeye.Lending.Api/Controllers/LoanApplicationsController.cs) — `Create` method                     |
| 404 (not 403) for non-owned resources      | ✅     | `GetById` and `Delete` — prevents existence leaking                                                                                          |

## Transport Security

| Item                               | Status | Location                                                                                                 |
| ---------------------------------- | ------ | -------------------------------------------------------------------------------------------------------- |
| HTTPS redirect in production       | ✅     | [Program.cs](backend/Buckeye.Lending.Api/Program.cs) — `UseHttpsRedirection()` behind `!IsDevelopment()` |
| HSTS in production                 | ✅     | [Program.cs](backend/Buckeye.Lending.Api/Program.cs) — `UseHsts()` behind `!IsDevelopment()`             |
| CORS restricted to frontend origin | ✅     | [Program.cs](backend/Buckeye.Lending.Api/Program.cs) — reads `Frontend:Origin` config                    |

## Security Headers

| Item                                          | Status | Location                                                                                            |
| --------------------------------------------- | ------ | --------------------------------------------------------------------------------------------------- |
| `X-Content-Type-Options: nosniff`             | ✅     | [SecurityHeadersMiddleware.cs](backend/Buckeye.Lending.Api/Middleware/SecurityHeadersMiddleware.cs) |
| `Referrer-Policy: no-referrer`                | ✅     | [SecurityHeadersMiddleware.cs](backend/Buckeye.Lending.Api/Middleware/SecurityHeadersMiddleware.cs) |
| `X-Frame-Options: DENY`                       | ✅     | [SecurityHeadersMiddleware.cs](backend/Buckeye.Lending.Api/Middleware/SecurityHeadersMiddleware.cs) |
| `Content-Security-Policy: default-src 'self'` | ✅     | [SecurityHeadersMiddleware.cs](backend/Buckeye.Lending.Api/Middleware/SecurityHeadersMiddleware.cs) |

## Injection Prevention

| Item                                            | Status | Location                                                                        |
| ----------------------------------------------- | ------ | ------------------------------------------------------------------------------- |
| No `FromSqlRaw` / `ExecuteSqlRaw` / `DbCommand` | ✅     | Verified: `grep -r "FromSqlRaw\|ExecuteSqlRaw\|DbCommand" backend/` — 0 matches |
| All queries use LINQ + EF Core                  | ✅     | All controllers use `_context.*.Where()`, `.FindAsync()`, etc.                  |
| No `dangerouslySetInnerHTML` in frontend        | ✅     | Verified: `grep -r "dangerouslySetInnerHTML" frontend/src/` — 0 matches         |

## Client-Side Security

| Item                                                | Status | Location                                                                                          |
| --------------------------------------------------- | ------ | ------------------------------------------------------------------------------------------------- |
| JWT stored in `sessionStorage` (not `localStorage`) | ✅     | [AuthContext.tsx](frontend/src/contexts/AuthContext.tsx) — comment cites XSS concern              |
| Token cleared on logout                             | ✅     | [AuthContext.tsx](frontend/src/contexts/AuthContext.tsx) — `logout()` removes from sessionStorage |
| Protected routes redirect unauthenticated users     | ✅     | [ProtectedRoute.tsx](frontend/src/components/ProtectedRoute.tsx)                                  |
| Bearer token attached via interceptor               | ✅     | [client.ts](frontend/src/api/client.ts) — axios request interceptor                               |

## Secrets Scan

Result of `git grep -iE "password|secret|jwt.*key"` (expected matches only in docs, tests, and Identity APIs):

- `backend/Buckeye.Lending.Api/Services/DbSeeder.cs` — seed user passwords (development only, documented in README)
- `backend/Buckeye.Lending.Api.Tests/` — test-only JWT keys and passwords
- `docs/update-plans.md` — documentation
- `SECURITY.md` — this file

No literal secrets found in production source code outside of Identity/test infrastructure.
