FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine as builder
WORKDIR /src

COPY ["src/EHRPlatform.Common/EHRPlatform.Common.csproj", "src/EHRPlatform.Common/"]
COPY ["src/EHRPlatform.Services.Audit/EHRPlatform.Services.Audit.csproj", "src/EHRPlatform.Services.Audit/"]

RUN dotnet restore "src/EHRPlatform.Services.Audit/EHRPlatform.Services.Audit.csproj"

COPY ["src/", "src/"]

RUN dotnet build "src/EHRPlatform.Services.Audit/EHRPlatform.Services.Audit.csproj" -c Release -o /app/build
RUN dotnet publish "src/EHRPlatform.Services.Audit/EHRPlatform.Services.Audit.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5007/health || exit 1

EXPOSE 5007
ENTRYPOINT ["dotnet", "EHRPlatform.Services.Audit.dll"]
