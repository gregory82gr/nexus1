import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { interval } from 'rxjs';
import { startWith, switchMap } from 'rxjs/operators';
import { InstrumentationApi, UnitSignalReading } from '../../core/api/instrumentation-api';
import { PlantStateService } from '../../core/state/plant-state';
import { TimedReading, deriveRateFromReadings, periodFromRate } from '../../core/physics/point-kinetics';
import { findPowerSignal } from './power-signal';

// Reactor Kinetics (Ch. 11) -- the one Reactor sub-screen kept genuinely
// separate from reactor-instrumentation.ts's consolidation, and for a
// different reason than a distinct backend source (it has none; same one
// signals endpoint): Kinetics needs real client-side derivation work
// reactor-instrumentation.ts doesn't. Reactor PERIOD is a rate of change,
// and Ch. 11's own point is that naively displaying a raw percent-per-poll
// delta is a worse answer than deriving the textbook rate (d ln P / dt)
// from consecutive real readings -- exactly what
// core/physics/point-kinetics.ts's deriveRateFromReadings/periodFromRate
// do, applied here to REAL polled telemetry rather than a simulated
// state (contrast with Training Mode's advanceReactor, which drives a
// simulated state forward; this screen only ever reads).
//
// "Reactor power" has no dedicated entity either (power-signal.ts's own
// doc comment) -- the first live, power-like-category signal in the same
// generic feed reactor-instrumentation.ts reads is used as the proxy, and
// the screen says so plainly rather than implying a dedicated telemetry
// channel exists.
const POLL_INTERVAL_MS = 5000;

type KineticsState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; signal: UnitSignalReading | null };

@Component({
  selector: 'nx-reactor-kinetics',
  standalone: true,
  templateUrl: './reactor-kinetics.html',
  styleUrl: './reactor-kinetics.scss',
})
export class ReactorKineticsComponent {
  private readonly api = inject(InstrumentationApi);
  private readonly plantState = inject(PlantStateService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly pollIntervalSeconds = POLL_INTERVAL_MS / 1000;

  readonly unitId = this.plantState.selectedId;
  readonly state = signal<KineticsState>({ status: 'loading' });
  readonly periodSeconds = signal<number | null>(null);
  readonly pollCount = signal(0);

  private lastReading: TimedReading | null = null;

  constructor() {
    interval(POLL_INTERVAL_MS)
      .pipe(
        startWith(0),
        switchMap(() => this.api.getSignals(this.unitId())),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (signals) => this.onSignals(signals),
        error: () =>
          this.state.set({
            status: 'error',
            message: 'The Instrumentation signals endpoint is unreachable.',
          }),
      });
  }

  private onSignals(signals: UnitSignalReading[]): void {
    const powerSignal = findPowerSignal(signals);
    this.state.set({ status: 'loaded', signal: powerSignal });
    this.pollCount.update((n) => n + 1);

    if (!powerSignal || powerSignal.latestValue === null || !powerSignal.latestTimestampUtc) {
      this.lastReading = null;
      this.periodSeconds.set(null);
      return;
    }

    const current: TimedReading = { value: powerSignal.latestValue, timestampUtc: powerSignal.latestTimestampUtc };
    if (this.lastReading) {
      const rate = deriveRateFromReadings(this.lastReading, current);
      this.periodSeconds.set(periodFromRate(rate));
    }
    this.lastReading = current;
  }

  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get powerSignal(): UnitSignalReading | null {
    const s = this.state();
    return s.status === 'loaded' ? s.signal : null;
  }
  get periodLabel(): string {
    const period = this.periodSeconds();
    if (period === null) return '∞ s';
    return `${period >= 0 ? '+' : ''}${period.toFixed(1)} s`;
  }
}
