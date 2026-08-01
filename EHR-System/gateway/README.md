# API Gateway

Single entry point for all EHR microservices.

## Quick Start

```bash
docker-compose up -d
curl http://localhost:5000/health
```

## Services Routed

| Service | Port | Route |
|---------|------|-------|
| Identity | 5003 | /api/v1/auth/* |
| Patient | 5004 | /api/v1/patients/* |
| Appointment | 5006 | /api/v1/appointments/* |
| Clinical | 5001 | /api/v1/clinical/* |
| Billing | 5002 | /api/v1/billing/* |
| Notification | 5007 | /api/v1/notifications/* |
| Analytics | 5008 | /api/v1/analytics/* |

## Features

- YARP reverse proxy
- JWT authentication
- Rate limiting
- Health monitoring
- Request/response transformation

## Architecture

See [ARCHITECTURE.md](./ARCHITECTURE.md)

## Building Blocks

- [Contracts](../building-blocks/Contracts/README.md)
- [Security](../building-blocks/Security/README.md)
- [Observability](../building-blocks/Observability/README.md)
- [EventBus](../building-blocks/EventBus/README.md)
