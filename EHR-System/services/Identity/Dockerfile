# Multi-stage build for Identity Service
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files
COPY ["src/Identity.API/Identity.API.csproj", "Identity.API/"]
COPY ["src/Identity.Application/Identity.Application.csproj", "Identity.Application/"]
COPY ["src/Identity.Contracts/Identity.Contracts.csproj", "Identity.Contracts/"]
COPY ["src/Identity.Domain/Identity.Domain.csproj", "Identity.Domain/"]
COPY ["src/Identity.Infrastructure/Identity.Infrastructure.csproj", "Identity.Infrastructure/"]
COPY ["src/Identity.Persistence/Identity.Persistence.csproj", "Identity.Persistence/"]

# Restore packages
RUN dotnet restore "Identity.API/Identity.API.csproj"

# Copy source
COPY . .

# Build
RUN dotnet build "src/Identity.API/Identity.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "src/Identity.API/Identity.API.csproj" -c Release -o /app/publish

# Final stage
FROM runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Identity.API.dll"]
