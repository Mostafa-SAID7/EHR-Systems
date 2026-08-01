# Multi-stage build for EHR-System API Gateway
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

WORKDIR /src

# Copy project files
COPY ["src/APIGateway/APIGateway.csproj", "src/APIGateway/"]

# Restore dependencies
RUN dotnet restore "src/APIGateway/APIGateway.csproj"

# Copy all source
COPY . .

# Build
RUN dotnet build "src/APIGateway/APIGateway.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "src/APIGateway/APIGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

WORKDIR /app

# Install curl for health checks
RUN apk add --no-cache curl

# Copy published application from build stage
COPY --from=build /app/publish .

# Create logs directory
RUN mkdir -p /app/logs /var/log/ehr-gateway && \
    chmod 755 /app/logs /var/log/ehr-gateway

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health/live || exit 1

# Run application
ENTRYPOINT ["dotnet", "EHRPlatform.Gateway.dll"]
