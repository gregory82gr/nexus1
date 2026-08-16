# ADR-019: Instrumentation (Phase 2, sector 4) — scope, domain shape, and persistence

## Status

Accepted.

## Context

Phase 2's fourth sector. Verified directly against the source material
before writing any code. **Numbering correction, caught before any code
was written**: `From_Schema_to_System`'s own printed table of contents
(pdftotext-garbled, its numbers off by one against the real section
headers, confirmed by reading actual `"C.N.1 Sector purpose"` headers
directly rather than trusting the table) puts Instrumentation at
Appendix **C.5**, not C.4 or C.8 as two different reads of the mangled
TOC would suggest. Real sequence, confirmed by reading each section's own
`Sector purpose` heading: C.1 CorePlatform, C.2 Security, C.3
Organization, C.4 ReactorFleet (the atlas's own full-fidelity
documentation of the sector Phase 1 already partially built), **C.5
Instrumentation**.

- `From_Schema_to_System` Appendix C.5: **forty tables** (fourteen lookup,
  twenty-six substantive) — one of the largest sectors, "because it
  stands between the physical registry and every data-consuming engine"
  (C.5.1). Owns the canonical signal registry (`Signal`), the acquisition
  chain mapping tags to physical/protocol sources, high-volume historian
  measurements, and signal quality/gap/calibration evidence. C.5.1's own
  design choice: *"The historian value table is intentionally narrow:
  SignalId, timestamp, value, quality and source. Rich metadata lives
  beside it in normalized tables so the hot path remains fast."* C.5.9's
  own boundary: *"The design deliberately records quality and gaps
  because a value without quality is not evidence."*
- `From_Domain_to_Twin`'s Chapter 14 explicitly classifies Instrumentation
  as a **Supporting domain**, alongside Alarm Management, Maintenance,
  Robotics, and Emergency Preparedness — the same tier as three contexts
  already built in Phase 1. Its own guidance row for Instrumentation:
  *"Feeds signals and readings to alarms, root cause, and the twin...
  Is the tag stable and unambiguous?"* — a real, present design concern
  (unlike Security's flat "generic, boring" classification), but scoped
  to *integration reliability*, not the deepest core-domain modeling
  investment (reserved for DigitalTwin/RootCause/ReinforcementLearning).
- The book's own worked SQL-schema chapter (Ch.22) shows the *intended*
  cross-context reference shape directly: its `AlarmManagement.AlarmEvent`
  example carries `SignalTag NVARCHAR(80) NOT NULL` — explicitly labeled
  *"Passport or published identity of the signal," owner of truth
  Instrumentation* — not a real `SignalId` foreign key. The book's own
  design intent for signal references from other contexts is a **string
  tag passport**, distinct from the hard-integer-FK passport pattern it
  uses for `UnitId`. This directly informs the reversal-note handling
  below.
- The atlas's own C.5.7.3 ("Incoming passports from later sectors") names
  every relationship other sectors declare into Instrumentation, and it is
  unusually narrow: `DigitalTwin.SignalBinding`/`TwinDivergence`,
  `AlarmManagement.AlarmDefinition`/`AlarmEvent`, `RootCause.CausalNode`/
  `EvidenceItem`, `ReinforcementLearning.Observation`,
  `Reporting.ReportSignal` — **every single one references
  `Instrumentation.Signal` and nothing else in the sector.** No incoming
  passport touches `Measurement`, `Sensor`, `Instrument`, calibration, or
  any historian-apparatus table.
- The atlas's own four "useful verification queries" (C.5.8) name real
  Application-layer operations: (1) active historized signals for a unit,
  (2) latest measurements for a tag, (3) stale/bad signals for a unit
  (`SignalQualityEvent`), (4) acquisition path from tag to raw point
  (`SignalMapping`→`AcquisitionPoint`→`AcquisitionConnection`→
  `DataAcquisitionNode`). None of the four touches `Instrument`, `Sensor`,
  `SensorChannel`, calibration, or historian-apparatus tables either —
  confirmed directly by reading each query's own `JOIN` list, not assumed.
- **`Signal.SensorChannelId` is nullable in the atlas's own DDL** — the
  schema itself treats the physical-hardware chain (`Instrument`→
  `Sensor`→`SensorChannel`) as optional to a signal's identity, not a
  required dependency. This is a stronger, schema-level confirmation of
  the same exclusion signal the verification queries and incoming
  passports already point to.
- AlarmManagement, RootCause, and Reporting — all three of C.5.7.3's
  already-built Phase 1 consumers — were checked directly (`grep` across
  each context's `Domain` project): **none currently has any
  `SignalTag`/`SignalId`-shaped field at all.** Confirms the same "deferred
  because the dependency didn't exist yet" situation ADR-004 already
  named for Organization's `SiteId`/`LineId`, now applying simultaneously
  to three contexts with respect to signal identity.

## Decision

### Scope: fifteen of forty tables — the passport-carrying signal registry plus the atlas's own four named operations

**Built** (8 lookups + 7 substantive):

- Lookups: `SignalType`, `SignalCategory`, `SignalRole`, `SamplingMode`,
  `HistorianRetentionClass` (all required by `Signal`'s own DDL),
  `SignalQuality`, `MeasurementSource` (required by `Measurement`),
  `ChannelStatus` (required by the acquisition chain).
- `Signal` — the passport-carrying core; every one of C.5.7.3's five
  incoming-passport relationships references this table and nothing else
  in the sector.
- `DataAcquisitionNode`, `AcquisitionConnection`, `AcquisitionPoint`,
  `SignalMapping` — C.5.8 query 4's exact join path, tracing a tag to its
  raw acquisition source.
- `Measurement` — C.5.8 query 2, the atlas's own deliberately-narrow
  historian fact table, with a real invariant worth modeling
  (`CK_Instrumentation_Measurement_OneValue`: `NumericValue` or
  `TextValue`, not neither).
- `SignalQualityEvent` — C.5.8 query 3, directly matching C.5.9's own
  "a value without quality is not evidence" emphasis.

**Not built, with reasoning per group** (6 lookups + 19 substantive):

- **`SensorType`, `SensorStatus`, `CalibrationStatus`, `ThresholdType`,
  `BackfillJobStatus`, `LineageRole`** (6 lookups) — each backs only an
  excluded group below.
- **`Instrument`, `Sensor`, `SensorChannel`** (3, "INSTRUMENTS" group) —
  no verification query or incoming passport touches any of the three,
  and `Signal.SensorChannelId` is nullable in the atlas's own DDL,
  confirming the schema itself decouples signal identity from the
  physical-hardware layer. A coherent, well-specified future slice, not
  speculative plumbing — but nothing this project's confirmed dependency
  graph needs yet.
- **`SignalAlias`, `SignalGroup`, `SignalGroupMember`, `SignalDependency`,
  `SignalLineage`** (5, alias/grouping/derivation apparatus) — zero
  verification-query or incoming-passport references.
- **`SignalLimit`, `SignalDeadband`** (2, "LIMITS" group) — zero
  references; alarm-threshold-adjacent, but `AlarmManagement` does not
  consume `Instrumentation` at all yet (see the reversal note below), so
  building alarm-preparation limits now would provision for a boundary
  that doesn't exist — the same mistake ADR-006 already named and ADR-016
  avoided repeating for Security's session/token tables.
- **`CalibrationPlan`, `CalibrationRecord`** (2, "CALIBRATION" group) —
  real and well-specified, but none of the four verification queries
  touches calibration; the atlas's own highlighted operations are
  signal/measurement/quality/acquisition, not calibration workflow.
- **`MeasurementAggregate`, `MeasurementAnnotation`, `DataGap`,
  `HistorianRetentionPolicy`, `HistorianImportBatch`,
  `HistorianImportBatchItem`, `HistorianBackfillJob`** (7, historian
  apparatus beyond the raw fact table) — real value (chart aggregates,
  gap detection, import/backfill jobs) but none of the four verification
  queries touches any of them; secondary to the "trace a tag, read its
  quality-tagged values" operations the atlas itself highlights.

This lands at 15/40 ≈ 38%, closer to Security's trim ratio (31%) than
Organization's (68%) — a different, honestly-derived answer each time:
Organization's C.3.7.4 named a broad set of future consumers across six
different sectors touching many different tables; Instrumentation's
C.5.7.3 is unusually narrow, naming five future consumers that all
converge on exactly one table. The ratio is a consequence of what the
atlas actually signals, not a target chosen in advance.

### Domain shape: Supporting-domain integration reliability — real invariants on identity and quality, not deep modeling investment

Matching the book's own "Is the tag stable and unambiguous?" design
question: `Signal.Tag` is modeled as the real unique business key
(`UQ_Instrumentation_Signal_Tag`), with a `Create` factory enforcing the
atlas's real `CHECK`s (`NormalMax > NormalMin` when both set,
`ScanRateHz > 0` when set). `SignalMapping`'s `EffectiveFromUtc`/
`EffectiveToUtc` gets the same time-bounded-history behavior pattern
already used for Organization's `DepartmentAssignment`/`TeamMembership`
(`CK_Instrumentation_SignalMapping_Effective`). `SignalQualityEvent` gets
its own `StartedAtUtc`/`EndedAtUtc` range check plus an `End(DateTime,
string?)`-shaped close-out behavior, directly serving C.5.9's "quality and
gaps" emphasis. `Measurement`'s `Create` factory enforces
`CK_Instrumentation_Measurement_OneValue` (numeric or text value present,
not neither) — a real invariant, not decoration. Audit columns not
modeled in Domain, same restraint as every prior sector.

### Application layer: the atlas's own four named verification queries

- `GetActiveHistorizedSignalsForUnitQuery` — C.5.8 query 1, verbatim.
- `GetLatestMeasurementsForTagQuery` — C.5.8 query 2, verbatim.
- `GetOpenSignalQualityEventsForUnitQuery` — C.5.8 query 3, verbatim.
- `GetAcquisitionPathForTagQuery` — C.5.8 query 4, verbatim.
- `RecordMeasurementCommand` — `Measurement`'s defining behavior.
- `OpenSignalQualityEventCommand` / `CloseSignalQualityEventCommand` —
  `SignalQualityEvent`'s defining lifecycle.

### Persistence: shares `AlarmManagementDb` — the cleanest sharing case yet, both axes agree

Re-derived, not copied, following the same two-axis check as every prior
sector:

- **Deployment topology**: no independent deployment exists for
  Instrumentation any more than for CorePlatform/ReactorFleet/
  Organization/Security — this alone argues for sharing.
- **Data sensitivity**: within this fifteen-table scope, every column is
  operational telemetry (tags, numeric values, timestamps, quality codes)
  — no PII, no credential-adjacent data. Nothing pulls toward isolation
  the way Security's credentials or Organization's `Person` PII did.
- **Cross-database FK locality — the deciding factor**: `Signal`'s own DDL
  has *required* FKs to `ReactorFleet.Unit` and *optional* FKs to
  `ReactorFleet.Equipment`/`PlantSystem`, plus a *required* FK to
  `CorePlatform.EngineeringUnit`; `DataAcquisitionNode` also FKs to
  `ReactorFleet.Unit`. **Every external reference in this chosen
  fifteen-table scope points at either `ReactorFleet` or `CorePlatform`
  — both of which already live in `AlarmManagementDb`.** None of the
  four excluded tables that reference `Security.ApplicationUser`
  (`CalibrationRecord`, `MeasurementAnnotation`, `HistorianImportBatch`,
  `HistorianBackfillJob`) are in scope, so there is no
  Security-cross-database tension to weigh here at all — unlike
  Organization/Security, this decision has no competing pull.

Both axes agree with the FK-locality argument: Instrumentation is
composed into `Nexus1.ModularRuntime` sharing `AlarmManagementDb` (own
`Instrumentation` SQL schema, own migration-history table
`__EFMigrationsHistory_Instrumentation`).

**Correction caught during implementation, not before**: this section
originally claimed every external reference in scope would be a real,
enforced FK. That is true for `Signal.UnitId`/`DataAcquisitionNode.UnitId`
(→ `ReactorFleet.Unit`) and `Signal.EngineeringUnitId` (→
`CorePlatform.EngineeringUnit`) — confirmed against the live database
below. It is **not** true for `Signal.EquipmentId`/`PlantSystemId`: this
codebase's actual `ReactorFleetDbContext` only ever exposes `Unit`/
`UnitPowerSnapshot` (`Unit.cs`'s own comment: *"the Schema Atlas's
Reactor/Equipment/etc. tables are deliberately not modeled yet"*, per
ADR-003's Phase 1 trim). A `FOREIGN KEY` cannot reference a table that
does not exist, so `EquipmentId`/`PlantSystemId` are plain nullable
passport ints with no enforced constraint — the same category of
"verify before asserting generalizes" correction ADR-016/ADR-017 already
had to make once each, caught here during implementation rather than
before writing this ADR, since the ReactorFleet Phase 1 scope fact wasn't
re-checked until code was actually written against it. Also net-new to
this codebase: no existing context had ever declared a real cross-context
FK before (`AlarmManagement.UnitId` is itself passport-only despite
sharing `AlarmManagementDb`), so there was no existing EF Core pattern to
copy for `Signal.UnitId`/`EngineeringUnitId`'s real FKs. The technique
used — a local, read-only shadow entity type per external table
(`ReactorFleetUnitReference`, `CorePlatformEngineeringUnitReference`),
mapped via `ToTable(..., ExcludeFromMigrations())` onto the same physical
table another context's own migration already owns — lets EF declare a
genuine `HasOne`/`WithMany` foreign key without an Infrastructure-layer
`ProjectReference` across contexts, which `Nexus1.ArchitectureTests`'
dependency-law test forbids. Verified directly: the generated migration's
`FOREIGN KEY` statements target `ReactorFleet.Unit`/
`CorePlatform.EngineeringUnit` with no matching `CreateTable` for either
(confirmed by reading the migration file), and `sys.foreign_keys` against
the live `AlarmManagementDb` after applying the migration shows exactly
three real cross-schema constraints:
`FK_Instrumentation_DataAcquisitionNode_Unit`,
`FK_Instrumentation_Signal_Unit`,
`FK_Instrumentation_Signal_EngineeringUnit`.

With that correction, every external reference this sector's actual
scope can support as a real FK is one — no passport-only-int downgrade
was needed for any table that has a real principal to reference, a first
among the four Phase 2 sectors built so far.

### Reversal note — recorded, not acted on

`AlarmManagement`, `RootCause`, and `Reporting` (three of C.5.7.3's five
named future consumers, all already built in Phase 1) currently have no
signal reference at all — confirmed directly, not assumed, by grepping
each context's `Domain` project. Instrumentation's existence makes wiring
these theoretically possible for the first time. **This ADR does not
perform any such retrofit.** Per the same explicit instruction pattern
already applied to Organization/ADR-004: the door is noted open, left
closed unless a future session is explicitly asked to walk through it.
One additional, specific note for whoever picks this up later: the book's
own worked example (Ch.22, `AlarmManagement.AlarmEvent`) models the
signal reference as a **`SignalTag` string passport**, not a real
`SignalId` foreign key — if this reversal is ever performed, that is the
pattern the source material itself specifies, not an automatic hard FK.

## Consequences

- `Nexus1.Instrumentation.Domain`, `Nexus1.Instrumentation.Application`,
  `Nexus1.Instrumentation.Infrastructure` — composed into
  `Nexus1.ModularRuntime` only (no independent host), sharing
  `AlarmManagementDb` alongside `ReactorFleet`/`CorePlatform`/
  `AlarmManagement`.
- Twenty-five tables remain unbuilt, named explicitly above in six
  groups — not a blanket cut.
- `AlarmManagement`, `RootCause`, `Reporting` still carry no signal
  reference — flagged as a now-possible, not-yet-performed future
  decision, with the book's own `SignalTag`-string-passport pattern
  recorded for whoever picks it up.
- Numbering correction recorded above (Instrumentation is atlas C.5, not
  C.4 or C.8) applies to any future session reading this same atlas file —
  trust `"C.N.1 Sector purpose"` headers directly, not the printed table
  of contents.

## Rejected alternatives

- **Isolate into its own `InstrumentationDb`, matching Organization/
  Security's precedent of isolating PII/credential-bearing sectors.**
  Rejected: nothing in this fifteen-table scope is PII- or
  credential-adjacent, and isolating would force every `ReactorFleet`/
  `CorePlatform` reference in scope into a passport-only int for no
  sensitivity reason — actively worse than sharing, not neutral.
- **Build the full forty-table sector for atlas fidelity.** Rejected:
  twenty-five tables have no named consumer anywhere in C.5.7.3's own
  forward-reference list or C.5.8's own verification queries, and
  `Signal.SensorChannelId`'s nullability confirms the schema itself does
  not require the largest excluded group (`Instrument`/`Sensor`/
  `SensorChannel`).
- **Retrofit `AlarmManagement`/`RootCause`/`Reporting` with real signal
  references now that Instrumentation exists.** Rejected per the same
  explicit-instruction pattern as ADR-017's ADR-004 note: technical
  possibility is not itself a trigger; recorded as an open door, left
  closed.

## Evidence required

- Domain unit tests, no persistence, for all seven substantive entities'
  creation validation and real behaviors (`Signal`'s range/scan-rate
  checks, `SignalMapping`'s effective-date range, `SignalQualityEvent`'s
  open/close lifecycle, `Measurement`'s one-value-required check).
- `dotnet ef migrations add` producing a readable migration under
  `Nexus1.Instrumentation.Infrastructure`, targeting the `Instrumentation`
  SQL schema against the existing `AlarmManagementDb` physical database,
  independent migration history, real (not passport-only) foreign keys to
  `ReactorFleet.Unit`/`Equipment`/`PlantSystem` and
  `CorePlatform.EngineeringUnit`.
- Component tests against real LocalDB for the six Application-layer
  operations, including all four atlas verification queries proven
  against real seeded data.
- `Nexus1.ArchitectureTests` passing with `Nexus1.Instrumentation.*`
  composed into `Nexus1.ModularRuntime` and correctly classified.
- Real host startup with an `instrumentation-db` health check reaching
  `AlarmManagementDb`, verified with the ADR-018-strengthened
  `DbContextHealthCheck<T>` (pending-migrations check, not just
  connectivity) — the first sector built since that fix, so this is also
  the first real confirmation the strengthened check behaves correctly
  for a *new* schema being added to an *already-migrated* shared
  database, not just re-verification of sectors that were already
  correct.
