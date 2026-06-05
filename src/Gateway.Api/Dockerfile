# Build context: Grotix_Web_Services/
# docker build -f src/Gateway.Api/Dockerfile -t grotix-gateway .

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Gateway.Api/Gateway.Api.csproj",                                    "src/Gateway.Api/"]
COPY ["src/BuildingBlocks/BuildingBlocks.csproj",                              "src/BuildingBlocks/"]
COPY ["src/Grotix.Contracts.Integration/Grotix.Contracts.Integration.csproj", "src/Grotix.Contracts.Integration/"]

RUN dotnet restore "src/Gateway.Api/Gateway.Api.csproj"

COPY . .

RUN dotnet publish "src/Gateway.Api/Gateway.Api.csproj" \
    -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
