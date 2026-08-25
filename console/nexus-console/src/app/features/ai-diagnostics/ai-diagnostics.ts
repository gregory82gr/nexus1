import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RootCauseCase, RootCauseCasesApi } from '../../core/api/root-cause-cases-api';
import { PlantStateService } from '../../core/state/plant-state';

// AI Diagnostics (Ch. 24) -- the first screen about advice, not data. The
// book's own source is candid: a DSLM (Domain-Specific Language Model)
// advisory panel labeled ROADMAP · PLANNED, explicitly "not running in this
// build," above a Predictive Diagnostics panel labeled a working Phase-0
// demonstrator. The first disclaimer survives unchanged below. The second
// panel does NOT survive as-is -- two problems, checked directly before
// building:
//
// 1. The book's own Component Risk table renders risk percentages inside
//    the SAME <span class="led ok/warn/crit"> classes the real alarm table
//    and safety panels use for genuinely wired conditions -- a demo score
//    visually claiming the same authority as a live alarm. Fixed here by
//    construction, not just convention: this screen's real-data panel below
//    uses its own distinct `.case-status` styling (features/ai-diagnostics/
//    ai-diagnostics.scss), never `.pill.ok/.warn/.crit` or the alarm/safety
//    LED classes, even though (unlike the book's fabricated score) the data
//    behind it is genuinely real.
//
// 2. The DSLM "Example Interaction" accordion cites "Component Registry"
//    and a "Root-Cause graph" -- checked directly: ComponentRegistry does
//    not exist anywhere in this codebase yet (Ch. 28, still a
//    PlaceholderComponent route, not reached); RootCause's real domain
//    (ADR-005) is a minimal investigation-case workflow with NO scored
//    causal graph, NO per-component risk concept, and NO confidence value
//    anywhere -- confirmed by reading every entity in
//    Nexus1.RootCause.Domain. The example interaction below is kept, and
//    marked ILLUSTRATIVE ONLY: it does not imply this system can produce a
//    cited explanation today, and no citation-generation, grounding, or
//    RAG pipeline of any kind is built here -- that remains a distinct,
//    future scope decision (a later book phase), not opened by this slice.
//
// What IS real and shown below: RootCause's actual investigation-case
// history for the selected unit (an alarm flood opened a case; eventually a
// free-text verdict), via Reporting's own projection
// (GetCaseSummariesForUnitQuery, already live). This is genuinely
// diagnostic in nature -- real investigations, real verdicts -- and named
// honestly as case history, never as a predictive score or percentage,
// because RootCause's domain model has neither. See
// core/api/root-cause-cases-api.ts's own doc comment for why this endpoint
// belongs to this screen rather than the book's own "Trends & History"
// label an earlier slice's route comment guessed at.
type CasesState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; cases: RootCauseCase[] };

@Component({
  selector: 'nx-ai-diagnostics',
  standalone: true,
  templateUrl: './ai-diagnostics.html',
  styleUrl: './ai-diagnostics.scss',
})
export class AiDiagnosticsComponent {
  private readonly api = inject(RootCauseCasesApi);
  private readonly plantState = inject(PlantStateService);
  private readonly destroyRef = inject(DestroyRef);

  readonly unitId = this.plantState.selectedId;
  readonly state = signal<CasesState>({ status: 'loading' });

  constructor() {
    this.api
      .getCasesForUnit(this.unitId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (cases: RootCauseCase[]) => this.state.set({ status: 'loaded', cases }),
        error: () =>
          this.state.set({
            status: 'error',
            message: 'The Reporting case-history endpoint is unreachable.',
          }),
      });
  }

  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get loadedCases(): RootCauseCase[] {
    const s = this.state();
    return s.status === 'loaded' ? s.cases : [];
  }
}
