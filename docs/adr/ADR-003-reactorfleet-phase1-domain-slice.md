# ADR-003: ReactorFleet Phase 1 domain slice and aggregate boundary

## Status

Accepted.

## Context

Building `Nexus1.ReactorFleet.Domain` requires two sources per CLAUDE.md §1:
`From_Schema_to_System` for table/key shape (priority 3) and
`From_Domain_to_Twin` for aggregate boundaries, invariants, value objects,
and domain events (priority 4). Reading both directly (not inferring)
surfaced a real gap between them, not a simple naming mismatch.

**Schema Atlas side** (Appendix C.4): ReactorFleet is a 48-table physical
model — 16 lookup tables plus 32 substantive tables forming a deep tree:
`Fleet → Unit → Reactor → ReactorCore → {CoreRegion, FuelAssembly,
ControlRodBank → ControlRod}`, a parallel `PrimaryLoop → {ReactorCoolantPump,
SteamGenerator}` branch, `Pressurizer`, a `TurbineTrain → TurbineStage`,
`Generator`, `Transformer`, `SwitchyardBay`, `GridConnection` power-conversion
branch, and time-series snapshot tables (`RodPositionSnapshot`,
`BoronChemistrySnapshot`, `UnitPowerSnapshot`, `FleetSnapshot`). The atlas
frames `Unit` as "the most important passport provider for later sectors"
and `Equipment` as "the central component registry," but states only
FK/ownership facts, never an aggregate/consistency-boundary claim (C.4.9:
"a unit has one identity, equipment has one registry... later sectors refer
to those rows rather than inventing their own copies").

**Domain_to_Twin side**: there is no dedicated ReactorFleet chapter. The
book's own index confirms this — five scattered appearances across
concept-organized chapters, never a chapter *about* ReactorFleet. Its one
deep worked aggregate example throughout the book is `RootCauseCase`
(Ch. 16), not anything in ReactorFleet. What it does say about ReactorFleet:

- Strategic classification (Ch. 14, p. 39): "Plant operational model" is one
  of three Core Domain areas — "It gives the rest of the system a shared
  physical identity." Named concepts: `Unit, Reactor, Component, SystemNode`.
- The only aggregate candidate given (Ch. 16, p. 48–49) is `Unit`, with a
  single stated rule: *"A reactor belongs to one physical unit."* No class
  body, no method, no invariant-guard code — contrast with `RootCauseCase`,
  which gets a full class with `AddEvidence`/`CloseWithVerdict`.
- The only code ever given (Ch. 9/15, pp. 24, 45) is a bare identity class:
  `UnitId`, `Code`, `Name` — no children, no behavior.
- No ReactorFleet-owned domain event is named anywhere in the book's event
  catalogue (Ch. 17). No ReactorFleet-specific value object is named in the
  book's VO catalogue (Ch. 24) — `SignalTag`, `EngineeringValue`,
  `TimeWindow`, `ConfidenceScore`, `ReactivityNudge` all belong to
  Instrumentation/DigitalTwin/ReinforcementLearning.
- A grep of the entire book for the atlas's reactor-internals table names
  (`FuelAssembly`, `ControlRod`, `SteamGenerator`, `CoreRegion`,
  `Pressurizer`, `TurbineTrain`, `SwitchyardBay`, `GridConnection`) returns
  zero matches. The book never discusses core/rods/steam-generators/
  turbine/grid at all.
- Its own explicit caveat (Ch. 22, p. 72): *"A SQL schema maps well to a
  bounded context. It does not automatically map to an aggregate... A
  context may contain many aggregates, and an aggregate may use more than
  one table inside the same schema."*

**The gap this creates:** the source material gives no guidance on where
the aggregate boundary should sit among the atlas's 48 tables. It does not
say whether `Unit` should be a large aggregate root owning
`Reactor`/`ReactorCore`/`FuelAssembly`/`ControlRodBank` (the way
`RootCauseCase` owns its evidence/hypotheses), or whether those should be
separate small aggregates carrying a `UnitId` passport back to `Unit` (the
pattern the book demonstrates for other contexts, e.g. `AlarmEvent.UnitId`).
Per this project's standing rule against silently picking one, this ADR
decides it explicitly instead of inferring it from FK shape.

## Decision

**Model only the Phase 1 slice ReactorFleet actually needs to exist for,
per ADR-001-amend: "ReactorFleet produces simulated unit/reactor telemetry
that AlarmManagement consumes locally to detect floods."** That need is
satisfied by two small aggregates, not by attempting a boundary decision
for all 32 substantive tables the source material doesn't help with:

1. **`Unit`** — aggregate root, matching Domain_to_Twin's own bare-identity
   example exactly (`UnitId`, `Code`, `Name`), with basic non-empty
   validation on `Code`/`Name` (the general Domain_to_Twin
   invariant-enforcement style — throw on violation — applied since the
   book gives no ReactorFleet-specific invariant beyond the single sentence
   about `Reactor`, which this slice does not model — see Consequences).
2. **`UnitPowerSnapshot`** — a separate small aggregate root, matching the
   atlas's append-only `UnitPowerSnapshot` table (no update columns — a
   write-once telemetry record, not a mutable entity nested inside `Unit`).
   Carries a `UnitId` passport back to `Unit`, exactly the cross-aggregate
   reference pattern the book demonstrates elsewhere. Wraps the atlas's
   `PowerPercent DECIMAL(9,6) CHECK(BETWEEN 0 AND 200)` column as a
   `PowerPercent` value object (readonly record struct, validated range,
   matching Domain_to_Twin's demonstrated VO style from other contexts).
   Recording a snapshot raises a `UnitPowerRecorded` domain event — this is
   the seam AlarmManagement's in-process flood detector consumes.

**Everything else in the atlas's 48 tables is explicitly deferred**:
`Reactor`, `ReactorCore`, `CoreRegion`, `FuelAssembly`, `ControlRodBank`,
`ControlRod`, `RodPositionSnapshot`, `BoronChemistrySnapshot`,
`NeutronicsParameterSet`, `PrimaryLoop`, `ReactorCoolantPump`,
`Pressurizer`, `SteamGenerator`, `TurbineTrain`, `TurbineStage`,
`Generator`, `Transformer`, `SwitchyardBay`, `GridConnection`, `Fleet`,
`FleetPlant`, `FleetSnapshot`, `PlantSystem`, `SystemDependency`,
`EquipmentLocation`, `Equipment`, `EquipmentDependency`,
`EquipmentExternalReference`, `UnitDesignParameter`,
`UnitOperationalState`. None of these are needed for AlarmManagement's
flood-detection consumer. This is the same restraint principle CLAUDE.md §2
already applies at the 17-schema level ("do not start pulling in the other
12 schemas... until Phase 1 is proven end-to-end"), applied recursively
inside ReactorFleet's own schema.

**Shared-kernel inference required:** Domain_to_Twin's `AddDomainEvent(...)`
call implies base plumbing it never declares (no `Entity<TId>` shown in the
excerpted chapters). Per CLAUDE.md §1's standing instruction to infer the
minimum sensible C# shape when a source assumes something it doesn't show,
`Nexus1.BuildingBlocks.Domain` gets:
```csharp
public abstract class Entity<TId> where TId : notnull { /* Id, equality-by-Id, DomainEvents, AddDomainEvent/ClearDomainEvents */ }
public interface IAggregateRoot { }
```
`IAggregateRoot` is the same marker ADR-002-amend already named as a
`Nexus1.BuildingBlocks.Application`-side constraint on `IRepository<TRoot,TId>`
— this ADR is what actually creates it in the Domain shared kernel.

## Consequences

- Domain_to_Twin's one stated ReactorFleet invariant — "a reactor belongs to
  one physical unit" — is **not enforced by this slice**, because `Reactor`
  is not modeled. This is a deliberate, named gap, not a silent drop: it
  becomes real again the moment `Reactor` is modeled (see Reversal
  condition).
- `UnitPowerSnapshot.PowerPercent` allows up to 200 (matching the atlas's
  `CHECK(BETWEEN 0 AND 200)` exactly, including its stated overload
  allowance for demonstrator scenarios) rather than the more intuitive 0–100
  — kept exactly as the atlas defines it rather than "corrected," per this
  project's discipline against silently improving on the source.
- `Nexus1.ReactorFleet.Domain` will look sparse next to what the Schema
  Atlas implies is a large sector. This is intentional and should not be
  read as an oversight if revisited later without this ADR in hand.
- AlarmManagement's domain model (next in the build order) must not assume
  any ReactorFleet concept beyond `UnitId` and `UnitPowerRecorded` — no
  `Reactor`, `Equipment`, or physics detail exists yet to reference.

## Rejected alternatives

- **Model the full 48-table tree now, deciding an aggregate boundary for
  all of it.** Rejected: the source material gives no guidance for this
  decision (Domain_to_Twin never engages with 30 of the 32 substantive
  tables), so any boundary chosen now would be invented, not
  source-grounded — the exact "vibe architecture" this project's CLAUDE.md
  prohibits. It would also violate the Phase-1 restraint principle for work
  with no current consumer.
- **Nest `UnitPowerSnapshot` inside the `Unit` aggregate** (load/save
  together). Rejected: the atlas models it as an append-only table with no
  update columns — a write-once stream, not mutable child state — and
  nesting a high-volume telemetry stream inside an identity aggregate would
  make every `Unit` load pull unbounded snapshot history, a well-known DDD
  aggregate-sizing mistake the book's general small-aggregate style (seen
  in its `RootCauseCase`/evidence pattern) argues against by example.
- **Invent a ReactorFleet-owned domain event name from scratch** (the book
  names none). Rejected in favor of naming it descriptively from the
  atlas's own table name (`UnitPowerSnapshot` → `UnitPowerRecorded`) rather
  than guessing at book terminology that was never given.

## Reversal condition

Revisit when:
- `Reactor` needs modeling (e.g. RootCause's domain model needs to
  reference reactor-level state for causal reasoning) — at that point the
  deferred "a reactor belongs to one physical unit" invariant becomes real
  and needs enforcing, and the `Unit`-vs-`Reactor` aggregate boundary
  question (still undecided by the source material) must be answered with
  its own ADR, not inferred from this one.
- AlarmManagement's flood-detection logic turns out to need more than
  `UnitId` + `PowerPercent` + timestamp (e.g. per-equipment granularity) —
  that pulls in `Equipment`/`EquipmentLocation`, which is a new decision,
  not an extension of this one.

## Evidence required

- `Nexus1.ReactorFleet.UnitTests` passing: `Unit` construction/validation,
  `PowerPercent` boundary validation (0, 100, 200 valid; negative and >200
  invalid), `UnitPowerSnapshot.Record(...)` raising exactly one
  `UnitPowerRecorded` domain event with the correct payload.
- `Nexus1.ArchitectureTests` still passing after `Nexus1.BuildingBlocks.Domain`
  gains real content (the shared-kernel dependency-direction tests must
  keep holding once `Entity<TId>`/`IAggregateRoot` exist, not just once the
  project was empty).
