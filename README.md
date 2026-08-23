# NEXUS-1

**A companion reference implementation for a nuclear facility management platform.**

NEXUS-1 is an educational/reference demonstrator built alongside a companion book
series. It exists to show, with real running code and real evidence rather than
illustrative snippets, how a hybrid modular-monolith + selective-microservice backend
is actually designed, built, and extended — including the parts that go wrong, the
architectural decisions that get revisited, and the gaps that get named instead of
faked.

Every claim in the companion books is backed by this codebase. If a book says a test
passes, that test is in this repository and it passes. If a book says a decision was
made, the ADR recording it is here too.

---

## Companion book series

| Volume | Covers |
|---|---|
| *From Schema to System* — Schema Atlas | Domain modeling, the full 17-sector data backbone |
| *From Flow to Services* | EF Core mapping, modular monolith architecture |
| *From Services to Runtime* (Parts 1–4) | Distributed slice, messaging, OpenTelemetry, the BFF layer this repo tracks live |
| *From Trial to Policy* | The ReinforcementLearning sector — tabular Q-learning, advisory-only |
| *From Flood to Cause* | RootCause's future causal-graph / RAG advisory phase (not yet built — see Roadmap) |

The books are written to match this repository's actual state at time of writing, and
are updated as the backend evolves. If you find a mismatch, the code is authoritative.

---

## Architecture

NEXUS-1 uses a **hybrid modular monolith + selective microservice** architecture
(see `ADR-001`):

- **`Nexus1.ModularRuntime`** hosts the majority of bounded contexts as a modular
  monolith — one process, one database per context, in-process composition, no
  cross-context transactions.
- **`Nexus1.RootCause.Host`** is the one context deliberately extracted as an
  independently deployed service, communicating over the messaging backbone.
- **`Nexus1.Bff`** is a Backend-for-Frontend layer serving a future Angular console,
  composing context Application layers in-process (see [BFF layer](#bff-layer) below).

### The 17 Schema Atlas sectors

| Sector | Phase | Notes |
|---|---|---|
| ReactorFleet | 1 | Core unit identity |
| AlarmManagement | 1 | Full messaging backbone (outbox/inbox, retry/DLQ) |
| RootCause | 1 | Independently deployed service (ADR-001) |
| Audit | 1 | |
| Compliance | 1 | |
| Reporting | 1 | Write-side projection from RootCause events (ADR-012) |
| CorePlatform | 2 | Reference/lookup data |
| Security | 2 | Application-level RBAC |
| Organization | 2 | Personnel/department hierarchy |
| Instrumentation | 2 | Generic signal/measurement telemetry |
| DigitalTwin | 2 | |
| Maintenance | 2 | Asset condition, degradation tracking |
| EventManagement | 2 | |
| Robotics | 2 | |
| RadiationMonitoring | 2 | |
| EmergencyPreparedness | 2 | |
| ReinforcementLearning | 2 | Training/persistence only, advisory-only (ADR-026) |

**Phase 1** (contexts 1–6) is the original distributed slice: full messaging backbone,
OpenTelemetry tracing and metrics, real broker proof, real host health checks.

**Phase 2** (contexts 7–17) are monolithic implementations built sector-by-sector
inside `ModularRuntime`, each verified with real databases and a full regression
suite before moving to the next.

---

## Current status

- ✅ **Phase 1** — distributed slice, complete.
- ✅ **Phase 2** — all 11 remaining sectors, complete. 869/869 tests passing.
- ✅ **BFF layer** — in progress, vertical-slice by vertical-slice (see below).
- 🔜 **Angular UI** — planned once BFF coverage is sufficient; will reuse the
  companion Angular book's screens/design system with a purpose-built API contract
  (not the Angular book's own backend contract — see `ADR-030`).
- 🔜 **RAG-based root-cause advisory** ("From Flood to Cause") — a large future phase,
  not started; would reopen `ADR-005`'s deliberate RootCause minimal-scope decision.

### BFF layer

`Nexus1.Bff` is being built as a series of proven, evidence-backed vertical slices —
each one composes a context's existing Application layer in-process, shapes an
endpoint around a real screen from the companion Angular book, and is verified against
a real database before being considered done. Screens that don't map to anything real
in the domain model are named as gaps rather than faked.

| # | Slice | Notes |
|---|---|---|
| 1 | ReactorFleet | Read-only walking skeleton |
| 2 | AlarmManagement | Read + write (acknowledge), no messaging side effects |
| 3 | DigitalTwin | |
| 4 | RadiationMonitoring | No per-unit dose concept — ambient/zone data only |
| 5 | Reporting | Built `Nexus1.Reporting.Application` from scratch — none existed |
| 6 | Robotics | |
| 7 | Instrumentation | 7 book screens map to 2 real domain groupings |
| 8 | Overview (aggregation) | First cross-context endpoint — proven concurrent, partial-failure-safe |
| 9 | Organization | No link to ReactorFleet.Unit exists (ADR-017) |
| 10 | Security | RBAC only — no physical/zone-access concept |
| 11 | Maintenance | Ageing/Degradation real; Decommissioning/Waste don't exist |
| 12 | CorePlatform | Software/lookup metadata — not a physical component registry |
| 13+ | Audit, Compliance, EventManagement, EmergencyPreparedness, ReinforcementLearning, RootCause | In progress |

A dev-mode subset-composition capability lets the BFF host start with only the
contexts a given session needs, roughly halving startup memory cost during
evidence-gathering.

---

## Architectural decisions of note

- **No Controllers** — Minimal API endpoints only, throughout.
- **MediatR deferred** — hand-rolled direct dispatch instead.
- **No cross-context database transactions** — saga/outbox deferred until a real need
  emerges.
- **Cross-context references** are real SQL FKs when contexts share a database and no
  sensitivity applies; otherwise they're deliberately downgraded to passport-only
  ints, enforced by restricting write access to a scoped SQL login (`nexus1_app`,
  `ADR-028`) rather than a real FK — preserving the option to extract a context as a
  service later.
- **OpenTelemetry** is fully wired for Phase 1 (Ch. 51–52) and deliberately deferred
  for Phase 2 sectors until they have a real external caller (`ADR-027`).
- **Evidence discipline**: nothing in this codebase or its accompanying reports claims
  completion without a real database, a real host, and captured output. Gaps are named
  explicitly rather than papered over — see the `evidence/` reports referenced in each
  ADR for examples.

Full ADR log lives in [`/docs/adr`](./docs/adr).

---

## Tech stack

- .NET 8, C#
- Entity Framework Core, SQL Server
- RabbitMQ (Phase 1 messaging backbone: transactional outbox/inbox, retry/DLQ)
- OpenTelemetry (Phase 1)
- ASP.NET Core Minimal APIs

---

## Getting started

```bash
git clone <this-repo>
cd nexus1
dotnet build
dotnet test
```

Each context's database is created via its own EF Core migrations; see
[`/docs/setup.md`](./docs/setup.md) for the full local-dev bring-up sequence
(LocalDB, the `nexus1_app` scoped login, and per-context connection strings).

To run the BFF layer in dev mode against a subset of contexts:

```bash
# see /src/Hosts/Nexus1.Bff/README.md for the full option
export BffContexts__Enabled__0=ReactorFleet
export BffContexts__Enabled__1=AlarmManagement
dotnet run --project src/Hosts/Nexus1.Bff
```

---

## License

See [`LICENSE`](./LICENSE).
