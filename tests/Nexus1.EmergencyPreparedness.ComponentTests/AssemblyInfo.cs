using Xunit;

// Each test class in this assembly migrates FOUR DbContexts (ReactorFleet, CorePlatform,
// RadiationMonitoring, EmergencyPreparedness) against its own fresh LocalDB database (ADR-025's
// own evidence-required fixture shape, following RadiationMonitoring's own three-context
// precedent, extended by one context since RadiationMonitoring itself has a real FK to
// ReactorFleet.Unit). Left parallelized, xUnit's default cross-class parallelism drives several
// concurrent CREATE DATABASE + multi-context migrations against the same LocalDB instance at
// once, which exhausts LocalDB's default memory pool ("insufficient system memory in resource
// pool 'internal'") — a resource-contention failure mode already solved the same way for every
// prior sector's own ComponentTests in this codebase, not a correctness bug in the fixture or the
// handlers under test.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
