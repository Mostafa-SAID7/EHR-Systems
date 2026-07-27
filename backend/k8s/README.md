# EHR Platform - Kubernetes Deployment Guide

Enterprise-grade Helm chart for deploying the EHR Platform microservices stack on Kubernetes.

## Prerequisites

- Kubernetes 1.24+
- Helm 3.10+
- kubectl configured
- Docker images built and pushed to registry
- Persistent Volume storage class available

## Quick Start

### 1. Install (Development)

```bash
cd backend/k8s

# Create namespace and deploy with dev values
helm install ehr ./ehr-platform \
  -f ehr-platform/values-dev.yaml \
  -n ehr-platform \
  --create-namespace
```

### 2. Install (Staging)

```bash
helm install ehr ./ehr-platform \
  -f ehr-platform/values-staging.yaml \
  -n ehr-platform \
  --create-namespace
```

### 3. Install (Production)

```bash
# Update secrets in values-prod.yaml first!
helm install ehr ./ehr-platform \
  -f ehr-platform/values-prod.yaml \
  -n ehr-platform \
  --create-namespace
```

## Architecture

### Services Deployed (8 total)

| Service | Port | Replicas (Prod) | Notes |
|---------|------|-----------------|-------|
| API Gateway | 5000 | 3-10 | Entry point, YARP routing |
| Identity | 5001 | 3-6 | Authentication, JWT |
| Patient | 5002 | 3-10 | Core patient data, search |
| Clinical | 5003 | 2-6 | Clinical records |
| Appointment | 5004 | 3-10 | Scheduling, search |
| Notification | 5005 | 2-5 | Event notifications |
| Audit | 5006 | 2-4 | Audit logging |
| Billing | 5007 | 2-6 | Invoicing, search |
| Prescription | 5008 | 2-5 | Prescriptions |
| Analytics | 5009 | 3-8 | Reporting, analytics search |

### Infrastructure (3 total)

| Component | Type | Port | Storage |
|-----------|------|------|---------|
| PostgreSQL | StatefulSet | 5432 | PersistentVolume |
| Redis | StatefulSet | 6379 | PersistentVolume |
| Elasticsearch | StatefulSet | 9200 | PersistentVolume |

## Customization

### Environment-Specific Values

```bash
# Development (minimal resources, single replicas)
helm install ehr ./ehr-platform -f values-dev.yaml

# Staging (moderate resources, multi-replica)
helm install ehr ./ehr-platform -f values-staging.yaml

# Production (full resources, autoscaling, HA)
helm install ehr ./ehr-platform -f values-prod.yaml
```

### Override Individual Values

```bash
helm install ehr ./ehr-platform \
  --set apiGateway.replicaCount=5 \
  --set infrastructure.postgresql.persistence.size=200Gi \
  --set global.imageRegistry=myregistry.azurecr.io
```

## Secrets Management

⚠️ **SECURITY WARNING**: Do not commit actual secrets to Git!

### Development (In-chart secrets)
Secrets are defined in `values.yaml` - acceptable for local/dev only.

### Production (External Secrets)

Use one of these tools:

1. **External Secrets Operator** (recommended)
   ```bash
   helm install external-secrets external-secrets/external-secrets -n external-secrets-system --create-namespace
   ```

2. **Sealed Secrets**
   ```bash
   helm install sealed-secrets sealed-secrets/sealed-secrets -n kube-system
   ```

3. **HashiCorp Vault**
   - Integrate via Vault Agent Injector
   - Mount Vault secrets into pods

## Monitoring

### Prometheus & Grafana

Services expose metrics on `/metrics` endpoint:
- Port 5000 (API Gateway)
- Ports 5001-5009 (Microservices)

Scrape interval: 15s (configurable in Prometheus)

### Health Checks

All services expose `/health` endpoint:
- Liveness: checks process health
- Readiness: checks DB/Redis/ES connectivity

## Networking

### Ingress

```bash
# Apply Ingress controller (nginx recommended)
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.1/deploy/static/provider/cloud/deploy.yaml

# Verify ingress routes
kubectl get ingress -n ehr-platform
```

### Network Policies

Deployed by default:
- Default-deny all traffic
- Allow ingress on API Gateway (port 5000)
- Allow internal service-to-service communication
- Allow database/cache/search access
- Allow Prometheus scraping

## Autoscaling

### HPA (Horizontal Pod Autoscaler)

Enabled by default in staging/production:

```bash
# Check HPA status
kubectl get hpa -n ehr-platform

# Manual scaling
kubectl scale deployment api-gateway -n ehr-platform --replicas=5
```

### Metrics

- CPU: 70% target (configurable)
- Memory: Not used for HPA (relies on CPU)
- Min/Max replicas per service in values

## Storage

### PersistentVolumes

Required storage classes:
- `standard`: For development (default)
- `fast-ssd`: For production (high-IOPS databases)

Create storage class:
```bash
kubectl apply -f - <<EOF
apiVersion: storage.k8s.io/v1
kind: StorageClass
metadata:
  name: fast-ssd
provisioner: kubernetes.io/aws-ebs  # or your provider
parameters:
  type: gp3
  iops: "3000"
  throughput: "125"
EOF
```

## Upgrade & Rollback

### Upgrade to New Version

```bash
# Pull latest chart
helm repo update

# Upgrade
helm upgrade ehr ./ehr-platform \
  -f ehr-platform/values-prod.yaml \
  -n ehr-platform

# Check status
helm status ehr -n ehr-platform
```

### Rollback

```bash
# View history
helm history ehr -n ehr-platform

# Rollback to previous version
helm rollback ehr 2 -n ehr-platform

# Force immediate rollback
helm rollback ehr -n ehr-platform --force
```

## Troubleshooting

### Pod not starting

```bash
# Check pod status
kubectl get pods -n ehr-platform
kubectl describe pod <pod-name> -n ehr-platform
kubectl logs <pod-name> -n ehr-platform
```

### Database connection error

```bash
# Check PostgreSQL
kubectl get statefulset postgresql -n ehr-platform
kubectl logs postgresql-0 -n ehr-platform

# Test connectivity
kubectl exec -it postgresql-0 -n ehr-platform -- psql -U ehr_user -d ehr_platform
```

### Redis connectivity

```bash
# Check Redis
kubectl get statefulset redis -n ehr-platform
kubectl exec -it redis-0 -n ehr-platform -- redis-cli
```

### Elasticsearch health

```bash
# Check ES cluster
kubectl exec -it elasticsearch-0 -n ehr-platform -- curl localhost:9200/_cluster/health
```

## Resource Quotas

Default limits per environment:

| Environment | CPU | Memory |
|------------|-----|--------|
| Development | 2 CPU / 4 CPU | 4Gi / 8Gi |
| Staging | 6 CPU / 12 CPU | 12Gi / 24Gi |
| Production | 20 CPU / 40 CPU | 40Gi / 80Gi |

Adjust in corresponding `values-*.yaml` file.

## Security Best Practices

✅ **Implemented**
- SecurityContext: Non-root user, read-only filesystem
- Network Policies: Restrict ingress/egress
- RBAC: Service accounts with least-privilege roles
- Pod Disruption Budgets: HA during node maintenance
- Health Checks: Liveness/readiness probes

🔒 **To Implement**
- Use External Secrets for production credentials
- Enable Pod Security Policy / Pod Security Standards
- Implement NetworkPolicy egress rules (whitelist IPs)
- Use Istio or Linkerd for mTLS between services
- Enable audit logging on all API calls

## Uninstall

```bash
# Delete release
helm uninstall ehr -n ehr-platform

# Delete namespace (optional)
kubectl delete namespace ehr-platform

# Delete PersistentVolumes (optional - data destruction!)
kubectl delete pvc --all -n ehr-platform
```

## Support

For issues, check:
1. Pod logs: `kubectl logs <pod> -n ehr-platform`
2. Events: `kubectl describe pod <pod> -n ehr-platform`
3. Resource limits: `kubectl top pods -n ehr-platform`
4. Network: `kubectl get svc,ingress -n ehr-platform`
