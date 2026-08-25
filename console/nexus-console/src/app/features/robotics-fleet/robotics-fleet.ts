import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RoboticsApi, UnitRobotStatus } from '../../core/api/robotics-api';
import { PlantStateService } from '../../core/state/plant-state';

// Robotics Fleet Overview (Ch. 19) -- wired to the real, already-proven
// GET /robotics/units/{id}. Fully real: robot code, name, status, latest
// battery/communication status. No dose or radiation field exists
// anywhere in the real domain (checked Robot/RobotHealthSnapshot
// directly, not just the DTO) -- a total absence, not a nullable field
// this screen is choosing to omit. The book's own Fleet Overview shows a
// synthetic accumulated-dose figure per robot; this screen simply has no
// such data to show, named here rather than fabricated.
type FleetState = { status: 'loading' } | { status: 'error'; message: string } | { status: 'loaded'; robots: UnitRobotStatus[] };

@Component({
  selector: 'nx-robotics-fleet',
  standalone: true,
  templateUrl: './robotics-fleet.html',
  styleUrl: './robotics-fleet.scss',
})
export class RoboticsFleetComponent {
  private readonly api = inject(RoboticsApi);
  private readonly plantState = inject(PlantStateService);
  private readonly destroyRef = inject(DestroyRef);

  readonly unitId = this.plantState.selectedId;
  readonly state = signal<FleetState>({ status: 'loading' });

  constructor() {
    this.api
      .getUnitOverview(this.unitId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (overview) => this.state.set({ status: 'loaded', robots: overview.robots }),
        error: () =>
          this.state.set({
            status: 'error',
            message: 'The Robotics unit-overview endpoint is unreachable.',
          }),
      });
  }

  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get loadedRobots(): UnitRobotStatus[] {
    const s = this.state();
    return s.status === 'loaded' ? s.robots : [];
  }
}
