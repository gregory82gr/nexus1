# ADR-043: The unexplained-precursor rule — the engine under a wrong causal model

## Status

Accepted. First model-mismatch probe of the diagnosis engine, recommended in place of a
physics-based generator: one hand-written, test-only fixture where the physical truth
differs from the authored graph in a controlled way.

## Context

ADR-042 made candidate selection observable-derived, but every built case (0418, 0419, the
ADR-042 pure-observable fixture) assumed the authored causal graph is **right**. Real fault
trees are incomplete; a diagnosis engine is only honest if it notices when the evidence says
its model is missing something.

The probe: an instrument-air header, **IA-1**, loses pressure and the air-operated valve it
feeds, **AOV-2**, drifts shut; the cascade then runs as modelled, AOV-2 → LVL-3 → TRP-4. The
graph's maintainers modelled AOV-2's downstream cascade but never its air supply — a classic
missing dependency.

On the engine before this ADR (captured red in the evidence file):

- the walker, seeing no edge from IA-1, names **AOV-2** origin (4 of 5 alarms, weight 0.80)
  and IA-1 `independent` (weight 0.20);
- the corroborator (cascade mode) walks only AOV-2's forward path, whose gaps fit — it
  reports "2 backbone edge(s) fit";
- with a corpus, the pipeline would seal a **confident wrong verdict, AOV-2**, with no
  caveat. (The fixture has no corpus, so the unpatched full pipeline happened to abstain with
  "no grounding" — for the wrong reason, which masks the mismatch.)

The contrary evidence was observable all along: IA-1's onset (t0) precedes AOV-2's (t20),
and nothing explains IA-1. Causes do not run backwards.

## Decision

### The rule

Corroboration fails if any **alarmed** component **outside the origin's reach** has an onset
**strictly earlier** than the origin's onset. The reason names it:

> unexplained precursor: IA-1 began before origin AOV-2 and is not explained by it -- the
> causal graph may be incomplete

- **Observable-only basis:** alarmed set (the incident's), reach over backbone + artefact
  edges (the walker's ADR-042 definition), and historian onsets (the earliest Quality==0
  sample — the corroborator's existing definition). No seeded weight, role or label.
- **Both modes:** applied after the cascade-mode or artefact-mode check passes, so every
  existing failure reason is unchanged.
- **Ties never trigger it.** An equal onset is simultaneity (ambiguous, not precedence); a
  missing onset is no evidence of precedence.
- **No interface or contract change:** the existing `Corroboration(false, detail)` result and
  the runner's existing abstention path ("telemetry corroboration failed: …") carry it.

### Non-regression, from the seed

0418: the alarmed components outside FV-104's reach are RCP-1B (onset 98 s) and BUS-2A
(52 s); FV-104's onset is 0 — neither precedes it. 0419: every alarmed channel (FT-9, LT-3,
PT-7) is inside BKR-2A's reach. Neither incident's verdict, ranking or corroboration detail
changes.

### The two controls — never expected to be red

- **(a) Complete graph** — the same fixture plus the IA-1 → AOV-2 edge. The engine names
  **IA-1** (weight 1.00) and corroboration fits all three edges. Proves the fixture's truth
  is recoverable when the model is right, so the missing edge is the only variable.
- **(b) Later unrelated alarm** — IA-1's onset moved to t100, after the cascade. The verdict
  stays **AOV-2**, no abstention. Proves the rule is "unexplained **precursor**", not "any
  independent candidate".

A boundary test additionally pins the tie rule (IA-1 onset equal to AOV-2's; IA-1 with no
historian samples) — neither triggers.

### Scope of the test

Test-only fixture on unit 91 (components 911–914), in `ModelMismatchTests` — never in
`IncidentRegistry`, never provisioned, not an incident, and **not an H9 case** (the harness
and its discovery guard target registered incidents only). It tests the **engine**, not RAG:
no corpus is authored, and grounding, retrieval and explanation are not exercised. The one
full-pipeline assertion abstains at the corroboration gate; a spy proves retrieval was never
called and a throwing explainer proves explanation was never reached.

## Consequences — what this result tells us

**Proven:** the engine now detects one class of model incompleteness from observables
alone — incompleteness that leaves a **timing footprint** (an unexplained alarm that began
before the named origin). This is narrow. Mismatch classes it still **cannot** catch:

- **A silent true cause** — the missing upstream node raised no alarm and has no historian
  samples, so there is no precursor to see.
- **A spurious authored edge whose timing happens to fit** — the graph claims a coupling
  that is not real; the engine over-attributes and nothing contradicts it.
- **A missing downstream edge** — it only lowers the origin's coverage and leaves an
  alarm unexplained *after* the origin, which (by control (b)) does not block the verdict.
- **A precursor that is itself a standing alarm** — an unrelated condition with an early
  historian onset would trigger a **false abstention**. This is the rule's known cost, the
  safe side of H8 (abstain rather than guess), and untested here.
- **A precursor whose onset equals the origin's** — by decision, not flagged.

**What this means for the generator question (ADR-042 follow-on):** one cell of the
mismatch-type × evidence-footprint map is now proven by hand at trivial cost. Hand-written
fixtures stay cheaper while the remaining questions are a few distinct cells, each needing
different evidence. A perturbation generator would only pay off when the variation **within**
a cell decides honesty — most plausibly the false-abstention/false-verdict trade-off of this
very rule across standing alarms and onset margins.

### Observation, named not fixed: 0418's delay windows have zero jitter tolerance

Every 0418 backbone edge has `DelayMinSeconds == DelayMaxSeconds` (point delays reproducing
Figure 2.1). Real onsets jitter; a one-second deviation on any edge would fail corroboration
and 0418 would abstain. This fixture deliberately uses ranged windows ([25,35] s, [15,25] s).
Widening 0418's windows is a seed/graph-authoring change with its own evidence burden and is
**not** made here.

## Rejected alternatives

- **Wrong delay window as the mismatch** — already caught by the existing window check (it
  would pass on first run and teach nothing new).
- **Attribution share as the signal** — 0418's FV-104 legitimately holds 0.79 (RCP-1B and
  BUS-2A are real parallel/contributing strands), so "origin share < 1" would misfire.
- **A named-skip test with the fix deferred** — a second skipped case beside the
  leading-question one, for a rule that is small, observable-only and checked against both
  incidents.
- **A caveat field instead of abstention** — a contract/API/UI change; abstention reuses the
  existing path and is the H8-consistent choice.

## Scope boundary

No physics, no generator, no `IncidentRegistry`/seed change, no new incident, no H9 case, no
change to the walker, Explain stage or retriever, no CauseTag-vs-origin fix (still separately
deferred), no fix to 0418's window tolerance.

## Reversal condition

If standing alarms are modelled (an alarm-state history), the rule should exempt conditions
already active before the flood window. If false abstentions appear in any real or generated
case, that is the trigger to sweep the rule's margins rather than tune it to one case.

## Evidence required

The mismatch test red on the unpatched engine, then green with the named reason; the full
pipeline sealing that abstention with retrieval never called; both controls and the boundary
test green (stated as never-red); 0418/0419 unchanged against the ADR-042 baseline; H9
goldens and traps unchanged; build 0/0; ArchitectureTests; each assembly in isolation.

See `artifacts/evidence/2026-09-25-adr-043-unexplained-precursor.md`.
