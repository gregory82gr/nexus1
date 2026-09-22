# ADR-035: The BFF's first outbound hop — proxying to RootCause.Host

## Status

Accepted.

## Context

The EVT-2026-0418 diagnosis pipeline has a real HTTP route on `Nexus1.RootCause.Host`
(ADR-034, commit 7d2856c), but its only consumer so far is curl. The product
consumer is the Angular console, which talks only to `Nexus1.Bff`. Giving the BFF a
way to reach the pipeline is the next honest step (a real consumer before more
domain depth, ADR-005).

RootCause is a **separately deployed service** by design: ADR-001 extracts it to
its own host, ADR-030 deliberately kept it **out** of the BFF's in-process
composition, and ADR-033's H7 isolation (read-only retrieval, no-DB served model)
lives inside that host. The BFF today composes all 17 in-process sectors and makes
**zero** outbound HTTP calls.

## Decision

**The BFF reaches RootCause over HTTP — a real network hop to a second backend —
never by composing RootCause in-process.**

### The boundary, kept

- The BFF gains **no** `ProjectReference` to `Nexus1.RootCause.Application`,
  `.Infrastructure`, or `.Explain`, and **no** RootCauseDb connection string.
  Referencing those would absorb RootCause (and, via Explain, Semantic Kernel +
  Ollama) back into the BFF process, undoing ADR-001/ADR-030/H7. This is asserted
  structurally (the BFF has no way to run the pipeline itself) and is part of the
  evidence.
- The BFF gets a typed `HttpClient` (via `IHttpClientFactory`, named
  `"RootCauseHost"`) whose base address is the configured `Services:RootCauseHost`
  URL, with a **180 s timeout** — generous over the measured 26–94 s CPU-inference
  latency (ADR-034), so the BFF never severs a legitimate slow run.
- This is the BFF's **first outbound hop**. Its other 17-sector routes stay
  in-process and unaffected; RootCause.Host being down degrades **only** this route.
  The browser still talks to a single origin (the BFF); RootCause.Host is never
  browser-exposed (correct CORS/security posture).

### Route — thin proxy, raw pass-through

**`POST /api/v1/root-cause/incidents/{incidentId}/diagnoses`** on the BFF — the
same path as the Host, so it reads as an obvious pass-through. The handler POSTs to
the Host's identical path and **relays the Host's response verbatim** — status code,
body, and content-type. It does **not** deserialize/rebuild the `DiagnosisResponse`
(no shared DTO, no reimplementation risk — a raw relay provably cannot drop or
invent a field).

### Status mapping

- Host responds → **relay its status and body verbatim**: `200` (including a
  first-class abstention body), `404` (unknown incident), relayed `503` (the Host
  ran but its infra, e.g. Ollama, failed).
- BFF **cannot reach** the Host (connection refused, or its own 180 s timeout) →
  **`502 Bad Gateway`**. This is deliberately distinct from a relayed `503`: `502`
  means the hop itself failed; a relayed `503` means the Host itself could not run
  the pipeline. The distinction is diagnostically honest and is the isolation proof
  (Host down → 502).

## Consequences

- The console's gateway can now reach the pipeline; the browser still sees one base
  URL. **No Angular change is needed** for the hop to work (every client already
  targets the single `BFF_BASE_URL`); the Angular API-client method and the Ch.29
  screen that will *consume* this route are a separate, later slice.
- The BFF now has a runtime dependency on RootCause.Host **for this one route only**.
- `Program.cs` stays a composition root; the route lambda is transport only
  (forward the call, relay the response, map unreachable→502).

## Scope boundary — what this does NOT do

No Angular UI/component or API-client method (Ch.29 Root Cause screen deferred); no
second incident (still EVT-2026-0418 only, the Host enforces the 404); no change to
`Nexus1.RootCause.Host`'s own route (reused as committed); no auth, caching, or
retry beyond the timeout.

## Rejected alternatives

- **Compose RootCause in-process in the BFF** — rejected outright; it undoes
  ADR-001/ADR-030/H7 and would pull SK/Ollama into the BFF.
- **Deserialize to a shared DTO and re-serialize** — rejected; introduces a shared
  contract and a reimplementation risk. Raw relay is provably a proxy.
- **Collapse unreachable-Host into 503** — rejected; 502 vs relayed-503 distinguishes
  a failed hop from a Host-side infra failure.
- **Let Angular call RootCause.Host directly** — rejected; it would expose the
  internal service to the browser and break the single-gateway/CORS posture.

## Reversal condition

Revisit if RootCause is ever re-merged into a single deployable (then the hop
becomes an in-process call), if the console needs non-blocking UX (async/poll,
which the persisted DiagnosisRun already supports via GET-by-id), or if the hop
needs auth/resilience (retry/circuit-breaker) beyond a demonstrator.

## Evidence required

- Solution build clean; `ArchitectureTests` 8/8; the BFF has zero RootCause/Explain
  ProjectReferences and no RootCauseDb connection string (inspection).
- A real `curl` POST to the **BFF's** base URL returns the same `verdict=FV-104`,
  citations, and `auditHash` proven at the Host.
- After the BFF call, a new `DiagnosisRun` + a new chained `RootCause.Audit` entry
  exist in RootCauseDb, and the newest audit `Hash` equals the BFF response's
  `auditHash` — proving the real Host pipeline ran via the hop.
- Host stopped → BFF returns `502`.
- BFF-observed latency ≈ Host latency + negligible overhead.

See `artifacts/evidence/2026-09-22-bff-rootcause-http-hop.md`.
