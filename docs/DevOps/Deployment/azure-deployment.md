# Azure Deployment

## App Service Deployment

```yaml
# azure-pipelines.yml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

stages:
- stage: Deploy
  jobs:
  - deployment: DeployToAppService
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
              runtimeStack: 'DOTNETCORE|8.0'
```

---

## Key Vault Secrets

```csharp
// Program.cs
var keyVaultEndpoint = new Uri("https://your-keyvault.vault.azure.net/");
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

// Access secrets
var connectionString = builder.Configuration["ConnectionString"];
var jwtSecret = builder.Configuration["JwtSecret"];
```

---

## Azure SQL Database

```csharp
// Connection string
"Server=tcp:your-server.database.windows.net,1433;Initial Catalog=EHR;User ID=admin@your-server;Password=YourPassword;Encrypt=true;Connection Timeout=30;"

// Enable Geo-replication for high availability
```

---

## Blob Storage

```csharp
// Upload files to blob storage
var containerClient = new BlobContainerClient(
    new Uri("https://yourstg.blob.core.windows.net/reports"),
    new DefaultAzureCredential()
);

await containerClient.UploadBlobAsync(
    "report-2024.pdf",
    File.OpenRead("path/to/report.pdf")
);
```
