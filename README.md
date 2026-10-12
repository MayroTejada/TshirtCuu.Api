# T-shirtCuu API (.NET 9)

Local Web API for the T-shirtCuu mock: quotes, catalog (designs, categories, sizes, colors, pricing), and admin JWT auth.

Hosting is **undecided** — this project is meant to run on your machine for now. Pick a host later when you are ready; nothing here assumes a specific cloud vendor.

## Solution layout

Clean architecture with exactly three projects:

| Project | Role |
|---------|------|
| `src/TshirtCuu.Domain` | Feature models + repository interfaces |
| `src/TshirtCuu.Services` | Feature services + in-memory repository implementations + DI |
| `src/TshirtCuu.Presentation` | ASP.NET Core 9 hosts, controllers, JWT wiring |

Features mirror the Flutter app: **Quotes**, **Catalog**, **Admin** (auth). Persistence is in-memory behind repository interfaces (SQL Server can replace the implementations later).

Solution file: [`TshirtCuu.sln`](./TshirtCuu.sln)

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

## Run locally

```bash
cd api/src/TshirtCuu.Presentation
dotnet run
```

Or from the solution folder:

```bash
cd api
dotnet run --project src/TshirtCuu.Presentation
```

By default (Development profile):

- HTTP: `http://localhost:5041`
- Swagger UI: `http://localhost:5041/swagger`

`ASPNETCORE_ENVIRONMENT=Development` loads the dev JWT signing key from `appsettings.Development.json`.

### Optional: user-secrets instead of Development appsettings

```bash
cd api/src/TshirtCuu.Presentation
dotnet user-secrets init
dotnet user-secrets set "Jwt:SigningKey" "your-local-dev-key-at-least-32-chars"
```

Do **not** put production signing keys in git. `appsettings.json` leaves `Jwt:SigningKey` empty on purpose.

## Dev admin login

| Field | Value |
|-------|-------|
| Username | `admin` |
| Password | `admin` |

```bash
curl -s -X POST http://localhost:5041/api/admin/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin"}'
```

Use the returned `accessToken` as `Authorization: Bearer <token>` for admin endpoints (list/update quotes, mutate catalog).

Public (no token): create quote, read catalog/sizes/colors/pricing.

## Useful endpoints

| Method | Path | Auth |
|--------|------|------|
| POST | `/api/admin/auth/login` | Anonymous |
| GET | `/api/admin/auth/me` | Admin JWT |
| POST | `/api/quotes` | Anonymous |
| GET | `/api/quotes` | Admin JWT |
| GET | `/api/quotes/{id}` | Admin JWT |
| PATCH | `/api/quotes/{id}/status` | Admin JWT |
| GET | `/api/catalog/designs` | Anonymous |
| GET | `/api/catalog/categories` | Anonymous |
| GET | `/api/catalog/sizes` | Anonymous |
| GET | `/api/catalog/colors` | Anonymous |
| GET | `/api/catalog/pricing` | Anonymous |
| POST/PUT/DELETE | `/api/catalog/...` | Admin JWT |

## Build

```bash
cd api
dotnet build
```
