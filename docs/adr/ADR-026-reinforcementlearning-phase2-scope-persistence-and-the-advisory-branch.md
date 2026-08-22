# ADR-026: ReinforcementLearning (Phase 2, sector 11, final) — scope, domain shape, persistence, and the optional advisory messaging branch

## Status

Proposed — scope decision only. Implementation does not begin until this
ADR is confirmed.

## Context

Phase 2's eleventh and final sector, atlas Appendix **C.11** (confirmed
via the real `"C.11.1 Sector purpose"` header, not the garbled TOC — the
real atlas sequence, now confirmed end-to-end across all eleven sectors:
C.1 CorePlatform, C.2 Security, C.3 Organization, C.4 ReactorFleet, C.5
Instrumentation, C.6 DigitalTwin, C.7 AlarmManagement, C.8
EventManagement, C.9 Maintenance, C.10 RootCause, **C.11
ReinforcementLearning**, C.12 Robotics, C.13 RadiationMonitoring, C.14
EmergencyPreparedness). This sector's own numbering carries the same
extraction quirk flagged before: no separate lettered ER-diagram heading
distinct from the FK-mapping section in the printed section list, and
C.11.6 (ER diagram) and C.11.7 (FK mapping) both had to be located by
reading forward from C.11.5 rather than trusted from the header block —
same discipline as every prior sector's numbering-verification step.

Three source documents were read in full for this sector, per the
architect's explicit instruction, because this sector is structurally
unlike the other ten:

1. **`From_Schema_to_System` Appendix C.11** — the persistence contract,
   same authority as every prior sector.
2. **`From_Trial_to_Policy`** (the dedicated RL companion volume,
   extracted from the uploaded PDF to `/tmp/From_Trial_to_Policy.txt`) —
   the domain narrative: environment, state/action discretization,
   Q-learning, exploration, the readable policy grid, and **Chapter 10,
   "From Policy to Advice,"** which is this book's own design for how
   advice reaches an operator.
3. **`From_Services_To_Runtime` Chapter 36, "The Complete Slice and the
   Optional RL Advisory Branch"** — the messaging/services book's
   treatment of RL Advisory as a case study in how an optional,
   non-blocking, non-command observer should be architected.

### What each source actually says — and the discrepancy between them that drives this decision

**`From_Trial_to_Policy`'s own advisory design uses no messaging at
all.** Chapter 10 is explicit: the advisory layer reads *live* state
directly from the DigitalTwin point-kinetics model in-process
(`double power = liveModel.Power;`), clamps the policy's pick to the
validated band, and hands the result to a human — a synchronous,
in-process consultation with four steps and two gates (clamp,
protection/SCRAM), never a broker. Chapter 12's own honest-boundary
ledger confirms this is deliberately small and auditable: *"175 numbers
you can print, inspect, and challenge... tiny and fast... reproducible...
safe to wrap."* Nothing in the RL book itself asks for a message
consumer.

**`From_Services_To_Runtime` Ch.36 places RL Advisory in a different
role: an illustrative optional subscriber to `RootCauseVerdictIssued.v1`,
used to demonstrate how *any* optional, non-blocking observer should be
built** — strict envelope admission (producer/event/schema checks, zero
private cross-database reads), an immutable `AdvisoryPolicyIdentity`,
`Recommend`/`Abstain` as the only two outcomes, a local-only commit (own
inbox receipt, own input lineage, own decision row — **no outbox, no
public event**), two-key idempotency (transport vs. semantic duplicate),
and — the enforcement machinery the architect specifically asked about —
architecture-test gates that the advisory assembly cannot reference
`Nexus1.Actuation`/`Nexus1.AlarmManagement.Application`/
`Nexus1.CommandDispatch`, and that the outcome type must not expose
`Execute`/`TargetEndpoint`/`AuthorizationToken`/`CommandPayload`/
`MustCompleteBy` members (Executable Assets 36-AF, `RL-ADV-001`).

**Ch.36's own decision ledger (36-47) already answers the extraction
question**, independent of anything this ADR needs to decide: *"Keep RL
Advisory as an optional adapter/module. The chapter records how to prove
a future extraction, but present evidence does not justify a service
boundary."* And `RecommendationGenerated.v1`, the public event a future
extraction might publish, is named explicitly as **"NOT ACTIVATED... a
design candidate only"** (Decision Ledger 36-46) — the book itself has
not built the branch it describes, only specified how it *would* be
built if the evidence ever justified it. Ch.36's own runtime status for
the entire complete-slice scenario, core and optional alike, is recorded
as **PENDING**.

**A concrete finding that resolves the tension the architect posed**:
the atlas's own `AdvisoryRecommendation` table (C.11.4.6) — the schema
this whole project is contractually bound to for every sector — carries
no `VerdictId`, no `OperationalEventId`, no FK into `EventManagement` or
`RootCause` at all. Its only FKs are `AdvisorySessionId`,
`RecommendationStatusId`, `StateDefinitionId`, and
`RecommendedActionDefinitionId`/`ClampedActionDefinitionId`. It is
modeled purely as a live state-and-action consultation record, exactly
matching Chapter 10's synchronous design — **not** as a
verdict-triggered event record. Ch.36's own illustrative advisory store
(`AdvisoryInboxRow`, `AdvisoryDecisionRow` keyed by `SourceVerdictId`/
`SourceMessageId`/`PolicyIdentityDigest`) is a genuinely different table
shape that appears **nowhere in the atlas's 37-table ReinforcementLearning
list**. Building Ch.36's broker consumer faithfully would mean adding
tables the atlas never specifies — the first time in this entire Phase 2
build that "add the messaging branch" and "match the atlas schema" would
pull in two different directions, rather than the same direction the
other ten sectors' work always confirmed.

- **Whole-sector FK audit, done first and across the FULL 37-table graph
  before any scope trim.** Every external target across all 37 tables —
  `ReactorFleet.Unit`, `DigitalTwin.TwinModel`, `CorePlatform.
  EngineeringUnit`, `Security.ApplicationUser` — belongs to a context
  that already exists. **Zero whole-sector gaps — the fourth consecutive
  Phase 2 sector with a clean result**, and by a wide margin the
  *simplest* external dependency footprint of any Phase 2 sector so far
  (four contexts, versus EmergencyPreparedness's eight). No individual-
  table gaps either — the sector never references `ReactorFleet.Equipment`
  or `EquipmentLocation` at all.
- `Nexus1.DigitalTwin.Domain/TwinModel.cs` confirmed to exist directly
  (table `DigitalTwin.TwinModel`, key column `TwinModelId`) before
  counting on it as a real FK target.

## Decision — Part 1: Scope

### Twenty-five of thirty-seven tables — the atlas's own four named verification queries, plus an unusually deep FK-integrity chain

Query-by-query (C.11.5.2):

1. *The 35×5 policy should have one entry per state* joins `Policy`,
   `PolicyEntry`.
2. *A final Q-table should contain 175 state-action values* joins
   `QTable`, `QTableEntry`.
3. *Read the policy in console form* joins `PolicyEntry`,
   `StateDefinition`, `ActionDefinition`.
4. *Review clamped advisory recommendations* joins
   `AdvisoryRecommendation`, `StateDefinition`, `ActionDefinition`.

Union of directly-named tables: `Policy`, `PolicyEntry`, `QTable`,
`QTableEntry`, `StateDefinition`, `ActionDefinition`,
`AdvisoryRecommendation` — seven tables, **zero lookups** directly named
(a first — no prior sector's verification queries skipped every lookup
table).

Unlike every prior sector, this FK-integrity closure is unusually deep,
not shallow — real DDL (read in full, C.11.4.3–C.11.4.6) shows each of
those seven tables sits at the *end* of a long `NOT NULL` chain rather
than near its start:

- `Policy.QTableId` `NOT NULL` → `QTable` (already have) →
  `QTable.TrainingRunId`/`StateSpaceId`/`ActionSpaceId` all `NOT NULL` →
  `TrainingRun`, `StateSpace`, `ActionSpace`.
- `TrainingRun` itself has **seven** `NOT NULL` FKs: `ExperimentId`,
  `EnvironmentModelId`, `StateSpaceId`, `ActionSpaceId`,
  `RewardFunctionId`, `HyperparameterSetId`, `LearningAlgorithmId`,
  `TrainingRunStatusId` → pulls in `Experiment`, `EnvironmentModel`,
  `RewardFunction`, `HyperparameterSet`, plus three lookups.
- `AdvisoryRecommendation.AdvisorySessionId` `NOT NULL` → `AdvisorySession`
  → `AdvisorySession.PolicyDeploymentId` `NOT NULL` → `PolicyDeployment`
  (which also needs `Policy`, already have, plus `AdvisoryMode`).
- `StateDefinition`/`ActionDefinition` each need their own `StateSpace`/
  `ActionSpace` (already pulled) and those need their own `StateSpaceType`/
  `ActionSpaceType` lookups.

Same FK-integrity-closure reasoning every prior sector's ADR has used —
just a longer chain, honestly followed rather than truncated.

**In scope (25):** lookups `EnvironmentModelType`, `StateSpaceType`,
`ActionSpaceType`, `RewardFunctionType`, `LearningAlgorithm`,
`TrainingRunStatus`, `PolicyStatus`, `AdvisoryMode`,
`RecommendationStatus` (9); substantive `EnvironmentModel`, `StateSpace`,
`StateDefinition`, `ActionSpace`, `ActionDefinition`, `RewardFunction`,
`HyperparameterSet`, `Experiment`, `TrainingRun`, `QTable`, `QTableEntry`,
`Policy`, `PolicyEntry`, `PolicyDeployment`, `AdvisorySession`,
`AdvisoryRecommendation` (16).

This is the largest scope of any Phase 2 sector by table count — a
genuine consequence of the atlas's own structure, not scope creep. The
whole training→Q-table→policy→deployment→advisory pipeline the book
narrates across Chapters 2–10 turns out to be *one* connected `NOT NULL`
graph in the atlas's own DDL; there is no shallower verification-query-
justified cut available the way there was for every prior sector.

**Out of scope (12), grouped by reason, not a blanket cut:**

- **`EpisodeStatus` (lookup), `Episode`, `EpisodeStep`** — the raw
  experience stream (Chapter 6's "one real transition," Chapter 8's
  reward curve) is not touched by any of the four named queries. The
  *product* of training (`QTable`) is queried; the process that produced
  it is not. `Episode`/`EpisodeStep`'s own FKs are entirely internal
  (`TrainingRun`, `StateDefinition`, `ActionDefinition`), so excluding
  them creates no dangling reference elsewhere in scope.
- **`ModelArtifact`, `TrainingRunAuditTrail`** — provenance/audit detail
  the atlas names but no verification query exercises.
- **`EvaluationMetricType` (lookup), `PolicyEvaluation`,
  `PolicyEvaluationMetric`** — the validation-gate detail; query 1 counts
  `PolicyEntry` rows directly, it does not check whether the policy
  passed evaluation.
- **`SafetyClampType` (lookup), `SafetyClamp`, `SafetyClampRule`** — the
  clamp *configuration* tables. Confirmed by reading `AdvisoryRecommendation`'s
  own DDL directly: it has no FK to `SafetyClamp`/`SafetyClampRule` at
  all — `WasClamped`/`ClampReason` are a plain bit and free text,
  `ClampedActionDefinitionId` points at `ActionDefinition`, not at a
  clamp-rule row. Excluding the clamp-configuration tables therefore
  creates no FK-integrity gap in `AdvisoryRecommendation`.
- **`AdvisoryRecommendationReason`** — the "Why This Action" explanation
  bars that are Chapter 9's own hero figure. A real, narratively
  important table, but its FK points *to* `AdvisoryRecommendation`, not
  the reverse — an optional child, not a `NOT NULL` dependency of
  anything in scope, and untouched by any of the four named queries.

### Domain shape: a training-pipeline spine ending in a clamped, human-facing advisory record — not a controller

`EnvironmentModel` anchors training to a real `ReactorFleet.Unit` and
optionally a `DigitalTwin.TwinModel` — the atlas's own realization of
Chapter 3's *"you train against a fast point-kinetics surrogate, never
the real plant."* `TrainingRun` pins seven inputs (`NOT NULL`
everywhere) as one deterministic, reproducible configuration — Chapter
11's own *"given the same configuration and seed, you must be able to
regenerate the exact 175 numbers."* `QTable`/`QTableEntry` is the raw
learned table; `Policy`/`PolicyEntry` is the *readable* extraction from
it (`BestQValue`, `SecondBestQValue`, `ActionMargin`, `IsTie` — Chapter
9's grid, made queryable). `PolicyDeployment`/`AdvisorySession`/
`AdvisoryRecommendation` is the advisory pipeline itself:
`AdvisoryRecommendation` carries both `RecommendedActionDefinitionId`
(the raw policy pick) and `ClampedActionDefinitionId` (after the safety
clamp) side by side — Chapter 10's clamp step, realized as two columns
on one row rather than an overwrite, so the "what the table said" vs.
"what was actually offered" distinction survives in the data the way the
book insists it must survive in the UI. Nothing in this scope models
actuation; `AdvisoryRecommendation` has no execution timestamp, no
command target, no authorization token — the schema itself already
enforces Chapter 10's *"it advises; it does not act"* boundary, independently
of anything Ch.36's architecture-test gates would add.

### Application layer: the atlas's own four named verification queries

1. `GetPolicyEntryCountQuery` — `Policy` joined to a count of
   `PolicyEntry` rows, grouped by policy code (matches the 35-entry
   check).
2. `GetFinalQTableEntryCountQuery` — `QTable` where `IsFinal = 1` joined
   to a count of `QTableEntry` rows (matches the 175-value check).
3. `GetPolicyGridQuery(int PolicyId)` — `PolicyEntry` joined to
   `StateDefinition`/`ActionDefinition`, ordered by `StateIndex` (the
   console-readable grid).
4. `GetClampedRecommendationsQuery` — `AdvisoryRecommendation` where
   `WasClamped = 1`, joined to `StateDefinition` and both
   `ActionDefinition` references, ordered by `RequestedAtUtc DESC`.

Plus the write paths the sector's own core premise needs to produce data
for those four reads to have anything to report on: `RecordTrainingRunCommand`
(creates a `TrainingRun` against its seven-way pinned configuration) and
`ExtractPolicyCommand` (creates a `Policy` from a final `QTable`) — same
"read queries need at least one write path to be provably real" reasoning
every prior sector's Application layer used. `RecordAdvisoryRecommendationCommand`
completes the pipeline (creates an `AdvisoryRecommendation` against an
`AdvisorySession`), giving query 4 real data to exercise.

### Persistence: shares `AlarmManagementDb` — all three axes agree cleanly

- **Topology.** ReinforcementLearning is plant-operational learning-
  apparatus data, tightly anchored to `ReactorFleet.Unit` and
  `DigitalTwin.TwinModel`, both already in `AlarmManagementDb` — same
  category as every other plant-operational sector.
- **Sensitivity.** Training/policy/advisory data carries no personnel-HR
  or access-control sensitivity — ordinary operational-adjacent data,
  same tier as its siblings.
- **FK-locality.** All three real cross-context FK families
  (`ReactorFleet.Unit` — four columns across `EnvironmentModel`,
  `Experiment`, `PolicyDeployment`, `AdvisorySession`; `DigitalTwin.
  TwinModel` — one nullable column on `EnvironmentModel`; `CorePlatform.
  EngineeringUnit` — one nullable column on `ActionSpace`) already live
  in `AlarmManagementDb`. Sharing makes all three genuine same-database
  `FOREIGN KEY`s. `ReactorFleetUnitReference` and
  `CorePlatformEngineeringUnitReference` are the already-established,
  reused shadow entities; a new `DigitalTwinTwinModelReference` is needed
  — the second instance (after EmergencyPreparedness's
  `RadiationMonitoringRadiationZoneReference`) of a shadow entity
  targeting a table built within this same Phase 2 sequence rather than
  a V1 or early-Phase-2 context.

Own migration history (`__EFMigrationsHistory_ReinforcementLearning`),
own schema (`ReinforcementLearning`), same physical database.
`Security.ApplicationUser` references (`Experiment.OwnerUserId`,
`PolicyDeployment.DeployedByUserId`, `AdvisorySession.StartedByUserId`,
all nullable) stay passport-only, no enforced constraint — `SecurityDb`
is a separate physical database, the same downgrade every prior sector's
Security references has needed.

## Decision — Part 2: The optional advisory messaging branch

### The tension, restated precisely

Every other Phase 2 sector correctly followed "Domain + Application +
Infrastructure only, no messaging, no HTTP surface" because none of them
had a genuine, book-designed reason to serve a broker. This sector is
the first where a source document — Ch.36 of `From_Services_To_Runtime`
— explicitly designs a broker-consumption role for the module being
built. The question is whether that design should be built now.

### Option A — training/persistence only, no messaging

**Assessment.** This is the direct continuation of every decision this
project has made for ten consecutive sectors: Domain, Infrastructure,
and Application, verified by build → test → real host → health check →
evidence → commit, nothing more. It fully satisfies the atlas's own
25-table scope and all four of its named verification queries. It
produces a real, inspectable, testable `Policy`/`AdvisoryRecommendation`
pipeline — an operator (or a test) can genuinely populate a
`TrainingRun`, extract a `Policy`, and record a clamped
`AdvisoryRecommendation` end to end, exactly matching Chapter 10's own
synchronous design. Its only real cost is that the optional branch Ch.36
describes stays unbuilt — but Ch.36 itself has not built it either
(`RecommendationGenerated.v1` is "NOT ACTIVATED," runtime status
"PENDING"), so choosing Option A does not put this codebase behind its
own source material; it puts it exactly even with it.

### Option B — training/persistence plus the optional advisory branch

**Assessment.** This would mean a real, non-blocking `RootCause
VerdictIssued.v1` consumer, admission-gated and architecturally barred
from ever becoming a command path (Ch.36's own `RL-ADV-001` gates:
no dependency on `Nexus1.Actuation`/`Nexus1.AlarmManagement.Application`/
`Nexus1.CommandDispatch`; the outcome type forbidden from exposing
`Execute`/`TargetEndpoint`/`AuthorizationToken`/`CommandPayload`/
`MustCompleteBy`), with the same real-broker-proof discipline as every
Phase 1 messaging feature. Its central problem, found only by reading
Ch.36 and the atlas side by side rather than either alone: **Ch.36's own
illustrative persistence shape for the advisory decision
(`AdvisoryInboxRow`, `AdvisoryDecisionRow` keyed by `SourceVerdictId`)
does not exist anywhere in the atlas's 37-table `ReinforcementLearning`
list.** Building Option B faithfully to Ch.36 means adding tables this
project's authoritative schema source never specifies — the first time
in eleven sectors that "match the book's messaging design" and "match
the atlas's persistence contract" would genuinely diverge rather than
reinforce each other. Forcing the verdict-triggered outcome into the
atlas's own `AdvisoryRecommendation` shape instead is not a clean
alternative either: that table's `NOT NULL` chain (`AdvisorySessionId`
→ `PolicyDeploymentId`, `StateDefinitionId`, `RecommendedActionDefinitionId`)
demands a live state and a policy-selected action that a `RootCause`
verdict does not naturally supply — satisfying it would mean fabricating
a session and a state/action pair to make the row insertable, which is
not an honest data shape.

### Recommendation: Option A

Three independent lines of reasoning converge on the same answer, and
none of them is close:

1. **Consistency with this project's own established restraint.** Every
   prior "defer the speculative surface" decision in this codebase —
   MediatR (ADR-002-amend), the Query BFF (ADR-007) — was made the same
   way: build what has a real, present consumer; name the deferred piece
   explicitly; record the condition under which it gets revisited. RL
   Advisory's messaging branch has no present consumer either — Ch.36's
   own scenario harness is a test/evidence tool, not a production
   trigger, and its own runtime status is PENDING.
2. **Ch.36 has already made this decision, for the same reasons, at the
   book's own authority level.** Decision Ledger 36-47 concludes present
   evidence does not justify even a service boundary, let alone a
   messaging branch inside the existing host; `RecommendationGenerated.v1`
   is explicitly a non-activated design candidate. Choosing Option A is
   not this project falling short of its source material — it is this
   project matching what the source material itself has and has not
   built.
3. **The atlas-schema argument, found only by reading both sources
   together.** Option B's own natural persistence shape sits outside the
   atlas this whole Phase 2 build has treated as the authoritative
   schema contract for every one of the other ten sectors. Building it
   now would be the first sector where "what the messaging design wants"
   and "what the atlas specifies" pull in different directions —
   exactly the kind of quiet scope-widening this project's discipline
   exists to catch before it happens, not after.

The optional advisory branch stays explicitly deferred, not rejected —
same status as MediatR and the Query BFF: a real, named gap, recorded
here rather than silently absent.

## Consequences

- ReinforcementLearning becomes the eleventh and final sector sharing
  `AlarmManagementDb`'s physical database, with the simplest external
  dependency footprint of any Phase 2 sector (four contexts).
- This is the fourth consecutive Phase 2 sector with a clean (zero-gap)
  whole-sector FK audit result.
- `DigitalTwinTwinModelReference` becomes the second shadow entity in
  this codebase targeting a table built within this same Phase 2
  sequence rather than a V1 or early-Phase-2 context.
- `Episode`/`EpisodeStep`, `ModelArtifact`, `TrainingRunAuditTrail`,
  `PolicyEvaluation`/`PolicyEvaluationMetric`, `SafetyClamp`/
  `SafetyClampRule`, and `AdvisoryRecommendationReason` are explicitly
  recorded as out of this pass's scope for verification-query reasons,
  not forgotten.
- The optional RL Advisory messaging branch (`RootCauseVerdictIssued.v1`
  consumption, `RL-ADV-001` non-command enforcement,
  `RecommendationGenerated.v1` as a still-not-activated future contract)
  remains unbuilt, with an explicit reversal condition below.
- Phase 2 is complete once this sector's implementation, verification,
  and evidence report land — no further sectors remain in the CLAUDE.md
  Phase 2 ordering.

## Rejected alternatives

- **Option B, scoped down to reuse the atlas's own `AdvisoryRecommendation`
  table instead of Ch.36's `AdvisoryDecisionRow` shape.** Considered and
  rejected above — `AdvisoryRecommendation`'s own `NOT NULL` chain
  requires a live session/state/action a verdict message does not supply;
  satisfying it would require fabricated data, not an honest mapping.
- **Build the messaging branch as a separate, undeployed-by-default
  project now, to have it ready.** Rejected — this project's own
  precedent (MediatR, Query BFF) is to defer the code entirely, not to
  build it disabled; an unused, untested messaging path is a liability
  (dead code, false confidence) rather than a saved step, and Ch.36's own
  evidence-maturity ledger (36-48) treats "advisory-enabled" as a
  separate release claim requiring its own full proof, not something
  worth having half-built.

## Reversal condition

Revisit the optional advisory branch specifically once one of two things
exists: (a) a real consumer of `AdvisoryRecommendation` data beyond this
project's own component tests — an operator console, a demonstrator
scenario, or another sector that needs to react to a recommendation —
that would genuinely benefit from a verdict-triggered advisory instead of
(or alongside) the live-consultation path Chapter 10 already designs; or
(b) the atlas itself is extended with a verdict-triggered advisory table
shape, resolving the schema mismatch this ADR identifies. When either
condition arrives, decide the branch's exact shape — its own ADR, its own
Ch.36-style admission/idempotency/non-command proof, its own real-broker
evidence — rather than backfilling one now for a consumer that does not
exist. This mirrors ADR-007's own Query BFF reversal condition exactly:
a deferral with a named trigger, not an open-ended "maybe later."

## Evidence required (once this ADR is confirmed and implementation proceeds)

- `dotnet build` warning-clean.
- `dotnet test` green, including `Nexus1.ArchitectureTests`.
- Migration applied to the real `AlarmManagementDb`; `ReinforcementLearning.*`
  tables and the real cross-context FK constraints (`ReactorFleet.Unit`
  ×4, `DigitalTwin.TwinModel` ×1, `CorePlatform.EngineeringUnit` ×1)
  confirmed via `sys.foreign_keys`.
- Real host startup; `GET /health/ready` returns `200 Healthy` with a
  `reinforcementlearning-db` check present.
- Evidence report written only after all of the above are independently
  confirmed — build, test, real host, health check, evidence report,
  commit, in that order. Same discipline as every prior sector.
