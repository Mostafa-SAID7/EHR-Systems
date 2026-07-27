# Kubernetes Quick Reference

## Deploy

```powershell
# Deploy to dev (1 replica per service)
.\deploy.ps1 -Environment dev -Wait

# Deploy to staging (2 replicas)
.\deploy.ps1 -Environment staging -Wait

# Deploy to production (3+ replicas, HA)
.\deploy.ps1 -Environment prod -Wait

# Dry run (see manifests)
.\deploy.ps1 -Environment dev -Dry
```

## Access Services

```bash
# API Gateway (port 5000)
kubectl port-forward -n ehr-platform svc/api-gateway 5000:80

# Grafana (port 3000)
kubectl port-forward -n ehr-platform svc/grafana 3000:3000

# Prometheus (port 9090)
kubectl port-forward -n ehr-platform svc/prometheus 9090:9090

# Loki (port 3100)
kubectl port-forward -n ehr-platform svc/loki 3100:3100

# Tempo (port 3200)
kubectl port-forward -n ehr-platform svc/tempo 3200:3200
```

## Monitoring

```bash
# Watch pods
kubectl get pods -n ehr-platform -w

# Pod status
kubectl get pods -n ehr-platform

# Pod details
kubectl describe pod <pod-name> -n ehr-platform

# View logs
kubectl logs -n ehr-platform deployment/api-gateway -f

# All logs
kubectl logs -n ehr-platform -f --all-containers=true --prefix=true

# Previous logs (crashed container)
kubectl logs -n ehr-platform <pod-name> --previous

# Resource usage
kubectl top pods -n ehr-platform
kubectl top nodes
```

## Deployments

```bash
# List all deployments
kubectl get deployments -n ehr-platform

# Rollout status
kubectl rollout status deployment/api-gateway -n ehr-platform

# Rollout history
kubectl rollout history deployment/api-gateway -n ehr-platform

# Rollback to previous
kubectl rollout undo deployment/api-gateway -n ehr-platform

# Restart deployment
kubectl rollout restart deployment/api-gateway -n ehr-platform

# Scale deployment
kubectl scale deployment api-gateway --replicas=3 -n ehr-platform
```

## Debugging

```bash
# Exec into pod
kubectl exec -it <pod-name> -n ehr-platform -- /bin/sh

# Port forward to pod
kubectl port-forward <pod-name> 5000:5000 -n ehr-platform

# Copy file from pod
kubectl cp ehr-platform/<pod-name>:/app/logs.txt ./logs.txt

# Events in namespace
kubectl get events -n ehr-platform --sort-by='.lastTimestamp'

# Describe node
kubectl describe node <node-name>

# Check storage
kubectl get pvc -n ehr-platform
kubectl get pv
```

## Configuration

```bash
# View ConfigMap
kubectl get configmap -n ehr-platform
kubectl describe configmap ehr-config -n ehr-platform

# Edit ConfigMap
kubectl edit configmap ehr-config -n ehr-platform

# View Secret
kubectl get secret -n ehr-platform

# Edit Secret (base64)
kubectl edit secret ehr-secrets -n ehr-platform
```

## Networking

```bash
# List services
kubectl get svc -n ehr-platform

# List ingress
kubectl get ingress -n ehr-platform

# Describe ingress
kubectl describe ingress ehr-ingress -n ehr-platform

# DNS resolution test
kubectl exec -it <pod-name> -n ehr-platform -- nslookup postgres

# Network policy
kubectl get networkpolicies -n ehr-platform
```

## Cleanup

```bash
# Delete namespace (cascades all resources)
kubectl delete namespace ehr-platform

# Delete deployment
kubectl delete deployment api-gateway -n ehr-platform

# Delete all resources in namespace
kubectl delete all -n ehr-platform

# Delete PVC (persistent data)
kubectl delete pvc --all -n ehr-platform

# Delete specific layer
kubectl delete -f devops/kubernetes/3-services.yml
```

## Status Checks

```bash
# Overall cluster health
kubectl cluster-info

# Node status
kubectl get nodes

# All resources
kubectl get all -n ehr-platform

# Detailed status
kubectl get all -n ehr-platform -o wide

# Resource usage
kubectl top nodes
kubectl top pods -n ehr-platform --sort-by=memory
```

## Common Ports (Port Forward)

| Service | Local | Remote | Command |
|---------|-------|--------|---------|
| API Gateway | 5000 | 80 | `kubectl port-forward svc/api-gateway 5000:80` |
| Grafana | 3000 | 3000 | `kubectl port-forward svc/grafana 3000:3000` |
| Prometheus | 9090 | 9090 | `kubectl port-forward svc/prometheus 9090:9090` |
| Loki | 3100 | 3100 | `kubectl port-forward svc/loki 3100:3100` |
| Tempo | 3200 | 3200 | `kubectl port-forward svc/tempo 3200:3200` |
| PostgreSQL | 5432 | 5432 | `kubectl port-forward svc/postgres 5432:5432` |
| MongoDB | 27017 | 27017 | `kubectl port-forward svc/mongodb 27017:27017` |
| RabbitMQ | 5672 | 5672 | `kubectl port-forward svc/rabbitmq 5672:5672` |
| RabbitMQ Mgmt | 15672 | 15672 | `kubectl port-forward svc/rabbitmq 15672:15672` |
| Redis | 6379 | 6379 | `kubectl port-forward svc/redis 6379:6379` |
