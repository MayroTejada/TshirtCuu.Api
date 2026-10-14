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

## Email (Zoho SMTP — Lista / “ya quedó”)

When an admin sets a quote status to **Lista**, the API emails the customer that the shirt is ready. SMTP is read only from configuration / environment (nothing secret in git).

| App Setting / env var | Default | Notes |
|----------------------|---------|--------|
| `Email__Host` | `smtp.zoho.com` | Zoho SMTP host |
| `Email__Port` | `465` | `465` → MailKit `SslOnConnect`; `587` → `StartTls` |
| `Email__Username` | _(empty)_ | Zoho mailbox login |
| `Email__Password` | _(empty)_ | Zoho app password / mailbox password |
| `Email__From` | _(empty)_ | From address (usually same as username) |
| `Email__FromName` | `T-SHIRTCUU` | Display name |
| `Email__NotifyTo` | _(empty)_ | Inbox for new-quote alerts (defaults to `Email__From`) |

Sending uses **MailKit** (not `System.Net.Mail`). Port **465** uses implicit SSL (`SslOnConnect`); port **587** uses `StartTls`. Azure App Settings stay `Email__*` only — nothing secret in git.

On **POST /api/quotes**, after the quote is saved, the API:

1. Emails an **admin alert** (folio, customer name, email, phone) to `Email__NotifyTo` or `Email__From`.
2. Emails the **customer** a receipt of their quote (folio; each shirt’s fit, color, size, quantity, front and back; estimated total from pricing rules when available).

Create always succeeds if the quote saved; each mail’s success/failure is only written to application logs (never faked as success).

If `Email__Username`, `Email__Password`, or `Email__From` is missing, **Lista still saves** and the PATCH response reports that mail was not sent (`emailSent: false` + `emailNote` + destination `to`). SMTP failures put the **full exception chain** in `emailNote` and do not fail the status change.

Admin resend (same SMTP path, no status change): `POST /api/quotes/{id}/lista-email` → `{ emailSent, emailNote, to }`.

```bash
export Email__Host=smtp.zoho.com
export Email__Port=465
export Email__Username='orders@yourdomain.com'
export Email__Password='<secret>'
export Email__From='orders@yourdomain.com'
export Email__FromName='T-SHIRTCUU'
export Email__NotifyTo='orders@yourdomain.com'   # optional; defaults to Email__From
```

## Azure Blob Storage (private `imagenes`)

Catalog uploads and customer artwork go to a **private** blob container named `imagenes` (created on first upload if missing). Clients never use a public blob URL — reads and admin PNG download stream through `GET /api/images/{blobPath}`.

| App Setting / env var | Notes |
|----------------------|--------|
| `AzureStorage__ConnectionString` | Storage account connection string (App Setting; never commit) |
| `ConnectionStrings__AzureStorage` | Alternate: Connection strings blade entry named `AzureStorage` |
| `AzureStorage__ContainerName` | Optional; default `imagenes` |

If `AzureStorage__ConnectionString` is missing, the API **still starts**. Upload endpoints return **503** with a clear error. Quote create without storage still works (artwork paths stay null).

```bash
export AzureStorage__ConnectionString='DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net'
```

Endpoints:

- `POST /api/quotes` — creates the quote; optional `frontImageBase64` / `backImageBase64` are uploaded only after save
- `POST /api/quotes/{quoteId}/shirts/{shirtId}/artwork?side=front|back` — multipart customer artwork after create (preferred)
- `POST /api/images?kind=catalog` — admin only (standalone customer artwork upload is not allowed)
- `POST /api/catalog/designs/upload` — admin multipart (`name`, `categoryId`, `file`) → blob + design row
- `GET /api/images/{*blobPath}` — stream bytes through the API

Blob paths are stored on `Designs.ImageBlobPath` and `ShirtLines.FrontImageBlobPath` / `BackImageBlobPath` (see TShirtMigration).

## Error log (admin Errores)

Unhandled exceptions and failed operations (including failed quote artwork uploads) are written to SQL via `usp_ErrorLogs_Insert`. Logging failures never fail the HTTP request.

| Endpoint | Auth | Notes |
|----------|------|--------|
| `GET /api/admin/errors?take=100` | Admin JWT | Latest rows (newest first) |

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
