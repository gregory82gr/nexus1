import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActiveTwin, DigitalTwinApi, ModelVariableSignalTrace, OpenDivergence } from '../../core/api/digital-twin-api';

// Digital Twin (Appendix A follow-up) -- the book's own Appendix A names
// this as never individually audited: a fleet-wide reconciliation table
// (model fidelity, signals mirrored, per-signal deviation), explicitly
// distinct from Plant 3D View's (Ch. 8) per-unit twin state + 3D scene.
//
// Investigated each of the book's three named concepts directly before
// building, not assumed:
//   - Model fidelity: real, fleet-wide (GetActiveTwinsForFleetQuery,
//     already DI-registered, never mapped to a route before this screen).
//   - Signals mirrored: real (TraceModelVariableToSignalQuery, a genuine
//     SignalBinding join to Instrumentation.Signal, real FK per ADR-020),
//     scoped per twin code -- select a twin above to see its trace.
//   - Per-signal deviation: real (GetOpenDivergencesQuery, modeled vs
//     measured value per signal) -- but FLEET-WIDE ONLY. Checked directly:
//     no per-unit divergence query exists anywhere in this backend.
//     IActiveTwinFinder.GetActiveTwinsForUnitAsync's own doc comment names
//     the real gap -- reaching a specific unit's divergences requires a
//     four-hop join (TwinDivergence -> TwinSnapshot -> TwinRuntimeSession
//     -> TwinModelVersion -> TwinModel.UnitId) that no existing query
//     performs, and OpenDivergenceDto itself carries no unit reference.
//     That join is a real, separate backend addition, not bundled into
//     this screen -- the Divergences panel below labels itself fleet-wide
//     explicitly, on-screen, never silently narrowed to look per-unit.
type FleetState = { status: 'loading' } | { status: 'error'; message: string } | { status: 'loaded'; twins: ActiveTwin[] };
type TraceState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; twinCode: string; trace: ModelVariableSignalTrace[] };
type DivergenceState = { status: 'loading' } | { status: 'error'; message: string } | { status: 'loaded'; divergences: OpenDivergence[] };

@Component({
  selector: 'nx-digital-twin',
  standalone: true,
  templateUrl: './digital-twin.html',
  styleUrl: './digital-twin.scss',
})
export class DigitalTwinComponent {
  private readonly api = inject(DigitalTwinApi);
  private readonly destroyRef = inject(DestroyRef);

  readonly fleetState = signal<FleetState>({ status: 'loading' });
  readonly traceState = signal<TraceState>({ status: 'idle' });
  readonly divergenceState = signal<DivergenceState>({ status: 'loading' });

  constructor() {
    this.api
      .getFleetTwins()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (twins) => this.fleetState.set({ status: 'loaded', twins }),
        error: () => this.fleetState.set({ status: 'error', message: 'The digital-twin fleet endpoint is unreachable.' }),
      });

    this.api
      .getDivergences()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (divergences) => this.divergenceState.set({ status: 'loaded', divergences }),
        error: () => this.divergenceState.set({ status: 'error', message: 'The digital-twin divergences endpoint is unreachable.' }),
      });
  }

  selectTwin(twinCode: string): void {
    this.traceState.set({ status: 'loading' });
    this.api
      .getSignalTrace(twinCode)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (trace) => this.traceState.set({ status: 'loaded', twinCode, trace }),
        error: () => this.traceState.set({ status: 'error', message: `The signal-trace endpoint is unreachable for twin ${twinCode}.` }),
      });
  }

  get loadedTwins(): ActiveTwin[] {
    const s = this.fleetState();
    return s.status === 'loaded' ? s.twins : [];
  }
  get fleetErrorMessage(): string {
    const s = this.fleetState();
    return s.status === 'error' ? s.message : '';
  }

  get selectedTwinCode(): string | null {
    const s = this.traceState();
    return s.status === 'loaded' ? s.twinCode : s.status === 'loading' ? '…' : null;
  }
  get loadedTrace(): ModelVariableSignalTrace[] {
    const s = this.traceState();
    return s.status === 'loaded' ? s.trace : [];
  }
  get traceErrorMessage(): string {
    const s = this.traceState();
    return s.status === 'error' ? s.message : '';
  }

  get loadedDivergences(): OpenDivergence[] {
    const s = this.divergenceState();
    return s.status === 'loaded' ? s.divergences : [];
  }
  get divergenceErrorMessage(): string {
    const s = this.divergenceState();
    return s.status === 'error' ? s.message : '';
  }
}
