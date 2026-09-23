import { HttpErrorResponse } from '@angular/common/http';
import { DestroyRef, Injectable, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DiagnosisCandidate, DiagnosisResponse, RootCauseDiagnosisApi } from '../../core/api/root-cause-diagnosis-api';

// Ch. 29's framework note: a root-cause conclusion has EXACTLY ONE place it is
// produced, and every screen that needs it reads that one place -- so two
// screens can never disagree about the same incident. Adapted to this project:
// the one place does not COMPUTE the answer, it FETCHES it once from the real
// engine (ADR-034/ADR-035) and caches it. Root Cause and Incident Summary both
// inject THIS service and read the same signal; a screen may display the answer
// differently, but it is not entitled to a different answer.
/** The one incident the diagnosis backend serves today (ADR-034). Any other id 404s. */
export const FIXED_INCIDENT_ID = 'EVT-2026-0418';

export type DiagnosisState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'ready'; result: DiagnosisResponse }
  | { status: 'abstained'; reason: string; result: DiagnosisResponse }
  | { status: 'unknown' } // 404: the backend has no such incident
  | { status: 'error'; message: string }; // 502/503/other: the pipeline could not run

@Injectable({ providedIn: 'root' })
export class RootCauseService {
  private readonly api = inject(RootCauseDiagnosisApi);
  private readonly destroyRef = inject(DestroyRef);

  private readonly _state = signal<DiagnosisState>({ status: 'idle' });
  readonly state = this._state.asReadonly();

  private loadedIncidentId: string | null = null;

  /** Fetch the diagnosis once per incident and cache it; repeat calls for a loaded/loading incident are no-ops so both screens share one computation and one network run. */
  load(incidentId: string): void {
    const status = this._state().status;
    if (this.loadedIncidentId === incidentId && status !== 'idle' && status !== 'error') {
      return;
    }

    this.loadedIncidentId = incidentId;
    this._state.set({ status: 'loading' });

    this.api
      .runDiagnosis(incidentId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result: DiagnosisResponse) =>
          this._state.set(
            result.abstained
              ? { status: 'abstained', reason: result.abstainReason ?? 'inconclusive', result }
              : { status: 'ready', result },
          ),
        error: (err: HttpErrorResponse) =>
          this._state.set(
            err.status === 404
              ? { status: 'unknown' }
              : {
                  status: 'error',
                  message:
                    err.status === 502 || err.status === 503
                      ? 'The diagnosis service is unavailable.'
                      : 'The diagnosis request failed.',
                },
          ),
      });
  }

  /** The ranked candidates, only when a verdict was actually reached (never on an abstention). */
  readonly rankedCandidates = computed(() => {
    const s = this._state();
    return s.status === 'ready' ? s.result.candidates : [];
  });

  /** The single top cause both screens read (Ch. 29). Null unless a verdict was reached. */
  readonly topCause = computed<DiagnosisCandidate | null>(() => this.rankedCandidates()[0] ?? null);

  readonly citations = computed(() => {
    const s = this._state();
    return s.status === 'ready' ? s.result.citations : [];
  });

  /** The audit-chain head that sealed the run -- present for a verdict or a (first-class) abstention. */
  readonly auditHash = computed(() => {
    const s = this._state();
    return s.status === 'ready' || s.status === 'abstained' ? s.result.auditHash : null;
  });
}
