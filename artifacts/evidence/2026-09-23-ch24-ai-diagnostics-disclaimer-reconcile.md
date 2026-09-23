# Evidence — reconcile the now-stale Ch.24 AI Diagnostics disclaimer

**Date:** 2026-09-23
**Type:** Copy/wording correction only. No feature change.
**Why:** The Ch.29 slice (66e1491) wired a real grounded, cited root-cause explanation
pipeline (the One-Truth Pipeline, ADR-032..ADR-035). That made the Ch.24 AI Diagnostics
screen's disclaimer — which asserted no such pipeline exists — an active inaccuracy in a
shipped screen. "Nothing claims to exist that does not" cuts both ways: a screen must not
claim something is absent once it is real.

## What changed (2 rendered paragraphs + 1 code doc comment)

### 1. DSLM advisory panel disclaimer (`ai-diagnostics.html`)

**Before:**
> Not running in this build. The advisory panel described here — natural-language questions
> answered with grounded citations into plant data — is future scope, not implemented,
> deployed, or wired to any live service in this console.

**After:**
> Not running in this build. This DSLM advisory — answering free-form natural-language
> questions with grounded citations into plant data — is future scope, not implemented,
> deployed, or wired to any service here. A real grounded, cited root-cause explanation
> pipeline does now exist, but only for the single fixed incident EVT-2026-0418 on the
> Root Cause screen — not the general question-answering advisory described here.

### 2. Example-interaction disclaimer (`ai-diagnostics.html`) — the primary now-false claim

**Before:**
> This example is scripted text from the source material, not a live query result. Component
> Registry does not exist anywhere in this build yet (Ch. 28, still unreached) and RootCause's
> real domain model (ADR-005) has no scored causal graph to cite — **nothing on this console
> can produce a cited explanation like this today, and no grounding or citation-generation
> pipeline is built here.**

**After:**
> This example is scripted text from the source material, not a live query result — this
> screen answers no free-form questions. Since the Root Cause slice (Ch. 29), the console
> does have a real grounding-and-citation pipeline that produces a cited root-cause
> explanation — but only for the single fixed incident EVT-2026-0418, on the Root Cause
> screen, not as a general capability invocable here or for arbitrary incidents or questions.

### 3. Component doc comment (`ai-diagnostics.ts`)

The original point-2 finding (accurate when Ch.24 was built) is preserved; a dated
"Update (2026-09-23, Ch. 29 reconciliation)" note corrects the now-false clause
("no citation-generation, grounding, or RAG pipeline of any kind is built here"),
recording that the pipeline now exists, is scoped to EVT-2026-0418, and is NOT invoked
by this screen.

## Accuracy discipline (what the new copy does and does not claim)

- **Does claim (true):** a grounded, cited root-cause pipeline exists now; it is scoped to
  one fixed incident (EVT-2026-0418); it lives on the Root Cause screen (Ch.29).
- **Does NOT overstate:** AI Diagnostics still does not answer free-form questions and does
  not invoke the pipeline; the pipeline is not a general capability or available for
  arbitrary incidents. The general DSLM advisory remains future scope.

## Linking decision

The corrected copy **names** the Root Cause screen but does **not** add a clickable
RouterLink. Rationale: keeps this a pure copy correction (no new import/interaction, no
scope creep), and a CTA from AI Diagnostics into Root Cause would overstate the
connection — Ch.29 deliberately keeps the causal verdict on its own screen, and AI
Diagnostics neither gateways nor invokes the RAG pipeline.

## Evidence

- **No other screen carries the stale claim** — grep across `console/nexus-console/src`
  for `no grounding | citation-generation | RAG pipeline | cited explanation | no ...
  pipeline is built | does not exist anywhere in this build` returned only the fixed
  `ai-diagnostics` file and an `incident-summary.spec.ts` assertion on the real backend
  **abstention reason** string ("no grounding: corpus retrieval returned no passages") —
  which proves the pipeline exists, not the opposite.
- **`npx jest` → 280 passed / 55 suites / 0 failed** — no regression (the ai-diagnostics
  spec asserts on states and `.case-status` styling, not the disclaimer copy).

## Scope boundary

Copy only. No behavior change, no new capability, no new dependency, no other screen
touched, no RouterLink added.
