import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReadinessFailure, RoboticsApi, UnitMission } from '../../core/api/robotics-api';
import { PlantStateService } from '../../core/state/plant-state';

// Mission Readiness (Ch. 19) -- reshaped around the real domain rather
// than the book's own design, and reported/decided explicitly before
// building.
//
// The book's own Mission Readiness computes, LIVE, whether the CURRENT
// fleet can cover each of six abstract "standard mission types"
// (Containment survey, Leak inspection, ...), using capability tags,
// battery, and accumulated radiation dose against a per-mission dose
// budget. Checked the real domain directly before assuming any of that
// carries over: it does not, on two separate axes --
//
//  1. No dose/radiation field exists anywhere on Robot or
//     RobotHealthSnapshot. Total absence, not a nullable column this
//     screen chooses not to show.
//  2. Mission (UnitId, MissionType/Status/Priority, timing) is a real,
//     already-dispatched work order, not an abstract mission-type
//     definition evaluated hypothetically against the fleet. There is no
//     "given the current fleet, could mission type X be covered right
//     now" concept anywhere in this domain.
//
// What the real domain DOES have, and what this screen is built around
// instead: MissionReadinessAssessment/MissionReadinessItem -- a genuinely
// recorded readiness verdict for one specific, already-known mission,
// with named blocking checks (CheckName/IsBlocking/Detail). This is the
// real analogue of the book's own "decompression panel" (why a mission's
// readiness failed), just scoped to a mission that already exists and
// was already assessed, not a live hypothetical evaluation.
//
// Two thin BFF routes were added for this (GET
// /robotics/missions/{id}/readiness-failures, wrapping
// GetBlockingReadinessFailuresQueryHandler, which already existed and was
// already registered). A further real limitation, found while wiring
// this up, not assumed: UnitMissionDto (the mission list this screen's
// own overview panel shows) carries a mission's Code but not its numeric
// Id, so there is no way to drill from a row in that list straight into
// its own readiness detail -- the lookup below is a separate, manually
// keyed tool, not a click-through, and the screen says so.
type MissionsState = { status: 'loading' } | { status: 'error'; message: string } | { status: 'loaded'; missions: UnitMission[] };
type FailuresState = { status: 'idle' } | { status: 'loading' } | { status: 'error'; message: string } | { status: 'loaded'; failures: ReadinessFailure[] };

@Component({
  selector: 'nx-mission-readiness',
  standalone: true,
  templateUrl: './mission-readiness.html',
  styleUrl: './mission-readiness.scss',
})
export class MissionReadinessComponent {
  private readonly api = inject(RoboticsApi);
  private readonly plantState = inject(PlantStateService);
  private readonly destroyRef = inject(DestroyRef);

  readonly unitId = this.plantState.selectedId;
  readonly missionsState = signal<MissionsState>({ status: 'loading' });
  readonly failuresState = signal<FailuresState>({ status: 'idle' });
  readonly lookupMissionId = signal('1');

  constructor() {
    this.api
      .getUnitOverview(this.unitId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (overview) => this.missionsState.set({ status: 'loaded', missions: overview.missions }),
        error: () =>
          this.missionsState.set({
            status: 'error',
            message: 'The Robotics unit-overview endpoint is unreachable.',
          }),
      });
  }

  onLookupIdInput(value: string): void {
    this.lookupMissionId.set(value);
  }

  lookupReadiness(): void {
    const id = this.lookupMissionId().trim();
    if (!id) return;
    this.failuresState.set({ status: 'loading' });
    this.api
      .getReadinessFailures(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (failures) => this.failuresState.set({ status: 'loaded', failures }),
        error: () =>
          this.failuresState.set({
            status: 'error',
            message: 'The Robotics readiness-failures endpoint is unreachable.',
          }),
      });
  }

  get missionsErrorMessage(): string {
    const s = this.missionsState();
    return s.status === 'error' ? s.message : '';
  }
  get loadedMissions(): UnitMission[] {
    const s = this.missionsState();
    return s.status === 'loaded' ? s.missions : [];
  }
  get failuresErrorMessage(): string {
    const s = this.failuresState();
    return s.status === 'error' ? s.message : '';
  }
  get loadedFailures(): ReadinessFailure[] {
    const s = this.failuresState();
    return s.status === 'loaded' ? s.failures : [];
  }
}
