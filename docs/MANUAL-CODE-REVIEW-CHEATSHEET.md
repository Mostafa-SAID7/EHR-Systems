# 🎯 Manual Code Review Cheatsheet

Quick reference for when you need to run manual code reviews and analysis.

---

## 🚀 Quick Start

### Setup (First Time Only)

```bash
# Install required tools
dotnet tool install --global dotnet-sonarscanner
dotnet tool install --global FxCopAnalyzers --version 3.3.2

# Get SONAR_TOKEN from: https://sonarcloud.io/account/security/
export SONAR_TOKEN="<your_token_here>"
```

---

## 📊 SonarQube Analysis

### Complete Analysis (Start to Finish)

```bash
cd backend

# 1. Start analysis
dotnet sonarscanner begin \
  /k:"Mostafa-SAID7_EHR-Systems-Microservices" \
  /o:"mostafa-said7" \
  /d:sonar.login="$SONAR_TOKEN" \
  /d:sonar.host.url="https://sonarcloud.io"

# 2. Build
dotnet build EHRPlatform.sln -c Release

# 3. End analysis
dotnet sonarscanner end /d:sonar.login="$SONAR_TOKEN"

# 4. View results
echo "Results: https://sonarcloud.io/dashboard?id=Mostafa-SAID7_EHR-Systems-Microservices"
```

### Quick Check (Build Only)

```bash
cd backend
dotnet build EHRPlatform.sln -c Release
```

---

## 📦 Dependency Management

### List All Packages

```bash
cd backend
dotnet list package
```

### Find Outdated Packages

```bash
cd backend
dotnet list package --outdated
```

### Find Vulnerable Packages

```bash
cd backend
dotnet list package --vulnerable
```

### Update Specific Package

```bash
cd backend
# Update to latest
dotnet add package PackageName

# Update to specific version
dotnet add package PackageName --version 1.2.3
```

### Update All Packages

```bash
cd backend
dotnet list package --vulnerable | grep -oP '(?<=>)[^<]*' | while read -r package; do
  dotnet add package "$package"
done
```

---

## 🔐 Security Scanning

### Check for Vulnerabilities

```bash
cd backend
dotnet list package --vulnerable
```

### Scan NuGet for CVEs

```bash
cd backend
dotnet tool install --global dotnet-outdated -v
dotnet outdated --include-prerelease
```

### Check Secrets in Code

```bash
# Install TruffleHog (secret scanner)
pip install truffleHog
truffleHog git https://github.com/Mostafa-SAID7/EHR-Systems-Microservices
```

---

## 💅 Code Style & Format

### Check Code Formatting

```bash
cd backend
dotnet format --verify-no-changes --verbosity diagnostic
```

### Auto-Fix Formatting

```bash
cd backend
dotnet format
```

### Enforce Style Rules

```bash
cd backend
dotnet build EHRPlatform.sln /p:EnforceCodeStyleInBuild=true
```

---

## 🧪 Testing & Coverage

### Run All Tests

```bash
cd backend/tests/EHRPlatform.Tests.Unit
dotnet test EHRPlatform.Tests.Unit.csproj
```

### Run Tests with Coverage

```bash
cd backend
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Run Specific Test

```bash
cd backend
dotnet test --filter "ClassName"
```

---

## 🔍 Code Analysis Tools

### FxCop Analysis

```bash
cd backend
dotnet build EHRPlatform.sln /p:EnableNETAnalyzers=true /p:AnalysisLevel=latest
```

### Roslynator Analysis

```bash
cd backend
dotnet tool install --global Roslynator.CommandLine
roslynator analyze EHRPlatform.sln --verbosity normal
```

### Code Metrics

```bash
cd backend
dotnet tool install --global Metrics.NET
metrics-analyze EHRPlatform.sln
```

---

## 📋 Build & Compilation

### Build Single Service

```bash
cd backend/src/EHRPlatform.Services.Identity
dotnet build EHRPlatform.Services.Identity.csproj -c Release
```

### Build All Services

```bash
cd backend
dotnet build EHRPlatform.sln -c Release
```

### Clean & Rebuild

```bash
cd backend
dotnet clean EHRPlatform.sln
dotnet build EHRPlatform.sln -c Release
```

### Verbose Build Output

```bash
cd backend
dotnet build EHRPlatform.sln -v diag
```

---

## 🐛 Debugging & Troubleshooting

### Enable Diagnostic Logging

```bash
export DOTNET_DIAGNOSTIC_LEVEL=verbose
cd backend
dotnet build EHRPlatform.sln -c Release
```

### Check Tool Versions

```bash
dotnet tool list --global
dotnet sonarscanner --version
dotnet format --version
```

### Reinstall Tools

```bash
# Remove
dotnet tool uninstall --global dotnet-sonarscanner
dotnet tool uninstall --global FxCopAnalyzers

# Reinstall
dotnet tool install --global dotnet-sonarscanner
dotnet tool install --global FxCopAnalyzers --version 3.3.2
```

---

## 🔄 GitHub Actions - Manual Trigger

### Trigger via CLI

```bash
# Install GitHub CLI if needed
# https://cli.github.com/

# Trigger specific workflow
gh workflow run code-review.yml --ref main

# Trigger all workflows
gh workflow run ci-build.yml --ref main

# View recent runs
gh run list --limit 5

# Watch a specific run
gh run watch <run-id>
```

### Trigger via GitHub UI

1. Go to **Actions** tab
2. Select workflow from left sidebar
3. Click **Run workflow** button
4. Choose branch (usually `main`)
5. Click **Run workflow**

---

## 📊 Review Checklist

Before approving a PR, manually verify:

- [ ] **Build**: `dotnet build EHRPlatform.sln -c Release` ✅
- [ ] **Tests**: All tests pass (or pre-existing failures)
- [ ] **Code Style**: `dotnet format --verify-no-changes` ✅
- [ ] **Security**: `dotnet list package --vulnerable` ✅
- [ ] **SonarQube**: No blocker/critical issues
- [ ] **Services**: All 11 services compile ✅
- [ ] **Dependencies**: No major version conflicts
- [ ] **Documentation**: Updated if needed

---

## 🔗 Important Links

```
SonarCloud Dashboard:
https://sonarcloud.io/dashboard?id=Mostafa-SAID7_EHR-Systems-Microservices

GitHub Actions:
https://github.com/Mostafa-SAID7/EHR-Systems-Microservices/actions

Repository Secrets:
https://github.com/Mostafa-SAID7/EHR-Systems-Microservices/settings/secrets/actions

Dependabot:
https://github.com/Mostafa-SAID7/EHR-Systems-Microservices/dependabot
```

---

## 💡 Pro Tips

### Alias Common Commands

```bash
# Add to ~/.bashrc or ~/.zshrc
alias eb="cd /path/to/backend && dotnet build EHRPlatform.sln -c Release"
alias et="cd /path/to/backend && dotnet test"
alias ef="cd /path/to/backend && dotnet format"
alias sq-begin="dotnet sonarscanner begin /k:Mostafa-SAID7_EHR-Systems-Microservices /o:mostafa-said7 /d:sonar.login=$SONAR_TOKEN"
alias sq-end="dotnet sonarscanner end /d:sonar.login=$SONAR_TOKEN"
```

### Store Token in Environment

```bash
# Add to ~/.bashrc or ~/.zshrc
export SONAR_TOKEN="your_token_here"

# Or use ~/.netrc for safe storage
echo "machine sonarcloud.io login yourtoken" >> ~/.netrc
chmod 600 ~/.netrc
```

### Create Review Script

```bash
#!/bin/bash
# save as: review.sh

echo "🔍 Starting Code Review..."
cd backend

echo "1️⃣ Building..."
dotnet build EHRPlatform.sln -c Release || exit 1

echo "2️⃣ Checking format..."
dotnet format --verify-no-changes || exit 1

echo "3️⃣ Checking vulnerabilities..."
dotnet list package --vulnerable

echo "4️⃣ Running SonarQube..."
dotnet sonarscanner begin /k:Mostafa-SAID7_EHR-Systems-Microservices /o:mostafa-said7 /d:sonar.login=$SONAR_TOKEN
dotnet build EHRPlatform.sln -c Release
dotnet sonarscanner end /d:sonar.login=$SONAR_TOKEN

echo "✅ Review Complete!"
echo "📊 View results: https://sonarcloud.io/dashboard?id=Mostafa-SAID7_EHR-Systems-Microservices"

# Usage: chmod +x review.sh && ./review.sh
```

---

## ⚡ Emergency Procedures

### If Build Breaks

```bash
# 1. Clean everything
cd backend
dotnet clean EHRPlatform.sln

# 2. Restore packages
dotnet restore EHRPlatform.sln

# 3. Build again
dotnet build EHRPlatform.sln -c Release --verbosity diagnostic
```

### If Tests Fail

```bash
cd backend/tests/EHRPlatform.Tests.Unit

# Run failing tests only
dotnet test --filter "TestClassName"

# Run with verbose output
dotnet test --verbosity normal
```

### If SonarQube Fails

```bash
# Check token
echo $SONAR_TOKEN

# Check tool is installed
dotnet tool list --global | grep sonarscanner

# Reinstall
dotnet tool update --global dotnet-sonarscanner

# Try again
cd backend
dotnet sonarscanner begin ...
```

---

*Quick reference v1.0*
*Last updated: July 2026*
