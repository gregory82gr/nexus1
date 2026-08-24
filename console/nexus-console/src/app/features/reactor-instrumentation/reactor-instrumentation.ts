import { Component, DestroyRef, Input, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { InstrumentationApi, UnitSignalReading } from '../../core/api/instrumentation-api';
import { PlantStateService } from '../../core/state/plant-state';
import { SignalGroup, groupByCategory } from './signal-grouping';

// Reactor Core / Control Rods / Neutronics / Coolant-TH / Steam &
// Secondary (Ch. 10, 11, 12, 13 of the book) -- five of the book's
// Reactor screens, consolidated into ONE real component.
//
// WHY CONSOLIDATED, decided explicitly, not assumed: checked
// Instrumentation's real domain model (and its own BFF endpoint's own doc
// comment) before building. There is no CoreState, ControlRodPosition,
// ReactivityMeasurement, CoolantReading, or SteamGeneratorReading entity
// anywhere in the backend -- every one of these five screens would just
// be a filtered view over the same generic Signal/Measurement rows. The
// book's own source-file audits reach the same conclusion from the other
// direction: Ch. 10's rod-bank table (5 rows from 1 real measurement)
// and core map (157 cells from 1 real measurement), and Ch. 12's coolant
// screen (2 real loop readings, the other 2 manufactured with random
// noise so they don't look identical) are both explicit warnings against
// exactly the kind of manufactured per-subsystem resolution six separate
// components reading the same one endpoint would recreate on the
// frontend. So this screen shows the real, undivided signal list,
// honestly grouped by the real CategoryCode field (never by the book's
// subsystem names, which have no backing category in this data).
//
// Control Rods, specifically: the book's own Ch. 10 permanently refuses
// to let the browser move rods ("not the front end's decision to make...
// not ever") -- matching this project's own no-control-authority
// discipline. So this screen is read-only everywhere, including when
// reached via the "Control Rods" nav entry; there is no rod-command UI
// to omit, because none should exist regardless of what the backend
// offers.
//
// All five of the book's routes (core, rods, neutronics, coolant, steam)
// point at this one component (app.routes.ts) -- preserving the nav/URL
// structure Ch. 3 established, while being honest on screen that they
// all render the same real data. `focusLabel` carries only which nav
// entry was clicked, for orientation in the page header; it is not a
// data filter, and the screen says so.
type SignalsState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; groups: SignalGroup[]; total: number };

@Component({
  selector: 'nx-reactor-instrumentation',
  standalone: true,
  templateUrl: './reactor-instrumentation.html',
  styleUrl: './reactor-instrumentation.scss',
})
export class ReactorInstrumentationComponent {
  private readonly api = inject(InstrumentationApi);
  private readonly plantState = inject(PlantStateService);
  private readonly destroyRef = inject(DestroyRef);

  @Input() focusLabel = 'Reactor Instrumentation';

  readonly unitId = this.plantState.selectedId;
  readonly state = signal<SignalsState>({ status: 'loading' });

  readonly reportingCount = computed(() => {
    const s = this.state();
    if (s.status !== 'loaded') return 0;
    return s.groups.reduce((count, g) => count + g.signals.filter((sig) => sig.latestValue !== null).length, 0);
  });

  constructor() {
    this.api
      .getSignals(this.unitId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (signals: UnitSignalReading[]) =>
          this.state.set({ status: 'loaded', groups: groupByCategory(signals), total: signals.length }),
        error: () =>
          this.state.set({
            status: 'error',
            message: 'The Instrumentation signals endpoint is unreachable.',
          }),
      });
  }

  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get loadedGroups(): SignalGroup[] {
    const s = this.state();
    return s.status === 'loaded' ? s.groups : [];
  }
  get totalCount(): number {
    const s = this.state();
    return s.status === 'loaded' ? s.total : 0;
  }
}
