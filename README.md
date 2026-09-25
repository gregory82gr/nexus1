# NEXUS-1

**A companion reference implementation for a nuclear facility management platform** — a .NET 8 / EF Core / SQL Server / RabbitMQ / Angular reference system, built one evidenced slice at a time.

NEXUS-1 is an educational/reference demonstrator built alongside a companion book series. It exists to show, with real running code and real evidence rather than illustrative snippets, how a hybrid modular-monolith + selective-microservice backend is actually designed, built, and extended — including the parts that go wrong, the architectural decisions that get revisited, and the gaps that get named instead of faked.

Every claim in the companion books is backed by this codebase. If a book says a test passes, that test is in this repository and it passes. If a book says a decision was made, the ADR recording it is here too.

> **Phase-0 educational demonstrator.** Not safety-class or operational software, and not connected to any real facility. Every plant, component and incident here is a teaching fixture.

📘 **Start here: [`docs/NEXUS-1-Programmers-Guide.pdf`](docs/NEXUS-1-Programmers-Guide.pdf)** — a 27-page programmer-to-programmer guide covering the topology, how the RAG was built and why, a *verify-the-claims-yourself* walkthrough, troubleshooting, and full installation. Every command in it was run against this repository while it was written.

---

## ★ This repository *is* *From Flood to Cause* — built and evidenced live

*From Flood to Cause* asks what happens **when 200 alarms fire at once**. Its answer: a deterministic causal graph finds the root, and the LLM only explains it. This repository is not an illustration of that idea — it is a real, running, evidence-backed implementation of it, in the `RootCause` bounded context.

### The core idea

A **deterministic, grounded, auditable root-cause engine** in which a language model only narrates a decision it never makes itself:

```
alarm flood ─▶ 1 graph walk ─▶ 2 telemetry corroboration ─▶ 3 retrieval ─▶ 4 explain ─▶ 5 validate ─▶ 6 seal
               └──────────── deterministic: decides the origin ────────────┘  (LLM)     (H3/H4)     (SHA-256 chain)
                         any gate can veto → a named abstention, never a guessed verdict
```

- **The engine decides.** It walks an engineered causal graph, checks the timing against historian telemetry (or, for a measurement artefact, checks that independent witnesses stayed *silent*), and grounds the result in a retrieved corpus.
- **An air-gapped local model only narrates.** `nexus-dslm` (a pinned `qwen2.5:3b` build) runs on a loopback-only Ollama, driven by Semantic Kernel with **no planner and no tool calling**. It is told the origin, has no database access, and must answer in a strict schema, citing only passages that were actually retrieved.
- **H1–H10 anti-hallucination controls**: closed-world prompting, a relevance floor, entity-registry and citation checks, a strict output schema, determinism, a read-only data path for the model side, abstain-don't-salvage, an evaluation harness, and a **tamper-evident SHA-256 audit hash chain** that seals every verdict *and* every abstention.

### What is real today

| | What it demonstrates | Evidence |
|---|---|---|
| **EVT-2026-0418** — feedwater cascade | FV-104 is computed as the origin (it explains 11 of 14 alarms), and every backbone edge fits its delay window against the historian | [ADR-032](docs/adr/ADR-032-rootcause-grounding-walking-skeleton.md) · [033](docs/adr/ADR-033-rootcause-explain-served-model-stage.md) · [034](docs/adr/ADR-034-rootcause-diagnosis-query-route.md) |
| **EVT-2026-0419** — EMI measurement artefact | BKR-2A is found as the origin **because the independent witnesses stayed flat**: silence is the evidence | [ADR-037](docs/adr/ADR-037-second-fixed-incident-emi-artefact.md) |
| **EVT-2026-0420** — QA escape | The engine's **honest boundary**: no alarm and no telemetry, so the engine correctly cannot solve it (its route returns `404`). The case is recorded as **Inconclusive** with a reason, never as a fabricated verdict | [ADR-039](docs/adr/ADR-039-rootcause-analysis-inconclusive-state.md) · [040](docs/adr/ADR-040-evt-0420-qa-escape-content.md) |
| **H9 evaluation harness** | JSON golden and trap cases (transposed entity, invented source, no grounding), with a deterministic tier and a live-model tier | [ADR-041](docs/adr/ADR-041-h9-evaluation-harness-core.md) |
| **Observables, not answer keys** | Candidate selection, ranking, weights and roles are derived from the graph plus observables. A poisoned-seed test proves the engine no longer reads seeded "answer" fields | [ADR-042](docs/adr/ADR-042-engine-candidate-selection-from-observables.md) |
| **Honest under a wrong model** | When the causal graph is missing a coupling, an unexplained earlier alarm makes the engine abstain instead of sealing a confident wrong verdict. The rule's blind spots are listed in the ADR | [ADR-043](docs/adr/ADR-043-unexplained-precursor-rule.md) |
| **Runs like a service** | Real HTTP route, BFF gateway, Angular screens, a live end-to-end test, Prometheus/Grafana diagnosis metrics, honest readiness checks, secrets kept out of tracked files | [ADR-035](docs/adr/ADR-035-bff-to-rootcause-http-hop.md) · [036](docs/adr/ADR-036-rootcause-incident-graph-read-endpoint.md) · [038](docs/adr/ADR-038-observability-prometheus-grafana.md) · [044](docs/adr/ADR-044-startup-resilience-readiness-secrets-error-detail.md) |

The full trail is **ADR-032 → ADR-044** in [`docs/adr/`](docs/adr/), with one evidence report per slice in [`artifacts/evidence/`](artifacts/evidence/). Each report records what was actually run and its real output.

### Don't take our word for it

Section 3 of the [Programmer's Guide](docs/NEXUS-1-Programmers-Guide.pdf) walks you through running one incident and checking that the `auditHash` in the HTTP response matches the newest row in `RootCause.Audit`. Then [`docs/guide/verify-audit-chain.ps1`](docs/guide/verify-audit-chain.ps1) recomputes **every link of the chain** independently, with no project code involved.

### Honest scope

This is a **fixed-incident demonstrator that proves the architecture end to end** — not yet the general system the book describes. Named, not hidden:

- Only two incidents are solvable, and the model narrates on a CPU. The *decision* is deterministic; the *narration* is not byte-identical across runs.
- True `retrieval_recall` (Appendix J) is not measured yet; three incidents are far too small a labelled set. The metric that exists is called `grounding_hit`, a proxy.
- There is no free-form question interface, and no check yet that the model's named cause matches the engine's origin. The H9 "leading question" trap is authored and **skipped by name** until that check exists.
- The Audit and Compliance contexts do not yet consume the Inconclusive event; there are no container images for the .NET hosts; there is no authentication.

---

## Current status

- ✅ **Phase 1** — the distributed slice (AlarmManagement → RootCause, with Audit, Compliance and Reporting as fan-out subscribers): transactional outbox/inbox, retry/DLQ, OpenTelemetry tracing and metrics, real broker proof, real host health checks.
- ✅ **Phase 2** — all 11 remaining sectors, built sector by sector inside `ModularRuntime` (ADR-015 – ADR-026).
- ✅ **BFF layer** — all 17 sectors: 16 composed in-process, RootCause reached over an HTTP proxy to `Nexus1.RootCause.Host` (ADR-035/036).
- ✅ **Angular console** — every screen, including Root Cause and Incident Analysis on the real diagnosis pipeline, the drawn fault-tree, and a live cross-screen end-to-end test. It reuses the companion Angular book's screens and design system with a purpose-built API contract (ADR-030).
- ✅ **RAG root-cause engine ("From Flood to Cause")** — built and evidenced as a fixed-incident demonstrator (ADR-032 – ADR-044; see above).
- **Latest regression gate** (ADR-044 evidence): build with 0 warnings and 0 errors; 36 test assemblies green, each run in isolation; the live `@slow` end-to-end test 2/2.
- 🔜 **Not yet built** — see *Honest scope* above and the consolidated list in Section 6.12 of the [guide](docs/NEXUS-1-Programmers-Guide.pdf).

---

## Architecture

NEXUS-1 uses a **hybrid modular monolith + selective microservice** architecture (see [`ADR-001-amend`](docs/adr/ADR-001-amend-reactorfleet.md)). All **17 Schema Atlas sectors** are bounded contexts, each with its own Domain / Application / Infrastructure projects.

- **`Nexus1.ModularRuntime`** hosts 16 of them as a modular monolith: one process, in-process composition, no cross-context transactions. Databases follow ownership, not a strict one-per-context rule: eleven plant-operational contexts share `AlarmManagementDb` (each with its own schema and migrations history); Security, Organization, Audit, Compliance and Reporting each own a database.
- **`Nexus1.RootCause.Host`** is the one context deliberately extracted as an independently deployed service. It owns `RootCauseDb`, consumes alarm floods and publishes its own events over the messaging backbone, and hosts the RAG diagnosis engine.
- **`Nexus1.Bff`** is a Backend-for-Frontend that serves the Angular console. It composes context Application layers in-process and reaches RootCause only over HTTP (see [BFF layer](#bff-layer) below).
- The dependency law is **enforced by architecture tests** that fail the build.

### The 17 Schema Atlas sectors

| Sector | Phase | Notes |
|---|---|---|
| ReactorFleet | 1 | Core unit identity |
| AlarmManagement | 1 | Full messaging backbone (outbox/inbox, retry/DLQ) |
| RootCause | 1 | Independently deployed service (ADR-001); hosts the RAG diagnosis engine (ADR-032 – ADR-044) |
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

**Phase 1** (contexts 1–6) is the original distributed slice: full messaging backbone, OpenTelemetry tracing and metrics, real broker proof, real host health checks.

**Phase 2** (contexts 7–17) are monolithic implementations built sector by sector inside `ModularRuntime`, each verified with real databases and a full regression suite before moving to the next.

### Repository map

```
src/BuildingBlocks/        shared kernel: Domain, Application (CQRS ports), Messaging, Observability, ServiceDefaults
src/Contexts/<Sector>/     Nexus1.<Sector>.{Domain,Application,Infrastructure}  (+ Nexus1.Contracts.<Sector>)
src/Hosts/                 Nexus1.ModularRuntime · Nexus1.RootCause.Host · Nexus1.Bff
tests/                     ArchitectureTests · <Sector>.UnitTests · <Sector>.ComponentTests (real LocalDB)
console/nexus-console/     Angular 18 console · Jest · Playwright E2E
docs/                      adr/ (44 decision records) · runbooks/ · observability/ · guide/
artifacts/evidence/        what was actually run for every slice, and its output
```

### BFF layer

`Nexus1.Bff` has been built as a series of proven, evidence-backed vertical slices. Each one composes a context's existing Application layer in-process, shapes an endpoint around a real screen from the companion Angular book, and is verified against a real database before being considered done. **All slices are complete**; RootCause, the last one, is reached through a thin HTTP proxy to its own host rather than composed in-process.

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
| 13+ | Audit, Compliance, EventManagement, EmergencyPreparedness, ReinforcementLearning, RootCause | All complete; RootCause via an HTTP proxy that relays the host's response verbatim, 502 vs relayed 503 (ADR-035/036) |

A dev-mode subset-composition capability (`BffContexts:Enabled`) lets the BFF host start with only the contexts a given session needs, roughly halving startup memory cost during evidence-gathering.

---

## Architectural decisions of note

- **No Controllers** — Minimal API endpoints only, throughout.
- **MediatR deferred** — hand-rolled direct dispatch instead (ADR-002-amend).
- **No cross-context database transactions.** Integration events use a transactional outbox and idempotent inbox, with retry and dead-lettering (ADR-008/009); there are no sagas yet.
- **Cross-context references** are real SQL FKs when contexts share a database and no sensitivity applies. Otherwise they are deliberately downgraded to passport-only ints, enforced by restricting write access to a scoped SQL login (`nexus1_app`, ADR-028) rather than a real FK — preserving the option to extract a context as a service later.
- **OpenTelemetry** is fully wired for Phase 1 (Ch. 51–52, ADR-013/014) and deliberately deferred for Phase 2 sectors until they have a real external caller (ADR-027). The RAG engine's diagnosis metrics are exported to Prometheus (ADR-038).
- **The served model is fenced in** — only `Nexus1.RootCause.Explain` may reference a language-model package, enforced by an architecture test (ADR-032/033).
- **No secrets in tracked files** — connection strings live in User Secrets, and a guard test fails the build if a password appears in any `appsettings*.json` (ADR-044).
- **Evidence discipline** — nothing in this codebase or its accompanying reports claims completion without a real database, a real host, and captured output. Gaps are named explicitly rather than papered over; see the reports in [`artifacts/evidence/`](artifacts/evidence/) referenced by each ADR.

Full ADR log lives in [`docs/adr`](docs/adr).

---

## Tech stack

- .NET 8, C#
- Entity Framework Core, SQL Server (LocalDB in development)
- RabbitMQ (transactional outbox/inbox, retry/DLQ)
- OpenTelemetry (Phase 1); Prometheus + Grafana for the diagnosis metrics
- ASP.NET Core Minimal APIs
- Angular 18 (Jest, Playwright)
- Ollama + Semantic Kernel — the local served model, used only by `Nexus1.RootCause.Explain`

---

## Getting started

```bash
git clone https://github.com/gregory82gr/nexus1
cd nexus1
```

The full procedure — LocalDB and migrations, scoped SQL logins, User Secrets, RabbitMQ, Ollama and the pinned model, provisioning, the hosts and the console — is **Section 6 of the [Programmer's Guide](docs/NEXUS-1-Programmers-Guide.pdf)**, consolidated from [`docs/runbooks/`](docs/runbooks/). It is proven on one Windows 10 machine (LocalDB is Windows-only; there are no containers yet).

What works with nothing but the .NET 8 SDK:

```bash
dotnet tool restore
dotnet build Nexus1.Runtime.sln
dotnet test tests/Nexus1.ArchitectureTests      # dependency law, served-model rule, secrets guard
dotnet test tests/Nexus1.RootCause.UnitTests
```

Component tests need SQL Server LocalDB. The RootCause live-model tests need Ollama with `nexus-dslm` and `nomic-embed-text`, and **skip by name** when it is absent. A plain `dotnet test` of the whole solution needs all of that, and on a modest machine it is more reliable to run the test assemblies one at a time (guide §4.4).

---

## Companion book series (22 titles)

**Author:** Grigorios Kyriakos Agathangelidis (Γρηγόριος Κυριάκος Αγαθαγγελίδης) · books on [Leanpub](https://leanpub.com/u/grigorios-kyriakos-agathangelidis)

The books are written to match this repository's actual state at time of writing, and are updated as the backend evolves. If you find a mismatch, the code is authoritative.

★ = this repository is its implementation  ·  ◆ = a direct source this repository is built from

### Physics and mechanics of the plant
- **From Grid to Core** — the series' foundation: from the 400 kV substation to the reactor core, with neutron kinetics and SCRAM.
- **From Queue to Core** — stochastic queueing theory as a rigorous mathematical foundation for reactor kinetics, implemented in C#.
- **From Core to Quantum** — from the quantum crisis to nuclear structure and a full quantum-circuit simulator in modeled C#.

### Interpretable and controlled AI
- ★ **From Flood to Cause** — when 200 alarms fire at once: a deterministic causal graph finds the root, the LLM only explains it. *(Built in this repository — see above.)*
- **From Trial to Policy** — reinforcement learning from scratch with a fully interpretable Q-learning agent: 175 numbers in an auditable table.
- **From Stochastic Chaos to Deterministic Certainty** — an industrial language model wrapped in proven deterministic boundaries, mapped to the EU AI Act.

### Data architecture
- ◆ **From Schema to System** — the complete schema atlas: 17 sectors, 654 tables, with ER diagrams and verification queries behind the digital twin.
- **From Table to Twin** — the same schema built twice — Database First and Code First with EF Core — and a chapter reconciling them.
- ◆ **From Entity to Context** — clean schema-to-EF-Core mapping, one configuration per entity instead of a giant `OnModelCreating`.

### Domain-driven design
- ◆ **From Domain to Twin** — DDD from scratch in plain language, applied to NEXUS-1: from entities and aggregates to the SQL schema.
- **From Context to Flow** — advanced DDD patterns — context maps, sagas, outbox — through a flow crossing nine bounded contexts.

### Backend trilogy (.NET)
- ◆ **From Blueprint to Core** — Domain and Application layers with no database or web server: 17 contexts, 50 green tests in ~600 ms with just the .NET SDK.
- **From Core to Contract** — the core gets a database (EF Core, 654 tables) and a public API, with infrastructure strictly below the seam.
- **From Contract to Container** — integration tests against real SQL Server via Testcontainers, an outbox that survives a killed process, and a CI gate.

### Microservices
- ◆ **From Flow to Services** — 70+ chapters: microservices as a consequence of mature boundaries, not a starting point — and when distribution is *not* worth it.
- ◆ **From Services to Runtime** — three owner services with a real .NET runtime blueprint: inbox/outbox, JWT, OpenTelemetry, Kubernetes.

### Formal methods
- **From Flow to Proof** — architectural promises become proofs: state machines, TLA+, Petri nets, category theory. Model vs. implementation.

### Systems trilogy (below .NET)
- **From Runtime to Distribution — Volume I** — from C# to hardware: the CPU, kernel mode, threads, inside the CLR (JIT, Native AOT), and memory/GC.
- **From Runtime to Distribution — Volume II** — the process boundary: concurrency, async/await as a state machine, IPC, TCP/TLS/gRPC, and the "Boundary Ledger."
- **From Runtime to Distribution — Volume III** — production: containers, Kubernetes, SLIs/SLOs, canary deployments, and practical observability tooling.

### Frontend (Angular)
- ◆ **From File to Framework** — a single 5,900-line console file becomes a real Angular application — and every screen is checked for "honesty."

### Retrospective evaluation
- **From Certainty to Calibration** — the author looks back and judges six of his own decisions: Then → Mechanism → Learned → Now → What the fix risks.

> The book PDFs are licensed material and are **not** part of this repository ([ADR-029](docs/adr/ADR-029-purge-source-material-pdfs-from-history.md)). Values the implementation takes from them are pinned in the ADRs and the code, so every decision traces to a recorded source.

---

## License

The code, tests, scripts and documentation in this repository are released under the [MIT License](LICENSE). The NEXUS-1 books themselves are separate copyrighted works and are **not** covered by it — see [`NOTICE.md`](NOTICE.md).

---

## The one rule

> **Nothing claims to exist that does not.** A diagram, a "done" checkbox, or a status message is valid only when it matches the repository, the contracts, the data ownership, the tests, and the observable runtime behavior.

That is why every slice ships with an ADR and an evidence report, why gaps are named where you will trip over them, and why the guide asks you to verify its claims rather than believe them.
