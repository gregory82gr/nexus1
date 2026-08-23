# Evidence: BFF thirteenth vertical slice — Audit (Audit half of the Audit & Compliance screen)

## Scope

Extended `Nexus1.Bff` with a thirteenth vertical slice:

- `GET /api/v1/audit/analyses/{analysisId}/evidence` — the audit evidence
  record for a given RootCause analysis.

Built the entire Application layer from scratch (Audit had none before this
slice) and added an `enableMessagingConsumer` opt-out to
`AddAuditInfrastructure`, following the exact pattern from Reporting/
AlarmManagement.

## 1. What Audit's Application layer already exposed

**Nothing — no `Nexus1.Audit.Application` project existed at all**, unlike
every other Phase 1/2 context checked so far except Reporting (which was in
the same state before its own slice). Audit's entire prior existence was
write-side only: `AuditVerdictMessageHandler` consumes
`root-cause.root-cause-verdict-issued.v1` events and appends
`AuditEvidenceRecord` rows. No query, no finder, no DTO existed anywhere.
Built all of it in this slice: `IAuditEvidenceFinder`,
`GetAuditEvidenceBySourceAnalysisIdQuery`/Handler, `AuditEvidenceRecordDto`,
`EfAuditEvidenceFinder`, and the project's own `ServiceCollectionExtensions`
(`AddAuditApplication`) — same shape as Reporting's own from-scratch build.

No dedicated component test was added for the new handler, matching
Reporting's own precedent at the time (`GetCaseSummariesForUnitQueryHandler`
also shipped with no dedicated test) — this project verifies BFF-wiring
slices via live evidence (real host, real database, real JSON), not new
xUnit coverage per endpoint.

## 2. Domain model — what's actually there, and a real scope-narrowing gap

`Nexus1.Audit.Domain` has exactly **one** entity: `AuditEvidenceRecord` —
an append-only evidence envelope (`SourceMessageId`, `SourceAnalysisId`,
`EventType`, `SchemaVersion`, raw `EnvelopeBytes`/`EnvelopeSha256`,
`CorrelationId`/`CausationId`, `OccurredAtUtc`/`RecordedAtUtc`). No public
mutators exist at all — the shape itself enforces "Audit never mutates
history," backed by a second line of defense at the EF layer
(`AuditAppendOnlyInterceptor`).

**Named gap:** there is no `UnitId` anywhere, and no general "who changed
what record" system audit-trail concept — the book-screen idea of "Audit"
(a broad activity/change log) doesn't exist here. What's real is a single,
narrow ledger tied to `SourceAnalysisId` — a RootCause analysis identity.
Since RootCause stays out-of-process (ADR-001), the BFF cannot resolve that
id into a human-readable case name; it's surfaced as an opaque `long`.

**A second, more specific narrowing, found by reading the EF configuration
directly:** `AuditEvidenceRecordConfiguration.cs` puts a **unique index** on
`SourceAnalysisId`
(`UX_Audit_AuditEvidenceRecord_SourceAnalysisId`) — at most one evidence row
can ever exist per analysis (the two-key dedup oracle, ch.34 34-AI, treats
a second verdict-issued event for the same analysis as a replay, not a new
audit entry). So this isn't really an "evidence trail" in the sense of
multiple events per analysis over time — it's **one row per analysis**.
The endpoint's `IReadOnlyList<...>` return shape is honest about what the
finder contract could support in principle, but in practice today it will
always return 0 or 1 rows, never more. Confirmed live below.

## 3. Hosted-service check — two unconditional hosted services found, same rigor as Reporting

Read every relevant file directly rather than assuming the "Phase 2, zero
hosted services" precedent (`ReactorFleet`/`CorePlatform`/etc.) carried over
— Audit is Phase 1, same class of context as Reporting/AlarmManagement,
which both had real unconditional hosted services.

Found **two**:

- `AuditConsumerBackgroundService` — constructor needs
  `RabbitMqConnectionManager`, `RabbitMqOptions`, `AuditVerdictMessageHandler`,
  `NexusRuntimeMetrics`.
- `RetryDispatcherBackgroundService` — depends on `RetryDispatcher`, whose
  own constructor needs `IBrokerPublisher`.

None of `RabbitMqConnectionManager`/`RabbitMqOptions`/`NexusRuntimeMetrics`/
`IBrokerPublisher` are registered by the BFF (no `AddNexusMessaging`, no
`AddNexusObservability`) — the same DI-crash shape already found and fixed
for AlarmManagement and Reporting. Applied the identical opt-out parameter:

```csharp
public static IServiceCollection AddAuditInfrastructure(
    this IServiceCollection services, string connectionString, bool enableMessagingConsumer = true)
```

`enableMessagingConsumer: false` in the BFF's composition; default `true`
unchanged for `Nexus1.ModularRuntime`.

## 4. Build and full regression suite

```
dotnet build src/Hosts/Nexus1.Bff/Nexus1.Bff.csproj → 0 Warning(s), 0 Error(s)
dotnet build Nexus1.Runtime.sln                     → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln                       → 37/37 assemblies green, 869/869 total, 0 failed
```

Unchanged from the CorePlatform slice's baseline — no regressions.

## 5. Memory discipline

| Check | Reading | Notes |
|---|---|---|
| During test-suite run, 1st | 1.27 GB | below threshold |
| During test-suite run, 2nd (+5s) | 1.07 GB | declining — **held off starting the host** |
| After test suite finished, 1st | 1.86 GB | recovered |
| After test suite finished, 2nd (+5s) | 1.88 GB | stable |

Started the host only once memory was confirmed stable above the ~1.7 GB
threshold.

## 6. Real host, real database — live evidence (subset composition: ReactorFleet + Audit)

`Audit.AuditEvidenceRecord` had **23 rows already present** — real,
pre-existing dev-run residue from earlier RootCause verdict processing
(same "check for existing data first" discipline as every prior slice).
No seeding needed.

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/audit/analyses/639223958897100060/evidence` (real, existing analysis)

```json
[{"auditEvidenceId":"78bb40a7-2946-4515-b034-ac52cd50f756","sourceAnalysisId":639223958897100060,"eventType":"nexus1.root-cause.root-cause-verdict-issued.v1","schemaVersion":1,"correlationId":"86a5df3a-55d1-44ef-b0e0-cabf9d4d3034","causationId":null,"envelopeSha256Hex":"B4A1F70DDBF9AB74A85B06ACC1E9429C5ABAE8C2DFA19CEF1AF11F160DC32039","occurredAtUtc":"2026-08-15T13:04:51.2951404","recordedAtUtc":"2026-08-15T13:04:51.7646253"}]
```

HTTP 200. Exactly one row returned, confirming the unique-index finding
from section 2 live, not just from reading the configuration.

### `GET /api/v1/audit/analyses/999999999999999999/evidence` (nonexistent analysis)

```json
[]
```

HTTP 200 — empty array, not an error.

### Login verification

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

```
login_name  program_name                            status
nexus1_app  Core Microsoft SqlClient Data Provider   sleeping
nexus1_app  Core Microsoft SqlClient Data Provider   sleeping
```

**Two sessions** — matching exactly the two composed contexts
(`ReactorFleet`, `Audit`), both under `nexus1_app`.

`sys.databases` confirmed all `ONLINE` after host stop; no corruption.

## Summary

Thirteen vertical slices now exist in `Nexus1.Bff`. Audit's contribution is
a from-scratch Application layer (like Reporting before it) plus an honest
narrowing of what "Audit" means here: not a general activity/change log, but
a single append-only evidence record per RootCause analysis, enforced by a
real unique index — confirmed by reading the EF configuration and then
verified live against real pre-existing data. Two unconditional hosted
services were found by direct inspection (not assumed from any Phase 2
precedent) and given the same `enableMessagingConsumer` opt-out already
proven for Reporting/AlarmManagement.

Compliance is next.
