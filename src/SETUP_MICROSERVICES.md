# Microservices local setup (Profiles + CultivationArea + Gateway)

## Environment configuration

- El repo ahora soporta un archivo raíz `.env` cargado automáticamente por `Profiles.Api`, `CultivationArea.Api` y `Gateway.Api` antes de construir la configuración.
- Usa `.env.example` como referencia. El archivo `.env` local tiene prioridad práctica sobre `appsettings.Development.json`, así que sirve para forzar MySQL/RabbitMQ/URLs locales sin editar los JSON del repo.
- Para entorno local, las variables más importantes son:
  - `ConnectionStrings__DefaultConnection`
  - `MySql__ServerVersion`
  - `TokenSettings__Secret`
  - `RabbitMq__Enabled`
  - `ReverseProxy__Clusters__profiles-cluster__Destinations__d1__Address`
  - `ReverseProxy__Clusters__cultivation-cluster__Destinations__d1__Address`

## Services and ports

- `Profiles.Api`: `http://localhost:5101`
- `CultivationArea.Api`: `http://localhost:5102`
- `Gateway.Api`: `http://localhost:5100`

## Run order

1. Ensure MySQL is running and `grotix_core` exists.
2. Verifica o ajusta `.env` para que `ConnectionStrings__DefaultConnection` apunte a tu MySQL local.
3. Apply EF migrations (usa `Profiles.Api`; incluye `Shared` + migraciones):

```bash
dotnet ef database update --project src/Profiles.Api/Profiles.Api.csproj
```

Incluye migraciones como `UpdateRolesAndPermissions` y `AddAssociationInvite` (tabla `association_invite`).

4. Start `Profiles.Api`:

```bash
dotnet run --project src/Profiles.Api/Profiles.Api.csproj --urls http://localhost:5101
```

5. Start `CultivationArea.Api`:

```bash
dotnet run --project src/CultivationArea.Api/CultivationArea.Api.csproj --urls http://localhost:5102
```

6. Start `Gateway.Api`:

```bash
dotnet run --project src/Gateway.Api/Gateway.Api.csproj --urls http://localhost:5100
```

## Roles y permisos (resumen)

| `role.Name` (JWT) | Uso |
|-------------------|-----|
| `admin` | Administrador del sistema (rol 1) |
| `staff` | Operador técnico (rol 2) |
| `user_admin` | Gestor de organización (rol 3) |
| `user_basic` | Agricultor básico (rol 4) — asignado vía **invitación** |
| `user_advanced` | Agricultor avanzado (rol 5) |

En el login, además de `role`, el JWT incluye un claim `permission` por cada código (p. ej. `TELEMETRY_VIEW`). Definidos en `KnownPermissionCodes` y en la migración `UpdateRolesAndPermissions`.

## Routes through gateway

### New prefixed routes

- Profiles: `/api/v1/profiles/*` -> forwarded to `Profiles.Api` as `/api/v1/*`
- Cultivation: `/api/v1/cultivation/*` -> forwarded to `CultivationArea.Api` as `/api/v1/*`

### Contratos (`POST`/`GET /api/v1/contracts`)

- **`POST`** (solo `admin` / `staff`): crea el contrato **y** el usuario **`user_admin`** para la `AssociationId` indicada. Requiere `orgAdminEmail`, `orgAdminPassword` y opcional `orgAdminName`. Si la asociación **ya tiene** un `user_admin`, devuelve error (un administrador por organización en este flujo).
- **`GET`** lista todos los contratos (`admin`/`staff`) o solo los de la propia asociación (`user_admin`).

### Invitaciones y registro (`association_invite`)

1. **`POST /api/v1/associations/{associationId}/invites`** (JWT): crea una invitación. Cuerpo `{ "roleId": 4 | 5, "expiresAt": null }`. Permitido: `admin`, `staff`, o `user_admin` de esa misma asociación. La respuesta incluye **`token`** en claro **una sola vez** (guárdalo).
2. **`POST /api/v1/auth/register`**: `{ "email", "password", "inviteToken" }`. El usuario queda con el rol y la organización definidos en la invitación (`user_basic` o `user_advanced`). Si el token está usado o caducado, falla.

El hash SHA-256 del token es lo que se guarda en BD (`TokenHash`).

### Users (directorio de agricultores)

- `GET /api/v1/users` — solo usuarios con rol agricultor (`user_admin`, `user_basic`, `user_advanced`); no lista cuentas `admin`/`staff`.
- `GET /api/v1/users/{id}` — mismo subconjunto por id (autorización: `admin`/`staff` global; `user_admin` solo misma asociación).
- `GET /api/v1/profile/me` — perfil del usuario autenticado (incluye cualquier rol).

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
- En local puedes dejar `RabbitMq__Enabled=false` en `.env` si solo quieres migrar y probar REST sin broker.
- Si RabbitMQ no está en marcha o falla usuario/clave, las APIs **siguen arrancando**; verás un warning en log y no se publicará/consumirá hasta que el broker responda (conexión lazy).
- Flujo de prueba: crear invitación (`POST .../associations/{id}/invites`), luego registrar con `inviteToken` vía `POST /api/v1/auth/register`. Tras el registro, `Profiles.Api` publica `user.registered` al exchange `grotix.events`. `CultivationArea.Api` consume la cola `cultivation.user.registered` y escribe el payload en log.
