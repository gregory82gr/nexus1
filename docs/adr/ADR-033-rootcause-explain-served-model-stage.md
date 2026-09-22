# ADR-033: RootCause Explain stage — the served-model half, air-gapped

## Status

Accepted. This closes out the deferred fifth stage of ADR-032; the EVT-2026-0418
fixed-incident walking skeleton is now complete end to end (engine decides →
model explains → H3/H4 validates → audit seals), running against a local,
loopback-only model.

## Context

ADR-032 built the deterministic, LLM-free half of the One-Truth Pipeline and
deferred the natural-language generation stage (Semantic Kernel + Ollama,
Listing 9.4) until the hardware was in place. The 16GB RAM upgrade and disk are
now installed, so the served-model half is built here.

Environment as built: Windows 10, .NET 8 SDK (Roslyn 4.11), **Intel i5-3340M CPU,
16GB RAM, Intel HD 4000 (no CUDA GPU) — CPU-only inference**. Ollama 0.34.2,
user-scope install, loopback server on 127.0.0.1:11434.

## Decision

**Build `Nexus1.RootCause.Explain` as the one and only project allowed to depend
on a served language model, and wire it behind the `IExplainer`/`IEmbedder`
seams so the deterministic engine stays LLM-free.**

### Pipeline shape (runner refactor)

`FixedIncidentDiagnosisRunner` now composes the full pipeline: graph walk →
telemetry corroboration → corpus retrieval → **`IExplainer` generation** → H3/H4
validation → audit seal. The draft is no longer a caller parameter; it comes from
the `IExplainer` seam (a fixture in tests, the real Ollama-backed one in
production), validated identically. The runner depends only on the interface, so
it carries no served-model package.

### The served model — `nexus-dslm`

A pinned Modelfile (`src/Contexts/RootCause/Nexus1.RootCause.Explain/nexus-dslm.Modelfile`)
`FROM qwen2.5:3b-instruct-q4_K_M` with determinism pinned (temperature 0, seed 42,
top_p 1, repeat_penalty 1, num_ctx 8192, stop `<|im_end|>`). The **H1 closed-world
contract and the H5 output schema live in the Modelfile SYSTEM block**, so the
contract travels with the pinned model; the explain code sends only the Unified
Context. Reproducibility record (pin by digest, not tag):

- Ollama runtime: **0.34.2**
- `nexus-dslm` base blob: **sha256:5ee4f07cdb9beadbbb293e85803c569b01bd37ed059d2715faa7bb405f31caa6**
  (= `qwen2.5:3b-instruct-q4_K_M`, id `357c53fb659c`)
- `nomic-embed-text` id: **0a109f422b47**
- Hardware class: Intel i5-3340M (CPU-only), 16GB RAM

### Generation via Semantic Kernel

`SemanticKernelExplainer` builds a kernel over the local Ollama model with
**`FunctionChoiceBehavior.None()`** (no planner, no tool-calling), temperature 0,
one `GetChatMessageContentAsync` call, parsed to the H5 schema
(`StructuredAnswerParser`). A refusal, an unparseable reply, or a missing cause is
a real abstention (H8) — never salvaged. **Entities are constrained (Modelfile
rule 2) to exactly the origin tag**, so the model names only what the engine
already decided; H3 then validates that tag against the registry. This is a
deliberate skeleton constraint — H3's hallucination-catching on arbitrary bad
drafts is proven deterministically with fixtures (GroundingDiagnosisTests), while
the real model is kept to output that passes H3 cleanly.

### Embeddings via Ollama HTTP + nomic prefixes

`OllamaEmbedder` calls Ollama's stable `/api/embed` directly rather than the
experimental SK embedding abstraction (which churns across alpha releases) — still
"via nomic-embed-text", the same served runtime. The `IEmbedder` seam carries an
`EmbedKind` (Document/Query) so nomic-embed-text's asymmetric task prefixes
(`search_document:` / `search_query:`) are applied correctly. The H2 relevance
floor is **0.5**, tuned to nomic's cosine range with those prefixes — a
demonstrator value, not an H9-calibrated one. `EmbeddingIngestor` (a
provisioning write, LLM-free, depends only on `IEmbedder`) populates
`Corpus.EmbeddingJson`, lighting up `EfRetriever`'s C#-cosine semantic path.

### Package pinning (net8 constraints)

Semantic Kernel **1.50.0** + Connectors.Ollama **1.50.0-alpha** (not latest 1.80.1):
1.80.1 transitively demands the .NET 10 `Microsoft.Extensions.*` wave (10.0.x),
which NU1605-downgrades against the solution's net8 8.0.x pins. 1.50.0 depends on
`Microsoft.Extensions.DependencyInjection.Abstractions` 8.0.2 (the solution pin),
so it composes cleanly. OllamaSharp (pulled transitively) ships a Roslyn source
generator built against a newer compiler than the net8 SDK's (CS9057); its
analyzer is stripped centrally in `Directory.Build.targets` (path-matched, a no-op
where absent). SKEXP0001/SKEXP0070 experimental diagnostics are suppressed only in
the Explain project.

### H7 — no write path

The read-only `nexus1_explain` login (db_datareader + explicit DENY
INSERT/UPDATE/DELETE/EXECUTE on the RootCause/messaging schemas) is provisioned
per `docs/runbooks/local-explain-readonly-sql-login.md`, following ADR-028's
pattern. Verified directly: SELECT succeeds; INSERT/UPDATE/DELETE each fail with
an explicit permission-denied error; not sysadmin, not db_owner. **Stronger still:
the model component (`SemanticKernelExplainer`) has no database dependency at all**
— it receives passages and returns a draft, so it structurally cannot read or
write the store. The read-only login guards the explain-side *retrieval*
connection; wiring a dedicated read-only retrieval DbContext into a host that runs
this pipeline is the recorded next step.

### Air-gap

The Ollama server binds **127.0.0.1 only** (verified via netstat, PID-owned).
During ~70s of continuous inference (46 samples), **zero non-loopback established
connections** were observed from any ollama process (`Get-NetTCPConnection` by
PID). Models are pulled once at provisioning (network-up) and pinned by digest;
inference thereafter needs no network.

## Consequences / honest boundaries

- **Determinism (H6):** the engine's verdict is identical across runs (FV-104,
  proven twice). The model's **narration is NOT byte-identical** across runs even
  at temperature 0 + fixed seed — CPU multi-threaded float-reduction order varies,
  exactly the GPU/CPU-batching non-determinism Appendix G flags. The audit chain
  seals whatever was actually produced; the *decision* is the deterministic
  guarantee, not the prose. Tests report this, they do not overclaim it.
- **Stochastic-model test strategy:** because a small CPU model is not reliable
  per call, the real-model tests assert the pipeline's *safety invariant* (reaches
  FV-104 when the draft validates, else safely abstains — never a wrong or
  unsourced verdict, always sealed) and log the actual outcome, rather than
  demanding the model comply every run (which would flake, and which H8's
  no-salvage rule forbids papering over with retries). Engine determinism is
  proven model-free (graph walk twice). H3/H4's catch-a-hallucination behaviour is
  proven deterministically with fixtures (ADR-032's GroundingDiagnosisTests).
- **The physical adapter-disable air-gap step is NOT done here:** disabling the
  network adapter (or a per-process firewall rule) requires Administrator, which
  this session does not have. The loopback-binding + zero-egress evidence above is
  the strongest proof obtainable without elevation; the user can complete the
  literal adapter-off re-run themselves (the pipeline needs no network, so the
  verdict will be unchanged).

## Rejected alternatives

- **Latest SK 1.80.1** — rejected: forces the .NET 10 Extensions wave onto a net8
  solution (NU1605). Revisit if the solution moves to .NET 10.
- **SK embedding abstraction for embeddings** — rejected: alpha API churn; Ollama's
  HTTP `/api/embed` is stable and equally "via Ollama".
- **Fabricating or forcing byte-identical narration** — rejected; the honest
  finding (deterministic decision, non-deterministic prose on CPU) is reported.
- **Letting the model enumerate all cascade components as entities** — rejected for
  the skeleton: a 3B model lists prose names that aren't registry tags, so H3
  (correctly) abstains; constraining entities to the origin tag gives a clean,
  reproducible H3 pass while fixtures prove H3's catch behaviour.

## Reversal condition

Revisit if: the solution targets .NET 10 (adopt latest SK, drop the analyzer
strip); a GPU is added (revisit narration determinism); or the demonstrator grows
past one fixed incident (entities/retrieval floor become per-incident, H9
calibration replaces the hand-set floor).

## Evidence required

- `dotnet build Nexus1.Runtime.sln` clean.
- `Nexus1.ArchitectureTests` green, incl. the served-model-package rule with the
  Explain project now present (the rule's one allowed exception).
- `Nexus1.RootCause.ComponentTests` green: the deterministic tier, the H5 parser
  tier, and the 8 real-Ollama tests (embedder, ingest+semantic retrieval,
  explainer→validated draft citing the book, full pipeline verdict, two-run
  determinism, both fault-injection abstentions, DI composition).
- H7 login permission proof (SELECT ok; INSERT/UPDATE/DELETE denied).
- Air-gap loopback + zero-egress capture.

See `artifacts/evidence/2026-09-22-rootcause-explain-airgapped-e2e.md`.
