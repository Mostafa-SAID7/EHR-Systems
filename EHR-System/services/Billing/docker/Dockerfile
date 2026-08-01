FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /src

# Copy project files
COPY ["src/Billing.API/Billing.API.csproj", "src/Billing.API/"]
COPY ["src/Billing.Application/Billing.Application.csproj", "src/Billing.Application/"]
COPY ["src/Billing.Contracts/Billing.Contracts.csproj", "src/Billing.Contracts/"]
COPY ["src/Billing.Domain/Billing.Domain.csproj", "src/Billing.Domain/"]
COPY ["src/Billing.Infrastructure/Billing.Infrastructure.csproj", "src/Billing.Infrastructure/"]
COPY ["src/Billing.Persistence/Billing.Persistence.csproj", "src/Billing.Persistence/"]

# Restore dependencies
RUN dotnet restore "src/Billing.API/Billing.API.csproj"

# Copy remaining source code
COPY ["src/", "src/"]

# Build
RUN dotnet build "src/Billing.API/Billing.API.csproj" -c Release -o /app/build

# Publish
FROM builder AS publish
RUN dotnet publish "src/Billing.API/Billing.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD dotnet tool install -g dotnet-monitor && dotnet-monitor collect -n 1 http://localhost:8080/health || exit 1

EXPOSE 5007
ENV ASPNETCORE_URLS=http://+:5007
ENTRYPOINT ["dotnet", "Billing.API.dll"]
