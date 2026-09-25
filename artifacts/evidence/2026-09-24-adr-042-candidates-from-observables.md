# Evidence — ADR-042: engine candidate selection and ranking from observables

Date: 2026-09-24. Branch `v1.0.0`, uncommitted at time of writing.
Everything below was actually run in this environment; outputs are quoted, not summarised.

## What changed

| Item | Path |
|---|---|
| ADR | `docs/adr/ADR-042-engine-candidate-selection-from-observables.md` |
| Walker rewritten (candidate set, greedy set cover, attribution weight, structural roles) | `src/Contexts/RootCause/Nexus1.RootCause.Infrastructure/Diagnosis/EfGraphWalker.cs` |
| Doc comments that now misdescribed the fields (comment-only) | `Component.cs`, `Candidate.cs`, `DiagnosisContracts.cs`, `GroundingSeed.cs` |
| New proofs: poisoned seed + pure-observable fixture | `tests/Nexus1.RootCause.ComponentTests/GraphWalkerDerivationTests.cs` |
| Updated consumers of the old seeded values | `GroundingDiagnosisTests.cs` (PB-2A), `GroundingDiagnosis0419Tests.cs` (0.70/0.16/0.10), H9 `golden-0418-fv104.json` + `golden-0419-bkr2a.json` (`expectCandidates` only) |

`EfGraphWalker.cs` no longer contains the string `Illustrative` except in its doc comment
stating it never reads those fields. No schema migration; no change to the corroborator,
retriever, Explain stage, H9 case format, or console.

## Build

`dotnet build Nexus1.Runtime.sln -c Debug` → `0 Warning(s)  0 Error(s)`.

## Before / after walker output (real LocalDB, real seed)

Captured with a temporary capture test (deleted afterwards, never committed) run once on the
old walker and once on the new. Decimal commas are the machine locale.

**Before (seeded):**
```
== EVT-2026-0418 origin=FV-104 (5 candidates)
   FV-104  origin        weight=0,6600 coverage=0,7857
   PB-2A   proximate     weight=0,2000 coverage=0,5000
   RCP-1B  parallel      weight=0,0700 coverage=0,5000
   BUS-2A  contributing  weight=0,0500 coverage=0,0714
   FT-7    ruled-out     weight=0,0200 coverage=0,0000
== EVT-2026-0419 origin=BKR-2A (3 candidates)
   BKR-2A  origin        weight=0,7000 coverage=1,0000
   FT-9    proximate     weight=0,1600 coverage=0,4000
   LOF-1   ruled-out     weight=0,1000 coverage=0,0000
```

**After (derived):**
```
== EVT-2026-0418 origin=FV-104 (10 candidates)
   FV-104  origin        weight=0,7857 coverage=0,7857
   RCP-1B  parallel      weight=0,1429 coverage=0,5000
   BUS-2A  contributing  weight=0,0714 coverage=0,0714
   SG-1    downstream    weight=0,0000 coverage=0,7143
   FWP-2A  downstream    weight=0,0000 coverage=0,5714
   PB-2A   downstream    weight=0,0000 coverage=0,5000
   LF-1    downstream    weight=0,0000 coverage=0,3571
   CT-1    downstream    weight=0,0000 coverage=0,2143
   RT-1    downstream    weight=0,0000 coverage=0,0714
   FT-7    ruled-out     weight=0,0000 coverage=0,0000
== EVT-2026-0419 origin=BKR-2A (5 candidates)
   BKR-2A  origin        weight=1,0000 coverage=1,0000
   FT-9    downstream    weight=0,0000 coverage=0,4000
   LT-3    downstream    weight=0,0000 coverage=0,3000
   PT-7    downstream    weight=0,0000 coverage=0,3000
   LOF-1   ruled-out     weight=0,0000 coverage=0,0000
```

Verdicts identical (FV-104, BKR-2A). Every row matches ADR-042's tables: FV-104 0.66→0.79,
RCP-1B 0.07→0.14, BUS-2A 0.05→0.07, PB-2A proximate 0.20→downstream 0, FT-7 0.02→0;
BKR-2A 0.70→1.00, FT-9 proximate 0.16→downstream 0, LOF-1 0.10→0; SG-1, FWP-2A, LF-1,
CT-1, RT-1, LT-3, PT-7 newly ranked as downstream. Every coverage value is unchanged for
the candidates that existed before.

## Red → green: the two new proofs fail on the old walker, pass on the new

Written and run **before** the walker change (against the seeded walker):

```
Failed GraphWalkerDerivationTests.Walker_ranks_a_fixture_with_no_illustrative_values_purely_from_observables
   Assert.Equal() Failure: Collections differ
   Expected: ["SRC-A", "M-SOLO", "Z-STRAND", "A-LEARN", "NODE-B", ···]
   Actual:   []
Failed GraphWalkerDerivationTests.Walker_output_is_byte_identical_when_the_illustrative_fields_are_poisoned
   Assert.Equal() Failure: Strings differ
   Expected: "[{"ComponentId":1,"Tag":"FV-104","Role":""···
   Actual:   "[{"ComponentId":4,"Tag":"PB-2A","Role":"o"···
Failed!  - Failed: 2, Passed: 0, Total: 2
```

The old walker ranked **zero** candidates when no illustrative values existed, and — with
FV-104's illustrative fields erased — named **PB-2A as origin**: direct proof the seed was
choosing the answer. After the change:

```
Passed GraphWalkerDerivationTests.Walker_ranks_a_fixture_with_no_illustrative_values_purely_from_observables
Passed GraphWalkerDerivationTests.Walker_output_is_byte_identical_when_the_illustrative_fields_are_poisoned
```

- **Poisoned seed:** 0418 walked clean, then FT-7 set to `IllustrativeWeight 0.99`,
  `IllustrativeRole "origin"` and FV-104's illustrative fields set null (the test re-reads
  the rows to confirm the poison landed); the JSON-serialised walker output is asserted
  byte-identical, with FV-104 still origin.
- **Pure observables:** unit 90, components 901–909, test data only (not in
  `IncidentRegistry`, not provisioned), no illustrative value anywhere. Hand-worked
  expectation (in the test's doc comment), all asserted: order SRC-A, M-SOLO, Z-STRAND,
  A-LEARN, NODE-B, LOUD, NODE-D, CORR; roles origin / independent / parallel /
  contributing / downstream ×3 / ruled-out; weights 10/16, 2/16 ×3, then 0; coverage per
  node; the edge-target-only node TGT excluded; weights summing to 1. Tie-breaks: a
  three-way tie at 2 new alarms is broken by onset (M-SOLO t5 beats t25 despite the highest
  ComponentId), and the remaining onset tie (Z-STRAND, A-LEARN both t25) by lower
  ComponentId (905 before 906 — the opposite of tag order). LOUD, the node with the most
  alarms, is `downstream` with weight 0.

## Regression — every assembly run in isolation, sequentially

| Assembly | Result |
|---|---|
| Nexus1.ArchitectureTests | 8/8 |
| Nexus1.RootCause.UnitTests | 39/39 |
| **Nexus1.RootCause.ComponentTests** (incl. H9 deterministic + live, Ollama up) | **80 total: 79 passed, 1 skipped** (`trap-leading-question`, named reason) |
| Reporting Unit / Component | 4/4, 19/19 |
| Audit / Compliance Component | 13/13, 13/13 |
| All other 30 assemblies (AlarmManagement … ServiceDefaults) | all passed, 0 failed |

RootCause.ComponentTests grew 78 → 80 (the two new proofs). H9 lines from that run:
`golden-0418-fv104 verdict=FV-104`, `golden-0419-bkr2a verdict=BKR-2A`,
`golden-0420-refused refused`, `golden-0420-record status=Inconclusive`, both traps abstain
as before, `golden-0418-live VERDICT FV-104`, `golden-0419-live VERDICT BKR-2A`,
`trap-no-grounding-live` control 2 passages / unrelated 0.

Pre-existing, not caused here: `Nexus1.Contracts.ContractTests` and
`Nexus1.DistributedSlice.EndToEndTests` report "No test is available" — both are
`.csproj`-only shells with zero tracked `.cs` files, unchanged since 2026-08-15/16.

## Live chain + E2E

Stack started for this check (RabbitMQ, RootCause.Host :5102, BFF :5103 with the four
contexts + `Services__RootCauseHost`, `ng serve` :4200; Ollama already running) and stopped
afterwards.

Live `POST /api/v1/root-cause/incidents/EVT-2026-0418/diagnoses` through the BFF → HTTP 200
in 26.3s, `verdict=FV-104`, the same 10-row derived ranking as above (FV-104 0.7857 …).

```
npx playwright test --grep @slow
  ok 1 root-cause-cross-screen.e2e.ts › root cause agreement @slow … (26.7s)
  ok 2 root-cause-cross-screen.spec-of-specs.e2e.ts › root cause agreement is load-bearing @slow … (26.1s)
  2 passed (54.4s)
```

Rendered console, read from the live pages (browser pane):
- Root Cause Graph candidate table: `FV-104 Origin 79% 79%`, `RCP-1B Parallel 14% 50%`,
  `BUS-2A Contributing 7% 7%`, `SG-1 Downstream 0% 71%`, … `PB-2A Downstream 0% 50%`, …
  `FT-7 Ruled-Out 0% 0%`.
- Incident Analysis: `FV-104 — origin — 79%` (was 66%).

**Correction to the approved plan:** the candidate table already has a Coverage column
(the second percentage). The plan's deferred "Coverage column" follow-up was based on an
assumption not checked against the template. The real deferred item is wording: the
"Illustrative weight" header, the "Weights and coverage are illustrative figures…"
footnote and Incident Analysis's "Illustrative confidence." caption now misdescribe a
derived attribution share. ADR-042 records this; no UI change was made.

## Cleanup

Temporary capture test deleted; E2E stack stopped (only Ollama, pre-existing, still
listening); 0 leftover `*Tests_*` databases in LocalDB.
