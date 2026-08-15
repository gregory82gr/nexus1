# ADR-005: RootCause Phase 1 domain slice — atlas naming over book naming

## Status

Accepted (per explicit user decision on the boundary conflict below).

## Context

RootCause is `From_Domain_to_Twin`'s one deep worked example — but reading
it directly, alongside the Schema Atlas, surfaced the most significant
boundary conflict of the three Phase-1 contexts, and the book is not even
internally consistent about its own example.

**Domain_to_Twin's `RootCauseCase`** is built up across three incompatible
passes that the book never reconciles into one final class:
- Ch. 16 (p. 50): a bare sketch — `int`/`string` fields, `AddEvidence`,
  `CloseWithVerdict(string verdict)`, invariants *"A verdict requires
  evidence"* / *"Closed cases cannot be changed."*
- Ch. 23 (p. 76–78): EF-shaped — adds `RootCauseCaseId`, `UnitId`,
  `RootCauseCaseStatus`, a public constructor, `RejectHypothesis`. Restates
  the close invariant as *"A root-cause case cannot close without
  evidence"* plus a second guard, *"At least one hypothesis must remain
  supported or accepted."*
- Ch. 27 / Expanded Ch. 6, 8, 10 (pp. 96, 114–124): adds `AddDomainEvent`
  calls, a strongly-typed `RootCauseCaseId`/`HypothesisId`, and a
  **different** construction style — a static factory
  `RootCauseCase.Open(flood.Id, candidates)` instead of the Ch. 23
  constructor. `HypothesisRejected` is shown with **three different field
  shapes** across these chapters (plain `int` vs. strongly-typed id,
  with/without `RejectedBy`, `RejectedAtUtc` vs. `OccurredAtUtc`). The
  book's own repository interface (`IRootCauseCaseRepository`) also has two
  incompatible shapes between Ch. 16 and Expanded Ch. 6.

The book explicitly owns this as intentional simplification, not an
oversight (Ch. 16, p. 51, its own "Honest boundary" note): *"This book uses
aggregate examples to teach design thinking. It does not claim there is
only one correct aggregate model for NEXUS-1."*

**The Schema Atlas has no `RootCauseCase`, `Evidence`, or `Hypothesis`
table at all.** Its real 44-table sector (Appendix C.10) roots at
`RootCauseAnalysis` (`BIGINT` PK, FK to `UnitId`, `CausalGraphVersionId`,
and — matching this project's chosen seam — a nullable `AlarmFloodId` FK to
`AlarmManagement.AlarmFlood`), then normalizes what the book flattens into
`Evidence`/`Hypothesis` into a much deeper pipeline: `AnalysisCandidate`
(scored: coverage/timing-fit/grounding/composite) → `AnalysisHypothesis`
→ `HypothesisEvidence`/`TelemetryWitness`/`RegistryWitness`/
`DocumentWitness`, plus `RejectedCandidate`, `AnalysisConclusion`, and an
**independently-versioned causal graph** (`CausalGraph`/
`CausalGraphVersion`/`CausalNode`/`CausalEdge`) that an analysis merely
*pins* via `CausalGraphVersionId` rather than owning — the atlas states
this decoupling explicitly (p. 216: *"The design deliberately separates
the engineered graph from the analysis result"*).

This is not a case of the book being silent (ADR-003) or the two sources
agreeing (ADR-004) — it is a genuine naming and structural disagreement,
and per this project's standing rule this was raised with the user rather
than resolved silently.

## Decision

Per explicit user choice: **use the atlas's naming for the aggregate and
its children, but keep Phase-1 behavior as minimal as the book's own
worked example** — right names now (avoiding a rename when EF Core mapping
lands in step 5's later work), without building out the atlas's full
scoring/witness/graph-versioning richness Phase 1 has no consumer for.

- **`RootCauseAnalysis`** (aggregate root) — `RootCauseAnalysisId` (long,
  matching the atlas's `BIGINT`), `UnitId`, `AlarmFloodId` (kept
  **non-nullable** for Phase 1, since `AlarmFloodDetected` is the only
  origin this project supports — the atlas's nullability accommodates
  `OperationalEventId`/`IncidentId` origins from `EventManagement`, out of
  scope), `AnalysisStatus` (`Open`/`Closed` only — the atlas's full
  `AnalysisStatus` lookup codes weren't enumerated by this session's
  research and aren't needed yet), `OpenedBy`/`ClosedBy` (plain `string`,
  matching the book's own `RootCauseCase.OpenedBy` field type exactly —
  Security isn't in scope for a strongly-typed actor id here any more than
  it was in ADR-004), `Verdict` (nullable until closed).
- **`AnalysisHypothesis`** (child entity, not its own aggregate — matches
  both sources agreeing that hypotheses live inside the case/analysis
  boundary) — `AnalysisHypothesisId` (int, atlas), `HypothesisStatement`,
  `HypothesisStatus` (`Proposed`/`Rejected` only — sufficient for the
  book's stated close invariant without inventing unconfirmed atlas lookup
  codes like a specific "Accepted"/"Supported" status name).
- **`HypothesisEvidence`** (child entity of `AnalysisHypothesis`) —
  `HypothesisEvidenceId` (int, atlas), `Description`, `RecordedAtUtc`. The
  atlas's typed `EvidenceType`/`WitnessType` and optional FKs to
  `AlarmEventId`/`SignalId`/`MeasurementId`/`EquipmentId`/`WorkOrderId`/
  `InspectionFindingId` are **deferred** — Phase 1 has no consumer that
  needs evidence provenance typed that precisely yet.

**Explicitly deferred** (per Option C, chosen by the user): `AnalysisCandidate`
scoring, `RejectedCandidate`, `TelemetryWitness`/`RegistryWitness`/
`DocumentWitness`, `CausalGraph`/`CausalGraphVersion`/`CausalNode`/
`CausalEdge` versioning, `AnalysisConclusion`,
`AnalysisModelRun`/`AnalysisModelOutput`, abstention modeling
(`AbstentionReason`), governance (`CausalGraphChangeRequest`/
`CausalGraphChangeReview`), and `AnalysisAuditTrail`/`AnalysisComment`/
`AnalysisTag`.

### Event renaming to match the aggregate name

The book's Ch. 17 event catalogue names `RootCauseCaseOpened` and
`RootCauseCaseClosed`. Since the aggregate itself is now named
`RootCauseAnalysis` (not `RootCauseCase`) per the decision above, its
lifecycle events are renamed to match: **`RootCauseAnalysisOpened`**,
**`RootCauseAnalysisClosed`** — keeping the event names consistent with
the aggregate that raises them, rather than carrying forward a name for an
aggregate that no longer exists in this codebase. `HypothesisRejected`
keeps its name (all three of the book's inconsistent versions agree on
this much) but uses this project's own reconciled field shape:
`(RootCauseAnalysisId AnalysisId, AnalysisHypothesisId HypothesisId,
string Reason, DateTime RejectedAtUtc)`.

### Invariant wording

Where the book's chapters disagree on exact guard wording, this ADR picks
one canonical set (closest to the earliest, Ch. 16/23 phrasing) rather than
treating any single chapter as more authoritative than another:
- *"A root-cause case cannot close without evidence."* — at least one
  hypothesis must have at least one piece of evidence.
- *"At least one hypothesis must remain supported or accepted."* —
  interpreted as: not every hypothesis may be `Rejected`.
- *"Only an open case can be closed."* (Ch. 25) — and, generalizing the
  book's broader stated principle *"Closed cases cannot be changed"*
  (Ch. 16) rather than gating only `Close` itself: **every** mutating
  method (`AddHypothesis`, `AddEvidence`, `RejectHypothesis`, `Close`)
  throws once the analysis is `Closed`.

### Domain purity maintained

`RootCauseAnalysis.AlarmFloodId` is `Nexus1.RootCause.Domain`'s own
passport type (a local `readonly record struct`), not a reference to
`Nexus1.AlarmManagement.Domain.AlarmFloodId`. Same pattern as ADR-004's
`UnitId`: no context's Domain project references another context's Domain
project, regardless of deployment topology (ADR-002's dependency law,
corrected into this project's actual practice by ADR-004).

## Consequences

- Any later step that needs the atlas's fuller candidate-scoring/witness/
  causal-graph structure starts from a real `RootCauseAnalysis` root
  already in place, not a `RootCauseCase` that needs renaming first.
- `RootCauseVerdictIssued.v1` (the external integration event, not yet
  built — that's Application/Host-layer work) will need its own translation
  from `RootCauseAnalysisClosed`, the same deferred-Contracts question
  ADR-004 already flagged for ReactorFleet→AlarmManagement.
- The atlas's richer evidence provenance (typed evidence/witness sources,
  candidate scoring, graph versioning) remains unmodeled technical debt,
  named explicitly here rather than silently absent.

## Rejected alternatives

(Presented to the user as options; not selected.)

- **Book's literal flat shape** (`RootCauseCase`/`Evidence`/`Hypothesis`,
  picking one of its three inconsistent passes). Rejected: would need
  renaming to the atlas's real table names the moment EF Core mapping
  starts, and the book itself doesn't offer one single canonical shape to
  be "literal" about.
- **Full atlas structure now** (candidate scoring, witnesses,
  `AnalysisConclusion`, causal-graph versioning). Rejected: no Phase-1
  consumer needs it yet; would repeat the ReactorFleet ADR-003 mistake of
  modeling depth with nothing to consume it.

## Reversal condition

Revisit when: `AnalysisCandidate` scoring is needed (e.g. RootCause needs
to rank multiple causes, not just accept/reject hypotheses one at a time);
the causal graph needs versioning/governance; evidence needs typed
provenance for `RootCauseVerdictIssued.v1` to cite specific alarms/signals
by FK rather than free text.

## Amendment (2026-08-15, §5 step 7): `RootCauseVerdictIssuedV1` gets an adapted, not frozen, payload

Same decision as ADR-004's `AlarmFloodDetectedV1` amendment, by direct
analogy — recorded here rather than duplicated at length. `From_Services_
To_Runtime` ch. 34 (Executable Asset 34-N) gives a frozen contract:

```csharp
public sealed record RootCauseVerdictIssuedV1(
    string VerdictId, string RootCauseCaseId, string SourceAlarmFloodMessageId,
    string SiteId, string LineId, string VerdictCode, decimal Confidence,
    string PolicyIdentity, string VerdictIdentity, int EvidenceRevision,
    int EvidenceCount, string EvidenceMembershipSha256,
    DateTimeOffset IssuedAtUtc, ProducerStreamPositionV1 ProducerStream);
```

Every field beyond identity, verdict text, and a timestamp traces back to
a concept this ADR's own Decision section already deferred:

| Book field | Requires | Why it's still out of scope |
|---|---|---|
| `SiteId`, `LineId` | Organization-schema Site/Line | Same reasoning as ADR-004 — never built in any context |
| `Confidence`, `PolicyIdentity`, `VerdictIdentity` | Scored, policy-tracked verdicts | This ADR deferred `AnalysisCandidate` scoring outright — `Close` takes a free-text `verdict` string, not a scored/policy-identified decision |
| `EvidenceRevision`, `EvidenceCount`, `EvidenceMembershipSha256` | Typed, hashed evidence provenance | Already named in this ADR's own original Reversal condition ("evidence needs typed provenance... to cite specific alarms/signals by FK rather than free text") — this amendment doesn't add a new gap, it's the same one, now also visible on the wire |
| `SourceAlarmFloodMessageId` | — | Not a missing domain concept, but redundant with the envelope's own `causationId` field (adopted book-exact) — carrying causation twice, once generically at the envelope level and once again as a domain-specific payload field, was rejected as duplication, not scope creep |
| `ProducerStreamPositionV1` | Event-sourcing stream position | Not part of this project's design at any layer |

**Decision:** adopt the book's transport/wire-level conventions exactly
(routing key `root-cause.root-cause-verdict-issued.v1`, the JSON+JCS
envelope, `eventType` `nexus1.root-cause.root-cause-verdict-issued.v1`).
The payload carries only what `RootCauseAnalysis` actually has:
`AnalysisId`, `UnitId`, `AlarmFloodId`, `Verdict` (free text, matching the
aggregate's own field, not a coded `VerdictCode`), `IssuedAtUtc` (from
`ClosedAtUtc`). Same rejection of placeholder data as ADR-004's amendment:
populating `Confidence`/`PolicyIdentity`/etc. with invented values would
violate CLAUDE.md's *"nothing claims to exist that does not."*

This contract is defined now but has **no publisher wired to it yet** —
Phase 1's end-to-end proof is AlarmManagement → RootCause only; nothing
consumes `RootCauseVerdictIssuedV1` (Audit/Compliance/Reporting don't
exist per CLAUDE.md §2's no-placeholder-projects rule), so there is
nothing to prove it flows to yet. It exists so the type is ready and
consistent with `AlarmFloodDetectedV1`'s treatment, not because it is
published in this step.

### Reversal condition (this amendment specifically)

Same as ADR-004's: revisit once `AnalysisCandidate` scoring, evidence
provenance, or a real policy/verdict-identity concept get built for some
other reason this project needs them — not to satisfy this contract in
isolation.

## Evidence required

- `Nexus1.RootCause.UnitTests` passing: `Open` succeeds and raises
  `RootCauseAnalysisOpened`; `AddHypothesis`/`AddEvidence` succeed while
  open and throw once closed; `RejectHypothesis` raises
  `HypothesisRejected`; `Close` throws with no evidence, throws with all
  hypotheses rejected, succeeds otherwise and raises
  `RootCauseAnalysisClosed`.
- `Nexus1.ArchitectureTests` still passing, confirming no
  `RootCause.Domain` → `AlarmManagement.Domain`/`ReactorFleet.Domain`
  reference was introduced.
