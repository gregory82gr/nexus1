# ADR-044: Broker-independent startup, honest readiness, relocated secrets, no error-detail leakage

## Status

Accepted. Fixes four real defects found by the Kubernetes-readiness investigation, as their
own slice and independent of any Kubernetes decision (no manifests, no container image).

## Context

Found by running RootCause.Host on a spare port with one dependency broken at a time:

1. **Startup crash without a broker.** `RabbitMqConnectionManager` opened its connection in
   its constructor. With RabbitMQ unreachable, RootCause.Host crashed with an unhandled
   `BrokerUnreachableException` (~13–16 s) and **ModularRuntime crashed the same way (~9 s)**
   — confirmed in this slice's "before" capture — before either served `/health/live`.
   Deferring the connection alone would not fix it: all four consumer `BackgroundService`s
   (RootCause, Audit, Compliance, Reporting) open a channel, declare topology and PUT a
   dead-letter policy at the top of `ExecuteAsync` with no retry, and no host overrides
   .NET 8's default `BackgroundServiceExceptionBehavior.StopHost` — the crash would merely
   move to just after Kestrel starts.
2. **Readiness blind spots.** RootCause.Host's `/health/ready` checked only the `nexus1_app`
   database. With the read-only `nexus1_explain` connection or Ollama unreachable it said
   **200 Healthy** while every diagnosis returned 503.
3. **Committed plaintext passwords.** The `nexus1_app` and `nexus1_explain` passwords sat in
   14 connection strings across the three tracked host `appsettings.json` files (BFF 6,
   ModularRuntime 6, RootCause.Host 2) and in three runbooks (two `CREATE LOGIN` statements,
   a `sqlcmd -P` argument, an example connection string and two prose mentions) — found by a
   value-based scan of every tracked file, which is how the third runbook turned up.
4. **Error-detail leakage.** The diagnosis 503 and the BFF's two RootCause proxy 502s
   returned `detail: ex.Message` — SQL network errors and internal host:port pairs.

## Decision

### 1. Broker-independent startup (shared `BuildingBlocks.Messaging`)

- **`RabbitMqConnectionManager`** no longer connects in its constructor. It starts a
  background connect loop that retries with backoff (`RetryBackoff.ExponentialCap`, 1 s
  doubling to a 30 s cap) and exposes `IsConnected` and `WaitUntilConnectedAsync`.
  `CreateChannel()` throws a clear `InvalidOperationException` until connected — the outbox
  relay and retry dispatchers already catch and loop, unchanged. **Once connected, the
  existing `AutomaticRecoveryEnabled` behaviour is untouched** (broker-down-after-startup
  stays exactly as confirmed correct).
- **`ConsumerStartup.RunWithRetryAsync`** (new, shared): waits for the connection, then runs
  a consumer's whole start sequence (open channel, declare queues, set the dead-letter
  policy, QoS, start consuming) in a retry loop with the same backoff, logging every failed
  attempt; it disposes a half-started channel before retrying and exits only on host
  shutdown. Four identical copies of that sequence meet this project's
  duplication-until-proven bar for the shared kernel. All four consumers use it.
- **Authentication failures get their own message** (connect loop and consumer start): an
  `AuthenticationFailureException` anywhere in the chain, or a 401/403 from the management
  API, is logged as a credential problem that retrying will not fix without a
  configuration change.
- **Health endpoints stay broker-agnostic.** `/health/live` is 200 while the process runs;
  `/health/ready` never includes the broker (no host HTTP route uses it). Broker-down at
  startup now behaves like broker-down after startup. Broker state is visible in the logs.

**Behaviour change for ModularRuntime (named):** it now **starts and serves its health
endpoints with the broker down, instead of crashing**, and its Audit/Compliance/Reporting
consumers and AlarmManagement outbox relay attach once the broker appears. The trade: it no
longer **fails fast** on a wrong broker host or credentials — it retries and logs (auth
failures distinctly). Pre-existing and unchanged: the outbox relay logs an error every
250 ms while the broker is down.

Rejected: switching `BackgroundServiceExceptionBehavior` to `Ignore` — the host would stay
up with its consumers silently dead forever.

### 2. Readiness: the two missing request-path dependencies (RootCause.Host only)

- **`rootcause-explain-db`** — the unchanged `DbContextHealthCheck<RootCauseDbContext>`,
  resolved against the **keyed** `"readonly"` context (the explain connection is a keyed
  registration of the same type, so a plain `AddCheck<DbContextHealthCheck<…>>` would
  silently re-check `nexus1_app`). A small generic ServiceDefaults helper,
  `AddKeyedDbContextCheck<TContext>`, does the keyed resolution; it knows nothing about
  RootCause. Its pending-migrations query runs under the read-only login
  (`db_datareader` covers `dbo.__EFMigrationsHistory_RootCause`).
- **Pre-existing divergence found by this check, fixed.** The first live run returned 503:
  "reachable but missing 10 migration(s)". The keyed read-only context
  (`AddRootCauseReadOnlyRetrieval`, ADR-034) was built with a bare `UseSqlServer`, without the
  `MigrationsHistoryTable("__EFMigrationsHistory_RootCause")` the read-write registration
  uses — so it looked for the default `__EFMigrationsHistory`, which RootCauseDb does not
  have. Retrieval never reads migration history, so nothing had noticed. Both registrations
  now share one constant. The component tests give their database the real history-table
  layout (the test base class migrates into the default table, which is why the first
  version of the test passed while production failed); the corrected test was red on the
  old registration, green on the fix.
- **`ollama`** — `OllamaReachabilityHealthCheck` in `Nexus1.RootCause.Explain`:
  `GET {Endpoint}/api/tags` with a 2 s timeout, Healthy only if both configured models
  (`nexus-dslm`, `nomic-embed-text`) are listed; Unhealthy (503) on no response, timeout,
  refusal, or a missing model. Its registration budget is 3 s, one second over the check's
  own timeout, so the check always reports its own reason instead of the framework
  cancelling it as an "unhandled exception" (seen once live and fixed). On Windows a refused
  loopback connect takes ~2 s of SYN retries, so it reads as the timeout there.

**Reachable is not warm.** `/api/tags` lists installed models and never loads one; a
Healthy `ollama` check proves the server answers and the models are present, **not** that a
model is resident — a cold first diagnosis can still take 100–150 s. The check's own
description says so.

**Accepted cost:** readiness is per pod, so an Ollama outage also takes the read-only graph
route out of rotation. Diagnosis is this host's primary purpose. No Ollama check is added to
any other host. Both new checks carry their own timeout (5 s explain DB, 2 s Ollama); the
existing `rootcause-db` check is unchanged (it still runs to SqlClient's ~15 s default
connect timeout when SQL is unreachable — named, not changed).

### 3. Secrets relocated, values unchanged

- The `ConnectionStrings` sections are removed from the three tracked `appsettings.json`
  files; the same, **unchanged** values live in each project's User Secrets store
  (`dotnet user-secrets`), which ASP.NET Core loads in the Development environment every
  runbook already uses. ModularRuntime and RootCause.Host already had a `UserSecretsId`; the
  BFF gets one. A missing key still fails fast with a message naming it.
- The three runbooks show placeholders plus the `dotnet user-secrets set` commands.
- **No rotation, no change to the local LocalDB logins** — by decision. Consequence, stated
  plainly: the old values remain in git history; removing them from HEAD does not un-publish
  them. History is not rewritten.
- A guard test (`Nexus1.ArchitectureTests`) fails if any `appsettings*.json` under `src/`
  contains `Password=`.

No production secrets story (Key Vault, Kubernetes Secrets) — not needed yet.

### 4. Error detail stays server-side

RootCause.Host's diagnosis 503 and the BFF's two RootCause proxy 502s return a generic
`detail` plus a `traceId` extension (the W3C trace id of the request's activity, falling back
to the ASP.NET request id). The full exception is still logged server-side by the existing
`LogError` calls, which now also log the same trace id, so an operator can join the response
to the log line. No consumer reads the old detail: the console branches on status only,
Jest specs flush null bodies, the H9 harness never goes through HTTP, and the E2E asserts
the verdict.

## Scope boundary

No Kubernetes manifests, no container image, no change to broker-down-after-startup
behaviour, no broker check in readiness, no Ollama check outside RootCause.Host, no change to
the hard-coded RabbitMQ management port or the `rootcause-db` check's timeout, no fix to the
outbox relay's log noise, no password rotation and no change to the actual LocalDB
credentials.

## Reversal condition

If fail-fast on broker misconfiguration becomes more valuable than availability (e.g. a
deployment that must never run without its consumers), add an opt-in startup gate rather than
restoring the constructor connect. If the graph route needs to stay routable during Ollama
outages, move the `ollama` check to `Degraded`.

## Evidence required

Before/after on both hosts with the broker unreachable; recovery proven by the RabbitMQ
management API showing live consumers on the real queues; broker-down-after-startup re-run;
readiness for explain-DB down, Ollama down, Ollama up with a missing model, a model unloaded
(`ollama stop`) but server reachable, and all healthy; the guard test red then green; hosts
starting from User Secrets; 503/502 bodies without internals while logs keep them under the
same trace id; unit/component tests for the helper, the Ollama check and the keyed check;
build 0/0; every assembly in isolation; H9 deterministic + live; the live @slow E2E once.

See `artifacts/evidence/2026-09-25-adr-044-startup-readiness-secrets.md`.
