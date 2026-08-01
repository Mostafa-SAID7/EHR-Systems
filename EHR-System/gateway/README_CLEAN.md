# API Gateway

Single entry point for all EHR microservices.

## What It Does

- **Routing** - Direct requests to correct microservice
- **Authentication** - Validate JWT tokens
- **Rate Limiting** - Protect backend services
- **Request Transformation** - Adapt external→internal contracts
- **Response Aggregation** - Combine data from multiple services
- **Health Checks** - Monitor all backend services
- **API Versioning** - Support multiple API versions

## Quick Start

```bash
# Development (Docker)
docker-compose up -d

# Verify gateway
curl http://localhost:5000/health

# Login
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password"}'

# Use token in requests
curl -H "Authorization: Bearer <token>" \
  http://localhost:5000/api/v1/patients
```

## Services Routed

| Service | Port | Route |
|---------|------|-------|
| Identity | 5003 | `/api/v1/auth/*` |
| Patient | 5004 | `/api/v1/patients/*` |
| Appointment | 5006 | `/api/v1/appointments/*` |
| Clinical | 5001 | `/api/v1/clinical/*` |
| Billing | 5002 | `/api/v1/billing/*` |
| Notification | 5007 | `/api/v1/notifications/*` |
| Analytics | 5008 | `/api/v1/analytics/*` |

## Architecture

See [ARCHITECTURE.md](./ARCHITECTURE.md) for detailed design.

## Building Blocks Used

- [building-blocks/Contracts](../building-blocks/Contracts/README.md) - Request/Response contracts
- [building-blocks/Security](../building-blocks/Security/README.md) - Authentication & tenant context
- [building-blocks/Observability](../building-blocks/Observability/README.md) - Health checks & logging
- [building-blocks/EventBus](../building-blocks/EventBus/README.md) - Event publishing

## Project Structure

```
src/APIGateway/
├── Controllers/          - API endpoints
├── Infrastructure/       - Middleware, routing, services
├── Services/            - Business logic
├── Middleware/          - Request/response processing
├── Routing/             - Route configuration
├── Program.cs           - Setup & configuration
└── appsettings.json     - Configuration
```

## Configuration

Gateway behavior controlled via `appsettings.json`:

```json
{
  "Gateway": {
    "Port": 5000,
    "RateLimitPerMinute": 100,
    "RequestTimeout": 30000
  },
  "Services": {
    "Patient": "http://localhost:5004",
    "Identity": "http://localhost:5003"
  }
}
```

## Related Links

- [← EHR-System](../README.md)
- [Building Blocks](../building-blocks/README.md)
