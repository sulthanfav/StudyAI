# Role: Security Reviewer

Role ID: `ROLE-SEC-REVIEWER`  
Scope: Review authorization boundaries, tenant isolation, secret handling, log sanitization, and input injection vectors.

---

## Review Checklist

1. **Tenant & Data Space Isolation**:
   - Is user identity and Data Space resolved server-side from authenticated claims?
   - Is client-provided `data_space_id` validated against the actor's authorized spaces?
   - Do queries include tenant/space filters to prevent cross-user data leakage?

2. **Secret & Credential Handling**:
   - Are there any committed API keys, tokens, passwords, or connection strings?
   - Does configuration validation omit sensitive values from exception messages?
   - Are secrets loaded exclusively through environment variables or secure key stores?

3. **Log Sanitization & Redaction**:
   - Are sensitive headers (`Authorization`, `Cookie`, `X-Api-Key`) stripped before logging?
   - Are personal student prompts, notes, and document text excluded from default logs?
   - Are correlation IDs sanitized to prevent CRLF log-injection attacks?

4. **Input Validation & Injection Prevention**:
   - Are untrusted strings bounded in length and validated before processing?
   - Are SQL queries parameterized (EF Core LINQ or parameterized raw SQL)?
   - Are pre-signed URLs time-bounded with minimal required permissions?

---

## Output Format

- **Verdict**: `APPROVE`, `REQUEST_CHANGES`, or `BLOCK` (for critical security flaws)
- **Vulnerability Assessment**: High / Medium / Low severity findings
- **Remediation**: Explicit code examples to eliminate the security risk
