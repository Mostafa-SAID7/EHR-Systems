FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine as builder
WORKDIR /src

COPY ["src/EHRPlatform.Common/EHRPlatform.Common.csproj", "src/EHRPlatform.Common/"]
COPY ["src/EHRPlatform.Services.Identity/EHRPlatform.Services.Identity.csproj", "src/EHRPlatform.Services.Identity/"]

RUN dotnet restore "src/EHRPlatform.Services.Identity/EHRPlatform.Services.Identity.csproj"

COPY ["src/", "src/"]

RUN dotnet build "src/EHRPlatform.Services.Identity/EHRPlatform.Services.Identity.csproj" -c Release -o /app/build
RUN dotnet publish "src/EHRPlatform.Services.Identity/EHRPlatform.Services.Identity.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5001/health || exit 1

EXPOSE 5001
ENTRYPOINT ["dotnet", "EHRPlatform.Services.Identity.dll"]
