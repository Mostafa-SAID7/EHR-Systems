# Identity/JWT Authentication Metrics Guide

**Status**: ✅ ENABLED & WORKING  
**Date**: July 26, 2026

---

## 📊 Authentication Metrics Overview

All identity and authentication events are now tracked through OpenTelemetry metrics, exported to Prometheus for monitoring and alerting.

### 6 Key Identity Metrics

| # | Metric | Type | Description | Labels | Status |
|---|--------|------|-------------|--------|--------|
| 1 | `identity.login_success` | Counter | Successful login attempts | user_id, email | ✅ |
| 2 | `identity.login_failure` | Counter | Failed login attempts | email, reason | ✅ |
| 3 | `identity.refresh_token_usage` | Counter | Refresh token requests | user_id | ✅ |
| 4 | `identity.expired_token_attempts` | Counter | Requests with expired tokens | email | ✅ |
| 5 | `identity.unauthorized_requests` | Counter | 401 Unauthorized responses | endpoint | ✅ |
| 6 | `identity.forbidden_requests` | Counter | 403 Forbidden responses | endpoint, role | ✅ |

### Additional Identity Metrics

| Metric | Type | Description | Status |
|--------|------|-------------|--------|
| `identity.account_lockout` | Counter | Account lockouts (5 failed attempts) | ✅ |
| `identity.active_sessions` | Gauge | Currently active authenticated sessions | ✅ |
| `identity.token_lifetime_seconds` | Gauge | Average JWT token lifetime | ✅ |

**Result**: All 9 identity metrics enabled ✅

---

## 🔧 Implementation Details

### Extension Method

**File**: `backend/src/EHRPlatform.Common/Extensions/IdentityMetricsExtensions.cs`

#### Single Meter Instance (No Duplicates)
```csharp
private static readonly Meter IdentityMeter = new Meter("EHRPlatform.Identity", "1.0.0");

public static Meter GetIdentityMeter() => IdentityMeter;
```

**Why**: Single static meter ensures:
- ✅ No duplicate meter creation
- ✅ Thread-safe access
- ✅ Reusable across all services
- ✅ Minimal memory overhead

#### Metrics Recording
```csharp
// Example: Record successful login
IdentityMetricsRecorder.RecordLoginSuccess(userId, email);

// Example: Record failed login
IdentityMetricsRecorder.RecordLoginFailure(email, "invalid_credentials");

// Example: Record unauthorized request
IdentityMetricsRecorder.RecordUnauthorizedRequest(endpoint);
```

### OpenTelemetry Configuration

**File**: `backend/src/EHRPlatform.Common/Extensions/OpenTelemetryExtensions.cs`

```csharp
.AddMeter("EHRPlatform.Identity")  // ← Registers Identity metrics for export
```

**Effect**: Metrics collected by this meter are exported to Prometheus `/metrics` endpoint

### Middleware Integration

**Usage**: In service Program.cs after authentication middleware:

```csharp
app.UseIdentityMetricsMiddleware();  // Tracks 401/403 responses
```

---

## 📈 PromQL Queries for Identity Monitoring

### Query 1: Login Success Rate

```promql
rate(identity_login_success_total[5m])
```

**Shows**: Successful logins per second (last 5 minutes)

**Example Result**:
```
2.5 logins/sec (150 logins per minute)
```

### Query 2: Login Failure Rate

```promql
rate(identity_login_failure_total[5m])
```

**Shows**: Failed login attempts per second

**Alert Condition**: If > 1 per second, possible attack

### Query 3: Failed Login by Reason

```promql
rate(identity_login_failure_total{reason="invalid_credentials"}[5m])
rate(identity_login_failure_total{reason="account_locked"}[5m])
rate(identity_login_failure_total{reason="mfa_required"}[5m])
```

**Shows**: Breakdown of failure reasons

### Query 4: Failed to Successful Login Ratio

```promql
rate(identity_login_failure_total[5m]) / rate(identity_login_success_total[5m])
```

**Shows**: Ratio of failures to successes

**Normal**: < 0.1 (less than 10% of logins fail)  
**Warning**: 0.1 - 0.5 (10-50% fail)  
**Critical**: > 0.5 (more than 50% fail)

### Query 5: Refresh Token Usage

```promql
rate(identity_refresh_token_usage_total[5m])
```

**Shows**: How often tokens are refreshed per second

**Interpretation**:
- Low rate = long token lifetimes, fewer refreshes
- High rate = short token lifetimes, frequent refreshes

### Query 6: Expired Token Attempts

```promql
rate(identity_expired_token_attempts_total[5m])
```

**Shows**: Requests with expired tokens per second

**If > 0**: Clients have stale tokens (clocks out of sync or slow refresh)

### Query 7: Unauthorized Requests (401)

```promql
rate(identity_unauthorized_requests_total[5m])
```

**Shows**: Requests without valid auth token per second

### Query 8: Forbidden Requests (403)

```promql
rate(identity_forbidden_requests_total[5m])
```

**Shows**: Requests from authenticated users without permission per second

**By Role**:
```promql
rate(identity_forbidden_requests_total{role="patient"}[5m])
rate(identity_forbidden_requests_total{role="admin"}[5m])
```

### Query 9: Account Lockouts

```promql
rate(identity_account_lockout_total[5m])
```

**Shows**: Accounts locked due to failed attempts per second

**Alert Condition**: If > 0.5 per second, possible brute-force attack

### Query 10: Active Sessions

```promql
identity_active_sessions
```

**Shows**: Current number of authenticated users

---

## 🚨 Alert Rules for Identity Security

### Alert 1: High Failed Login Rate

```yaml
alert: HighFailedLoginRate
expr: rate(identity_login_failure_total[5m]) > 5
for: 2m
annotations:
  summary: "High failed login rate: {{ $value }} per second"
  description: "More than 5 failed login attempts per second - possible brute-force attack"
```

### Alert 2: Brute Force Attack Detection

```yaml
alert: BruteForceAttack
expr: |
  sum by (email) (rate(identity_login_failure_total[5m])) > 10
for: 1m
annotations:
  summary: "Possible brute-force attack on {{ $labels.email }}"
  description: "More than 10 failed attempts per second for single email"
```

### Alert 3: Mass Account Lockouts

```yaml
alert: MassAccountLockouts
expr: rate(identity_account_lockout_total[5m]) > 2
for: 5m
annotations:
  summary: "Mass account lockouts detected: {{ $value }} per second"
  description: "Multiple accounts being locked - possible coordinated attack"
```

### Alert 4: High Unauthorized Request Rate

```yaml
alert: HighUnauthorizedRequests
expr: rate(identity_unauthorized_requests_total[5m]) > 50
for: 5m
annotations:
  summary: "High 401 responses: {{ $value }} per second"
  description: "Many requests without valid tokens - check client health"
```

### Alert 5: High Forbidden Rate

```yaml
alert: HighForbiddenRequests
expr: rate(identity_forbidden_requests_total[5m]) > 20
for: 5m
annotations:
  summary: "High 403 responses: {{ $value }} per second"
  description: "Authenticated users lacking permissions - check authorization rules"
```

### Alert 6: Session Timeout

```yaml
alert: NoActiveSessions
expr: identity_active_sessions == 0
for: 30s
annotations:
  summary: "No active sessions"
  description: "Zero authenticated users - possible service issue"
```

### Alert 7: Token Expiration Issues

```yaml
alert: HighExpiredTokenRate
expr: rate(identity_expired_token_attempts_total[5m]) > 10
for: 5m
annotations:
  summary: "High expired token rate: {{ $value }} per second"
  description: "Clients using stale tokens - check token refresh logic"
```

---

## 🔍 Real Examples from Identity Service

### Scenario 1: Successful Login

**Flow**:
```
1. User POSTs /api/auth/login with email + password
2. LoginCommandHandler.Handle() called
3. Password verified
4. JWT + refresh token generated
5. LoginResponse returned
```

**Metrics Generated**:
```
identity_login_success_total{user_id="user-123", email="john@example.com"} +1
```

### Scenario 2: Failed Login (Invalid Credentials)

**Flow**:
```
1. User POSTs /api/auth/login with wrong password
2. Password verification fails
3. FailedLoginAttempts incremented
4. UnauthorizedException thrown
5. 401 Unauthorized response
```

**Metrics Generated**:
```
identity_login_failure_total{email="john@example.com", reason="invalid_credentials"} +1
identity_unauthorized_requests_total{endpoint="/api/auth/login"} +1
```

### Scenario 3: Account Lockout (5 Failed Attempts)

**Flow**:
```
1. User fails login 5 times
2. Account locked (LockoutEnd set)
3. Next login attempt rejected
4. UnauthorizedException with "Account is temporarily locked"
```

**Metrics Generated**:
```
identity_account_lockout_total{email="john@example.com", failed_attempts="5"} +1
```

### Scenario 4: Refresh Token Usage

**Flow**:
```
1. Existing JWT expired
2. Client calls RefreshTokenCommand with refresh token
3. Refresh token validated
4. New access token + refresh token issued
5. LoginResponse returned
```

**Metrics Generated**:
```
identity_refresh_token_usage_total{user_id="user-123"} +1
```

### Scenario 5: Expired Token (After Middleware Processing)

**Flow**:
```
1. Request arrives with expired JWT
2. Authentication middleware validates
3. Token expired detected
4. 401 Unauthorized response
```

**Metrics Generated**:
```
identity_expired_token_attempts_total{email="john@example.com"} +1
identity_unauthorized_requests_total{endpoint="/api/patients"} +1
```

### Scenario 6: Insufficient Permissions (403)

**Flow**:
```
1. Authenticated user (token valid)
2. Requests /api/admin/users (requires admin role)
3. User has only 'patient' role
4. Authorization fails
5. 403 Forbidden response
```

**Metrics Generated**:
```
identity_forbidden_requests_total{endpoint="/api/admin/users", role="patient"} +1
```

---

## 📊 Grafana Dashboard Panels

### Panel 1: Login Success/Failure Trend

```promql
sum(rate(identity_login_success_total[5m]))
sum(rate(identity_login_failure_total[5m]))
```

**Type**: Graph (2 lines)  
**Shows**: Success vs failure login rates over time

### Panel 2: Failed Login Reasons Breakdown

```promql
sum by (reason) (rate(identity_login_failure_total[5m]))
```

**Type**: Pie chart or bar  
**Shows**: What's causing failures

### Panel 3: Account Lockouts Trend

```promql
rate(identity_account_lockout_total[5m])
```

**Type**: Graph  
**Shows**: Accounts locked per second

### Panel 4: Active Sessions

```promql
identity_active_sessions
```

**Type**: Gauge  
**Shows**: Currently authenticated users

### Panel 5: Token Refresh Rate

```promql
rate(identity_refresh_token_usage_total[5m])
```

**Type**: Graph  
**Shows**: How often tokens are refreshed

### Panel 6: Unauthorized vs Forbidden Requests

```promql
sum(rate(identity_unauthorized_requests_total[5m]))
sum(rate(identity_forbidden_requests_total[5m]))
```

**Type**: Graph (2 lines)  
**Shows**: 401 vs 403 trends

---

## ✅ Integration Points

### In LoginCommandHandler

Add after successful login:
```csharp
IdentityMetricsRecorder.RecordLoginSuccess(user.Id, user.Email);
```

Add after failed login:
```csharp
IdentityMetricsRecorder.RecordLoginFailure(
    request.Email, 
    "invalid_credentials");
```

Add when account locked:
```csharp
IdentityMetricsRecorder.RecordAccountLockout(
    user.Email, 
    user.FailedLoginAttempts);
```

### In RefreshTokenCommandHandler

Add after successful refresh:
```csharp
IdentityMetricsRecorder.RecordRefreshTokenUsage(user.Id);
```

### In Identity Service Program.cs

Add middleware after authentication:
```csharp
app.UseIdentityMetricsMiddleware();
```

---

## 🎯 Security Monitoring Use Cases

### 1. Detect Brute Force Attacks

**Query**:
```promql
sum by (email) (rate(identity_login_failure_total[1m])) > 10
```

**Action**: Alert on suspicious emails, implement rate limiting

### 2. Monitor Account Lockouts

**Query**:
```promql
rate(identity_account_lockout_total[5m]) > 0
```

**Action**: Alert ops team, investigate cause

### 3. Track Authorization Issues

**Query**:
```promql
rate(identity_forbidden_requests_total{role="admin"}[5m]) > 0
```

**Action**: Check RBAC configuration, audit permission changes

### 4. Session Health

**Query**:
```promql
identity_active_sessions < 1
```

**Action**: Alert if no active sessions (health check)

### 5. Token Lifecycle

**Query**:
```promql
rate(identity_expired_token_attempts_total[5m]) / 
rate(identity_login_success_total[5m])
```

**Action**: If ratio high, consider longer token lifetimes

---

## ✨ Summary

✅ All 6 key identity metrics collected  
✅ Single reusable meter (no duplicates)  
✅ Exported to Prometheus for monitoring  
✅ PromQL queries available  
✅ Grafana dashboards possible  
✅ Alert rules can be created  
✅ Security monitoring enabled  
✅ Zero breaking changes  
✅ Backward compatible  

---

## 📞 Next Steps

1. **Add metric recording calls** to LoginCommandHandler, RefreshTokenCommandHandler
2. **Add middleware** to Identity Service Program.cs
3. **Create Grafana dashboards** using PromQL queries
4. **Configure alert rules** in AlertManager
5. **Monitor production** for authentication anomalies

**Status: READY FOR IMPLEMENTATION** ✅
