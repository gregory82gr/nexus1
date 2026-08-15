# Evidence: repository skeleton build (§8 steps 3–4)

Date: 2026-08-15
Command environment: local dev machine, .NET SDK 8.0.424 (pinned via `global.json`).

## Built

`dotnet build Nexus1.Runtime.sln` — all 22 projects (9 src classlibs across
BuildingBlocks/ReactorFleet/AlarmManagement/RootCause, 2 host worker
projects, 7 xUnit test projects, plus `Nexus1.ArchitectureTests`) compile
clean:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

`TreatWarningsAsErrors=true` is set solution-wide (`Directory.Build.props`),
so this is a warning-clean build, not just an error-free one.

## Tested

`dotnet test Nexus1.Runtime.sln`:

- `Nexus1.ArchitectureTests`: **6/6 passed** — the dependency-law tests
  (Domain/Application/Infrastructure/Contracts/SharedKernel reference
  rules from ADR-002, plus a classifier-completeness check) all pass
  against the real `.csproj` reference graph.
- The other 6 test projects (`Nexus1.ReactorFleet.UnitTests`,
  `Nexus1.AlarmManagement.UnitTests`, `Nexus1.RootCause.UnitTests`,
  `Nexus1.RootCause.ComponentTests`, `Nexus1.Contracts.ContractTests`,
  `Nexus1.DistributedSlice.EndToEndTests`) correctly report **"No test is
  available"** — honest, since no domain code exists yet to test. This is
  not the "green suite discovering zero tests" anti-pattern (that failure
  mode is a suite that *looks* green while silently finding nothing); this
  suite says explicitly that it found nothing, because there is nothing yet.

## Verified the architecture tests actually detect violations

Before trusting the 6/6 pass as meaningful, two violations were injected
and confirmed caught, then reverted:

1. `Nexus1.ReactorFleet.Domain` → `Nexus1.ReactorFleet.Infrastructure`
   (Domain referencing its own context's Infrastructure): this created a
   circular project-reference graph
   (Domain→Infrastructure→Application→Domain), which `dotnet restore`
   itself rejected with `MSB4006` before the architecture test even ran —
   a second, independent enforcement layer.
2. `Nexus1.AlarmManagement.Domain` → `Nexus1.Contracts.RootCause`
   (Domain referencing another context's Contracts, non-circular): this
   built successfully, and `Domain_projects_depend_only_on_the_domain_
   shared_kernel` failed with the exact violation named:
   `Nexus1.AlarmManagement.Domain -> Nexus1.Contracts.RootCause`.

Both injected references were removed; `git status`/`git diff` after
reverting showed no residual changes to the two affected `.csproj` files.

## Owned

This is pure scaffolding — no business logic, no data ownership yet. Every
project under `src/Contexts/*` is empty (zero `.cs` files) except the
project-reference graph itself; `Nexus1.ArchitectureTests` is the only test
project with real content.
