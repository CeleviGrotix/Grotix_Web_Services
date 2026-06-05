# Build context: Grotix_Web_Services/  (raíz de la solución)
# docker build -f src/Profiles.Api/Dockerfile -t grotix-profiles-api .

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# --- Copiar .csproj para cachear restore ---
COPY ["src/Profiles.Api/Profiles.Api.csproj",                                     "src/Profiles.Api/"]
COPY ["src/BuildingBlocks/BuildingBlocks.csproj",                                  "src/BuildingBlocks/"]
COPY ["src/Grotix.Shared/Grotix.Shared.csproj",                                    "src/Grotix.Shared/"]
COPY ["src/Grotix.IAM/Grotix.IAM.csproj",                                          "src/Grotix.IAM/"]
COPY ["src/Grotix.Profiles/Grotix.Profiles.csproj",                                "src/Grotix.Profiles/"]
COPY ["src/Grotix.Persistence/Grotix.Persistence.csproj",                          "src/Grotix.Persistence/"]
COPY ["src/Grotix.Contracts.Auth/Grotix.Contracts.Auth.csproj",                    "src/Grotix.Contracts.Auth/"]
COPY ["src/Grotix.Contracts.Integration/Grotix.Contracts.Integration.csproj",      "src/Grotix.Contracts.Integration/"]
COPY ["src/Grotix.Contracts.Profiles/Grotix.Contracts.Profiles.csproj",            "src/Grotix.Contracts.Profiles/"]

RUN dotnet restore "src/Profiles.Api/Profiles.Api.csproj"

# --- Copiar todo el código fuente ---
COPY . .

RUN dotnet publish "src/Profiles.Api/Profiles.Api.csproj" \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Profiles.Api.dll"]
