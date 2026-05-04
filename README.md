# Cursor_CFP_FinancialFreedom

## SSDT user `CFP_FinancialFreedom` (SQL71501)

The database project defines the user **without** `FOR LOGIN` so the build does not require a server-level **Login** object inside the DACPAC (which triggers SQL71501). After publishing to a new database, if your app connects with SQL authentication using that login name, create the **login** on the server (if needed) and link it: `ALTER USER [CFP_FinancialFreedom] WITH LOGIN = [CFP_FinancialFreedom];` (see comments in `database/FinancialFreedom.Database/Post-Deployment/Script.PostDeployment.sql`).

## SQL Server connection password (do not commit)

The SQL login password for `CFP_FinancialFreedom` belongs in **[User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)** for the Server project, not in `appsettings.json` (so it is never pushed to git). User secrets override the same `ConnectionStrings:DefaultConnection` key in Development.

From `src/Server`:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=CFP_FinancialFreedom;User Id=CFP_FinancialFreedom;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=True;MultipleActiveResultSets=true"
```

If `appsettings.json` already defines `DefaultConnection` without `Password=`, set the **full** connection string in secrets (secrets replace the whole value for that key).

## Authentication (ASP.NET Core Identity)

The hosted Blazor WebAssembly app uses **cookie-based** sign-in with Identity **JSON endpoints** (`/login?useCookies=true`, `/register`, `/manage/info`, `/logout`, `/roles`). The `AppDbContext` stores users in the same database as your app data.

### Development demo account

When `ASPNETCORE_ENVIRONMENT=Development`, a demo user is created if missing:

- **Email:** `demo@local`
- **Password:** `Passw0rd!`

Do not rely on this in production.

### Database: apply Identity schema

`FinancialGoals` is maintained by the **SSDT database project** (`database/FinancialFreedom.Database`). Identity tables (`AspNetUsers`, `AspNetRoles`, etc.) are maintained by **EF Core migrations** in `src/Server/Data/Migrations/` and are **excluded from touching** `FinancialGoals` so DACPAC and EF do not fight over that table.

**Option A — Let the API update the database (typical for local SQL Server)**

1. Ensure `FinancialGoals` (and security objects) already exist on the target database (publish the `.sqlproj` DACPAC, or run your existing scripts).
2. Set `ConnectionStrings:DefaultConnection` and `Database:UseInMemory` = `false` in configuration (see `src/Server/appsettings.Development.json`; override with [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) for local SQL).
3. From the repository root:

```powershell
dotnet ef database update --project src/Server --startup-project src/Server
```

The app also runs `Database.Migrate()` on startup when not using the in-memory provider.

**Option B — Run the generated SQL script manually (DBA / pipeline)**

An idempotent script matching the current EF migration is checked in at:

`database/FinancialFreedom.Database/Scripts/Migrations/002_AddAspNetCoreIdentity_EfCore.sql`

Run it **once** against your SQL Server database (e.g. in SSMS) if you prefer not to use `dotnet ef database update`. After it runs, the `__EFMigrationsHistory` row keeps `dotnet ef database update` from re-applying the same migration.

The SSDT project includes this file as `None` (reference only); it is **not** auto-run from post-deployment (see comments in `Post-Deployment/Script.PostDeployment.sql`).

**Regenerate the script after you add new migrations**

```powershell
cd src/Server
dotnet ef migrations script --idempotent -o "../../database/FinancialFreedom.Database/Scripts/Migrations/002_AddAspNetCoreIdentity_EfCore.sql"
```

(Adjust the output path if you rename the file.)

### EF CLI tool

The repo includes `dotnet-ef` in `.config/dotnet-tools.json`. Restore tools once:

```powershell
dotnet tool restore
```

### In-memory (tests / no SQL Server)

Set `Database:UseInMemory` to `true`. Tests force this via `FinancialFreedomWebAppFactory`. Optional: `Database:InMemoryDatabaseName` isolates multiple hosts.

## Page visit log

Blazor client navigations are posted to **`POST /api/PageVisitLog`** (anonymous). Rows are stored in **`PageVisitLogs`** (`Id`, `VisitedAt`, `UserId`, `UserName`, `PagePath`, `QueryString`). When a user is signed in, the API fills user fields from the auth cookie.

After pulling the latest migration, run **`dotnet ef database update`** (or apply the matching idempotent scripts under `database/FinancialFreedom.Database/Scripts/Migrations/` manually, e.g. `003_AddPageVisitLogs.sql`, then **`004_AddApplicationUserLastLoginAt_EfCore.sql`** for `AspNetUsers.LastLoginAt`). The SSDT project also includes **`dbo/Tables/PageVisitLogs.sql`** for DACPAC publishes.

Signed-in users can open **`/page-visit-log`** (nav: **Page visits**) to call **`GET /api/PageVisitLog`** and browse recent rows.
