# FileStorage Service

Cloud-native document storage and management service for the EHR Platform. Handles secure document uploads, virus scanning, encryption, and retention policies with full HIPAA compliance.

## Features

- **Secure Document Storage**: AWS S3 integration with encryption and versioning
- **Virus Scanning**: ClamAV integration for malware detection
- **Document Encryption**: AES-256 encryption at rest and in transit
- **Retention Policies**: Automatic document lifecycle management
- **Audit Logging**: Full HIPAA-compliant access logging
- **Document Versioning**: Track document history and recover previous versions
- **Classification**: Support for PHI, Public, and Confidential classifications
- **Categories**: Organize documents (LabResult, Prescription, Imaging, etc.)
- **Access Control**: Integration with Identity service for fine-grained permissions

## Architecture

Clean architecture with clear separation of concerns:

```
FileStorage.API           → Controllers & HTTP endpoints (Port 5008)
FileStorage.Application   → CQRS Handlers, Commands, Queries, Mappers
FileStorage.Contracts     → DTOs for API contracts
FileStorage.Domain        → Domain entities, aggregates, value objects, events
FileStorage.Infrastructure → S3, ClamAV, encryption services
FileStorage.Persistence   → Entity Framework Core, repositories, DbContext
```

## Database Schema

PostgreSQL with separate `filestorage` schema containing:
- `StoredDocuments`: Document metadata and S3 references
- `DocumentVersions`: Version history for recovery
- `VirusScanResults`: Scan results and threat tracking
- `DocumentAccesses`: HIPAA audit log of all document access

## S3 Storage Structure

```
s3://ehr-documents/
├── patients/{patientId}/
│   ├── documents/{documentId}
│   ├── documents/{documentId}.v1
│   ├── documents/{documentId}.v2
```

All files encrypted with AES-256 at rest. Versioning enabled for recovery.

## API Endpoints

### Documents

```bash
# Get document by ID
GET /api/v1/documents/{documentId}

# Health check
GET /api/v1/documents/health
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 12+
- AWS S3 bucket
- Docker & Docker Compose (optional)

### Local Development

1. **Setup Database**
   ```bash
   docker-compose -f docker/docker-compose.yml up -d postgres
   ```

2. **Configure AWS**
   ```bash
   aws configure
   ```

3. **Apply Migrations**
   ```bash
   cd src/FileStorage.API
   dotnet ef database update
   ```

4. **Run Service**
   ```bash
   cd src/FileStorage.API
   dotnet run
   ```

5. **Access Swagger UI**
   - Navigate to http://localhost:5008/swagger

### Docker Deployment

```bash
docker-compose -f docker/docker-compose.yml up --build
```

## Configuration

### Environment Variables

```bash
# Database
DATABASE_CONNECTION_STRING=Host=postgres;Port=5432;Database=filestorage_service;Username=postgres;Password=postgres

# AWS S3
AWS_S3_BUCKET=ehr-documents
AWS_REGION=us-east-1
AWS_ACCESS_KEY_ID=your-key
AWS_SECRET_ACCESS_KEY=your-secret

# API
ASPNETCORE_ENVIRONMENT=Development
```

## Security

- **Encryption**: AES-256 encryption for all documents in S3
- **Virus Scanning**: ClamAV integration for threat detection
- **Audit Logging**: Every document access logged for compliance
- **Access Control**: Role-based access integrated with Identity service
- **Data Retention**: Configurable retention policies with automatic purging

## Performance

- **Lazy Loading**: Documents loaded on-demand
- **Indexing**: Optimized indexes on PatientId, CreatedAt, Status
- **Caching**: Redis caching for frequently accessed metadata
- **Pagination**: All list endpoints paginated

## HIPAA Compliance

- ✅ Encryption at rest (AES-256)
- ✅ Encryption in transit (TLS)
- ✅ Full audit logging
- ✅ Access control integration
- ✅ Document retention policies
- ✅ Automatic purging of expired documents
- ✅ Integrity verification (SHA-256 hashing)

## Virus Scanning

Documents are automatically scanned using ClamAV:

1. Document uploaded
2. ClamAV scan initiated asynchronously
3. Result recorded in database
4. If threat detected, document marked as Quarantined
5. Alert sent to security team

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

### Kubernetes

```bash
helm install filestorage ./k8s/filestorage-chart \
  --set image.tag=1.0.0 \
  --set aws.s3.bucket=ehr-documents \
  --set postgresql.enabled=true
```

## Contributing

Please see CONTRIBUTING.md for guidelines.

## License

Proprietary - EHR Platform
