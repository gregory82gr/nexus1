import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OverviewApi, Overview } from '../../core/api/overview-api';
import { PlantStateService } from '../../core/state/plant-state';

// Plant Overview (Ch. 6), wired to Nexus1.Bff's real composed
// GET /api/v1/overview/units/{id} -- four independently-nullable sections
// (unit, activeAlarms, radiation, signals), each keyed in `errors` when its
// own call failed, per the endpoint's own proven partial-failure design.
//
// Scoping: the book's own Ch. 6 OverviewComponent is per-unit too (`unit =
// this.state.selected`) -- no mismatch to report here, the real endpoint
// and the book agree on shape. Unit selection uses PlantStateService's
// sensible default (id 1, the first real seeded unit) rather than a real
// cross-screen selector -- see plant-state.ts's own doc comment.
//
// Named gaps against the book's own Figure 6.1, reported rather than
// papered over:
//   - Electrical Output (MWe) / Thermal Power (MWt): the domain has no MWe
//     or MWt field at all -- only LatestPowerPercent, the same gap named
//     in Plant Fleet. Shown as a percentage, not fabricated as MWe/MWt.
//   - The five-node energy-flow mimic (Reactor -> Steam Gen -> Turbine ->
//     Generator -> Grid, chained thermal/steam/rpm/electrical/frequency
//     values): no such chain exists anywhere in the composed DTO --
//     Instrumentation's Signals section is a flat list of whatever signals
//     exist, not six named physics stages. Rendered as a real signal list,
//     not the book's fixed five-node diagram.
//   - Output Trend (60-minute chart): no continuous time-series exists;
//     ReactorFleet's own UnitDetailDto caps RecentPowerSnapshots at 10,
//     most-recent-first. Rendered as a real "last N snapshots" list,
//     named honestly as that, not as a 60-minute window.
//   - Availability: still NO SOURCE, same as the book's own admission --
//     this one wasn't expected to exist yet on either side.
//   - Subsystem Status (six named rows -- Reactor Core, Primary Coolant,
//     Turbine/Generator, Radiation Monitors, Safety Systems, Grid
//     Connection, all NO SOURCE until Ch. 19): the real backend's four
//     composed sections don't map onto those six names, and -- unlike the
//     book's own point in its narrative -- Radiation genuinely IS real
//     data here, not NO SOURCE, because this project built its backend
//     context-by-context well ahead of this frontend port. Replaced with a
//     "Section Status" panel over the four REAL sections (unit,
//     activeAlarms, radiation, signals), each showing OK or the real error
//     message from `errors` -- an honest generalization of the same
//     no-fabricated-verdict discipline, not a literal six-row port.
type OverviewState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; overview: Overview };

@Component({
  selector: 'nx-overview',
  standalone: true,
  templateUrl: './overview.html',
  styleUrl: './overview.scss',
})
export class OverviewComponent {
  private readonly api = inject(OverviewApi);
  private readonly plantState = inject(PlantStateService);
  private readonly destroyRef = inject(DestroyRef);

  readonly unitId = this.plantState.selectedId;
  readonly state = signal<OverviewState>({ status: 'loading' });

  readonly alarmCount = computed(() => {
    const s = this.state();
    return s.status === 'loaded' ? (s.overview.activeAlarms?.length ?? 0) : 0;
  });

  constructor() {
    this.api
      .getOverview(this.unitId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (overview) => this.state.set({ status: 'loaded', overview }),
        error: (err) =>
          this.state.set({
            status: 'error',
            message: err?.status === 404 ? `Unit ${this.unitId()} does not exist.` : 'The overview endpoint is unreachable.',
          }),
      });
  }

  get overview(): Overview | null {
    const s = this.state();
    return s.status === 'loaded' ? s.overview : null;
  }

  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }

  sectionError(section: string): string | null {
    return this.overview?.errors[section] ?? null;
  }
}
