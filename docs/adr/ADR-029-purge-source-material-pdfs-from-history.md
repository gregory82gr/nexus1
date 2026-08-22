# ADR-029: Purge licensed source-material PDFs from the repository and its history

## Status

Accepted.

## Context

`docs/source-material/` (and its `From_Service_To_Runtime/` subfolder)
held nine copyrighted PDF copies of the NEXUS-1 Companion book series
used as this project's specification source — checked in from the
repository's very first commit. This repository's remote
(`origin`, `https://github.com/gregory82gr/nexus1.git`) is a **public**
GitHub repository (confirmed directly: `GET
https://api.github.com/repos/gregory82gr/nexus1` returns
`"private": false`), and a direct fetch of a file under
`docs/source-material/` via `raw.githubusercontent.com` returned `200` —
genuine, current, verified exposure, not a hypothetical risk.

`git log --all --full-history -- docs/source-material/` shows exactly one
commit ever touched this path: the repository's own initial commit. Since
every commit in both `master` and `v1.0.0` (and every other ref) descends
from that initial commit, the PDFs exist in the history of every branch,
not merely one.

## Decision

**Remove the nine PDFs from the working tree, `.gitignore` the pattern
that matches them, and rewrite git history to remove every blob of every
one of them from every commit and branch — then force-push the rewritten
history to `origin`.**

### What is removed and what is kept

The nine PDFs (five directly under `docs/source-material/`, four under
`docs/source-material/From_Service_To_Runtime/`) are removed from the
working tree and purged from history. `docs/source-material/
ProjectDescriptionOfNexus.txt` — a three-line, non-copyrighted project
description with no licensed content — is kept, in the working tree and
in history; it was never the exposure this ADR addresses.

### `.gitignore`

`docs/source-material/**/*.pdf` added — covers both the top-level PDFs
and the nested `From_Service_To_Runtime/` subfolder in one pattern, so a
future session cannot accidentally re-stage any of them, regardless of
which of the two locations a new or replaced book copy lands in.

### History rewrite tool and method

`git filter-repo` (the tool git's own documentation recommends over
`git filter-branch`, and the tool actively maintained by a Git core
contributor) run with `--path` arguments targeting exactly the nine PDF
paths and `--invert-paths`, removing those blobs from every commit that
ever contained them, across every local branch and ref, not merely the
currently checked-out branch. `ProjectDescriptionOfNexus.txt` is
untouched by the rewrite since it was never named in the path list.

### Local copies remain available outside the repository

The PDFs' real "master copies" already live outside this repository, in
the developer's own `Downloads`/`Desktop` folders (confirmed directly —
e.g. `From_Trial_to_Policy.pdf` was located at `C:\Users\USER\Downloads
\Books&Application-.../Books&Application\From_Trial_to_Policy.pdf` during
the ReinforcementLearning sector's research, entirely outside
`C:\dev\nexus1`). Removing them from the repository does not remove the
developer's own access to the source material — only stops the
repository itself from redistributing licensed content publicly.

## Consequences

- Every commit hash in the repository's history changes from this point
  onward — the rewrite is not additive. Anyone who has already cloned,
  forked, or fetched this repository (including GitHub's own caches, any
  fork, any local clone made before the force-push) retains the original
  history, PDFs included, and will not automatically receive the
  rewritten history; their copies must be independently addressed (see
  "Explicit limitation" below).
- Both `master` and `v1.0.0` (and any other ref that existed before the
  rewrite) are rewritten and force-pushed. A collaborator with an
  existing clone will need to re-clone or hard-reset to the new history
  rather than pull normally, since the old and new histories share no
  common lineage from the initial commit forward.
- A full local backup (`git bundle create ... --all`) was taken before
  any rewrite began, so the pre-rewrite state is recoverable locally if
  the rewrite itself needs to be undone before the force-push (once
  pushed, the remote's own prior state is what would need restoring, from
  the same bundle, if ever needed).
- No application code, build output, or test behavior changes — this is
  a repository-hygiene and licensing-exposure fix, not a functional
  change to `Nexus1.ModularRuntime`/`Nexus1.RootCause.Host` or any
  context.

## Explicit limitation — recorded plainly, not glossed over

**This does not retroactively remove any copy already obtained by someone
else.** Git history rewriting only controls what this repository's own
remote serves from this point forward. It cannot and does not reach:

- Anyone who already cloned or forked the repository before the
  force-push — their local copy (and any fork's own remote) still has the
  original history with the PDFs, untouched by anything done here.
- GitHub's own internal caching, CDN edges, or any point-in-time
  archival/mirroring service (e.g. web archives, code-search indexers)
  that may have already crawled or cached the raw file content while it
  was public.
- Anyone who downloaded the PDF content directly (e.g. via the
  `raw.githubusercontent.com` URL confirmed reachable above) without
  cloning the repository at all — a file download leaves no trace this
  project's own git history could ever have controlled.

Nothing about `git filter-repo`, BFG, or a force-push changes this. This
is a real, permanent limitation of the situation, not a gap this ADR's
remaining steps close — recorded here so it is never mistaken for fully
resolved.

## Rejected alternatives

- **Only remove from the current working tree and commit a deletion,
  without rewriting history.** Rejected — explicitly what this ADR exists
  to avoid: a plain deletion commit leaves every prior blob fully
  recoverable from `git log`/`git show` by anyone with clone access,
  solving nothing about the actual exposure.
- **`git filter-branch`.** Rejected — git's own documentation deprecates
  it in favor of `git filter-repo` for exactly this class of operation
  (safety, performance, and correctness of ref/reflog handling); no
  reason to reach for the older, discouraged tool when the newer one is
  available.
- **BFG Repo-Cleaner instead of `git filter-repo`.** Considered as the
  user's own named fallback — not needed once `git filter-repo` was
  successfully installed; recorded as the fallback path if `filter-repo`
  had been unavailable.

## Evidence required

- `git log --all --full-history -- docs/source-material/*.pdf
  docs/source-material/From_Service_To_Runtime/*.pdf` returns nothing
  after the rewrite, confirmed locally before any push.
- `git status` clean; `.gitignore` contains the new pattern;
  `docs/source-material/` contains only `ProjectDescriptionOfNexus.txt`
  and the now-empty `From_Service_To_Runtime/` directory.
- The force-push completed only after explicit confirmation from the
  repository owner, given the operation's irreversibility for anyone who
  has already pulled the old history.
