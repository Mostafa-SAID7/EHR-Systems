# Grafana Dashboard Guide

Complete documentation of the EHR Platform Grafana dashboard (`ehr-overview.json`), including panel descriptions, PromQL queries, and customization instructions.

---

## 📊 Dashboard Overview

**Dashboard**: `devops/monitoring/grafana/dashboards/ehr-overview.json`

**Purpose**: Real-time visibility into application performance (RED method) and infrastructure health (USE method).

**Refresh Rate**: 30 seconds (configurable)

**Time Range**: Last 1 hour (adjustable via time picker)

---

## 🎯 Dashboard Panels

### Row 1: Application Health (RED Method)

#### Panel 1.1: Request Rate (Requests/sec)

**Description**: Green line showing the number of HTTP requests being processed per second across all services.

**PromQL Query**:
```promql
sum(rate(http_requests_total[5m])) by (service)
```

**Visualization**: Line graph with time series

**What It Means**:
- **Spike**: Increased traffic (expected during peak hours)
- **Drop**: Outage or service restart
- **Steady**: Normal operation

**Thresholds**:
- Green (0-100 req/s): Healthy
- Yellow (100-500 req/s): High but normal
- Red (500+ req/s): Potential bottleneck

**Action If Alert**:
- Check if legitimate traffic spike (marketing campaign, backup job)
- If unexpected, scale up services horizontally

---

#### Panel 1.2: Error Rate (5xx/4xx %)

**Description**: Red area chart showing the percentage of requests returning 5xx (server errors) or 4xx (client errors) responses.

**PromQL Query**:
```promql
(
  sum(rate(http_requests_total{status=~"5.."}[5m])) by (service)
  /
  sum(rate(http_requests_total[5m])) by (service)
) * 100
```

**Visualization**: Area graph (stacked by service)

**What It Means**:
- **< 0.5%**: Normal error rate
- **0.5% - 2%**: Acceptable but monitor
- **> 2%**: **Alert triggered** (HighErrorRate)

**Thresholds**:
- Green (0-0.5%): Excellent
- Yellow (0.5%-2%): Warning
- Red (>2%): Critical

**Action If Alert**:
- Check error logs: `docker compose logs identity-service | grep ERROR`
- Check Jaeger for failed traces
- Verify downstream dependencies (database, external APIs)

**Common Causes**:
- Database connection pool exhaustion
- Missing environment variables
- Invalid input from clients
- Downstream service failure

---

#### Panel 1.3: Latency Heatmap (P50/P95/P99)

**Description**: Stacked area chart showing response latencies at different percentiles:
- **Blue (P50)**: 50% of requests respond in this time (median)
- **Yellow (P95)**: 95% of requests respond in this time
- **Red (P99)**: 99% of requests respond in this time

**PromQL Queries**:
```promql
# P50 (Median)
histogram_quantile(0.50, sum(rate(http_request_duration_seconds_bucket[5m])) by (le, service))

# P95
histogram_quantile(0.95, sum(rate(http_request_duration_seconds_bucket[5m])) by (le, service))

# P99
histogram_quantile(0.99, sum(rate(http_request_duration_seconds_bucket[5m])) by (le, service))
```

**Visualization**: Stacked area graph (time series)

**What It Means**:
- **P50 = 50ms, P99 = 200ms**: Fast response, good UX
- **P50 = 100ms, P99 = 2000ms**: Wide gap = slow outliers
- **All increasing**: System degradation

**Thresholds**:
- Green: P99 < 1000ms
- Yellow: P99 1000-2000ms
- Red: P99 > 2000ms (**Alert triggered**)

**Action If Alert**:
- Check for slow database queries (see Database panels)
- Profile CPU/memory usage
- Check Jaeger for bottleneck services
- Review recent code deployments

---

#### Panel 1.4: Service Status (Health Check)

**Description**: Table showing up/down status of each microservice.

**PromQL Query**:
```promql
up{job="ehr-services"}
```

**Visualization**: Table with color-coded status
- **Green (1)**: Service UP
- **Red (0)**: Service DOWN

**Services Monitored**:
- API Gateway
- Identity Service
- Patient Service
- Clinical Service
- Appointment Service
- Notification Service
- Audit Service
- Billing Service
- Prescription Service
- Analytics Service

**Action If Service DOWN**:
1. Check service logs: `docker compose logs <service-name>`
2. Check if container is running: `docker ps | grep <service-name>`
3. Restart if needed: `docker compose restart <service-name>`
4. Check PagerDuty for incidents

---

### Row 2: Database Performance

#### Panel 2.1: Database Connections

**Description**: Line graph showing active database connections vs. max pool size.

**PromQL Queries**:
```promql
# Active connections
pg_stat_activity_count

# Max connections (threshold)
pg_settings_max_connections
```

**Visualization**: Line graph with threshold line

**What It Means**:
- **Flat near bottom**: Normal, plenty of capacity
- **Climbing toward top**: Risk of connection pool exhaustion
- **Spikes**: Transient connection storms

**Thresholds**:
- Green: 0-50% of pool
- Yellow: 50-80% of pool
- Red: > 80% (**Alert triggered**)

**Action If Alert**:
- Check for connection leaks: unused connections not closed
- Increase pool size: Update connection string `Max Pool Size=100`
- Scale database replicas for read-heavy workloads

---

#### Panel 2.2: Slow Query Log

**Description**: Table showing SQL queries taking longer than 1 second to execute.

**PromQL Query**:
```promql
pg_slow_queries
```

**Visualization**: Table with query text, duration, execution count

**What It Means**:
- Empty = No slow queries (good)
- Few queries: Normal, optimize those specific queries
- Growing count: Possible regression, investigate recent code

**Common Slow Queries**:
- `SELECT * FROM patients WHERE ...` (missing index)
- `SELECT * FROM prescriptions JOIN appointments ...` (complex join)
- Bulk inserts without batch processing

**Action**:
1. Identify slow query: `SELECT query, mean_time FROM pg_stat_statements ORDER BY mean_time DESC`
2. Add index: `CREATE INDEX idx_patients_name ON patients(name)`
3. Rewrite query to use join hints
4. Consider denormalization if appropriate

---

#### Panel 2.3: Replication Lag (Standby)

**Description**: Line graph showing seconds of lag on standby replica.

**PromQL Query**:
```promql
pg_replication_lag_seconds
```

**Visualization**: Line graph with warning threshold

**What It Means**:
- **0 seconds**: Perfect replication (unlikely in practice)
- **< 1 second**: Excellent, acceptable for HA
- **1-10 seconds**: Normal, manageable
- **> 60 seconds**: **Alert triggered** (ReplicationLag)

**Action If Alert**:
- Check standby server CPU/disk I/O: `iostat 1 5`
- Check network latency: `ping standby-host`
- Reduce write load on primary if possible
- Upgrade standby hardware

---

### Row 3: Message Queue Performance (Kafka)

#### Panel 3.1: Kafka Consumer Lag

**Description**: Line graph showing number of messages queued per consumer group.

**PromQL Query**:
```promql
sum(kafka_consumer_lag) by (consumer_group, topic)
```

**Visualization**: Line graph (one line per consumer group)

**What It Means**:
- **0 messages**: Consumers keeping up (ideal)
- **Growing**: Producers faster than consumers
- **Plateau at high number**: Consumer crashed or paused

**Thresholds**:
- Green: < 1000 messages
- Yellow: 1000-10000 messages
- Red: > 10000 messages (**Alert triggered**)

**Common Causes**:
- Consumer crashed: Check logs, restart if needed
- Consumer overwhelmed: Scale up consumer replicas
- Producer backpressure: Slow producer down or add topics

**Action If Alert**:
1. Check consumer status: `kafka-consumer-groups.sh --group <group> --describe`
2. Restart stuck consumers: `docker compose restart notification-service`
3. Scale consumers: Add `replicas: 3` in Docker Compose
4. Monitor producer rate vs consumer processing capacity

---

#### Panel 3.2: Kafka Throughput (Messages/sec)

**Description**: Area chart showing message publish rate and consumption rate.

**PromQL Query**:
```promql
sum(rate(kafka_producer_messages_total[5m])) by (topic) # Published
sum(rate(kafka_consumer_messages_total[5m])) by (topic) # Consumed
```

**Visualization**: Area graph (stacked)

**What It Means**:
- **Producer > Consumer**: Queue growing (lag increasing)
- **Consumer catching up**: Queue shrinking (lag decreasing)
- **Equal**: Steady state, in equilibrium

---

### Row 4: Cache Performance (Redis)

#### Panel 4.1: Redis Memory Usage

**Description**: Gauge showing Redis memory as percentage of max.

**PromQL Query**:
```promql
(redis_memory_used_bytes / redis_memory_max_bytes) * 100
```

**Visualization**: Gauge with thresholds

**What It Means**:
- **< 50%**: Healthy, plenty of cache space
- **50-80%**: Watch for growth
- **80-90%**: Getting full, consider cleanup
- **> 90%**: **Alert triggered** (RedisHighMemory)

**Action If Alert**:
1. Increase maxmemory: Update Redis config `maxmemory 2gb` (was 1gb)
2. Review eviction policy: `CONFIG GET maxmemory-policy` (e.g., `allkeys-lru`)
3. Check cache hit rate to verify cache is effective
4. Consider separating caches by TTL (short-lived vs persistent)

---

#### Panel 4.2: Redis Hit Rate

**Description**: Percentage of cache lookups that returned cached values.

**PromQL Query**:
```promql
(
  sum(redis_keyspace_hits_total)
  /
  (sum(redis_keyspace_hits_total) + sum(redis_keyspace_misses_total))
) * 100
```

**Visualization**: Gauge

**What It Means**:
- **> 80%**: Excellent cache effectiveness
- **50-80%**: Good caching, room for improvement
- **< 50%**: Cache not effective, review cache strategy

**Action If Low**:
- Increase TTL on cached items
- Cache more frequently-used queries
- Review what's being cached vs what could be

---

### Row 5: Infrastructure Metrics (USE Method)

#### Panel 5.1: CPU Utilization (%)

**Description**: Line graph showing CPU usage on the host/container.

**PromQL Query**:
```promql
100 - (avg(rate(node_cpu_seconds_total{mode="idle"}[5m])) * 100)
```

**Visualization**: Line graph with thresholds

**What It Means**:
- **< 50%**: Good utilization, capacity available
- **50-70%**: Normal load
- **70-85%**: High but manageable
- **> 85%**: **Alert triggered** (HighCPU)

**Action If Alert**:
- Check running processes: `top` or `ps aux`
- Profile CPU usage: Identify hot code paths
- Scale horizontally: Add more containers/nodes
- Optimize expensive operations (algorithms, database queries)

---

#### Panel 5.2: Memory Utilization (%)

**Description**: Line graph showing RAM usage.

**PromQL Query**:
```promql
(1 - (node_memory_MemAvailable_bytes / node_memory_MemTotal_bytes)) * 100
```

**Visualization**: Line graph with thresholds

**What It Means**:
- **< 60%**: Healthy
- **60-80%**: Normal, monitor
- **80-90%**: High, may see slowdown from swapping
- **> 85%**: **Alert triggered** (HighMemory), risk of OOM

**Action If Alert**:
- Check memory leaks: Are memory-hungry processes accumulating?
- Restart containers if suspected leak: `docker compose restart identity-service`
- Review garbage collection logs
- Increase container memory limit in Docker Compose
- Consider memory profiling if persistent

---

#### Panel 5.3: Disk Usage (%)

**Description**: Line graph showing disk space usage on root filesystem.

**PromQL Query**:
```promql
(1 - (node_filesystem_avail_bytes{mountpoint="/"} / node_filesystem_size_bytes{mountpoint="/"})) * 100
```

**Visualization**: Line graph with thresholds

**What It Means**:
- **< 70%**: Safe
- **70-80%**: Monitor growth
- **80-90%**: **Alert triggered** (HighDisk), cleanup needed
- **> 90%**: **Critical alert** (DiskFull), immediate action required

**Common Causes**:
- Docker image layers not cleaned: `docker image prune -a`
- Old logs accumulating: `docker logs <container> | wc -l`
- Database dump files: Check `/var/lib/postgresql/backup/`

**Action**:
1. Find large files: `du -sh /* | sort -h`
2. Delete old Docker images: `docker image prune -a`
3. Clear Docker container logs: `find /var/lib/docker -name "*-json.log" -delete`
4. Compress old database backups
5. Expand filesystem if needed

---

### Row 6: Business Metrics

#### Panel 6.1: Appointments Scheduled (Daily)

**Description**: Bar chart showing number of appointments created per day.

**PromQL Query**:
```promql
sum(increase(appointments_created_total[1d])) by (date)
```

**Visualization**: Bar chart

**What It Means**:
- Trend indicator of platform usage
- Spikes may indicate marketing campaigns or user growth
- Drops may indicate technical issues or business problem

---

#### Panel 6.2: Prescriptions Filled (Daily)

**Description**: Bar chart showing number of prescriptions processed per day.

**PromQL Query**:
```promql
sum(increase(prescriptions_filled_total[1d])) by (date)
```

**Visualization**: Bar chart

---

#### Panel 6.3: Billing Transactions (Daily)

**Description**: Bar chart with transaction count and revenue.

**PromQL Queries**:
```promql
sum(increase(billing_transactions_total[1d])) # Count
sum(increase(billing_revenue_total[1d])) # Revenue in $
```

**Visualization**: Bar + line combo chart

---

### Row 7: System Alerts Status

#### Panel 7.1: Active Alerts

**Description**: Table showing all currently active (firing) alerts with severity.

**PromQL Query**:
```promql
ALERTS{alertstate="firing"}
```

**Visualization**: Table with columns:
- Alert Name
- Severity (critical/warning)
- Service
- Duration firing
- Summary

**Action If Any Alert**:
1. Click alert name to see full details
2. Check AlertManager routing: Are correct recipients notified?
3. Investigate root cause using other panels
4. Manually silence if false positive: AlertManager UI → Alerts → Silence

---

#### Panel 7.2: Alert History (24h)

**Description**: Time series showing count of alerts fired over last 24 hours.

**PromQL Query**:
```promql
sum(increase(ALERTS_FOR_STATE{alertstate="firing"}[1h])) by (alertname)
```

**Visualization**: Stacked area chart

**What It Means**:
- Spikes: Indicates when incidents occurred
- Flat: Stable operation
- Growing trend: Something degrading over time

---

## 🛠️ Customizing the Dashboard

### Add a New Panel

1. **Click "Add panel" button** (top right)
2. **Choose visualization type**:
   - Line Graph: Time series trends
   - Gauge: Single value with thresholds
   - Table: Structured data
   - Stat: Large number display
   - Bar Chart: Categorical comparison
   - Heatmap: 2D distribution

3. **Enter PromQL query**
4. **Configure thresholds** (color zones)
5. **Set title and description**
6. **Save dashboard**

### Example: Add "Appointments by Status" Panel

```promql
sum by (status) (appointments_total)
```

**Visualization**: Pie chart
**Thresholds**: None
**Title**: "Appointments by Status"

---

## 📈 Creating Custom Dashboards

### Scenario: Team-Specific Dashboard

**Create dashboard for Billing Team**:

1. Grafana → Dashboards → New
2. Add panels:
   - Transactions per hour
   - Revenue trend (7-day moving average)
   - Payment gateway failures
   - Refund rate
   - Invoice processing time (P95)

3. Save as "Billing Team Dashboard"

**PromQL Queries**:
```promql
# Transactions per hour
sum(rate(billing_transactions_total[1h])) by (payment_method)

# Revenue trend
sum(billing_revenue_total) - sum(billing_refunds_total)

# Payment gateway failures
sum(rate(payment_gateway_errors_total[5m]))

# Invoice processing time (P95)
histogram_quantile(0.95, sum(rate(invoice_processing_time_seconds_bucket[5m])) by (le))
```

---

## 🎨 Dashboard Best Practices

### Organization
- Group related panels into rows
- Use descriptive titles and descriptions
- Order by importance (critical metrics first)

### Color Coding
- Red: Errors, failures, critical alerts
- Yellow: Warnings, degradation
- Green: Healthy, good performance
- Blue: Information, neutral data

### Refresh Rate
- 30s: Real-time monitoring dashboards
- 5m: Long-term trend dashboards
- 1h: Historical analysis dashboards

### Time Range
- 1h: Current operations (default)
- 24h: Daily trends
- 7d: Weekly patterns
- 30d: Monthly comparison

---

## 📚 Related Documentation

- **Setup**: See `Configuration-Guide.md` for importing pre-built dashboard
- **Alert Definitions**: See `devops/monitoring/alert-rules/ehr-alerts.yml`
- **PromQL Reference**: See Prometheus official documentation
- **Main Monitoring Guide**: See `README.md`
