# ADR-032: RootCause grounding walking skeleton — deterministic engine now, LLM explain deferred

## Status

Accepted (deterministic half). The natural-language generation stage it
describes is explicitly **deferred, not done** — see "What is deliberately not
built yet".

## Context

`From Flood to Cause` (a NEXUS-1 companion book) describes a root-cause
diagnosis pipeline — the "One-Truth Pipeline" — in which a deterministic engine
*decides* and a served language model *explains*, with ten anti-hallucination
controls (H1–H10) making confident fabrication structurally hard. The book's
Chapters 8–9 give the C# and SQL production form of that pipeline; Appendix C
gives the grounding schema; Figure 2.1 (Ch.2) gives the worked incident
EVT-2026-0418 as a causal fault-propagation graph with delay windows.

This book is **not** one of the seven authority sources in `CLAUDE.md` §1 and
was deliberately never copied into `docs/source-material/`. It is nonetheless the
direct source for this slice: the project owner and Claude (the architects, in
Claude.ai) chose to build EVT-2026-0418 as a fixed-incident *walking skeleton*
on top of the existing RootCause context. The standing rule "the PDFs win; flag
the conflict" applies — this ADR is that flag. The book stays out of
`source-material/`; the values it supplies to this slice are pinned here and in
`GroundingSeed` instead, so the implementation traces to a recorded decision
rather than to a PDF the repo does not carry.

Two environmental facts shape the decision:

1. The available database is **SQL Server 2016**. The book's Appendix C corpus
   table declares `Embedding VECTOR(1024)` and its Listing 9.2 retrieval uses
   `VECTOR_DISTANCE('cosine', …)` and `AI_GENERATE_EMBEDDINGS(… USE MODEL …)` —
   all **SQL Server 2025 preview** features, unavailable on 2016.
2. The served model half (Semantic Kernel + Ollama, Listing 9.4, model
   `nexus-dslm`, plus an embedding model for the corpus) needs an Ollama install
   and a RAM upgrade that are **not yet in place**.

The prior architects' resolution of Figure 2.1's exact delay windows was never
persisted to the repo. It was re-derived here directly from the book (pages 6
and 8, `pdftotext -table`) and cross-checked arithmetically; see "Figure 2.1
derivation" below.

## Decision

**Build the entire deterministic, LLM-free half of the pipeline now, against
SQL Server 2016, and defer only the generation stage — visibly unbuilt, never
faked.**

### What is built (this ADR)

- **Grounding store** (`RootCause` schema, migration `AddGroundingStore`):
  `Component`, `Edge`, `Historian`, `Corpus`, `DiagnosisRun`, `Candidate`,
  `Audit` (named `AuditEntry` in the domain to avoid colliding with the Audit
  bounded context). One `IEntityTypeConfiguration` per entity, Fluent API only.
- **The four seams** (`Application.Diagnosis` interfaces; `Infrastructure.Diagnosis`
  implementations), all LLM-free:
  - `EfGraphWalker` — the node test: walk the backbone forward, compute coverage
    as the share of the flood explained, compute the origin.
  - `EfTelemetryCorroborator` — read physical onsets from the historian and
    confirm each backbone edge fits its modelled delay window.
  - `EfRetriever` — hybrid retrieval: exact lexical tag match, plus a C#-cosine
    semantic path that stays dormant until embeddings exist (H2 floor applied).
  - `RegistryAntiHallucinationValidator` — H3 (entity in registry) and H4
    (claim cites a retrieved passage); one miss abstains.
- **`Sha256AuditChainWriter`** — H10, `Hash = SHA-256(PrevHash + Payload)` from a
  fixed genesis, matching Listing 9.5.
- **`FixedIncidentDiagnosisRunner`** — composes the seams; any gate's veto yields
  a named abstention with no verdict (inconclusive is never a silent pass).
- **`GroundingSeed`** — the resolved EVT-2026-0418 fixture, single source of truth.
- **New architecture rule** —
  `DependencyLawTests.No_project_except_the_deferred_Explain_project_references_a_served_model_package`
  fails the build if any served-model / LLM package leaks into an engine
  project, reserving `Nexus1.RootCause.Explain` as the one future home for it.

### Corpus labelling (H-controls, honesty)

`Corpus.SourceLabel` names what the corpus **actually is**: the book's own
EVT-2026-0418 worked-example passages ("From Flood to Cause — NEXUS-1 Companion,
worked example, Ch.1-2"). It is **never** a fabricated plant procedure or vendor
manual this project does not have. The citation line the future explain step
emits will therefore cite the book, not a fictional plant document. This is a
deliberate anti-fabrication choice, not a placeholder to be "filled in later."

### Cosine in C#, defer SQL Server 2025

Cosine similarity is computed in C# (`CosineSimilarity`), not via SQL Server
2025's native `VECTOR_DISTANCE`. `Corpus.EmbeddingJson` is a plain
`nvarchar(max)` column holding a `float[]` as JSON, left **null** until an
embedding model is available. Rationale: (1) the walking skeleton's corpus is a
handful of chunks — native vector search is unwarranted; (2) adopting the VECTOR
type would force a major-version engine migration off the available SQL Server
2016; (3) the lexical path needs no embeddings, so the engine can ground on an
exact tag today and light up the identical C#-cosine path the day embeddings
land — proven math waiting on real inputs, not a stub.

### Figure 2.1 derivation (recovered, with confidence noted)

Backbone edges (walkable, `DelayMin == DelayMax` exact point delays):
FV-104 →36s→ SG-1 →18s→ FW Pump 2A →42s→ Pump 2A Bearing →12s→ Loop Flow
→30s→ Core T →4s→ Reactor Trip; and RCP-1B →10s→ Loop Flow (parallel strand).
Four of these (36/18/42/4) are stated in the book's prose; the rest follow from
the `-table` figure layout and the prose cascade ordering. The learned edge
4kV Bus →2s→ FW Pump 2A carries the only medium-confidence value (the 2s is the
sole unassigned label); it is **immaterial to the verdict** because learned
edges are never walked as backbone. The rejected edge FT-7 → Loop Flow is kept
visible and never walked.

Independent check: summing seeded alarm counts over FV-104's backbone-forward
reachable set (1+2+1+2+2+2+1) = **11 of 14**, exactly the book's figure —
computed by the walk, not hard-coded. RCP-1B and the bearing each cover 7, so
FV-104 wins the origin role from the data.

## Consequences

- The engine side is real and provable today on SQL Server 2016 / LocalDB with
  no model, no broker, no HTTP: 15 component tests (real DB, real migrations,
  real seams) and 19 unit/architecture tests pass.
- Every conclusion and abstention is sealed into a tamper-evident hash chain.
- The walking skeleton's status is honestly partial: it decides and grounds, but
  it does not yet generate prose. Anyone reading `DiagnosisResult` sees a verdict
  or a named abstention, never a fabricated narrative.
- `RootCauseAnalysis` (the human-owned investigation aggregate, ADR-005) is
  untouched; `DiagnosisRun` is the engine's separate record.

## What is deliberately not built yet (deferred, not done)

- **`Nexus1.RootCause.Explain`** — the Semantic Kernel + Ollama wiring of
  Listing 9.4. No project, no package reference, no code.
- **The pinned `nexus-dslm` served model** and the corpus **embedding model**
  (e.g. nomic-embed-text). Until the embedding model exists, `EmbeddingJson`
  stays null and the C#-cosine path stays dormant by design.
- **The air-gap proof** (H7's read-only boundary demonstrated end to end with a
  local, network-isolated model).

These wait on the 16GB RAM upgrade and an Ollama install genuinely being in
place. No placeholder LLM output and no simulated model response has been
produced; the generation stage is left visibly unrun. A future ADR records it
when it lands, with real evidence.

## Rejected alternatives

- **Adopt SQL Server 2025 VECTOR now.** Rejected: forces a major-version engine
  migration for a corpus that does not need vector search; the C#-cosine path
  gives identical semantics when embeddings arrive.
- **Fabricate a plausible LLM narrative / stub the model.** Rejected outright —
  it is the exact failure mode this whole project is built against. The draft
  answer is a caller-supplied fixture, validated by H3/H4 identically to a real
  model's output; it is never presented as model-generated.
- **Invent plant documents for the corpus.** Rejected; the corpus is labelled
  honestly as the book's worked example.
- **Copy `From_Flood_to_Cause.pdf` into `source-material/`.** Rejected; it stays
  outside the seven-source authority list. Its contribution to this slice is
  pinned here and in `GroundingSeed` instead.
- **Numbering as ADR-031.** The in-progress code referenced "ADR-031", but that
  number was already taken (`ADR-031-latest-per-parent-linq-translation-pattern`);
  the code references were corrected to ADR-032.

## Reversal condition

Revisit if: the RAM/Ollama environment lands (then build `Nexus1.RootCause.Explain`
and the air-gap proof under a new ADR); the demonstrator grows beyond a single
fixed incident (then coverage/weight become per-run inputs, not registry
columns, and unit scoping in `EfRetriever` becomes a real discriminator); or the
database moves to SQL Server 2025 (then the C#-cosine path can be reconsidered
against native `VECTOR_DISTANCE`, re-run against an H9 evaluation set first).

## Evidence required

- `dotnet build Nexus1.Runtime.sln` clean.
- `Nexus1.RootCause.ComponentTests` (LocalDB) green, including the 11-of-14
  coverage computation and the four fault-injection abstention proofs.
- `Nexus1.RootCause.UnitTests` (cosine math, runner veto logic) green.
- `Nexus1.ArchitectureTests` green, including the served-model-package rule.

See `artifacts/evidence/2026-09-07-rootcause-grounding-walking-skeleton.md`.
