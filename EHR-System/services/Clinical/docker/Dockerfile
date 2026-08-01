# ── Build Stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY ["src/Clinical.API/Clinical.API.csproj",            "src/Clinical.API/"]
COPY ["src/Clinical.Application/Clinical.Application.csproj",   "src/Clinical.Application/"]
COPY ["src/Clinical.Contracts/Clinical.Contracts.csproj",       "src/Clinical.Contracts/"]
COPY ["src/Clinical.Domain/Clinical.Domain.csproj",             "src/Clinical.Domain/"]
COPY ["src/Clinical.Infrastructure/Clinical.Infrastructure.csproj", "src/Clinical.Infrastructure/"]
COPY ["src/Clinical.Persistence/Clinical.Persistence.csproj",   "src/Clinical.Persistence/"]

# Restore packages
RUN dotnet restore "src/Clinical.API/Clinical.API.csproj"

# Copy source
COPY . .

# Build & publish
RUN dotnet publish "src/Clinical.API/Clinical.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime Stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN addgroup --system --gid 1001 appgroup && \
    adduser  --system --uid 1001 --ingroup appgroup appuser

COPY --from=build /app/publish .

USER appuser

EXPOSE 5003

HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:5003/health || exit 1

ENTRYPOINT ["dotnet", "EHRPlatform.Services.Clinical.API.dll"]
