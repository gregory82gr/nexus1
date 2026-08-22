using Xunit;

// Each test class in this assembly migrates FOUR DbContexts (ReactorFleet, CorePlatform,
// Instrumentation, Maintenance) against its own fresh LocalDB database (ADR-021's own
// evidence-required fixture shape, following DigitalTwin's own precedent, ADR-020). Left
// parallelized, xUnit's default cross-class parallelism drives several concurrent CREATE
// DATABASE + four-context migrations against the same LocalDB instance at once, which
// exhausts LocalDB's default memory pool ("insufficient system memory in resource pool
// 'internal'") — a resource-contention failure mode already solved the same way for
// AlarmManagement/Audit/Compliance/Reporting/RootCause/DigitalTwin's own ComponentTests in
// this codebase, not a correctness bug in the fixture or the handlers under test.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
