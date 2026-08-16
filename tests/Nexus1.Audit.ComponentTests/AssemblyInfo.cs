using Xunit;

// TracingTests' CaptureSpans helper registers a process-global ActivityListener
// filtered only by ActivitySource name (ADR-013). Left parallelized, xUnit's
// default cross-class parallelism lets another test class in this assembly
// start a same-named span concurrently and leak into the capture, producing a
// racy assertion failure that has nothing to do with tracing correctness.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
