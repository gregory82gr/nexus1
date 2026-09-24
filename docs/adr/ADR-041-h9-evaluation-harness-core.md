# ADR-041: H9 evaluation harness core

## Status

Accepted. Closes the `From Flood to Cause` Appendix J / Appendix B **H9** gap
("golden and trap questions run on every change; a regression fails the build") for the
three already-built fixed incidents. Independent of any physics generator.

## Context

H9 was the one Appendix-B control with no implementation. Appendix J describes JSON cases
(an id, a *question*, an expectation, assertions the answer must not make, required
citations), four trap categories (leading question, no-grounding, transposed entity,
invented source), and an xUnit `[Theory]` runner where a failed expectation fails the build.

Four facts about this repo shape the harness (found while planning):
1. **There is no free-form question interface.** The diagnosis route takes an incident id;
   the AI Diagnostics advisory is explicitly not running in this build.
2. **Nothing compares the model draft's `CauseTag` to the engine's origin.** `CauseTag` is only
   sealed into the audit payload.
3. **Corpus `ChunkId`s are identity-generated** — they differ per database.
4. **There is no CI pipeline** — `.github/workflows/` is empty.

## Decision

### Case grammar — `question` becomes `target` + `intent`

JSON, one file per case, in `tests/Nexus1.RootCause.ComponentTests/Evaluation/Cases/`:

| Field | Meaning |
|---|---|
| `id`, `category` | `golden` \| `trap-transposed-entity` \| `trap-invented-source` \| `trap-leading` \| `trap-no-grounding` |
| `tier` | `deterministic` (fixture explainer, always runs) \| `live` (real Ollama, skippable) |
| `seam` | `pipeline` (full One-Truth run) \| `record` (the human case record) \| `retriever` (the H2 gate) |
| `target.incidentId` | Replaces the book's `question` |
| `intent` | The book-style natural-language phrasing — **documentation only, never sent anywhere** |
| `injectedDraft` | Trap cases: the draft injected at the `IExplainer` seam (citations as `retrieved` or `unretrieved:<label>`) |
| `expect` | `verdict` \| `abstain` \| `refused` \| `verdict_or_safe_abstain` \| `inconclusive` |
| `expectVerdict`, `expectAbstainReasonContains`, `expectReasonContains` | Expected values |
| `expectCandidates`, `expectOriginCoverage` | Golden pipeline cases: the full ranked list in engine order (tag, role, weight) and the origin's share of the flood (`"11/14"`) |
| `mustNotAssert`, `mustCite`, `mustNotCite` | Tags never named as verdict/origin; citations matched on **`SourceLabel` substrings**, never ChunkIds |
| `retrieverQuery` | Retriever-seam cases only: the unrelated query embedded on the semantic path |
| `retrieverControlQuery` | Retriever-seam cases only: a related query that **must** return ≥1 passage, proving the semantic path is live so the empty result is not vacuous |
| `skip` | A named reason; the case is reported **Skipped** by name, never silently absent |

**The missing question interface is named, not hidden.** A field called `question` that was
silently ignored would claim a capability that does not exist, so the book's phrasing
survives only as `intent`. `expect` is finer-grained than the book's `abstain_or_correct`
because the pipeline has distinct outcomes (verdict / abstention-with-reason / refusal),
and `inconclusive` was added for the one record-seam case (0420's human case is a terminal
Inconclusive record, not a pipeline outcome).

### Posable-vs-blocked trap matrix

| Category | Status | How |
|---|---|---|
| Transposed entity | **Posable** (deterministic) | Inject a draft naming **FV-140** (the book's own example) on the real 0418 run → abstain `unknown entity FV-140`. H3 + H8. |
| Invented source | **Posable** (deterministic) | Inject a draft citing a **real but unretrieved** document (a 0419 passage during a 0418 run) → abstain `uncited claim`. H4 + H8. |
| No-grounding | **Partially posable** — live tier, retriever seam only | `IRetriever` with an unrelated query and no tag exercises the H2 floor on the semantic path. Only meaningful with the real embedder (the `NoOp` embedder returns nothing for any query — a vacuous pass). |
| Leading question | **Blocked** — authored, **skipped with a named reason** | Needs the question interface to pose. Its injectable failure mode (a draft with `CauseTag: RCP-1B` for 0418) would **pass today**: RCP-1B is registered (H3), claims cite retrieved passages (H4), and no control compares `CauseTag` to the engine origin. Skip reason: *"blocked: no CauseTag-vs-origin check exists — separate engine slice"*. |

**Central boundary:** every trap posable today is injected at the explainer seam, so these
cases test the **deterministic controls** (H3, H4, H8, the engine verdict) — **not the
model's own susceptibility** (e.g. a model update that starts agreeing with leading
questions). That needs the question interface.

### Golden cases (real, previously verified values only)

- `golden-0418-fv104` — origin FV-104; FV-104 0.66, PB-2A 0.20, RCP-1B 0.07, BUS-2A 0.05,
  FT-7 0.02; coverage 11/14; sealed verdict FV-104; cites Ch.1-2 only.
- `golden-0419-bkr2a` — origin BKR-2A via artefact coverage; FT-9 0.16, LOF-1 0.10; verdict
  BKR-2A; cites Ch.5 only.
- `golden-0420-refused` — the engine refuses (not in `IncidentRegistry` → 404).
- `golden-0420-record` — the provisioned human case is Inconclusive, no verdict, no flood,
  reason names CR-7 and WV-318. To assert the **actual** provisioned content, the 0420
  identity (fixed id, unit, timestamp, reason) and its construction move into a public
  holder next to `IncidentRegistry` (`Evt20260420QaEscape`), used by **both** provisioning
  and the harness — a test-enabling move, not an engine change.
- `golden-0418-live`, `golden-0419-live` — real Ollama, `verdict_or_safe_abstain`.

### Runners, gate, and skip semantics

- `EvaluationHarnessTests` (deterministic) and `LiveEvaluationHarnessTests` (live), both
  `[SkippableTheory]` + `[MemberData]` yielding **case ids**, so each case appears by name.
- The live tier **skips (does not fail)** when local Ollama with `nexus-dslm` +
  `nomic-embed-text` is absent — the existing `OllamaExplainPipelineTests` pattern.
- A discovery guard asserts cases exist, every category/tier/seam/expect value is known,
  and every target is 0418/0419/0420 (CLAUDE.md's "green suite discovering zero tests"
  anti-pattern).
- **No CI pipeline exists.** "Fails the build" here means the project's existing local
  `dotnet test` gate discipline, not an automated per-change job — the same caveat the e2e
  README carries.

### Migration, not duplication

The harness becomes the single home for four full-pipeline tests, which are removed from
their old files: the FV-104 verdict test and the two draft-trap tests
(`GroundingDiagnosisTests`), and the BKR-2A verdict test (`GroundingDiagnosis0419Tests`).
The live `golden-0418-live` case supersedes
`OllamaExplainPipelineTests.Full_pipeline_end_to_end_reaches_the_FV104_verdict_with_the_real_model`.
Unit-level validator tests, fault-injection abstentions, audit/run-store, 0419
silence/isolation, metrics, registry and workflow tests stay where they are.

## Consequences / what this does NOT solve

- **Circularity stays out of scope.** The walker still ranks only components carrying a
  seeded `IllustrativeWeight`, keeps seeded non-origin roles, and computes coverage from
  seeded `AlarmCount`. A passing golden case proves the known answers are **reproduced**,
  not that the engine **derives** them honestly from observables.
- No physics generator; no model-susceptibility testing (no question interface).
- The leading-question gap is **surfaced, not fixed** — a separate engine slice.
- Three incidents are far too small a labelled set to replace ADR-038's `grounding_hit`
  proxy with real `retrieval_recall`.
- Appendix J's own limit applies: a clean run raises the floor on trust; it does not
  guarantee correctness.

## Scope boundary

No physics generator, no engine changes (including **not** adding the CauseTag check), no
new incident, no question interface, no CI pipeline creation.

## Reversal condition

When a question interface exists, `intent` becomes executable and the leading-question and
no-grounding traps move to the full pipeline. When the CauseTag-vs-origin check lands, the
leading-question case's skip is removed and it must pass. When a CI pipeline is created, the
deterministic tier joins it.

See `artifacts/evidence/2026-09-24-h9-evaluation-harness-core.md`.
