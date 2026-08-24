import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OrganizationApi, StaffingScenarioGap } from '../../core/api/organization-api';

// Absence Stress Test (Ch. 17) -- wired to a real, newly-added BFF route
// (GET /organization/staffing-scenarios/{id}/gaps) wrapping an
// Application-layer query that already existed
// (GetLatestStaffingGapsQuery/StaffingScenarioGapDto, atlas C.3.8 query 3)
// but had never been mapped to any HTTP route before this slice.
//
// Position-level required-vs-available headcount for a named scenario's
// most recently recorded evaluation -- no names, no absence reasons,
// same minimization shape as Personnel Overview's own aggregation. The
// book's scenarios "remove role slots, not people"; this screen's
// numbers are exactly that: real recorded role-slot counts, not a
// simulated individual roster.
//
// Named gap, not fabricated: StaffingScenarioGapDto exposes a raw
// PositionId with no title resolution anywhere in this endpoint (the
// roster's own PositionTitle field belongs to a different query, scoped
// to a different department, and cross-referencing the two would be a
// fragile guess dressed up as a real join). Rendered honestly as
// "Position #{id}".
//
// An empty response means the scenario has never been evaluated (no
// StaffingScenarioResult recorded yet) -- a real, loaded, empty state,
// not an error, matching every other list endpoint's own "real 200, not
// a 404" convention in this host.
type GapsState = { status: 'loading' } | { status: 'error'; message: string } | { status: 'loaded'; gaps: StaffingScenarioGap[] };

@Component({
  selector: 'nx-absence-stress-test',
  standalone: true,
  templateUrl: './absence-stress-test.html',
  styleUrl: './absence-stress-test.scss',
})
export class AbsenceStressTestComponent {
  private readonly api = inject(OrganizationApi);
  private readonly destroyRef = inject(DestroyRef);

  // Scenario-scoped, not department- or unit-scoped -- local to this
  // screen since nothing else in the console needs "the current staffing
  // scenario." Defaults to 1, the scenario seeded for this slice's own
  // live evidence.
  readonly scenarioId = signal(1);
  readonly state = signal<GapsState>({ status: 'loading' });

  readonly breachedCount = computed(() => {
    const s = this.state();
    return s.status === 'loaded' ? s.gaps.filter((g) => g.gapCount > 0).length : 0;
  });

  constructor() {
    this.load(this.scenarioId());
  }

  private load(scenarioId: number): void {
    this.state.set({ status: 'loading' });
    this.api
      .getStaffingGaps(scenarioId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (gaps) => this.state.set({ status: 'loaded', gaps }),
        error: () =>
          this.state.set({
            status: 'error',
            message: 'The Organization staffing-gaps endpoint is unreachable.',
          }),
      });
  }

  onScenarioIdInput(value: string): void {
    const id = Number(value);
    if (!Number.isFinite(id) || id <= 0) return;
    this.scenarioId.set(id);
    this.load(id);
  }

  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get loadedGaps(): StaffingScenarioGap[] {
    const s = this.state();
    return s.status === 'loaded' ? s.gaps : [];
  }
}
