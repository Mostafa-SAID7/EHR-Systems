FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Analytics.API/Analytics.API.csproj", "Analytics.API/"]
COPY ["src/Analytics.Application/Analytics.Application.csproj", "Analytics.Application/"]
COPY ["src/Analytics.Domain/Analytics.Domain.csproj", "Analytics.Domain/"]
COPY ["src/Analytics.Infrastructure/Analytics.Infrastructure.csproj", "Analytics.Infrastructure/"]
COPY ["src/Analytics.Persistence/Analytics.Persistence.csproj", "Analytics.Persistence/"]

RUN dotnet restore "Analytics.API/Analytics.API.csproj"

COPY . .

RUN dotnet build "src/Analytics.API/Analytics.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Analytics.API/Analytics.API.csproj" -c Release -o /app/publish

FROM runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Analytics.API.dll"]
