# Security Policy

## Supported Versions

| Version | Supported |
|:--- |:--- |
| 1.x (current) | ✅ Active |
| < 1.0 | ❌ End of life |

---

## Reporting a Vulnerability

**Please do NOT open a public GitHub issue for security vulnerabilities.**

Report security issues privately via one of the following:

- **GitHub Security Advisories**: [Report here](https://github.com/Mostafa-SAID7/EHR-Systems/security/advisories/new)
- **Email**: security@ehrplatform.com *(use subject line: `[SECURITY]`)*

You will receive a response within **48 hours** and a fix timeline within **7 days** for critical issues.

---

## Scope

Issues in scope for responsible disclosure:

- Authentication & authorization bypass
- PHI (Protected Health Information) exposure
- SQL injection / RCE / SSRF
- JWT token forgery or improper validation
- Privilege escalation
- Insecure direct object references (IDOR)

---

## Security Documentation

Full implementation details are in:

- [`docs/SECURITY.md`](./docs/SECURITY.md) — HIPAA compliance, encryption, audit logging
- [`docs/Security/`](./docs/Security/) — AuthN/AuthZ, OWASP Top 10, rate limiting

---

*Last Updated: July 2026*
