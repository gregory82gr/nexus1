# ADR-042: Engine candidate selection and ranking from observables

## Status

Accepted. Removes the circularity recorded as out of scope in ADR-041: the graph walker no
longer reads the seeded `IllustrativeWeight` / `IllustrativeRole` fields for anything.

## Context

`EfGraphWalker` (ADR-032, extended by ADR-037) computed **coverage** honestly — a forward
walk over backbone and artefact edges, summing the flood's alarm counts it reaches — but
everything else about a candidate came from the seed:

- **Selection.** Only components carrying a seeded `IllustrativeWeight` were ranked (5 of
  10 in 0418, 3 of 7 in 0419). The seed chose who could be blamed.
- **Weight and role.** Copied from `IllustrativeWeight` / `IllustrativeRole`; only the
  origin's role was computed.
- **Order.** Coverage, then **seeded weight** as the tie-break — and that tie-break was
  load-bearing: PB-2A and RCP-1B both cover 7/14 in 0418, and PB-2A ranked second only
  because its seeded 0.20 beat RCP-1B's 0.07.

Any eval case or generated scenario that also wrote those fields would hand the engine its
own answer key. A generator cannot be trusted on top of that.

## Decision

The walker derives candidate set, order, weight and role from the authored graph plus the
incident's observables. The illustrative columns stay in the schema (no migration) as
**presentation-only** data: the graph reader still serves them to the console's book figure;
the walker never reads them.

### What stays authored (the maintained causal graph + the incident's facts)

Edge `Kind` (backbone / artefact / learned / rejected), delay windows and `SourceRef`; the
component list, `Tag`, `Kind`; the incident identity (`AlarmedComponentIds`, flood start);
the corpus. Observable **inputs**: `AlarmCount` (the flood's alarm tally — a future generator
may supply it only as the true count of the alarms it generated) and historian samples (raw
signals).

### Candidate set

Every component on the incident's unit that is **alarmed in the flood** or is the **source
of any authored edge** (any kind) on that unit. This admits the cascade's intermediate
nodes and the non-alarmed hypotheses the graph itself names (an artefact source like
BKR-2A; rejected-edge correlates like FT-7 and LOF-1), and excludes nodes that are only
ever edge targets (the flat witnesses RTD-1, NFX-1).

### Ranking — greedy set cover over the flood's alarms

Each candidate's **explained set** is the alarmed nodes reachable from it over backbone and
artefact edges, itself included (unchanged from today); **coverage** = its alarm count /
the flood's total (unchanged).

1. Repeatedly pick the candidate that explains the most alarms **not already explained** by
   earlier picks. Ties: **earlier historian onset** (MIN timestamp of its Quality==0
   samples — the corroborator's own onset definition; a candidate with no samples sorts
   after any onset), then **lower ComponentId**.
2. Stop when the best pick would add zero alarms. The first pick is the **origin** — the
   same rule as today's "highest coverage", since nothing is explained yet.
3. Candidates never picked follow, ordered by coverage (descending), then onset, then
   ComponentId.

### Weight — attribution share

A candidate's weight is the number of alarms it newly explained when picked, divided by the
flood's total: its share of the flood explained beyond what earlier-ranked candidates
already covered. Never-picked candidates weigh 0. The weights sum to the explained fraction
of the flood, so an unexplained remainder stays visible rather than being normalised away.

### Roles — structural, relative to the origin O

"O's reach" is every node reachable from O over walkable edges, O included. Evaluated in
this order:

| Role | Rule |
|---|---|
| `origin` | the first greedy pick (explains ≥ 1 alarm) |
| `downstream` | reachable from O — its alarms are a subset of O's explanation |
| `ruled-out` | explains no alarm in this flood (coverage 0) |
| `parallel` | not reachable from O, and its own reach intersects O's reach (a strand converging into the cascade) |
| `contributing` | not reachable from O, and has a **learned** edge into O's reach |
| `independent` | explains alarms with no structural connection to O's cascade |

If the flood has no alarms (no origin), every candidate is `ruled-out` and the runner
abstains as today ("graph walk found no origin candidate").

### "Proximate" has no honest rule — it is `downstream` in the engine

The book's "proximate" labels the loud symptom a naive reader would blame (PB-2A in 0418,
FT-9 in 0419). Coverage, timing and topology do not define it: any rule that happens to
select PB-2A and FT-9 (e.g. "most alarms among downstream nodes, then lowest health") would
be fitted to the answer — the circularity again in another form. So the engine calls them
`downstream` with weight 0, which is the book's actual point (they are symptoms FV-104 /
BKR-2A already explain). The "proximate" label survives only as the book-figure annotation
the console's graph view reads from `IllustrativeRole`.

## Consequences — the behaviour change, disclosed

**Verdicts are unchanged.** The origin was already the top-coverage candidate and the top
coverage is unique in both incidents (FV-104 11/14 vs the next, SG-1, 10/14; BKR-2A 10/10 vs
FT-9 4/10), so corroboration, retrieval, explain and the sealed verdict all receive the same
origin. 0420 never reaches the walker.

**Weights, roles and the candidate list change.** The book's illustrative numbers are not
reproducible from observables and are not reproduced.

EVT-2026-0418 (total 14 alarms):

| Candidate | Before (seeded role, weight) | After role | After weight | Coverage |
|---|---|---|---|---|
| FV-104 | origin 0.66 | origin | **0.79** (11/14) | 0.79 |
| RCP-1B | parallel 0.07 | parallel | **0.14** (2/14) | 0.50 |
| BUS-2A | contributing 0.05 | contributing | **0.07** (1/14) | 0.07 |
| SG-1 | — (not ranked) | downstream | 0 | 0.71 |
| FWP-2A | — | downstream | 0 | 0.57 |
| PB-2A | proximate 0.20 | **downstream** | **0** | 0.50 |
| LF-1 | — | downstream | 0 | 0.36 |
| CT-1 | — | downstream | 0 | 0.21 |
| RT-1 | — | downstream | 0 | 0.07 |
| FT-7 | ruled-out 0.02 | ruled-out | **0** | 0 |

EVT-2026-0419 (total 10 alarms):

| Candidate | Before | After role | After weight | Coverage |
|---|---|---|---|---|
| BKR-2A | origin 0.70 | origin | **1.00** (10/10) | 1.00 |
| FT-9 | proximate 0.16 | **downstream** | **0** | 0.40 |
| LT-3 | — (not ranked) | downstream | 0 | 0.30 |
| PT-7 | — | downstream | 0 | 0.30 |
| LOF-1 | ruled-out 0.10 | ruled-out | **0** | 0 |

(LT-3 and PT-7 tie on coverage and onset; LT-3 ranks first on the lower ComponentId.)

Knock-on effects: persisted Candidate rows and the API response grow (5→10, 3→5); new
audit-chain hashes differ (already-sealed runs are untouched — the chain is append-only);
the console's origin percentage reads 79% instead of 66% (Root Cause Graph and Incident
Analysis) and the candidate table shows 0% weight for downstream symptoms. The table
**already has a Coverage column** beside the weight (the plan assumed it did not; checked
against the live render), so a downstream symptom reads e.g. PB-2A "0% / 50%" — its
attribution next to its reach. **No console change in this slice.** The named, deferred
follow-up is **wording**, not a column: the weight column header ("Illustrative weight"),
the candidate-table footnote ("Weights and coverage are illustrative figures from the
engineered graph") and Incident Analysis's "Illustrative confidence." caption now
misdescribe a derived attribution share, and should be relabelled in a UI slice.

**Known limitation.** Roles are relative to the origin only (as specified). A symptom of a
*second*, independent explanation would be labelled `independent` rather than "downstream
of that explanation". No built incident has one; see the reversal condition.

The H9 golden cases' `expectCandidates` are rewritten to the tables above; every other
expectation (`expectOriginCoverage`, `expectVerdict`, `mustNotAssert`, `mustCite`,
`mustNotCite`) is unchanged, as is the case format.

## Rejected alternatives

- **A fitted "proximate" rule.** Rejected above: reproducing the book's label from a rule
  chosen because it reproduces the label is circular.
- **Weight = coverage.** Simpler, but duplicates the Coverage field and double-counts every
  alarm a symptom shares with its cause (weights would sum to far more than 1).
- **Normalising to the book's figures.** No observable formula yields 0.66/0.20/0.07/0.05/
  0.02; scaling to them would be the answer key again.
- **Filtering the list to non-zero weights + ruled-out.** Shorter, but hides the symptoms
  the engine considered and cleared; the fuller list is the honest output.
- **Dropping the illustrative columns.** Needs a migration and breaks the console's book
  figure for no engine benefit; the poisoned-seed test enforces that the walker ignores them.

## Scope boundary

No physics generator, no new incidents (the pure-observable fixture is test data on an
unused unit, never registered or provisioned), no change to the H9 case format (only
expected values), no change to the Explain/LLM stage, corroborator or retriever, no schema
migration, no console change.

## Reversal condition

If an incident with two independent explanations is built, extend `downstream` to "reachable
from any earlier greedy pick" (recording which). If a probabilistic model replaces coverage
(real posteriors), weight becomes that posterior and this ADR is superseded.

## Evidence required

Before/after walker output for 0418 and 0419 (verdicts identical, each change matching the
tables); a poisoned-seed test proving byte-identical output with adversarial illustrative
values; a pure-observable fixture with no illustrative values, hand-worked; full RootCause
regression per assembly in isolation; H9 deterministic + live tiers; build 0/0;
ArchitectureTests; the live cross-screen E2E once.

See `artifacts/evidence/2026-09-24-adr-042-candidates-from-observables.md`.
