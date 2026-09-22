# Evidence — real query route for the EVT-2026-0418 diagnosis pipeline

**Date:** 2026-09-22
**ADR:** ADR-034
**Scope:** One synchronous HTTP route on `Nexus1.RootCause.Host` that runs the
committed pipeline (ADR-032/ADR-033) for the seeded incident and returns its
sealed result — proven end to end over the network, not just in in-process tests.

## Built

| Piece | Location |
|---|---|
| `POST /api/v1/root-cause/incidents/{incidentId}/diagnoses` | `src/Hosts/Nexus1.RootCause.Host/Program.cs` |
| `RunFixedIncidentDiagnosisCommand` + `DiagnosisResponse` DTO | Application/Diagnosis |
| `RunFixedIncidentDiagnosisCommandHandler` (Result<T>, hand-rolled, no MediatR) | Application/Diagnosis |
| `FixedIncident` — single source of the incident identity | Application/Diagnosis (GroundingSeed delegates to it) |
| `AddRootCauseReadOnlyRetrieval` — split read-only retrieval context (H7) | Infrastructure/ServiceCollectionExtensions |
| Host wiring: `AddRootCauseExplain` + read-only connection + route | Host/Program.cs, appsettings.json |
| `Provisioning` (`dotnet run -- provision`, explicit, not on startup) | Host/Provisioning.cs |
| Provisioning runbook | `docs/runbooks/local-rootcause-diagnosis-provisioning.md` |

## Gate results

- `dotnet build Nexus1.Runtime.sln` → **0 warnings / 0 errors**.
- `Nexus1.ArchitectureTests` → **8/8** — the served-model-package rule still holds
  with `Nexus1.RootCause.Host` now referencing the Explain **project** (a
  ProjectReference, not a served-model PackageReference).
- `Nexus1.RootCause.UnitTests` → **35/35**.
- `Nexus1.RootCause.ComponentTests` → **53/53** (real LocalDB + live Ollama) — the
  `GroundingSeed`→`FixedIncident` refactor caused no regression.

## Provisioning (explicit, one-off)

`dotnet run --project src/Hosts/Nexus1.RootCause.Host -- provision`:
```
Seed complete: 10 components, 2 corpus chunks.
Embedding ingest complete: 2 newly embedded, 0 still missing an embedding.
```

## Real HTTP evidence (host on http://localhost:5102, over TCP)

### 1. First call — verdict, real citations, sealed
`POST /api/v1/root-cause/incidents/EVT-2026-0418/diagnoses` → **HTTP 200 in 93.6 s**
(cold: model load + full pipeline). Body:
```json
{ "diagnosisRunId": 1, "incidentId": "EVT-2026-0418", "verdict": "FV-104",
  "abstained": false, "abstainReason": null,
  "candidates": [ {"tag":"FV-104","role":"origin","weight":0.66,"coverage":0.7857…},
                  {"tag":"PB-2A","role":"proximate",…}, {"tag":"RCP-1B","role":"parallel",…},
                  {"tag":"BUS-2A","role":"contributing",…}, {"tag":"FT-7","role":"ruled-out","coverage":0} ],
  "citations": [ {"chunkId":1,"sourceLabel":"From Flood to Cause -- NEXUS-1 Companion, worked example, Ch.1-2"},
                 {"chunkId":2,"sourceLabel":"From Flood to Cause -- NEXUS-1 Companion, worked example, Ch.2 (delay windows)"} ],
  "auditHash": "3f0986425052060bcd0b8b3f45094295cc7ec7802e5b65fea399a6261b9c2a81" }
```

### 2. It really ran the pipeline (not a canned result)
`SELECT TOP 1 … FROM RootCause.Audit ORDER BY Seq DESC` after the call:
```
seq=1  hash=3f0986425052060b…  prev=0000…0000   (genesis)
payload={"IncidentId":"EVT-2026-0418","Verdict":"FV-104",…"Candidates":[{"Tag":"FV-104","Role":"origin",…}]…}
```
The audit `Hash` equals the response `auditHash`; the payload holds the real sealed content.

### 3. Second call — new run, audit chained on the previous hash
Second POST → **HTTP 200 in 21.2 s** (warm), verdict FV-104. Audit chain:
```
seq=1  hash=3f0986425052060b…  prev=0000…0000
seq=2  hash=df122b7c017c99bd…  prev=3f0986425052060b…
chain integrity: seq2.PrevHash == seq1.Hash  -> CHAINED OK
```
Two DiagnosisRun rows, both FV-104. seq2's hash differs from seq1's — the model
narration is non-deterministic (ADR-033's honest boundary) — but the verdict is
identical and the chain links.

### 4. H7 split proven LIVE (not just declared)
`sys.dm_exec_sessions` sampled during a live call — distinct RootCauseDb login_names:
```
nexus1_app        (engine reads + writes: walk, corroborate, store, audit)
nexus1_explain    (read-only retrieval + H3/H4 validator)
```
Both present → retrieval genuinely runs under the read-only login while writes run
under the read-write login. The model (`SemanticKernelExplainer`) has no DB access
at all. (The read-only login itself was proven read-only in ADR-033: SELECT ok,
INSERT/UPDATE/DELETE denied.)

### 5. Safe outcomes, never a fabricated verdict
- **Missing historian channel** (deleted RootCause.Historian ch=6, then restored):
  `POST` → **HTTP 200 in 0.03 s**, `verdict=null, abstained=true, abstainReason="telemetry corroboration failed: missing historian data for component 6"`. Abstains before the model; no verdict invented.
- **Model unreachable** (second host pointed at a dead Ollama endpoint 127.0.0.1:59999):
  `POST` → **HTTP 503 in 2.5 s**, `"Diagnosis pipeline could not run" … "No connection could be made … (127.0.0.1:59999)"`. No DiagnosisRun written (failed before the store). A genuine infrastructure failure is a 503, never a fabricated verdict.

### 6. Latency (honest, CPU inference)
Cold first call **93.6 s**, warm subsequent call **21.2 s**, abstention (pre-model) **0.03 s**, 503 (fail-fast) **2.5 s**.

## Operational notes

- RootCause.Host also runs its messaging responsibility, so it requires RabbitMQ at
  startup (started from the documented portable binaries for this run). This is a
  pre-existing host dependency, unrelated to the diagnosis route.
- The host binds `http://localhost:5102` from its launch profile.
- Left running after the evidence run: the portable RabbitMQ and the Ollama tray
  server (both dev dependencies). The two host instances were stopped.

## Scope boundary (unchanged from ADR-034)

No listing/search/history, no UI/Angular wiring, no second incident (others → 404),
no general query surface, no new messaging, no auth beyond dev status quo. Just: the
walking skeleton now has a real door — one POST that runs it and returns the sealed
result.
