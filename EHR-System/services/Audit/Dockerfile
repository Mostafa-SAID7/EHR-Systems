FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Audit.API/Audit.API.csproj", "Audit.API/"]
COPY ["src/Audit.Application/Audit.Application.csproj", "Audit.Application/"]
COPY ["src/Audit.Domain/Audit.Domain.csproj", "Audit.Domain/"]
COPY ["src/Audit.Infrastructure/Audit.Infrastructure.csproj", "Audit.Infrastructure/"]
COPY ["src/Audit.Persistence/Audit.Persistence.csproj", "Audit.Persistence/"]

RUN dotnet restore "Audit.API/Audit.API.csproj"

COPY . .

RUN dotnet build "src/Audit.API/Audit.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Audit.API/Audit.API.csproj" -c Release -o /app/publish

FROM runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Audit.API.dll"]
