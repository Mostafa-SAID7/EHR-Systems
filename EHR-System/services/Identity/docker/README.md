# Docker Configuration for Identity Service

This directory contains Docker-specific configurations for the Identity service.

## Files

### docker-compose.yml
Development environment with full debugging capabilities and extended logging.

**Usage:**
```bash
docker-compose -f docker/docker-compose.yml up -d
docker-compose -f docker/docker-compose.yml down
```

### docker-compose.prod.yml
Production-optimized environment with security and resource constraints.

**Usage:**
```bash
docker-compose -f docker/docker-compose.prod.yml up -d
```

**Required Environment Variables:**
- `DB_PASSWORD`: SQL Server SA password
- `REDIS_PASSWORD`: Redis password
- `RABBITMQ_USER`: RabbitMQ username
- `RABBITMQ_PASSWORD`: RabbitMQ password
- `JWT_SECRET`: JWT signing secret
- `JWT_ISSUER`: JWT issuer
- `JWT_AUDIENCE`: JWT audience
- `JWT_EXPIRATION_MINUTES`: Token expiration in minutes
- `JWT_REFRESH_EXPIRATION_DAYS`: Refresh token expiration in days
- `ENCRYPTION_KEY`: Data encryption key

### Dockerfile & .dockerignore
See root directory (../Dockerfile, ../.dockerignore) - single source of truth for build configuration.

## Services

### identity-db
SQL Server 2022 database instance.
- Port: 1433
- Default User: sa
- Database: EHRIdentity

### redis
Redis cache for token storage and caching.
- Port: 6379
- Development: No password
- Production: Password protected

### rabbitmq
RabbitMQ message broker for event publishing.
- AMQP Port: 5672
- Management UI: 15672

### identity-api
The Identity service API.
- Port: 5001
- Health Check: /health
- Ready Check: /ready

## Development Commands

```bash
# Start services
docker-compose -f docker/docker-compose.yml up -d

# View logs
docker-compose -f docker/docker-compose.yml logs -f identity-api

# Stop services
docker-compose -f docker/docker-compose.yml down

# Remove volumes
docker-compose -f docker/docker-compose.yml down -v

# Rebuild image
docker-compose -f docker/docker-compose.yml build --no-cache
```

## Health Checks

All services include health checks. View status:
```bash
docker ps --format "table {{.Names}}\t{{.Status}}"
```

## Network

Services communicate via the `ehr-network` bridge network for service discovery by hostname.
