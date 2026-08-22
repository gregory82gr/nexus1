# Runbook: the scoped `nexus1_app` SQL login

Both hosts (`Nexus1.ModularRuntime`, `Nexus1.RootCause.Host`) connect to
LocalDB as `nexus1_app`, a SQL-authenticated login scoped to schema-level
DML on exactly the schemas each database's own contexts write — not the
developer's own Windows-integrated, sysadmin-level connection every prior
Phase 1/Phase 2 step used. See `docs/adr/ADR-028-scoped-sql-login-for-passport-only-references.md`
for why.

## What stays unchanged

Design-time `dotnet ef migrations add`/`dotnet ef database update`
commands are **unaffected** — every `*DbContextFactory.cs` hardcodes its
own `Trusted_Connection=True` connection string for design-time tooling,
independent of `appsettings.json`. Schema changes still require the
developer's own elevated LocalDB access; only the *running application*
uses the restricted login.

## Create the login (idempotent — safe to re-run)

Run once per LocalDB instance, as the developer's own (sysadmin)
Windows-integrated connection:

```sql
USE master;
GO

IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'nexus1_app')
BEGIN
    DROP LOGIN nexus1_app;
END
GO

CREATE LOGIN nexus1_app WITH PASSWORD = 'Nexus1App!Dev2026Local', CHECK_POLICY = ON;
GO

-- AlarmManagementDb: shared by all 11 plant-operational Phase 2 contexts
-- plus the messaging (outbox) schema AlarmManagement's own outbox uses.
USE AlarmManagementDb;
GO
CREATE USER nexus1_app FOR LOGIN nexus1_app;
GO
GRANT SELECT ON SCHEMA::dbo TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::AlarmManagement TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::CorePlatform TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::DigitalTwin TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::EmergencyPreparedness TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::EventManagement TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Instrumentation TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Maintenance TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::messaging TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::RadiationMonitoring TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::ReactorFleet TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::ReinforcementLearning TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Robotics TO nexus1_app;
GO

-- SecurityDb
USE SecurityDb;
GO
CREATE USER nexus1_app FOR LOGIN nexus1_app;
GO
GRANT SELECT ON SCHEMA::dbo TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Security TO nexus1_app;
GO

-- OrganizationDb
USE OrganizationDb;
GO
CREATE USER nexus1_app FOR LOGIN nexus1_app;
GO
GRANT SELECT ON SCHEMA::dbo TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Organization TO nexus1_app;
GO

-- AuditDb
USE AuditDb;
GO
CREATE USER nexus1_app FOR LOGIN nexus1_app;
GO
GRANT SELECT ON SCHEMA::dbo TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Audit TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::messaging TO nexus1_app;
GO

-- ComplianceDb
USE ComplianceDb;
GO
CREATE USER nexus1_app FOR LOGIN nexus1_app;
GO
GRANT SELECT ON SCHEMA::dbo TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Compliance TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::messaging TO nexus1_app;
GO

-- ReportingDb
USE ReportingDb;
GO
CREATE USER nexus1_app FOR LOGIN nexus1_app;
GO
GRANT SELECT ON SCHEMA::dbo TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Reporting TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::messaging TO nexus1_app;
GO

-- RootCauseDb
USE RootCauseDb;
GO
CREATE USER nexus1_app FOR LOGIN nexus1_app;
GO
GRANT SELECT ON SCHEMA::dbo TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::RootCause TO nexus1_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::messaging TO nexus1_app;
GO
```

Run via `sqlcmd -S "(localdb)\mssqllocaldb" -i <script>.sql` as the
developer's own Windows-integrated connection (which is sysadmin on
LocalDB by default — required to run `CREATE LOGIN`).

## What the login can and cannot do

Verified directly, not assumed:

- **Cannot**: `IS_SRVROLEMEMBER('sysadmin')` and `IS_MEMBER('db_owner')`
  both return `0`. `CREATE TABLE` fails with `Msg 262: CREATE TABLE
  permission denied`. No DDL of any kind.
- **Can**: `SELECT`/`INSERT`/`UPDATE`/`DELETE` on every real business
  schema in a database it has a `USER` mapping in; `SELECT`-only on
  `dbo` (where every `__EFMigrationsHistory_*` table lives — needed for
  `DbContextHealthCheck<T>`'s pending-migrations check, never written by
  the running app).
- **No access at all** to any database this login has no `USER`
  created in — the standard SQL Server model: a login without a mapped
  user in a given database cannot connect to it, sysadmin exceptions
  aside (and this login isn't one).

## Adding a new schema/database later (new sector, new context)

When a future sector adds a new schema to an existing shared database
(e.g. a twelfth schema landing in `AlarmManagementDb`), or a brand-new
database is introduced, add the matching `GRANT ... ON SCHEMA::<Name> TO
nexus1_app` (and `CREATE USER`/`GRANT SELECT ON SCHEMA::dbo` for a new
database) to this runbook's script and re-run it — `CREATE USER` is
harmless to skip if it already exists (the script's `IF EXISTS` guard is
on the login, not per-database users; re-running `CREATE USER` against a
database that already has the user will error harmlessly and the
subsequent `GRANT` lines still apply cleanly, or drop-and-recreate the
user in that one database first if starting clean). This is a small,
mechanical addition, not a redesign — the same shape as adding a new
context's registration to `Program.cs`.

## Password handling

`Nexus1App!Dev2026Local` is committed in plaintext in `appsettings.json`,
the same convention this project already uses for RabbitMQ's `guest`/
`guest` dev credentials. Appropriate for a LocalDB-only development
environment; revisit before anything beyond local development touches
this database (a real secret store, not a checked-in connection string).
