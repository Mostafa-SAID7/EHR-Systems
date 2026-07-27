# EHR Platform Docker Stack

**Modular, Fast, Single Responsibility Architecture**

## Overview

The EHR Platform uses a **3-layer Docker Compose architecture** for clean separation and fast startup:

1. **Infrastructure** (`1-infrastructure.yml`) - Databases, cache, messaging ~30s
2. **Monitoring** (`2-monitoring.yml`) - Observability stack ~20s  
3. **Services** (`3-services.yml`) - 10 microservices ~15s

**Total startup: ~65 seconds**

### Why Modular?

- **Single Responsibility**: Each layer has ONE job
- **Independent Startup**: Can run layers separately for development
- **No Duplicates**: All configuration and volumes DRY
- **Fast Iteration**: Don't rebuild entire stack
- **Easy Scaling**: Add/remove services without touching infrastructure

---

## Quick Start

### 1. Start Everything

```powershell
# Full stack in ~65 seconds
.\devops\scripts\docker-up.ps1

# Or with waiting for health checks
.\devops\scripts\docker-up.ps1 -Wait -Timeout 120
```

### 2. Check Status

```powershell
.\devops\scripts\docker-status.ps1
```

### 3. Access Services

| Service       | URL                              | Credentials        |
|---------------|----------------------------------|--------------------|
| API Gateway   | http://localhost:5000/swagger    | -                  |
| Grafana       | http://localhost:3001            | admin/admin        |
| Prometheus    | http://localhost:9090            | -                  |
| Loki          | http://localhost:3100            | -                  |
| Tempo         | http://localhost:3200            | -                  |
| RabbitMQ      | http://localhost:15672           | ehr_user/password  |
| MongoDB       | mongodb://localhost:27017        | ehr_admin/password |
| PostgreSQL    | localhost:5432 (5 instances)     | postgres/postgres  |

---

## Layer Details

### Layer 1: Infrastructure (1-infrastructure.yml)

**Services**:
- PostgreSQL (5 instances on 5432-5436)
- MongoDB (27017)
- MySQL Billing (3306)
- Redis (6379)
- RabbitMQ (5672, 15672 management)
- Kafka + Zookeeper (9092-9093)
- Elasticsearch (9200)

**Volumes**:
- Each service has dedicated persistent volume
- Data survives container restarts

**Health Checks**:
- All services include health checks
- Docker waits for dependencies

---

### Layer 2: Monitoring (2-monitoring.yml)

**Services**:
- Prometheus (9090) - metrics storage
- Grafana (3001) - dashboards
- Loki (3100) - log aggregation
- Tempo (3200) - distributed tracing
- OTEL Collector (4317, 4318) - observability pipeline

**Configuration**:
- Preloaded dashboards: Infrastructure, API, Database, RabbitMQ, Business
- Datasources auto-configured

---

### Layer 3: Services (3-services.yml)

**10 Microservices**:

| Service       | Port | Database    | Technology |
|---------------|------|-------------|------------|
| API Gateway   | 5000 | -           | YARP proxy |
| Identity      | 5001 | PostgreSQL  | Auth/JWT   |
| Patient       | 5002 | PostgreSQL  | Core       |
| Clinical      | 5003 | PostgreSQL  | Core       |
| Appointment   | 5004 | PostgreSQL  | Core       |
| Prescription  | 5005 | MongoDB     | Core       |
| Billing       | 5006 | MySQL       | Core       |
| Audit         | 5007 | PostgreSQL  | Event log  |
| Notification  | 5008 | MongoDB     | Events     |
| Analytics     | 5009 | MongoDB+ES  | Analytics  |

**All send telemetry to OTEL Collector**

---

## Usage Commands

### Start Specific Layers

```powershell
# Infrastructure only (databases, cache, messaging)
.\devops\scripts\docker-up.ps1 -Layer infrastructure

# Monitoring stack only
.\devops\scripts\docker-up.ps1 -Layer monitoring

# Services only (requires infrastructure + monitoring running)
.\devops\scripts\docker-up.ps1 -Layer services
```

### Stop Stack

```powershell
# Stop all containers (keep volumes)
.\devops\scripts\docker-down.ps1

# Stop and remove volumes
.\devops\scripts\docker-down.ps1 -Volumes

# Stop and remove images
.\devops\scripts\docker-down.ps1 -Images

# Stop specific layer
.\devops\scripts\docker-down.ps1 -Layer services
```

### Check Status

```powershell
.\devops\scripts\docker-status.ps1
```

### Manual Docker Compose

```powershell
# Start infrastructure with docker-compose directly
docker-compose -f devops/docker/1-infrastructure.yml up -d

# View logs for a specific service
docker-compose -f devops/docker/1-infrastructure.yml logs postgres-identity -f

# Scale a service
docker-compose -f devops/docker/3-services.yml up -d --scale patient-service=2

# Stop and cleanup
docker-compose -f devops/docker/1-infrastructure.yml down -v
```

---

## Environment Variables

Edit `devops/docker/.env` to customize:

```env
# Database credentials
POSTGRES_PASSWORD=postgres
MONGO_PASSWORD=ehr_mongo_password
MYSQL_PASSWORD=billing_password
REDIS_PASSWORD=redis_password

# Message broker
RABBITMQ_USER=ehr_user
RABBITMQ_PASSWORD=ehr_password

# Auth
JWT_SECRET=change-in-production

# Monitoring
GRAFANA_PASSWORD=admin
```

---

## Troubleshooting

### Containers won't start

```powershell
# Check logs
docker logs ehr-postgres-identity
docker logs ehr-api-gateway

# Check network
docker network inspect ehr-network

# Check volumes
docker volume ls | grep ehr
```

### Port conflicts

If ports are already in use:
1. Stop conflicting services: `docker ps` → `docker stop <container>`
2. Or edit compose files to use different ports

### Database connection refused

Wait for health checks:
```powershell
# Check service status
docker ps --filter "name=ehr-"

# Wait for PostgreSQL to be ready
docker exec ehr-postgres-identity pg_isready -U postgres
```

### No data in Grafana

1. Ensure services are running: `docker-compose -f devops/docker/3-services.yml ps`
2. Check Prometheus targets: http://localhost:9090/targets
3. Check OTEL Collector logs: `docker logs ehr-otel-collector`

---

## Development Workflow

### During Development

**Option A: Only Infrastructure**
```powershell
# Run only databases + cache + messaging
.\devops\scripts\docker-up.ps1 -Layer infrastructure

# Run .NET services locally in Visual Studio
# Services connect to Docker infrastructure
```

**Option B: Full Stack**
```powershell
.\devops\scripts\docker-up.ps1

# All running in Docker, easier to test
```

### Build Images

```powershell
# Rebuild all service images
docker-compose -f devops/docker/3-services.yml build

# Rebuild one service
docker-compose -f devops/docker/3-services.yml build api-gateway

# Parallel build (faster)
docker-compose -f devops/docker/3-services.yml build --parallel
```

---

## Performance Notes

- **First run**: ~2-3 minutes (downloading images, building services)
- **Subsequent runs**: ~65 seconds (layers start in sequence)
- **Layer parallelization**: Layers start after previous layer is healthy
- **Multi-stage builds**: Lean runtime images (~200MB each service)
- **Alpine Linux**: All images use alpine for small footprint

---

## Monitoring & Observability

### View Logs

```powershell
# All containers
docker-compose -f devops/docker/{1,2,3}-*.yml logs -f

# Specific service
docker logs -f ehr-api-gateway

# Follow specific pattern
docker logs -f ehr-* 2>&1 | Select-String "error|warning"
```

### Access Grafana

1. Open http://localhost:3001
2. Login: admin/admin
3. Pre-built dashboards:
   - **Infrastructure**: CPU, RAM, containers, network
   - **API Metrics**: Requests/sec, latency, errors
   - **Database**: Connections, slow queries, deadlocks
   - **RabbitMQ**: Queues, consumers, DLQ
   - **Business**: Patients, appointments, prescriptions, billing

### OpenTelemetry Pipeline

```
Services (OTLP)
    ↓
OTEL Collector
    ↙        ↘        ↙
Prometheus  Tempo   Loki
    ↓         ↓       ↓
    └─ Grafana ─┘
```

---

## Cleanup

```powershell
# Remove all containers, volumes, networks
docker-compose -f devops/docker/1-infrastructure.yml down -v
docker-compose -f devops/docker/2-monitoring.yml down -v
docker-compose -f devops/docker/3-services.yml down -v
docker network rm ehr-network

# Or use cleanup script
.\devops\scripts\docker-down.ps1 -Volumes
```

---

## References

- [Docker Compose Docs](https://docs.docker.com/compose/)
- [Multi-stage Build Best Practices](https://docs.docker.com/build/building/multi-stage/)
- [Health Checks](https://docs.docker.com/compose/compose-file/compose-file-v3/#healthcheck)
- [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/)
