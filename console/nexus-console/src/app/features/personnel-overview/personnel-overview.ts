import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DepartmentRosterEntry, OrganizationApi } from '../../core/api/organization-api';
import { DepartmentStateService } from '../../core/state/department-state';
import { RosterSummary, aggregateRoster } from './personnel-aggregation';

// Personnel Overview (Ch. 17) -- the first screen in this book where the
// right answer is to render LESS than the real data allows, not more.
//
// The real roster endpoint (GET /organization/departments/{id}/roster)
// returns each person's real DisplayName, PersonId, ApplicationUserId,
// PersonnelNumber, and StartDate -- unlike Ch. 16's Rod Inspection, there
// is genuinely nothing missing here. But Ch. 17's own argument is that
// the operational question this screen exists to answer ("does the
// department meet its complement, is every safety-critical position
// covered?") needs counts and position coverage, not names -- and the
// version that omits names is also the simpler screen. So
// personnel-aggregation.ts's aggregateRoster() strips every identifying
// field before this component ever touches it; no name, person id, or
// login reference is held anywhere in this component's own state, let
// alone rendered.
//
// The book's own "one screen that needs names" (guarded, for contacting
// a specific qualified person when a role is uncovered) is deliberately
// NOT built here: it requires a real route guard to be responsible, and
// this Angular app has no auth/guard infrastructure at all yet. Building
// an unguarded names screen would be the wrong call regardless of
// whether the backend could support it -- named as a real gap, not
// silently dropped.
//
// Department-scoped, not unit-scoped (see department-state.ts's own doc
// comment) -- a plain numeric department-id field lets the operator
// change which department is shown, since no "list departments"
// endpoint exists yet to back a real dropdown, and inventing one with
// guessed department names would be less honest than a bare id input.
type RosterState = { status: 'loading' } | { status: 'error'; message: string } | { status: 'loaded'; summary: RosterSummary };

@Component({
  selector: 'nx-personnel-overview',
  standalone: true,
  templateUrl: './personnel-overview.html',
  styleUrl: './personnel-overview.scss',
})
export class PersonnelOverviewComponent {
  private readonly api = inject(OrganizationApi);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly departmentState = inject(DepartmentStateService);

  readonly state = signal<RosterState>({ status: 'loading' });

  constructor() {
    this.load(this.departmentState.selectedId());
  }

  private load(departmentId: number): void {
    this.state.set({ status: 'loading' });
    this.api
      .getDepartmentRoster(departmentId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (entries: DepartmentRosterEntry[]) => this.state.set({ status: 'loaded', summary: aggregateRoster(entries) }),
        error: () =>
          this.state.set({
            status: 'error',
            message: 'The Organization roster endpoint is unreachable.',
          }),
      });
  }

  onDepartmentIdInput(value: string): void {
    const id = Number(value);
    if (!Number.isFinite(id) || id <= 0) return;
    this.departmentState.select(id);
    this.load(id);
  }

  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get summary(): RosterSummary | null {
    const s = this.state();
    return s.status === 'loaded' ? s.summary : null;
  }
}
