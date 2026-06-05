# Build context: Grotix_Web_Services/
# docker build -f src/HardwareDevice.Api/Dockerfile -t grotix-hardware-api .

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/HardwareDevice.Api/HardwareDevice.Api.csproj",                                "src/HardwareDevice.Api/"]
COPY ["src/BuildingBlocks/BuildingBlocks.csproj",                                         "src/BuildingBlocks/"]
COPY ["src/Grotix.Shared/Grotix.Shared.csproj",                                           "src/Grotix.Shared/"]
COPY ["src/Grotix.IAM/Grotix.IAM.csproj",                                                 "src/Grotix.IAM/"]
COPY ["src/Grotix.Profiles/Grotix.Profiles.csproj",                                       "src/Grotix.Profiles/"]
COPY ["src/Grotix.CultivationArea/Grotix.CultivationArea.csproj",                         "src/Grotix.CultivationArea/"]
COPY ["src/Grotix.HardwareDevice/Grotix.HardwareDevice.csproj",                           "src/Grotix.HardwareDevice/"]
COPY ["src/Grotix.Persistence/Grotix.Persistence.csproj",                                 "src/Grotix.Persistence/"]
COPY ["src/Grotix.Persistence.HardwareDevice/Grotix.Persistence.HardwareDevice.csproj",   "src/Grotix.Persistence.HardwareDevice/"]
COPY ["src/Grotix.Persistence.Telemetry/Grotix.Persistence.Telemetry.csproj",             "src/Grotix.Persistence.Telemetry/"]
COPY ["src/Grotix.Telemetry/Grotix.Telemetry.csproj",                                     "src/Grotix.Telemetry/"]
COPY ["src/Grotix.Contracts.Auth/Grotix.Contracts.Auth.csproj",                           "src/Grotix.Contracts.Auth/"]
COPY ["src/Grotix.Contracts.Profiles/Grotix.Contracts.Profiles.csproj",                   "src/Grotix.Contracts.Profiles/"]

RUN dotnet restore "src/HardwareDevice.Api/HardwareDevice.Api.csproj"

COPY . .

RUN dotnet publish "src/HardwareDevice.Api/HardwareDevice.Api.csproj" \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HardwareDevice.Api.dll"]
