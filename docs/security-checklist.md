# Security Checklist

| Category           | Check                        | Common Mistake                                   |
| ------------------ | ---------------------------- | ------------------------------------------------ |
| **Access Control** | UserId from JWT, not URL     | Still using `CURRENT_USER_ID` constant           |
| **Access Control** | Admin roles enforced         | `[Authorize]` without `Roles` on admin endpoints |
| **Auth**           | Passwords hashed             | Plain text or weak MD5/SHA1                      |
| **Auth**           | JWT expiration set           | Token never expires (or expires in 30 days)      |
| **Auth**           | Signing key not in code      | Key in `appsettings.json` committed to GitHub    |
| **Injection**      | LINQ queries only            | `FromSqlRaw` with string concatenation           |
| **XSS**            | No `dangerouslySetInnerHTML` | Rendering user HTML input                        |
| **CSRF**           | JWT in localStorage          | JWT in cookie without anti-CSRF token            |
