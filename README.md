# nexus1

**The companion implementation of the NEXUS-1 book series** — a .NET 8 / EF Core / SQL Server / RabbitMQ / Angular reference system for a fictional nuclear-plant operations platform, built one evidenced slice at a time.

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

## What else is in this repository

NEXUS-1 is a **hybrid modular monolith**: all **17 Schema Atlas sectors** are bounded contexts (Domain / Application / Infrastructure projects each).

- **`Nexus1.ModularRuntime`** runs 16 of them in one process.
- **`RootCause`** is the one context extracted to its own independently deployed host, **`Nexus1.RootCause.Host`**.
- **`Nexus1.Bff`** composes the in-process contexts for the **Angular 18 console** and reaches RootCause only over HTTP.
- Integration events travel over **RabbitMQ**, with a transactional outbox, idempotent inboxes and dead-lettering.
- The dependency law is **enforced by architecture tests** that fail the build.

```
src/BuildingBlocks/        shared kernel: Domain, Application (CQRS ports), Messaging, Observability, ServiceDefaults
src/Contexts/<Sector>/     Nexus1.<Sector>.{Domain,Application,Infrastructure}  (+ Nexus1.Contracts.<Sector>)
src/Hosts/                 Nexus1.ModularRuntime · Nexus1.RootCause.Host · Nexus1.Bff
tests/                     ArchitectureTests · <Sector>.UnitTests · <Sector>.ComponentTests (real LocalDB)
console/nexus-console/     Angular 18 console · Jest · Playwright E2E
docs/                      adr/ (44 decision records) · runbooks/ · observability/ · guide/
artifacts/evidence/        what was actually run for every slice, and its output
```

## Quick start

The full procedure — LocalDB and migrations, scoped SQL logins, User Secrets, RabbitMQ, Ollama and the pinned model, provisioning, the hosts and the console — is **Section 6 of the [Programmer's Guide](docs/NEXUS-1-Programmers-Guide.pdf)**, consolidated from [`docs/runbooks/`](docs/runbooks/). It is proven on one Windows 10 machine (LocalDB is Windows-only; there are no containers yet).

What works with nothing but the .NET 8 SDK:

```bash
dotnet tool restore
dotnet build Nexus1.Runtime.sln
dotnet test tests/Nexus1.ArchitectureTests      # dependency law, served-model rule, secrets guard
dotnet test tests/Nexus1.RootCause.UnitTests
```

Component tests need SQL Server LocalDB. The RootCause live-model tests need Ollama with `nexus-dslm` and `nomic-embed-text`, and **skip by name** when it is absent.

---

## The book series (22 titles)

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
