# Evidence — RootCause grounding walking skeleton (deterministic half)

**Date:** 2026-09-07
**ADR:** ADR-032
**Scope:** The LLM-free half of the One-Truth Pipeline for the fixed incident
EVT-2026-0418. The Semantic Kernel + Ollama generation stage is **deferred, not
built** (see "Deferred" below).

## Built

| Piece | Location | Status |
|---|---|---|
| Grounding store (7 tables) | migration `20260907111135_AddGroundingStore`, `RootCause` schema | Built + applied by tests |
| Domain entities + EF configs | `Domain/Grounding/*`, `Infrastructure/Persistence/Configurations/Grounding/*` | Built |
| `EfGraphWalker` (node test, coverage, origin) | `Infrastructure/Diagnosis` | Built + tested |
| `EfTelemetryCorroborator` (delay-window fit) | `Infrastructure/Diagnosis` | Built + tested |
| `EfRetriever` (lexical + dormant C#-cosine) | `Infrastructure/Diagnosis` | Built + tested |
| `RegistryAntiHallucinationValidator` (H3/H4) | `Infrastructure/Diagnosis` | Built + tested |
| `Sha256AuditChainWriter` (H10) | `Infrastructure/Diagnosis` | Built + tested |
| `EfDiagnosisRunStore` | `Infrastructure/Diagnosis` | Built + tested |
| `FixedIncidentDiagnosisRunner` composition | `Application/Diagnosis` (pre-existing) | Tested |
| `GroundingSeed` (resolved Figure 2.1 fixture) | `Infrastructure/Diagnosis` | Built + used by tests |
| DI wiring | `Infrastructure/ServiceCollectionExtensions` | Built |
| Architecture rule (engine is LLM-free) | `ArchitectureTests/DependencyLawTests` | Built + passing |
| ADR-032 | `docs/adr/` | Written |

## Commands run and actual output

### `dotnet build Nexus1.Runtime.sln -c Debug`
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### `dotnet test tests/Nexus1.RootCause.ComponentTests` (real LocalDB, real migrations, no mocks)
```
Passed!  - Failed: 0, Passed: 39, Skipped: 0, Total: 39, Duration: 59 s
```
Of these, 15 are the new `GroundingDiagnosisTests`, including:
- `Graph_walk_computes_FV104_as_origin_explaining_eleven_of_fourteen` — coverage
  computed from seeded alarm counts, `round(coverage × 14) == 11`, origin = FV-104.
- `Telemetry_corroboration_fits_the_delay_windows_for_the_origin_cascade` and the
  missing-channel fault → fail.
- `Retrieval_grounds_lexically_on_the_tag_with_an_honest_source_label` (asserts
  "From Flood to Cause" / "worked example" label) and the absent-tag → empty.
- Validator H3 (unregistered entity) and H4 (uncited claim) → fail; valid → pass.
- `Audit_chain_links_each_entry_to_the_previous_hash_from_genesis`.
- Full pipeline: FV-104 verdict + sealed run, and **four abstention proofs**
  (telemetry cannot corroborate / nothing to ground on / unregistered entity /
  uncited passage) — each a named abstention with no verdict.

### `dotnet test tests/Nexus1.RootCause.UnitTests`
```
Passed!  - Failed: 0, Passed: 34, Skipped: 0, Total: 34, Duration: 164 ms
```
New: 7 `CosineSimilarityTests` (synthetic vectors) + 5 `FixedIncidentDiagnosisRunnerTests`
(composition/veto logic with in-memory fakes — happy path + each gate's abstention).

### `dotnet test tests/Nexus1.ArchitectureTests`
```
Passed!  - Failed: 0, Passed: 8, Skipped: 0, Total: 8, Duration: 944 ms
```
New: `No_project_except_the_deferred_Explain_project_references_a_served_model_package`.

## Owned

- The engine's diagnosis record (`DiagnosisRun`, `Candidate`) is owned by the
  RootCause context, separate from the human-owned `RootCauseAnalysis` (ADR-005).
- The grounding store tables live under the `RootCause` schema in the RootCause
  database — same ownership boundary as the rest of the context.

## Deferred — NOT built, NOT faked, NOT run

- `Nexus1.RootCause.Explain` (Semantic Kernel + Ollama, Listing 9.4): no project,
  no package, no code. The architecture test above guards this boundary.
- The pinned `nexus-dslm` served model and the corpus embedding model:
  `Corpus.EmbeddingJson` is null; the C#-cosine path is dormant by design.
- The air-gap proof (H7 read-only boundary end-to-end with a local model).

These wait on the 16GB RAM upgrade + Ollama install. No placeholder or simulated
model output was produced. The walking skeleton is honestly partial: it decides
and grounds; it does not yet generate prose.

## Note on Figure 2.1

The prior architects' resolution of Figure 2.1's delay windows was never
persisted to the repo. It was re-derived from `From_Flood_to_Cause.pdf`
(`pdftotext -table`, pages 6 and 8) and cross-checked: FV-104's 11-of-14 coverage
falls out of the seeded alarm counts, which independently confirms the topology.
One value is medium-confidence — the learned 4kV→pump edge's 2s — but it is
immaterial to the verdict (learned edges are never walked). Full derivation in
ADR-032. If the owners' original resolution differs, the discrepancy is visible
here and in `GroundingSeed` and is a one-line change.
