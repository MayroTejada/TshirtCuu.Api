# T-shirtCuu API (.NET 9)

Standalone Web API for T-shirtCuu: quotes, catalog, and admin JWT auth.

**Persistence:** Azure SQL via **stored procedures**. Schema and procs live in the private repo [TShirtMigration](https://github.com/MayroTejada/TShirtMigration) — not in this repository. This API calls those procedures with `Microsoft.Data.SqlClient` + Dapper. **No Entity Framework.**

## Solution layout

| Project | Role |
|---------|------|
| `src/TshirtCuu.Domain` | Feature models + repository interfaces |
| `src/TshirtCuu.Services` | Feature services + SqlClient repository implementations |
| `src/TshirtCuu.Presentation` | ASP.NET Core 9 hosts, controllers, DI |

Features: **Quotes**, **Catalog**, **Admin**.

## Database setup

1. Apply `001_create.sql` from [TShirtMigration](https://github.com/MayroTejada/TShirtMigration) against Azure SQL:
   - Server: `tshirtcuu.database.windows.net`
   - Database: `free-sql-db-5621248`
2. Never commit the SQL password or a real connection string into this repo.

## Connection string (no secrets in git)

Set the connection string with an environment variable (recommended):

```bash
export ConnectionStrings__TshirtCuu='Server=tcp:tshirtcuu.database.windows.net,1433;Initial Catalog=free-sql-db-5621248;User ID=<user>;Password=<secret>;Encrypt=True;TrustServerCertificate=False;'
```

Or user-secrets:

```bash
cd src/TshirtCuu.Presentation
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:TshirtCuu" "<your-connection-string>"
dotnet user-secrets set "Jwt:SigningKey" "your-local-dev-key-at-least-32-chars"
```

`appsettings.json` leaves `ConnectionStrings:TshirtCuu` empty on purpose. `appsettings.Development.json` has a **dev-only JWT signing key** and **no** database password.

## Run locally

```bash
# after ConnectionStrings__TshirtCuu is set and TShirtMigration script applied
cd src/TshirtCuu.Presentation
dotnet run
```

- HTTP: `http://localhost:5041`
- Swagger: `http://localhost:5041/swagger`

## Dev admin login

| Field | Value |
|-------|-------|
| Username | `admin` |
| Password | `admin` |

Password is stored as an ASP.NET Identity hash in `AdminUsers` (seeded by TShirtMigration).

```bash
curl -s -X POST http://localhost:5041/api/admin/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin"}'
```

## Build

```bash
dotnet build
```
