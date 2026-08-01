FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /src

COPY ["src/FileStorage.API/FileStorage.API.csproj", "src/FileStorage.API/"]
COPY ["src/FileStorage.Application/FileStorage.Application.csproj", "src/FileStorage.Application/"]
COPY ["src/FileStorage.Contracts/FileStorage.Contracts.csproj", "src/FileStorage.Contracts/"]
COPY ["src/FileStorage.Domain/FileStorage.Domain.csproj", "src/FileStorage.Domain/"]
COPY ["src/FileStorage.Infrastructure/FileStorage.Infrastructure.csproj", "src/FileStorage.Infrastructure/"]
COPY ["src/FileStorage.Persistence/FileStorage.Persistence.csproj", "src/FileStorage.Persistence/"]

RUN dotnet restore "src/FileStorage.API/FileStorage.API.csproj"

COPY ["src/", "src/"]

RUN dotnet build "src/FileStorage.API/FileStorage.API.csproj" -c Release -o /app/build

FROM builder AS publish
RUN dotnet publish "src/FileStorage.API/FileStorage.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5008
ENV ASPNETCORE_URLS=http://+:5008
ENTRYPOINT ["dotnet", "FileStorage.API.dll"]
