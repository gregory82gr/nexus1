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
  between ReactorFleet and AlarmManagement in Phase 1.
- No `Nexus1.Contracts.ReactorFleet` project is created yet — ReactorFleet
  has no external consumers, so a public contracts project would be an empty
  placeholder (the explicit anti-pattern this repo's instructions call out).
  Create it only if/when something outside the modular runtime host needs to
  consume ReactorFleet data directly.
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
- ReactorFleet needs to publish events consumed outside the modular runtime
  host (triggers creation of `Nexus1.Contracts.ReactorFleet`), or
- ReactorFleet's data/update-frequency profile makes in-process composition
  impractical (triggers a new ADR proposing extraction, evaluated against
  the book's own extraction gates), or
- The book's ADR-001 is itself revised in a later source-material update.

## Evidence required

- `Nexus1.ArchitectureTests` passing, confirming ReactorFleet's
  Domain/Application/Infrastructure respect the same inward-dependency law
  as AlarmManagement and RootCause, and that no `Contracts.ReactorFleet`
  reference exists anywhere in the solution while this ADR stands.
- At domain-model time (§5 step 2): a passing unit test demonstrating
  AlarmManagement consuming ReactorFleet telemetry in-process (direct
  method/interface call, not broker traffic).
