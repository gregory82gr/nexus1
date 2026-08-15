# ADR-008: Messaging backbone — topology, wire format, outbox/inbox shape, and phase (a)/(b) split

## Status

Accepted.

## Context

`From_Services_To_Runtime`'s messaging backbone (Part V, ch. 25–29 in
`From_Services_To_Runtime_Part_2.pdf`) is unusually prescriptive — the book
itself labels its code samples "Executable Assets," intended to be lifted
close to verbatim, not adapted from theory. Unlike most of this project's
prior source-material gaps (ReactorFleet's thin coverage, RootCause's
naming mismatch), this chapter cluster gives exact, concrete answers for
nearly everything asked of it: exchange/queue/routing-key names, table
schemas down to the column, numeric tunables, canonicalization algorithm,
even golden serialized instances with byte counts and digests.

That precision creates a different kind of decision than usual: not "what
does the book say," but "how much of what the book says lands in this
phase." The user explicitly split step 7 into phase (a) — topology,
outbox, inbox, basic publish/consume proven end-to-end — and phase (b) —
retry/backoff/DLQ hardening. The book itself does not separate these; ch.
25–29 presents outbox, inbox, and retry/DLQ as one integrated design, with
the outbox table's own `State` enum (`Pending/InFlight/Retryable/
Dispatched/Quarantined`) and lease/lock columns existing specifically to
support the retry machinery ch. 27 and 29 build on top of it. This ADR
records which book elements land in phase (a) and which are deliberately
deferred, so a future session (or phase (b) itself) doesn't have to
re-derive the split from scratch.

## Decision

### Topology — adopted exactly, no deviation

Transport naming has no domain-modeling cost and no reason to diverge from
the book (ch. 25.2, p. 556; concrete application pp. 556, 747–748,
821–822):

- **Exchanges**: one shared durable topic exchange `nexus.events` for all
  producers (not per-context, not per-event-type); one shared durable topic
  exchange `nexus.dead` for dead-lettered messages.
- **Queues**: quorum type (`x-queue-type: quorum`), durable, one per
  consumer subscription — "a queue is never shared merely to reduce broker
  objects."
- **Routing keys**: `<producer>.<event-name>.v<major>` — concretely
  `alarm-management.alarm-flood-detected.v1` and
  `root-cause.root-cause-verdict-issued.v1`.
- **Consumer queue names**: `<consumer>.<subscription>.v<major>` —
  concretely `rootcause.alarm-events.v1` (RootCause's subscription to
  AlarmManagement's flood events).
- **`eventType` envelope field** (distinct from the routing key — carries
  the `nexus1.` prefix the routing key never does):
  `nexus1.<producer>.<event-name>.v<major>` — concretely
  `nexus1.alarm-management.alarm-flood-detected.v1`.
- **Dead-letter naming**: `<live-queue>.dead`, e.g.
  `rootcause.alarm-events.v1.dead` — the queue itself is declared in phase
  (b) (see below), but the naming convention is recorded now for
  consistency.

### Wire format — adopted exactly, hand-rolled canonicalizer

**JSON**, explicit in the book (`ContentType: application/json`,
`ContentEncoding: utf-8`), with **RFC 8785 JSON Canonicalization Scheme
(JCS)** canonicalization fingerprinted via SHA-256 (ch. 27, Executable
Asset 27-Q/27-V-adjacent material, p. ~604).

Two RFC-8785-compliant NuGet packages exist (`MackySoft.Json.
Canonicalization`, `Baqhub.Packages.JsonCanonicalization`) but both have
low adoption (under 4,000 total downloads each) and are unverified
publishers — not a trust tier this project extends to a dependency that
sits on every message this system sends. **Hand-rolled instead**: this
project's message payloads use only strings, integers, and nested
objects/arrays — no floating-point numbers — which sidesteps RFC 8785's
hardest correctness area (ECMAScript-compatible serialization of IEEE 754
doubles). The hand-rolled canonicalizer implements: object keys sorted by
UTF-16 code unit (RFC 8785 §3.2.3), no insignificant whitespace, standard
JSON string escaping. It is documented as scoped to the JSON shapes this
project actually produces, not a general-purpose RFC 8785 library.

**Envelope shape** (frozen, ch. 32–34): `envelopeVersion`, `messageId`,
`eventType`, `schemaVersion`, `occurredAtUtc`, `producer` (plain string,
e.g. `"alarm-management"` — the book's earlier ch. 25 illustrative envelope
nested `producer` as an object; ch. 32's frozen contract supersedes that
per the book's own stated rule), `correlationId`, `causationId`,
`contentType`, `fingerprintAlgorithm`, `payload`, `contentFingerprint`.

**AMQP property mapping** (Executable Asset 27-Q): `AppId = producer`,
`MessageId = messageId` (D-format GUID), `Type = eventType`,
`ContentType`, `ContentEncoding = "utf-8"`, `CorrelationId`,
`DeliveryMode = Persistent`, `Timestamp` (Unix seconds); custom headers
`schema-version`, `causation-id`, `envelope-sha256`. `traceparent`/
`tracestate` (W3C trace context) are in the book's mapping but have no
current consumer in this project (no distributed tracing wired up) —
omitted from phase (a), not a deviation from the wire contract, just an
unused header slot until tracing exists.

### Outbox table — reduced to phase (a)'s actual need

The book's `messaging.OutboxMessage` (Executable Asset 26-E, p. 581) has
18 columns, several of which exist specifically for retry/backoff (ch. 27,
29) or multi-instance lease-based claiming: `State` (5-value enum
including `Retryable`/`Quarantined`), `AttemptCount`, `NextAttemptAtUtc`,
`LockedBy`, `LockExpiresAtUtc`. **Phase (a) adopts the identity/integrity
columns, defers the retry/lease columns to phase (b)**:

Adopted now: `MessageId` (uniqueidentifier PK), `Producer`, `EventType`,
`SchemaVersion`, `RoutingKey`, `ContentType`, `OccurredAtUtc`,
`StoredAtUtc`, envelope bytes + `EnvelopeSha256` (integrity verification —
the relay publishes exactly the stored bytes and checks them against the
digest before every attempt, per the book, regardless of retry phase),
`ProcessedAtUtc` (nullable — phase (a)'s reduced stand-in for the book's
full `State` enum: null means pending, non-null means dispatched; no
`InFlight`/`Retryable`/`Quarantined` distinction yet).

Deferred to phase (b): `State` enum's fuller values, `AttemptCount`,
`NextAttemptAtUtc`, `LockedBy`, `LockExpiresAtUtc`, the lease-based
`UPDATE...OUTPUT` claiming pattern (meaningless with a single
`ModularRuntime` instance, which is all phase (a) runs), the deterministic
per-message jittered backoff. Single-instance phase (a) polling is a
simple "select unprocessed, publish, mark processed" loop — safe because
nothing else is contending for the same rows yet.

### Inbox table — adopted close to verbatim, not retry-scoped

The book's `messaging.InboxReceipt` (Executable Asset 28-F, p. 622) has no
retry-specific columns — dedup is phase (a)'s core need, not phase (b)'s.
Adopted as-is: composite PK `(ConsumerName, MessageId)`, `Producer`,
`EventType`, `SchemaVersion`, `EnvelopeSha256`, `OccurredAtUtc`,
`ReceivedAtUtc`, `CompletedAtUtc` (check constraint `CompletedAtUtc >=
ReceivedAtUtc`), `CorrelationId`.

**Dedup algorithm adopted as specified** (ch. 28, pp. 619–630): a fast
pre-read by `(ConsumerName, MessageId)` for the common-case early exit, but
the **database primary key is the authoritative dedup decision**, not the
pre-read — because two concurrent first-deliveries can both miss the
pre-read. Business logic runs, stages the owned aggregate's changes plus
the inbox receipt row, commits once. A PK-conflict on that commit means a
concurrent delivery won; a fresh `DbContext` re-reads the now-committed
receipt rather than continuing with the losing transaction's tracked
objects. The broker ack happens only after a successful commit or a
resolved duplicate — an ambiguous outcome does not ack, allowing
redelivery rather than risking a silent drop.

### Retry policy — deferred to phase (b), numbers recorded for later

The book gives RootCause's actual subscription policy (`rc-alarm-read-v1`:
5 attempts / 15-minute budget, equal-jitter bounded backoff, broker
delivery-limit 8) and dedicated `RetryTicket`/`PoisonMessage` tables (ch.
29, pp. 645–650). None of this is built in phase (a) — recorded here so
phase (b) starts from the book's actual numbers rather than re-researching
them.

## Consequences

- Phase (a)'s outbox table will need a migration in phase (b) to add the
  retry/lease columns — an additive schema change, not a rewrite, since
  the identity/integrity columns already match the book's final shape.
- The inbox table needs no phase (b) migration — it was already adopted at
  (close to) its final shape, since dedup isn't a retry concern.
- Single-instance outbox polling (no lease claiming) is only safe as long
  as exactly one `ModularRuntime` instance runs. Scaling to multiple
  instances without first building phase (b)'s lease mechanism would risk
  duplicate publishes — not a phase (a) concern today, but a real
  constraint worth stating plainly rather than leaving implicit.

## Rejected alternatives

- **Build the full book-specified outbox/inbox/retry design in one pass**,
  ignoring the user's phase split since the book presents it as one
  integrated design. Rejected: the user's phase split is a deliberate
  engineering-sequencing decision independent of how the book organizes
  its chapters; a working, proven basic pub/sub is worth checkpointing
  before adding resilience hardening on top of it, matching this
  project's discipline of small verifiable steps at every prior stage.
- **Use one of the two available JCS NuGet packages.** Rejected: both are
  low-adoption, unverified community packages — a canonicalization routine
  that every message's integrity digest depends on warrants either a
  well-established dependency or code this project can read and verify
  itself.

## Reversal condition

Phase (b) revisits the outbox table (add retry/lease columns), builds the
`RetryTicket`/`PoisonMessage` tables, wires the broker-level DLQ policy and
`<queue>.dead` queues, and implements the lease-based multi-instance
claiming pattern — all using the book's numbers already recorded above,
not re-researched.

## Evidence required

- Real RabbitMQ management API output showing the exchange, queue, and
  binding actually exist with the book's exact names — not just code that
  compiles against the right constant strings.
- A test proving outbox transactionality: a simulated post-commit publish
  failure leaves the outbox row unprocessed (`ProcessedAtUtc` still null),
  not lost.
- A test proving inbox idempotency: duplicate delivery of the same
  `MessageId` does not re-invoke the command handler a second time.
- A real end-to-end run: both hosts live against the real broker and real
  databases, an `AlarmFloodDetectedV1` message actually published and
  actually consumed, RootCause's reaction (opening an analysis) verified
  against its own database, not inferred from logs alone.
