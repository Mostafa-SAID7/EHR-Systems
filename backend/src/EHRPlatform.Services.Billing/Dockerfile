FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine as builder
WORKDIR /src

COPY ["src/EHRPlatform.Common/EHRPlatform.Common.csproj", "src/EHRPlatform.Common/"]
COPY ["src/EHRPlatform.Services.Billing/EHRPlatform.Services.Billing.csproj", "src/EHRPlatform.Services.Billing/"]

RUN dotnet restore "src/EHRPlatform.Services.Billing/EHRPlatform.Services.Billing.csproj"

COPY ["src/", "src/"]

RUN dotnet build "src/EHRPlatform.Services.Billing/EHRPlatform.Services.Billing.csproj" -c Release -o /app/build
RUN dotnet publish "src/EHRPlatform.Services.Billing/EHRPlatform.Services.Billing.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5006/health || exit 1

EXPOSE 5006
ENTRYPOINT ["dotnet", "EHRPlatform.Services.Billing.dll"]
