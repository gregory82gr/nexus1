# ADR-011: Compliance fan-out subscriber — ComplianceDb ownership and the mutable-vs-append-only fork

## Status

Accepted.

## Context

Compliance is the second of §5 step 8's three fan-out subscribers, following
Audit (ADR-010). `From_Services_To_Runtime` covers both in the same chapter
(ch.34, pp. 831-834, Executable Assets 34-AK through 34-AO), and the
dedup/idempotency mechanism is structurally identical between them — but the
user was explicit not to assume Compliance collapses to Audit's shape just
because it's the next subscriber in the same chapter, and reading the book's
own text confirms a real structural difference, not just a naming one.

**No prerequisite work was needed this time.** RootCause's producer-side
outbox (built for Audit, ADR-010) already publishes `RootCauseVerdictIssuedV1`
on routing key `root-cause.root-cause-verdict-issued.v1`. A topic exchange
fans a single publish out to every bound queue independently — Compliance
gets its own queue bound to that same routing key, with zero changes to
RootCause's publish side. This is exactly what ch.34's own "Broker Topology"
section describes (34-U): "Independent subscribers require independent
queues... A single shared queue would create competing consumers."

## Decision

### ComplianceDb — its own physical database, same reasoning as AuditDb

Executable Asset 34-AN is explicit: `ComplianceReview`'s constraints include
"no FK to RootCauseDb/AuditDb". Same data-ownership reasoning ADR-010 already
recorded for AuditDb applies here without modification — this is a
data-integrity requirement independent of deployment topology, not something
to re-derive. `Nexus1.Compliance.Infrastructure` gets its own
`ComplianceDbContext`, `__EFMigrationsHistory_Compliance` table, and
connection string.

### The structural fork: ComplianceReview is mutable by design; AuditEvidenceRecord is not

This is the genuine fork the user asked to be flagged rather than assumed
away. Ch.34's own authority table (34-AL) draws the line explicitly:

> Compliance **may own**: review identity and Pending state; review
> assignment, findings and decision; human-review authorization and
> deadlines; its own later public review facts.
>
> Compliance **must never**: infer or mutate RootCause verdict correctness;
> RootCause evidence membership; Audit evidence history; source event or
> producer stream.

Contrast with Audit's own authority table (34-AF): Audit "deliberately does
not... erase or overwrite history" — full stop, no reserved future mutation
authority at all. `AuditEvidenceRecord` is permanent historical fact,
enforced with a `SaveChangesInterceptor` (ADR-010). `ComplianceReview` is a
workflow record that starts `Pending` and is explicitly designed for later
human-driven state transitions (assignment, findings, decision) — those
commands don't exist yet ("Adding one later needs its own consumer, schema
and evidence decision", 34-AP's refinement note), so nothing in *this* step
actually mutates a `ComplianceReview` after `Open()` — but the entity must
not be locked down the way `AuditEvidenceRecord` is, because that would
contradict the book's own authority model for a capability this project
hasn't built yet. Concretely: `Nexus1.Compliance.Infrastructure` has no
append-only interceptor, and `ComplianceReviewState` is a private-set
property in the domain model precisely because it is expected to change,
even though today only one value (`Pending`) is ever assigned.

### Contract minimization — ComplianceReview does not copy the envelope

Executable Asset 34-AK's `ComplianceReview` has no `EnvelopeBytes`/
`EnvelopeSha256` fields at all, unlike `AuditEvidenceRecord`. Ch.34 states
the reason directly (34-AL): "Compliance receives only the public verdict
summary required to open and correlate a review. Detailed RootCause evidence
remains owner-private" — Audit already owns the evidentiary copy; Compliance
only needs enough to open and correlate a review. Reduced to this project's
actual payload (no `SiteId`/`LineId`/`ObservedVerdictIdentity` — RootCause's
already-adopted minimal `RootCauseVerdictIssuedV1`, ADR-005), `ComplianceReview`
carries: `Id`, `SourceMessageId`, `SourceAnalysisId` (the correlation key,
same collapsed-identity reasoning as Audit's `SourceAnalysisId`, ADR-010),
`Verdict` (the plain string a human reviewer actually looks at — this
project's reduced stand-in for the book's cryptographic
`ObservedVerdictIdentity` hash, which nothing in this project computes),
`State`, `OpenedAtUtc`.

### Idempotency — the same two-key oracle, genuinely reused

Compliance's two-key oracle (34-AO) is the same shape as Audit's (34-AI):
transport dedup via `InboxReceipt(ConsumerName, MessageId)`, plus a unique
index on `ComplianceReview.SourceAnalysisId` for the semantic half — a
replay under a new `MessageId` for an already-reviewed verdict records a new
receipt (`review-already-exists`) but does not open a second review. This
part of Audit's shape transfers directly, no adaptation needed.

### Retry/DLQ and topology — reused verbatim, one queue

`RetryTicket`/`PoisonMessage`/`RetryDispatcher`/`RetryDispatcherBackgroundService`
mirror Audit's (and RootCause's) shapes exactly — same reduced columns, same
`RetryBudget`/`RetryBackoff` from `Nexus1.BuildingBlocks.Messaging`, same
"retry until budget exhausted, then quarantine" simplification, same
per-queue DLX policy via `RabbitMqDeadLetterPolicyProvisioner`. Retry policy
numbers are book-given (ch.29 Policy Catalogue 29-H): `compliance.
verdict-events.v1 -> cmp-verdict-read-v1: 4 attempts / 30-minute budget`.
The book's catalogue key is ch.25's superseded illustrative queue name, not
ch.34's frozen `compliance.root-cause-verdicts.v1` — renamed to
`compliance-root-cause-verdicts-v1` to match the actual queue, same
adaptation ADR-010 already made for Audit's policy.

Topology: queue `compliance.root-cause-verdicts.v1`, bound only to routing
key `root-cause.root-cause-verdict-issued.v1` (Executable Asset 34-U) —
Audit and Compliance are independent bindings from the same exchange to two
different queues, not a shared queue and not a chained subscription.

### No Application project, no Contracts project — same anti-placeholder discipline as Audit

Compliance's only behavior in this step is message-driven consumption —
same reasoning ADR-010 already gave for Audit's missing Application layer
applies unchanged. No `Nexus1.Contracts.Compliance`: Compliance does not
publish anything outward in this step (a future
`ComplianceReviewOpened.v1` fact is explicitly deferred by the book itself,
34-AP's refinement note — "no approved downstream contract requires that
fact yet").

## Consequences

- When Compliance's review-assignment/findings/decision workflow is
  eventually built, it lands as new commands against the already-mutable
  `ComplianceReview` entity — no schema rework, no re-litigating the
  append-only-vs-mutable question, since this ADR already settled it.
- Reporting (ch.35) is next and needs a second contract
  (`RootCauseCaseOpenedV1`) this project doesn't have yet — already flagged
  in ADR-010, repeated here since it's now one step closer.

## Rejected alternatives

- **Lock `ComplianceReview` down with the same append-only interceptor as
  `AuditEvidenceRecord`.** Rejected: contradicts the book's own explicit
  authority model (34-AL), which reserves future mutation authority to
  Compliance that Audit is explicitly denied. Building the lock now would
  have to be undone later for no benefit today.
- **Copy the full envelope onto `ComplianceReview`, matching
  `AuditEvidenceRecord`'s shape for consistency.** Rejected: the book states
  the opposite principle directly (contract minimization, 34-AL) — Audit
  already owns the evidentiary copy, duplicating it onto Compliance would be
  redundant storage with no stated purpose.

## Reversal condition

Revisit when Compliance's review-assignment/findings/decision commands are
actually built — `ComplianceReviewState` gains real transitions beyond
`Pending`, and those commands are the first real content for a
`Nexus1.Compliance.Application` project.

## Evidence required

- `dotnet ef migrations add` producing a readable migration for the new
  `ComplianceDb` (`InboxReceipt`, `ComplianceReview`, `RetryTicket`,
  `PoisonMessage`).
- Component tests proving: the two-key dedup oracle, retry-then-poison,
  and — the asymmetry check — that `ComplianceReview` can be mutated without
  throwing, directly contrasting with Audit's
  `AuditMutationRejectedException` test.
- A real end-to-end run showing the *same* `RootCauseVerdictIssuedV1`
  publish reaching both Audit and Compliance independently (matching
  `MessageId` in both `AuditDb` and `ComplianceDb`) — proving genuine
  fan-out via one topic-exchange routing key to two queues, not two
  isolated single-consumer proofs.
