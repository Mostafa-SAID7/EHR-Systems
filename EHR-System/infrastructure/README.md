# EHR-System Infrastructure Configuration

This folder contains all infrastructure configurations for the EHR Platform microservices architecture.

## Directory Structure

```
infrastructure/
├── Authentication/           # JWT, OAuth2, RBAC, permission configs
│   └── jwt-config.yaml      # JWT secret, token lifetime, roles/permissions
├── BlobStorage/             # S3, file upload configurations
├── Caching/                 # Redis configuration
│   └── redis-config.yaml    # Redis connection, cache policies, replication
├── Logging/                 # ELK stack, structured logging
├── Messaging/               # RabbitMQ, Kafka, message queue configs
├── Monitoring/              # Prometheus, Grafana, alerting
│   └── prometheus.yml       # Prometheus scrape configs, alerts
├── Persistence/             # Database initialization scripts
│   └── postgres-init.sql    # PostgreSQL database creation, schemas
└── README.md               # This file
```

## Quick Start

### Local Development

```bash
# Initialize PostgreSQL databases
docker-compose exec postgres psql -U ehruser -f /init-scripts/postgres-init.sql

# Start Redis cache
docker-compose up -d redis

# Verify connections
redis-cli ping
psql -U ehruser -d ehr_patient
```

### Production Deployment

```bash
# Initialize databases via Kubernetes Job
kubectl apply -f deployment/k8s/db-init-job.yaml

# Verify schema creation
kubectl exec -it postgres-pod -- psql -U ehruser -d ehr_patient -c "\dt"

# Configure Redis replication
kubectl apply -f deployment/k8s/redis-cluster.yaml
```

## Service Database Mapping

| Service | Database | Schema | Port |
|---------|----------|--------|------|
| Identity | ehr_identity | identity | 5432 |
| Patient | ehr_patient | patient | 5432 |
| Appointment | ehr_appointment | appointment | 5432 |
| Integration | ehr_integration | integration | 5432 |
| Terminology | ehr_terminology | terminology | 5432 |
| FileStorage | ehr_filestorage | filestorage | 5432 |
| AI | ehr_ai | ai | 5432 |

## Authentication & Authorization

### JWT Configuration

- **Issuer**: `ehr-platform`
- **Audience**: `ehr-api`
- **Token Lifetime**: 60 minutes
- **Refresh Token**: 7 days
- **Claims**: sub, iat, exp, iss, aud, roles, permissions, scope

### Roles

- `admin` - Full system access
- `clinician` - Clinical records + patient management
- `nurse` - Read-only clinical data
- `patient` - Own records only
- `pharmacist` - Prescription management
- `billing_officer` - Billing and invoicing
- `auditor` - Audit trail access
- `system` - Service-to-service auth

### Setup

```bash
# Set JWT secret
export JWT_SECRET="your-secret-key-min-32-chars"

# In Kubernetes
kubectl create secret generic jwt-secret --from-literal=secret="$JWT_SECRET" -n ehr-platform
```

## Caching Strategy

### Redis Usage

1. **Session Cache**: Authenticated user sessions (TTL: 60 min)
2. **Patient Cache**: Patient demographics (TTL: 30 min)
3. **Appointment Cache**: Appointment schedules (TTL: 15 min)
4. **Terminology Cache**: Medical codes/mappings (TTL: 24 hours)
5. **Clinical Cache**: Clinical records (TTL: 60 min)
6. **Permission Cache**: User permissions (TTL: 30 min)

### Eviction Policy

- **Policy**: Least Recently Used (LRU)
- **Max Memory**: 80% before eviction
- **Sampling**: 5 keys

## Monitoring

### Prometheus Scrapes

- All 9 microservices `/metrics` endpoint
- PostgreSQL metrics
- Redis metrics
- RabbitMQ metrics

### Grafana Dashboards

Preconfigured dashboards:
- Services Overview (9 services health)
- Database Performance (PostgreSQL metrics)
- Cache Efficiency (Redis hit rate)
- API Gateway Metrics (requests, latency)
- RabbitMQ Queue Depth

### Alert Rules

Located in `EHR-System/deployment/helm/templates/alerts.yml`

Key alerts:
- Service down (5 min)
- High error rate (>5%)
- High latency (>1 second)
- Database connection pool exhausted
- Redis memory > 80%
- Queue depth > 10000 messages

## Database Initialization

### PostgreSQL Setup

1. **User Creation**: `ehruser` with appropriate permissions
2. **Database Creation**: 7 databases (one per service)
3. **Schema Creation**: Per-service schema for isolation
4. **Permissions**: Least privilege per service

```sql
-- View created databases
SELECT datname FROM pg_database WHERE datname LIKE 'ehr_%' ORDER BY datname;

-- View schemas
\dn

-- Verify permissions
\dp
```

### Schema Updates

Use EF Core migrations (see `MIGRATION_STRATEGY.md`):

```bash
# Per service
dotnet ef database update --project services/Patient/src/Patient.Persistence
```

## Troubleshooting

### Redis Connection Issues

```bash
# Test connection
redis-cli -h redis -p 6379 ping

# Check memory
redis-cli -h redis -p 6379 INFO memory

# Monitor commands
redis-cli -h redis -p 6379 MONITOR
```

### PostgreSQL Connection Issues

```bash
# Test connection
psql -h postgres -U ehruser -d ehr_patient -c "SELECT version();"

# Check active connections
SELECT datname, count(*) FROM pg_stat_activity GROUP BY datname;

# View slow queries
SELECT query, mean_exec_time FROM pg_stat_statements ORDER BY mean_exec_time DESC;
```

### JWT Token Issues

```bash
# Decode JWT (use online tool or jq)
echo $TOKEN | cut -d. -f2 | base64 -d | jq .

# Check token expiration
jq .exp <<< "$TOKEN" | date -f - +%Y-%m-%d\ %H:%M:%S
```

## Configuration Management

### Environment Variables

**Required**:
- `JWT_SECRET` - JWT signing key (min 32 characters)
- `REDIS_PASSWORD` - Redis password
- `DB_MASTER_USERNAME` - Database user
- `DB_MASTER_PASSWORD` - Database password

**Optional**:
- `REDIS_HOST` - Redis hostname (default: redis)
- `REDIS_PORT` - Redis port (default: 6379)
- `DB_HOST` - Database hostname (default: postgres)
- `DB_PORT` - Database port (default: 5432)

### Kubernetes Secrets

```bash
# Create secrets
kubectl create secret generic ehr-secrets \
  --from-literal=jwt-secret="$JWT_SECRET" \
  --from-literal=redis-password="$REDIS_PASSWORD" \
  --from-literal=db-password="$DB_MASTER_PASSWORD" \
  -n ehr-platform

# Use in deployments
valueFrom:
  secretKeyRef:
    name: ehr-secrets
    key: jwt-secret
```

## Security Best Practices

1. **Secrets Management**
   - Never commit secrets to git
   - Use Kubernetes Secrets or AWS Secrets Manager
   - Rotate credentials monthly

2. **Database Access**
   - Schema-per-service isolation
   - Least privilege per user
   - No cross-service direct DB access

3. **Cache Security**
   - Redis requires password authentication
   - No sensitive data in cache keys
   - TTL for session data

4. **JWT Security**
   - Rotate signing key annually
   - Use HTTPS for all API calls
   - Validate token expiration

5. **Network Security**
   - Services communicate via Kubernetes DNS (service.namespace.svc.cluster.local)
   - NetworkPolicies restrict traffic
   - Mutual TLS (mTLS) for service mesh

## References

- Prometheus Configuration: [https://prometheus.io/docs/prometheus/latest/configuration/configuration/](https://prometheus.io/docs/prometheus/latest/configuration/configuration/)
- Redis Configuration: [https://redis.io/topics/config](https://redis.io/topics/config)
- PostgreSQL Documentation: [https://www.postgresql.org/docs/](https://www.postgresql.org/docs/)
- JWT.io: [https://jwt.io](https://jwt.io)

## Support

For infrastructure issues, contact: ehr-platform-team@example.com
