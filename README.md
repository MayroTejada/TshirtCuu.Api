# T-shirtCuu API (.NET 9)

Standalone Web API for the T-shirtCuu mock (separate from the Flutter app repo [TshirtCUU](https://github.com/MayroTejada/TshirtCUU)): quotes, catalog (designs, categories, sizes, colors, pricing), and admin JWT auth.

**Production persistence target: Azure SQL.** Configure only a SQL Server connection string (local Docker/SQL Server for development, Azure SQL connection string for production). This repo does not provision Azure resources.

## Solution layout

Clean architecture with exactly three projects:

| Project | Role |
|---------|------|
| `src/TshirtCuu.Domain` | Feature models + repository interfaces |
| `src/TshirtCuu.Services` | Feature services + EF Core SQL Server repositories + migrations |
| `src/TshirtCuu.Presentation` | ASP.NET Core 9 hosts, controllers, DI, connection string |

Features: **Quotes**, **Catalog**, **Admin** (auth with hashed passwords).

Solution file: [`TshirtCuu.sln`](./TshirtCuu.sln)

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [EF Core tools](https://learn.microsoft.com/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`
- SQL Server reachable at the connection string (e.g. local container on `localhost,1433`, or Azure SQL later)

## Connection string

Development placeholder (committed intentionally — **not** a real Azure password):

`appsettings.Development.json` → `ConnectionStrings:TshirtCuu`

```
Server=localhost,1433;Database=TshirtCuu;User Id=sa;Password=Your_strong_Password123;TrustServerCertificate=True;Encrypt=False
```

Match the `sa` password to your local SQL Server. Override via user-secrets if you prefer:

```bash
cd src/TshirtCuu.Presentation
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:TshirtCuu" "Server=localhost,1433;Database=TshirtCuu;User Id=sa;Password=YOUR_LOCAL_PASSWORD;TrustServerCertificate=True;Encrypt=False"
dotnet user-secrets set "Jwt:SigningKey" "your-local-dev-key-at-least-32-chars"
```

For **Azure SQL** later, set the same `ConnectionStrings:TshirtCuu` key to your Azure SQL connection string through environment variables, Key Vault, or user-secrets — do **not** commit production passwords. `appsettings.json` leaves the connection string empty on purpose.

## Apply migrations

From the repo root (with SQL Server running and the connection string correct):

```bash
dotnet ef database update \
  --project src/TshirtCuu.Services \
  --startup-project src/TshirtCuu.Presentation
```

To add a new migration after model changes:

```bash
dotnet ef migrations add <Name> \
  --project src/TshirtCuu.Services \
  --startup-project src/TshirtCuu.Presentation \
  --output-dir Persistence/Migrations
```

On Development startup the API also seeds catalog/admin/sample quotes **idempotently** once the database exists (after migrations).

## Run locally

```bash
cd src/TshirtCuu.Presentation
dotnet run
```

Or from the repo root:

```bash
dotnet run --project src/TshirtCuu.Presentation
```

By default (Development profile):

- HTTP: `http://localhost:5041`
- Swagger UI: `http://localhost:5041/swagger`

## Dev admin login

| Field | Value |
|-------|-------|
| Username | `admin` |
| Password | `admin` |

Password is stored as an ASP.NET Identity hash (`AdminUsers.PasswordHash`), not plaintext.

```bash
curl -s -X POST http://localhost:5041/api/admin/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin"}'
```

Use the returned `accessToken` as `Authorization: Bearer <token>` for admin endpoints.

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
dotnet build
```
