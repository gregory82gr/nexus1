# Evidence: Application layer for ReactorFleet, AlarmManagement, RootCause (§5 step 5)

Date: 2026-08-15
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`.

## Built

`dotnet build Nexus1.Runtime.sln` — all 28 projects (25 from the EF Core
step, plus `Nexus1.ReactorFleet.ComponentTests` and
`Nexus1.AlarmManagement.ComponentTests`, new this step) compile clean:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Tested

`dotnet test Nexus1.Runtime.sln` — real output, not summarized:

```
Nexus1.ReactorFleet.UnitTests           12/12 passed
Nexus1.AlarmManagement.UnitTests        16/16 passed
Nexus1.RootCause.UnitTests               9/9  passed
Nexus1.ReactorFleet.ComponentTests       3/3  passed   (real LocalDB)
Nexus1.AlarmManagement.ComponentTests   12/12 passed   (real LocalDB)
Nexus1.RootCause.ComponentTests          6/6  passed   (real LocalDB)
Nexus1.ArchitectureTests                 7/7  passed
Nexus1.Contracts.ContractTests           no tests available (honest — no content yet)
Nexus1.DistributedSlice.EndToEndTests    no tests available (honest — no broker/hosts yet)
```

**65 tests passing, 0 failing**, up from 44 before this step (+21: the 6
domain-layer additions are 0 — no domain code changed — the +21 is entirely
the three ComponentTests projects' handler tests: 3 + 12 + 6).

## What the ComponentTests actually prove

Every command/query handler across all three contexts was run against a
real, migrated LocalDB database (fresh per test, dropped after), not
against mocks or an in-memory provider:

- **ReactorFleet**: `RecordUnitPowerSnapshotCommand` — success path read
  back through an independent `DbContext`; unknown-unit and out-of-range
  `PowerPercent` failure paths verified to write nothing.
- **AlarmManagement**: `DefineAlarm`, `EvaluateReading`, `AcknowledgeAlarm`,
  `DetectFlood`, and `GetActiveAlarmsForUnit` — including the seam this
  whole ADR chain (ADR-001-amend's correction, ADR-004) was building
  toward: `EvaluateReadingCommand` takes
  `Nexus1.Contracts.ReactorFleet.UnitPowerSnapshotRecordedV1` directly and
  was exercised end-to-end against a real database, not just type-checked.
- **RootCause**: the full `Open → AddHypothesis → AddEvidence →
  RejectHypothesis → Close` workflow across independent `DbContext`
  instances per step — proving `RootCauseAnalysisRepository`'s explicit
  `Include(...).ThenInclude(...)` really reloads the full aggregate graph
  on every handler call, not just within one `DbContext`'s lifetime (a bare
  `FindAsync`, used correctly for ReactorFleet/AlarmManagement's flatter
  aggregates, would have silently handed back an analysis with empty
  `Hypotheses`/`Evidence` here — caught by design, not by a failing test).
  Both close-invariant failure paths (`no evidence`, `all hypotheses
  rejected`) and the closed-aggregate mutation guard were verified against
  real persisted state.

## Architecture tests

`Nexus1.ArchitectureTests` stayed at 7/7 through every change in this step,
including the moment `Nexus1.AlarmManagement.Application` gained a **real**
`ProjectReference` to `Nexus1.Contracts.ReactorFleet` (previously only
theoretical — the reference didn't exist until `EvaluateReadingCommand` was
written). The dedicated cross-context test written in the prior session
(before that reference existed) correctly allowed it while the generic
Application-layer rule kept forbidding direct `ReactorFleet.Domain`/
`.Application`/`.Infrastructure` references — confirmed by running, not
just re-reading the test source.

## Owned

- `Nexus1.BuildingBlocks.Application`'s CQRS interfaces (`ICommand`,
  `IQuery`, handlers, `Result`/`Result<T>`, `IRepository<TRoot,TId>`,
  `IUnitOfWork`, `IDateTimeProvider`) are implemented per ADR-002-amend's
  planned shape, with hand-rolled direct dispatch (no MediatR) per that
  ADR's now-recorded decision — no dispatcher/mediator layer exists yet;
  handlers are constructed and invoked directly in tests, matching what a
  future Host's DI composition root will do.
- `IIdGenerator`/`SequentialIdGenerator` — a Phase-1, single-process,
  per-process-monotonic id strategy, explicitly not safe across multiple
  host instances. Revisit before any multi-instance deployment.
- Not verified in this environment: DI container wiring (no Host
  composition root exists yet — §5 step 6), concurrent-write behavior,
  connection resilience/retry policies.
