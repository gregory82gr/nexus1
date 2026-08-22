using Xunit;

// Each test class in this assembly migrates TWO DbContexts (ReactorFleet, Robotics) against its
// own fresh LocalDB database (ADR-023's own evidence-required fixture shape, following
// EventManagement/Maintenance's own precedent). Left parallelized, xUnit's default cross-class
// parallelism drives several concurrent CREATE DATABASE + two-context migrations against the
// same LocalDB instance at once, which exhausts LocalDB's default memory pool ("insufficient
// system memory in resource pool 'internal'") — a resource-contention failure mode already
// solved the same way for every prior sector's own ComponentTests in this codebase, not a
// correctness bug in the fixture or the handlers under test.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
