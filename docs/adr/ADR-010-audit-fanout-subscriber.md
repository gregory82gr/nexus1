# ADR-010: Audit fan-out subscriber — AuditDb ownership, scope, and RootCause's producer-side outbox

## Status

Accepted.

## Context

Step 8 (CLAUDE.md §5) starts Audit, Compliance, and Reporting as independent
subscribers to `RootCauseVerdictIssued.v1`, one at a time, starting with
Audit as the simplest fan-out shape (record that a verdict occurred, no
business logic beyond persistence). `From_Services_To_Runtime` ch.34
("A Two-Consumer Fan-Out From One Verdict", pp. 816-841) covers exactly this
— Audit and Compliance both consume the same public fact independently; ch.35
activates Reporting on top of the same topology. Reading ch.34 before writing
any code surfaced two things worth recording before the Audit-specific work:

**RootCause has no producer-side outbox yet.** `CloseAnalysisCommandHandler`
never wired an outbox write, `RootCauseDb` had no `OutboxMessage` table, and
`RootCauseVerdictIssuedV1`'s own doc comment said as much ("not yet published
anywhere"). This is not a design choice with alternatives — Audit cannot
consume a fact RootCause never publishes — so it's built here as a
prerequisite, mirroring `Nexus1.AlarmManagement`'s already-proven phase (a)
outbox pattern exactly (ADR-008), not re-derived.

**Ch.34 gives Audit's shape precisely**, matching what the user described
independently: `AuditEvidenceRecord` (Executable Asset 34-AE) is an
append-only evidence record — `SourceMessageId`, `SourceVerdictId`,
`RootCauseCaseId`, `EventType`, `SchemaVersion`, `EnvelopeBytes`,
`EnvelopeSha256`, `CorrelationId`, `CausationId`, `OccurredAtUtc`,
`RecordedAtUtc`. "Audit deliberately does not do: recompute the verdict,
open/close Compliance review, update the source evidence record, erase or
overwrite history" (34-AF). Its own database, `AuditDb`, is explicit:
"no FK to RootCauseDb" (34-AH).

## Decision

### RootCause's producer-side outbox — adopted exactly, mirroring AlarmManagement

`Nexus1.RootCause.Application.IOutboxWriter`, `Nexus1.RootCause.
Infrastructure.Messaging.{OutboxMessage, OutboxMessageConfiguration,
EfOutboxWriter, OutboxRelay, OutboxPublisherBackgroundService}` are the exact
same shape as `Nexus1.AlarmManagement`'s phase (a) equivalents — same
reduced `OutboxMessage` columns (identity/integrity now, no retry/lease
columns, matching ADR-008's already-recorded reduction), same
`ProcessedAtUtc` null/non-null stand-in for the book's fuller state machine.
`CloseAnalysisCommandHandler` enqueues `RootCauseVerdictIssuedV1` in the same
transaction as the `Closed` status commit, using the frozen routing key
`root-cause.root-cause-verdict-issued.v1` and eventType
`nexus1.root-cause.root-cause-verdict-issued.v1` (ch.34 Executable Asset
34-P's frozen coordinates, matching ADR-005's already-adopted naming
convention).

### AuditDb — its own physical database, for a different reason than ReactorFleet's shared one

**AuditDb is a separate physical database from both `RootCauseDb` and
`AlarmManagementDb`.** This is the book's explicit constraint (34-AH: "no FK
to RootCauseDb"), and it is worth contrasting with ADR-006's opposite call
for ReactorFleet, since both ReactorFleet and Audit are process-composed
into `Nexus1.ModularRuntime` rather than independently deployed:

- ADR-006's reasoning for ReactorFleet sharing `AlarmManagementDb` was about
  **deployment topology** — the book's DB-per-service argument (isolation,
  independent scaling, independent failure domains) doesn't apply to a
  context that isn't independently deployed yet.
- Audit's separate-database requirement is about **data ownership and
  integrity**, not deployment topology, and holds regardless of which
  process happens to host the consumer code: an audit trail's value depends
  on it being isolable from the systems it observes — no shared connection,
  no cross-database transaction, no FK that could let a future change
  accidentally couple Audit's append-only history to RootCause's mutable
  state. Composing Audit's *process* into `ModularRuntime` (see below) is a
  deployment-topology call consistent with ADR-006; giving it its own
  *database* is a data-integrity call independent of that topology, and the
  book is explicit enough about it that this isn't a judgment call to
  re-derive.

`Nexus1.Audit.Infrastructure` gets its own `AuditDbContext`, its own
`__EFMigrationsHistory_Audit` table, and its own connection string.

### Audit context shape — Domain + Infrastructure, no Application, no Contracts

- **`Nexus1.Audit.Domain`**: `AuditEvidenceRecord` only, with an `Append(...)`
  factory mirroring Executable Asset 34-AE, reduced to this project's actual
  fields (`RootCauseVerdictIssuedV1`'s already-adopted minimal payload has no
  `SiteId`/`LineId`/`PolicyIdentity`/etc. per ADR-005 — `AuditEvidenceRecord`
  carries what the envelope actually carries, not the book's fuller frozen
  shape). This earns a real Domain project (not folded into Infrastructure
  like `InboxReceipt`/`OutboxMessage`) because it's the actual business
  artifact Audit exists to produce, not transport plumbing — matching how
  every other context's genuinely-owned entity lives in Domain.
- **No `Nexus1.Audit.Application`.** Audit's only behavior today is
  message-driven consumption — there is no command/query surface distinct
  from the consumer handler, exactly mirroring how `AlarmFloodMessageHandler`
  already bypasses Application/CQRS entirely in `Nexus1.RootCause.
  Infrastructure`. Creating an Application project with nothing in it would
  be exactly the empty-placeholder anti-pattern CLAUDE.md §2 forbids;
  add one when Audit gets a real query/command surface (e.g. an operator
  read API), not before.
- **No `Nexus1.Contracts.Audit`.** Audit does not publish anything outward
  in this phase — it is a terminal consumer. Matches the project's own rule
  (Contracts projects only if/when a context needs to publish outward).

### Idempotency — transport dedup plus one semantic layer, reusing what RootCause already proved

`AuditInboxReceipt` (`(ConsumerName, MessageId)` PK) is the same transport
dedup shape RootCause's `InboxReceipt` already proved in ADR-008 — reused,
not reinvented, as asked. On top of it, Audit adds the one genuinely new
piece ch.34's "two-key oracle" (34-AI) calls for: a unique index on
`AuditEvidenceRecord.SourceAnalysisId` (this project's single collapsed
identity standing in for the book's separate `VerdictId`, per ADR-005).
Transport dedup alone isn't enough here, because a *replayed* delivery could
arrive under a *new* `MessageId` for the *same* verdict — the fast pre-read
would miss it (different `MessageId`), but recording a second evidence row
for a verdict already audited would be a real correctness bug, not just a
redundant retry. The handler checks both: a known `MessageId`
short-circuits to ack (existing behavior); an unknown `MessageId` but
already-recorded `SourceAnalysisId` still records the new inbox receipt
(transport truth) but skips the `AuditEvidenceRecord` insert (semantic
truth), returning `audit-evidence-already-recorded` per 34-AI's oracle
table.

### Append-only enforcement — a real EF interceptor, not just a code comment

Executable Asset 34-AJ's `SavingChanges` interceptor (rejecting `Modified`/
`Deleted` states on `AuditEvidenceRecord`) is adopted as-is. This is cheap
(one small `SaveChangesInterceptor`, no new dependency) and it is the one
piece of "Audit never mutates history" that's worth enforcing in code rather
than trusting call-site discipline — matching CLAUDE.md's own constitution
discipline #1 (verdicts/records are honest) applied to this context.

### Retry/DLQ — reuses phase (b)'s exact mechanism, duplicated per context like every other inbox/outbox table

`AuditRetryTicket`/`AuditPoisonMessage`/`AuditVerdictMessageHandler`/
`AuditConsumerBackgroundService`/`AuditRetryDispatcher`/
`AuditRetryDispatcherBackgroundService` mirror RootCause's phase (b) shapes
(ADR-009) exactly — same reduced `RetryTicket` (no lease columns, no
encryption), same `RetryPolicy`/`RetryBudget`/`RetryBackoff` (shared,
already in `Nexus1.BuildingBlocks.Messaging`), same "retry until budget
exhausted, then quarantine" simplification (no allowlisted failure
classifier). `MessageHandlingOutcome` moves from `Nexus1.RootCause.
Infrastructure.Messaging` to `Nexus1.BuildingBlocks.Messaging` — it is a
plain, zero-dependency enum, genuinely shared logic, unlike the DbContext-
coupled entities around it. The entities themselves stay duplicated per
context rather than becoming a shared "MessagingKernel" abstraction, matching
this project's existing convention (`OutboxMessage`/`InboxReceipt` are
already independently defined per context, never shared) — this is the
established pattern, not a new decision.

Audit's DLX policy (`audit.root-cause-verdicts.v1` -> `nexus.dead`) is
provisioned the same way RootCause's is (`RabbitMqDeadLetterPolicyProvisioner`,
ADR-009) — one policy per queue, same reasoning.

### Topology — ch.34's frozen shape, not ch.25's broader illustrative one

Queue `audit.root-cause-verdicts.v1`, bound only to routing key
`root-cause.root-cause-verdict-issued.v1` (Executable Asset 34-U). This is
narrower than ch.25's original illustrative topology (`audit.integration-
events.v1`, wildcard-bound to `alarm-management.#`, `root-cause.#`,
`compliance.#`) — ch.34's frozen shape supersedes it the same way ch.32
"resolves the earlier illustrative V1 shapes" for AlarmFloodDetected, and it
matches this project's own already-scoped Phase 1 decision (Audit subscribes
to `RootCauseVerdictIssued.v1` specifically, not a firehose of every producer's
output — CLAUDE.md §2).

### Host composition — `Nexus1.ModularRuntime`, not independently deployed

Per ADR-001-amend's "protected modular core" principle: only RootCause is
independently deployed in Phase 1; everything else (now including Audit)
stays composed into `Nexus1.ModularRuntime` until it earns its own
extraction. Audit's own database is separate (see above), but its *process*
is not.

## Consequences

- A future Compliance subscriber follows the identical shape (own database
  per the book's `ComplianceDb` "no FK" constraint, own reduced retry/DLQ,
  own queue `compliance.root-cause-verdicts.v1`) — this ADR's reasoning
  transfers directly, expect a short ADR-011 recording specifics rather than
  re-deriving the pattern.
- Reporting (ch.35) needs a second contract, `RootCauseCaseOpenedV1`, that
  does not exist in this project yet — out of scope for Audit, noted here so
  it isn't a surprise when Reporting's turn comes.

## Rejected alternatives

- **Share `AuditDb` with `AlarmManagementDb`** (since Audit is also
  process-composed into `ModularRuntime`, matching ReactorFleet's ADR-006
  precedent). Rejected: ADR-006's reasoning is about deployment topology,
  which doesn't govern Audit's database-separation requirement — that
  requirement is about data-ownership integrity and is explicit in the book
  independent of topology (see "AuditDb" section above).
- **Fold `AuditEvidenceRecord` into Infrastructure alongside `InboxReceipt`**
  rather than giving Audit a real Domain project. Rejected: unlike inbox/
  outbox rows, `AuditEvidenceRecord` is the actual artifact Audit exists to
  produce, not transport bookkeeping.
- **Build the book's full allowlisted failure classifier for Audit's
  consumer** (matching ch.29's `FailureClassifier`, deferred for RootCause
  in ADR-009). Rejected for the same reason ADR-009 already gave: no real
  permanent-vs-transient failure taxonomy exists yet for this project's
  actual dependencies to justify it.

## Reversal condition

Revisit AuditDb's isolation if Audit is ever independently deployed (no
change needed then — it already has its own database). Revisit the reduced
failure classifier under the same condition ADR-009 already recorded.
Revisit Audit's missing Application layer once a real query/command surface
is needed.

## Evidence required

- `dotnet ef migrations add` producing readable migrations for both
  `RootCauseDb` (new `OutboxMessage` table) and the new `AuditDb`
  (`InboxReceipt`, `AuditEvidenceRecord`, `RetryTicket`, `PoisonMessage`).
- Component tests proving: RootCause's outbox transactionality (mirroring
  `OutboxRelayTests`), Audit's two-key dedup oracle (transport-new but
  semantically-duplicate does not re-insert evidence), Audit's retry-then-
  poison path, Audit's append-only enforcement (`AuditMutationRejected` on
  an attempted update).
- A real end-to-end run: both hosts live against the real broker and real
  databases, `RootCauseVerdictIssuedV1` actually published by RootCause and
  actually consumed by Audit, `AuditEvidenceRecord` verified against
  `AuditDb`, broker queue/exchange counters as evidence — same bar as
  phase (a).
