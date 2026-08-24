import { Injectable, OnDestroy, signal } from '@angular/core';
import { DRILLS, DrillDefinition } from './drills';
import { DrillProgress, advanceDrill, initProgress } from './drill-runner';
import { DrillScore } from './training-scoring';
import { ReactorState, advanceReactor, resetReactor, scram as scramReactor } from '../../core/physics/point-kinetics';

export type Phase = 'idle' | 'running' | 'frozen' | 'done';
export type TimeMultiplier = 1 | 10 | 60 | 600;

const TICK_MS = 100;

// Deliberately NOT providedIn: 'root'. Registered instead on
// TrainingComponent's own `providers` array (training.ts) -- a
// component-scoped injector, created when the component is instantiated
// and destroyed with it, which for a lazily-routed component means
// "created on navigating to /training, destroyed on navigating away."
// The book's own Ch. 9 registers DrillStore on the route object itself;
// component-level `providers` gets the identical containment property
// (injection only looks upward, so nothing outside this component's own
// subtree can ever obtain one) with one fewer file, since this route has
// exactly one component and no children that would need to share it.
//
// The other half of containment is structural rather than DI-based: this
// file has no import from core/state or core/api, and containment.spec.ts
// asserts that stays true by reading the source tree, not by running the
// app -- Ch. 9's own "architectural, not behavioural" test shape, for a
// rule that is invisible at runtime and easy to violate by accident.
@Injectable()
export class DrillStore implements OnDestroy {
  readonly reactor = signal<ReactorState>(resetReactor(100));
  readonly rodPositionPcm = signal(0);
  readonly selectedDrillId = signal<string | null>(null);
  readonly phase = signal<Phase>('idle');
  readonly progress = signal<DrillProgress | null>(null);
  readonly score = signal<DrillScore | null>(null);
  readonly timeMultiplier = signal<TimeMultiplier>(1);

  private intervalId: ReturnType<typeof setInterval> | null = null;
  private scramPending = false;

  get selectedDrill(): DrillDefinition | null {
    const id = this.selectedDrillId();
    return DRILLS.find((d) => d.id === id) ?? null;
  }

  selectDrill(id: string): void {
    if (this.phase() === 'running') return;
    this.selectedDrillId.set(id);
    this.score.set(null);
  }

  start(): void {
    const drill = this.selectedDrill;
    if (!drill || this.phase() === 'running') return;
    this.reactor.set(resetReactor(drill.initialPowerPercent));
    this.rodPositionPcm.set(0);
    this.progress.set(initProgress(drill));
    this.score.set(null);
    this.scramPending = false;
    this.phase.set('running');
    this.runLoop();
  }

  abort(): void {
    this.stopLoop();
    this.phase.set('idle');
    this.progress.set(null);
  }

  freeze(): void {
    if (this.phase() !== 'running') return;
    this.stopLoop();
    this.phase.set('frozen');
  }

  resume(): void {
    if (this.phase() !== 'frozen') return;
    this.phase.set('running');
    this.runLoop();
  }

  reset(): void {
    this.stopLoop();
    this.phase.set('idle');
    this.selectedDrillId.set(null);
    this.progress.set(null);
    this.score.set(null);
    this.reactor.set(resetReactor(100));
    this.rodPositionPcm.set(0);
  }

  setRodPosition(pcm: number): void {
    this.rodPositionPcm.set(pcm);
  }
  nudgeRod(deltaPcm: number): void {
    this.rodPositionPcm.update((v) => v + deltaPcm);
  }
  setTimeMultiplier(multiplier: TimeMultiplier): void {
    this.timeMultiplier.set(multiplier);
  }
  scram(): void {
    this.scramPending = true;
  }

  private runLoop(): void {
    this.stopLoop();
    this.intervalId = setInterval(() => this.tick(), TICK_MS);
  }

  private stopLoop(): void {
    if (this.intervalId !== null) {
      clearInterval(this.intervalId);
      this.intervalId = null;
    }
  }

  private tick(): void {
    const drill = this.selectedDrill;
    const progress = this.progress();
    if (!drill || !progress) return;

    const dtSeconds = (TICK_MS / 1000) * this.timeMultiplier();
    const scramThisTick = this.scramPending;
    this.scramPending = false;

    const reactorBefore = this.reactor();
    const reactor = scramThisTick ? scramReactor(reactorBefore) : advanceReactor(reactorBefore, this.rodPositionPcm(), dtSeconds);
    this.reactor.set(reactor);

    const result = advanceDrill(drill, progress, reactor, dtSeconds, scramThisTick);
    this.progress.set(result.progress);

    if (result.done) {
      this.stopLoop();
      this.phase.set('done');
      this.score.set(result.score);
      // A completed drill's result would, per the book's own Ch. 9,
      // be logged to Incident Analysis and the Compliance log, tagged
      // TRAINING with no real unitId. Neither log exists yet in this
      // frontend (Ch. 23/30 aren't built), so that step is a named gap,
      // not a silent omission -- containment here is enforced only by
      // the source-import absence containment.spec.ts asserts.
    }
  }

  // Called by Angular when the providing injector (the /training route)
  // is destroyed -- the timer must not survive route navigation, the
  // same "lazy routes really destroy things" discipline Ch. 8 enforces
  // for a WebGL context, applied here to a JS interval instead.
  ngOnDestroy(): void {
    this.stopLoop();
  }
}
