# Evidence — ADR-043: the unexplained-precursor rule (model-mismatch probe)

Date: 2026-09-25. Branch `v1.0.0`, uncommitted at time of writing.
Everything below was actually run in this environment; outputs are quoted, not summarised.

## What changed

| Item | Path |
|---|---|
| ADR | `docs/adr/ADR-043-unexplained-precursor-rule.md` |
| The rule (+68/−4; no interface/contract change) | `src/Contexts/RootCause/Nexus1.RootCause.Infrastructure/Diagnosis/EfTelemetryCorroborator.cs` |
| Test-only fixture, mismatch + controls + boundary + full pipeline | `tests/Nexus1.RootCause.ComponentTests/ModelMismatchTests.cs` |

Fixture: unit 91, components 911–914 (IA-1, AOV-2, LVL-3, TRP-4), no illustrative values,
ranged delay windows. Not in `IncidentRegistry`, not provisioned, not an incident, not an
H9 case. No corpus authored: **this tests the engine, not RAG.**

## Build

`dotnet build Nexus1.Runtime.sln -c Debug` → `0 Warning(s)  0 Error(s)`.

## Red on the unpatched engine (tests written first)

```
Failed ModelMismatchTests.Full_pipeline_seals_a_named_abstention_for_the_mismatch_before_retrieval_runs
   Assert.Equal() Failure: Strings differ
   Expected: "telemetry corroboration failed: unexplain"···
   Actual:   "no grounding: corpus retrieval returned n"···
Failed ModelMismatchTests.Missing_upstream_coupling_with_an_earlier_unexplained_precursor_fails_corroboration
   expected the unexplained precursor to fail corroboration; got: 2 backbone edge(s) fit their delay windows against the historian
Failed!  - Failed: 2, Passed: 4, Total: 6
```

- **Engine seam (the clean red):** the unpatched corroborator green-lit AOV-2 —
  "2 backbone edge(s) fit".
- **Full pipeline:** the unpatched pipeline abstained only **by accident** — "no grounding",
  because the fixture has no corpus. With a corpus it would have reached the explain stage
  and sealed a confident AOV-2 verdict. The accidental abstention masks the mismatch; it is
  not the engine noticing anything.
- The 4 that passed are the two controls and the two boundary rows — **never expected to be
  red** (see below).

## Green after the rule

```
Passed ModelMismatchTests.Full_pipeline_seals_a_named_abstention_for_the_mismatch_before_retrieval_runs
Passed ModelMismatchTests.Control_complete_graph_names_the_true_origin_and_corroborates_cleanly
Passed ModelMismatchTests.Missing_upstream_coupling_with_an_earlier_unexplained_precursor_fails_corroboration
Passed ModelMismatchTests.Boundary_equal_or_missing_precursor_onset_never_triggers_the_rule(ia1OnsetSeconds: null)
Passed ModelMismatchTests.Boundary_equal_or_missing_precursor_onset_never_triggers_the_rule(ia1OnsetSeconds: 20)
Passed ModelMismatchTests.Control_an_unexplained_alarm_that_starts_after_the_origin_does_not_block_the_verdict
Total tests: 6  Passed: 6
```

What each asserts:

- **Mismatch, engine seam:** walker names AOV-2 origin (weight 4/5) with IA-1 `independent`
  (1/5); corroboration fails with exactly
  `unexplained precursor: IA-1 began before origin AOV-2 and is not explained by it -- the causal graph may be incomplete`.
- **Mismatch, full pipeline:** abstains with
  `telemetry corroboration failed: unexplained precursor: IA-1 began before origin AOV-2 …`;
  verdict null, citations empty; the persisted DiagnosisRun carries that reason; an audit
  entry with the run's hash exists (sealed). A counting spy over the real retriever records
  **0 calls**, and a throwing explainer proves explanation was **never reached** —
  grounding/retrieval/explanation are not exercised.
- **Control (a), complete graph** (+ IA-1 → AOV-2): origin IA-1, weight 1.00, AOV-2
  downstream; corroboration `3 backbone edge(s) fit their delay windows against the historian`.
- **Control (b), later alarm** (IA-1 onset t100): origin AOV-2, IA-1 independent;
  corroboration `2 backbone edge(s) fit …` — no abstention.
- **Boundary:** IA-1 onset equal to AOV-2's (t20), and IA-1 with no historian samples —
  neither triggers the rule.

The controls and boundary rows passed on the unpatched engine and still pass: they are
controls, not red-then-green.

## 0418 / 0419 unchanged — before vs after, byte-identical

A temporary capture test (deleted afterwards, never committed) printed walker output plus
corroboration result for both incidents, run once with the **committed** corroborator
(`git show HEAD:…`) and once with the patched one:

```
== EVT-2026-0418 origin=FV-104 fits=True detail="6 backbone edge(s) fit their delay windows against the historian"
   FV-104  origin        weight=0,7857 coverage=0,7857
   RCP-1B  parallel      weight=0,1429 coverage=0,5000
   BUS-2A  contributing  weight=0,0714 coverage=0,0714
   SG-1 … RT-1           downstream    weight=0,0000 (coverage 0,7143 … 0,0714)
   FT-7    ruled-out     weight=0,0000 coverage=0,0000
== EVT-2026-0419 origin=BKR-2A fits=True detail="3 coupled channel(s) deflected together and 2 independent witness(es) stayed flat"
   BKR-2A  origin        weight=1,0000 coverage=1,0000
   FT-9 / LT-3 / PT-7    downstream    weight=0,0000 (coverage 0,4000 / 0,3000 / 0,3000)
   LOF-1   ruled-out     weight=0,0000 coverage=0,0000
--- diff: IDENTICAL
```

Verdicts, rankings, weights (= the ADR-042 baseline) and corroboration detail strings are
identical. Further, the live H9 golden runs sealed **the same audit hashes as in the ADR-042
slice** — `golden-0419-live 811547bad5fa…`, `golden-0418-live 5c0ad35cd3d3…` — so the sealed
payloads for both incidents are byte-identical too.

## Regression — every assembly in isolation, sequentially

| Assembly | Result |
|---|---|
| **Nexus1.RootCause.ComponentTests** (Ollama up: H9 deterministic + live) | **86 total: 85 passed, 1 skipped** (`trap-leading-question`, named) |
| Nexus1.ArchitectureTests | 8/8 |
| Nexus1.RootCause.UnitTests | 39/39 |
| All other 34 assemblies | all passed, 0 failed |

RootCause.ComponentTests grew 80 → 86 (the 6 new). H9 unchanged: `golden-0418-fv104
verdict=FV-104`, `golden-0419-bkr2a verdict=BKR-2A`, `golden-0420-refused`,
`golden-0420-record Inconclusive`, `trap-transposed-entity` and `trap-invented-source`
abstain as before, `golden-0418-live VERDICT FV-104`, `golden-0419-live VERDICT BKR-2A`,
`trap-no-grounding-live` control 2 / unrelated 0.

(One capture attempt lost its output to a malformed `sed` expression in my own shell loop;
the loop was re-run with a plain filter and the results above are from that re-run.)

Pre-existing, unchanged: `Nexus1.Contracts.ContractTests` and
`Nexus1.DistributedSlice.EndToEndTests` still report "No test is available" (`.csproj`-only
shells, noted in the ADR-042 slice).

## Observation, named not fixed

0418's backbone windows are exact points (`DelayMinSeconds == DelayMaxSeconds`); a one-second
jitter on any edge would fail corroboration and 0418 would abstain. Recorded in ADR-043; the
fixture uses ranged windows; 0418 is not changed.

## What this proves, and what it doesn't

Proven: detection of model incompleteness **that leaves a timing footprint** (an unexplained
alarm beginning before the named origin), from observables only, with no change to 0418/0419.
Not caught (ADR-043): a silent true cause, a spurious edge whose timing fits, a missing
downstream edge, and — the rule's own cost — a standing alarm with an early onset, which
would cause a false abstention (untested here).

## Cleanup

Temporary capture test deleted; 0 leftover `*Tests_*` databases; only Ollama (pre-existing)
listening.
