# Multi-stage build: lean, fast, no duplicates
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine as builder
WORKDIR /src

# Copy only project files first (layer caching)
COPY ["src/EHRPlatform.Common/EHRPlatform.Common.csproj", "src/EHRPlatform.Common/"]
COPY ["src/EHRPlatform.Services.OutboxProcessor/EHRPlatform.Services.OutboxProcessor.csproj", "src/EHRPlatform.Services.OutboxProcessor/"]

# Restore (cached if no csproj changes)
RUN dotnet restore "src/EHRPlatform.Services.OutboxProcessor/EHRPlatform.Services.OutboxProcessor.csproj"

# Copy source code
COPY ["src/", "src/"]

# Build
RUN dotnet build "src/EHRPlatform.Services.OutboxProcessor/EHRPlatform.Services.OutboxProcessor.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "src/EHRPlatform.Services.OutboxProcessor/EHRPlatform.Services.OutboxProcessor.csproj" -c Release -o /app/publish

# Runtime stage: minimal image
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
WORKDIR /app

# Copy published app
COPY --from=builder /app/publish .

# Health check endpoint (optional for worker services)
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD test -f /proc/1/status || exit 1

ENTRYPOINT ["dotnet", "EHRPlatform.Services.OutboxProcessor.dll"]
