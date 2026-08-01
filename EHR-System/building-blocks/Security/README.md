# Security Package

Authentication, authorization, multi-tenancy, and encryption.

## Contents (23 files)

### Authentication (5 files)
- `IAuthenticationService.cs` - User authentication
- `ITokenProvider.cs` - JWT/token generation
- `AuthenticationResult.cs` - Auth result
- `IPasswordHasher.cs` - Password hashing
- `IUserStore.cs` - User persistence

### Authorization (4 files)
- `IAuthorizationService.cs` - Permission checking
- `IPermissionStore.cs` - Permission persistence
- `IClaimsProvider.cs` - Claims generation
- `AuthorizationContext.cs` - Auth context

### Multi-Tenancy (5 files)
- `ITenantResolver.cs` - Resolve current tenant
- `ITenantContext.cs` - Manage tenant scope
- `TenantInfo.cs` - Tenant metadata
- `TenantStatus.cs` - Tenant status enum

### Encryption (4 files)
- `IEncryptionService.cs` - Data encryption
- `IKeyManagementService.cs` - Key management
- `EncryptionAlgorithm.cs` - Algorithm enum
- `EncryptionKeyInfo.cs` - Key metadata

### Audit (5 files)
- `IAuditService.cs` - Audit logging
- `IAuditRepository.cs` - Audit storage
- `AuditEntry.cs` - Audit record
- `AuditAction.cs` - Action enumeration
- `AuditContext.cs` - Audit context

---

## Usage

```csharp
using EHRPlatform.Security.Authentication;
using EHRPlatform.Security.MultiTenancy;
using EHRPlatform.Security.Encryption;
```

## Parent

[← Building Blocks](../README.md)
