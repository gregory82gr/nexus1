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

**Author:** Grigorios Kyriakos Agathangelidis (Γρηγόριος Κυριάκος Αγαθαγγελίδης)

The NEXUS-1 Companion Series comprises 19 volumes (all available on [Leanpub](https://leanpub.com/u/grigorios-kyriakos-agathangelidis)):

| # | Title | Covers |
|---|-------|--------|
| 1 | *From Grid to Core* | Reverse-engineering approach to reactor kinetics — from the switchyard to the core |
| 2 | *From Flood to Cause* | Deterministic, auditable root-cause analysis with causal engine + LLM explanation |
| 3 | *From Trial to Policy* | Interpretable reinforcement learning with Q-table, live NEXUS-1 console |
| 4 | *From Schema to System* — Schema Atlas | Domain modeling, the full 17-sector data backbone |
| 5 | *From Entity to Context* | EF Core Code First configuration atlas — one entity, one mapping file |
| 6 | *From Domain to Twin* | Domain-Driven Design from zero, applied to NEXUS-1 |
| 7 | *From Table to Twin* | SQL Server and EF Core backbone — by hand and from code |
| 8 | *From Blueprint to Core* | Clean Architecture, DDD, CQRS — zero database, zero web server, 50 green tests |
| 9 | *From Context to Flow* | Advanced DDD: domain events, outbox, sagas, anti-corruption layers, eventual consistency |
| 10 | *From Core to Contract* | Infrastructure and API layers: EF Core, repositories, outbox, JWT |
| 11 | *From Contract to Container* | Containerization, CI, SQL Server on every test run, honest deployment |
| 12 | *From Services to Runtime* | Microservices with owned truth, stable contracts, idempotent messages, controlled failure |
| 13 | *From File to Framework* | Angular companion — ports a 5,900-line console into a real application |
| 14 | *From Flow to Proof* | Distributed-system promises → explicit models, properties, counterexamples, evidence |
| 15 | *From Flow to Services* | Discovering true microservice boundaries with DDD, APIs, messaging, sagas |
| 16 | *From Queue to Core* | Stochastic foundation: birth–death chains, master equations, delayed neutrons, point kinetics |
| 17 | *From Runtime to Distribution — Volume I* | Scaling the runtime, distributed deployment patterns, and multi-node orchestration |
| 18 | *From Runtime to Distribution — Volume II* | Advanced distribution: consensus, service discovery, and production-grade resilience |
| 19 | *From Certainty to Calibration* | Revisiting architectural decisions, recalibrating the system for long-term reliability |

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
- ✅ **BFF layer** — complete for all vertical slices except RootCause (pending).
- ✅ **Angular UI** — complete for all screens except RootCause; reuses the companion Angular book's screens/design system with a purpose-built API contract (ADR-030).
- 🔜 **RootCause UI & BFF slice** — the remaining piece; currently in progress to reach full coverage.
- 🔜 **RAG-based root-cause advisory ("From Flood to Cause")** — currently in **analysis phase**; design and feasibility studies are underway. Implementation has not started.

### BFF layer

`Nexus1.Bff` has been built as a series of proven, evidence-backed vertical slices —
each one composes a context's existing Application layer in-process, shapes an
endpoint around a real screen from the companion Angular book, and is verified against
a real database before being considered done. **All slices are now complete except for
RootCause**, which is the final piece.

The table below lists the planned slices for reference; all except RootCause are
finished.

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
| 13+ | Audit, Compliance, EventManagement, EmergencyPreparedness, ReinforcementLearning, RootCause | All completed except RootCause (in progress) |

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
git clone https://github.com/gregory82gr/nexus1
cd nexus1
dotnet build
dotnet test
