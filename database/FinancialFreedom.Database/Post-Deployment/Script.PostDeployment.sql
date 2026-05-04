/*
  Runs after the DAC publishes schema changes.
  Use :r syntax to include ordered scripts, e.g.:
    :r ..\Scripts\ReferenceData\001_example.sql

  ASP.NET Core Identity (AspNet* tables + __EFMigrationsHistory) is applied by the
  web app at startup (dotnet ef database update) or by running the generated script once:
    Scripts\Migrations\002_AddAspNetCoreIdentity_EfCore.sql
  Do not :r that script here unless you intend every publish to re-run it (usually you run it manually once).

  If you use SQL auth with login [CFP_FinancialFreedom], after the first publish from SSDT (user created WITHOUT LOGIN),
  run once on the server (SSMS) after creating the login:
    -- CREATE LOGIN [CFP_FinancialFreedom] WITH PASSWORD = N'...';
    -- ALTER USER [CFP_FinancialFreedom] WITH LOGIN = [CFP_FinancialFreedom];
*/

PRINT N'FinancialFreedom.Database post-deployment finished.';
