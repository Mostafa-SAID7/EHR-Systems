# Multi-stage build: lean, fast, no duplicates
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine as builder
WORKDIR /src

# Copy only project files first (layer caching)
COPY ["src/EHRPlatform.Common/EHRPlatform.Common.csproj", "src/EHRPlatform.Common/"]
COPY ["src/EHRPlatform.Services.ApiGateway/EHRPlatform.Services.ApiGateway.csproj", "src/EHRPlatform.Services.ApiGateway/"]

# Restore (cached if no csproj changes)
RUN dotnet restore "src/EHRPlatform.Services.ApiGateway/EHRPlatform.Services.ApiGateway.csproj"

# Copy source code
COPY ["src/", "src/"]

# Build
RUN dotnet build "src/EHRPlatform.Services.ApiGateway/EHRPlatform.Services.ApiGateway.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "src/EHRPlatform.Services.ApiGateway/EHRPlatform.Services.ApiGateway.csproj" -c Release -o /app/publish

# Runtime stage: minimal image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app

# Copy published app
COPY --from=builder /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5000/health || exit 1

EXPOSE 5000
ENTRYPOINT ["dotnet", "EHRPlatform.Services.ApiGateway.dll"]
