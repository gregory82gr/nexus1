# ADR-009: Messaging backbone phase (b) — retry, backoff, and dead-letter hardening

## Status

Accepted.

## Context

Phase (a) (ADR-008) proved basic topology, outbox, inbox, and publish/consume
end-to-end against a real broker, deliberately deferring retry/backoff/DLQ
hardening as a separate, later step. This ADR covers that step.

`From_Services_To_Runtime` ch.29 ("Retry, Backoff, Dead Letters, and Poison
Messages", pp. 645-669) specifies a full consumer-owned retry/DLQ subsystem:
a `RetryTicket` table with a six-state lifecycle (Due/Claimed/Published/
Consumed/Superseded/Quarantined), lease-based claiming for multi-instance
dispatch safety, **encrypted** envelope storage (`ProtectedEnvelope`/
`ProtectionKeyId`), a `PoisonMessage` table, and a full operator
replay-approval workflow (`ReplayRequest`/`ReplayDispatch`, legal holds,
ETag-guarded reveal endpoints — ch.29 pp. 653-669, continuing into ch.42's
maintenance-agent material). The broker's own dead-letter exchange
(`nexus.dead`, quorum `delivery-limit`, ch.25.5 Executable Asset 25-C) is
explicitly described as an "imported safety net" *underneath* this
consumer-owned ledger, not the primary mechanism: "default re-publication
does not itself prove lossless terminal evidence" (ch.29 p.654).

This is a materially larger system than a Phase-0 educational demonstrator
needs, and the encryption/lease/replay-operator apparatus pulls in scope
(a key-management dependency, multi-instance deployment, an operator
approval workflow) this project doesn't otherwise have. Presented with this
gap between the book's full design and what phase (b) actually needs, three
options were on the table: build the full book design regardless; skip
straight to a from-scratch minimal design; or adopt the same
reduce-and-record pattern phase (a) already used for the outbox/inbox
tables. The user chose the third explicitly, matching the MediatR
(ADR-002-amend) and Query BFF (ADR-007) deferrals already recorded in this
project — not rejecting the fuller design, deferring it with a stated
reversal condition.

## Decision

### Retry policy and backoff — adopted exactly

`RetryPolicy`, `RetryBudget.Evaluate`, `RetryBackoff.ExponentialCap`, and
`RetryBackoff.EqualJitter` (`Nexus1.BuildingBlocks.Messaging`) are the
book's Executable Assets 29-G/29-I/29-K/29-L verbatim, with one type change:
`DateTimeOffset` parameters became plain `DateTime` (see "Owned" below for
why). RootCause's subscription policy is the book's own catalogue entry
(29-H, p.649): `rc-alarm-read-v1`, `MaxRetryAttempts=5`,
`MaxElapsed=15 minutes` — already recorded in ADR-008, used verbatim, not
re-derived. `InitialDelay`/`MaxDelay`/`EqualJitterPercent` are **not**
book-given (the catalogue only states attempt/elapsed budgets); chosen as
2s/15s/30% for demonstrator practicality — a full retry-to-poison run
completes in well under a minute of wall-clock time rather than being tuned
for a real dependency's recovery characteristics. Worst-case cumulative
wait across all 5 retries is ~44s, comfortably inside the book-given
15-minute budget.

### RetryTicket — reduced, per the outbox's own precedent

Adopted: identity (`RetryTicketId`, `ConsumerName`, `MessageId`, `Attempt`),
policy/failure metadata (`PolicyId`, `FailureCode`, `FirstFailedAtUtc`,
`DueAtUtc`), the frozen republish payload (`Producer`, `EventType`,
`SchemaVersion`, `OriginalRoutingKey`, `EnvelopeBytes`, `EnvelopeSha256`),
and `PublishedAtUtc` as the reduced stand-in for the book's fuller state
machine (null = due/pending, non-null = dispatched) — exactly the pattern
`OutboxMessage.ProcessedAtUtc` already established in phase (a).

Deferred: `ProtectedEnvelope`/`ProtectionKeyId` (no key-management
infrastructure exists anywhere else in this repo — `OutboxMessage` already
stores envelopes unencrypted, so encrypting only `RetryTicket` would be
inconsistent, not more secure); `LeaseOwner`/`LeaseUntilUtc` (single-instance
`RootCause.Host` has nothing to lease-protect against — same rationale
phase (a) used for `OutboxMessage`'s lease columns); `ReplayGeneration` and
the `Due/Claimed/Published/Consumed/Superseded/Quarantined` state machine
(no operator replay workflow exists).

### RetryDispatcher — reuses phase (a)'s inbox dedup instead of a second idempotency mechanism

`RetryDispatcher`/`RetryDispatcherBackgroundService` mirror
`OutboxRelay`/`OutboxPublisherBackgroundService` exactly: poll due tickets,
republish the frozen envelope bytes through the normal exchange with the
original routing key, mark published on success, leave unpublished on
failure. The redelivered message re-enters the live queue as an ordinary
delivery and is deduplicated by the same `InboxReceipt` mechanism phase (a)
already proved — deliberately not a second retry-scoped idempotency
mechanism.

### PoisonMessage — the consumer-owned terminal record, reduced

Adopted as a terminal record only: identity, `EnvelopeSha256`, `EventType`,
`SchemaVersion`, `TerminalReason`, `RetryAttempts`, `FirstFailedAtUtc`,
`QuarantinedAtUtc`. This is the book's primary evidence ("Application poison
is recorded before broker acknowledgement", ch.29 p.654) — the broker's
`nexus.dead` queue is the independent safety net on top of it, not a
replacement for it. Deferred: `State`, legal holds, and the
`ReplayRequest`/`ReplayDispatch` operator workflow.

### Failure classification — reduced from an allowlist to attempt/elapsed budget alone

The book's `FailureClassifier` (Executable Asset 29-F) is an allowlist over
`FailureKind`/`FailurePhase` that can route a genuinely permanent failure
(e.g. `ContractAdmissionException`) straight to quarantine without spending
retry budget. This phase does not build that classifier: every exception
`AlarmFloodMessageHandler` doesn't otherwise recognize as ambiguous is
treated as retryable until the attempt/elapsed budget is exhausted, then
quarantined. Concretely this means a permanently-malformed message consumes
its full retry budget (worst case ~44s) before landing in `PoisonMessage`,
rather than being classified straight to poison. Accepted for this phase:
the ceiling cost is small (bounded by the same policy numbers either way)
and building a real allowlist classifier without any actual permanent-vs-
transient failure taxonomy from this project's own dependencies would be
speculative.

One case is still classified explicitly, unchanged from phase (a): a
`DbUpdateException`-resolved-ambiguous outcome (the fresh-DbContext re-read
still can't confirm success) returns `NackRequeue`, matching the book's
"local commit outcome unknown -> none; stop without acknowledgement"
(Guarantee Ledger 29-A) — the closest available action with this client's
synchronous ack/nack API.

### Dead-letter topology — adopted exactly, via broker policy not queue arguments

Matches Executable Asset 25-C: `dead-letter-exchange=nexus.dead`,
`dead-letter-strategy=at-least-once`, `overflow=reject-publish`,
`delivery-limit=8`, applied as **broker policy** (via the RabbitMQ
management HTTP API — `Nexus1.BuildingBlocks.Messaging.
RabbitMqDeadLetterPolicyProvisioner`, no new NuGet package, just the BCL
`HttpClient`) rather than queue x-arguments — the book is explicit that
`dead-letter-strategy` has no x-argument form at all and operability is the
reason for preferring policy ("operators can change them without deleting
and recreating the live queue", ch.25.5). `NexusTopology.DeclareQuorumQueue`
still declares only `x-queue-type: quorum` at queue-create time, matching
Executable Asset 25-B verbatim.

One correction to the book's own literal code: its `NexusTopology` model
computes a per-subscription `DeadRoute` (e.g.
`rootcause.alarm-events.v1.dead`) and uses it only to bind the dead queue to
`nexus.dead` — it never sets `dead-letter-routing-key` anywhere (not as a
queue argument, not in the Asset 25-C policy body). Without that override,
RabbitMQ dead-letters a message using its *original* routing key (e.g.
`alarm-management.alarm-flood-detected.v1`), which does not match the dead
queue's binding key — the message would be unroutable in `nexus.dead` and
silently dropped. This looks like a gap in the book's own code rather than
a deliberate simplification, so `RabbitMqDeadLetterPolicyProvisioner` adds
`dead-letter-routing-key` to the policy, set to the queue's own dead route.
Because that key is inherently per-queue, one policy is applied per live
queue (scoped by an exact-match regex on that queue's name) rather than one
pattern matching several queues at once, unlike the book's single shared
policy across all four subscriptions.

### Two dispositions, not the naive unbounded loop

`AlarmFloodConsumerBackgroundService` previously nacked with `requeue: true`
on *every* failure, unconditionally and forever — exactly the "loop that
must not ship" the book calls out by name (Rejected Asset 29-B: "immediate,
unclassified, unbounded"). Replaced with three explicit dispositions
(`MessageHandlingOutcome`): `Ack` (success, confirmed duplicate, or a retry
ticket recorded — ownership moves to `RetryDispatcher` either way);
`NackRequeue` (the one still-ambiguous case above); `NackNoRequeue`
(retry budget exhausted, routes to the broker's `nexus.dead` safety net via
the policy above).

### Publisher-side resilience — already correct, made explicit

`OutboxRelay`'s existing broad `catch (Exception ex)` around
`publisher.PublishAsync` already left a row's `ProcessedAtUtc` null on any
publish failure, including a broker outage — no code change needed there.
Made explicit rather than relying on the RabbitMQ.Client's implicit default:
`RabbitMqConnectionManager`'s `ConnectionFactory` now sets
`AutomaticRecoveryEnabled = true` and `NetworkRecoveryInterval =
TimeSpan.FromSeconds(5)` directly, so the same long-lived `IConnection`
reconnects on its own once the broker returns, without requiring a host
restart. `RetryDispatcher` uses the identical try/leave-unpublished contract
as `OutboxRelay`.

## Consequences

- A future phase adding a second RootCause instance (or any second consumer
  instance anywhere) needs `RetryTicket` lease columns before it's safe —
  today's single-instance dispatch has nothing to protect against, so this
  isn't built yet.
- `RabbitMqDeadLetterPolicyProvisioner`'s per-queue policy call needs to run
  once per live queue added — step 8 (Audit/Compliance/Reporting fan-out)
  will need to call it for each new subscription queue, not extend one
  shared pattern.
- A permanently-malformed message still spends its full retry budget before
  quarantine (see "Failure classification" above) — acceptable now, revisit
  if a real permanent-failure case makes that cost matter.

## Rejected alternatives

- **Build the full ch.29 design as specified** (encrypted envelopes,
  lease-based claiming, the six-state lifecycle, operator replay/legal-hold
  workflow). Rejected: substantially larger scope than phase (b) needs,
  pulls in a key-management dependency this repo doesn't otherwise have,
  and targets a multi-instance/compliance-driven deployment shape this
  project isn't in yet. Explicit user decision, not a unilateral cut.
- **Queue x-arguments instead of broker policy for dead-lettering.**
  Considered for simplicity (no HTTP management API call needed), but
  rejected: the book is explicit that `dead-letter-strategy` has no
  x-argument form, and the user asked for the DLX policy "exactly as the
  book specifies."
- **A full allowlisted `FailureClassifier`** (Executable Asset 29-F).
  Rejected for this phase: without any real permanent-vs-transient failure
  taxonomy from this project's actual dependencies (SQL Server, RabbitMQ),
  an allowlist would be speculative rather than evidence-based.

## Reversal condition

Revisit envelope encryption when this project (or a real deployment
derived from it) has actual key-management infrastructure and a stated
need to protect retry payloads at rest beyond what `OutboxMessage` already
does. Revisit lease-based claiming when a second consumer instance is
actually deployed. Revisit the operator replay/legal-hold workflow when a
real incident-response or compliance requirement calls for republishing a
poisoned message rather than fixing the root cause and letting new traffic
flow. Revisit the reduced failure classifier if a specific permanent
failure case (e.g. a schema-invalid payload) is observed spending its full
retry budget in practice rather than being classified immediately.

## Evidence required

- Real RabbitMQ management API output showing `rootcause.alarm-events.v1`'s
  DLX policy actually applied (dead-letter-exchange, dead-letter-routing-key,
  delivery-limit) and the dead queue actually bound.
- A simulated poison message (a handler that always fails) proven to exhaust
  its retry budget and land in `rootcause.alarm-events.v1.dead`, evidenced
  by the broker's own queue/exchange counters, not a passing unit test
  alone.
- A simulated broker outage (RabbitMQ stopped) with unpublished outbox rows
  present, proving `ProcessedAtUtc` stays null while the broker is down and
  the rows get published once it recovers — real broker state, not a
  description of expected behavior.

## Owned

- `RetryBudget.Evaluate`'s parameters are `DateTime`, not the book's
  `DateTimeOffset` — this repo's convention everywhere else is plain UTC
  `DateTime` via `IDateTimeProvider`, and a `DateTime` read back from SQL
  Server through EF Core loses its `Kind` (becomes `Unspecified`). The
  implicit `DateTime`-to-`DateTimeOffset` conversion then treats an
  `Unspecified` value as *local* time — on the development machine (UTC+3),
  this silently shifted a persisted `FirstFailedAtUtc` by three hours and
  tripped the elapsed-budget check on the very first retry, caught by the
  new `Exhausting_the_retry_budget_...` component test failing with an
  unexpected `elapsed-budget-exhausted` reason on attempt 2. Matching the
  surrounding codebase's type eliminates the whole class of bug rather than
  requiring every future call site to remember `DateTime.SpecifyKind`.
