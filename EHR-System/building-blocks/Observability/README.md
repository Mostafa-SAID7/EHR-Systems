# Observability Package

Monitoring, tracing, health checks, and logging.

## Contents (36 files)

### Health Checks (12 files)
- `IHealthCheckService.cs` - Health check registry
- `HealthCheckResult.cs` - Check result data
- `HealthCheckRegistry.cs` - Registry service
- Database checks: PostgreSQL, MySQL, MongoDB
- Cache checks: Redis
- Message checks: RabbitMQ, Kafka
- Search checks: Elasticsearch
- Service checks: HTTP health

### Logging (6 files)
- `ILogService.cs` - Logging contract
- `LogLevel.cs` - Severity enumeration
- `LogEntry.cs` - Log message structure
- `ILogProvider.cs` - Provider abstraction
- `IStructuredLogger.cs` - Structured logging
- `LogContext.cs` - Contextual logging

### Telemetry (5 files)
- `ITelemetryService.cs` - Telemetry collection
- `Metric.cs` - Metric data
- `IMetricsCollector.cs` - Metrics collection
- `IMeter.cs` - Meter abstraction
- `ICounter.cs` - Counter metric

### Tracing (7 files)
- `ITracingService.cs` - Tracing context
- `ISpanBuilder.cs` - Span builder
- `ISpan.cs` - Active span
- `SpanContext.cs` - Span context
- `SpanKind.cs` - Span types
- `SpanStatus.cs` - Span status
- `LogEntry.cs` - Log entry

### Performance (6 files)
- `IPerformanceMonitor.cs` - Performance tracking
- `IProfiler.cs` - Code profiling
- `PerformanceMetrics.cs` - Metrics data
- `ProfileSnapshot.cs` - Snapshot data

---

## Usage

```csharp
using EHRPlatform.Observability.Logging;
using EHRPlatform.Observability.Telemetry;
using EHRPlatform.Observability.HealthChecks;
```

## Parent

[← Building Blocks](../README.md)
