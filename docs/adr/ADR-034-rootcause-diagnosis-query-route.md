# ADR-034: A real query route for the EVT-2026-0418 diagnosis pipeline

## Status

Accepted.

## Context

The fixed-incident walking skeleton is complete and committed (ADR-032 engine,
ADR-033 air-gapped Explain stage), but **nothing calls it** — `Nexus1.RootCause.Host`
has only `/health`. Building more depth (a second incident, a query atlas) before
there is a real consumer is the exact risk this project argues against (ADR-005).
This ADR gives the existing pipeline a real door: one HTTP route that runs the
seeded incident and returns its sealed result, over the network boundary ADR-001
actually built.

## Decision

### Route — synchronous POST

**`POST /api/v1/root-cause/incidents/{incidentId}/diagnoses`** on
`Nexus1.RootCause.Host`. Only `EVT-2026-0418` is valid (any other id → `404`).
Running the pipeline writes (a new `DiagnosisRun` + `Candidate` rows + a new audit
link every call), so it is POST — neither safe nor idempotent.

- **`200`** with a `DiagnosisResponse` DTO: `diagnosisRunId, incidentId, verdict
  (nullable), abstained, abstainReason (nullable), candidates[{tag,role,weight,
  coverage}], citations[{chunkId,sourceLabel}], auditHash`.
- **An abstention is a `200`**, first-class (H8) — the body carries `abstained:true`
  and the reason. It is never a 4xx/5xx.
- **`404`** for an unknown incident id (a validation refusal, `Result.Failure`).
- **`503`** reserved for genuine infrastructure failure — the served model
  unreachable, or the database down. These surface as exceptions (not
  abstentions); the route maps them to 503, never to a fabricated verdict.

**Why synchronous, not async/poll:** it is the smallest honest thing — one route,
no job store, no status model, no polling. The caller is curl or (later) the
console triggering one fixed incident. Sync returns the full live result including
citations, which are not otherwise persisted per-run (they live only in the audit
payload). The measured cost is a 26–70 s request on CPU inference; that latency is
documented and accepted for a demonstrator rather than hidden behind machinery
with no present consumer. If the caller disconnects mid-run the run still completes
and seals server-side — no lost work. Async-with-poll (the persisted `DiagnosisRun`
already supports a GET-by-id, with citations reconstructed from the audit payload)
is the clean evolution if the console later needs non-blocking UX — deferred, no
consumer needs it yet (same restraint as ADR-007/ADR-027).

### Composition — split connections, H7 preserved live

The route must not collapse retrieval into the engine's read-write path. Each part
uses exactly the connection it should:

| Part | Connection |
|---|---|
| EfGraphWalker, EfTelemetryCorroborator, EfDiagnosisRunStore, Sha256AuditChainWriter | `nexus1_app` (read-write) — the engine reads topology/historian and **writes** the run + audit |
| EfRetriever, RegistryAntiHallucinationValidator | **`nexus1_explain` (read-only)** — the grounding/retrieval path (H7) |
| SemanticKernelExplainer | **no database at all** — the strongest H7 (structural) |

Wired with a keyed scoped `RootCauseDbContext` ("readonly", built from the
`RootCauseExplainDb` connection string) that the retriever and validator resolve,
while every write seam keeps the default read-write context. `AddRootCauseReadOnlyRetrieval`
(Infrastructure) `Replace`s the two seam registrations after `AddRootCauseInfrastructure`
and `AddRootCauseExplain`. The keyed context is IDisposable and disposed per
request scope by the container. The split is proven **live** (not just declared)
by `sys.dm_exec_sessions` showing the retrieval ran under `login_name =
nexus1_explain` while writes ran under `nexus1_app` (evidence). A fail-loud
property falls out: if a write seam were ever handed the read-only context, the
write would throw permission-denied rather than silently succeed.

### Where it lives

Directly on `Nexus1.RootCause.Host` — the separate out-of-process service ADR-001
established, which already owns RootCauseDb. The BFF (ADR-030) deliberately keeps
RootCause out of the in-process composition; it would reach this route over HTTP
later. `Program.cs` stays a pure composition root: the route delegates to a thin
Application-layer command handler (`RunFixedIncidentDiagnosisCommandHandler`,
`Result<T>`, hand-rolled dispatch per ADR-002-amend, no MediatR), exactly like the
BFF's handlers. The host now references the `Nexus1.RootCause.Explain` project to
call `AddRootCauseExplain` — this does not add a served-model *PackageReference* to
the host, so the ArchitectureTests served-model-package rule (ADR-033) still holds.

The fixed incident's **identity** (id, unit, flood-start, alarmed component ids)
moves to `FixedIncident` in the Application layer — the handler needs it and cannot
reference `GroundingSeed` (Infrastructure) under the dependency law. `GroundingSeed`
delegates to `FixedIncident` so there is a single source of truth.

### Provisioning is a prerequisite, not part of the request path

The route assumes real data already exists. Seeding RootCauseDb (`GroundingSeed`)
and running the embedding ingest once is an explicit, documented provisioning step
(`docs/runbooks/local-rootcause-diagnosis-provisioning.md`), dev-only — **not** a
seed-on-startup baked into the host. The route stays pure and honest about what it
assumes.

## Consequences

- The walking skeleton has a real, network-reachable door; the pipeline is proven
  end to end over HTTP, not only in in-process tests.
- Latency is a real 26–70 s per call (CPU inference), documented.
- H7 is preserved and proven live at the route level.

## Scope boundary — what this explicitly does NOT do

No listing/search/history/filtering (ADR-005's atlas stays deferred); no UI/Angular
wiring (this is the door, not the consumer); no second incident (others → 404); no
general RootCause query surface or new CQRS queries beyond the single run-and-return
command; no new messaging; no auth beyond the dev status quo (noted for later).

## Rejected alternatives

- **Async/poll** — rejected for this step (job/status machinery with no consumer).
- **A new composition root/host** — rejected; RootCause.Host is the right home.
- **Seed-on-startup** — rejected; provisioning stays an explicit step so the route
  assumes, never fabricates, its data.
- **One shared read-write context for the whole route** — rejected; it would
  undermine H7's read-only retrieval boundary.

## Reversal condition

Revisit if the console needs non-blocking UX (add async/poll — the persisted run
already supports GET-by-id), if a second incident lands (the id check and
`FixedIncident` generalise), or if the route needs auth/throughput beyond a
demonstrator.

## Evidence required

- Solution build clean; `ArchitectureTests` 8/8 with the host referencing Explain.
- A real `curl` POST against the running host → `verdict=FV-104`, correct
  candidates, book citations, an `auditHash`.
- RootCauseDb `RootCause.Audit`: latest `Hash` equals the response `auditHash` and
  the payload holds the real sealed content; a second call adds a new `DiagnosisRun`
  and a new audit entry chained on the previous hash.
- `sys.dm_exec_sessions` during a live call: retrieval under `nexus1_explain`,
  writes under `nexus1_app`.
- Abstention path over HTTP (missing historian channel → `200` safe abstention;
  Ollama stopped → `503`), never a fabricated verdict.
- Recorded end-to-end HTTP latency.

See `artifacts/evidence/2026-09-22-rootcause-diagnosis-query-route.md`.
