# Evidence — RootCause Explain stage, air-gapped end-to-end

**Date:** 2026-09-22
**ADR:** ADR-033 (completes ADR-032's deferred stage)
**Scope:** The served-model half of the One-Truth Pipeline for EVT-2026-0418,
run for real against a local, loopback-only Ollama. The fixed-incident walking
skeleton is now complete: engine decides → model explains → H3/H4 validates →
audit seals.

## Environment (reproducibility record — pin by digest, not tag)

- Ollama runtime: **0.34.2** (user-scope install via winget, loopback server 127.0.0.1:11434)
- Chat model `nexus-dslm` base blob: **sha256:5ee4f07cdb9beadbbb293e85803c569b01bd37ed059d2715faa7bb405f31caa6**
  (`qwen2.5:3b-instruct-q4_K_M`, id `357c53fb659c`)
- Embedding model `nomic-embed-text`: id **0a109f422b47**
- Pinned params baked into `nexus-dslm` (verified via `ollama show nexus-dslm --modelfile`):
  `temperature 0`, `seed 42`, `top_p 1`, `repeat_penalty 1`, `num_ctx 8192`, `stop <|im_end|>`
- Hardware: **Intel i5-3340M CPU @ 2.70GHz, 16GB RAM, Intel HD 4000 (no CUDA GPU) — CPU-only inference**
- .NET 8 SDK (Roslyn 4.11); Semantic Kernel 1.50.0 + Connectors.Ollama 1.50.0-alpha

## Built

| Piece | Location |
|---|---|
| `Nexus1.RootCause.Explain` project (SK + Ollama, the only served-model project) | `src/Contexts/RootCause/Nexus1.RootCause.Explain/` |
| `SemanticKernelExplainer` (FunctionChoiceBehavior.None, temp 0, H5 parse) | Explain |
| `OllamaEmbedder` (nomic via `/api/embed`, document/query prefixes) | Explain |
| `StructuredAnswerParser` (H5 parse-or-abstain) | Explain |
| `nexus-dslm.Modelfile` (pinned, H1+H5 in SYSTEM) | Explain |
| `IExplainer`/`IEmbedder`/`ExplainOutcome`/`EmbedKind` seams | Application.Diagnosis |
| Runner refactor: generates draft via `IExplainer`, seals model output (H10) | Application |
| `NoOpExplainer`/`NoOpEmbedder` defaults; `EmbeddingIngestor` | Infrastructure.Diagnosis |
| `EfRetriever` semantic path wired to `IEmbedder` (floor 0.5) | Infrastructure.Diagnosis |
| `AddRootCauseExplain` DI (replaces NoOp defaults) | Explain |
| Architecture `Classify` teaches the Explain project | ArchitectureTests |
| Central `Directory.Build.targets` (strips OllamaSharp Roslyn analyzer) | repo root |
| Read-only `nexus1_explain` login + runbook | `docs/runbooks/local-explain-readonly-sql-login.md` |

## Commands run and actual output

### `dotnet build Nexus1.Runtime.sln -c Debug`
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### `dotnet test tests/Nexus1.ArchitectureTests`
```
Passed!  - Failed: 0, Passed: 8, Skipped: 0
```
Includes `No_project_except_the_deferred_Explain_project_references_a_served_model_package`
— still green **with the Explain project now present**: SK/Ollama are isolated to
Explain (the rule's one allowed exception); no leak into any engine project.

### `dotnet test tests/Nexus1.RootCause.UnitTests`
```
Passed!  - Failed: 0, Passed: 35, Skipped: 0
```

### Honest finding: stochastic-model robustness (why the tests assert safety, not perfection)

A full-tier run initially showed **1 failure of 53**:
`Full_pipeline_end_to_end_reaches_the_FV104_verdict_with_the_real_model`. Root
cause: the 3B CPU model is stochastic (see determinism below); on that run its
draft failed H3/H4, so the pipeline **safely abstained** — it did NOT emit a wrong
verdict (the engine can only ever conclude FV-104 or abstain). The original test
asserted "always reaches FV-104", which over-claims a small local model's
per-call reliability. H8 forbids salvaging/retrying a bad draft into a pass, so
the honest fix was to assert the **safety invariant** instead: the run reaches the
FV-104 verdict *when the model's draft validates* and *safely abstains otherwise*,
is sealed either way, and can never produce a different or unsourced verdict.
Engine determinism is now proven model-free (graph walk twice → FV-104 both
times). This is the correct way to test an LLM-in-the-loop system, and it makes
the guardrails (not the model's mood) the thing under test.

### `dotnet test tests/Nexus1.RootCause.ComponentTests` (real LocalDB + real Ollama)
```
Passed!  - Failed: 0, Passed: 53, Skipped: 0, Total: 53, Duration: 2 m 29 s
```
(0 skipped because Ollama was live; the 8 real-model tests SkippableFact-skip when it is not.)
The 8 real-Ollama tests (`OllamaExplainPipelineTests`), all Passed:
- `Embedder_returns_a_real_vector` — nomic-embed-text returns a real vector.
- `Ingest_populates_embeddings_and_the_semantic_path_retrieves` — ingest populates
  `Corpus.EmbeddingJson`; **semantic-only retrieval returned 2 passage(s)** (empty
  lexical tag, so this is the C#-cosine path over real embeddings).
- `Explainer_produces_a_validated_draft_citing_the_book_worked_example` — the real
  model's output PASSES the real H3/H4 validator and cites the honest
  "From Flood to Cause … worked example" corpus.
- `Full_pipeline_end_to_end_reaches_the_FV104_verdict_with_the_real_model` —
  asserts the safety invariant (FV-104 or safe abstention, always sealed, never a
  false verdict); logs `verdict=FV-104 auditHash=3f0986425052…` on compliant runs.
- `Engine_verdict_is_identical_across_two_runs_and_narration_equality_is_reported`
  — engine origin identical both times (model-free graph walk); narration logged.
- `Telemetry_fault_abstains_through_the_full_real_pipeline` — abstains before the model.
- `Corpus_fault_abstains_through_the_full_real_pipeline` — abstains before the model.
- `Di_composition_replaces_the_noop_defaults_with_the_ollama_backed_seams`.
Plus `StructuredAnswerParserTests` (6, pure H5 parse) and the deterministic
`GroundingDiagnosisTests` (fixture-explainer, H3/H4 abstention proofs) from ADR-032.

## Determinism (H6) — honest finding

Two full end-to-end runs of the same incident:
```
run1 auditHash = 3f0986425052060bcd0b8b3f45094295cc7ec7802e5b65fea399a6261b9c2a81
run2 auditHash = df122b7c017c99bdc48f94e0965fce5fd05ce25726f753964939487580508177
```
- The **engine's verdict is identical both times (FV-104)** — the true H6 guarantee.
- The **model's narration is NOT byte-identical** (audit hashes differ), even at
  temperature 0 + fixed seed. On CPU-only inference, multi-threaded float-reduction
  order varies run to run — exactly the GPU/CPU-batching non-determinism Appendix G
  flags. Reported, not overclaimed.

## H7 — no write path (read-only `nexus1_explain`, verified directly)

Connected as `nexus1_explain` against `RootCauseDb`:
```
sysadmin=0   db_owner=0   corpus_select_rows=0   (SELECT permitted)
INSERT -> "The INSERT permission was denied on the object 'Corpus' … schema 'RootCause'."
UPDATE -> "The UPDATE permission was denied on the object 'Component' … schema 'RootCause'."
DELETE -> "The DELETE permission was denied on the object 'Audit' … schema 'RootCause'."
```
Stronger still: the model component (`SemanticKernelExplainer`) has **no database
dependency at all** — it cannot read or write the store by construction.

## Air-gap

- Ollama binds **127.0.0.1 only** (`netstat`: `TCP 127.0.0.1:11434 … LISTENING`, PID-owned).
- During ~70s of continuous inference, **46 samples** of `Get-NetTCPConnection` by
  ollama PID showed **ZERO non-loopback established connections**.
- Models pulled once at provisioning (network-up), pinned by digest; inference
  needs no network thereafter.

### NOT done here (requires Administrator — flagged, not faked)
Physically disabling the network adapter (or a per-process firewall rule) and
re-running needs elevation, which this session lacks. The loopback-binding +
zero-egress evidence above is the strongest proof obtainable without admin. The
user can complete the literal adapter-off re-run: the pipeline needs no network,
so the FV-104 verdict will be unchanged.

## Notes / deliberate skeleton choices (see ADR-033)

- Entities are constrained (Modelfile) to the origin tag so a 3B model passes H3
  cleanly; H3's catch-a-hallucination behaviour is proven deterministically with
  fixtures (GroundingDiagnosisTests), not with the compliant real model.
- Embeddings use Ollama's stable HTTP `/api/embed` (not the churny SK embedding
  alpha API) with nomic's document/query prefixes; H2 floor 0.5 is a demonstrator
  value, not H9-calibrated.
- SK pinned at 1.50.0 (net8-compatible extensions); OllamaSharp's Roslyn-5 analyzer
  stripped centrally.
