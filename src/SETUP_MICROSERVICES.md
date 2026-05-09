# Microservices local setup (Profiles + CultivationArea + Gateway)

## Services and ports

- `Profiles.Api`: `http://localhost:5101`
- `CultivationArea.Api`: `http://localhost:5102`
- `Gateway.Api`: `http://localhost:5100`

## Run order

1. Ensure MySQL is running and `grotix_core` exists.
2. Apply EF migrations (usa `Profiles.Api`; incluye `Shared` + migraciones):

```bash
dotnet ef database update --project src/Profiles.Api/Profiles.Api.csproj
```

3. Start `Profiles.Api`:

```bash
dotnet run --project src/Profiles.Api/Profiles.Api.csproj --urls http://localhost:5101
```

4. Start `CultivationArea.Api`:

```bash
dotnet run --project src/CultivationArea.Api/CultivationArea.Api.csproj --urls http://localhost:5102
```

5. Start `Gateway.Api`:

```bash
dotnet run --project src/Gateway.Api/Gateway.Api.csproj --urls http://localhost:5100
```

## Routes through gateway

### New prefixed routes

- Profiles: `/api/v1/profiles/*` -> forwarded to `Profiles.Api` as `/api/v1/*`
- Cultivation: `/api/v1/cultivation/*` -> forwarded to `CultivationArea.Api` as `/api/v1/*`

### Legacy compatibility routes

- Profiles: `/api/v1/auth/*`, `/api/v1/users/*`, `/api/v1/contracts/*`, `/api/v1/staff/*`, `/api/v1/roles/*`, `/api/v1/associations/*`
- Cultivation: `/api/v1/farms/*`, `/api/v1/zones/*`, `/api/v1/catalog/*`

## Health checks

En el **gateway** (`5100`), mismo path que en Profiles (proxied a `5101`):

- `GET http://localhost:5100/live` — proceso vivo (backend).
- `GET http://localhost:5100/ready/core` — readiness MySQL central (`ready` tag).
- `GET http://localhost:5100/ready/telemetry` — readiness Timescale (`timescale` tag).

Directo en APIs: `5101` / `5102` también exponen `/live`, `/ready/core`, `/ready/telemetry`.

## RabbitMQ (opcional)

- Broker AMQP: `localhost:5672`. UI: `http://localhost:15672` (usuario/contraseña por defecto `guest`/`guest`).
- Arranque con Docker (raíz del repo): `docker compose -f docker-compose.rabbitmq.yml up -d`
- Configuración: sección `RabbitMq` en `appsettings` de `Profiles.Api` y `CultivationArea.Api`. Con `Enabled: false` no se conecta ni publica ni consume.
- Si RabbitMQ no está en marcha o falla usuario/clave, las APIs **siguen arrancando**; verás un warning en log y no se publicará/consumirá hasta que el broker responda (conexión lazy).
- Flujo de prueba: registrar usuario vía `POST /api/v1/auth/register` (Profiles o gateway). Tras el registro, `Profiles.Api` publica el evento `user.registered` al exchange `grotix.events`. `CultivationArea.Api` consume la cola `cultivation.user.registered` y escribe el payload en log.
