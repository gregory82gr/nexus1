# Evidence: ReactorFleet, AlarmManagement, RootCause domain models (§5 step 2)

Date: 2026-08-15
Command environment: local dev machine, .NET SDK 8.0.424 (pinned via `global.json`).

## Built

`dotnet build Nexus1.Runtime.sln` — all 22 projects compile clean with the
three contexts' Domain projects now populated (previously empty):

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Tested

`dotnet test Nexus1.Runtime.sln`:

| Project | Result |
|---|---|
| `Nexus1.ReactorFleet.UnitTests` | **12/12 passed** |
| `Nexus1.AlarmManagement.UnitTests` | **16/16 passed** |
| `Nexus1.RootCause.UnitTests` | **9/9 passed** |
| `Nexus1.ArchitectureTests` | **6/6 passed** (dependency law still holds with real Domain content in all three contexts, not just BuildingBlocks) |
| `Nexus1.Contracts.ContractTests` | No tests available — honest, no Contracts content built yet |
| `Nexus1.RootCause.ComponentTests` | No tests available — honest, no persistence/host yet |
| `Nexus1.DistributedSlice.EndToEndTests` | No tests available — honest, no broker/hosts wired yet |

37 domain-layer tests + 6 architecture tests = 43 passing, 0 failing, 0 skipped.

## Owned

Three ADRs record every source-material gap or conflict found while
building these models, all resolved before code was written (not
discovered as build breaks):

- **ADR-003** (ReactorFleet): Schema Atlas describes 48 tables; Domain_to_Twin
  models only a bare `Unit` class. Modeled the Phase-1 slice only
  (`Unit`, `UnitPowerSnapshot`), the rest deferred and named explicitly.
- **ADR-004** (AlarmManagement): no real boundary conflict this time; but
  writing `AlarmDefinition.Evaluate` surfaced a correction to
  ADR-001-amend — cross-context Domain purity applies regardless of
  same-host deployment, so ReactorFleet→AlarmManagement wiring is deferred
  to Application/Host layer, not resolved here. Flood-detection threshold
  is a required parameter, not an invented default.
- **ADR-005** (RootCause): the significant one — Domain_to_Twin's own
  worked example is internally inconsistent across three chapters, and none
  of its naming (`RootCauseCase`/`Evidence`/`Hypothesis`) matches the Schema
  Atlas's real `RootCauseAnalysis`/`AnalysisHypothesis`/`HypothesisEvidence`
  structure. Raised to the user rather than resolved silently; user chose
  atlas naming with Phase-1-minimal behavior.

No context's Domain project references another context's Domain project —
each defines its own local passport ID types (`UnitId`, `AlarmFloodId`)
rather than sharing types across the boundary. Verified by
`Nexus1.ArchitectureTests`, not just asserted.
