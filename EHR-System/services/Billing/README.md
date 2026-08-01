# Billing Service

Comprehensive billing and invoice management service for the EHR Platform. Handles invoice creation, payment tracking, insurance claim management, and financial reporting.

## Features

- **Invoice Management**: Create, retrieve, and manage patient invoices
- **Payment Processing**: Record and track payments with multiple payment methods
- **Insurance Claims**: Submit invoices to insurance and track claim status
- **Prior Authorization**: Request and manage insurance pre-approvals
- **Financial Reporting**: Aggregate billing data for financial analysis
- **HIPAA Compliance**: Full audit logging and encryption support
- **Multi-Currency Support**: Track different payment methods and currencies
- **Outbox Pattern**: Reliable event publishing for microservices integration

## Architecture

The service follows a clean architecture with clear separation of concerns:

```
Billing.API           → Controllers & HTTP endpoints (Port 5007)
Billing.Application   → CQRS Handlers, Commands, Queries, Mappers
Billing.Contracts     → DTOs for API contracts
Billing.Domain        → Domain entities, aggregates, value objects, events
Billing.Infrastructure → External services, event publishers
Billing.Persistence   → Entity Framework Core, repositories, DbContext
```

## Database Schema

PostgreSQL with separate `billing` schema containing:
- `Invoices`: Main invoice aggregate (INV-YYYYMMDD-XXXXXX format)
- `LineItems`: Individual service charges on invoices
- `Payments`: Payment records with method tracking
- `InsuranceClaims`: Insurance claim submissions and status
- `PriorAuthorizations`: Insurance pre-approvals

## API Endpoints

### Invoices

```bash
# Create invoice
POST /api/v1/invoices
Content-Type: application/json
{
  "patientId": "guid",
  "appointmentId": "guid|null",
  "serviceDate": "2024-01-15",
  "insuranceProvider": "Blue Shield",
  "insurancePolicyNumber": "BS123456",
  "lineItems": [
    {
      "description": "Office Visit",
      "cptCode": "99213",
      "quantity": 1,
      "unitPrice": 150.00
    }
  ]
}

# Get invoice by number
GET /api/v1/invoices/by-number/{invoiceNumber}

# Health check
GET /api/v1/invoices/health
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 12+
- Docker & Docker Compose (for containerized deployment)

### Local Development

1. **Setup Database**
   ```bash
   docker-compose -f docker/docker-compose.yml up -d postgres
   ```

2. **Apply Migrations**
   ```bash
   cd src/Billing.API
   dotnet ef database update
   ```

3. **Run Service**
   ```bash
   cd src/Billing.API
   dotnet run
   ```

4. **Access Swagger UI**
   - Navigate to http://localhost:5007/swagger

### Docker Deployment

```bash
docker-compose -f docker/docker-compose.yml up --build
```

## Configuration

### Environment Variables

```bash
# Database
DATABASE_CONNECTION_STRING=Host=postgres;Port=5432;Database=billing_service;Username=postgres;Password=postgres

# API
ASPNETCORE_ENVIRONMENT=Development
```

### appsettings.json

- `ConnectionStrings.DefaultConnection`: PostgreSQL connection string
- `Serilog`: Logging configuration
- `Jwt`: JWT authentication settings (when integrated)

## Testing

```bash
# Run unit tests
dotnet test

# Run integration tests
dotnet test --filter Category=Integration

# Generate coverage report
dotnet test /p:CollectCoverage=true
```

## Production Deployment

### Kubernetes (Helm)

```bash
helm install billing ./k8s/billing-chart \
  --set image.tag=1.0.0 \
  --set postgresql.enabled=true
```

### Environment-Specific Configuration

- `Development`: Log to console + file, auto-migrations enabled
- `Staging`: Log to Elasticsearch, manual migrations
- `Production`: Log to Elasticsearch, manual migrations, audit logging

## HIPAA Compliance

- ✅ Audit logging on all invoice operations
- ✅ PHI encryption at rest
- ✅ Audit trail for payments and claims
- ✅ Patient consent tracking
- ✅ Data retention policies
- ✅ Access control integration with Identity service

## Performance Optimization

- **Caching**: Redis for frequently accessed invoices
- **Indexing**: Composite indexes on PatientId + ServiceDate
- **Pagination**: Implemented on all list endpoints
- **Connection Pooling**: Npgsql connection pooling configured

## Contributing

Please see CONTRIBUTING.md for guidelines.

## License

Proprietary - EHR Platform
