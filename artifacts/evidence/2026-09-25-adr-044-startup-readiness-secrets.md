# Evidence — ADR-044: broker-independent startup, honest readiness, relocated secrets, no error-detail leakage

Date: 2026-09-25. Branch `v1.0.0`, uncommitted at time of writing.
Everything below was actually run in this environment; outputs are quoted, not summarised.
Method (the investigation's own): run a host on a spare port (RootCause.Host :5112,
ModularRuntime :5111, BFF :5113) from its built DLL with **environment-variable overrides
only**, break one dependency at a time, and observe. The probe script lives in the session
scratchpad, not the repo.

## What changed

| Area | Files |
|---|---|
| Broker-independent startup | `BuildingBlocks.Messaging/RabbitMqConnectionManager.cs` (background connect loop), `ConsumerStartup.cs` (new, shared retry helper), `RabbitMqFailure.cs` (new, auth classification), `.csproj` (+Logging.Abstractions, pinned centrally at the already-resolved 8.0.2); the four consumers (RootCause, Audit, Compliance, Reporting) rewired — **+19/−7 each ignoring whitespace**: the wrapper, one `using`, `EnsureAsync` takes the start token; handler and ack/nack logic untouched |
| Readiness | `ServiceDefaults/HealthChecksBuilderExtensions.cs` (new, generic keyed-context check), `RootCause.Explain/OllamaReachabilityHealthCheck.cs` (new), `RootCause.Infrastructure/ServiceCollectionExtensions.cs` (named key constant + shared migrations-history-table constant), `RootCause.Host/Program.cs` |
| Secrets | three host `appsettings.json` (`ConnectionStrings` removed), `Nexus1.Bff.csproj` (+`UserSecretsId`), three runbooks, `ArchitectureTests/NoSecretsInTrackedConfigTests.cs` (new) |
| Error detail | `RootCause.Host/Program.cs` (diagnosis 503), `Bff/Program.cs` (two proxy 502s) |
| Tests | `RootCause.UnitTests/ConsumerStartupTests.cs` (6), `RootCause.ComponentTests/ReadinessChecksTests.cs` (7) |

## 1. Startup without a broker

**Before (today's code, broker down):**
```
=== BEFORE RootCause.Host broker down at startup
  host never served /health/live (exited=True, exit code=-532462766) after 16.1s
  Unhandled exception. RabbitMQ.Client.Exceptions.BrokerUnreachableException: None of the specified endpoints were reachable
=== BEFORE ModularRuntime broker down at startup
  host never served /health/live (exited=True, ...) after 9.7s
  Unhandled exception. RabbitMQ.Client.Exceptions.BrokerUnreachableException: None of the specified endpoints were reachable
```
The ModularRuntime crash expectation is **confirmed**.

(My first "before" attempt reported "exited=False after 272 s": the scratch probe redirected
stdout without draining it, so the crashing host blocked on a full pipe. The probe was fixed
to write host output to files; the captures above are from the fixed probe.)

**After (broker down, both hosts started together, 170 s hold):**
```
=== AFTER RootCause.Host broker down then started
  /health/live first 200 after 6s
=== AFTER ModularRuntime broker down then started
  /health/live first 200 after 11.6s
  GET /health/ready -> 200
RabbitMQ unreachable at localhost:5672 (connect attempt 1); retrying in 00:00:01.
RabbitMQ unreachable at localhost:5672 (connect attempt 2); retrying in 00:00:02.
RabbitMQ unreachable at localhost:5672 (connect attempt 3); retrying in 00:00:04.
RabbitMQ unreachable at localhost:5672 (connect attempt 4); retrying in 00:00:08.
RabbitMQ unreachable at localhost:5672 (connect attempt 5); retrying in 00:00:16.
```
(RootCause.Host's readiness was 503 in this run — **not** because of the broker; see §2's
migrations-history finding, fixed afterwards. ModularRuntime's readiness was 200 throughout.)

**Real recovery — broker started mid-hold, RabbitMQ management API:**
```
management API up at 10:49:51
  rootcause.alarm-events.v1: consumers=1 type=quorum state=running      (10:50:14)
Connected to RabbitMQ at localhost:5672 on attempt 9.
Consumer rootcause.alarm-events.v1 started on attempt 1.
--- at 10:50:32:
  rootcause.alarm-events.v1: consumers=1
  audit.root-cause-verdicts.v1: consumers=1
  compliance.root-cause-verdicts.v1: consumers=1
  reporting.integration-events.v1: consumers=1
Connected to RabbitMQ at localhost:5672 on attempt 9.            (ModularRuntime)
Consumer audit.root-cause-verdicts.v1 started on attempt 1.
Consumer compliance.root-cause-verdicts.v1 started on attempt 1.
Consumer reporting.integration-events.v1 started on attempt 1.
  process alive after hold: True   (both hosts)
```

**Broker stops after startup — unchanged:**
```
consumers attached before the stop: all four queues consumers=1
both hosts holding; stopping broker at 11:01:18
=== F2 RootCause.Host   ... after hold: live 200, ready 200, process alive: True
=== F2 ModularRuntime   ... after hold: live 200, ready 200, process alive: True
```

**Incidental real-world race (E2E stack start):** RootCause.Host was launched while RabbitMQ
was still booting — `connect attempt 1 … 2`, then `Connected to RabbitMQ … on attempt 3`,
`Consumer rootcause.alarm-events.v1 started on attempt 1`. The old code would have crashed.

## 2. Readiness (RootCause.Host)

**Pre-existing divergence found live, fixed.** The first after-run returned 503; the host's
own log named the check:
```
Health check rootcause-explain-db with status Unhealthy completed after 301.8801ms with message
'RootCauseDbContext's database is reachable but missing 10 migration(s): 20260815053325_InitialRootCauseSchema, ...
```
Cause: the keyed read-only context (`AddRootCauseReadOnlyRetrieval`, ADR-034) used a bare
`UseSqlServer`, without the `MigrationsHistoryTable("__EFMigrationsHistory_RootCause")` the
read-write registration uses — it looked for the default `__EFMigrationsHistory`, which
RootCauseDb does not have (`sys.tables` shows only `dbo.__EFMigrationsHistory_RootCause`).
My first component test had passed because the test base class migrates into the default
table. The corrected test gives its database the real layout (`sp_rename` to the custom
name) and was **red on the old registration**:
```
Failed ReadinessChecksTests.Explain_connection_check_is_healthy_against_the_real_migrations_history_layout
   RootCauseDbContext's database is reachable but missing 10 migration(s): 20260815053325_InitialRootCauseSchema, ...
```
then green after both registrations shared one constant.

**Scenarios on the final code** (failing check named by the host's own health-check log):

| Scenario | `/health/ready` | Named in log |
|---|---|---|
| R1 all healthy | **200** | no unhealthy check (also proves the migrations check under `nexus1_explain`) |
| R2 explain connection unreachable | **503** | `rootcause-explain-db … 'Cannot connect to RootCauseDbContext…'` |
| R3 Ollama unreachable | **503** | `ollama … 'Ollama did not answer /api/tags within 2s.'` |
| R4 Ollama up, chat model missing | **503** | `ollama … 'Ollama reachable but model(s) not installed: nexus-dslm-missing.'` |
| R5 model unloaded, server reachable | **200** | none |

**Reachable is not warm (R5):**
```
ollama stop nexus-dslm
ollama ps (models resident in memory):
    NAME    ID    SIZE    PROCESSOR    CONTEXT    UNTIL          <- nothing loaded
=== R5 model unloaded but server reachable
  GET /health/ready -> 200 in 232ms body=[Healthy]
ollama ps after the readiness probe:
    NAME    ID    SIZE    PROCESSOR    CONTEXT    UNTIL          <- still nothing loaded
```

**A regression I introduced and fixed within the slice:** an intermediate version filtered
`TaskCanceledException` on the caller's token; the framework's own 2 s registration timeout
cancels that token, so the check surfaced as `Health check ollama threw an unhandled
exception after 2016ms` (still Unhealthy, worse message). Fixed by catching the timeout
unconditionally and giving the registration a 3 s budget (one second over the check's own
2 s); re-run live → `'Ollama did not answer /api/tags within 2s.'`; pinned by the test
`Ollama_check_reports_its_own_timeout_reason_when_the_server_never_answers`. On Windows a
refused loopback connect spends ~2 s in SYN retries, so a refused port reads as the timeout.

## 3. Secrets

**Guard red before, green after:**
```
Failed NoSecretsInTrackedConfigTests.No_appsettings_file_under_src_contains_a_password
   Plaintext password(s) found in tracked configuration -- move the connection string to User Secrets (ADR-044):
   Hosts\Nexus1.Bff\appsettings.json, Hosts\Nexus1.ModularRuntime\appsettings.json, Hosts\Nexus1.RootCause.Host\appsettings.json
...
Passed!  - Failed: 0, Passed: 9, Skipped: 0, Total: 9 - Nexus1.ArchitectureTests.dll
```

**Relocated unchanged** (values piped from the tracked file straight into
`dotnet user-secrets set`, never printed; BFF got `UserSecretsId` via `dotnet user-secrets init`):
```
Nexus1.Bff:            tracked 6 keys fp=d4ecb852b3bb | user-secrets 6 keys fp=d4ecb852b3bb | identical=True
Nexus1.ModularRuntime: tracked 6 keys fp=d4ecb852b3bb | user-secrets 6 keys fp=d4ecb852b3bb | identical=True
Nexus1.RootCause.Host: tracked 2 keys fp=751bc77c04ad | user-secrets 2 keys fp=751bc77c04ad | identical=True
```
No rotation; the LocalDB logins are untouched.

**Scope was wider than planned — found by a value-based scan.** Scanning every tracked file
for the two password values (counts only) found them in **three** runbooks, not two: the two
`CREATE LOGIN` statements, a `sqlcmd -P` argument in
`local-rootcause-diagnosis-provisioning.md`, an example connection string, and two prose
mentions. All replaced with placeholders plus the `user-secrets set` commands.
(Two of my own read commands printed the explain-login value in tool output while I was
locating these lines — a masking pattern that missed prose/code-fence forms; it is the same
value already in git history, and every later look went through value-masking.)

**After:**
```
git grep 'Password=' in tracked appsettings*.json:  (none)
git grep 'Password=' in runbooks: only the two placeholder `dotnet user-secrets set` commands
value-based scan: 2202 tracked+untracked-not-ignored files, password-value occurrences = 0
built copies (bin/.../appsettings.json) containing Password=: 0, 0, 0
```
**Hosts start from User Secrets:** RootCause.Host readiness 200 (both SQL logins resolved
from secrets — R1), ModularRuntime readiness 200 (its six), and the E2E stack's BFF readiness
200 with four composed contexts.

Stated plainly: the old values remain in git history; history was not rewritten. RabbitMQ's
`guest`/`guest` dev credentials are unchanged in `appsettings.json` (out of scope).

## 4. Error detail

**RootCause.Host diagnosis 503 (Ollama unreachable):**
```
POST diagnosis 0418 -> 503 body=[{"type":"https://tools.ietf.org/html/rfc9110#section-15.6.4","title":"Diagnosis pipeline could not run",
  "status":503,"detail":"A dependency of the diagnosis pipeline is unavailable. The cause is recorded in the server log under this traceId.",
  "traceId":"4b1eb1800293b23b8b83a704455fb23f"}]
server log:
  Diagnosis pipeline failed for EVT-2026-0418 (traceId 4b1eb1800293b23b8b83a704455fb23f)
  System.Net.Http.HttpRequestException: No connection could be made because the target machine actively refused it. (127.0.0.1:11999)
```

**BFF proxies, RootCause.Host unreachable (localhost:5199):**
```
POST diagnosis via BFF -> 502 {"title":"RootCause.Host is unreachable","status":502,
  "detail":"The root-cause service could not be reached. The cause is recorded in the server log under this traceId.","traceId":"47cfa50a9597933ac4b27d1cdb0a8a79"}
GET graph via BFF      -> 502 {... same detail ...,"traceId":"463fbf8e36b63ae28427b41c66686698"}
BFF log:
  RootCause.Host unreachable for EVT-2026-0418 (traceId 47cfa50a9597933ac4b27d1cdb0a8a79)
  System.Net.Http.HttpRequestException: ... actively refused it. (localhost:5199)
  RootCause.Host graph unreachable for EVT-2026-0418 (traceId 463fbf8e36b63ae28427b41c66686698)
  System.Net.Http.HttpRequestException: ... actively refused it. (localhost:5199)
```
`grep 'detail: ex.Message' src/Hosts/*/Program.cs` → none left.

## 5. New tests

- `ConsumerStartupTests` (6): retries until success and disposes each failed channel; retries
  when opening the channel fails; stops cleanly on shutdown while the broker never appears
  (returns, no throw); stops cleanly during a 5-minute backoff; auth failure logged as a
  credential problem (Error), not an outage; failure classification (AMQP auth anywhere in
  the chain, management 401/403 yes; socket outage, 503 no).
- `ReadinessChecksTests` (7): explain check Healthy on the real history layout; Unhealthy when
  unreachable; checks the **keyed** connection, not the primary (primary Healthy, keyed
  Unhealthy); Ollama Healthy with both models and "NOT a warmth check" in the description;
  Unhealthy on a missing model (a prefix-sharing `nexus-dslm-old` does not count); Unhealthy
  on refused; own timeout reason when the server never answers.

## 6. Regression — every assembly in isolation, sequentially

```
dotnet build Nexus1.Runtime.sln -c Debug -> 0 Warning(s) 0 Error(s)
assemblies passed: 36  failed: 0
Nexus1.ArchitectureTests         9/9   (8 + the new guard)
Nexus1.RootCause.UnitTests       45/45 (39 + 6)
Nexus1.RootCause.ComponentTests  93 total: 92 passed, 1 skipped (trap-leading-question, named)  (86 + 7)
Audit / Compliance / Reporting ComponentTests 13/13, 13/13, 19/19; ServiceDefaults.ComponentTests 3/3
```
H9 inside that run: goldens and traps unchanged; `golden-0419-live VERDICT BKR-2A
auditHash=811547bad5fa…`, `golden-0418-live VERDICT FV-104 auditHash=5c0ad35cd3d3…` — the
same sealed hashes as the ADR-042 and ADR-043 slices. Pre-existing empty projects
`Nexus1.Contracts.ContractTests` and `Nexus1.DistributedSlice.EndToEndTests` unchanged.

**Live @slow E2E (full stack, all credentials from User Secrets):**
```
ok 1 root-cause-cross-screen.e2e.ts › root cause agreement @slow … (29.3s)
ok 2 root-cause-cross-screen.spec-of-specs.e2e.ts › root cause agreement is load-bearing @slow … (26.6s)
2 passed (59.4s)
```

## Cleanup

All probe hosts and the E2E stack stopped; RabbitMQ stopped (as before the slice); only
Ollama (pre-existing) listening; 0 leftover `*Tests_*` databases. User Secrets stores for the
three hosts now hold the (unchanged) connection strings — the intended relocation.
