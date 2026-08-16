using Xunit;

// Multiple test classes here register a MeterListener/ActivityListener
// filtered only by Meter/ActivitySource name ("Nexus1.Runtime" is shared
// across NexusRuntimeMetricsTests and OutboxMetricStateTests). xUnit's
// default cross-class parallelism would let two same-named Meters be alive
// at once and cross-contaminate measurements — the exact bug already found
// and fixed in the *.ComponentTests projects for tracing's ActivityListener
// (see artifacts/evidence/2026-08-16-observability-tracing-extension.md).
// Applied here proactively rather than waiting to rediscover it as flakiness.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
