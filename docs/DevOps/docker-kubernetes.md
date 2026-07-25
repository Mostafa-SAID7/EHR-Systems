# Docker & Kubernetes

## Docker Basics

### Dockerfile

```dockerfile
# Use base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Build stage
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM runtime
WORKDIR /app
COPY --from=build /app/publish .

# Open port
EXPOSE 5000

# Start app
ENTRYPOINT ["dotnet", "EHRPlatform.API.dll"]
```

### Build & Run

```bash
# Build image
docker build -t ehr-api:1.0 .

# Run container
docker run -p 5000:5000 ehr-api:1.0

# List containers
docker ps

# View logs
docker logs <container_id>

# Stop container
docker stop <container_id>
```

---

## Docker Compose

```yaml
version: '3.8'

services:
  api:
    build: .
    ports:
      - "5000:5000"
    environment:
      - ConnectionString=Server=db;Database=EHR;
    depends_on:
      - db
    volumes:
      - ./logs:/app/logs

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123
    ports:
      - "1433:1433"
    volumes:
      - sqlvolume:/var/opt/mssql

volumes:
  sqlvolume:
```

---

## Kubernetes Basics

### Pod (Smallest deployable unit)

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: ehr-api-pod
spec:
  containers:
  - name: api
    image: ehr-api:1.0
    ports:
    - containerPort: 5000
    env:
    - name: ConnectionString
      value: "Server=db-service;Database=EHR;"
```

### Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: ehr-api-deployment
spec:
  replicas: 3  # 3 pods
  selector:
    matchLabels:
      app: ehr-api
  template:
    metadata:
      labels:
        app: ehr-api
    spec:
      containers:
      - name: api
        image: ehr-api:1.0
        ports:
        - containerPort: 5000
        resources:
          requests:
            cpu: "100m"
            memory: "128Mi"
          limits:
            cpu: "500m"
            memory: "512Mi"
```

### Service (Expose pods)

```yaml
apiVersion: v1
kind: Service
metadata:
  name: ehr-api-service
spec:
  type: LoadBalancer  # External access
  selector:
    app: ehr-api
  ports:
  - port: 80
    targetPort: 5000
```

### ConfigMap (Configuration)

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: ehr-config
data:
  appsettings.json: |
    {
      "Logging": {
        "LogLevel": "Information"
      }
    }
```

### Secret (Sensitive data)

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: ehr-secrets
type: Opaque
stringData:
  db-password: "YourPassword123"
  jwt-secret: "your-secret-key"
```

---

## Interview Q&A

**Q: Docker vs VM?**

A:
- Docker: Lightweight, shared OS kernel, fast startup
- VM: Full OS, isolated, heavier resource use

**Q: Why Kubernetes?**

A:
- Orchestration: Deploy and manage containers
- Scaling: Auto-scale based on load
- Self-healing: Restart failed containers
- Updates: Zero-downtime deployments

**Q: How does load balancing work in K8s?**

A: Service distributes traffic to pods. kubectl applies round-robin.
