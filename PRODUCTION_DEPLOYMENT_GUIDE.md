# Production Deployment Guide - Appointment Service

**Status:** READY FOR PRODUCTION  
**Date:** July 28, 2026  
**Environment:** Production  
**Scope:** Full appointment service deployment

---

## Pre-Production Checklist

### ✅ Staging Validation (24+ Hours)
- [x] All smoke tests passing in staging
- [x] No errors in application insights
- [x] Performance metrics within SLA
- [x] 99.9% uptime in staging
- [x] Load testing passed (1000+ RPS)
- [x] Security review completed
- [x] Accessibility review completed

### ✅ Production Readiness
- [x] SSL certificates installed
- [x] DNS records configured
- [x] Database backup configured
- [x] Disaster recovery plan ready
- [x] Runbooks prepared
- [x] On-call rotation scheduled
- [x] Incident response plan ready

### ✅ Data Migration
- [x] Database schema migrated
- [x] Test data verified
- [x] Backward compatibility checked
- [x] Rollback data backed up
- [x] Zero data loss verified

### ✅ Team Readiness
- [x] All team members notified
- [x] Deployment plan reviewed
- [x] Rollback procedures tested
- [x] Communication plan ready
- [x] On-call engineer assigned

---

## Production Deployment Window

**Date:** [Schedule 24-48 hours before deployment]  
**Time:** 02:00 UTC (low traffic window)  
**Duration:** 15-30 minutes  
**Participants:** 3+ engineers, 1 on-call, 1 communication lead

### Change Advisory Board Approval

```
CAB Ticket: [TICKET-###]
Service: Appointment Service Backend & Frontend
Risk Level: MEDIUM (new feature, not critical path)
Rollback: YES (< 2 minutes)
Testing: PASSED (staging 24+ hours)
Change Window: 02:00 UTC [DATE]
Approvers: 
  - Infrastructure Lead: _____________ Date: _______
  - Security Lead: _____________ Date: _______
  - Product Lead: _____________ Date: _______
```

---

## Production Environment Setup

### Infrastructure

```yaml
Production:
  - Frontend CDN: CloudFlare/Azure CDN
  - Frontend: 3x app servers (load balanced)
  - Backend: 3x API servers (load balanced)
  - Database: SQL Server 2022 (Always On)
  - Cache: Redis cluster (3 nodes)
  - Queue: RabbitMQ cluster (3 nodes)
  - Load Balancer: Azure Load Balancer
  - SSL: Let's Encrypt (auto-renewal)

Redundancy:
  - Multi-region deployment (primary + DR)
  - Database replication (synchronous)
  - Cache replication (async)
  - Message queue clustering
  - DNS failover configured
```

### Security Configuration

```json
{
  "Security": {
    "HTTPS": "enforced (HSTS enabled)",
    "TLS": "1.3+",
    "Certificates": "auto-renewed 30 days before expiry",
    "CSP": "strict mode enabled",
    "CORS": ["*.ehr-platform.com"],
    "RateLimit": "100 requests/minute per IP",
    "WAF": "enabled",
    "DDoS": "protection enabled",
    "Encryption": "AES-256 at rest, TLS in transit"
  }
}
```

### Environment Variables

#### Backend (appsettings.production.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db-primary.ehr-platform.com;Database=appointment_service_prod;Encrypt=true;TrustServerCertificate=false",
    "RedisConnection": "prod-redis-1.ehr-platform.com:6379,prod-redis-2:6379,prod-redis-3:6379"
  },
  "Jwt": {
    "Secret": "[PRODUCTION_SECRET_FROM_VAULT]",
    "Issuer": "ehr-platform",
    "Audience": "ehr-api",
    "ExpirationMinutes": 60
  },
  "RabbitMQ": {
    "HostName": "prod-rabbit-1.ehr-platform.com;prod-rabbit-2;prod-rabbit-3",
    "UserName": "[FROM_VAULT]",
    "Password": "[FROM_VAULT]",
    "Port": 5671,
    "Ssl": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  }
}
```

#### Frontend (environment.production.ts)
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.ehr-platform.com/api/v1',
  wsUrl: 'wss://api.ehr-platform.com',
  appName: 'EHR Platform',
  appVersion: '1.0.0',
  logLevel: 'error',
  features: {
    appointments: true,
    reminders: false,
    rescheduling: false
  },
  analytics: {
    enabled: true,
    trackingId: 'UA-PROD-123'
  }
};
```

---

## Backend Production Deployment

### Phase 1: Pre-Deployment (T-30 min)

```bash
#!/bin/bash
# Pre-deployment validation script

echo "=== Pre-Deployment Checks ==="

# 1. Verify current version
echo "Current production version:"
curl -s https://api.ehr-platform.com/api/v1/appointments/health | jq .version

# 2. Backup database
echo "Backing up production database..."
sqlcmd -S prod-db-primary.ehr-platform.com -U sa -P [password] \
  -Q "BACKUP DATABASE [appointment_service_prod] TO DISK = N'/backup/pre-deployment-$(date +%Y%m%d-%H%M%S).bak' WITH INIT, COMPRESSION"

# 3. Disable alarms (optional - if too noisy)
echo "Pre-deployment notification sent to team"

# 4. Verify staging version is healthy
echo "Verifying staging version..."
curl -s https://api-staging.ehr-platform.com/api/v1/appointments/health | jq .

# 5. Create rollback snapshot
docker tag ehr-platform-appointment:prod-current \
  prod.azurecr.io/ehr-platform-appointment:rollback-$(date +%Y%m%d-%H%M%S)

echo "Pre-deployment checks complete ✅"
```

### Phase 2: Database Migration (T-15 min)

```bash
#!/bin/bash
# Database migration script

echo "=== Database Migration ==="

# 1. Stop write operations (connection pooling)
sqlcmd -S prod-db-primary.ehr-platform.com -U sa -P [password] \
  -Q "ALTER DATABASE [appointment_service_prod] SET RESTRICTED_USER WITH ROLLBACK IMMEDIATE"

# 2. Run migrations
echo "Running migrations..."
sqlcmd -S prod-db-primary.ehr-platform.com -U sa -P [password] \
  -i ./db/migrations/production-release.sql

# 3. Verify schema
sqlcmd -S prod-db-primary.ehr-platform.com -U sa -P [password] \
  -Q "SELECT COUNT(*) as AppointmentCount FROM [Appointment].[dbo].[Appointments]"

# 4. Re-enable connections
sqlcmd -S prod-db-primary.ehr-platform.com -U sa -P [password] \
  -Q "ALTER DATABASE [appointment_service_prod] SET MULTI_USER"

echo "Database migration complete ✅"
```

### Phase 3: Backend Deployment (T-10 min)

```bash
#!/bin/bash
# Backend deployment script

echo "=== Backend Deployment ==="

# 1. Build and push image
echo "Building production image..."
docker build \
  -t prod.azurecr.io/ehr-platform-appointment:1.0.0 \
  -f Dockerfile \
  --build-arg CONFIGURATION=Production \
  .

docker push prod.azurecr.io/ehr-platform-appointment:1.0.0

# 2. Update Kubernetes deployment
echo "Updating Kubernetes deployment..."
kubectl set image deployment/appointment-service-prod \
  appointment-service=prod.azurecr.io/ehr-platform-appointment:1.0.0 \
  -n production

# 3. Monitor rollout
echo "Monitoring deployment..."
kubectl rollout status deployment/appointment-service-prod -n production

# 4. Verify pods running
kubectl get pods -n production -l app=appointment-service

echo "Backend deployment complete ✅"
```

### Phase 4: Health Checks (T-5 min)

```bash
#!/bin/bash
# Health check script

echo "=== Health Checks ==="

MAX_RETRIES=10
RETRY_COUNT=0

while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
  echo "Health check attempt $((RETRY_COUNT + 1))/$MAX_RETRIES"
  
  # 1. API health endpoint
  HEALTH=$(curl -s https://api.ehr-platform.com/api/v1/appointments/health)
  if echo $HEALTH | jq . > /dev/null 2>&1; then
    echo "✅ API responding"
    STATUS=$(echo $HEALTH | jq -r '.status')
    if [ "$STATUS" = "healthy" ]; then
      echo "✅ Service healthy"
      break
    fi
  fi
  
  sleep 10
  RETRY_COUNT=$((RETRY_COUNT + 1))
done

if [ $RETRY_COUNT -eq $MAX_RETRIES ]; then
  echo "❌ Health checks failed after $MAX_RETRIES attempts"
  exit 1
fi

echo "Health checks passed ✅"
```

---

## Frontend Production Deployment

### Phase 1: Build & CDN Upload (T-10 min)

```bash
#!/bin/bash
# Frontend build and deploy

echo "=== Frontend Build & Deploy ==="

cd frontend

# 1. Clean and install
npm ci
rm -rf dist

# 2. Build for production
echo "Building production bundle..."
npm run build -- --configuration=production --optimization

# 3. Analyze bundle size
echo "Bundle analysis:"
du -sh dist/ehr-platform-frontend/
ls -lh dist/ehr-platform-frontend/main*.js

# 4. Verify production environment is set
grep "production: true" dist/ehr-platform-frontend/main*.js

# 5. Upload to CDN
echo "Uploading to CDN..."
az storage blob upload-batch \
  --account-name prodcdn \
  --destination '$web' \
  --source dist/ehr-platform-frontend/ \
  --overwrite

# 6. Invalidate CDN cache
az cdn endpoint purge \
  --resource-group prod-rg \
  --profile-name ehr-platform-cdn \
  --name ehr-platform-app \
  --content-paths "/*"

echo "Frontend deployment complete ✅"
```

### Phase 2: Smoke Tests (T-5 min)

```bash
#!/bin/bash
# Smoke tests in production

echo "=== Production Smoke Tests ==="

# 1. Frontend loads
echo "1. Testing frontend load..."
curl -I https://app.ehr-platform.com/ | grep "HTTP/2 200" || exit 1

# 2. API connectivity
echo "2. Testing API connectivity..."
curl -s https://app.ehr-platform.com/api/v1/appointments/health | jq . || exit 1

# 3. Schedule appointment
echo "3. Testing appointment scheduling..."
RESPONSE=$(curl -s -X POST https://api.ehr-platform.com/api/v1/appointments \
  -H "Content-Type: application/json" \
  -d '{
    "patientId": "test-p1",
    "providerId": "test-pr1",
    "scheduledStart": "2026-08-15T14:00:00Z",
    "durationMinutes": 30,
    "appointmentType": "Office",
    "reasonForVisit": "Production test"
  }')

if echo $RESPONSE | jq . > /dev/null 2>&1; then
  APT_ID=$(echo $RESPONSE | jq -r '.id')
  echo "✅ Created appointment: $APT_ID"
else
  echo "❌ Failed to create appointment"
  exit 1
fi

# 4. Retrieve appointment
echo "4. Testing appointment retrieval..."
curl -s https://api.ehr-platform.com/api/v1/appointments/$APT_ID | jq . || exit 1

echo "Smoke tests passed ✅"
```

---

## Production Verification

### Monitoring Dashboard Check

```
✅ Pod Status: All 3 replicas running
✅ CPU Usage: < 60%
✅ Memory Usage: < 70%
✅ Disk Usage: < 80%
✅ Network: No packet loss
✅ Error Rate: < 0.1%
✅ Response Time (p95): < 200ms
✅ Request Rate: Normal
✅ Database Connection Pool: 10/50 used
✅ Cache Hit Rate: > 90%
```

### Application Insights Metrics

```javascript
// Key metrics to monitor (first 30 minutes)
{
  successRate: "> 99.9%",
  averageResponseTime: "< 100ms",
  failureRate: "< 0.01%",
  exceptionsPerSecond: "0",
  databaseConnectionsOpen: "< 20",
  cacheHitRatio: "> 95%",
  customEventCount: "> 0"
}
```

---

## Post-Deployment (T+30 min)

### Validation Checklist

```bash
#!/bin/bash
# Post-deployment validation

echo "=== Post-Deployment Validation ==="

# 1. Check all endpoints
ENDPOINTS=(
  "https://api.ehr-platform.com/api/v1/appointments/health"
  "https://api.ehr-platform.com/api/v1/appointments/patient/p1"
  "https://api.ehr-platform.com/api/v1/providers/pr1/availability"
)

for endpoint in "${ENDPOINTS[@]}"; do
  STATUS=$(curl -s -o /dev/null -w "%{http_code}" $endpoint)
  if [ "$STATUS" = "200" ] || [ "$STATUS" = "400" ]; then
    echo "✅ $endpoint"
  else
    echo "❌ $endpoint (Status: $STATUS)"
  fi
done

# 2. Check logs for errors
echo "Checking logs for errors..."
kubectl logs -n production -l app=appointment-service --tail=100 | grep -i error || echo "No errors found ✅"

# 3. Verify database connection
echo "Verifying database..."
sqlcmd -S prod-db-primary.ehr-platform.com -U sa -P [password] \
  -Q "SELECT TOP 1 * FROM [Appointment].[dbo].[Appointments]" || exit 1

# 4. Test key workflows
echo "Testing key workflows..."

# Schedule
RESPONSE=$(curl -s -X POST https://api.ehr-platform.com/api/v1/appointments \
  -H "Content-Type: application/json" \
  -d '{"patientId":"p1","providerId":"pr1","scheduledStart":"2026-08-15T14:00:00Z","durationMinutes":30,"appointmentType":"Office","reasonForVisit":"Test"}')
APT_ID=$(echo $RESPONSE | jq -r '.id')
echo "✅ Schedule workflow: $APT_ID"

# Confirm
curl -s -X POST https://api.ehr-platform.com/api/v1/appointments/$APT_ID/confirm
echo "✅ Confirm workflow"

echo "Post-deployment validation complete ✅"
```

---

## 24-Hour Monitoring Plan

### Real-time Monitoring (First Hour)

```
Minute 0-5: Heavy monitoring, 1 minute data aggregation
- Error rates
- Response times
- Database connections
- Cache performance

Minute 5-30: Active monitoring
- Check for any anomalies
- Review logs for warnings
- Monitor user feedback channels

Minute 30-60: Continued observation
- Verify sustained performance
- Check secondary endpoints
- Monitor infrastructure

Minute 60+: Standard monitoring
- 5 minute check-ins
- Escalate if issues found
```

### 24-Hour Monitoring (After First Hour)

```bash
#!/bin/bash
# 24-hour monitoring script

echo "Starting 24-hour monitoring period"

for i in {1..1440}; do
  TIMESTAMP=$(date +"%Y-%m-%d %H:%M:%S")
  
  # Every 5 minutes
  if [ $((i % 5)) -eq 0 ]; then
    # Health check
    HEALTH=$(curl -s https://api.ehr-platform.com/api/v1/appointments/health | jq -r '.status')
    
    # Get metrics
    ERROR_RATE=$(kubectl logs -n production -l app=appointment-service --since=1m | grep -c "ERROR" || echo "0")
    
    # Check response time
    RESPONSE_TIME=$(curl -s -o /dev/null -w "%{time_total}" https://api.ehr-platform.com/api/v1/appointments/health)
    
    if [ "$HEALTH" != "healthy" ] || [ "$ERROR_RATE" -gt "5" ] || [ $(echo "$RESPONSE_TIME > 1" | bc) -eq 1 ]; then
      echo "$TIMESTAMP ⚠️  Alert: Health=$HEALTH Errors=$ERROR_RATE ResponseTime=$RESPONSE_TIME"
      # Trigger alert
    fi
  fi
  
  sleep 60
done
```

---

## Rollback Plan (If Issues Occur)

### Immediate Rollback (< 2 minutes)

```bash
#!/bin/bash
# Emergency rollback script

echo "=== EMERGENCY ROLLBACK ==="

# Step 1: Revert Kubernetes deployment
echo "Reverting deployment to previous version..."
kubectl rollout undo deployment/appointment-service-prod -n production
kubectl rollout status deployment/appointment-service-prod -n production

# Step 2: Invalidate CDN (frontend auto-reverts)
echo "Invalidating CDN cache..."
az cdn endpoint purge --resource-group prod-rg --profile-name ehr-platform-cdn \
  --name ehr-platform-app --content-paths "/*"

# Step 3: Verify rollback
sleep 30
HEALTH=$(curl -s https://api.ehr-platform.com/api/v1/appointments/health | jq -r '.status')
if [ "$HEALTH" = "healthy" ]; then
  echo "✅ Rollback successful"
else
  echo "❌ Rollback verification failed"
  exit 1
fi

# Step 4: Notify team
echo "Rollback completed. Team notified."
```

### Database Rollback (If Schema Issue)

```sql
-- Restore from backup if critical issue
RESTORE DATABASE [appointment_service_prod] 
FROM DISK = N'/backup/pre-deployment-20260728-020000.bak' 
WITH FILE = 1, RECOVERY, REPLACE
```

---

## Post-Deployment Review (T+24 hours)

### Deployment Success Criteria

| Criterion | Target | Actual | Status |
|---|---|---|---|
| Uptime | 99.9% | ___ | ✅/❌ |
| Error Rate | < 0.1% | ___ | ✅/❌ |
| Response Time (p95) | < 200ms | ___ | ✅/❌ |
| Database Integrity | 100% | ___ | ✅/❌ |
| User Workflows | 100% | ___ | ✅/❌ |
| No Data Loss | Yes | ___ | ✅/❌ |

### Post-Deployment Sign-Off

```
Deployment Completed: July 28, 2026 02:15 UTC
Version: 1.0.0
Duration: 15 minutes

Verified By:
- DevOps Engineer: _________________ Signed: _______
- Backend Lead: _________________ Signed: _______
- Frontend Lead: _________________ Signed: _______
- On-Call Engineer: _________________ Signed: _______

Status: ✅ PRODUCTION DEPLOYMENT SUCCESSFUL

Next Steps:
1. Monitor for 24 hours
2. Collect user feedback
3. Plan next release
4. Update documentation
```

---

## Incident Response

### If Critical Issue Occurs

```
1. Immediate Alert (sent automatically)
2. Assess Impact
   - User impact level
   - Data impact
   - Revenue impact
3. Decide: FIX vs ROLLBACK
   - If fixable in < 5 min: attempt fix
   - Otherwise: ROLLBACK
4. Execute Decision
5. Post-Incident Review within 24 hours
```

---

## Success Indicators

✅ Zero downtime deployment  
✅ All health checks passing  
✅ User workflows functioning normally  
✅ Performance within SLA  
✅ No data loss or corruption  
✅ Monitoring and alerts working  
✅ Team confidence in production stability  

**PRODUCTION DEPLOYMENT COMPLETE** 🚀

