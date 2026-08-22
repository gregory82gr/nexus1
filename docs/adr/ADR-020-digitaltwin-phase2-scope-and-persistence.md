# ADR-020: DigitalTwin (Phase 2, sector 5) — scope, domain shape, and persistence

## Status

Accepted.

## Context

Phase 2's fifth sector, atlas Appendix **C.6** (confirmed directly by
reading `"C.6.1 Sector purpose"`, not the file's own garbled table of
contents — same numbering-verification discipline ADR-019 already
established).

- `From_Schema_to_System` Appendix C.6: **forty-two tables** (fifteen
  lookup, twenty-seven substantive). C.6.1 calls this **"the keystone
  sector"**: it consumes the physical hierarchy from `ReactorFleet`, the
  signal registry from `Instrumentation`, engineering units from
  `CorePlatform`, and user identity from `Security`, and provides model,
  snapshot, divergence and simulation records to later sectors. C.6.1's
  own design choice: *"The divergence table is not an error log. It is a
  first-class data object. A twin that hides disagreement becomes unsafe
  to trust; a twin that records disagreement can be corrected, audited
  and used honestly."*
- `From_Domain_to_Twin` explicitly places DigitalTwin in the **Core
  domain** (Chapter 14's own core-area table: *"Digital Twin binding...
  It connects the physical unit to the model and records where they
  agree or diverge"* — `TwinModel`, `SignalBinding`, `TwinSnapshot`,
  `TwinDivergence` named directly as the chapter's own example concepts).
  This is the deepest-investment tier the book defines (*"The core domain
  deserves deeper examples, sharper invariants, better diagrams, and more
  domain tests"*) — a different signal than every other Phase 2 sector so
  far (Organization: absent from all three tables; Security: flat
  generic; Instrumentation: supporting).
- The book's own worked `TwinDivergence` examples (two separate versions,
  Ch.14 and Ch.23) both model `Difference`/divergence as **computed from
  modeled and measured values, not independently caller-supplied** — the
  first version literally: `public decimal Difference => MeasuredValue -
  ModelValue;`. The atlas's real DDL stores `DeltaValue FLOAT NOT NULL`
  as a plain column rather than a SQL computed column (unlike
  Instrumentation's `StaffingScenarioGap.GapCount`), but the book's own
  domain-modeling instinct — compute it, don't trust a caller-supplied
  value — is followed in this sector's domain shape below. **Named
  discrepancy, not acted on**: the book's second worked example uses a
  `SignalTag` value object and `EngineeringValue` owned-type pairs
  (`Value`+`UnitSymbol`) for Modelled/Measured, structurally different
  from the atlas's real schema (`SignalId INT` FK, plain
  `ModeledValue`/`MeasuredValue`/`DeltaValue`/`DeltaPercent`/
  `ThresholdAbs`/`ThresholdPercent` columns, real `DivergenceSeverity`/
  `DivergenceStatus` lookups). Per CLAUDE.md, the atlas is the schema
  authority; this ADR follows the atlas's real, richer shape.
- The atlas's own three "useful verification queries" (C.6.8) name real
  Application-layer operations: (1) every active twin and the unit it
  mirrors, (2) trace one model variable to the real signal feeding it,
  (3) list open divergences with measured signal and model value.
- The atlas's own C.6.7.3 ("Incoming passports from later sectors") names
  four future consumers: `AlarmManagement.AlarmEvent` →
  `TwinDivergence`, `RootCause.EvidenceItem` → `TwinDivergence`,
  `ReinforcementLearning.TrainingRun` → `TwinModelVersion`,
  `Reporting.ReportSnapshot` → `TwinSnapshot`, `Compliance.Evidence` →
  `TwinModelValidation`. **Three of these five (`AlarmManagement`,
  `RootCause`, `Reporting`) are already-built Phase 1 contexts** —
  confirmed via `grep`, none currently references any DigitalTwin table.
  This is the third occurrence of the same reversal-note pattern already
  named for Organization (ADR-004's `SiteId`/`LineId`) and Instrumentation
  (`SignalTag`/`SignalId`) — an expected, recurring consequence of Phase
  2's atlas-driven approach surfacing latent Phase 1 gaps sector by
  sector, not a surprise each time.
- `TwinModelComponent.EquipmentId`/`PlantSystemId` reference
  `ReactorFleet.Equipment`/`PlantSystem`, **with a `CHECK` constraint
  requiring at least one of the two to be non-null**
  (`CK_DigitalTwin_TwinModelComponent_PhysicalAnchor`). Confirmed again
  (same finding as ADR-019's Instrumentation correction): this codebase's
  actual `ReactorFleetDbContext` only exposes `Unit`/`UnitPowerSnapshot` —
  `Equipment`/`PlantSystem` were never built in Phase 1. Unlike
  Instrumentation's `Signal.EquipmentId`/`PlantSystemId` (both nullable,
  so the table itself still has a valid identity without them),
  `TwinModelComponent`'s entire reason to exist is anchoring a model
  component to physical equipment or a plant system — its own `CHECK`
  constraint cannot be satisfied by a passport-only-int downgrade in any
  way that preserves real meaning. This is caught **before** writing any
  code this time (ADR-019's correction was caught during implementation;
  this one is caught during drafting, applying the "verify before
  asserting" convention proactively rather than reactively).

## Decision

### Scope: twenty of forty-two tables — the model/binding/runtime/snapshot/divergence spine the atlas's own three queries and five bullet points name, minus the one capability physical-anchor tables can't support and the separable simulation/what-if capability

**Built** (11 lookups + 9 substantive):

- Lookups: `TwinModelType`, `TwinModelStatus`, `TwinFidelityLevel`,
  `ModelVariableType`, `SolverType`, `ValidationStatus`, `BindingRole`,
  `BindingStatus`, `SnapshotReason`, `DivergenceSeverity`,
  `DivergenceStatus` — every lookup required by the nine substantive
  tables below, and nothing else.
- `TwinModel` — C.6.8 query 1's own subject; C.6.1's first bullet
  ("names the physical unit mirrored by the model").
- `TwinModelVersion` — required by `TwinRuntimeSession` (see below); the
  atlas's own `ApprovedByUserId` passport (Security, see Persistence).
- `TwinVariable` — required by `SignalBinding`/`TwinSnapshotValue`/
  `TwinDivergence`; C.6.1's variable catalogue.
- `SignalBinding` — C.6.8 query 2's own subject; C.6.1's second bullet
  ("wires a model variable to a real signal").
- `TwinRuntimeSession` — required by `TwinSnapshot`; without it,
  `TwinSnapshot` has no valid parent and C.6.1's third bullet
  ("freezes the model state at a timestamp") cannot be satisfied at all.
- `TwinSnapshot` + `TwinSnapshotValue` — C.6.1's third bullet, built as a
  pair deliberately: `TwinSnapshot` alone (no values) would be an empty
  shell that "freezes" nothing.
- `TwinDivergence` — C.6.8 query 3's own subject; C.6.1's fourth bullet
  and the sector's own "conscience table" framing.
- `TwinDivergenceReview` — closes the review loop C.6.1's design choice
  explicitly asks for ("a twin that records disagreement can be
  corrected, audited and used honestly" — recording the disagreement
  without a review path would only half-honor that design choice).

**Not built, with reasoning per group** (4 lookups + 18 substantive):

- **`TwinModelComponent`** (1) — its own `CHECK` constraint requires a
  real link to `ReactorFleet.Equipment` or `PlantSystem`, neither of
  which exists in this codebase's actual Phase 1 `ReactorFleet` scope (see
  Context). Unlike Instrumentation's nullable-FK exclusions, there is no
  faithful passport-only-int version of this table that preserves its
  actual meaning — excluded entirely, not downgraded.
- **`TwinParameter`** (1) — real and well-specified (versioned solver/
  calibration constants), but not touched by any of the three
  verification queries; secondary to the model/binding/snapshot/
  divergence spine.
- **`SignalBindingCalibration`** (1) — calibration provenance for a
  binding; same "no consumer named" reasoning as Instrumentation's
  `CalibrationRecord` exclusion.
- **`TwinSynchronization`, `TwinStateVector`** (2, runtime internals) —
  replay/restart machinery; not touched by any verification query.
- **`SimulationScenario`, `SimulationScenarioInput`, `SimulationRun`,
  `SimulationRunStep`, `SimulationRunOutput`, `WhatIfCase`,
  `WhatIfCaseInput`, `WhatIfCaseResult`** (8, the entire simulation/
  what-if capability) plus **`SimulationScenarioType`,
  `SimulationRunStatus`** (2 lookups) — C.6.1's own fifth bullet
  ("SimulationRun and WhatIfCase make the twin usable for training,
  replay and analysis") names this as real, distinct future value, but
  zero verification query touches it and its only named incoming
  consumer (`ReinforcementLearning.TrainingRun`) is a sector that doesn't
  exist yet anywhere in this project's build order. A coherent, separable
  future slice — not speculative plumbing, but not this step's job either.
- **`TwinHealthCheck`** (1) — periodic ops health reporting; no
  verification query touches it.
- **`TwinModelValidation`, `TwinValidationMetric`** (2) plus
  **`ValidationStatus`... already included above for `TwinModelVersion`,
  so only the two substantive tables are excluded here** — real
  validation-campaign apparatus with a named outgoing passport
  (`Compliance.Evidence`), but `Compliance`'s own Phase 1 build was never
  revisited to consume it and no verification query touches it here
  either — the same "pick up when the actual consumer needs it" reasoning
  already used for Instrumentation's `Certification` deferral.
- **`ModelAssumption`, `TwinAnnotation`** (2, governance/documentation
  apparatus) — no verification query touches either.

This lands at 20/42 ≈ 48% — between Instrumentation's 38% and
Organization's 68%, matching a Core-domain sector: more of the spine gets
built than a Supporting-domain sector's minimal passport-plus-queries cut
(Instrumentation), but a real, separable secondary capability
(simulation/what-if) still gets deferred rather than built preemptively,
the same restraint discipline every prior sector has applied to its own
speculative apparatus.

### Domain shape: Core-domain investment — real invariants, computed divergence, not boring pass-through

Matching the book's own strategic-design instruction for the core domain
(*"sharper invariants... more domain tests"*):

- `TwinModel`, `TwinModelVersion`, `TwinVariable`, `SignalBinding`,
  `TwinRuntimeSession`, `TwinSnapshot`, `TwinSnapshotValue` get `Create`
  factories enforcing the atlas's real `CHECK`s (`TwinVariable`'s
  `LowerBound <= UpperBound`, `SignalBinding`'s effective-date range,
  `TwinRuntimeSession`'s time range, `TwinSnapshotValue`'s one-value
  check).
- **`TwinDivergence.Create` computes `DeltaValue` itself**
  (`MeasuredValue - ModeledValue`), never accepting it as an independent
  caller-supplied parameter — even though the atlas's own DDL does not
  mark `DeltaValue` as a SQL computed column (unlike
  `StaffingScenarioGap.GapCount`), the book's own worked `TwinDivergence`
  examples (`Difference => MeasuredValue - ModelValue`) make the domain
  intent unambiguous: a twin that could disagree with its own arithmetic
  about how much it disagrees with reality would undermine the sector's
  entire "conscience table" premise. `DeltaPercent` is left
  caller-supplied (nullable, threshold-relative — no single correct
  derivation the atlas specifies).
- `TwinDivergenceReview` gets a real review-recording behavior (`Create`
  with the reviewer's disposition), not just storage.

Audit columns not modeled in Domain, same restraint as every prior
sector.

### Application layer: the atlas's own three named verification queries

- `GetActiveTwinsForFleetQuery` — C.6.8 query 1, verbatim.
- `TraceModelVariableToSignalQuery` — C.6.8 query 2, verbatim.
- `GetOpenDivergencesQuery` — C.6.8 query 3, verbatim.
- `CaptureTwinSnapshotCommand` — writes a `TwinSnapshot` plus its
  `TwinSnapshotValue` rows in one operation, matching how a snapshot
  actually gets produced (all variable values at once, not one at a
  time).
- `RecordTwinDivergenceCommand` — `TwinDivergence`'s defining behavior
  (computes `DeltaValue`, see above).
- `ReviewTwinDivergenceCommand` — `TwinDivergenceReview`'s defining
  behavior.

### Persistence: shares `AlarmManagementDb` — same FK-locality reasoning as Instrumentation, now with a third co-located dependency

Re-derived per this sector's own facts:

- **Deployment topology**: no independent deployment, same as every
  prior sector — argues for sharing.
- **Data sensitivity**: model definitions, variable catalogues, signal
  bindings, divergence values are all operational/technical data — no
  PII, no credentials.
- **FK locality**: every external reference this sector's built scope can
  support as a real FK — `TwinModel.UnitId` → `ReactorFleet.Unit`,
  `SignalBinding.SignalId`/`TwinDivergence.SignalId` →
  `Instrumentation.Signal`, `TwinVariable.EngineeringUnitId` →
  `CorePlatform.EngineeringUnit` — points at contexts that **all three**
  already live in `AlarmManagementDb` (`ReactorFleet` since ADR-006,
  `CorePlatform` since ADR-015, `Instrumentation` since ADR-019). This is
  a stronger case than Instrumentation's own (which had two co-located
  dependencies); DigitalTwin has three.

Instrumentation is composed sharing `AlarmManagementDb` (own
`DigitalTwin` SQL schema, own migration-history table
`__EFMigrationsHistory_DigitalTwin`), and every one of those three
external references becomes a real, enforced FK using the
`ExcludeFromMigrations` shadow-entity technique ADR-019 introduced for
Instrumentation — now an established, reusable pattern for this codebase
rather than a one-off (a fourth shadow reference,
`InstrumentationSignalReference`, joins
`ReactorFleetUnitReference`/`CorePlatformEngineeringUnitReference`).

`TwinModelVersion.ApprovedByUserId`, `TwinRuntimeSession.
StartedByUserId`, `TwinDivergenceReview.ReviewedByUserId` all reference
`Security.ApplicationUser` — `SecurityDb` is a separate physical database
from `AlarmManagementDb` regardless of where DigitalTwin itself lives, so
all three stay plain nullable passport ints with no enforced constraint,
the same downgrade every prior sector's Security references has needed.

### Reversal note — third occurrence, recorded, not acted on

`AlarmManagement`, `RootCause`, `Reporting` — three of C.6.7.3's five
named future consumers, all three already built in Phase 1 — currently
have no reference to any DigitalTwin table (confirmed via `grep`).
DigitalTwin's existence makes wiring `AlarmManagement.AlarmEvent`/
`RootCause.EvidenceItem` → `TwinDivergence` and `Reporting.ReportSnapshot`
→ `TwinSnapshot` theoretically possible for the first time. **This ADR
does not perform any such retrofit.** Per the same explicit-instruction
pattern already applied twice before, the door is noted open, left
closed.

## Consequences

- `Nexus1.DigitalTwin.Domain`, `Nexus1.DigitalTwin.Application`,
  `Nexus1.DigitalTwin.Infrastructure` — composed into
  `Nexus1.ModularRuntime` only, sharing `AlarmManagementDb` alongside
  `ReactorFleet`/`CorePlatform`/`AlarmManagement`/`Instrumentation`.
- Twenty-two tables remain unbuilt, named explicitly above in eight
  groups — not a blanket cut. The entire simulation/what-if capability
  (10 tables including lookups) is the largest deferred slice of any
  Phase 2 sector so far, recorded as coherent future work, not silently
  dropped.
- Three more already-built Phase 1 contexts (`AlarmManagement`,
  `RootCause`, `Reporting`) now have a technically-possible-but-not-
  performed reference into this sector, joining Organization's and
  Instrumentation's own reversal notes.
- The `ExcludeFromMigrations` shadow-entity technique for real
  cross-context FKs, introduced ad hoc by Instrumentation, is now used a
  second time — worth promoting to an explicitly-named, documented
  pattern if a third sector needs it.

## Rejected alternatives

- **Build `TwinModelComponent` with passport-only ints for `EquipmentId`/
  `PlantSystemId`, same as Instrumentation's `Signal.EquipmentId`/
  `PlantSystemId`.** Rejected: `TwinModelComponent`'s own `CHECK`
  constraint requires at least one of the two — a passport-only version
  could still store data, but the table's entire reason to exist (mapping
  a model component to a *real* physical anchor) would be unfalsifiable
  once neither column enforces anything, which is worse than not
  building it.
- **Build the full simulation/what-if capability now, since C.6.1 names
  it as one of the sector's five defining behaviors.** Rejected: real and
  well-specified, but zero verification query or currently-buildable
  incoming passport touches it — its only named consumer
  (`ReinforcementLearning`) doesn't exist in this project's build order
  yet, and building it now would be exactly the "provisioning for a
  boundary that doesn't exist" mistake ADR-006/ADR-016 already avoided
  elsewhere.
- **Model `TwinDivergence` using the book's `SignalTag`/`EngineeringValue`
  owned-type shape instead of the atlas's `SignalId`/plain-column
  shape.** Rejected: the atlas is the schema authority per CLAUDE.md; the
  book's second worked example is illustrative of the *owned-type*
  technique, not a literal schema to copy over a richer, already-verified
  real schema.
- **Retrofit `AlarmManagement`/`RootCause`/`Reporting` with real
  divergence/snapshot references now.** Rejected per the same explicit
  instruction pattern as the two prior reversal notes.

## Evidence required

- Domain unit tests, no persistence, for all nine substantive entities'
  creation validation and real behaviors — especially `TwinDivergence`'s
  computed `DeltaValue` (verified by reading the actual `Create` factory,
  not assumed from a comment).
- `dotnet ef migrations add` producing a readable migration under
  `Nexus1.DigitalTwin.Infrastructure`, targeting the `DigitalTwin` SQL
  schema against the existing `AlarmManagementDb`, independent migration
  history, real (not passport-only) foreign keys to `ReactorFleet.Unit`,
  `Instrumentation.Signal`, `CorePlatform.EngineeringUnit` — verified
  directly against `sys.foreign_keys` on the live database, the same
  independent-verification standard ADR-019 established.
- Component tests against real LocalDB for the six Application-layer
  operations, including all three atlas verification queries proven
  against real seeded data spanning `ReactorFleet`, `Instrumentation`,
  `CorePlatform`, and `DigitalTwin` migrations together.
- `Nexus1.ArchitectureTests` passing with `Nexus1.DigitalTwin.*` composed
  into `Nexus1.ModularRuntime` and correctly classified.
- Real host startup with a `digitaltwin-db` health check reaching
  `AlarmManagementDb`, confirmed via the ADR-018-strengthened
  `DbContextHealthCheck<T>` — the second real confirmation (after
  Instrumentation) that the fix correctly handles a new schema being
  added to an already-migrated shared database.
