# ADR-001-amend: Include ReactorFleet in the Phase 1 distributed slice

## Status

Accepted (project-level amendment to the book's ADR-001).

## Context

`From_Services_To_Runtime`'s own ADR-001 (Part 1, Ch. 3, pp. 54–56) selects
**AlarmManagement → RootCause** as the first distributed slice, with Audit,
Compliance, and Reporting as independent subscribers to
`RootCauseVerdictIssued.v1`. The book is explicit that this seam was chosen
because "RootCause is the strongest decision-rich candidate, but an isolated
extraction would not test durable asynchronous ownership. The first slice
must be reversible and outside safety-like control."

Read carefully, ReactorFleet was never a head-to-head contender for this
seam. The ADR-001 table's own rejected-alternatives list is: *"RootCause-only
synchronous extraction, Instrumentation → DigitalTwin first, Reporting-only as
the main slice, RL Advisory as the primary flow, and a transaction spanning
RootCause plus all subscribers."* ReactorFleet does not appear in that list.

Instead, ReactorFleet is named twice elsewhere in the same chapter as part of
a **protected, not-yet-extractable set**:

- Preface (p. 9): the prior book classified RootCause/AlarmManagement/
  Compliance/Reporting/Audit as "the strongest candidates," while "keeping
  DigitalTwin, Instrumentation, ReactorFleet, and ReinforcementLearning
  Advisory conditional."
- Chapter 3 closing, "What is deliberately not in the first slice" (p. 63),
  boxed as **PROTECTED FOUNDATION**: *"Instrumentation, DigitalTwin,
  ReactorFleet, simulation internals, shared platform concerns, and
  safety-like control remain modular until their own language, data,
  failure, and reversal gates are satisfied."*

So this amendment is not overturning a rejected candidate — it is **lifting
a stated protection** on ReactorFleet specifically, for this project only,
because we need a concrete origin for alarm data and are not willing to
hand-wave it as an untested stub. AlarmManagement needs *something* to
produce the telemetry it floods-detects on; the book's own Phase 1 doesn't
model that origin at all.

### Correction (2026-08-15, per ADR-004)

This ADR originally decided **no `Nexus1.Contracts.ReactorFleet` project is
needed**, reasoning that ReactorFleet has no external consumers since
AlarmManagement consumes its telemetry "in-process." That reasoning was
wrong and is corrected here rather than left to mislead a future reader.

ADR-002's dependency law (CLAUDE.md §4) is a **compile-time ownership**
rule: *"Cross-context code references only producer-owned Contracts"* —
it makes no exception for same-host or in-process composition, and
`Nexus1.ArchitectureTests` enforces it structurally (parsing `.csproj`
`ProjectReference` graphs), not by inspecting runtime transport. Whether
`AlarmManagement.Application` calls into ReactorFleet over a broker, an
HTTP client, or a plain in-process method call, it is still a cross-context
reference the moment it takes a compile-time dependency on anything outside
`Nexus1.Contracts.ReactorFleet`. "In-process" describes how data moves at
runtime; it says nothing about which project is allowed to reference which
at compile time. This was first caught while building AlarmManagement's
domain model (ADR-004) and is fixed here at the source.

**ReactorFleet publishes through `Nexus1.Contracts.ReactorFleet`, exactly
like AlarmManagement (`Nexus1.Contracts.AlarmManagement`) and RootCause
(`Nexus1.Contracts.RootCause`) already do — regardless of hosting.** This
is not a new exception to the "no empty placeholder projects" anti-pattern
this ADR originally invoked to justify skipping it: ReactorFleet has a real
consumer today (AlarmManagement's future flood-detection wiring), so the
Contracts project is not a placeholder, it was simply never created when it
should have been.

## Decision

ReactorFleet is included in this project's Phase 1, but **only as an
in-process bounded context inside the modular runtime host** — not as a
separately deployed/remote service, and not by satisfying ReactorFleet's own
future extraction gates (that would require the "language, data, failure,
and reversal gates" the book reserves for a real extraction decision, which
is out of scope here).

Concretely:

- `Nexus1.ReactorFleet.{Domain,Application,Infrastructure}` live under
  `src/Contexts/ReactorFleet/`, composed only into `Nexus1.ModularRuntime`.
- ReactorFleet produces simulated unit/reactor telemetry consumed
  **in-process** by AlarmManagement to detect floods. No broker traffic
  between ReactorFleet and AlarmManagement in Phase 1 — but the reference
  is still made through `Nexus1.Contracts.ReactorFleet` (see Correction
  above), not directly against `Nexus1.ReactorFleet.Domain`/`.Application`.
  "In-process" governs transport (both contexts run in one host, one
  process, no broker), not which project may compile against which.
- `Nexus1.Contracts.ReactorFleet` is created alongside
  `Nexus1.Contracts.AlarmManagement` and `Nexus1.Contracts.RootCause`,
  exposing the minimal DTO shape ReactorFleet's Phase-1-minimal domain model
  (ADR-003) already has to publish outward: `UnitPowerSnapshotRecordedV1`,
  carrying `UnitPowerSnapshot`'s data.

  **Correction (2026-08-15, before Application-layer work):** this bullet
  originally argued the contract needed no version suffix since it's never
  broker-published in Phase 1, only referenced in-process. That reasoning
  was itself corrected before anything came to depend on the unversioned
  name: it is still a *public contract crossing a context boundary* — that
  is the entire reason it lives in a `Contracts.*` project instead of
  `ReactorFleet.Domain` — and every other public contract in this repo is
  versioned regardless of transport. Renamed to `UnitPowerSnapshotRecordedV1`
  (a `V1` suffix on the type name, since C# identifiers can't hold a
  literal dot) for consistency with `AlarmFloodDetected.v1`/
  `RootCauseVerdictIssued.v1` — which, note, aren't built as C# types yet
  either (deferred to broker wiring, §5 step 7); when they are, they should
  follow this same `EventNameV1` convention.
- AlarmManagement remains in the modular runtime host per the book, and
  still publishes `AlarmFloodDetected.v1` externally, unchanged from ADR-001.
- RootCause remains the one service independently extracted to its own host
  in Phase 1, unchanged from ADR-001.
- Audit, Compliance, Reporting still get projects only when their first
  owned subscriber behavior is actually implemented — this amendment does
  not touch that rule.

## Consequences

- ReactorFleet's persistence question (own database vs. sharing the modular
  runtime's database) is deferred to a later ADR, at the point in the build
  order (§5 step 3, EF Core configuration) where it must actually be
  decided.
- If ReactorFleet's simulated data volume or update frequency later makes
  in-process composition impractical even for a demonstrator, that is a
  **new ADR**, not a silent promotion to a remote service — per this
  project's standing instruction not to let architecture drift
  undocumented.
- This amendment is local to this project's Phase 1 scope. It does not
  claim ReactorFleet has earned extraction-worthiness by the book's own
  criteria; the protection the book placed on it is lifted only for the
  narrow purpose of giving AlarmManagement a real telemetry origin inside
  the modular core, not removed globally.

## Rejected alternatives

- **Treat AlarmManagement's telemetry input as an untyped stub / fake data
  generator with no bounded context of its own.** Rejected: this would
  violate the project's DDD discipline for the one context whose data is
  most schema-heavy in the Schema Atlas, and would leave "where does alarm
  data come from" unanswered in the domain model.
- **Extract ReactorFleet as its own independently deployed service in Phase
  1**, matching RootCause's treatment. Rejected: this fights the book's own
  ADR-001 pattern (only one seam is proven distributed per slice) and
  reproduces exactly the risk the PROTECTED FOUNDATION box is guarding
  against — extracting a context before its own gates are satisfied.

## Reversal condition

Revisit this ADR if:
- `Nexus1.Contracts.ReactorFleet` needs a second public type or its
  existing `UnitPowerSnapshotRecordedV1` shape needs to change — that's an
  ordinary contract change, not a re-litigation of this ADR.
- ReactorFleet's data/update-frequency profile makes in-process composition
  impractical (triggers a new ADR proposing extraction, evaluated against
  the book's own extraction gates), or
- The book's ADR-001 is itself revised in a later source-material update.

## Evidence required

- `Nexus1.ArchitectureTests` passing, confirming ReactorFleet's
  Domain/Application/Infrastructure respect the same inward-dependency law
  as AlarmManagement and RootCause, **and** confirming
  `Nexus1.AlarmManagement.*` never references `Nexus1.ReactorFleet.Domain`
  or `Nexus1.ReactorFleet.Application` directly — only
  `Nexus1.Contracts.ReactorFleet` — the same shape as the existing
  AlarmManagement→RootCause cross-context check.
- At domain-model time (§5 step 2, done): a passing unit test demonstrating
  AlarmManagement consuming ReactorFleet telemetry in-process (direct
  method/interface call, not broker traffic) — still pending until
  Application-layer wiring (§5 step 5); the Contracts project existing now
  is what makes that wiring legal when it's built.
