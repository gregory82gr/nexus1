# Evidence: BFF fourteenth vertical slice — Compliance (Compliance half of the Audit & Compliance screen)

## Scope

Extended `Nexus1.Bff` with a fourteenth vertical slice:

- `GET /api/v1/compliance/analyses/{analysisId}/reviews` — the compliance
  review(s) opened for a given RootCause analysis.

Built the Application layer from scratch (Compliance had none before this
slice) and added an `enableMessagingConsumer` opt-out to
`AddComplianceInfrastructure`, same pattern as Audit/Reporting/
AlarmManagement.

## 1. What Compliance's Application layer already exposed

**Nothing — no `Nexus1.Compliance.Application` project existed**, same
situation as Audit and Reporting before their own slices. Compliance's
entire prior existence was write-side only: `ComplianceVerdictMessageHandler`
consumes the same `root-cause.root-cause-verdict-issued.v1` events Audit
does (an independent binding from the same exchange/routing key, not a
shared queue) and opens `ComplianceReview` rows. Built the read side from
scratch: `IComplianceReviewFinder`, `GetComplianceReviewsBySourceAnalysisIdQuery`/
Handler, `ComplianceReviewDto`, `EfComplianceReviewFinder`,
`AddComplianceApplication` — mirroring Audit's own from-scratch shape.

No dedicated component test was added for the new handler, consistent with
the Audit and Reporting precedent — verified via live evidence instead.

## 2. Domain model — what's actually there, and the central named gap

`Nexus1.Compliance.Domain` has exactly one entity: `ComplianceReview`
(`SourceMessageId`, `SourceAnalysisId`, `Verdict` — a plain string, not a
cryptographic identity hash — `State`, `OpenedAtUtc`). Unlike
`AuditEvidenceRecord`, it's **deliberately mutable** (`State` has a private
setter; ADR-011) and carries no envelope bytes at all — Audit already owns
the evidentiary copy (contract minimization, ch.34 34-AL).

**Central named gap, more significant than a scope narrowing:**
`ComplianceReviewState` (the enum backing `State`) has **exactly one
member: `Pending`.** Nothing in `ComplianceReview.cs` ever transitions it —
there is no method that assigns any other value, only a private setter with
no caller. The doc comments on both files are explicit that
review-assignment, findings, and a decision are the book's own **named
future authority** (ch.34 34-AL) — not implemented yet, not merely absent
from a DTO. This means a "Compliance status/findings" screen, in the sense
the screen name implies (open/closed findings, pass/fail, a decision), is
**not something this codebase can honestly show today.** Every real row
this slice's endpoint returns will read `"state": "Pending"`, and no code
path exists that could ever produce anything else. This is a stronger gap
than CorePlatform's naming mismatch or Maintenance's Decommissioning
absence — the *concept* of a compliance decision is named and reserved in
the domain's own documentation, but genuinely does not exist in code yet.

Same per-analysis scoping as Audit, for the identical reason: no `UnitId`
anywhere in `ComplianceReview`, and a **unique index** on
`SourceAnalysisId` (`UX_Compliance_ComplianceReview_SourceAnalysisId`) —
at most one review per analysis, confirmed by reading the EF configuration
directly (same "one row per analysis" shape as Audit, not assumed to carry
over — checked independently).

## 3. Hosted-service check — two unconditional hosted services, confirmed directly

Read every relevant file rather than assuming Audit's finding carried over
unchanged for a different context:

- `ComplianceConsumerBackgroundService` — needs `RabbitMqConnectionManager`,
  `RabbitMqOptions`, `ComplianceVerdictMessageHandler`, `NexusRuntimeMetrics`.
- `RetryDispatcherBackgroundService` — depends on `RetryDispatcher`, whose
  constructor needs `IBrokerPublisher`.

Same unregistered-dependency shape as Audit/Reporting/AlarmManagement.
Applied the identical opt-out:

```csharp
public static IServiceCollection AddComplianceInfrastructure(
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

Unchanged from the Audit slice's baseline — no regressions.

## 5. Memory discipline

| Check | Reading | Notes |
|---|---|---|
| Before host start, 1st | 1.93 GB | |
| Before host start, 2nd (+5s) | 1.92 GB | stable |

Comfortably above threshold both readings; started the host without delay
this time.

## 6. Real host, real database — live evidence (subset composition: ReactorFleet + Compliance)

`Compliance.ComplianceReview` had **22 rows already present** — real,
pre-existing dev-run residue from the same earlier RootCause verdict
processing that produced Audit's 23 rows (one analysis apparently produced
an audit record without a matching compliance review, or vice versa — not
investigated further, out of scope for this slice). No seeding needed.
Sampled rows before testing to confirm `State` values are correctly
spelled (`Pending`, exact PascalCase) — written by the real message
handler, not a hand-typed guess, so the CorePlatform-style enum/seed
mismatch risk didn't apply here.

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/compliance/analyses/639223980837418031/reviews` (real, existing analysis)

```json
[{"complianceReviewId":"9b15d636-bb01-4784-81ab-89681f6e3cc0","sourceAnalysisId":639223980837418031,"verdict":"SENSOR_CALIBRATION_DRIFT","state":"Pending","openedAtUtc":"2026-08-15T13:41:26.5654891"}]
```

HTTP 200. Exactly one row, confirming the unique-index finding live;
`state` reads `"Pending"` exactly as the domain's own scope currently
allows.

### `GET /api/v1/compliance/analyses/999999999999999999/reviews` (nonexistent analysis)

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
(`ReactorFleet`, `Compliance`), both under `nexus1_app`.

`sys.databases` confirmed all `ONLINE` after host stop; no corruption.

## Summary

Fourteen vertical slices now exist in `Nexus1.Bff`. Compliance's own
contribution mirrors Audit's shape closely (from-scratch Application layer,
same unconditional-hosted-service pair, same per-analysis scoping, same
unique-index-backed one-row-per-analysis reality) but surfaces a sharper
named gap: the "Compliance" half of the screen name promises status and
findings that genuinely do not exist in code yet — `ComplianceReviewState`
has exactly one reachable value, `Pending`, forever, until a future
milestone actually implements review assignment, findings, and a decision.
Reported honestly rather than fabricated or hinted at with placeholder
fields, consistent with the discipline applied across all fourteen slices.

Audit and Compliance together close out the Audit & Compliance screen.
