# Runbook: provisioning RootCauseDb for the EVT-2026-0418 diagnosis route

The `POST /api/v1/root-cause/incidents/{incidentId}/diagnoses` route (ADR-034)
assumes real data already exists — it does **not** seed on startup. Run these
steps once per environment before calling the route. All are dev-only (LocalDB).

## Prerequisites

- LocalDB (`(localdb)\mssqllocaldb`) reachable; the developer's own Windows login
  is sysadmin on it.
- Ollama running on `127.0.0.1:11434` with `nexus-dslm` and `nomic-embed-text`
  present (`ollama list`). See the ADR-033 evidence for building `nexus-dslm` from
  its Modelfile.

## 1. Apply migrations first (developer credentials, DDL)

Migrations come **before** the SQL logins: they are what create the databases, and the
login scripts in step 2 `USE` each database (`USE RootCauseDb;` fails with `Msg 911 …
does not exist` on a fresh machine). The scoped logins cannot do DDL, so migrations run
through each context's design-time factory (its own `Trusted_Connection`), not the host.

Use the **Infrastructure project as both `--project` and `--startup-project`**. The hosts
do not reference `Microsoft.EntityFrameworkCore.Design`, so pointing
`--startup-project` at `src/Hosts/Nexus1.RootCause.Host` fails with *"Your startup project
'Nexus1.RootCause.Host' doesn't reference Microsoft.EntityFrameworkCore.Design"*; every
`*.Infrastructure` project references it and carries the design-time factory.

```bash
dotnet tool restore   # dotnet-ef 8.0.11 from .config/dotnet-tools.json
dotnet ef database update --project src/Contexts/RootCause/Nexus1.RootCause.Infrastructure --startup-project src/Contexts/RootCause/Nexus1.RootCause.Infrastructure
```

(Verified 2026-09-25: the host-as-startup form fails as quoted above; this form prints
`No migrations were applied. The database is already up to date.` on a migrated database.
For the whole stack, repeat it for every `src/Contexts/<Name>/Nexus1.<Name>.Infrastructure`
— the `nexus1_app` script in step 2 grants on all seven databases, so they must all exist.)

## 2. Create the SQL logins

The running host uses two scoped logins — read-write `nexus1_app` (ADR-028) and
read-only `nexus1_explain` (ADR-033). Create both, **after** step 1, as your own
(sysadmin) Windows login:

```bash
sqlcmd -S "(localdb)\mssqllocaldb" -i docs/runbooks/local-scoped-sql-login.md-derived-script.sql   # nexus1_app (ADR-028)
sqlcmd -S "(localdb)\mssqllocaldb" -i docs/runbooks/local-explain-readonly-sql-login.md-derived-script.sql  # nexus1_explain (ADR-033)
```

(Use the T-SQL in `local-scoped-sql-login.md` and `local-explain-readonly-sql-login.md`.
On this box, invoke `sqlcmd` without `-E` and pass an absolute Windows path to `-i`.)
Then put both connection strings in RootCause.Host's User Secrets — the commands are in
those two runbooks (ADR-044; the tracked `appsettings.json` holds no connection strings).

## 3. Seed the fixture + populate embeddings (explicit provisioning command)

Seeds the grounding rows for both fixed incidents (`GroundingSeed`: EVT-2026-0418 on
unit 1, EVT-2026-0419 on unit 2), populates `Corpus.EmbeddingJson` via
`nomic-embed-text`, and creates the fixed EVT-2026-0420 Inconclusive case (ADR-040),
then exits. Writes go through `nexus1_app` (read-write); no DDL. Idempotent — safe to
re-run. `dotnet run` picks up `launchSettings.json` (Development), which loads User Secrets.

```bash
dotnet run --project src/Hosts/Nexus1.RootCause.Host -- provision
```

Expected log lines on a fresh database:
```
Seed complete: 17 components, 4 corpus chunks.
Embedding ingest complete: 4 newly embedded, 0 still missing an embedding.
Provisioned EVT-2026-0420 QA-escape case as Inconclusive (AnalysisId 20260420, unit 1).
```

On a re-run (verified 2026-09-25) the seed and ingest are no-ops:
```
Seed complete: 17 components, 4 corpus chunks.
Embedding ingest complete: 0 newly embedded, 0 still missing an embedding.
EVT-2026-0420 QA-escape case already provisioned (AnalysisId 20260420).
```

(Earlier revisions of this runbook showed `10 components, 2 corpus chunks` — the counts
before EVT-2026-0419 was added, ADR-037.) The 0420 case's two integration events wait in
RootCause's outbox and are published once RootCause.Host runs with RabbitMQ up.

If "still missing an embedding" is non-zero, Ollama or the embedding model is not
available — the semantic retrieval path stays dormant until it is (lexical
retrieval still works).

## 4. Verify

```bash
sqlcmd -S "(localdb)\mssqllocaldb" -d RootCauseDb -U nexus1_explain -P "<your local nexus1_explain password>" -Q "SELECT COUNT(*) AS corpus, SUM(CASE WHEN EmbeddingJson IS NULL THEN 0 ELSE 1 END) AS embedded FROM RootCause.Corpus;"
```

Then start the host normally (`dotnet run --project src/Hosts/Nexus1.RootCause.Host`)
and POST to the route.

## Optional: keep the model warm (reduce cold-start latency)

Cold qwen2.5:3b CPU inference is ~93–130 s (model load + first generation); warm calls
are ~20 s. The Host's chat-model call timeout is 150 s (ADR-033 addendum) so a cold
call succeeds, just slowly. To make cold-starts *rare* in practice, keep the model
resident by setting Ollama's own keep-alive on the **Ollama process** (not Host code) —
e.g. keep it loaded for 30 minutes:

```powershell
$env:OLLAMA_KEEP_ALIVE = "30m"   # or "-1" to keep loaded indefinitely
# (set before starting `ollama serve`, or user-level for persistence)
```

This is an optional ops-level lever, complementary to the 150 s timeout floor — it does
not eliminate the first cold call after an Ollama (re)start, only makes subsequent calls
warm. No Host change is involved.
