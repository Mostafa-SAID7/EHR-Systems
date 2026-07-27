FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine as builder
WORKDIR /src

COPY ["src/EHRPlatform.Common/EHRPlatform.Common.csproj", "src/EHRPlatform.Common/"]
COPY ["src/EHRPlatform.Services.Analytics/EHRPlatform.Services.Analytics.csproj", "src/EHRPlatform.Services.Analytics/"]

RUN dotnet restore "src/EHRPlatform.Services.Analytics/EHRPlatform.Services.Analytics.csproj"

COPY ["src/", "src/"]

RUN dotnet build "src/EHRPlatform.Services.Analytics/EHRPlatform.Services.Analytics.csproj" -c Release -o /app/build
RUN dotnet publish "src/EHRPlatform.Services.Analytics/EHRPlatform.Services.Analytics.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5009/health || exit 1

EXPOSE 5009
ENTRYPOINT ["dotnet", "EHRPlatform.Services.Analytics.dll"]
