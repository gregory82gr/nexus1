import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActiveDegradationCase, MaintenanceApi } from '../../core/api/maintenance-api';
import { SeverityTone, severityTone } from './severity-tone';

// Ageing & Degradation (Ch. 18) -- wired to a real, newly-added BFF route
// (GET /maintenance/degradation-cases) wrapping an Application-layer
// query that already existed (GetActiveDegradationCasesQuery/
// ActiveDegradationCaseDto, atlas C.9.5.2 query 5) but had never been
// mapped to any HTTP route before this slice. Genuinely fleet-wide, not
// unit- or department-scoped -- the query itself takes no parameter.
//
// The book's own Ch. 18 is almost entirely about ONE argument: a progress
// bar asserts continuous measurement toward a known, monotonically-
// approached endpoint, and almost nothing on a decade timescale (vessel
// embrittlement, tube wear, insulation ageing) can support that claim.
// The honest replacement it lands on is "last real reading and how many
// data points exist, not a percentage" -- and that is exactly the shape
// this real endpoint already returns: Mechanism, Severity, DetectedAtUtc,
// and a COUNT of trend points, with no life-consumed percentage
// anywhere.
//
// This is narrower than the book's own chart, named explicitly rather
// than silently: the book's AgeingSeries draws individual measured
// points against a limit line with a widening projection band. This
// endpoint exposes none of that -- no per-point values (only a count),
// no limit/threshold field, and no per-record trend detail query exists
// anywhere in this codebase yet. So this screen renders the honest case
// list the real data supports (which data points exist, how many, how
// severe, when detected) rather than the fuller surveillance chart,
// and says so on screen -- the same restraint as Model Analysis naming
// why its own checks are narrower than the book's six-group solver
// audit.
//
// Decommissioning and Waste & Spent Fuel are not built at all: checked
// directly before this slice, neither has any entity, table, or concept
// anywhere in Maintenance's domain -- a total-absence gap, the same
// shape as Security's own zone-access finding, not missing fields on an
// otherwise-shaped model. Matches the book's own admission that its
// source file's numbers here are entirely generated from commissioning
// year and a seeded random factor -- there was nothing to connect on
// either side, book or backend.
type CasesState = { status: 'loading' } | { status: 'error'; message: string } | { status: 'loaded'; cases: ActiveDegradationCase[] };

@Component({
  selector: 'nx-ageing-degradation',
  standalone: true,
  templateUrl: './ageing-degradation.html',
  styleUrl: './ageing-degradation.scss',
})
export class AgeingDegradationComponent {
  private readonly api = inject(MaintenanceApi);
  private readonly destroyRef = inject(DestroyRef);

  readonly state = signal<CasesState>({ status: 'loading' });

  constructor() {
    this.api
      .getActiveDegradationCases()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (cases) => this.state.set({ status: 'loaded', cases }),
        error: () =>
          this.state.set({
            status: 'error',
            message: 'The Maintenance degradation-cases endpoint is unreachable.',
          }),
      });
  }

  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get loadedCases(): ActiveDegradationCase[] {
    const s = this.state();
    return s.status === 'loaded' ? s.cases : [];
  }

  toneOf(severity: string): SeverityTone {
    return severityTone(severity);
  }
}
