# Nexus1.Bff

Backend-for-Frontend host for the future Angular console (ADR-030). One
Minimal API host composing eleven contexts' Application/Infrastructure
layers in-process, screen-first endpoints per vertical slice (see
`docs/adr/ADR-030-bff-walking-skeleton-reactorfleet-slice.md` and each
slice's own evidence report under `artifacts/evidence/` for what each
endpoint does and doesn't cover).

## Dev-testing convenience: composing a subset of contexts

**This is not a new architectural layer** — just a guard around each
existing `Add...Application()`/`Add...Infrastructure()` call pair (and its
matching health check) at the composition root, added because host startup
memory cost scales with the number of composed contexts, and an
evidence-gathering session for one slice rarely needs all eleven.

By default (nothing configured), **every context is composed**, identical
to production/full-integration behavior — this is what every slice's own
evidence report was proven against, and nothing about that changes.

To compose only a subset, set `BffContexts:Enabled` to the list of context
names you want (case-insensitive), either in `appsettings.json`:

```json
{
  "BffContexts": {
    "Enabled": ["ReactorFleet", "Maintenance"]
  }
}
```

or via environment variables (double-underscore separates configuration
sections, and array entries are indexed):

```bash
BffContexts__Enabled__0=ReactorFleet
BffContexts__Enabled__1=Maintenance
```

Valid names match the `IsContextEnabled(...)` calls in `Program.cs`:
`ReactorFleet`, `AlarmManagement`, `DigitalTwin`, `RadiationMonitoring`,
`Reporting`, `Robotics`, `Instrumentation`, `Organization`, `Security`,
`Maintenance`, `CorePlatform`, `Audit`, `Compliance`, `EventManagement`,
`EmergencyPreparedness`, `ReinforcementLearning`.

A context left out is simply never composed — its `DbContext`, finders, and
query handlers are never registered. Routes for every context stay mapped
regardless (route registration isn't conditional, only service composition
is); an endpoint whose handler wasn't composed still exists as a route, and
will fail if called, which is expected and fine for a dev-only subset run.

**Found while building this, corrected before shipping**: every endpoint
handler parameter must carry an explicit `[FromServices]` attribute. Without
it, ASP.NET Core's Minimal API infers each parameter's binding source by
checking whether its type is a known DI service at the time the route table
is first built (lazily, on the first incoming request to *any* endpoint —
not per-endpoint). When a handler type isn't registered (because its
context was excluded), that inference fails, and — because GET endpoints
don't allow an inferred body parameter — the *entire* route table fails to
build, breaking every endpoint, including ones for contexts that *were*
composed. This is a global failure at first-request time, not a contained
per-endpoint one; it was found by actually calling an endpoint under a
subset composition, not assumed from reading the framework docs.
`[FromServices]` forces DI resolution to happen per-request instead of
during that inference step, so a genuinely-missing handler now fails only
the one endpoint that needed it, with an ordinary per-request "unable to
resolve service" error — the behavior this README always intended.
