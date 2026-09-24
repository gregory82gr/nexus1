# Evidence — second fixed incident EVT-2026-0419 (EMI artefact)

**Date:** 2026-09-24
**ADR:** ADR-037
**Scope:** Add EVT-2026-0419 (the EMI measurement artefact, `From Flood to Cause`
Ch.5/Ch.10) as a second seeded incident, exercising the artefact edge kind and
telemetry-silence-as-evidence for the first time. Three genuine engine changes
(artefact-edge walking, silence-as-corroboration, rejected-edges-as-witness-map) +
three contained generalizations (incident registry, corpus scoping key, per-incident
seed guard). No production frontend change; UI stays hardcoded to 0418.

## Source confirmation (fresh read of Ch.5 / Ch.10)

`pdftotext -layout` of `From_Flood_to_Cause`: BKR-2A closes **17:42:10**; EMI couples
into FT-9 (+18%), LT-3, PT-7 **at the same instant**; witnesses **RTD T-avg** and
**neutron flux** stay flat; FT-9 self-clears **+8 s**. Ranking (Ch.10 p.29): artefact
**0.70**, FT-9 **0.16**, genuine excursion **ruled out 0.10**. Artefact edges from the
transient to the three transmitters; struck-grey (rejected) links to the flat
witnesses. All numbers matched the approved plan. The genuine-excursion node has **no
tag in the book** — modelled as a distinct ruled-out node `LOF-1` per the approved
decision (ADR-037).

## Built

| Piece | Location |
|---|---|
| ADR-037 | docs/adr/ADR-037-second-fixed-incident-emi-artefact.md |
| `IncidentRegistry` (id → IncidentContext) | Application/Diagnosis/IncidentRegistry.cs |
| `IncidentContext` gains QueryText + CorpusVersion; `FixedIncident` deleted | Application/Diagnosis |
| Both route handlers do a registry lookup (unknown/0420 → 404) | Application/Diagnosis |
| Runner reads ctx.QueryText/ctx.CorpusVersion (constants removed) | FixedIncidentDiagnosisRunner.cs |
| Walker: coverage over backbone ∪ artefact | Infrastructure/Diagnosis/EfGraphWalker.cs |
| Corroborator: artefact mode (silence) + unit scoping | Infrastructure/Diagnosis/EfTelemetryCorroborator.cs |
| `CorpusChunk.UnitId` + config + retriever filter + migration | Domain/Infra/Migrations |
| GroundingSeed: 0419 rows, per-unit seed guard, corpus UnitId | Infrastructure/Diagnosis/GroundingSeed.cs |
| 0419 component tests | tests/…ComponentTests/GroundingDiagnosis0419Tests.cs |

## Gates

- `dotnet build Nexus1.Runtime.sln` → **0 warnings / 0 errors**.
- `Nexus1.ArchitectureTests` → **8/8** (dependency law intact).
- `Nexus1.RootCause.UnitTests` → **35/35** (runner tests updated for the new context shape).
- `Nexus1.RootCause.ComponentTests` (deterministic, `!~Ollama`) → **55/55**, incl. the
  **7 new** `GroundingDiagnosis0419Tests` (run alone: 7/7, 0 skipped).
- EF migration `AddCorpusUnitId` generated, hand-edited to backfill the pre-existing
  0418 corpus to UnitId 1 (add nullable → UPDATE → NOT NULL, no lingering default),
  applied to dev RootCauseDb.

## Engine verdict correctness (component tests, LocalDB, no mocks)

`GroundingDiagnosis0419Tests` (7/7):
- **Walker names BKR-2A as origin via artefact-edge coverage** — not FT-9, not
  FV-104; ranking artefact **0.70** / FT-9 **0.16** (proximate) / LOF-1 **0.10**
  (ruled-out); BKR-2A coverage > FT-9 coverage (from the data).
- **Corroborator confirms via witness silence** (`TimingFits`, detail "…stayed flat").
- **Silence is load-bearing (fault injection):** seed a deflection on the neutron-flux
  witness → corroboration **flips to fail** ("witness … deflected … real excursion,
  not an artefact"). Also: delete a coupled channel's deflection → fail ("no
  deflection").
- **Full deterministic pipeline** reaches the **BKR-2A** verdict, sealed, CorpusVersion
  `evt-2026-0419-book-worked-example-v1`.
- **No cross-contamination (unit-scoped retrieval):** 0419 grounds only on Ch.5/Ch.10
  corpus; 0418 grounds only on its own (no Ch.5).
- **Graph reader** returns the 7-node / 5-edge (3 artefact + 2 rejected) topology.

## API == DB (0419 graph route)

`GET /api/v1/root-cause/incidents/EVT-2026-0419/graph` via BFF → **HTTP 200**, 7 nodes
(BKR-2A, FT-9, LT-3, PT-7, RTD-1, NFX-1, LOF-1), 3 artefact + 2 rejected edges. Edge
list (API) vs direct SELECT (DB) — **identical**:
```
BKR-2A -> FT-9   artefact [0,0]      LOF-1 -> RTD-1  rejected [0,0]
BKR-2A -> LT-3   artefact [0,0]      LOF-1 -> NFX-1  rejected [0,0]
BKR-2A -> PT-7   artefact [0,0]
```
DB (unit 2): 7 components, 2 corpus chunks (both embedded), edges `artefact n=3,
rejected n=2`.

## Full real pipeline over HTTP (BFF → Host → real Ollama → H3/H4 → audit seal)

`POST …/EVT-2026-0419/diagnoses` → **HTTP 200, 122.3 s (cold model load)**:
```
verdict: BKR-2A   abstained: false   auditHash: 15eac95fdd35e779…
candidates: BKR-2A origin w0.70 cov1.0 | FT-9 proximate w0.16 cov0.4 | LOF-1 ruled-out w0.10 cov0.0
citations:  Ch.5, Ch.10 (artefact) | Ch.5 (silence as evidence)   [0419 corpus only]
```
The real served model explained the artefact origin, passed H3/H4 (abstained=false),
and the run was sealed. 122 s = a real cold inference, not a mock.

## No cross-contamination (proven both directions, live)

- 0419 run cited **only** the 0419 corpus (Ch.5/Ch.10). 
- `POST …/EVT-2026-0418/diagnoses` → **HTTP 200, FV-104**, citing **only** the 0418
  corpus (Ch.1-2, Ch.2); **zero** Ch.5 citations. 0418 is unchanged by the artefact
  work.

## Determinism (H6, honest boundary)

Two 0419 runs → engine **verdict identical (BKR-2A)** both times. Audit hashes differ
(`15eac95fdd35…` vs `fa37e575a6dd…`) — the hash seals the model's **narration**, which
can vary on CPU multi-threaded float reduction even at temperature 0 (the project's
standing honest boundary; the engine verdict is the true H6 guarantee). Warm re-run
**29.4 s** vs cold **122.3 s**.

## 404 preserved

- `GET …/EVT-9999-9999/graph` → **404**.
- `GET …/EVT-2026-0420/graph` → **404**, `POST …/EVT-2026-0420/diagnoses` → **404**
  (0420 is deliberately unregistered — it needs the deferred AnalysisStatus third
  state; absence from the registry is the refusal).

## Honest boundaries / what did NOT run

- **Live E2E is API-level, not browser.** The Angular Root Cause / Incident Analysis
  screens remain hardcoded to EVT-2026-0418 (UI selector out of scope, ADR-037); 0419
  is reachable only via the API, proven by the HTTP runs above — there is no browser
  cross-screen test for 0419, and none is implied.
- **`OllamaExplainPipelineTests` (the 0418 real-model component test class) was not
  re-run in this slice.** It compiles against the new context shape; the real-model
  path is instead proven live here for **both** incidents (0418 → FV-104, 0419 →
  BKR-2A) through the actual Host. Stated plainly rather than claimed as run.
- Dev stack (RabbitMQ, RootCause.Host, BFF) stopped after evidence; Ollama left
  running (not started by this slice).

## Scope boundary honoured

No incident-selector UI; no change to `RootCauseAnalysis`/`AnalysisStatus`; no work on
0420; no new tables beyond the `Corpus.UnitId` column.
