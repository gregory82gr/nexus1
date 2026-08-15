# NEXUS-1 — Companion Implementation (Claude Code Instructions)

## 0. What this repository is

NEXUS-1 is a **Phase-0 educational demonstrator**, not safety-class or operational software,
and not connected to any real facility. It is the reference implementation of a companion
book series about designing and building a .NET microservice architecture from first
principles: domain model → data backbone → EF Core mapping → modular monolith →
selective microservice extraction → distributed runtime.

We (the project owner + Claude, in Claude.ai) are the architects. **You (Claude Code) are the
implementer.** Your job is to turn the specifications in `/docs/source-material/` into a real,
buildable, testable .NET solution — not to re-derive the architecture from scratch, and not to
silently deviate from it. When the source material is ambiguous or contradicts itself, stop and
raise it rather than guessing.

**Standing rule inherited from the source material, and binding on this repo:**

> Nothing claims to exist that does not. A diagram, a "done" checkbox, or a status message
> is valid only when it matches the repository, the contracts, the data ownership, the tests,
> and the observable runtime behavior.

Concretely: never mark something done, tested, or working unless you actually ran it and
saw it pass. If you can't run `dotnet build`, `dotnet test`, or the containers in this
environment, say so explicitly instead of describing success.

---

## 1. Source material and its authority

All source PDFs live in the project and should be treated as the specification, in this order
of authority for implementation questions:

| Priority | Source | What it governs |
|---|---|---|
| 1 | **`From_Services_To_Runtime` (Parts 1–4)** | The actual implementation blueprint: solution topology, service templates, databases, contracts, messaging, security, observability, migration. This is the primary spec for everything you build. |
| 2 | **`From_Flow_to_Services`** | Why boundaries were drawn where they were (service candidate map, ownership, contracts, failure modes). Use this to understand *why*, not to override Part 1's *how*. |
| 3 | **`From_Schema_to_System` (Schema Atlas)** | Canonical SQL Server DDL for all 17 sector schemas — table names, keys, constraints. Source of truth for anything persisted. |
| 4 | **`From_Entity_to_Context` (EF Core Configuration Atlas)** | Canonical EF Core mapping discipline: one entity → one `IEntityTypeConfiguration` file, Fluent API only, explicit names, schema folders. |
| 5 | `ProjectDescriptionOfNexus.txt` | High-level intent (.NET, C#, EF Core, SQL Server). |

**Known gap (as originally written) — SUPERSEDED, see session amendment below:** two books
referenced heavily inside the above (`From Blueprint to Core` — Clean/Onion + CQRS shape,
and `From Domain to Twin` — the base DDD domain model) were believed **not** to be in this
project. Where `From_Services_To_Runtime` assumes concepts from those books (e.g. exact
aggregate shapes, exact CQRS command/query interfaces), the fallback was to infer the minimum
sensible C# shape consistent with what *is* provided.

**Session amendment (2026-08-15):** both `From_Blueprint_to_Core.pdf` and
`From_Domain_to_Twin_DDD_NEXUS1_FINAL.pdf` were found present in the source-material folder
the user provided. Per user decision, both are now treated as **real, authoritative sources**
for CQRS/Clean-Onion shape and DDD domain-model shape respectively — read them directly
instead of inferring. The "known gap" language above is kept for history but no longer
describes this repo's actual source set. `ProjectDescriptionOfNexus.txt` (priority 5) was not
found in the original provided folder; it was supplied separately from
`C:\Users\USER\Desktop\ProjectDescriptionOfNexus.txt` and copied into
`docs/source-material/`. Eleven other books present in the original folder
(`From_Certainty_to_Calibration`, `From_Context_to_Flow`, `From_Core_to_Contract`,
`From_Flood_to_Cause`, `From_Flow_to_Proof` + its TLA/Petri-net chapter artifacts, three
`From_Grid_to_Core` versions, `From_Queue_to_Core`, `From_Table_to_Twin`,
`From_Trial_to_Policy`) were deliberately **left out** of `docs/source-material/` per user
decision — out of the stated 5-source authority list and out of Phase-1 scope. Revisit only if
a named, in-scope source explicitly references one of them.

If you are ever about to implement something and can't find it in the source material, **stop
and ask** rather than inventing architecture. This project is explicitly against "vibe
architecture" — every boundary should trace back to a decision in the books or an ADR you
wrote because the books didn't cover it.

---

## 2. Phase 1 scope — the first distributed slice

The blueprint's own ADR-001 (`From_Services_To_Runtime`, Ch. 3) selects **AlarmManagement
→ RootCause** as the first distributed slice, with **Audit, Compliance, and Reporting** as
independent subscribers to `RootCauseVerdictIssued.v1`. Everything else (DigitalTwin,
Instrumentation, Robotics, RadiationMonitoring, EmergencyPreparedness, ReinforcementLearning,
and — per the book — ReactorFleet) is explicitly left in the **protected modular core** until it
earns its own extraction.

**Amendment for this project:** we are including **ReactorFleet** in Phase 1 as the origin of
alarm data, which the book's ADR-001 does not do. To avoid fighting the book's proven
pattern, the default approach is:

- **ReactorFleet** is implemented as a bounded context (`Nexus1.ReactorFleet.*`) living inside
  the **modular runtime host**, producing simulated unit/reactor telemetry that
  **AlarmManagement** consumes locally (in-process) to detect floods. It is *not* a separately
  deployed/remote service in Phase 1.
- **AlarmManagement** also stays in the modular runtime host initially (per the book), and
  publishes `AlarmFloodDetected.v1` to the broker.
- **RootCause** is the one service extracted to its own independently deployed host in Phase 1
  (per the book), consuming `AlarmFloodDetected.v1` and publishing
  `RootCauseVerdictIssued.v1`.
- **Audit, Compliance, Reporting** get their own projects only when their first owned behavior
  is actually implemented (no empty placeholder projects — this is an explicit anti-pattern
  in the book).

Write this as **ADR-001-amend-reactorfleet** in `docs/adr/` before writing code, so it's a
recorded decision rather than an implicit deviation. If, once you're deeper into the schema
atlas / EF configuration atlas for ReactorFleet, this shape looks wrong (e.g. ReactorFleet's
data volume or update frequency makes in-process composition impractical even for a
demonstrator), raise it — don't silently promote it to a remote service without a new ADR.

**Do not** start pulling in the other 12 schemas (Robotics, RadiationMonitoring,
EmergencyPreparedness, ReinforcementLearning, DigitalTwin, Instrumentation, Maintenance,
EventManagement, Organization, Security beyond what auth requires, CorePlatform beyond
what lookups Phase 1 needs) until Phase 1 is proven end-to-end. Bringing in the full
17-schema atlas up front is explicitly the anti-pattern this whole book series argues against.

---

## 3. Technology stack

- **.NET 8**, C#, pinned via `global.json` (see Ch. 4 template below).
- **EF Core** with **SQL Server**, Code First, Fluent API only (no data annotations for
  persistence concerns) — per `From_Entity_to_Context` discipline.
- **RabbitMQ** for the message backbone (outbox/inbox, publisher confirms, DLQ) — introduced
  only once the solution topology and the AlarmManagement→RootCause contract compile and
  are tested locally (see build order in §5). Don't wire up the broker before the databases and
  contracts exist and are tested.
- **xUnit** for tests, organized to mirror context ownership (architecture tests, unit tests per
  context, component tests, contract tests, end-to-end tests) — not organized by executable.
- **Central package management** via `Directory.Packages.props`; `Directory.Build.props` for
  shared build settings; `.editorconfig` for style.
- Docker / Docker Compose for local reproducible runtime (introduced later, per book Ch. 55 —
  not needed for the first compile gate).

---

## 4. Repository / solution topology

This is the reference tree from `From_Services_To_Runtime` Ch. 4 (ADR-002: context-first
solution topology), adapted to include ReactorFleet per §2:

```
Nexus1.Runtime.sln
Directory.Build.props
Directory.Packages.props
global.json
.editorconfig
src/
  BuildingBlocks/
    Nexus1.BuildingBlocks.Domain/        # shared kernel primitives only — grows by
                                          # duplication-until-proven, not by convenience
    Nexus1.ServiceDefaults/              # shared host composition (health checks, telemetry wiring)
  Contexts/
    ReactorFleet/
      Nexus1.ReactorFleet.Domain/
      Nexus1.ReactorFleet.Application/
      Nexus1.ReactorFleet.Infrastructure/
      Nexus1.Contracts.ReactorFleet/     # only if/when ReactorFleet needs to publish outward
    AlarmManagement/
      Nexus1.AlarmManagement.Domain/
      Nexus1.AlarmManagement.Application/
      Nexus1.AlarmManagement.Infrastructure/
      Nexus1.Contracts.AlarmManagement/  # public integration events (AlarmFloodDetected.v1)
    RootCause/
      Nexus1.RootCause.Domain/
      Nexus1.RootCause.Application/
      Nexus1.RootCause.Infrastructure/
      Nexus1.Contracts.RootCause/        # public integration events (RootCauseVerdictIssued.v1)
  Hosts/
    Nexus1.ModularRuntime/               # composes ReactorFleet + AlarmManagement
    Nexus1.RootCause.Host/               # independently deployed
tests/
  Nexus1.ArchitectureTests/              # enforces the dependency rules below — build-breaking
  Nexus1.ReactorFleet.UnitTests/
  Nexus1.AlarmManagement.UnitTests/
  Nexus1.RootCause.UnitTests/
  Nexus1.RootCause.ComponentTests/
  Nexus1.Contracts.ContractTests/
  Nexus1.DistributedSlice.EndToEndTests/
docs/
  source-material/                       # the PDFs / book excerpts, for reference
  adr/                                   # architecture decision records — see §6
  ledgers/
  runbooks/
artifacts/evidence/                      # build/test/coverage evidence — see §7
```

**Dependency law (enforced by `Nexus1.ArchitectureTests`, must fail the build if violated):**

- Dependencies point inward within a context: Infrastructure → Application → Domain.
- Cross-context references are **only** allowed to another context's `Nexus1.Contracts.*`
  project — never to its `Domain`, `Application`, or `Infrastructure` projects.
- `Domain` never references transport, persistence, Host, or any `Contracts` project (domain
  events ≠ integration events — keep that distinction real in code, not just in prose).
- Host projects are composition roots only — no business decisions live there.

**Anti-patterns to actively avoid** (explicit in the book, worth repeating so they don't creep
back in during a long session):

- A generic `Shared.Contracts` project holding every DTO.
- Folders named `API/Worker/Database` as the top-level organizing principle.
- Empty placeholder projects for Audit/Compliance/Reporting/RL before they have real behavior.
- Domain referencing Infrastructure "just for convenience."
- A green test suite that's actually discovering zero tests.

---

## 5. Build order (do not reorder or parallelize away the gates)

This is the incremental sequence from `From_Services_To_Runtime` Ch. 1–3, adapted for the
Phase 1 scope in §2. Each step should be a working, compiling, reviewable state before moving
to the next — treat each as a checkpoint you report back on, not a batch to blast through
silently:

1. **Repository skeleton** — `global.json`, `Directory.Build.props`, `Directory.Packages.props`,
   `.editorconfig`, empty context/host/test projects per §4, `Nexus1.ArchitectureTests` with the
   dependency-law tests in place and passing (or explicitly xfail with a reason) before any
   business code exists. This is the first compile gate.
2. **Domain models** — ReactorFleet, AlarmManagement, RootCause domain layers: entities,
   value objects, aggregates, domain events, invariants — built from the Schema Atlas
   (`From_Schema_to_System`) for shape and the architecture books for behavior. Unit-tested
   with no persistence, no broker, no host.
3. **EF Core configuration + local persistence** — one `IEntityTypeConfiguration` per entity,
   schema folders matching the atlas, `AlarmManagementDb` and `RootCauseDb` as **separate**
   databases per the book's data-ownership rule (ReactorFleet persistence can share the
   modular runtime's database initially, or get its own — decide via ADR when you get there).
   Migrations reviewed for readable names, no shadow columns, correct FK/index names.
4. **Contracts** — versioned `AlarmFloodDetected.v1` and `RootCauseVerdictIssued.v1` as public
   integration event types in the relevant `Nexus1.Contracts.*` projects. Contract tests before
   any transport exists.
5. **Application layer (CQRS)** — commands/queries/handlers per context, still no broker: prove
   the use cases against the local database.
6. **Hosts** — `Nexus1.ModularRuntime` composing ReactorFleet + AlarmManagement;
   `Nexus1.RootCause.Host` standing alone. Health checks (liveness/readiness) per book Ch. 11.
7. **Messaging backbone** — RabbitMQ topology, transactional outbox in AlarmManagement,
   inbox/idempotent consumption in RootCause, retry/backoff/DLQ. Only after steps 1–6 are
   solid and tested.
8. **Fan-out subscribers** — Audit, Compliance, Reporting each get a project only once their
   first real subscriber behavior is being implemented, one at a time, each independently
   tested.
9. **End-to-end slice tests + failure experiments** — duplicate delivery, delay, outage,
   restart, poison message, replay, projection lag — per book Ch. 36.

Do not silently skip a step's tests "to save time" — the whole point of this source material is
that untested claims of completeness are the failure mode being designed against.

---

## 6. Architecture Decision Records (ADRs)

Every deviation from, or gap-filling addition to, the source material gets a short ADR in
`docs/adr/`, numbered sequentially, following the format already used in the books
(Title / Status / Context / Decision / Consequences / Rejected alternatives / Reversal
condition / Evidence required). Start with:

- `ADR-001-amend-reactorfleet.md` — recording the §2 amendment before writing ReactorFleet
  code.
- Any ADR needed to fill the `From Blueprint to Core` / `From Domain to Twin` gap noted in §1 —
  superseded by the session amendment in §1: since both books are actually present and in
  use, this becomes an ADR recording that they are being read directly rather than a gap-fill.

Don't skip this step even under time pressure — it's the mechanism that keeps this
implementation honest and revisable instead of accumulating undocumented drift.

---

## 7. Definition of done / evidence discipline

Borrowing directly from the book's own standard (Ch. 6, Appendix A/H): a piece of work is
"done" only when there's evidence for it, not when code exists that plausibly does it. For each
component, be explicit about:

- **Built** — it compiles in the full solution, not just in isolation.
- **Tested** — the relevant test tier (unit / component / contract / end-to-end) actually ran and
  passed; report the real test output, don't summarize confidently without running it.
- **Owned** — which context/service owns this data, decision, or contract — no shared/ambiguous
  ownership.

When something can't be verified in this environment (e.g. no live SQL Server or RabbitMQ
available), say so plainly rather than describing it as working. Use `artifacts/evidence/` to
record what was actually run and what its output was, mirroring the book's own evidence
ledger habit.

---

## 8. First task for Claude Code

1. Read `docs/source-material/` (this project's PDFs, once copied in) and this file.
2. Write `docs/adr/ADR-001-amend-reactorfleet.md` per §2.
3. Scaffold the repository skeleton per §4 (step 1 of §5) — solution file, project references,
   `Nexus1.ArchitectureTests` enforcing the dependency law, everything else still empty.
4. Get that skeleton building cleanly (`dotnet build`) and report the actual output.
5. Stop and report back before starting step 2 of §5 (domain models) — this is a good checkpoint
   for us to confirm the topology looks right before you build on top of it.

If anything in this file conflicts with what you find in the actual PDFs when you read them
directly, the PDFs win — flag the conflict and we'll fix this file.
