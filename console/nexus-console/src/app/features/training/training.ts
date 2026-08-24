import { DecimalPipe, UpperCasePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { DRILLS, DrillDefinition } from './drills';
import { DrillStore, TimeMultiplier } from './drill-store';

// Training Mode (Ch. 9) -- the one screen in this console where inventing
// the numbers is the point. Every reading in the live-reactor panel is a
// local simulation (core/physics/point-kinetics.ts); every drill's score is a judgement
// the app computes about the operator, not a measurement of anything
// (training-scoring.ts), and always carries calibrated: false.
//
// Genuinely self-contained, confirmed against the book and the real
// backend before building rather than assumed: no HTTP call anywhere in
// this feature, no PlantStateService import, no shared type with any
// real-plant screen (see drill-store.ts's own doc comment, and
// containment.spec.ts, which asserts the import boundary by reading the
// source tree).
@Component({
  selector: 'nx-training',
  standalone: true,
  imports: [DecimalPipe, UpperCasePipe],
  providers: [DrillStore],
  templateUrl: './training.html',
  styleUrl: './training.scss',
})
export class TrainingComponent {
  protected readonly store = inject(DrillStore);
  protected readonly drills: readonly DrillDefinition[] = DRILLS;
  protected readonly timeMultipliers: readonly TimeMultiplier[] = [1, 10, 60, 600];

  get selectedDrill(): DrillDefinition | null {
    return this.store.selectedDrill;
  }

  countdownLabel(): string {
    const drill = this.selectedDrill;
    const progress = this.store.progress();
    if (!drill || !progress) return '00:00';
    const remaining = Math.max(0, Math.ceil(drill.timeLimitSeconds - progress.elapsedSeconds));
    const m = Math.floor(remaining / 60)
      .toString()
      .padStart(2, '0');
    const s = (remaining % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  }

  periodLabel(): string {
    const period = this.store.reactor().periodSeconds;
    if (period === null) return '∞ s';
    return `${period >= 0 ? '+' : ''}${period.toFixed(1)} s`;
  }

  targetLabel(): string {
    const drill = this.selectedDrill;
    if (!drill) return '—';
    if (drill.kind === 'hold') return `${drill.targetPercent}%`;
    if (drill.kind === 'follow') return `${drill.schedule![0].targetPercent}% →`;
    return 'SCRAM on cue';
  }

  onRodInput(value: string): void {
    this.store.setRodPosition(Number(value));
  }
}
