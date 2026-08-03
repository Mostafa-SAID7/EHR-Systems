# 🔐 Identity Service

Enterprise authentication and authorization microservice for the EHR Platform.

---

## 📋 Overview

The Identity Service provides:
- **JWT-based authentication** with access and refresh tokens
- **Role-based access control** (RBAC)
- **Two-factor authentication** (2FA)
- **Password management** with strength validation
- **Token refresh** and revocation
- **Audit logging** for security events

---

## 🏗️ Architecture

**6-Layer Clean Architecture:**

```
Identity.API           → HTTP endpoints (Controllers)
Identity.Application   → Business logic (Handlers, Services)
Identity.Contracts     → DTOs, Requests, Responses
Identity.Domain        → Entities, Events, Exceptions
Identity.Infrastructure → JWT, Password Policy, Metrics
Identity.Persistence   → EF Core, Database, Repositories
```

---

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- SQL Server 2022
- Redis 7+
- RabbitMQ 3+

### Local Development

**1. Build the service:**
```bash
dotnet build src/Identity.API/Identity.API.csproj
```

**2. Run with Docker Compose:**
```bash
docker-compose -f docker-compose.yml up -d
```

**3. Access the API:**
- **Base URL:** `http://localhost:5001`
- **Health Check:** `GET /health`
- **API Docs:** `http://localhost:5001/swagger`

---

## 🐳 Docker

### Development Build
```bash
docker build -t ehr-identity-api:dev -f Dockerfile .
docker-compose -f docker-compose.yml up -d
```

### Production Build
```bash
docker build -t ehr-identity-api:latest -f Dockerfile .
docker-compose -f docker/docker-compose.prod.yml up -d
```

**Environment Variables (Production):**
```
DB_PASSWORD              # SQL Server SA password
REDIS_PASSWORD          # Redis password
RABBITMQ_USER          # RabbitMQ username
RABBITMQ_PASSWORD      # RabbitMQ password
JWT_SECRET             # JWT signing secret (min 32 chars)
JWT_ISSUER             # JWT issuer
JWT_AUDIENCE           # JWT audience
ENCRYPTION_KEY         # Data encryption key
```

---

## 🔌 API Endpoints

### Authentication
- `POST /api/auth/login` - Login with email/password
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout (revoke token)
- `POST /api/auth/register` - Register new user

### Users
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user
- `POST /api/users/{id}/2fa/enable` - Enable 2FA
- `POST /api/users/{id}/password/change` - Change password

### Roles & Permissions
- `GET /api/roles` - List all roles
- `POST /api/roles` - Create role
- `GET /api/permissions` - List permissions
- `POST /api/users/{id}/roles` - Assign role to user

---

## 🔐 Security Features

| Feature | Details |
|---------|---------|
| **JWT Tokens** | HS256 signed, 60-min expiration |
| **Refresh Tokens** | 7-day expiration with rotation |
| **Password Policy** | Min 12 chars, uppercase, lowercase, digit, special char |
| **2FA** | OTP via email/SMS, backup codes |
| **Encryption** | AES-256 for sensitive data |
| **Audit Logging** | All auth events logged |

---

## 📊 Database Schema

**Core Tables:**
- `Users` - User accounts
- `Roles` - Role definitions
- `Permissions` - Permission definitions
- `UserRoles` - User-to-role mapping
- `RolePermissions` - Role-to-permission mapping
- `RefreshTokens` - Refresh token storage
- `LoginAudits` - Login attempt history
- `MfaSetup` - 2FA configurations

---

## 🛠️ Development

### Run Tests
```bash
dotnet test src/Identity.Domain.Tests/Identity.Domain.Tests.csproj
dotnet test src/Identity.Application.Tests/Identity.Application.Tests.csproj
```

### Code Style
```bash
# Format code
dotnet format src/

# Run linter
dotnet build /p:EnforceCodeStyleInBuild=true
```

### Database Migrations
```bash
# Create migration
dotnet ef migrations add <MigrationName> -p src/Identity.Persistence -s src/Identity.API

# Apply migration
dotnet ef database update -p src/Identity.Persistence -s src/Identity.API
```

---

## 📈 Monitoring

### Health Checks
- `/health` - Basic health
- `/health/ready` - Readiness probe (all dependencies)

### Metrics (Prometheus)
- `/metrics` - Prometheus-format metrics

**Tracked Metrics:**
- `identity_login_success` - Successful logins
- `identity_login_failure` - Failed logins
- `identity_token_refresh` - Token refreshes
- `identity_unauthorized_requests` - 401 errors
- `identity_forbidden_requests` - 403 errors

---

## 🔄 Integration

### Dependencies
- **Building-Blocks Security:** JWT, Encryption, Rate Limiting
- **Building-Blocks Common:** Exceptions, Validation
- **Building-Blocks EventBus:** Domain event publishing

### Publishes Events
- `UserCreated` - When user registers
- `UserLoggedIn` - When user logs in
- `PasswordChanged` - When password updated

### Subscribes to Events
- (None - Identity is the auth authority)

---

## 🚨 Troubleshooting

| Issue | Solution |
|-------|----------|
| **Connection refused** | Check SQL Server is running on port 1433 |
| **Redis timeout** | Verify Redis container health: `docker ps` |
| **RabbitMQ errors** | Access Management UI at `http://localhost:15672` (guest/guest) |
| **JWT validation failed** | Ensure `Jwt__Secret` is identical across services |
| **2FA not working** | Check email service configuration |

---

## 📚 Documentation

- [API Documentation](./docs/API.md)
- [Security Model](./docs/SECURITY.md)
- [Database Schema](./docs/DATABASE.md)
- [Docker Configuration](./docker/README.md)

---

## 📝 License

Part of the EHR Platform. See LICENSE file.

---

## 👥 Support

For issues or questions, open a GitHub issue or contact the DevOps team.

**Status:** ✅ Production Ready | **Last Updated:** August 2026
