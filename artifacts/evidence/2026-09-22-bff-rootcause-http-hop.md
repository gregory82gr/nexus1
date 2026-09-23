# Evidence — BFF→RootCause HTTP hop

**Date:** 2026-09-22
**ADR:** ADR-035
**Scope:** `Nexus1.Bff` reaches the EVT-2026-0418 diagnosis pipeline on
`Nexus1.RootCause.Host` over a real network hop and relays the result. Backend-only
(no Angular change). The console's gateway can now reach the pipeline.

## Built

| Piece | Location |
|---|---|
| Typed `HttpClient` "RootCauseHost" (IHttpClientFactory, base = `Services:RootCauseHost`, 180s timeout) | `src/Hosts/Nexus1.Bff/Program.cs` |
| Proxy route `POST /api/v1/root-cause/incidents/{incidentId}/diagnoses` (raw pass-through; 502 on unreachable) | `src/Hosts/Nexus1.Bff/Program.cs` |
| `Services:RootCauseHost` config | `src/Hosts/Nexus1.Bff/appsettings.json` |
| ADR-035 | `docs/adr/` |

## Gate results

- `dotnet build Nexus1.Runtime.sln` → **0 warnings / 0 errors**.
- `Nexus1.ArchitectureTests` → **8/8**.
- **Structural isolation (inspection):** the BFF csproj has **0** references matching
  RootCause/Explain, and its appsettings has **0** RootCauseDb connection strings —
  the BFF has no way to run the pipeline itself; it can only proxy.

## Live evidence (two backends: BFF on :5103, RootCause.Host on :5102)

### 1. Real hop → same result as at the Host
`curl -X POST http://localhost:5103/api/v1/root-cause/incidents/EVT-2026-0418/diagnoses`
(the **BFF's** own base URL, the same origin the Angular console uses) → **HTTP 200
in 25.1 s** (warm). Body: `verdict=FV-104`, the five candidates (FV-104 origin
0.66/0.786, PB-2A proximate, RCP-1B parallel, BUS-2A contributing, FT-7 ruled-out),
citations naming *"From Flood to Cause … worked example"*, `diagnosisRunId=4`,
`auditHash=470f62f95bfc2dc4c0ac3a1ad57af395dd049183e00e7981ee3d050c7c050f20`.

### 2. DB proof the real Host pipeline ran via the hop
After the BFF call, RootCauseDb:
```
newest audit: seq=4  hash=470f62f95bfc2dc4…  prev=40a4dbf847a4d3b8…
newest run:   run=4  verdict=FV-104
tail chain:   seq4.PrevHash == seq3.Hash  -> TAIL CHAINED OK
```
The newest audit `Hash` **equals the auditHash in the BFF's response**, and a new
`DiagnosisRun` (id 4) was created, chained onto the previous entry. The BFF has no
RootCauseDb access, so only the real Host pipeline could have written and sealed
this — proving the hop, not a reimplementation.

### 3. Isolation proof (the killer): Host down → BFF 502
Stopped `Nexus1.RootCause.Host` (its `/health` then returns nothing), then called the
BFF → **HTTP 502 in 4.1 s**, body `{"title":"RootCause.Host is unreachable","status":502,
"detail":"No connection could be made because the target machine actively refused it.
(localhost:5102)"}`. This is the BFF's **own** 502 (distinct from a relayed 503) —
a reimplementation would still have answered; a real proxy cannot.

### 4. Pass-through of a relayed Host 503 (bonus)
An earlier cold call surfaced the Host's own 503 relayed verbatim: **HTTP 503 in
102 s**, body title *"Diagnosis pipeline could not run"* (the **Host's** 503 title,
not the BFF's "unreachable") with detail *"HttpClient.Timeout of 100 seconds
elapsing"*. So the BFF relays the Host's 503 verbatim, distinct from its own 502.

### 5. Latency correlation
BFF-observed **25.1 s** (warm) tracks the Host's warm latency (~21 s at the Host
level, ADR-034 evidence) plus negligible proxy overhead — confirming the BFF
genuinely waited on the downstream call.

## Honest finding — a Host-side edge surfaced (NOT a BFF issue, NOT fixed here)

The first (cold) call failed at **~102 s** because `Nexus1.RootCause.Host`'s own
Semantic-Kernel/OllamaSharp `HttpClient` to Ollama uses the **default 100 s
timeout**, and a **cold** qwen2.5:3b CPU load + inference sits right at that boundary
(93.6 s succeeded in the ADR-034 run; this cold run exceeded 100 s and the Host
returned 503). The BFF behaved correctly — it relayed the Host's 503. This is a
latent Host-side (ADR-033) robustness edge, surfaced by exercising the pipeline
again: the model-call timeout should be raised above worst-case cold inference, or
the model kept warm. **Deliberately not fixed in this BFF-only slice** — recorded as
a follow-up for a RootCause.Host change.

## Operational notes

- Both hosts run over HTTP: BFF `http://localhost:5103` (the Angular client's
  `BFF_BASE_URL`), RootCause.Host `http://localhost:5102` (`Services:RootCauseHost`).
- The browser only ever talks to the BFF; RootCause.Host is never browser-exposed.
- Dev dependencies left running: portable RabbitMQ, Ollama tray server. Both host
  instances were stopped after the run.

## Scope boundary (unchanged from ADR-035)

No Angular UI/component or API-client method (Ch.29 screen deferred); no second
incident; no change to RootCause.Host's own route; no auth/caching/retry beyond the
timeout. Just: the BFF, as the console's gateway, reaches the pipeline over the
network and relays the real result.
