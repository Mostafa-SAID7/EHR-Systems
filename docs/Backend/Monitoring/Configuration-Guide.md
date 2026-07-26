# Monitoring Configuration Guide

Step-by-step setup and customization of the EHR Platform observability stack (Prometheus, Grafana, Jaeger, OpenTelemetry Collector).

---

## 📋 Prerequisites

- Docker & Docker Compose ≥ 24 with Compose V2
- `kubectl` and `kustomize` for Kubernetes (production)
- Terraform ≥ 1.6 (for AWS/cloud infrastructure)
- Basic understanding of Prometheus metrics format

---

## 🚀 Local Development Setup

### 1. Start the Monitoring Stack with Docker Compose

```bash
cd devops/docker

# Start only monitoring (Prometheus, Grafana, Jaeger, Alertmanager)
docker compose --profile monitoring up -d

# Or: Start full stack (services + monitoring)
docker compose --profile monitoring up -d
```

**Services Started**:
- **Prometheus** (http://localhost:9090) - Time-series database
- **Grafana** (http://localhost:3001) - Visualization & dashboards
- **Jaeger** (http://localhost:16686) - Distributed tracing
- **AlertManager** (http://localhost:9093) - Alert routing
- **OpenTelemetry Collector** (localhost:4317) - Telemetry receiver

### 2. Configure OpenTelemetry Collector

The collector is pre-configured in `devops/monitoring/otel-collector.yml`:

```yaml
receivers:
  otlp:                          # Receives traces/metrics from services
    protocols:
      grpc:
        endpoint: "0.0.0.0:4317" # Services send spans here
      http:
        endpoint: "0.0.0.0:4318"

processors:
  batch:                         # Batch spans for efficiency
    timeout: 10s
    send_batch_size: 1024
  
  resource:                      # Add deployment context
    attributes:
      - key: deployment.environment
        value: "${ENVIRONMENT:-development}"
  
  attributes:                    # Redact PHI (HIPAA)
    actions:
      - key: patient.ssn
        action: delete
      - key: http.request.body
        action: delete

exporters:
  otlp/jaeger:                   # Traces → Jaeger
    endpoint: "jaeger:4317"
  
  prometheus:                    # Metrics → Prometheus
    endpoint: "0.0.0.0:8889"

service:
  pipelines:
    traces:                      # Trace processing pipeline
      receivers: [otlp]
      processors: [memory_limiter, resource, attributes, batch]
      exporters: [otlp/jaeger]
    
    metrics:                      # Metric processing pipeline
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [prometheus]
```

**To Customize**:
- Edit `devops/monitoring/otel-collector.yml`
- Restart collector: `docker compose restart otel-collector`
- Verify: `docker compose logs otel-collector`

### 3. Configure Prometheus Scrape Targets

Edit `devops/monitoring/prometheus.yml` to add services:

```yaml
global:
  scrape_interval: 15s           # How often to scrape metrics
  evaluation_interval: 15s       # How often to evaluate alerts

rule_files:
  - "alert-rules/*.yml"          # Load all alert rules

alerting:
  alertmanagers:
    - static_configs:
        - targets: ["alertmanager:9093"]

scrape_configs:
  - job_name: ehr-services       # Scrape all microservices
    static_configs:
      - targets:
          - api-gateway:8080
          - identity-service:8080
          - patient-service:8080
          # Add new services here
    relabel_configs:
      - source_labels: [__address__]
        regex: "([^:]+):.*"
        target_label: service
        replacement: "$1"
```

**To Add a New Service**:
1. Add service to `targets` list
2. Restart Prometheus: `docker compose restart prometheus`
3. Verify in Prometheus UI → Targets tab (should show "UP")

### 4. Configure Alert Routing (Alertmanager)

Edit `devops/monitoring/alertmanager.yml`:

```yaml
global:
  resolve_timeout: 5m
  slack_api_url: "$SLACK_WEBHOOK_URL"      # Set via .env
  pagerduty_url: "https://events.pagerduty.com/v2/enqueue"

route:
  group_by: ["alertname", "service"]
  group_wait: 30s                # Wait 30s before sending alert
  group_interval: 5m             # Regroup every 5m
  repeat_interval: 4h            # Re-notify every 4h
  receiver: slack-default

  routes:
    # Critical alerts → PagerDuty
    - matchers:
        - severity="critical"
      receiver: pagerduty-critical
      continue: true             # Also route to default

    # HIPAA/compliance alerts → dedicated channel
    - matchers:
        - alertname=~"AuditLog.*|Unencrypted.*"
      receiver: slack-hipaa-audit

receivers:
  - name: slack-default
    slack_configs:
      - channel: "#ehr-alerts"
        title: "{{ .GroupLabels.alertname }}"
        text: "Alert: {{ .Annotations.description }}"
        send_resolved: true

  - name: pagerduty-critical
    pagerduty_configs:
      - routing_key: "$PAGERDUTY_ROUTING_KEY"  # Set via .env
        description: "{{ .CommonAnnotations.summary }}"
        send_resolved: true
```

**To Add Slack Integration**:
1. Create Slack webhook: Slack workspace → Settings → Apps → Incoming Webhooks → Create
2. Copy webhook URL
3. Set in `.env`: `SLACK_WEBHOOK_URL=https://hooks.slack.com/services/YOUR/WEBHOOK/URL`
4. Restart Alertmanager: `docker compose restart alertmanager`

**To Add PagerDuty Integration**:
1. Get routing key from PagerDuty: Services → Integration → Routing Key
2. Set in `.env`: `PAGERDUTY_ROUTING_KEY=your-key`
3. Restart Alertmanager

### 5. Configure Grafana

#### First Login
- URL: http://localhost:3001
- Default credentials: admin / admin
- **Change password immediately**

#### Add Prometheus Data Source
1. Grafana → Configuration → Data Sources → Add data source
2. Select "Prometheus"
3. Set URL: `http://prometheus:9090`
4. Click "Test" → should see green "Data source is working"
5. Save

#### Import Pre-built Dashboard
1. Grafana → Dashboards → Import
2. Upload `devops/monitoring/grafana/dashboards/ehr-overview.json`
3. Select Prometheus as data source
4. Save

#### Create Custom Dashboard
1. Grafana → Dashboards → New → Dashboard
2. Click "Add panel"
3. Enter Prometheus query (e.g., `rate(http_requests_total[5m])`)
4. Configure visualization (graph, gauge, table)
5. Save dashboard

---

## 🔧 Adding New Alert Rules

Alert rules are defined in `devops/monitoring/alert-rules/ehr-alerts.yml`.

### Example: Create Alert for High Order Processing Time

```yaml
- name: ehr.billing
  interval: 30s
  rules:
    - alert: BillingProcessingTimeHigh
      expr: |
        histogram_quantile(0.95, 
          sum(rate(billing_process_duration_seconds_bucket[5m])) by (le)
        ) > 10
      for: 5m
      labels:
        severity: warning
        service: billing
      annotations:
        summary: "High billing processing time"
        description: |
          P95 billing process time is {{ $value }}s.
          This may delay invoice generation. Check for slow database queries.
```

### Alert Expression Syntax

**Metric Query**:
```
http_requests_total{service="patient-service", status="500"}
```

**Rate (requests/sec)**:
```
rate(http_requests_total[5m])
```

**Percentage (errors / total)**:
```
(
  sum(rate(http_requests_total{status=~"5.."}[5m]))
  /
  sum(rate(http_requests_total[5m]))
) * 100
```

**Percentile (P99 latency)**:
```
histogram_quantile(0.99, sum(rate(http_request_duration_seconds_bucket[5m])) by (le))
```

### Reload Alert Rules
1. Edit `devops/monitoring/alert-rules/ehr-alerts.yml`
2. Restart Prometheus: `docker compose restart prometheus`
3. Verify in Prometheus UI → Alerts tab

---

## 🎯 Kubernetes Production Setup

### 1. Deploy Monitoring Stack (Helm)

```bash
cd devops/kubernetes

# Add Prometheus Helm repo
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update

# Install kube-prometheus-stack (includes Prometheus, Grafana, AlertManager)
helm install monitoring prometheus-community/kube-prometheus-stack \
  --namespace monitoring \
  --create-namespace \
  -f - <<EOF
prometheus:
  retention: 30d
  storageSpec:
    accessModes: ["ReadWriteOnce"]
    resources:
      requests:
        storage: 50Gi

grafana:
  adminPassword: $(openssl rand -base64 32)
  persistence:
    enabled: true
    size: 10Gi

alertmanager:
  config:
    slack_api_url: "${SLACK_WEBHOOK_URL}"
    pagerduty_url: "https://events.pagerduty.com/v2/enqueue"
EOF
```

### 2. Deploy OpenTelemetry Collector

```bash
# Create ConfigMap from otel-collector.yml
kubectl create configmap otel-collector \
  -n monitoring \
  --from-file=devops/monitoring/otel-collector.yml

# Deploy Collector
kubectl apply -f - <<EOF
apiVersion: apps/v1
kind: Deployment
metadata:
  name: otel-collector
  namespace: monitoring
spec:
  replicas: 2
  selector:
    matchLabels:
      app: otel-collector
  template:
    metadata:
      labels:
        app: otel-collector
    spec:
      containers:
      - name: collector
        image: otel/opentelemetry-collector:latest
        ports:
        - containerPort: 4317  # gRPC
        - containerPort: 4318  # HTTP
        - containerPort: 8889  # Prometheus metrics
        volumeMounts:
        - name: config
          mountPath: /etc/otel-collector-config.yaml
          subPath: otel-collector.yml
        env:
        - name: ENVIRONMENT
          value: "production"
      volumes:
      - name: config
        configMap:
          name: otel-collector
---
apiVersion: v1
kind: Service
metadata:
  name: otel-collector
  namespace: monitoring
spec:
  selector:
    app: otel-collector
  ports:
  - name: otlp-grpc
    port: 4317
    targetPort: 4317
  - name: otlp-http
    port: 4318
    targetPort: 4318
  - name: prometheus
    port: 8889
    targetPort: 8889
EOF
```

### 3. Update Microservices to Send Telemetry

Add to microservice deployment env vars:

```yaml
env:
- name: OTEL_EXPORTER_OTLP_ENDPOINT
  value: "http://otel-collector:4317"
- name: OTEL_RESOURCE_ATTRIBUTES
  value: "service.name=patient-service,environment=production"
```

### 4. Persistent Storage (AWS Example)

```bash
# Create EBS volume for Prometheus storage
aws ec2 create-volume \
  --size 50 \
  --region us-east-1 \
  --availability-zone us-east-1a \
  --tag-specifications 'ResourceType=volume,Tags=[{Key=Name,Value=prometheus-storage}]'

# Create PersistentVolume in K8s
kubectl apply -f - <<EOF
apiVersion: v1
kind: PersistentVolume
metadata:
  name: prometheus-pv
spec:
  capacity:
    storage: 50Gi
  accessModes:
    - ReadWriteOnce
  awsElasticBlockStore:
    volumeID: vol-xxxxxx
    fsType: ext4
EOF
```

---

## 🧪 Testing & Troubleshooting

### Test Prometheus Scrape

```bash
# Check if Prometheus is scraping targets
curl http://localhost:9090/api/v1/targets | jq '.data.activeTargets'

# Check alert status
curl http://localhost:9090/api/v1/alerts | jq '.data.alerts'
```

### Test OpenTelemetry Collector

```bash
# Send sample span to collector
curl -X POST http://localhost:4318/v1/traces \
  -H "Content-Type: application/json" \
  -d '{
    "resourceSpans": [{
      "resource": {
        "attributes": [
          {"key": "service.name", "value": {"stringValue": "test-service"}}
        ]
      },
      "scopeSpans": [{
        "spans": [{
          "traceId": "1234567890abcdef",
          "spanId": "fedcba0987654321",
          "name": "test-span",
          "startTimeUnixNano": 1234567890000000000,
          "endTimeUnixNano": 1234567895000000000
        }]
      }]
    }]
  }'

# Check Jaeger UI
curl http://localhost:16686/api/traces?service=test-service
```

### Debug Alert Not Firing

1. **Check Prometheus rule evaluation**:
   ```bash
   curl http://localhost:9090/api/v1/rules | jq '.data.groups'
   ```

2. **Manually test PromQL query**:
   ```bash
   curl "http://localhost:9090/api/v1/query?query=http_requests_total"
   ```

3. **Check AlertManager routing**:
   ```bash
   curl http://localhost:9093/api/v1/status | jq '.data.config'
   ```

4. **View AlertManager logs**:
   ```bash
   docker compose logs alertmanager
   ```

### High Memory Usage in Prometheus

**Symptoms**: Prometheus container using 2GB+

**Solutions**:
1. Reduce retention: `--storage.tsdb.retention.time=7d` (default 15d)
2. Reduce scrape frequency: `scrape_interval: 30s` (default 15s)
3. Remove unused metrics: Comment out scrape jobs in `prometheus.yml`

---

## 📚 Configuration File Reference

| File | Purpose | Edit When |
|------|---------|-----------|
| `prometheus.yml` | Scrape targets, alert rules | Adding new services, changing scrape interval |
| `otel-collector.yml` | Telemetry receiver/processor | Redacting new PHI fields, changing export backends |
| `alertmanager.yml` | Alert routing rules | Adding Slack/PagerDuty channels, adjusting thresholds |
| `alert-rules/ehr-alerts.yml` | Alert definitions | Adding new alerts, adjusting thresholds |
| `grafana/dashboards/*.json` | Visualization panels | Creating custom dashboards |

---

## 🔐 Security Best Practices

### 1. Secure Alertmanager Credentials

**Never commit secrets to Git**:
```bash
# ❌ WRONG - Do not do this
slack_api_url: "https://hooks.slack.com/services/XXXX"

# ✅ RIGHT - Use environment variables
slack_api_url: "$SLACK_WEBHOOK_URL"
```

Set in `.env`:
```bash
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/XXXX
```

### 2. Restrict Prometheus/Grafana Access

In production, use authentication + HTTPS:
```bash
# Generate htpasswd file
htpasswd -c prometheus.htpasswd admin

# Use with reverse proxy (nginx)
location /prometheus {
  auth_basic "Prometheus";
  auth_basic_user_file /etc/nginx/prometheus.htpasswd;
  proxy_pass http://prometheus:9090;
}
```

### 3. Network Policies (Kubernetes)

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: prometheus-ingress
  namespace: monitoring
spec:
  podSelector:
    matchLabels:
      app: prometheus
  policyTypes:
  - Ingress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: ingress-nginx
    ports:
    - protocol: TCP
      port: 9090
```

---

## 📖 Next Steps

- **Dashboard customization**: See `Grafana-Dashboard-Guide.md`
- **Understanding queries**: See `README.md` for PromQL examples
- **DevOps setup**: See `devops/README.md` for full infrastructure setup
