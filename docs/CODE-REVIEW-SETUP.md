# 🔍 Code Review Integration Setup Guide

Complete guide for setting up SonarQube, Dependabot, and GitHub Actions code review automation.

---

## 📋 Table of Contents

1. [SonarQube Setup](#sonarqube-setup)
2. [Dependabot Configuration](#dependabot-configuration)
3. [GitHub Actions Setup](#github-actions-setup)
4. [Manual Review Procedures](#manual-review-procedures)
5. [Troubleshooting](#troubleshooting)

---

## 🔧 SonarQube Setup

### Step 1: Create SonarCloud Account

1. Go to https://sonarcloud.io
2. Click "Sign up"
3. Choose "GitHub" as your authentication method
4. Authorize GitHub access
5. Accept terms and create organization

### Step 2: Generate Security Token

1. Login to SonarCloud
2. Click your avatar → "My Account"
3. Go to "Security" tab
4. Click "Generate Tokens"
5. Name: `GitHub Actions CI/CD`
6. Select "User" type
7. Click "Generate" and copy the token

### Step 3: Add GitHub Repository Secret

1. Go to: `https://github.com/Mostafa-SAID7/EHR-Systems-Microservices/settings/secrets/actions`
2. Click "New repository secret"
3. **Name**: `SONAR_TOKEN`
4. **Value**: Paste the token from Step 2
5. Click "Add secret"

### Step 4: Configure Organization in SonarCloud

1. In SonarCloud, go to "Organizations"
2. Create new organization with key: `mostafa-said7`
3. Link to GitHub account
4. Import repository: `EHR-Systems-Microservices`

### Step 5: Set Quality Gates (Optional)

1. In SonarCloud project settings
2. Go to "Quality Gates"
3. Create custom gate or use default
4. Define thresholds for:
   - Code Coverage: >= 80%
   - Code Smells: < 50
   - Security Rating: A
   - Maintainability Rating: A

---

## 🤖 Dependabot Configuration

### What Dependabot Does:

- Automatically checks for dependency updates
- Creates pull requests with updates
- Includes security patches and new versions
- Can auto-merge minor/patch updates

### Already Configured:

The `.github/dependabot.yml` file is already set up to monitor:

✅ **NuGet Packages** (C# / .NET)
- Updates check: Weekly (Monday)
- Auto-opens PRs for updates
- Labels: `dependencies`, `dotnet`

✅ **NPM Packages** (Frontend/Node)
- Updates check: Weekly (Monday)
- Auto-opens PRs for updates
- Labels: `dependencies`, `npm`

✅ **GitHub Actions**
- Updates check: Weekly (Monday)
- Auto-opens PRs for workflow updates
- Labels: `dependencies`, `ci-cd`

✅ **Docker Images**
- Updates check: Weekly (Tuesday)
- Auto-opens PRs for base image updates
- Labels: `dependencies`, `docker`

### Enable Dependabot Alerts

1. Go to: `https://github.com/Mostafa-SAID7/EHR-Systems-Microservices/settings/security_analysis`
2. Enable:
   - ✅ Dependabot alerts
   - ✅ Dependabot security updates
   - ✅ Dependabot version updates

### Auto-Merge Setup (Optional)

To automatically merge Dependabot PRs:

1. Go to branch settings
2. Enable "Allow auto-merge"
3. Choose merge method: "Squash and merge"
4. Require status checks to pass

---

## 🚀 GitHub Actions Setup

### Workflows Included:

#### 1. **CI - Build & Test** (`ci-build.yml`)
- Builds all 11 services
- Runs unit tests (non-blocking)
- Validates code compilation

#### 2. **Tag Endpoints Testing** (`test.yml`)
- Runs service-specific tests
- Security scanning
- Service integrity checks

#### 3. **Docker Build & Push** (`docker-push.yml`)
- Builds Docker images
- Pushes to GHCR (GitHub Container Registry)
- Scans images for vulnerabilities

#### 4. **Code Review** (`code-review.yml`) ← NEW
- SonarQube analysis
- Code metrics collection
- Security scanning
- Posts results as PR comments

### How to Manually Trigger:

#### Via GitHub UI:
1. Go to "Actions" tab
2. Select workflow
3. Click "Run workflow"
4. Choose branch
5. Click "Run workflow"

#### Via Command Line:
```bash
# Trigger code review workflow
gh workflow run code-review.yml --ref main

# Trigger CI build
gh workflow run ci-build.yml --ref main

# View workflow runs
gh run list --workflow=code-review.yml
```

---

## 📝 Manual Review Procedures

### When to Do Manual Review:

- 🔴 Blocker or Critical issues found
- 🟠 Security vulnerabilities detected
- 🟡 Major code smells > 50
- 📊 Code coverage drops below threshold
- 🚨 Integration test failures

### Manual SonarQube Analysis

#### Prerequisites:
```bash
# Install SonarScanner
dotnet tool install --global dotnet-sonarscanner
```

#### Run Analysis Locally:

```bash
cd backend

# Start analysis
dotnet sonarscanner begin \
  /k:"Mostafa-SAID7_EHR-Systems-Microservices" \
  /o:"mostafa-said7" \
  /d:sonar.login="<YOUR_SONAR_TOKEN>" \
  /d:sonar.host.url="https://sonarcloud.io"

# Build
dotnet build EHRPlatform.sln -c Release

# End analysis
dotnet sonarscanner end /d:sonar.login="<YOUR_SONAR_TOKEN>"

# View results at:
# https://sonarcloud.io/dashboard?id=Mostafa-SAID7_EHR-Systems-Microservices
```

### Manual Dependency Check

#### Check for outdated packages:
```bash
cd backend
dotnet list package
dotnet list package --outdated
```

#### Check for vulnerable packages:
```bash
cd backend
dotnet list package --vulnerable
```

#### Update specific package:
```bash
cd backend
dotnet add package PackageName --version X.Y.Z
```

### Manual Code Analysis

#### Check code style:
```bash
cd backend
dotnet format --verify-no-changes --verbosity diagnostic
```

#### Run security analysis:
```bash
cd backend
dotnet build EHRPlatform.sln /p:EnforceCodeStyleInBuild=true
```

#### Generate code coverage report:
```bash
cd backend
dotnet test --collect:"XPlat Code Coverage"
```

---

## 🔗 Important Links

### SonarCloud:
- **Dashboard**: https://sonarcloud.io/dashboard?id=Mostafa-SAID7_EHR-Systems-Microservices
- **Organization**: https://sonarcloud.io/organizations/mostafa-said7
- **Account Security**: https://sonarcloud.io/account/security

### GitHub:
- **Actions**: https://github.com/Mostafa-SAID7/EHR-Systems-Microservices/actions
- **Secrets**: https://github.com/Mostafa-SAID7/EHR-Systems-Microservices/settings/secrets/actions
- **Branch Protection**: https://github.com/Mostafa-SAID7/EHR-Systems-Microservices/settings/branches
- **Security Analysis**: https://github.com/Mostafa-SAID7/EHR-Systems-Microservices/settings/security_analysis
- **Dependabot**: https://github.com/Mostafa-SAID7/EHR-Systems-Microservices/dependabot

---

## 🐛 Troubleshooting

### SonarQube Not Running

**Problem**: SonarQube analysis step fails
**Solution**:
```bash
# Check token is set
echo $SONAR_TOKEN

# Verify SonarScanner is installed
dotnet tool list --global | grep sonarscanner

# Reinstall if needed
dotnet tool update --global dotnet-sonarscanner
```

### Dependabot PRs Not Created

**Problem**: No Dependabot PRs in pull requests
**Solution**:
1. Check `.github/dependabot.yml` is in main branch
2. Enable Dependabot alerts in settings
3. Wait for scheduled time (Monday 03:00 UTC)
4. Or manually trigger via Actions

### GitHub Actions Secrets Not Found

**Problem**: Workflow fails with "secret not found"
**Solution**:
1. Go to Settings → Secrets
2. Verify secret name matches exactly: `SONAR_TOKEN`
3. Ensure secret value is not empty
4. Secrets are repo-specific, not inherited

### Code Review Comments Not Posting

**Problem**: PR comments not showing up
**Solution**:
1. Ensure workflow has `pull-requests: write` permission
2. Check GitHub token has correct scopes
3. Verify `github.event_name == 'pull_request'` condition

---

## ✅ Verification Checklist

Run through this checklist to verify everything is set up:

- [ ] SonarCloud account created
- [ ] Repository imported to SonarCloud
- [ ] `SONAR_TOKEN` secret added to GitHub
- [ ] Dependabot alerts enabled
- [ ] Dependabot security updates enabled
- [ ] `.github/dependabot.yml` present
- [ ] `.github/workflows/code-review.yml` present
- [ ] `.github/workflows/ci-build.yml` present
- [ ] GitHub Actions enabled
- [ ] Webhook notifications configured (optional)

---

## 🎯 Typical Workflow

### When You Create a Pull Request:

1. **Automatic Checks Run** (2-5 minutes)
   - ✅ CI Build (ci-build.yml)
   - ✅ Code Review (code-review.yml)
   - ✅ SonarQube Analysis
   - ✅ Security Scan

2. **GitHub Comments Appear**
   - PR gets comments with analysis results
   - SonarCloud dashboard link provided
   - Security scan results posted

3. **Code Review Steps**
   - Read automated comments
   - Check SonarCloud dashboard
   - Fix any critical issues
   - Re-push commits

4. **Approval & Merge**
   - All checks pass ✅
   - Maintainer approves PR
   - Auto-merge (if configured)
   - Deploy to production

### When Dependabot Creates a PR:

1. **Auto-Approval** (if configured)
   - Dependabot PR created
   - Auto-approved by CI
   - All checks run

2. **Auto-Merge** (if configured)
   - Checks pass ✅
   - Auto-merged to main
   - CI/CD deploys new version

3. **Manual Review** (if needed)
   - Check PR for major changes
   - Run manual tests
   - Approve before merge

---

## 📞 Support

For issues or questions:

1. Check GitHub Actions logs for error details
2. Review SonarCloud documentation
3. Check Dependabot settings
4. Review this guide again

---

*Last Updated: July 2026*
*Code Review Integration v1.0*
