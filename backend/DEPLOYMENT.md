# EHR Platform - Deployment Guide

## Quick Start (Development)

### Prerequisites
- Docker Desktop (Windows/Mac) or Docker + Docker Compose (Linux)
- .NET 8 SDK
- Node.js 18+ (for build tools, optional)
- Git

### One-Command Deployment

```bash
cd backend
docker-compose up -d
```

**Expected output:**
```
Creating ehr-postgres ... done
Creating ehr-mysql ... done
Creating ehr-mongodb ... done
Creating ehr-kafka ... done
Creating ehr-zookeeper ... done
Creating ehr-redis ... done
Creating ehr-elasticsearch ... done
Creating ehr-prometheus ... done
Creating ehr-grafana ... done
Creating ehr-identity-service ... done
Creating ehr-patient-service ... done
... (all 10 services)
```

**Wait 30-60 seconds for initialization.**

### Verify Deployment

```bash
# Check all services are running
docker-compose ps

# Expected: All services should show "Up"
```

### Access Services

```yaml
API Gateway: http://localhost:5000/swagger
Identity Service: http://localhost:5001/swagger
Patient Service: http://localhost:5002/swagger
Grafana Dashboards: http://localhost:3000
Prometheus Metrics: http://localhost:9090
Kafka UI: http://localhost:8080 (if installed)
```

**Default Credentials:**
- Grafana: admin / admin
- Prometheus: No auth required

---

## Detailed Deployment Steps

### Step 1: Initialize Databases

The init scripts run automatically in docker-compose.yml.

**Manual verification:**

```bash
# Verify PostgreSQL databases
docker exec ehr-postgres psql -U ehr_user -c "\l" | grep ehr_

# Expected output:
# ehr_identity_db
# ehr_patient_db
# ehr_clinical_db
# ehr_appointment_db
# ehr_notification_db
# ehr_audit_db
# ehr_billing_db
# ehr_prescription_db
# ehr_analytics_db
# ehr_outbox_db

# Verify MySQL databases
docker exec ehr-mysql mysql -u ehr_user -p -e "SHOW DATABASES LIKE 'ehr_%';"

# Verify MongoDB databases
docker exec ehr-mongodb mongosh -u root -p --eval "db.adminCommand('listDatabases')" | grep ehr_
```

### Step 2: Apply Migrations

Migrations run automatically when services start.

**Manual migration (if needed):**

```bash
# Inside Patient Service container
docker exec ehr-patient-service dotnet ef database update \
  --context PatientContext \
  --project src/EHRPlatform.Services.Patient

# Expected: "Done. Executed X migrations."
```

### Step 3: Verify Kafka Setup

```bash
# List Kafka topics
docker exec ehr-kafka kafka-topics \
  --list \
  --bootstrap-server localhost:9092

# Create topics if not auto-created
docker exec ehr-kafka kafka-topics \
  --create \
  --bootstrap-server localhost:9092 \
  --topic patient-events \
  --partitions 3 \
  --replication-factor 1
```

### Step 4: Test API Endpoints

```bash
# Test Identity Service - Create User
curl -X POST http://localhost:5001/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@hospital.com",
    "firstName": "John",
    "lastName": "Doe",
    "password": "SecurePassword123!"
  }'

# Expected response:
# {
#   "id": "12345-abcde...",
#   "email": "user@hospital.com",
#   "createdAt": "2025-01-15T10:30:00Z"
# }

# Test Patient Service - Create Patient
curl -X POST http://localhost:5002/api/patients \
  -H "Content-Type: application/json" \
  -d '{
    "mrn": "MRN001",
    "firstName": "Jane",
    "lastName": "Smith",
    "dateOfBirth": "1990-05-15",
    "gender": "F"
  }'

# Expected response:
# {
#   "id": "67890-fghij...",
#   "mrn": "MRN001",
#   "status": "Active"
# }
```

### Step 5: Verify Event Processing

```bash
# Check Notification Service received UserCreatedEvent
docker logs ehr-notification-service | grep -i "UserCreatedEvent"

# Expected: "Consuming UserCreatedEvent: UserId=..., Email=user@hospital.com"

# Check Audit Service logged the event
docker logs ehr-audit-service | grep -i "UserCreated"

# Expected: "Audit logged: UserCreated for 12345-abcde..."
```

### Step 6: Monitor Health

```bash
# Check Prometheus for metrics
curl http://localhost:9090/api/v1/query?query=up

# Check service health endpoints
curl http://localhost:5000/health
curl http://localhost:5001/health
curl http://localhost:5002/health
```

---

## Environment Configuration

### Default .env File

```env
# PostgreSQL
POSTGRES_USER=ehr_user
POSTGRES_PASSWORD=ehr_secure_password
POSTGRES_DB=ehr_identity_db

# MySQL
MYSQL_ROOT_PASSWORD=root_secure_password
MYSQL_USER=ehr_user
MYSQL_PASSWORD=ehr_secure_password
MYSQL_DATABASE=ehr_identity_db

# MongoDB
MONGO_INITDB_ROOT_USERNAME=root
MONGO_INITDB_ROOT_PASSWORD=mongo_secure_password

# Kafka
KAFKA_BROKER_ID=1
KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181

# Redis
REDIS_PASSWORD=redis_secure_password

# Services
IDENTITY_SERVICE_PORT=5001
PATIENT_SERVICE_PORT=5002
CLINICAL_SERVICE_PORT=5003
APPOINTMENT_SERVICE_PORT=5004
NOTIFICATION_SERVICE_PORT=5005
AUDIT_SERVICE_PORT=5006
BILLING_SERVICE_PORT=5007
PRESCRIPTION_SERVICE_PORT=5008
OUTBOX_PROCESSOR_PORT=5009
ANALYTICS_SERVICE_PORT=5010

# Environment
ASPNETCORE_ENVIRONMENT=Development
LOG_LEVEL=Information
```

### Production .env Overrides

```env
# Production: Strong passwords required
POSTGRES_PASSWORD=${PROD_POSTGRES_PASSWORD}
MYSQL_PASSWORD=${PROD_MYSQL_PASSWORD}
MONGO_INITDB_ROOT_PASSWORD=${PROD_MONGO_PASSWORD}

# Production: Reduce logging (security)
LOG_LEVEL=Warning

# Production: Enable HTTPS
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_HTTPS_PORT=5443

# Production: Database backups
BACKUP_ENABLED=true
BACKUP_INTERVAL=daily
BACKUP_RETENTION_DAYS=30
```

---

## Troubleshooting

### Problem: Services won't start

```bash
# Check logs
docker-compose logs identity-service

# Common issues:
# 1. Port already in use
#    Solution: Change port in docker-compose.yml
#
# 2. Database connection timeout
#    Solution: Wait 60 seconds for DB initialization
#
# 3. Out of memory
#    Solution: Increase Docker memory limit
```

### Problem: Database initialization fails

```bash
# Remove existing volumes (WARNING: Deletes data)
docker-compose down -v

# Restart
docker-compose up -d

# Wait 60 seconds for DB init scripts to run
sleep 60

# Verify
docker-compose ps
```

### Problem: Kafka topics not created

```bash
# Manually create topics
docker exec ehr-kafka bash -c '
  kafka-topics --create --bootstrap-server localhost:9092 \
    --topic patient-events --partitions 3 --replication-factor 1
  kafka-topics --create --bootstrap-server localhost:9092 \
    --topic user-events --partitions 3 --replication-factor 1
  kafka-topics --create --bootstrap-server localhost:9092 \
    --topic appointment-events --partitions 3 --replication-factor 1
'
```

### Problem: Consumer lag is high

```bash
# Check consumer lag
docker exec ehr-kafka kafka-consumer-groups \
  --bootstrap-server localhost:9092 \
  --group patient-service \
  --describe

# If LAG is high:
# 1. Increase service memory
# 2. Increase Kafka partitions
# 3. Check for processing errors in logs
```

### Problem: API returns 503 Service Unavailable

```bash
# Check if all dependencies are up
docker-compose ps

# If service shows "Down":
docker-compose restart <service-name>

# Check service logs
docker logs ehr-<service-name>

# Look for:
# - Database connection errors
# - Kafka connection errors
# - Migration failures
```

---

## Scaling

### Horizontal Scaling (Add more instances)

```bash
# Scale Patient Service to 3 instances
docker-compose up -d --scale patient-service=3

# Verify
docker-compose ps | grep patient-service

# Load balancer (api-gateway) distributes traffic
```

### Vertical Scaling (Increase resources per instance)

Edit `docker-compose.yml`:

```yaml
patient-service:
  # Increase memory
  deploy:
    resources:
      limits:
        memory: 1G  # Was 512M
      reservations:
        memory: 512M  # Was 256M
```

Restart:
```bash
docker-compose up -d
```

---

## Database Backups

### Automated Backups

```bash
# Add to crontab (daily at 2 AM)
0 2 * * * /path/to/backup-script.sh

# Backup script:
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/backups/ehr/$DATE"
mkdir -p $BACKUP_DIR

# PostgreSQL backup
docker exec ehr-postgres pg_dump -U ehr_user -d ehr_identity_db \
  > $BACKUP_DIR/ehr_identity_db.sql

# MySQL backup
docker exec ehr-mysql mysqldump -u ehr_user -p ehr_appointment_db \
  > $BACKUP_DIR/ehr_appointment_db.sql

# MongoDB backup
docker exec ehr-mongodb mongodump --uri="mongodb://root:password@localhost:27017" \
  --out $BACKUP_DIR/mongo

echo "Backup completed: $BACKUP_DIR"
```

### Restore from Backup

```bash
# Restore PostgreSQL
docker exec ehr-postgres psql -U ehr_user -d ehr_patient_db \
  < /backups/ehr/20250115_020000/ehr_patient_db.sql

# Restore MySQL
docker exec ehr-mysql mysql -u ehr_user -p ehr_billing_db \
  < /backups/ehr/20250115_020000/ehr_billing_db.sql

# Restore MongoDB
docker exec ehr-mongodb mongorestore --uri="mongodb://root:password@localhost:27017" \
  /backups/ehr/20250115_020000/mongo
```

---

## Production Deployment (Kubernetes)

### Prerequisites
- Kubernetes cluster (AWS EKS, Azure AKS, or on-premises)
- kubectl configured
- Helm 3.x
- Docker registry credentials

### Deploy with Helm

```bash
# Install EHR Platform Helm chart
helm install ehr-platform ./k8s/ehr-platform \
  --namespace ehr \
  --values k8s/ehr-platform/values-prod.yaml

# Verify deployment
kubectl get pods -n ehr
kubectl get svc -n ehr

# Expected: 10 service pods running
```

### Kubernetes Structure

```
k8s/ehr-platform/
├── Chart.yaml                 # Helm chart metadata
├── values.yaml               # Default values
├── values-dev.yaml           # Development overrides
├── values-prod.yaml          # Production overrides
└── templates/
    ├── namespace-rbac.yaml   # Namespace + RBAC
    ├── secrets-config.yaml   # Secrets + ConfigMaps
    ├── deployment-*.yaml     # Service deployments
    ├── statefulset-*.yaml    # Database StatefulSets
    ├── service-*.yaml        # Kubernetes services
    └── ingress-*.yaml        # Ingress routing
```

### Example: Deploy Identity Service

```yaml
# k8s/ehr-platform/templates/deployment-identity-service.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: identity-service
  namespace: ehr
spec:
  replicas: 3
  selector:
    matchLabels:
      app: identity-service
  template:
    metadata:
      labels:
        app: identity-service
    spec:
      containers:
      - name: identity-service
        image: myregistry.azurecr.io/ehr-identity-service:latest
        ports:
        - containerPort: 5001
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: identity-connection-string
        livenessProbe:
          httpGet:
            path: /health
            port: 5001
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 5001
          initialDelaySeconds: 5
          periodSeconds: 5
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

---

## Monitoring & Alerting

### Prometheus Alerts

```yaml
# monitoring/prometheus-rules/alerts.yml
groups:
- name: ehr-platform
  rules:
  - alert: HighErrorRate
    expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.05
    for: 5m
    annotations:
      summary: "High error rate detected ({{ $value }}%)"

  - alert: ServiceDown
    expr: up{job="ehr-services"} == 0
    for: 1m
    annotations:
      summary: "Service {{ $labels.instance }} is down"

  - alert: HighMemoryUsage
    expr: container_memory_usage_bytes / container_spec_memory_limit_bytes > 0.9
    for: 5m
    annotations:
      summary: "High memory usage on {{ $labels.container }}"

  - alert: DatabaseConnectionPoolExhausted
    expr: db_connection_pool_available == 0
    for: 1m
    annotations:
      summary: "Database connection pool exhausted"

  - alert: KafkaConsumerLagHigh
    expr: kafka_consumer_lag > 10000
    for: 10m
    annotations:
      summary: "Kafka consumer lag is high: {{ $value }}"
```

### Grafana Dashboards

Pre-built dashboards available at:
```
monitoring/grafana-provisioning/dashboards/
├── ehr-overview.json       # System overview
├── ehr-services.json       # Per-service metrics
└── ehr-infrastructure.json # Database + Kafka metrics
```

**Access:** http://localhost:3000 (admin / admin)

---

## Security Hardening

### Network Policies

```yaml
# k8s/ehr-platform/templates/network-policy.yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: ehr-network-policy
  namespace: ehr
spec:
  podSelector: {}
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: ehr
    ports:
    - protocol: TCP
      port: 5000  # API Gateway
  egress:
  - to:
    - namespaceSelector: {}
    ports:
    - protocol: TCP
      port: 5432  # PostgreSQL
    - protocol: TCP
      port: 9092  # Kafka
```

### Secrets Management

```bash
# Create secrets in Kubernetes
kubectl create secret generic db-credentials \
  --from-literal=identity-connection-string='...' \
  --from-literal=patient-connection-string='...' \
  -n ehr

# Or use Sealed Secrets (recommended)
helm repo add sealed-secrets https://bitnami-labs.github.io/sealed-secrets
helm install sealed-secrets -n kube-system sealed-secrets/sealed-secrets
```

### TLS/HTTPS

```yaml
# Ingress with TLS
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: ehr-ingress
  namespace: ehr
spec:
  tls:
  - hosts:
    - api.ehr.hospital.com
    secretName: ehr-tls-cert
  rules:
  - host: api.ehr.hospital.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: api-gateway
            port:
              number: 5000
```

---

## Health Checks

### Readiness Probe

Service is ready to receive traffic when:
- Database is connected and responsive
- Kafka is reachable
- Dependencies (Redis, Elasticsearch) are accessible

```csharp
// Program.cs
app.MapGet("/ready", async (IHostApplicationLifetime lifetime) =>
{
    try
    {
        // Check database
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PatientContext>();
            await context.Database.ExecuteSqlAsync("SELECT 1");
        }
        return Results.Ok("Ready");
    }
    catch
    {
        return Results.StatusCode(503);
    }
});
```

### Liveness Probe

Service is alive and should not be restarted:

```csharp
app.MapGet("/health", () => Results.Ok("Healthy"));
```

---

## Rollback Strategy

### Automatic Rollback on Failure

```bash
# Kubernetes automatically rolls back if readiness probe fails
kubectl set image deployment/patient-service \
  patient-service=myregistry/ehr-patient-service:new-version \
  -n ehr --record

# If new version is unhealthy:
kubectl rollout undo deployment/patient-service -n ehr

# View rollout history
kubectl rollout history deployment/patient-service -n ehr
```

---

## Cleanup

### Stop All Services (Preserve data)

```bash
docker-compose stop
```

### Remove All Services (Delete data)

```bash
docker-compose down -v
```

### Full Cleanup

```bash
docker-compose down -v
docker system prune -a
```

---

## Deployment Checklist

- [ ] All services running (`docker-compose ps`)
- [ ] All databases initialized
- [ ] Kafka topics created
- [ ] Migrations applied
- [ ] Health checks passing
- [ ] API endpoints responding
- [ ] Events being published to Kafka
- [ ] Consumers processing events
- [ ] Metrics visible in Prometheus
- [ ] Dashboards configured in Grafana
- [ ] Backup strategy in place
- [ ] Monitoring alerts configured
- [ ] Secrets secured (not in version control)
- [ ] Network policies enforced (Kubernetes)
- [ ] TLS certificates installed (production)

---

## Summary

✅ **Development:** `docker-compose up -d` (30 seconds)
✅ **Testing:** Verify with curl/Postman
✅ **Staging:** Deploy to Kubernetes cluster
✅ **Production:** Use values-prod.yaml with hardened security

**Next:** See [MONITORING.md](./MONITORING.md) for detailed observability setup.

