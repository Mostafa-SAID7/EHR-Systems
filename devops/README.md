# EHR Platform — DevOps & Infrastructure

Production-grade, HIPAA-compliant DevOps infrastructure for the Electronic Health Records platform.

---

## Folder Structure

```
devops/
├── docker/                     # Local development stack
│   ├── docker-compose.yml      # Full stack (infra + all microservices)
│   ├── docker-compose.override.yml  # Dev overrides (hot-reload, extra tools)
│   ├── .env.example            # Environment variable template
│   └── services/
│       ├── Dockerfile.backend  # Multi-stage .NET 8 image (all microservices)
│       ├── Dockerfile.frontend # Multi-stage Angular 18 + Nginx image
│       └── nginx.conf          # SPA routing + security headers
├── kubernetes/
│   ├── base/                   # Kustomize base manifests
│   │   ├── namespace.yaml
│   │   ├── configmap.yaml      # Shared non-secret config
│   │   ├── secrets.yaml        # Placeholder — use Sealed Secrets / ESO in prod
│   │   ├── rbac.yaml           # Least-privilege service account
│   │   ├── api-gateway.yaml    # Deployment + Service + PDB
│   │   ├── identity-service.yaml
│   │   ├── patient-service.yaml
│   │   ├── services-remaining.yaml  # Clinical, Appointment, Notification, Audit, Billing, Prescription, Analytics, Frontend
│   │   ├── hpa.yaml            # Horizontal Pod Autoscalers for all services
│   │   ├── ingress.yaml        # NGINX Ingress + cert-manager TLS
│   │   ├── network-policy.yaml # Zero-trust network policies
│   │   └── kustomization.yaml
│   ├── overlays/
│   │   ├── dev/                # Scaled-down, debug logging
│   │   ├── staging/            # Production-parity, 2 replicas
│   │   └── prod/               # Full HA, 3+ replicas, HPA active
│   └── kustomization.yaml
├── terraform/
│   ├── main.tf                 # Root module — calls all sub-modules
│   ├── variables.tf            # Input variables
│   ├── outputs.tf              # Useful post-apply outputs
│   ├── backend.tf              # Remote state (S3 + DynamoDB)
│   ├── modules/
│   │   ├── networking/         # VPC, subnets, NAT gateway
│   │   ├── kubernetes/         # EKS / AKS / GKE cluster
│   │   ├── databases/          # RDS Aurora PG, ElastiCache Redis
│   │   ├── messaging/          # Amazon MSK (Kafka), Amazon MQ (RabbitMQ)
│   │   ├── storage/            # S3 with encryption + HIPAA 7-year retention
│   │   ├── monitoring/         # Prometheus stack + Grafana via Helm
│   │   └── security/           # KMS, WAF, Secrets Manager, IAM
│   └── environments/
│       ├── dev/                # Cost-optimised, single-AZ
│       ├── staging/            # HA, multi-AZ, production-parity
│       └── prod/               # Full production — approved reviewers only
├── ci-cd/
│   ├── build-and-test.yml      # GitHub Actions: build → test → scan → push images
│   ├── deploy.yml              # GitHub Actions: migrate → deploy (dev/staging/prod)
│   └── security-scan.yml       # Trivy + CodeQL + HIPAA compliance checks (daily)
├── monitoring/
│   ├── prometheus.yml          # Prometheus scrape config (local dev)
│   ├── alertmanager.yml        # Alert routing → PagerDuty + Slack
│   ├── otel-collector.yml      # OpenTelemetry collector (traces → Jaeger, metrics → Prometheus)
│   └── grafana/dashboards/
│       └── ehr-overview.json   # Grafana dashboard: request rate, latency p95/p99, error rate
├── scripts/
│   ├── build.sh                # Build all Docker images
│   ├── deploy.sh               # Deploy to a Kubernetes environment
│   ├── migrate.sh              # Run EF Core DB migrations
│   ├── backup.sh               # Backup all PostgreSQL databases (encrypted, S3)
│   └── health-check.sh         # Check health of all services (local + k8s)
└── README.md
```

---

## Quick Start — Local Development

### Prerequisites
- Docker Desktop ≥ 24 with Compose V2
- (Optional) `.NET 8 SDK` for running services outside Docker

### 1. Configure environment

```bash
cp devops/docker/.env.example devops/docker/.env
# Edit devops/docker/.env — set JWT_SECRET (≥32 chars) and ENCRYPTION_KEY (exactly 32 chars)
```

### 2. Start the full stack

```bash
# Infrastructure + all microservices + frontend
docker compose -f devops/docker/docker-compose.yml up -d

# Also start Prometheus + Grafana + Jaeger
docker compose -f devops/docker/docker-compose.yml --profile monitoring up -d
```

### 3. Access services

| Service            | URL                          |
|--------------------|------------------------------|
| Frontend (Angular) | http://localhost:4200        |
| API Gateway        | http://localhost:5000        |
| Identity Service   | http://localhost:5001/swagger |
| Kafka UI           | http://localhost:8090        |
| RabbitMQ UI        | http://localhost:15672       |
| Kibana             | http://localhost:5601        |
| Grafana            | http://localhost:3001        |
| Jaeger UI          | http://localhost:16686       |

### 4. Run health check

```bash
./devops/scripts/health-check.sh local
```

### 5. Run database migrations (local)

```bash
./devops/scripts/migrate.sh
```

---

## Kubernetes Deployment

### Prerequisites
- `kubectl` configured for your cluster
- `kustomize` ≥ 5.0

### Deploy to dev

```bash
kubectl apply -k devops/kubernetes/overlays/dev
```

### Deploy to staging

```bash
kubectl apply -k devops/kubernetes/overlays/staging
```

### Deploy to production (requires approved reviewer)

```bash
# Update image tags in the overlay first
./devops/scripts/deploy.sh prod --tag 1.2.3
```

---

## Terraform

### Prerequisites
- Terraform ≥ 1.6
- AWS CLI configured (or Azure/GCP equivalent)
- S3 bucket for remote state (see `backend.tf`)

### Bootstrap state storage (AWS, one-time)

```bash
aws s3 mb s3://ehr-platform-terraform-state
aws dynamodb create-table \
  --table-name ehr-platform-terraform-locks \
  --attribute-definitions AttributeName=LockID,AttributeType=S \
  --key-schema AttributeName=LockID,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST
```

### Deploy infrastructure

```bash
cd devops/terraform/environments/dev
terraform init
terraform plan -out=tfplan
terraform apply tfplan
```

---

## CI/CD (GitHub Actions)

Copy the workflow files to `.github/workflows/`:

```bash
cp devops/ci-cd/*.yml .github/workflows/
```

### Required repository secrets

| Secret                   | Description                                        |
|--------------------------|----------------------------------------------------|
| `SNYK_TOKEN`             | Snyk security scanning                             |
| `PAGERDUTY_ROUTING_KEY`  | PagerDuty critical alert integration               |
| `SLACK_WEBHOOK_URL`      | Slack alert webhook                                |
| `TF_VAR_grafana_admin_password` | Grafana admin password                      |
| `AWS_ACCESS_KEY_ID`      | Deployment IAM credentials                         |
| `AWS_SECRET_ACCESS_KEY`  | Deployment IAM credentials                         |

### Pipeline overview

1. **Build & Test** (`build-and-test.yml`) — runs on every push/PR
   - Build .NET backend + Angular frontend
   - Run unit tests + collect coverage
   - Trivy filesystem scan (CRITICAL/HIGH → fail)
   - Build and push Docker images to GHCR (main branch only)

2. **Deploy** (`deploy.yml`) — runs after Build & Test passes on main
   - Run DB migrations
   - Apply Kustomize overlay for target environment
   - Wait for rollouts + smoke test
   - Production requires manual approval

3. **Security Scan** (`security-scan.yml`) — runs daily at 02:00 UTC
   - Trivy + CodeQL SAST + dependency review
   - HIPAA compliance checks (no hardcoded secrets, non-root containers, TLS enforced)

---

## Monitoring & Observability

| Signal   | Tool                          | Storage       |
|----------|-------------------------------|---------------|
| Metrics  | Prometheus + Grafana          | Prometheus TSDB (30 days) |
| Traces   | OpenTelemetry → Jaeger        | Jaeger in-memory (dev) / Tempo (prod) |
| Logs     | Serilog → stdout → Loki/ELK   | Elasticsearch (90 days) |
| Alerts   | Alertmanager → PagerDuty + Slack | — |

### Import Grafana dashboard

1. Open Grafana → Dashboards → Import
2. Upload `devops/monitoring/grafana/dashboards/ehr-overview.json`

---

## Security & HIPAA Compliance

| Control                    | Implementation                                    |
|----------------------------|---------------------------------------------------|
| Encryption at rest         | KMS-encrypted RDS, S3 SSE-KMS, Redis TLS          |
| Encryption in transit      | TLS everywhere (mTLS via service mesh optional)   |
| Secrets management         | AWS Secrets Manager / Sealed Secrets — no plaintext in Git |
| Least-privilege            | Non-root containers, RBAC service accounts, scoped IAM |
| Network segmentation       | K8s NetworkPolicy zero-trust, VPC private subnets |
| Audit logging              | All infrastructure changes logged (CloudTrail/AuditLog service) |
| Backup retention           | 7 years — S3 Object Lock COMPLIANCE mode          |
| Vulnerability scanning     | Trivy (daily) + Snyk + CodeQL                     |
| WAF                        | AWS WAFv2 with managed rule sets + rate limiting  |

---

## Scripts Reference

```bash
# Build all images
./devops/scripts/build.sh [--push] [--tag v1.2.3] [--service EHRPlatform.Services.Identity]

# Deploy to an environment
./devops/scripts/deploy.sh <dev|staging|prod> [--tag v1.2.3]

# Run migrations
./devops/scripts/migrate.sh [--service EHRPlatform.Services.Identity] [--env local]

# Backup all databases
./devops/scripts/backup.sh [--target s3] [--bucket my-bucket]

# Health check
./devops/scripts/health-check.sh [local|k8s]
```
