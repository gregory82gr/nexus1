# Evidence — H9 evaluation harness core (ADR-041)

Date: 2026-09-24. Branch `v1.0.0`, uncommitted at time of writing.
Everything below was actually run in this environment; outputs are quoted, not summarised.

## What was built

| Item | Path |
|---|---|
| ADR | `docs/adr/ADR-041-h9-evaluation-harness-core.md` |
| 0420 identity holder (shared by provisioning + harness) | `src/Contexts/RootCause/Nexus1.RootCause.Application/Diagnosis/Evt20260420QaEscape.cs` |
| Provisioning now uses the holder | `src/Hosts/Nexus1.RootCause.Host/Provisioning.cs` |
| Case model, loader, fixture explainer, shared assertions | `tests/Nexus1.RootCause.ComponentTests/Evaluation/EvalCase.cs` |
| Deterministic runner | `tests/.../Evaluation/EvaluationHarnessTests.cs` |
| Live runner (`[SkippableTheory]`, Ollama-gated) | `tests/.../Evaluation/LiveEvaluationHarnessTests.cs` |
| Discovery guard (4 facts) | `tests/.../Evaluation/EvaluationCaseDiscoveryTests.cs` |
| 10 case files | `tests/.../Evaluation/Cases/*.json` (copied to output via csproj `None` item) |

Cases: deterministic — `golden-0418-fv104`, `golden-0419-bkr2a`, `golden-0420-refused`,
`golden-0420-record`, `trap-transposed-entity` (FV-140), `trap-invented-source` (real,
unretrieved Ch.5 passage during a 0418 run), `trap-leading-question` (skipped, named reason);
live — `golden-0418-live`, `golden-0419-live`, `trap-no-grounding-live` (retriever seam).

No engine change: `git diff` touches no file under `src/Contexts/RootCause/*` other than the
new holder; the CauseTag-vs-origin check was **not** added.

## Build

`dotnet build Nexus1.Runtime.sln -c Debug` → `0 Warning(s)  0 Error(s)` (TreatWarningsAsErrors on).
(A first attempt failed with MSB3027 file locks from a ModularRuntime host left running from
the 0420 slice; that host was stopped and the rebuild was clean.)

## Deterministic tier + discovery guard

`dotnet test tests/Nexus1.RootCause.ComponentTests --filter "…EvaluationHarnessTests|…EvaluationCaseDiscoveryTests" --logger "console;verbosity=detailed"`

```
Passed  EvaluationCaseDiscoveryTests.Skipped_cases_carry_a_named_reason_rather_than_being_left_out
Passed  EvaluationCaseDiscoveryTests.Every_case_uses_the_known_grammar_and_targets_a_built_incident
Passed  EvaluationCaseDiscoveryTests.Every_trap_category_and_all_three_incidents_are_represented
Passed  EvaluationCaseDiscoveryTests.Case_files_are_present_and_both_tiers_enumerate_cases
[xUnit.net 00:00:04.40]     …EvaluationHarnessTests.Case(id: "trap-leading-question") [SKIP]
[xUnit.net 00:00:04.40]       blocked: no CauseTag-vs-origin check exists — separate engine slice
Skipped …EvaluationHarnessTests.Case(id: "trap-leading-question")
Passed  …Case(id: "trap-transposed-entity")
  [trap-transposed-entity] verdict=- abstain=validation failed: unknown entity FV-140 citations=[… Ch.1-2 | … Ch.2 (delay windows)]
Passed  …Case(id: "golden-0420-record")
  [golden-0420-record] status=Inconclusive verdict=- flood=- reason=EVT-2026-0420 (QA escape): … CR-7 … WV-318 …
Passed  …Case(id: "trap-invented-source")
  [trap-invented-source] verdict=- abstain=validation failed: uncited claim citing chunk 3 citations=[… Ch.1-2 | … Ch.2 (delay windows)]
Passed  …Case(id: "golden-0419-bkr2a")
  [golden-0419-bkr2a] verdict=BKR-2A abstain=- citations=[… Ch.5, Ch.10 (artefact) | … Ch.5 (silence as evidence)]
Passed  …Case(id: "golden-0418-fv104")
  [golden-0418-fv104] verdict=FV-104 abstain=- citations=[… Ch.1-2 | … Ch.2 (delay windows)]
Passed  …Case(id: "golden-0420-refused")
  [golden-0420-refused] refused: unknown incident 'EVT-2026-0420'
Test Run Successful.  Total tests: 11  Passed: 10  Skipped: 1
```

The golden verdict cases assert everything the migrated tests asserted (not abstained,
verdict, non-empty audit hash, persisted run with that verdict and null abstain reason,
candidates persisted, audit entry with that hash, corpus version) **plus** the full ranked
candidate list in engine order (tag/role/weight), origin coverage (11/14, 10/10),
mustNotAssert, mustCite, mustNotCite.

## Live tier — Ollama UP (nexus-dslm, nomic-embed-text served on 127.0.0.1:11434)

```
Passed …LiveEvaluationHarnessTests.Case(id: "golden-0419-live") [2 m 6 s]
  [golden-0419-live] VERDICT BKR-2A auditHash=1474ac0173177a723d32514a5c071ddb0657a21232d4700a41b6abbc46831867 citations=[… Ch.5, Ch.10 (artefact) | … Ch.5 (silence as evidence)]
Passed …Case(id: "golden-0418-live") [57 s]
  [golden-0418-live] VERDICT FV-104 auditHash=3f0986425052060bcd0b8b3f45094295cc7ec7802e5b65fea399a6261b9c2a81 citations=[… Ch.1-2 | … Ch.2 (delay windows)]
Passed …Case(id: "trap-no-grounding-live") [2 s]
  [trap-no-grounding-live] control query "what starved the steam generator and tripped the reactor?" -> 2 passage(s)
  [trap-no-grounding-live] unrelated query "chocolate cake recipe with buttercream frosting" -> 0 passage(s) []
Test Run Successful.  Total tests: 3  Passed: 3
```

Both live goldens reached the real verdict this run (not the safe-abstention branch). The
no-grounding case is non-vacuous: the same real-embedder semantic path returned 2 passages
for the related control query and 0 for the unrelated one.

## Live tier — Ollama DOWN

Ollama (`ollama app.exe` + `ollama.exe`, running before this slice) was stopped; `/api/tags`
→ "Unable to connect to the remote server".

```
…Case(id: "golden-0419-live") [SKIP]  Local Ollama with nexus-dslm + nomic-embed-text is not available on 127.0.0.1:11434.
…Case(id: "golden-0418-live") [SKIP]  (same reason)
…Case(id: "trap-no-grounding-live") [SKIP]  (same reason)
Test Run Successful.  Total tests: 3  Skipped: 3     (exit 0)
```

Ollama was then restarted; `/api/tags` again served `nexus-dslm:latest, nomic-embed-text:latest`.

## 0420 holder matches what is provisioned

`Evt20260420QaEscape.Reason` (as persisted by the harness) vs the dev `RootCauseDb` row
`RootCause.RootCauseAnalysis` id 20260420:

```
holder: len=417 sha256(utf16)=4098574413EA544B35CF97BA3091FE884D7E79BAC70375A6EDD24B1CAD113D1A
devdb : 417 4098574413EA544B35CF97BA3091FE884D7E79BAC70375A6EDD24B1CAD113D1A
devdb row: Status=Inconclusive Verdict=NULL AlarmFloodId=NULL UnitId=1 OpenedBy=provenance.audit
```

## No duplicate coverage (before → after)

Before (ripgrep over `tests/*.cs`, captured prior to migration):

```
GroundingDiagnosisTests.cs:200  Full_pipeline_reaches_the_FV104_verdict_and_persists_a_sealed_run
GroundingDiagnosisTests.cs:256  Full_pipeline_abstains_when_the_draft_names_an_unregistered_entity
GroundingDiagnosisTests.cs:271  Full_pipeline_abstains_when_the_draft_cites_a_passage_not_retrieved
GroundingDiagnosis0419Tests.cs:115  Full_pipeline_reaches_the_BKR2A_artefact_verdict_and_seals_it
OllamaExplainPipelineTests.cs:117  Full_pipeline_end_to_end_reaches_the_FV104_verdict_with_the_real_model
```

After: the same five-name ripgrep over `tests/*.cs` → **No matches found**. `FV-140`,
`unretrieved` and the `RCP-1B` leading draft appear only in the Evaluation folder.

Test-attribute counts (`[Fact]/[SkippableFact]/[Theory]/[SkippableTheory]`), RootCause.ComponentTests:

| File | Before | After |
|---|---|---|
| GroundingDiagnosisTests.cs | 15 | 12 |
| GroundingDiagnosis0419Tests.cs | 7 | 6 |
| OllamaExplainPipelineTests.cs | 8 | 7 |
| Evaluation/* (new) | — | 6 (1 + 1 theories, 4 guard facts) |
| **All files** | **69 / 13 files** | **70 / 16 files** |

Remaining `runner.RunAsync` callers test different concerns and repeat no harness case:
unit-tier runner branching with fakes (`RootCause.UnitTests/FixedIncidentDiagnosisRunnerTests`),
metrics instruments (`DiagnosisMetricsTests`), fault-injection abstentions (historian / corpus
removed — not Appendix-J trap categories), and live narration determinism.

## Regression — full solution

`dotnet test Nexus1.Runtime.sln --no-build` (all assemblies in parallel, Ollama up):
34 assemblies green, including `Nexus1.ArchitectureTests` 8/8 (no files changed under
`tests/Nexus1.ArchitectureTests`) and `Nexus1.RootCause.UnitTests` 39/39. **Three assemblies
reported failures on that run, all environmental, all green on isolated rerun:**

| Assembly | Parallel run | Cause (observed) | Isolated rerun |
|---|---|---|---|
| EmergencyPreparedness.ComponentTests | 2 failed / 8 | `SqlException: Execution Timeout Expired` (LocalDB contention) | **8/8 passed** |
| ReinforcementLearning.ComponentTests | 2 failed / 7 | `SqlException: Execution Timeout Expired` | **7/7 passed** |
| RootCause.ComponentTests | 2 failed / 78 | Ollama `/api/chat` HTTP 500 in two pre-existing `OllamaExplainPipelineTests`; Ollama's `server.log`: `failed to allocate CPU buffer of size 1923946496 … unable to allocate CPU buffer` (RAM exhausted with ~30 test hosts + LocalDB loaded) | **78 total: 77 passed, 1 skipped (trap-leading-question)** — both failed tests and all three live harness cases passed |

Honest caveat: on this machine the whole-solution parallel run is not a reliable single gate
while the CPU model is loaded — the live model tests need memory the parallel run consumes.
The three timed-out EP/RL runs left per-run test databases behind in LocalDB
(`EmergencyPreparednessComponentTests_911c…`, `…_f3b0…`, `ReinforcementLearningComponentTests_8348…`);
all three were dropped after review (`SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE`),
leaving 0 `*Tests_*` databases in LocalDB.

## Not done / deliberately out of scope

- The leading-question trap is **authored and skipped**, not passing: no CauseTag-vs-origin
  check exists (separate engine slice).
- No question interface, so `intent` is documentation only; traps test the deterministic
  controls, not the model's own susceptibility.
- No CI pipeline (`.github/workflows/` empty): "a regression fails the build" means the local
  `dotnet test` gate.
- Circularity (seeded `IllustrativeWeight`/roles/`AlarmCount`) unchanged: golden cases prove
  the known answers are reproduced, not derived.
