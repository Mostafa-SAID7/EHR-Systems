FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Patient.API/Patient.API.csproj", "Patient.API/"]
COPY ["src/Patient.Application/Patient.Application.csproj", "Patient.Application/"]
COPY ["src/Patient.Domain/Patient.Domain.csproj", "Patient.Domain/"]
COPY ["src/Patient.Infrastructure/Patient.Infrastructure.csproj", "Patient.Infrastructure/"]
COPY ["src/Patient.Persistence/Patient.Persistence.csproj", "Patient.Persistence/"]

RUN dotnet restore "Patient.API/Patient.API.csproj"

COPY . .

RUN dotnet build "src/Patient.API/Patient.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Patient.API/Patient.API.csproj" -c Release -o /app/publish

FROM runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Patient.API.dll"]
