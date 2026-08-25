import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ClampedRecommendation, PolicyGridEntry, ReinforcementLearningApi } from '../../core/api/reinforcement-learning-api';

// Optimization (Ch. 25) -- fleet-wide, not unit-scoped: neither real query
// behind this screen takes a unit parameter (GetActivePolicyIdQuery/
// GetPolicyGridQuery resolve one fleet-wide "active" policy;
// GetClampedRecommendationsQuery has no scoping at all).
//
// Already settled before this cluster (ADR-026): ReinforcementLearning is
// training/persistence only. There is no live advisory computation, no
// running RL agent, and no real-time "here's what to do next" suggestion
// engine anywhere in this codebase -- confirmed again directly in Domain
// before building: no Episode/EpisodeStep/reward-trend entity exists at
// all (TrainingRun.TotalReward/AverageReward are single aggregate values
// per run, not a series), and no query lists training-run history or
// policy-version metadata (RecordTrainingRunCommand/ExtractPolicyCommand
// are write-only, no corresponding read side was ever built). Both are
// named gaps below, not fabricated.
//
// What IS real and shown here:
//
// 1. The active policy grid (state -> best action, real learned Q-values
//    and margins) -- but "active policy" is itself an application-layer
//    judgment call (GetActivePolicyIdQueryHandler's own doc comment: the
//    most recently extracted Policy whose source QTable IsFinal), not a
//    recovered domain fact -- there is no IsCurrent flag anywhere.
//    Carried-forward finding, restated here because it bears directly on
//    what this screen shows: Policy/PolicyEntry are real, materialized
//    tables, not the book's own Appendix C design (a SQL VIEW recomputed
//    on every read that "cannot drift from the values beneath it"). Here
//    it CAN drift from QTableEntry if ExtractPolicyCommand isn't re-run
//    after a QTable update -- this grid is a snapshot as of its own
//    extraction, not a live-computed view.
// 2. Clamped-recommendation history -- a real, already-recorded list of
//    past advisory recommendations that were clamped to a safety band.
//    This is recorded history, not a live "why this action" explanation:
//    nothing in this codebase reads a live plant state or computes a
//    recommendation in response to it.
type PolicyState =
  | { status: 'loading' }
  | { status: 'no-policy' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; entries: PolicyGridEntry[] };

type RecommendationsState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; recommendations: ClampedRecommendation[] };

@Component({
  selector: 'nx-optimization',
  standalone: true,
  templateUrl: './optimization.html',
  styleUrl: './optimization.scss',
})
export class OptimizationComponent {
  private readonly api = inject(ReinforcementLearningApi);
  private readonly destroyRef = inject(DestroyRef);

  readonly policyState = signal<PolicyState>({ status: 'loading' });
  readonly recommendationsState = signal<RecommendationsState>({ status: 'loading' });

  constructor() {
    this.api
      .getActivePolicyGrid()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (entries) => this.policyState.set(entries === null ? { status: 'no-policy' } : { status: 'loaded', entries }),
        error: () => this.policyState.set({ status: 'error', message: 'The ReinforcementLearning policy endpoint is unreachable.' }),
      });

    this.api
      .getClampedRecommendations()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (recommendations) => this.recommendationsState.set({ status: 'loaded', recommendations }),
        error: () => this.recommendationsState.set({ status: 'error', message: 'The ReinforcementLearning recommendations endpoint is unreachable.' }),
      });
  }

  get policyErrorMessage(): string {
    const s = this.policyState();
    return s.status === 'error' ? s.message : '';
  }
  get loadedPolicyEntries(): PolicyGridEntry[] {
    const s = this.policyState();
    return s.status === 'loaded' ? s.entries : [];
  }
  get recommendationsErrorMessage(): string {
    const s = this.recommendationsState();
    return s.status === 'error' ? s.message : '';
  }
  get loadedRecommendations(): ClampedRecommendation[] {
    const s = this.recommendationsState();
    return s.status === 'loaded' ? s.recommendations : [];
  }
}
