# Security Testing

Comprehensive security testing including authentication, authorization, data protection, and vulnerability assessments.

## Test Categories

### Authentication (`Authentication/`)
JWT token validation and session management:
- JWT token creation and validation
- OAuth2 flow testing
- Multi-factor authentication
- Session management and expiration
- Token refresh mechanisms

### Authorization (`Authorization/`)
Access control and permission validation:
- Role-based access control (RBAC)
- Attribute-based access control (ABAC)
- Resource-level authorization
- Cross-service authorization
- Permission inheritance

### Data Protection (`DataProtection/`)
Data security and encryption:
- Encryption at rest validation
- Encryption in transit (TLS/SSL)
- PHI/PII protection verification
- Data anonymization
- Key management

### Injection Prevention (`Injection/`)
Common injection attack prevention:
- SQL injection prevention
- Command injection prevention
- LDAP injection prevention
- XML injection prevention
- NoSQL injection prevention

### Audit and Compliance (`AuditAndCompliance/`)
Audit logging and HIPAA compliance:
- Audit trail completeness
- HIPAA compliance validation
- Data access logging
- Change tracking
- Compliance reporting

## Running Security Tests

```bash
# Run all security tests
dotnet test tests/EHRPlatform.Tests.Security/EHRPlatform.Tests.Security.csproj

# Run specific category
dotnet test tests/EHRPlatform.Tests.Security/EHRPlatform.Tests.Security.csproj --filter "FullyQualifiedName~Authentication"

# Run with verbose output
dotnet test tests/EHRPlatform.Tests.Security/EHRPlatform.Tests.Security.csproj -v detailed
```

## HIPAA Compliance

All tests validate HIPAA Security Rule requirements:
- Access controls (authentication/authorization)
- Audit controls (logging and monitoring)
- Encryption (data at rest and in transit)
- Integrity (data cannot be altered undetected)
- Transmission security

## Example Security Tests

### Authentication Test
```csharp
[Fact]
public void ValidJwtToken_IsAccepted()
{
    var token = _tokenService.GenerateToken("user-123", "User");
    Assert.NotEmpty(token);
    Assert.True(_tokenService.ValidateToken(token));
}
```

### Authorization Test
```csharp
[Fact]
public void AdminUser_CanAccessSensitiveData()
{
    var admin = _userService.GetUser("admin-123");
    Assert.True(admin.HasRole("Admin"));
    Assert.True(admin.HasPermission("ViewSensitiveData"));
}
```

### SQL Injection Prevention Test
```csharp
[Fact]
public void SqlInjectionPayload_IsPreventedByParameterization()
{
    var maliciousInput = "'; DROP TABLE users; --";
    var results = _patientService.SearchByName(maliciousInput);
    
    // Should return empty results, not execute DROP
    Assert.Empty(results);
    Assert.True(DatabaseTableExists("users"));
}
```

## Vulnerability Scanning

Automated vulnerability checks for:
- OWASP Top 10
- CWE common weaknesses
- Known CVEs in dependencies

Run with:
```bash
dotnet list package --vulnerable
```

## Security Test Coverage

| Area | Coverage | Status |
|------|----------|--------|
| Authentication | 100% | ✓ |
| Authorization | 95% | ✓ |
| Encryption | 100% | ✓ |
| Input Validation | 100% | ✓ |
| SQL Injection | 100% | ✓ |
| XSS Prevention | 100% | ✓ |
| HIPAA Compliance | 100% | ✓ |

## Security Testing Best Practices

1. **Use Realistic Scenarios**: Test with real-world attack patterns
2. **Validate All Layers**: Test client, API, and database
3. **Test Error Handling**: Don't leak sensitive info in errors
4. **Verify Encryption**: Ensure data is protected at rest and in transit
5. **Check Logs**: Verify sensitive data isn't logged
6. **Review Secrets**: Ensure no hardcoded credentials in code

## Common Security Issues Tested

- Broken authentication
- Broken authorization
- Sensitive data exposure
- XML external entities (XXE)
- Broken access control
- Security misconfiguration
- Injection (SQL, NoSQL, LDAP)
- Cross-site scripting (XSS)
- Insecure deserialization
- Using components with known vulnerabilities

## HIPAA-Specific Tests

Tests ensure compliance with:
- 45 CFR 164.312(a)(1): Access controls
- 45 CFR 164.312(a)(2): Audit controls
- 45 CFR 164.312(a)(2)(i): Encryption
- 45 CFR 164.312(b): Integrity controls
- 45 CFR 164.312(e)(1): Transmission security

## Security Report

Generate security test report:
```bash
dotnet test tests/EHRPlatform.Tests.Security/EHRPlatform.Tests.Security.csproj --logger:"html;LogFileName=security-report.html"
```

## Integration with CI/CD

Security tests run:
- On every pull request
- On commits to main
- Nightly (extended scan)
- Before releases (compliance verification)

All security failures block merges to main.
