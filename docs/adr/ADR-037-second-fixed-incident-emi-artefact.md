# ADR-037: Second fixed incident EVT-2026-0419 (EMI artefact) — artefact-edge walking, silence-as-corroboration, and the incident registry

## Status

Accepted. Builds on ADR-032 (grounding walking skeleton), ADR-034 (diagnosis query
route), ADR-036 (incident-graph read endpoint). First slice to run the diagnosis
engine on more than one incident.

## Context

Phase-1 RootCause shipped a single fixed incident, **EVT-2026-0418** (a feedwater
cascade), proven end to end. `From Flood to Cause` works **three** case studies
(Ch.10): 0418 the cascade, **0419 the artefact**, 0420 the QA-escape. 0419 is the
one that teaches the engine to rule a real-looking event *out* on the strength of an
**absence** — the opposite reasoning direction from 0418's "channels moved together,
timing fits."

Adding 0419 exercises, for the first time:

- the **`FixedIncident`/404 seam** beyond one incident;
- the **`artefact`** edge kind, which the schema has always carried (`Edge.Kind`) but
  which no seeded data used and — decisively — **no engine code ever walked**;
- **telemetry-silence-as-evidence**: confirming an artefact hypothesis partly
  *because* the independent witness channels stayed flat when a genuine excursion
  would have moved them.

The exact numbers, timing and edge structure below are taken from a fresh read of
Ch.5 ("Telemetry as the Witness") and Ch.10 ("Three Case Studies"), not reused from
0418's Figure 2.1 extraction.

### Source facts (Ch.5 / Ch.10, verified)

- At **17:42:10** switchgear breaker **BKR-2A** closes; a 4kV-bus voltage transient
  couples EMI into three unrelated channels **at the same instant**: **FT-9**
  primary flow **step +18%**, **LT-3** pressuriser level spike, **PT-7** RCS
  pressure spike. Ten alarms in eight seconds, reading as a developing loss-of-flow.
- **Decisive evidence — silence:** the instruments a real excursion must disturb —
  **primary RTD average temperature** and **neutron flux** — **stay flat**. FT-9
  **self-clears to nominal +8 s** once the transient passes.
- **Ranking (Ch.10, p.29):** EMI artefact **0.70** (origin), FT-9 **0.16**
  (proximate), the genuine excursion **ruled out 0.10**.
- **Edges (Ch.5):** dashed-**orange artefact** edges from the transient source into
  the three coupled transmitters; **struck-grey (rejected)** links to the RTD and
  flux nodes — "the corroboration a real excursion would have produced, drawn
  precisely because it is absent."

## Decision

Add EVT-2026-0419 as a second seeded incident on its **own unit** (UnitId 2, "SMR
Module A"), reusing the existing unit-scoping as the isolation seam, and make the
three genuinely-new pieces of engine behaviour real (not schema-only):

### 1. Incident registry (replacing the single `FixedIncident` identity)

`IncidentRegistry` (Application/Diagnosis) maps `IncidentId → IncidentContext`, and
`IncidentContext` gains **`QueryText`** and **`CorpusVersion`** (moved off the
runner's hardcoded constants, which were 0418-specific). Both route handlers
(`RunFixedIncidentDiagnosisCommandHandler`, `GetIncidentGraphQueryHandler`) do a
registry lookup instead of `string.Equals` against one constant; an unknown id is
still a first-class refusal (`Result.Failure` → 404), and **EVT-2026-0420 is not in
the registry, so it still 404s** (it is deliberately unbuilt — see Scope boundary).
`FixedIncident` is deleted; `GroundingSeed` and the handlers read the registry.

### 2. Artefact-edge walking (the one genuinely-new engine rule)

`EfGraphWalker` computed coverage over **`backbone` edges only**, so an artefact
source — which has no backbone cascade — would score coverage 0, no origin would be
named, and the runner would **abstain**, never producing the 0.70 artefact verdict.
The walker now computes coverage over **`backbone` ∪ `artefact`** edges: an artefact
source explains the alarms on the channels it corrupts, exactly as a cascade origin
explains its downstream alarms. `learned` edges stay subordinate (never walked) and
`rejected` edges stay visible-but-never-walked, unchanged. This is a **general
rule** — any artefact-origin incident works — not a special case keyed to 0419.

### 3. Silence-as-corroboration (artefact mode in the corroborator)

`EfTelemetryCorroborator` had one mode: walk the origin's forward **backbone** path
and pass only if every edge's historian onset-gap fits its window; **missing
historian data fails**. It now branches: when the origin has outgoing `artefact`
edges it runs **artefact mode** —

- confirm the **coupled channels** (artefact-edge targets) deflected
  **near-simultaneously** (each onset-gap fits the artefact edge's `[0,0]` window —
  simultaneity that a real hydraulic event could not produce); a coupled channel
  with no deflection *fails* (the artefact story needs them to move);
- confirm the **witness channels** (the targets of the `rejected` edges) **stayed
  flat** — here **absence of a deflection is confirmation**, the inverse of backbone
  mode. A witness that *did* deflect fails: "an independent channel moved, so this is
  a real excursion, not an artefact."

The `ITelemetryCorroborator` interface is unchanged; artefact mode is selected from
the data (does the origin have artefact edges?). While here, the corroborator is
also given the **explicit `UnitId` scoping** it previously lacked (it loaded all
edges/historian globally and worked only by id-disjointness) — a correctness fix.

### 4. Rejected edges as the witness map

In 0418 `rejected` edges are purely visual (a ruled-out candidate, kept visible). In
0419 the rejected edges *to the witnesses* become **load-bearing**: they are the map
the corroborator reads to know which channels must be silent. Same data structure,
new semantics — no schema change.

### 5. Corpus scoping key (a real cross-contamination fix)

`CorpusChunk` gains a **`UnitId`** column (+ migration); `EfRetriever` — which
accepted `unitId` but **ignored it** — now filters by it. Without this, 0419's run
would retrieve and cite 0418's passages (and vice versa). Existing 0418 corpus rows
are backfilled to UnitId 1 in the migration.

### 6. The ruled-out "genuine excursion" node identity (flagged decision)

The book quotes the genuine-excursion hypothesis's weight (0.10) but gives it **no
tag**. Per the approved plan it is modelled as a **distinct ruled-out hypothesis
node**, mirroring FT-7's treatment in 0418 (a node kept visible with role
`ruled-out`), tagged **`LOF-1`** (loss-of-flow) as an honest, clearly-synthetic
label. Its two `rejected` edges run to the RTD and flux witnesses. The RTD and
neutron-flux witness tags (`RTD-1`, `NFX-1`) are likewise lightly formalized from
the book's informal labels ("RTD Primary T-avg", "Flux Neutron power"). All other
tags (BKR-2A, FT-9, LT-3, PT-7) are the book's own.

### Seeded 0419 shape (UnitId 2, component ids 11–17)

| Id | Tag | Role/weight | Alarms | Notes |
|----|-----|-------------|--------|-------|
| 11 | BKR-2A | weight 0.70, role computed→origin | 0 | EMI source (breaker close) |
| 12 | FT-9 | weight 0.16, role proximate | 4 | primary flow, +18% step |
| 13 | LT-3 | non-ranked | 3 | pressuriser level spike |
| 14 | PT-7 | non-ranked | 3 | RCS pressure spike |
| 15 | RTD-1 | non-ranked, role witness | 0 | primary T-avg, flat |
| 16 | NFX-1 | non-ranked, role witness | 0 | neutron flux, flat |
| 17 | LOF-1 | weight 0.10, role ruled-out | 0 | genuine-excursion hypothesis |

Edges: 3× `artefact` (11→12, 11→13, 11→14, window `[0,0]`); 2× `rejected` (17→15,
17→16). Historian: simultaneous t=0 onsets for 11/12/13/14, a +8 s self-clear sample
for FT-9, and **no sample for RTD-1/NFX-1** (the silence). AlarmedComponentIds =
[12,13,14] (total 10 alarms). Corpus: 0419's own book worked-example passages
(Ch.5/Ch.10), UnitId 2.

## Consequences

- The engine names **BKR-2A (EMI artefact) as origin** for 0419 via artefact-edge
  coverage, ranks FT-9 0.16 proximate and LOF-1 0.10 ruled-out, and corroborates on
  witness **silence** — a verdict, not an abstention.
- 0418 is unchanged (no artefact edges in unit 1; walker and corroborator behave
  identically), now proven by re-running it after the change.
- Retrieval is genuinely unit-isolated; a run cites only its own incident's corpus.
- No structural change to `DiagnosisRun`/`Candidate`/`RootCauseAnalysis`; 0419 is new
  rows under the existing shape. The only new column is `Corpus.UnitId`.

## Scope boundary

- **No incident-selector UI.** The Angular Root Cause / Incident Analysis screens
  stay hardcoded to EVT-2026-0418; 0419 is reachable only via the API/E2E. Live
  E2E for this slice is therefore **API-level**, not a browser cross-screen test.
- **No work on EVT-2026-0420** (the QA-escape). 0420 needs the still-deferred
  abstention-capable third `AnalysisStatus` state on the human-owned
  `RootCauseAnalysis` aggregate; 0419 lives entirely in the grounding/`DiagnosisRun`
  world and produces a verdict, touching neither `RootCauseAnalysis` nor
  `AnalysisStatus`. 0420 is not registered, so it 404s.
- **No new tables** beyond the `Corpus.UnitId` column.

## Rejected alternatives

- **Add an `IncidentId` FK across Component/Edge/Historian** — rejected; `UnitId`
  already isolates the physical topology and 0419 is genuinely a different unit. Only
  the corpus needed a scoping key.
- **Special-case artefact handling for this exact incident** — rejected; the walker
  change is the general rule "coverage over backbone ∪ artefact," and the
  corroborator selects artefact mode from the data, so a third artefact incident
  would need no new engine code.
- **A separate corroborator interface for artefact mode** — rejected; the contract
  ("confirm the origin hypothesis against the historian") is unchanged, so the mode
  is an implementation branch, not a new port.
- **Model the excursion as a role on FT-9** — rejected; FT-9 is the proximate
  (0.16); the genuine excursion is a separate ruled-out hypothesis (0.10), so it
  needs its own node to carry its own weight, exactly as FT-7 did in 0418.

## Reversal condition

- **The "one seeded incident per unit" invariant** is the simplifying assumption
  that lets `UnitId` stand in for incident scope on the topology tables. If a future
  seeded incident shares a unit with another, Component/Edge/Historian would then
  need explicit incident scoping (and the corpus key would move from `UnitId` to
  `IncidentId`). Revisit then.
- Revisit the `LOF-1`/`RTD-1`/`NFX-1` synthetic tags if the source later pins down
  real tags for them.

## Evidence required

- **API == DB** for 0419's graph route (nodes/edges/kinds match direct SELECTs).
- **Engine verdict correctness** (component tests, LocalDB): walker names BKR-2A via
  artefact-edge coverage (not FV-104, not FT-9); ranking 0.70/0.16/0.10; corroborator
  confirms via witness silence.
- **Silence is load-bearing** (fault injection): seed a deflection on RTD/flux and
  show corroboration flips/fails.
- **Full real pipeline over HTTP**: POST …/EVT-2026-0419/diagnoses through
  BFF → Host → real Ollama → H3/H4 → audit seal returns the artefact verdict with
  real book citations; H6 re-run gives the same engine verdict.
- **No cross-contamination**: 0419 cites only 0419 corpus; 0418 still returns FV-104
  with its own corpus only.
- **404 preserved** for unknown ids and for 0420.

See `artifacts/evidence/2026-09-24-second-fixed-incident-0419.md`.
