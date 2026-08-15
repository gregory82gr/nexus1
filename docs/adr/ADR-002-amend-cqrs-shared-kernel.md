# ADR-002-amend: Resolve the ADR-002 / Blueprint_to_Core topology conflict

## Status

Accepted.

## Context

Two of this project's source books prescribe genuinely different solution
topologies for the same system:

- `From_Services_To_Runtime`'s ADR-002 (Part 1, Ch. 4, pp. 64–78, priority-1
  source per this repo's CLAUDE.md) adopts **project-per-context-per-layer**:
  a separate `Domain`/`Application`/`Infrastructure`/`Contracts.*` project
  per bounded context. Its stated dependency law: *"Dependencies point
  inward inside one context. Cross-context code references only
  producer-owned Contracts. Domain never references transport, persistence,
  Host, or public Contracts."* Host projects are composition roots only.

- `From_Blueprint_to_Core.pdf` Ch. 3 ("Structuring the Nexus Solution")
  explicitly argues against exactly this shape: *"project-per-context-per-
  layer... is an obvious grid, and the obvious grid is a trap,"* and instead
  prescribes a single `Nexus.Domain` project and a single `Nexus.Application`
  project, each internally organized into per-context folders (17 of them,
  including ReactorFleet).

These read as two independently-planned book series with different topology
philosophies for the same domain, not one coherent lineage. This repo's
CLAUDE.md already committed, before this ADR, to the ADR-002 tree — every
project name in `docs/../CLAUDE.md` §4 matches ADR-002's reference tree
verbatim (right down to project names), and `Blueprint_to_Core` was
originally brought in specifically to fill a *DDD/CQRS shape gap*, not to
supply topology.

## Decision

**Keep ADR-002's project-per-context-per-layer topology.** It is the
priority-1 source's explicit topology decision ("the primary spec for
everything you build," per CLAUDE.md §1), and the gap-filling books were
scoped from the start to supply "exact aggregate shapes, exact CQRS
command/query interfaces" — not to override Part 1's *how*, the same
relationship CLAUDE.md already draws between Part 1 and `From_Flow_to_
Services` (source #2). Treating `Blueprint_to_Core`'s topology argument as
equally authoritative on topology specifically would contradict the
project's own stated source-priority order.

`Blueprint_to_Core`'s CQRS **interface shapes** are still adopted, because
they are separable from its single-project structural argument. They move
into a new shared-kernel project **not in ADR-002's original tree**:

```
src/BuildingBlocks/Nexus1.BuildingBlocks.Application/
  ICommand, ICommand<TResponse>
  ICommandHandler<TCommand>, ICommandHandler<TCommand,TResponse>
  IQuery<TResponse>, IQueryHandler<TQuery,TResponse>
  IRepository<TRoot,TId>, IUnitOfWork, IDateTimeProvider
  IAggregateRoot (marker, constrains IRepository<TRoot,TId>)
  Result, Result<T>
```

Each context's own `Application` project (`Nexus1.AlarmManagement.
Application`, etc.) references this new project and implements concrete
handlers per context — matching Blueprint_to_Core's description of the
Application layer's public surface as "just the CQRS marker interfaces plus
the three ports," with the concrete handlers living per-context rather than
in one monolithic `Nexus.Application` project.

`Blueprint_to_Core` uses MediatR as the dispatcher underneath its marker
interfaces. Adding MediatR is a new dependency and is deliberately **not**
decided here — this ADR fixes the interface shapes only; whether to wire
MediatR (or a hand-rolled dispatcher) is deferred to §5 step 5 (Application
layer / CQRS), and per this project's dependency discipline, any new NuGet
package still needs an explicit ask before it's added.

### Domain-invariant style conflict (`Result<T>` vs. exceptions)

The two gap books also disagree on invariant-enforcement style:
`Blueprint_to_Core` returns `Result`/`Result<T>` throughout (explicit
dissent from strict CQS — refusals are first-class, not exceptions);
`From_Domain_to_Twin` has aggregate methods throw (e.g.
`InvalidOperationException` for "A root-cause case cannot close without
evidence").

Resolution: these are not actually incompatible once scoped to different
layers. **Domain methods throw on invariant violation** (matches
Domain_to_Twin, and keeps Domain free of any dependency on the CQRS
abstractions project, preserving the dependency law). **Application-layer
command/query handlers catch known domain exceptions and translate them
into `Result.Failure`/`Result<T>.Failure`**, matching Blueprint_to_Core's
handler-facing contract. Unexpected (non-domain) exceptions propagate
rather than being silently swallowed into a generic failure result — fail
loud, per this project's standing "diagnostics, not exceptions, for content
problems, but real faults are never masked" instinct (carried over from the
sibling AdbChecker project's disciplines, applied here by analogy since
NEXUS-1 has no equivalent stated rule of its own yet).

This is a provisional synthesis, not a book-sourced rule — flagged here so
it's visible and revisable at §5 step 2 (domain models), where it will
first be exercised in real code.

## Consequences

- One new project not in ADR-002's original tree:
  `Nexus1.BuildingBlocks.Application`. `Nexus1.ArchitectureTests` must
  additionally assert that this project has no outward references (no
  transport, no persistence, no per-context project references) — it is a
  second shared-kernel project alongside `Nexus1.BuildingBlocks.Domain`,
  not a context.
- Every per-context `Application` project depends on
  `Nexus1.BuildingBlocks.Application` in addition to its own context's
  `Domain` project.
- `Blueprint_to_Core`'s single-project-per-layer topology, and its 17-context
  folder structure (including ReactorFleet appearing as a first-class
  context there), are **not** adopted as this repo's structure. Its content
  is used only for the interface/abstraction shapes above.

## Rejected alternatives

- **Adopt Blueprint_to_Core's single-project topology wholesale**, discarding
  ADR-002. Rejected: contradicts this repo's own source-priority order and
  would require redoing the already-scaffolded skeleton for a lower-priority
  source's structural preference.
- **Ignore Blueprint_to_Core's CQRS shapes entirely**, hand-rolling
  ad hoc command/query interfaces per context. Rejected: the CLAUDE.md
  explicitly brought this book in to fill exactly this gap; ignoring it
  after finding it present would repeat the same "silently infer instead of
  reading the source" mistake the session's earlier amendment already
  corrected once (see ADR-001-amend and CLAUDE.md §1's session note).
- **Put the CQRS interfaces directly in `Nexus1.BuildingBlocks.Domain`**
  instead of a new project. Rejected: `IRepository<>`, `IUnitOfWork`, and the
  command/query marker interfaces are Application-layer ports by
  Blueprint_to_Core's own description ("Nexus.Application.Abstractions");
  putting them in the Domain shared kernel would let Domain assemblies
  implicitly couple to persistence/transaction shapes, violating the
  dependency law this same ADR is trying to keep intact.

## Reversal condition

Revisit if a later source-material update reconciles the two books
explicitly (e.g. an addendum stating which lineage supersedes the other), or
if MediatR is adopted at §5 step 5 and its pipeline-behavior needs push the
abstractions project's shape in a direction incompatible with what's defined
here.

## Evidence required

- `Nexus1.ArchitectureTests` passing, including a new assertion that
  `Nexus1.BuildingBlocks.Application` has no cross-context or
  transport/persistence references.
- `dotnet build` clean across the full solution with the new project wired
  into all three contexts' Application projects.
