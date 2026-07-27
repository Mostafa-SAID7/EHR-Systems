# EHR Platform on Kubernetes

**Production-ready Kubernetes manifests with 3-layer modular architecture**

## Overview

The EHR Platform uses a **modular, 3-layer Kubernetes architecture**:

1. **Infrastructure** (`1-infrastructure.yml`) - Stateful services (databases, cache, messaging)
2. **Monitoring** (`2-monitoring.yml`) - Observability stack (Prometheus, Grafana, Loki, Tempo, OTEL)
3. **Services** (`3-services.yml`) - 10 microservices
4. **Ingress** (`4-ingress.yml`) - External access, autoscaling, pod disruption budgets

Plus:
- **Namespace & RBAC** (`0-namespace.yml`) - Network policies, service accounts, RBAC
- **Kustomization** (`kustomization.yml`) - Environment-specific overlays (dev/staging/prod)

## Quick Start

### Prerequisites

```bash
# Install kubectl
# Install Kubernetes cluster (minikube, kind, EKS, GKE, AKS, etc.)
# Install cert-manager (for TLS)
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Verify cluster
kubectl cluster-info
kubectl get nodes
```

### Deploy to Dev Environment

```powershell
# Deploy to development (1 replica per service)
.\deploy.ps1 -Environment dev -Wait

# Dry run (see manifests without applying)
.\deploy.ps1 -Environment dev -Dry

# Deploy to staging
.\deploy.ps1 -Environment staging -Wait

# Deploy to production
.\deploy.ps1 -Environment prod -Wait
```

### Access Services

```bash
# Port forward API Gateway
kubectl port-forward -n ehr-platform svc/api-gateway 5000:80

# Port forward Grafana
kubectl port-forward -n ehr-platform svc/grafana 3000:3000

# Access via Ingress (if configured)
# api.ehr-platform.local
# grafana.ehr-platform.local
# prometheus.ehr-platform.local
```

## Architecture

### Layer 1: Infrastructure (Stateful Services)

**StatefulSets** with persistent volumes:
- **PostgreSQL** (1 instance) - 5 logical databases
- **MongoDB** (1 instance) - Document storage
- **MySQL** (1 instance) - Billing database
- **Elasticsearch** (1 instance) - Search
- **RabbitMQ** (1 instance) - Message broker
- **Kafka** + **Zookeeper** (1 each) - Event streaming

**Deployments**:
- **Redis** (1 instance) - Cache

All include:
- Health checks (liveness, readiness)
- Resource requests/limits
- Persistent volume claims (10Gi each)
- Service discovery via DNS

### Layer 2: Monitoring (Observability)

**Deployments**:
- **Prometheus** (9090) - Metrics scraper & storage
- **Loki** (3100) - Log aggregation
- **Tempo** (3200) - Distributed tracing
- **OTEL Collector** (4317/4318) - Telemetry pipeline
- **Grafana** (3000) - Dashboards & visualization

All pre-configured:
- Datasources provisioned
- Scrape configs for Prometheus
- OTLP receivers for application telemetry

### Layer 3: Services (Microservices)

**10 Deployments** (ports 5000-5009):

| Service | Port | Database | Replicas |
|---------|------|----------|----------|
| API Gateway | 5000 | - | 2 (dev: 1) |
| Identity | 5001 | PostgreSQL | 2 (dev: 1) |
| Patient | 5002 | PostgreSQL | 2 (dev: 1) |
| Clinical | 5003 | PostgreSQL | 2 (dev: 1) |
| Appointment | 5004 | PostgreSQL | 2 (dev: 1) |
| Prescription | 5005 | MongoDB | 2 (dev: 1) |
| Billing | 5006 | MySQL | 2 (dev: 1) |
| Audit | 5007 | PostgreSQL | 1 |
| Notification | 5008 | MongoDB | 1 |
| Analytics | 5009 | MongoDB | 1 |

All configured with:
- OTLP telemetry export
- Health checks (liveness, readiness)
- Resource requests/limits
- ConfigMap & Secret references

### Layer 4: Ingress & Networking

**Ingress** routes external traffic:
- `api.ehr-platform.local` → API Gateway
- `grafana.ehr-platform.local` → Grafana
- `prometheus.ehr-platform.local` → Prometheus
- `loki.ehr-platform.local` → Loki

**Autoscaling** (HPA):
- API Gateway: 2-5 replicas (CPU 70%)
- Patient Service: 2-4 replicas (CPU 70%)
- Clinical Service: 2-4 replicas (CPU 70%)
- Identity Service: 2-3 replicas (CPU 75%)

**High Availability**:
- Pod Disruption Budgets (minAvailable: 1)
- Pod anti-affinity on API Gateway (dev/prod only)

## Deployment Models

### Development

```bash
kubectl apply -k devops/kubernetes/overlays/dev
```

**Configuration**:
- 1 replica per service (save resources)
- Smaller resource limits (128-256Mi memory)
- Development environment settings
- No autoscaling

### Staging

```bash
kubectl apply -k devops/kubernetes/overlays/staging
```

**Configuration**:
- 2 replicas per core service
- Medium resource limits (256-512Mi memory)
- Staging environment settings
- Limited autoscaling

### Production

```bash
kubectl apply -k devops/kubernetes/overlays/production
```

**Configuration**:
- 3 replicas per core service (HA)
- Full resource limits (512Mi-1Gi memory)
- Production environment settings
- Aggressive autoscaling (2-5 replicas)
- Pod anti-affinity (spread across nodes)
- Pod Disruption Budget (minAvailable: 2)

## Deployment Script Usage

### Basic Deployment

```powershell
# Deploy dev environment
.\deploy.ps1 -Environment dev

# Wait for deployment to be ready (timeout 5 min)
.\deploy.ps1 -Environment dev -Wait -Timeout 300

# Dry run (preview without applying)
.\deploy.ps1 -Environment dev -Dry
```

### Manual Kustomize

```bash
# Generate manifests
kubectl kustomize devops/kubernetes/overlays/dev

# Apply all layers
kubectl apply -k devops/kubernetes/

# Apply specific overlay
kubectl apply -k devops/kubernetes/overlays/prod

# Update single layer
kubectl apply -f devops/kubernetes/2-monitoring.yml
```

## Monitoring & Observability

### Access Grafana

```bash
# Port forward
kubectl port-forward -n ehr-platform svc/grafana 3000:3000

# Open browser: http://localhost:3000
# Login: admin/admin (configured in secrets)
```

### View Logs

```bash
# Pod logs
kubectl logs -n ehr-platform deployment/api-gateway -f

# All containers in namespace
kubectl logs -n ehr-platform -f --all-containers=true

# Export to Loki
# Logs automatically shipped via OTEL Collector
```

### Check Metrics

```bash
# Port forward Prometheus
kubectl port-forward -n ehr-platform svc/prometheus 9090:9090

# Scrape targets: http://localhost:9090/targets
```

### Distributed Traces

```bash
# Port forward Tempo
kubectl port-forward -n ehr-platform svc/tempo 3200:3200

# Access via Grafana: Explore → Tempo datasource
```

## Troubleshooting

### Deployment Stuck

```bash
# Check pod status
kubectl get pods -n ehr-platform

# Check pod events
kubectl describe pod <pod-name> -n ehr-platform

# Check resource requests vs available
kubectl top nodes
kubectl top pods -n ehr-platform

# View pod logs
kubectl logs -n ehr-platform <pod-name> --previous
```

### Database Connection Errors

```bash
# Check if infrastructure is running
kubectl get statefulsets -n ehr-platform

# Verify service DNS resolution
kubectl exec -it deployment/api-gateway -n ehr-platform -- ping postgres

# Check StatefulSet readiness
kubectl get statefulsets -n ehr-platform -o wide
```

### Ingress Not Working

```bash
# Verify ingress controller is installed
kubectl get ingressclass
kubectl get ingress -n ehr-platform

# Check ingress status
kubectl describe ingress ehr-ingress -n ehr-platform

# Verify DNS: /etc/hosts or nslookup
nslookup api.ehr-platform.local
```

## Cleanup

```bash
# Delete all resources in namespace
kubectl delete namespace ehr-platform

# Delete specific environment
kubectl delete -k devops/kubernetes/overlays/dev

# Delete persistent volumes
kubectl delete pvc --all -n ehr-platform

# Purge everything
kubectl get all -n ehr-platform -o name | xargs kubectl delete -n ehr-platform
```

## Production Checklist

- [ ] Use external database (not in-cluster) for production data
- [ ] Configure persistent volumes with replication
- [ ] Set resource requests/limits for all containers
- [ ] Enable network policies (restrict pod-to-pod communication)
- [ ] Configure RBAC (service accounts, role bindings)
- [ ] Use sealed-secrets or external-secrets for sensitive data
- [ ] Enable audit logging on API server
- [ ] Configure pod security policies
- [ ] Set up monitoring alerts (Prometheus AlertManager)
- [ ] Configure log aggregation (ELK, Splunk, DataDog)
- [ ] Test disaster recovery (backup/restore)
- [ ] Load test before production deployment
- [ ] Use production-grade container registry
- [ ] Enable image scanning for vulnerabilities
- [ ] Configure horizontal pod autoscaling
- [ ] Set up ingress with TLS/SSL
- [ ] Configure CORS policies
- [ ] Rate limiting on API Gateway

## Performance Notes

- **StatefulSets**: Guarantee pod naming, DNS, storage persistence
- **Init Containers**: Kafka waits for Zookeeper before starting
- **PVC**: StorageClass `ehr-fast` with `WaitForFirstConsumer` binding
- **HPA**: Metrics server required (`kubectl top` command)
- **Network Policy**: Allows intra-namespace and DNS (kube-system)

## References

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Kustomize Guide](https://kustomize.io/)
- [StatefulSets](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/)
- [Horizontal Pod Autoscaler](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/)
- [Network Policies](https://kubernetes.io/docs/concepts/services-networking/network-policies/)
- [Pod Disruption Budgets](https://kubernetes.io/docs/tasks/run-application/configure-pdb/)
- [OpenTelemetry Operator](https://github.com/open-telemetry/opentelemetry-operator)
