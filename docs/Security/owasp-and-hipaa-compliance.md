# Security, OWASP & HIPAA Compliance Guide

Comprehensive security patterns for backend applications, OWASP Top 10 mitigation, and HIPAA data protection.

---

## 1. OWASP Top 10 Mitigation in ASP.NET Core

1. **Broken Access Control**: Enforce policy-based authorization (`[Authorize(Policy = "RequireAdmin")]`) and resource-based imperative authorization (`IAuthorizationService`).
2. **Cryptographic Failures**: Encrypt Sensitive Personal Health Information (PHI) at rest using AES-256 and in transit using TLS 1.3.
3. **Injection**: Use parameterized queries via EF Core or Dapper; never concatenate raw user strings into SQL queries.

---

## 2. HIPAA Compliance Architectural Rules

- **Immutable Audit Logging**: Log every read and mutation access to patient medical records with timestamps and user IDs.
- **Data Anonymization**: Strip PII/PHI in non-production development environments.
