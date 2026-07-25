# CI/CD Pipelines

## GitHub Actions Example

```yaml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore backend/EHRPlatform.sln
    
    - name: Build
      run: dotnet build backend/EHRPlatform.sln --no-restore
    
    - name: Run tests
      run: dotnet test backend/EHRPlatform.sln --no-build --verbosity normal
    
    - name: Build Docker image
      run: docker build -t ehr-api:latest -f backend/Dockerfile .
    
    - name: Push to registry
      run: docker push myregistry.azurecr.io/ehr-api:latest
      env:
        REGISTRY_USERNAME: ${{ secrets.REGISTRY_USERNAME }}
        REGISTRY_PASSWORD: ${{ secrets.REGISTRY_PASSWORD }}
    
    - name: Deploy to Azure
      run: |
        az login --service-principal -u ${{ secrets.AZURE_CLIENT_ID }} -p ${{ secrets.AZURE_CLIENT_SECRET }} --tenant ${{ secrets.AZURE_TENANT_ID }}
        az container create -n ehr-api --image myregistry.azurecr.io/ehr-api:latest
```

---

## Azure DevOps Pipeline

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

stages:
- stage: Build
  jobs:
  - job: BuildAndTest
    steps:
    - task: UseDotNet@2
      inputs:
        packageType: 'sdk'
        version: '8.0.x'
    
    - task: DotNetCoreCLI@2
      inputs:
        command: 'build'
        arguments: '--configuration $(buildConfiguration)'
    
    - task: DotNetCoreCLI@2
      inputs:
        command: 'test'
        arguments: '--configuration $(buildConfiguration)'
    
    - task: DotNetCoreCLI@2
      inputs:
        command: 'publish'
        arguments: '-c $(buildConfiguration) -o $(Build.ArtifactStagingDirectory)'

- stage: Deploy
  dependsOn: Build
  condition: succeeded()
  jobs:
  - deployment: DeployToAzure
    environment: production
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            inputs:
              azureSubscription: 'Azure Connection'
              appType: 'webAppLinux'
              appName: 'ehr-api-prod'
              package: '$(Pipeline.Workspace)/**/*.zip'
```

---

## Deployment Stages

```
Code Push → CI (Build/Test) → CD (Stage) → Production
```

- **CI**: Automated build and test
- **CD Staging**: Deploy to test environment
- **CD Production**: Deploy to live environment

---

## Interview Q&A

**Q: CI vs CD?**

A:
- CI (Continuous Integration): Automated build and test on every commit
- CD (Continuous Deployment): Automatically deploy to production
