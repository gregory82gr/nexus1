# Runbook: the read-only `nexus1_explain` SQL login (H7)

The `Nexus1.RootCause.Explain` project — the only project allowed to touch a
served language model (ADR-032, ADR-033) — connects to `RootCauseDb` at
**read-only** runtime through `nexus1_explain`, a SQL-authenticated login with
`db_datareader` plus **explicit `DENY`** on every write and execute verb. This
is the H7 "no write path" control of `From Flood to Cause` Ch.9 made real: the
model and its retrieval physically cannot mutate the grounding store, enforced by
the database, not by C# discipline. See ADR-033 for why.

This is distinct from `nexus1_app` (ADR-028, read/write, used by the running
hosts) and from the developer's own `Trusted_Connection` (sysadmin on LocalDB,
used only for migrations and provisioning). Embedding **ingest** — a one-time
provisioning write — does NOT use this login; it runs under a write-capable
connection at provisioning time. Only the read-time retrieve+generate path uses
`nexus1_explain`.

## Create the login (idempotent — safe to re-run)

Run once per LocalDB instance, as the developer's own (sysadmin)
Windows-integrated connection:

```sql
USE master;
GO

IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'nexus1_explain')
BEGIN
    DROP LOGIN nexus1_explain;
END
GO

CREATE LOGIN nexus1_explain WITH PASSWORD = 'Nexus1Explain!Dev2026Local', CHECK_POLICY = ON;
GO

USE RootCauseDb;
GO

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'nexus1_explain')
BEGIN
    DROP USER nexus1_explain;
END
GO

CREATE USER nexus1_explain FOR LOGIN nexus1_explain;
GO

-- Read: the grounding schema (Corpus, Component, ...) and dbo (migrations history).
ALTER ROLE db_datareader ADD MEMBER nexus1_explain;
GO

-- Write path denied outright, at the schema level, belt-and-suspenders over the
-- read-only role: the explain runtime cannot INSERT/UPDATE/DELETE/EXECUTE.
DENY INSERT   ON SCHEMA::RootCause  TO nexus1_explain;
DENY UPDATE   ON SCHEMA::RootCause  TO nexus1_explain;
DENY DELETE   ON SCHEMA::RootCause  TO nexus1_explain;
DENY EXECUTE  ON SCHEMA::RootCause  TO nexus1_explain;
DENY INSERT   ON SCHEMA::messaging  TO nexus1_explain;
DENY UPDATE   ON SCHEMA::messaging  TO nexus1_explain;
DENY DELETE   ON SCHEMA::messaging  TO nexus1_explain;
GO
```

Run via `sqlcmd -S "(localdb)\mssqllocaldb" -i <script>.sql` as the developer's
own Windows-integrated connection.

## What the login can and cannot do — verify directly, not assume

- **Can**: `SELECT` on `RootCause` (Corpus/Component/Edge/Historian/DiagnosisRun/
  Candidate/Audit) and `dbo` (`__EFMigrationsHistory_RootCause`).
- **Cannot**: `INSERT`/`UPDATE`/`DELETE`/`EXECUTE` — each fails with
  `Msg 229/230: permission denied`. `IS_SRVROLEMEMBER('sysadmin')` and
  `IS_MEMBER('db_owner')` both return `0`.
- **No access** to any other database (no `USER` mapping there).

Verification snippet (connected as `nexus1_explain`):

```sql
SELECT COUNT(*) FROM RootCause.Corpus;              -- succeeds
INSERT INTO RootCause.Corpus (DocId, TrustTier, Body, SourceLabel)
    VALUES (99, 0, 'x', 'x');                        -- Msg 229: INSERT denied
```

## Connection string

`appsettings` key `ConnectionStrings:RootCauseExplainDb`:

```
Server=(localdb)\mssqllocaldb;Database=RootCauseDb;User Id=nexus1_explain;Password=Nexus1Explain!Dev2026Local;
```

`Nexus1Explain!Dev2026Local` is committed in plaintext, the same LocalDB-only dev
convention as `nexus1_app` (ADR-028). Revisit before anything beyond local
development.
