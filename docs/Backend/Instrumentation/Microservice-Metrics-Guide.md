# Microservice Metrics Instrumentation Guide

Complete guide for OpenTelemetry metrics instrumentation across all EHR Platform microservices.

---

## 📊 Overview

Every microservice in the EHR Platform is instrumented with OpenTelemetry metrics and exposes a `/metrics` endpoint for Prometheus scraping.

**All 10 microservices instrumented**:
- ✅ API Gateway
- ✅ Identity Service
- ✅ Patient Service
- ✅ Clinical Service
- ✅ Appointment Service
- ✅ Notification Service
- ✅ Billing Service
- ✅ Prescription Service
- ✅ Audit Service
- ✅ Analytics Service

---

## 🔧 Architecture

### Metric Collection Flow

```
┌──────────────────────────────────────────────────────────┐
│ Microservice (any of 10 services)                        │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  OpenTelemetry Instrumentation                          │
│  ├─ ASP.NET Core (HTTP requests)                       │
│  ├─ HTTP Client (outbound calls)                       │
│  ├─ Runtime (.NET GC, memory, threads)                 │
│  └─ Process (CPU, memory usage)                        │
│                                                          │
│  Metrics Exported via: /metrics endpoint                │
│  Format: Prometheus text format                         │
│                                                          │
└──────────────────────────────────────────────────────────┘
         ↓
    Prometheus scrapes /metrics every 15s
         ↓
    Prometheus TSDB stores time-series data
         ↓
    Grafana queries Prometheus for visualization
```

---

## 📈 Metrics Collected Per Service

### 1. HTTP Metrics (ASP.NET Core)

**Collected automatically** when requests flow through the service.

```
http.request.duration
├─ Label: method (GET, POST, PUT, DELETE)
├─ Label: route (/api/patients, /api/appointments, etc.)
├─ Label: status_code (200, 400, 500, etc.)
├─ Label: service (identity-service, patient-service, etc.)
└─ Type: Histogram (P50, P95, P99 latencies)

http.request.body.size
├─ Size of incoming request body
└─ Type: Histogram

http.response.body.size
├─ Size of outgoing response body
└─ Type: Histogram
```

**Example PromQL queries**:
```promql
# Request rate per service
rate(http_requests_total[5m]) by (service)

# Error rate by service
sum(rate(http_requests_total{status=~"5.."}[5m])) by (service)
  / sum(rate(http_requests_total[5m])) by (service)

# P95 latency by route
histogram_quantile(0.95, 
  sum(rate(http_request_duration_seconds_bucket[5m])) by (route, le))
```

---

### 2. Runtime Metrics (.NET CLR)

**Garbage Collection & Memory**

```
dotnet.gc.collections.count
├─ Counter: number of GC collections by generation
├─ Label: generation (gen0, gen1, gen2)
└─ Shows: garbage collection pressure

dotnet.gc.objects.collected
├─ Histogram: bytes collected per GC
└─ Shows: memory pressure

dotnet.mem.committed
├─ Gauge: committed memory in bytes
└─ Shows: current memory usage

dotnet.gc.last_collection.pause_duration
├─ Histogram: GC pause time
└─ Shows: STW (stop-the-world) pause impact
```

**Example PromQL queries**:
```promql
# Memory usage trending
dotnet_mem_committed_bytes by (service)

# GC collection rate
rate(dotnet_gc_collections_total[5m]) by (generation, service)

# GC pause times (max)
max(dotnet_gc_last_collection_pause_duration_seconds) by (service)
```

---

### 3. Process Metrics (System)

**CPU & System Resource Usage**

```
process.cpu.time
├─ Counter: accumulated CPU time
└─ Shows: total CPU work

process.cpu.utilization
├─ Gauge: % CPU cores in use
└─ Shows: immediate CPU load

process.memory.physical_usage_bytes
├─ Gauge: physical RAM used
└─ Shows: resident set size (RSS)

process.memory.virtual_usage_bytes
├─ Gauge: virtual memory used
└─ Shows: total address space

process.disk.operations
├─ Counter: disk I/O operations
└─ Shows: disk activity

process.disk.io_bytes
├─ Counter: total disk bytes read/written
└─ Shows: disk throughput
```

**Example PromQL queries**:
```promql
# CPU utilization by service
process_cpu_utilization by (service)

# Memory by service (RSS)
process_resident_memory_bytes by (service)

# Disk I/O rate
rate(process_disk_io_bytes_total[5m]) by (service)
```

---

### 4. ASP.NET Core Specific Metrics

**Connection & Request State**

```
aspnetcore.http.connections.open
├─ Gauge: active HTTP connections
└─ Shows: concurrent connection count

aspnetcore.http.requests.active
├─ Gauge: currently-processing requests
└─ Shows: request concurrency

aspnetcore.routing.match_attempts
├─ Counter: route matching attempts
└─ Shows: routing overhead
```

---

## 🔌 Implementation Details

### Extension Method: `AddOpenTelemetryMetrics()`

Located in: `backend/src/EHRPlatform.Common/Extensions/OpenTelemetryExtensions.cs`

**Usage in Program.cs**:
```csharp
// Add after Serilog/logging setup
builder.Services.AddOpenTelemetryMetrics("service-name");

// Add in endpoint mapping (before app.RunAsync())
app.MapPrometheusMetricsEndpoint();
```

### What the Extension Does

```csharp
public static IServiceCollection AddOpenTelemetryMetrics(
    this IServiceCollection services,
    string serviceName)
{
    services.AddOpenTelemetry()
        .WithMetrics(metrics =>
        {
            metrics
                // HTTP request metrics (automatically)
                .AddAspNetCoreInstrumentation(options =>
                {
                    // Exclude /health and /metrics from metrics (reduce noise)
                    options.Filter = context =>
                        !context.Request.Path.StartsWithSegments("/health") &&
                        !context.Request.Path.StartsWithSegments("/metrics");
                })
                
                // Outbound HTTP calls to other services/APIs
                .AddHttpClientInstrumentation()
                
                // .NET runtime: GC, memory, threads
                .AddRuntimeInstrumentation()
                
                // Process: CPU, memory, disk I/O
                .AddProcessInstrumentation()
                
                // Export to Prometheus format on /metrics
                .AddPrometheusExporter();
        });
    
    return services;
}
```

### Filtering `/metrics` and `/health`

**Why we filter**:
- `/metrics` endpoint itself causes metric collection → infinite loop feedback
- `/health` endpoint called constantly → noisy baseline data
- Filtering keeps metrics meaningful and reduces cardinality

**Without filtering** (❌ don't do this):
```
http_requests_total{service="patient-service", route="/metrics", method="GET", status="200"}
http_requests_total{service="patient-service", route="/health", method="GET", status="200"}
```
→ 1000s of scrapes per hour = noise

**With filtering** (✅ correct):
```
http_requests_total{service="patient-service", route="/api/patients", method="GET", status="200"}
http_requests_total{service="patient-service", route="/api/patients/{id}", method="POST", status="201"}
```
→ Only business-relevant metrics

---

## 📍 Accessing Metrics

### From Docker Compose (Local Development)

```bash
# Start services
docker compose -f devops/docker/docker-compose.yml up -d

# Get metrics from Identity Service
curl http://localhost:5001/metrics

# Get metrics from Patient Service
curl http://localhost:5002/metrics

# Get metrics from API Gateway
curl http://localhost:5000/metrics
```

### Response Format (Prometheus Text Format)

```
# HELP http_request_duration_seconds HTTP request duration in seconds
# TYPE http_request_duration_seconds histogram
http_request_duration_seconds_bucket{service="patient-service", route="/api/patients", le="0.005"} 10
http_request_duration_seconds_bucket{service="patient-service", route="/api/patients", le="0.01"} 25
http_request_duration_seconds_bucket{service="patient-service", route="/api/patients", le="1"} 247
http_request_duration_seconds_bucket{service="patient-service", route="/api/patients", le="+Inf"} 250
http_request_duration_seconds_sum{service="patient-service", route="/api/patients"} 125.43
http_request_duration_seconds_count{service="patient-service", route="/api/patients"} 250
```

---

## 🚀 Adding Custom Metrics to a Service

If you need to track custom business metrics (e.g., "patients created per hour", "billing transactions processed"):

### Step 1: Create a Meter (in your service)

```csharp
using System.Diagnostics.Metrics;

namespace EHRPlatform.Services.Patient.Metrics;

public static class PatientMetrics
{
    private static readonly Meter Meter = new("patient-service", "1.0.0");
    
    // Counter: increments monotonically
    public static readonly Counter<int> PatientCreatedCounter = 
        Meter.CreateCounter<int>(
            "patient.created.total",
            description: "Total patients created");
    
    // Gauge: can go up/down
    public static readonly ObservableGauge<int> ActivePatientsGauge =
        Meter.CreateObservableGauge<int>(
            "patient.active.count",
            description: "Active patient count");
    
    // Histogram: measures distribution
    public static readonly Histogram<double> RegistrationTimeHistogram =
        Meter.CreateHistogram<double>(
            "patient.registration.duration_seconds",
            description: "Patient registration duration");
}
```

### Step 2: Record Metrics in Your Handlers

```csharp
public class CreatePatientCommandHandler : ICommandHandler<CreatePatientCommand, int>
{
    public async Task<int> Handle(CreatePatientCommand command, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        
        // Your business logic
        var patientId = await _repository.CreatePatientAsync(command.Patient, ct);
        
        // Record metrics
        PatientMetrics.PatientCreatedCounter.Add(1);
        PatientMetrics.RegistrationTimeHistogram.Record(sw.Elapsed.TotalSeconds);
        
        return patientId;
    }
}
```

### Step 3: Register the Meter

The Meter is automatically registered if you use standard OpenTelemetry SDK. Verify it appears in `/metrics`:

```bash
curl http://localhost:5002/metrics | grep patient_created
```

Expected output:
```
# HELP patient_created_total Total patients created
# TYPE patient_created_total counter
patient_created_total{service="patient-service"} 42
```

---

## 🔍 Monitoring Metrics in Grafana

### Pre-built Dashboards

**Dashboard**: `devops/monitoring/grafana/dashboards/ehr-overview.json`

**Panels using microservice metrics**:
1. **Request Rate** - sum all HTTP request rates
2. **Error Rate** - % of 5xx responses
3. **P95/P99 Latency** - histogram_quantile across all services
4. **Service Status** - health check on all 10 services
5. **CPU/Memory** - process metrics by service
6. **GC Collections** - dotnet.gc_* metrics

### Query Examples for Custom Dashboard

**Request rate by service**:
```promql
sum by (service) (rate(http_requests_total[5m]))
```

**Error rate by service**:
```promql
sum by (service) (rate(http_requests_total{status=~"5.."}[5m]))
  / sum by (service) (rate(http_requests_total[5m]))
```

**P99 latency by service**:
```promql
histogram_quantile(0.99, 
  sum by (service, le) (rate(http_request_duration_seconds_bucket[5m])))
```

**Memory usage by service**:
```promql
process_resident_memory_bytes by (service)
```

---

## 🚨 Alerting on Metrics

All 22 alert rules are defined in `devops/monitoring/alert-rules/ehr-alerts.yml`.

**Example alert that uses microservice metrics**:
```yaml
- alert: HighErrorRate
  expr: |
    (
      sum(rate(http_requests_total{status=~"5.."}[5m])) by (service)
      /
      sum(rate(http_requests_total[5m])) by (service)
    ) > 0.02
  for: 5m
  labels:
    severity: critical
    service: "{{ $labels.service }}"
  annotations:
    summary: "High error rate on {{ $labels.service }}"
    description: "Error rate is {{ $value | humanizePercentage }} (threshold: 2%)"
```

---

## ✅ Verification Checklist

After deploying, verify all services expose metrics:

```bash
#!/bin/bash
services=(
  "http://localhost:5000/metrics"    # API Gateway
  "http://localhost:5001/metrics"    # Identity Service
  "http://localhost:5002/metrics"    # Patient Service
  "http://localhost:5003/metrics"    # Clinical Service
  "http://localhost:5004/metrics"    # Appointment Service
  "http://localhost:5005/metrics"    # Notification Service
  "http://localhost:5006/metrics"    # Billing Service
  "http://localhost:5007/metrics"    # Prescription Service
  "http://localhost:5008/metrics"    # Audit Service
  "http://localhost:5009/metrics"    # Analytics Service
)

for service in "${services[@]}"; do
  echo "Testing $service..."
  curl -s "$service" | head -20
  echo ""
done
```

Expected response (first 20 lines):
```
# HELP process_cpu_utilization Process CPU utilization
# TYPE process_cpu_utilization gauge
process_cpu_utilization{service="patient-service"} 0.05

# HELP process_resident_memory_bytes Process resident memory
# TYPE process_resident_memory_bytes gauge
process_resident_memory_bytes{service="patient-service"} 267456000

# HELP http_request_duration_seconds HTTP request duration
# TYPE http_request_duration_seconds histogram
http_request_duration_seconds_bucket{service="patient-service", le="0.005"} 5
...
```

---

## 📚 Related Documentation

- **Monitoring Setup**: `docs/Backend/Monitoring/README.md` — Architecture, concepts
- **Configuration Guide**: `docs/Backend/Monitoring/Configuration-Guide.md` — How to configure Prometheus
- **Grafana Dashboard**: `docs/Backend/Monitoring/Grafana-Dashboard-Guide.md` — Dashboard panels reference
- **Alert Rules**: `devops/monitoring/alert-rules/ehr-alerts.yml` — All 22+ alert definitions
- **OpenTelemetry Extension**: `backend/src/EHRPlatform.Common/Extensions/OpenTelemetryExtensions.cs`

---

## 🔗 See Also

- [OpenTelemetry .NET Documentation](https://github.com/open-telemetry/opentelemetry-dotnet)
- [Prometheus Format Specification](https://prometheus.io/docs/instrumenting/exposition_formats/)
- [PromQL Query Language](https://prometheus.io/docs/prometheus/latest/querying/basics/)
