# ADR-012: Reporting fan-out subscriber — RootCauseCaseOpenedV1, ReportingDb ownership, and the reduced projection

## Status

Accepted.

## Context

Reporting is the third and last of §5 step 8's fan-out subscribers. Ch.35
("Reporting Builds Delayed Truth", pp. 850-863+) makes clear before any code
gets written that this one does not collapse into Audit's or Compliance's
shape (both ch.34, ADR-010/ADR-011) — it is a genuine event-sourced
**projection** consuming two independent event types into one read model,
with an entire apparatus for out-of-order delivery: an immutable event
ledger separate from the projection row, a contiguous producer-stream
watermark, a durable gap buffer, a separate operational status/health
sampler, and row-versioned optimistic concurrency (Executable Assets 35-D
through 35-M).

**Prerequisite, built the same way RootCause's missing outbox was built for
Audit: `RootCauseCaseOpenedV1` did not exist.** Reporting cannot build a
case-lifecycle projection without a "case opened" fact to seed each row —
this was flagged twice already (ADR-010, ADR-011) as a known gap for when
Reporting's turn came.

**The genuine structural fork, confirmed by reading ch.35 directly rather
than assumed**: the book's entire ordering apparatus is keyed on a producer
stream position (`StreamId`/`Position`) — a concept `ProducerStreamPositionV1`
that ADR-004 and ADR-005 already explicitly declined to add to this
project's contracts ("traces back to an already-deferred domain concept").
Building the book's full gap-buffering design would mean reopening those
closed decisions and changing contracts Audit and Compliance already
consume as frozen — a materially larger and different cost than any prior
deferral in this project. This was raised to the user explicitly (not
picked silently); the decision below is the user's own choice.

## Decision

### RootCauseCaseOpenedV1 — reduced, published from *two* call sites

`Nexus1.Contracts.RootCause.RootCauseCaseOpenedV1(long AnalysisId, int UnitId,
long AlarmFloodId, DateTime OpenedAtUtc)` — same reduction pattern ADR-005
already applied to `RootCauseVerdictIssuedV1`: no `SiteId`/`LineId`/
`InitialEvidenceCount`, because this domain model has none of those and
`RootCauseAnalysis.Open()` takes no evidence.

Published from **both** places an analysis can be opened, not just the one
the initial instruction named:

- `OpenAnalysisCommandHandler` (as explicitly instructed) — the manual/
  operator-invoked open path.
- `AlarmFloodMessageHandler`'s inline `RootCauseAnalysis.Open(...)` call —
  the *actual production auto-open path* (a flood detection opening an
  analysis automatically). This handler was deliberately built to inline
  `Open()` rather than call `OpenAnalysisCommandHandler`, because that
  handler commits its own transaction and reusing it would have broken the
  inbox's one-`SaveChanges` atomicity requirement (ADR-008). That earlier
  decision was about transactional correctness, not about which opens
  matter to Reporting — every opened case matters equally. Wiring only
  `OpenAnalysisCommandHandler` would have left Reporting's projection
  systematically blind to the primary production flow (real floods
  auto-opening real analyses), which is not a scope question with two
  reasonable answers — it's a completeness bug the same way RootCause's
  missing outbox was, so it's fixed here as part of the same prerequisite,
  not raised as a separate fork.

Both call sites reuse RootCause's **existing** outbox (`IOutboxWriter`,
`OutboxMessage` table, `OutboxRelay`) — one more message type through the
same mechanism, not a second outbox.

### ReportingDb — its own physical database, same pattern as AuditDb/ComplianceDb

Ch.35's own "CHAPTER DECISION" frames `ReportingDb` as its own store
(inbox receipt, event ledger, projection, checkpoint, generation) exactly
the way `AuditDb`/`ComplianceDb` were each their own store — consistent with
every consumer in this book never sharing a database with the context it
observes. `Nexus1.Reporting.Infrastructure` gets its own `ReportingDbContext`,
`__EFMigrationsHistory_Reporting` table, and connection string, following
ADR-010/ADR-011's already-established reasoning (data ownership, not
deployment topology — Reporting's *process* still composes into
`Nexus1.ModularRuntime`).

### The reduced projection — user-confirmed scope

Two reducers feed one row, matching the book's core idea (Executable Assets
35-G/35-H) but reduced:

- **`RootCauseCaseSummary`** (`Nexus1.Reporting.Domain`) — a real domain
  entity, not Infrastructure plumbing, for the same reason `AuditEvidenceRecord`
  and `ComplianceReview` earned their own Domain projects: it is the actual
  artifact Reporting exists to produce. Keyed by `RootCauseCaseSummaryId`
  wrapping the source `AnalysisId` directly (this project's natural case
  identity — no separate `RootCauseCaseId`/`VerdictId` split to key by,
  same collapse ADR-005 already made). `ApplyOpened(...)` is a static
  factory (creates the row, `Status = Open`); `ApplyVerdictIssued(...)` is
  an instance method (mutates the row, `Status = VerdictIssued`) — it
  throws if called on an already-`VerdictIssued` row, a cheap domain-level
  guard against double-applying a duplicate.
- **Out-of-order handling: an `AnalysisId`-keyed pending buffer**, not the
  book's stream-position/watermark/gap machinery. If `RootCauseVerdictIssuedV1`
  arrives before its case's `RootCauseCaseSummary` row exists, it is held in
  a `PendingVerdict` row (Infrastructure-layer plumbing, keyed by
  `AnalysisId` — not a domain concept, matching how `InboxReceipt`/
  `OutboxMessage` are also plumbing, not domain). When
  `RootCauseCaseOpenedV1` later creates the row, the handler checks for and
  applies any pending verdict for that `AnalysisId` in the same pass.

**Deferred, with an explicit reversal condition** (user-confirmed): the
`ProducerStreamPositionV1` concept and everything built on top of it — the
immutable `ProjectionEventLedger`, contiguous watermark/checkpoint tracking,
a separate `ProjectionStatus` health sampler, `RowVersion` optimistic
concurrency, and projection generations for rebuilds. None of these are
buildable without first reopening the producer-stream-position decision
ADR-004/ADR-005 already closed.

### Wildcard binding — one queue, two admitted event types, explicit allowlist

Ch.35's topology (Executable Asset 35-A) binds Reporting's queue with
`root-cause.#` (a wildcard), not the exact-routing-key bindings Audit and
Compliance use — because Reporting genuinely needs both
`root-cause.root-cause-case-opened.v1` and
`root-cause.root-cause-verdict-issued.v1` on the same queue.
`NexusTopology.DeclareQuorumQueue` needed no code change — `QueueBind`
already accepts any valid topic pattern, wildcard or exact.

The book is explicit that a broad binding is not a license for a broad
reducer (35's "PROJECTION CONTRACT" section): "The Reporting dispatcher
still rejects every event type/schema that is absent from its explicit
projection contract set." `ReportingProjectionMessageHandler` checks
`eventType` against exactly the two known types; anything else is a
**permanent** classification, not a transient one, so it is quarantined to
`PoisonMessage` immediately with `TerminalReason = "unsupported-contract"` —
no retry budget spent. This is a narrow, book-explicit, single check
specific to Reporting's wildcard binding, not the general allowlisted
`FailureClassifier` ADR-009 already declined to build for every consumer;
every *other* failure in this handler still goes through the same
retry-until-exhausted-then-poison path as Audit and Compliance.

### Retry/DLQ — book-given policy, no renaming needed this time

Ch.29's Policy Catalogue (29-H) keys Reporting's policy by
`reporting.integration-events.v1` — which is *already* ch.35's actual
frozen queue name (unlike Audit's and Compliance's catalogue entries, which
named ch.25's superseded illustrative queues). `PolicyId = "report-project-v1"`,
`MaxRetryAttempts = 6`, `MaxElapsed = 45 minutes` — used verbatim, no
adaptation. `InboxReceipt`/`RetryTicket`/`PoisonMessage`/`RetryDispatcher`
mirror Audit's and Compliance's shapes exactly, same reasoning (ADR-009/
ADR-010/ADR-011).

### No Application project, no Contracts project

Same anti-placeholder reasoning as Audit and Compliance: Reporting's only
behavior in this step is message-driven projection — no command/query
surface, nothing published outward.

## Consequences

- A future BFF/console surface reading `RootCauseCaseSummary` gets a
  genuinely useful "case + verdict" projection today, without the book's
  freshness/watermark evidence the BFF is supposed to return alongside it
  (35-A's "never labels delayed, unknown or failed data as live truth") —
  that evidence plane doesn't exist yet, a direct consequence of deferring
  the checkpoint/status apparatus. Worth remembering before building a BFF
  against this projection: it does not yet know how to say "I might be
  stale."
- All three fan-out subscribers (Audit, Compliance, Reporting) now publish/
  consume through the same outbox/inbox/retry/DLQ machinery — §5 step 8 is
  complete after this step.

## Rejected alternatives

- **Build the full book design, including `ProducerStreamPositionV1`.**
  Rejected by explicit user decision after the fork was raised (not picked
  silently) — reopens ADR-004/ADR-005 and changes contracts Audit/Compliance
  already treat as frozen, a larger and different cost than any prior
  deferral.
- **Wire only `OpenAnalysisCommandHandler`, leaving `AlarmFloodMessageHandler`'s
  auto-open path silent.** Rejected: not a real scope question — the
  primary production flow (flood-triggered auto-open) would never appear in
  Reporting's projection, which defeats the point of building it. Treated as
  a completeness fix within the same prerequisite, matching how RootCause's
  missing outbox itself was treated.
- **No out-of-order handling at all** (drop or fail a verdict that arrives
  before its case). Rejected: violates this project's own constitution
  discipline #1 in spirit — a valid fact must not be silently lost because
  of transport-order timing, even in the reduced design.

## Reversal condition

Revisit the full book apparatus (stream positions, immutable ledger,
watermark/checkpoint, status sampler, row versioning, projection
generations) if/when a real need emerges for rebuild-from-ledger,
concurrent-writer conflict detection, or genuine cross-context stream
positioning — not before, and not merely because the book describes it.

## Evidence required

- `dotnet ef migrations add` producing a readable migration for the new
  `ReportingDb` (`InboxReceipt`, `RootCauseCaseSummary`, `PendingVerdict`,
  `RetryTicket`, `PoisonMessage`).
- Component tests proving: in-order application (Opened then VerdictIssued)
  produces a correct row; out-of-order application (VerdictIssued then
  Opened) buffers then correctly applies; dedup on both event types;
  retry-then-poison for transient failures; straight-to-poison (no retry
  spent) for an unsupported event type.
- A real end-to-end run proving **both** orderings against the live broker:
  the normal chain, and a deliberately out-of-order publish (verdict
  published before its case-opened event) showing the pending buffer holds
  and later correctly resolves — broker counters and matching `MessageId`s
  as evidence, same bar as Audit and Compliance.
