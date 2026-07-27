# Quick Start - 30 Seconds

## Start Everything

```powershell
cd "c:\Users\cw_14\Downloads\New folder (5)"
.\devops\scripts\docker-up.ps1
```

**Wait ~65 seconds...**

## Access Services

| What | URL |
|------|-----|
| 🚀 API | http://localhost:5000/swagger |
| 📊 Grafana | http://localhost:3001 (admin/admin) |
| 📈 Prometheus | http://localhost:9090 |
| 📝 Loki | http://localhost:3100 |
| 🔍 Traces | http://localhost:3200 |
| 🐰 RabbitMQ | http://localhost:15672 (guest/guest) |

## Check Status

```powershell
.\devops\scripts\docker-status.ps1
```

## Stop Everything

```powershell
.\devops\scripts\docker-down.ps1
```

## Logs

```powershell
# All
docker-compose -f devops/docker/{1,2,3}-*.yml logs -f

# One service
docker logs -f ehr-api-gateway
```

---

## Architecture

```
Infrastructure (30s)
  ↓ PostgreSQL, MongoDB, Redis, RabbitMQ
    
Monitoring (20s)
  ↓ Prometheus, Grafana, Loki, Tempo
    
Services (15s)
  ↓ 10 Microservices (5000-5009)
    
Total: ~65 seconds
```

---

## What's Running?

- **5 PostgreSQL** (Identity, Patient, Clinical, Appointments, Audit)
- **1 MongoDB** (Prescriptions, Notifications, Analytics)
- **1 MySQL** (Billing)
- **Redis** (Cache)
- **RabbitMQ** (Messaging)
- **Kafka+Zookeeper** (Event streaming)
- **Elasticsearch** (Search)
- **Prometheus** (Metrics)
- **Grafana** (Dashboards)
- **Loki** (Logs)
- **Tempo** (Traces)
- **OTEL Collector** (Telemetry pipeline)
- **10 Microservices** (business logic)

Total: 28 containers

---

## Ports

- 5000-5009: Services
- 3001: Grafana
- 3100: Loki
- 3200: Tempo
- 9090: Prometheus
- 5432-5436: PostgreSQL
- 27017: MongoDB
- 3306: MySQL
- 6379: Redis
- 5672: RabbitMQ
- 15672: RabbitMQ Management
- 9092-9093: Kafka
- 2181: Zookeeper
- 9200: Elasticsearch
