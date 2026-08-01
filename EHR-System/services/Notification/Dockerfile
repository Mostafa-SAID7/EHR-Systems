FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Notification.API/Notification.API.csproj", "Notification.API/"]
COPY ["src/Notification.Application/Notification.Application.csproj", "Notification.Application/"]
COPY ["src/Notification.Domain/Notification.Domain.csproj", "Notification.Domain/"]
COPY ["src/Notification.Infrastructure/Notification.Infrastructure.csproj", "Notification.Infrastructure/"]
COPY ["src/Notification.Persistence/Notification.Persistence.csproj", "Notification.Persistence/"]

RUN dotnet restore "Notification.API/Notification.API.csproj"

COPY . .

RUN dotnet build "src/Notification.API/Notification.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Notification.API/Notification.API.csproj" -c Release -o /app/publish

FROM runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Notification.API.dll"]
