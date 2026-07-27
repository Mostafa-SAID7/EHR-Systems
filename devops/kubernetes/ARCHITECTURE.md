# Kubernetes Architecture - EHR Platform

## Overview

**Multi-layer, kustomize-based, production-ready Kubernetes deployment**

- **Base**: Common configuration (namespaces, secrets, storage, policies)
- **Overlays**: Environment-specific (production, development, staging)
- **No duplicates**: DRY principle - single source of truth

## Directory Structure

```
kubernetes/
├── base/                          # Shared base configuration
│   ├── 00-namespace.yaml         # Namespace definition
│   ├── 01-configmaps.yaml        # ConfigMaps (non-sensitive config)
│   ├── 02-secrets.yaml           # Secrets (sensitive data - rotate in prod!)
│   ├── 03-storage.yaml           # Storage classes, PVCs
│   ├── 04-policies.yaml          # RBAC, network policies, PSP
│   └── kustomization.yaml        # Base kustomization
│
├── overlays/
│   ├── production/               # Production environment
│   │   └── kustomization.yaml   # Prod-specific patches, replicas, resources
│   └── development/              # Development environment
│       └── kustomization.yaml   # Dev-specific patches, lighter resources
│
├── ARCHITECTURE.md              # This file
├── DEPLOYMENT.md                # Deployment guide
├── TROUBLESHOOTING.md           # Troubleshooting guide
└── QUICK-REFERENCE.md           # Quick commands
```

## Base Configuration

### 00-namespace.yaml
- Creates `ehr-platform` namespace
- Isolates resources from other applications
- Enables RBAC per namespace

### 01-configmaps.yaml
- **ehr-config**: Service environment variables
  - ASPNETCORE_ENVIRONMENT, OTEL endpoints, database hosts
  - RabbitMQ, Redis, MongoDB, PostgreSQL, MySQL, Elasticsearch
  - Kafka brokers
- **prometheus-config**: Prometheus scrape targets
- **loki-config**: Loki log aggregation
- **tempo-config**: Tempo tracing backend

### 02-secrets.yaml
- **ehr-database-credentials**: DB passwords
- **ehr-broker-credentials**: RabbitMQ, Redis passwords
- **ehr-auth-secrets**: JWT secret
- **ehr-monitoring-credentials**: Grafana, RabbitMQ admin
- **ehr-registry-credentials**: Docker registry auth

**⚠️ SECURITY**: Never commit actual secrets! Use:
- External Secrets Operator
- Sealed Secrets
- HashiCorp Vault
- AWS Secrets Manager / Azure Key Vault

### 03-storage.yaml
**Three Storage Classes**:
1. **ehr-fast-storage** (gp3 SSD)
   - Databases: PostgreSQL (5×20Gi), MongoDB (50Gi), MySQL (20Gi)
   - Elasticsearch (50Gi)
   - iops: 3000, throughput: 125
   
2. **ehr-standard-storage** (gp3 general)
   - Redis (10Gi), RabbitMQ (20Gi)
   - Prometheus (30Gi), Grafana (5Gi)
   - iops: 1000, throughput: 125
   
3. **ehr-archive-storage** (sc1 HDD)
   - Loki (100Gi), Tempo (100Gi)
   - Cost-optimized for logs and traces

### 04-policies.yaml
- **ResourceQuota**: Prevent namespace from consuming too much
  - CPU: 100 req / 200 limits
  - Memory: 200Gi req / 400Gi limits
  - Pods, PVCs, Services, Ingresses limits
  
- **NetworkPolicy**: Zero-trust networking
  - Default: Deny all ingress/egress
  - Allow: Intra-namespace communication
  - Allow: DNS (port 53)
  
- **PodDisruptionBudget**: Ensure high availability
  - API Gateway: min 2 available
  - Services: min 1 available
  
- **PodSecurityPolicy**: Restrict pod security
  - No privileged containers
  - No root execution
  - Read-only root filesystem recommended

## Overlays

### Production Overlay

**Resource Configuration**:
```yaml
- api-gateway: 3 replicas
- identity-service: 2 replicas
- patient-service: 2 replicas
- clinical-service: 2 replicas
- appointment-service: 2 replicas
```

**Resource Limits**:
```yaml
- CPU: 500m request / 1000m limit
- Memory: 256Mi request / 512Mi limit
```

**Image Tags**: Semantic versioning (1.0.0, 1.0.1, etc.)

**High Availability**:
- Multiple replicas
- Pod anti-affinity (spread across nodes)
- Health checks (readiness, liveness)

### Development Overlay

**Resource Configuration**:
```yaml
- All services: 1 replica
```

**Resource Limits**:
```yaml
- CPU: 100m request / 500m limit
- Memory: 128Mi request / 256Mi limit
```

**Image Tags**: Latest (dev builds)

**Storage**: Smaller volumes (5Gi instead of 20-100Gi)

---

## Deployment Commands

### View Final Manifests (no apply)

```bash
# Production
kustomize build overlays/production

# Development
kustomize build overlays/development
```

### Apply to Cluster

```bash
# Production (apply with kustomize)
kustomize build overlays/production | kubectl apply -f -

# Or use kubectl directly (requires kubectl 1.14+)
kubectl apply -k overlays/production

# Development
kubectl apply -k overlays/development
```

### Check Status

```bash
# Watch deployments
kubectl get deployments -n ehr-platform -w

# Check pods
kubectl get pods -n ehr-platform -o wide

# Check volumes
kubectl get pvc -n ehr-platform

# Check secrets
kubectl get secrets -n ehr-platform

# Check events
kubectl get events -n ehr-platform --sort-by='.lastTimestamp'
```

### View Logs

```bash
# All pods
kubectl logs -n ehr-platform -l app.kubernetes.io/part-of=ehr-platform --tail=100 -f

# Specific service
kubectl logs -n ehr-platform -l app=api-gateway -f

# Previous pod (if crashed)
kubectl logs -n ehr-platform <pod-name> --previous
```

---

## Service Discovery

**In-cluster DNS**: `<service-name>.<namespace>.svc.cluster.local`

Examples:
- PostgreSQL Identity: `postgresql-identity.ehr-platform.svc.cluster.local:5432`
- MongoDB: `mongodb.ehr-platform.svc.cluster.local:27017`
- RabbitMQ: `rabbitmq.ehr-platform.svc.cluster.local:5672`

## Networking

**Network Policies**:
- Services can communicate within namespace
- External egress: Only DNS (port 53)
- Ingress Controller handles external traffic

**Service Types**:
- **ClusterIP**: Internal service discovery
- **LoadBalancer**: External access (API Gateway)
- **NodePort**: Debug/testing only

---

## Scaling

### Horizontal Scaling

```bash
# Scale api-gateway to 5 replicas
kubectl scale deployment api-gateway -n ehr-platform --replicas=5

# Auto-scaling with HPA
kubectl autoscale deployment api-gateway -n ehr-platform \
  --min=2 --max=10 --cpu-percent=80
```

### Vertical Scaling

Edit kustomization overlay:
```yaml
patches:
  - target:
      kind: Deployment
    patch: |-
      - op: replace
        path: /spec/template/spec/containers/0/resources/limits/cpu
        value: "2000m"
```

---

## Updates & Rollouts

### Rolling Update

```bash
# Update image
kubectl set image deployment/api-gateway \
  api-gateway=ehr-platform/api-gateway:1.1.0 \
  -n ehr-platform

# Watch rollout
kubectl rollout status deployment/api-gateway -n ehr-platform -w

# Check rollout history
kubectl rollout history deployment/api-gateway -n ehr-platform

# Rollback
kubectl rollout undo deployment/api-gateway -n ehr-platform
```

---

## Monitoring

### Prometheus Scrape Targets

Configured in `01-configmaps.yaml`:
- OTEL Collector (:8888)
- 10 microservices (:5000-:5009)

### Grafana Dashboards

Pre-configured datasources:
- Prometheus (metrics)
- Loki (logs)
- Tempo (traces)

### Alerts

Define in overlays as PrometheusRule resources (not in base)

---

## Security Best Practices

✅ **Implemented**:
- Namespace isolation
- Network policies (zero-trust)
- Pod security policies
- Resource quotas
- Read-only root filesystems (recommended)

⚠️ **TODO (Production)**:
- Use External Secrets Operator for secret rotation
- RBAC roles and bindings per service
- Pod security standards (K8s 1.25+)
- Network policy egress to external services
- Image vulnerability scanning
- OPA/Gatekeeper policies

---

## Disaster Recovery

### Backup

```bash
# Backup namespace
kubectl get all,configmap,secret,pvc -n ehr-platform -o yaml > backup.yaml

# Backup specific resource
kubectl get deployment api-gateway -n ehr-platform -o yaml > api-gateway-backup.yaml
```

### Restore

```bash
# Restore from backup
kubectl apply -f backup.yaml

# Restore specific resource
kubectl apply -f api-gateway-backup.yaml
```

### PersistentVolume Backup

Use cloud provider tools:
- AWS: AWS Backup, EBS Snapshots
- Azure: Azure Backup
- GCP: GKE backup services

---

## References

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Kustomize Documentation](https://kustomize.io/)
- [Network Policies](https://kubernetes.io/docs/concepts/services-networking/network-policies/)
- [Pod Security Policies](https://kubernetes.io/docs/concepts/policy/pod-security-policy/)
- [Resource Quotas](https://kubernetes.io/docs/concepts/policy/resource-quotas/)
