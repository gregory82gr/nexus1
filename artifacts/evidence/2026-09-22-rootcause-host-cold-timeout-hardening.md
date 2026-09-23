# Evidence — RootCause.Host cold-inference timeout hardening

**Date:** 2026-09-22
**ADR:** ADR-033 addendum
**Scope:** Raise the Host's chat-model call timeout from the SK/OllamaSharp default
100s to a configurable 150s, so a genuine cold qwen2.5:3b CPU inference (~93–130s)
succeeds instead of hitting the 100s wall. Self-contained hardening slice.

## Built

| Piece | Location |
|---|---|
| `SemanticKernelExplainer` builds a dedicated long-lived `HttpClient` (BaseAddress=Endpoint, Timeout=RequestTimeout) → `AddOllamaChatCompletion(modelId, HttpClient)` overload | `src/Contexts/RootCause/Nexus1.RootCause.Explain/SemanticKernelExplainer.cs` |
| `OllamaOptions.RequestTimeout` (default 150s) | `Explain/ServiceCollectionExtensions.cs` |
| `Ollama:RequestTimeoutSeconds` config read | `Host/Program.cs`, `Host/appsettings.json` |
| `OLLAMA_KEEP_ALIVE` documented as an optional ops lever | `docs/runbooks/local-rootcause-diagnosis-provisioning.md` |
| ADR-033 addendum | `docs/adr/ADR-033-...md` |

Embedder timeout left untouched (nomic calls are sub-second). No BFF change, no route
change, no new incident, no new background service.

## The fix, precisely

Before: `AddOllamaChatCompletion(ChatModelId, Endpoint)` (Uri overload) → OllamaApiClient
over a default `HttpClient` with a **100s** timeout. After: a dedicated `HttpClient`
with `Timeout = 150s` passed to the `AddOllamaChatCompletion(ChatModelId, HttpClient)`
overload. 150s is above the observed 110s cold worst case and **below the BFF's 180s**
(ADR-035), so the Host times out first and returns its own clean 503 (relayed by the
BFF) rather than the BFF cutting off a working Host.

## Gate results

- `dotnet build Nexus1.Runtime.sln` → **0 warnings / 0 errors**.
- `Nexus1.ArchitectureTests` → **8/8**.
- `Nexus1.RootCause.ComponentTests` → **53/53** (change is internal to the explainer
  ctor; the gated Ollama tests still pass).

## Before / after — the same cold scenario

| | Latency | Result |
|---|---|---|
| **Before** (default 100s, from the ADR-035 hop run) | ~102 s | **503** "HttpClient.Timeout of 100 seconds elapsing" |
| **After** — direct Host (:5102), cold | **102.5 s** | **200** verdict=FV-104, runId=5, auditHash 1fa89f19… |
| **After** — via BFF (:5103), cold | **101.3 s** | **200** verdict=FV-104, runId=6, auditHash 4a59ab8d… |

The cold start was genuine: `ollama stop nexus-dslm` unloaded the model
(`ollama ps` showed nexus-dslm absent) before each call, so each paid the full cold
load + inference.

## DB proof the cold calls genuinely ran and sealed

```
run=6 verdict=FV-104   (via BFF, cold)
run=5 verdict=FV-104   (direct Host, cold)
newest audit: seq=6 hash=4a59ab8d31f9f847…  prev=1fa89f19dbab9524…   (= run 5's hash)
tail chain: seq6.PrevHash == seq5.Hash  ->  TAIL CHAINED OK
```
Both cold calls wrote a new DiagnosisRun and a chained Audit entry — not a fast-fail;
the pipeline ran end to end under the new timeout, and the newest audit hash matches
the BFF cold response (4a59ab8d…).

## Optional complementary ops lever (not Host code)

`OLLAMA_KEEP_ALIVE` (e.g. `30m` or `-1`) on the Ollama process keeps the model resident
so cold-starts are rare in practice. It does not eliminate the first cold call after an
Ollama restart — which is why the 150s timeout is the floor. Documented in the
provisioning runbook; not exercised here (the point of this slice is that even a fully
cold call now succeeds).

## Scope boundary

Only the Host chat-model timeout (+ its config knob). No BFF change, no embedder change,
no route/incident change, no new background service, no other timeout touched.
