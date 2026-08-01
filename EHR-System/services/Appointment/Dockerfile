FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Appointment.API/Appointment.API.csproj", "Appointment.API/"]
COPY ["src/Appointment.Application/Appointment.Application.csproj", "Appointment.Application/"]
COPY ["src/Appointment.Domain/Appointment.Domain.csproj", "Appointment.Domain/"]
COPY ["src/Appointment.Persistence/Appointment.Persistence.csproj", "Appointment.Persistence/"]

RUN dotnet restore "Appointment.API/Appointment.API.csproj"

COPY . .

RUN dotnet build "src/Appointment.API/Appointment.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Appointment.API/Appointment.API.csproj" -c Release -o /app/publish

FROM runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Appointment.API.dll"]
