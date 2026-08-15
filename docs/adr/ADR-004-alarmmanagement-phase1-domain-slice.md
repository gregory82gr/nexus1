# ADR-004: AlarmManagement Phase 1 domain slice, flood-detection policy, and a correction to ADR-001-amend

## Status

Accepted.

## Context

### Schema Atlas vs. Domain_to_Twin: no boundary conflict this time

Reading both sources directly for AlarmManagement (unlike ReactorFleet, ADR-003)
found no real disagreement. The Schema Atlas (Appendix C.7, 45 tables: 18
lookup + 27 substantive, grouped LOOKUP/DEFINITION/RUNTIME/CONTROL/FLOOD/
GOVERNANCE) and `From_Domain_to_Twin` agree on the two aggregates that
matter: `AlarmEvent` (explicit aggregate root, Ch. 16/26, with a real
invariant — `Acknowledge` throws `InvalidOperationException` unless the
alarm is in its raised state) and `AlarmFlood` (separate aggregate root,
Ch. 16, stated rule: *"a flood cannot contain alarms from unrelated time
windows"*). `AlarmFloodMember` references `AlarmEventId` by ID rather than
owning the event — exactly the book's "groups without owning" pattern.

Two things the book's own worked example gets wrong for our purposes,
worth naming so nobody copies them literally:
- The book's illustrative `AlarmEvent` C#/SQL (Ch. 9/15, pp. 59, 71) has
  only 6 columns and a `SignalTag` string; the atlas's real `AlarmEvent`
  has 18 columns including an `EquipmentId` FK and a `SignalId` FK to
  `Instrumentation.Signal` (out of Phase-1 scope). The book states outright
  (p. 50) that its example "is not trying to be complete production code."
  The Schema Atlas's column list governs, not the book's snippet.
- The book's example enum uses `AlarmState.Raised`; the atlas's persisted
  lookup codes are `ACTIVE, ACKNOWLEDGED, RETURNED, CLEARED, SHELVED,
  SUPPRESSED`. Since the atlas is authoritative for anything persisted
  (CLAUDE.md §1, priority 3), the enum uses `Active`, not `Raised`.

### The flood-detection algorithm is undefined in both sources

`AlarmFlood.DetectionRule` in the atlas is a free-text `NVARCHAR(200)`
audit label, not a computable rule. The book states the grouping invariant
("no unrelated time windows") but never gives a count-within-window formula
or a numeric threshold — `AlarmFloodDetected` never even gets a full record
definition in the book's event catalogue, only a handler-parameter mention.
Per CLAUDE.md §1 ("If you are ever about to implement something and can't
find it in the source material, stop and ask rather than inventing
architecture"), this is a decision the source material leaves genuinely
open, not one I'm resolving between disagreeing sources.

### A correction to ADR-001-amend

Designing `AlarmEvent`'s creation path surfaced a real problem with
ADR-001-amend's own reasoning. That ADR said no `Nexus1.Contracts.ReactorFleet`
project is needed because AlarmManagement consumes ReactorFleet telemetry
"in-process." But ADR-002's dependency law (`Nexus1.ArchitectureTests`) says
*"Cross-context code references only producer-owned Contracts"* — full
stop, with no carve-out for same-host/in-process composition. ADR-001-amend
conflated "in-process" (a deployment/transport fact) with "doesn't need
Contracts" (a code-architecture fact); those are independent axes. Whether
AlarmManagement.Domain or .Application takes a compile-time dependency on
`Nexus1.ReactorFleet.Domain` is forbidden by the dependency law regardless
of how the data physically moves at runtime.

## Decision

### AlarmManagement Phase 1 domain slice

Model exactly what `AlarmEvent`/`AlarmFlood` need to exist and be tested,
following the same recursive restraint principle ADR-003 applied to
ReactorFleet:

- **`AlarmDefinition`** (aggregate root) — a *minimal* single-threshold
  slice: `AlarmDefinitionId`, `UnitId` (passport), `Code`, `Name`,
  `AlarmSeverity`, `ThresholdValue` (decimal). The atlas's full
  `AlarmCondition`/`AlarmLimit`/`AlarmDeadband` tree (multiple condition
  types, hysteresis, on/off delay) is **not modeled** — nothing in Phase 1
  needs more than "does this value cross this one limit." `Evaluate(decimal
  sourceValue, AlarmEventId newEventId, DateTime atUtc)` returns a raised
  `AlarmEvent` or `null`.
- **`AlarmEvent`** (aggregate root) — matches the atlas's real column set
  (not the book's simplified snippet): `AlarmDefinitionId`, `UnitId`,
  `AlarmSeverity`, `AlarmState` (starts `Active`), `RaisedAtUtc`,
  `SourceValue`, `ThresholdValue`, `Message`. `Acknowledge(UserId, DateTime)`
  enforces the book's exact invariant.
- **`AlarmFlood`** (aggregate root) — `AlarmFloodId`, `UnitId`,
  `AlarmFloodStatus` (starts `Detected`), `StartedAtUtc`,
  `EndedAtUtc?`, and a member list of `AlarmEventId` (by reference, per the
  book's grouping-not-owning pattern). `AddMember(AlarmEventId, DateTime
  raisedAtUtc, TimeSpan window)` enforces the one stated invariant: throws
  if `raisedAtUtc` falls outside `[StartedAtUtc, StartedAtUtc + window]`.
  `window` is caller-supplied, not hardcoded — see below.

Deferred entirely (no source-material guidance needed yet, no Phase-1
consumer): `AlarmCondition`, `AlarmLimit`, `AlarmDeadband`,
`AlarmEventStateHistory`, `AlarmAcknowledgement` (as its own table —
folded into `AlarmEvent.Acknowledge` for now), `AlarmComment`,
`AlarmOperatorAction`, `AlarmEventEvidence`, `AlarmNotification`,
`AlarmEscalation`/`AlarmEscalationAction`, `AlarmSuppression`,
`AlarmShelving`, `AlarmStandingOrder`, `AlarmFloodSummary`,
`AlarmFloodRanking`, `AlarmRationalization`/`AlarmRationalizationReview`,
`AlarmKpiSnapshot`.

### Flood-detection policy: parameterized, not defaulted

`AlarmFloodDetector` is a stateless domain service (pure function over
already-materialized `AlarmEvent` timestamps — no persistence, no
subscription plumbing, so it belongs in Domain, not Application):

```csharp
public static bool ShouldDetectFlood(
    IReadOnlyList<DateTime> recentAlarmRaisedAtUtc, DateTime nowUtc,
    int countThreshold, TimeSpan window)
```

**No default `countThreshold`/`window` is chosen in this ADR.** Neither
source gives a number, and picking one myself (e.g. "5 alarms in 60
seconds") would be inventing a business rule with no source backing —
exactly the "vibe architecture" this project prohibits. The threshold
becomes a real decision (with its own justification, or a value the user
supplies) when something calls this policy for real — at the earliest,
step 5 (Application layer), realistically when `Nexus1.ModularRuntime`
actually wires ReactorFleet's telemetry through it.

### Correcting ADR-001-amend: Domain purity, not a Contracts decision yet

`AlarmDefinition.Evaluate` takes a plain `decimal sourceValue` — it does
**not** reference `Nexus1.ReactorFleet.Domain.PowerPercent` or any other
ReactorFleet type. This isn't a workaround; it's the correct DDD shape
regardless of the Contracts question — `AlarmDefinition` should evaluate
against whatever value type its own condition-modeling eventually needs,
which the atlas already models as a schema-agnostic
`DECIMAL(18,6)`/`EngineeringUnitId` pair, not specifically ReactorFleet's
power percentage.

This defers, rather than resolves, ADR-001-amend's error: translating a
`ReactorFleet.UnitPowerRecorded` domain event into a call to
`AlarmDefinition.Evaluate(decimal, ...)` is Application/Host-layer wiring
(steps 5–6), not Domain. **Whether that wiring needs
`Nexus1.Contracts.ReactorFleet` (per the dependency law, most likely: yes,
even for same-host in-process composition) is an open question for that
step, not decided here.** ADR-001-amend's specific claim — "no Contracts
project needed because consumption is in-process" — is flagged as
**likely wrong** and should be revisited explicitly when Application/Host
wiring is built, not silently carried forward.

### Other inferences made without book/atlas backing

- `UserId` — `AlarmEvent.Acknowledge` needs an actor identity, and the
  atlas's `AcknowledgedByUserId` FKs to `Security.ApplicationUser`, which
  is out of Phase-1 scope entirely (not one of the three Phase-1
  contexts). Modeled as a bare `UserId(Guid Value)` passport with no
  assumption about Security's actual primary-key type, since that schema
  hasn't been read.
- `AlarmFloodDetected`'s payload shape (`AlarmFloodId`, `UnitId`,
  `StartedAtUtc`) is invented — the book names the event but never defines
  its fields anywhere, even in the handler-parameter usage that references
  it.

## Consequences

- `Nexus1.AlarmManagement.Domain` has no compile-time dependency on
  `Nexus1.ReactorFleet.Domain` — verified by `Nexus1.ArchitectureTests`
  already in place (no code change needed to the test suite itself, since
  the existing rules already forbid this; this ADR is what makes sure the
  domain code doesn't try).
- The flood-detection threshold/window remains unresolved technical debt,
  visible in code (a required parameter with no default) rather than
  hidden behind a plausible-looking default.
- Step 5/6 inherits an explicit open question about `Contracts.ReactorFleet`
  instead of discovering the dependency-law conflict as a build break.

## Rejected alternatives

- **Model the full `AlarmCondition`/`AlarmLimit`/`AlarmDeadband` tree now.**
  Rejected: no Phase-1 consumer needs more than one threshold per
  definition; same restraint principle as ADR-003.
- **Pick a default flood threshold (e.g. 5-in-60s) so the domain service
  has a usable default.** Rejected: neither source states one: a default
  would be presented as if source-derived when it is not.
- **Have `AlarmManagement.Domain` reference `ReactorFleet.Domain.PowerPercent`
  directly**, since Phase 1's only telemetry source is power readings.
  Rejected: violates Domain purity/the dependency law regardless of
  same-host deployment, and hard-codes AlarmManagement's generic
  threshold-evaluation concept to one telemetry shape it may not always
  have (the atlas's `AlarmCondition`/`AlarmLimit` design is explicitly
  signal/value-type-agnostic).

## Reversal condition

Revisit when Application/Host wiring (steps 5–6) needs to actually
translate `ReactorFleet.UnitPowerRecorded` into an `AlarmDefinition.Evaluate`
call — that step must explicitly decide the `Contracts.ReactorFleet`
question flagged above, and should amend or supersede ADR-001-amend's
"no Contracts needed" claim rather than silently working around it.

## Evidence required

- `Nexus1.AlarmManagement.UnitTests` passing: `AlarmDefinition.Evaluate`
  above/below/at threshold; `AlarmEvent.Acknowledge` success and
  double-acknowledge failure; `AlarmFlood.AddMember` accepting an in-window
  event and rejecting an out-of-window one; `AlarmFloodDetector
  .ShouldDetectFlood` true/false at the threshold boundary.
- `Nexus1.ArchitectureTests` still passing, confirming no accidental
  `AlarmManagement.Domain` → `ReactorFleet.Domain` reference was
  introduced.
