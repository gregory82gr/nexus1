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

## 1. Create the SQL logins

The running host uses two scoped logins — read-write `nexus1_app` (ADR-028) and
read-only `nexus1_explain` (ADR-033). Create both if not already present:

```bash
sqlcmd -S "(localdb)\mssqllocaldb" -i docs/runbooks/local-scoped-sql-login.md-derived-script.sql   # nexus1_app (ADR-028)
sqlcmd -S "(localdb)\mssqllocaldb" -i docs/runbooks/local-explain-readonly-sql-login.md-derived-script.sql  # nexus1_explain (ADR-033)
```

(Use the T-SQL in `local-scoped-sql-login.md` and `local-explain-readonly-sql-login.md`.
On this box, invoke `sqlcmd` without `-E` and pass an absolute Windows path to `-i`.)

## 2. Apply migrations (developer credentials, DDL)

The scoped logins cannot do DDL. Create/update the schema with the design-time
factory (its own `Trusted_Connection`), not the host:

```bash
dotnet ef database update --project src/Contexts/RootCause/Nexus1.RootCause.Infrastructure --startup-project src/Hosts/Nexus1.RootCause.Host
```

## 3. Seed the fixture + populate embeddings (explicit provisioning command)

Seeds the grounding rows (`GroundingSeed`) and populates `Corpus.EmbeddingJson`
via `nomic-embed-text`, then exits. Writes go through `nexus1_app` (read-write);
no DDL. Idempotent — safe to re-run.

```bash
dotnet run --project src/Hosts/Nexus1.RootCause.Host -- provision
```

Expected log lines:
```
Seed complete: 10 components, 2 corpus chunks.
Embedding ingest complete: 2 newly embedded, 0 still missing an embedding.
```

If "still missing an embedding" is non-zero, Ollama or the embedding model is not
available — the semantic retrieval path stays dormant until it is (lexical
retrieval still works).

## 4. Verify

```bash
sqlcmd -S "(localdb)\mssqllocaldb" -d RootCauseDb -U nexus1_explain -P "Nexus1Explain!Dev2026Local" -Q "SELECT COUNT(*) AS corpus, SUM(CASE WHEN EmbeddingJson IS NULL THEN 0 ELSE 1 END) AS embedded FROM RootCause.Corpus;"
```

Then start the host normally (`dotnet run --project src/Hosts/Nexus1.RootCause.Host`)
and POST to the route.
