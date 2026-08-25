import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActiveAlarm, AlarmManagementApi } from '../../core/api/alarm-management-api';
import { AlarmGroup, groupBySeverity } from './alarm-grouping';

// Alarms & Events (Ch. 23) -- this chapter's subject is the aggregator
// itself, not a new measurement. The book's own source mixes two
// structurally different rows in identical styling: 11 hand-written
// pooled events drawn at random every 5 seconds (decorative, checking no
// real condition), and a real #rod-scram click handler that pushes
// "Manual SCRAM initiated" the instant the button is clicked, before any
// rod has actually moved (a real trigger that overclaims its own effect).
//
// Checked directly before building, same discipline as every prior
// cluster this arc:
//
// 1. No decorative/random alarm generation is ported here, by design --
//    no timer, no invented pooled events. Every alarm this screen shows
//    comes from the real fleet-wide active-alarms endpoint, already live
//    and proven since the first BFF vertical slice.
//
// 2. This system's AlarmManagement context DOES have one real
//    condition-check mechanism (AlarmDefinition.Evaluate, a genuine
//    threshold comparison) -- but nothing in this solution invokes it
//    automatically. EvaluateReadingCommand exists in Application with
//    zero live wiring (no background service, no event handler
//    subscribing to any other context's state). Every alarm currently
//    seeded in this system (95 active, at this slice's own live check)
//    is manually-inserted demo/test residue, not proof of live condition
//    monitoring -- named explicitly, not implied by this screen's
//    existence.
//
// 3. The book's own #rod-scram premature-firing risk (an alarm claiming
//    an effect before the effect has happened) does NOT exist in this
//    system today: checked solution-wide, there is no SCRAM or
//    rod-position WRITE path anywhere in this backend, and the frontend's
//    only SCRAM-adjacent code is Training Mode's own client-side,
//    stateless drill reducer (features/training/drill-runner.ts) with no
//    backend call at all. Control Rods stay read-only everywhere (see
//    reactor-instrumentation.ts's own doc comment) -- so there is no
//    button anywhere in this console that could raise an alarm ahead of
//    a real state change. This is named here explicitly so a future
//    Ch.23-style "action button" is not added to AlarmManagement without
//    re-checking this guarantee: AlarmEvent.Raise itself is an ungated
//    factory (Domain layer) that would accept a fabricated alarm from any
//    caller with no verification of what actually triggered it.
//
// Acknowledge is a real write (AcknowledgeAlarmCommand, already proven:
// changes AlarmEvent.State in the database, confirmed by direct SQL in
// the original 2026-08-22 evidence session) -- unlike a hypothetical
// scram button, it never claims a plant-state effect it didn't cause; it
// only ever changes the alarm's own acknowledgement state.
const ACKNOWLEDGED_BY_USER_ID = '11111111-1111-1111-1111-111111111111'; // same placeholder operator id as the original read/write BFF evidence session -- no login/auth system exists in this console yet.

type AlarmsState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; groups: AlarmGroup[]; total: number };

@Component({
  selector: 'nx-alarms-events',
  standalone: true,
  templateUrl: './alarms-events.html',
  styleUrl: './alarms-events.scss',
})
export class AlarmsEventsComponent {
  private readonly api = inject(AlarmManagementApi);
  private readonly destroyRef = inject(DestroyRef);

  readonly state = signal<AlarmsState>({ status: 'loading' });
  readonly acknowledgingIds = signal<ReadonlySet<number>>(new Set());

  constructor() {
    this.fetch();
  }

  private fetch(): void {
    this.api
      .getActiveAlarms()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (alarms: ActiveAlarm[]) => this.state.set({ status: 'loaded', groups: groupBySeverity(alarms), total: alarms.length }),
        error: () =>
          this.state.set({
            status: 'error',
            message: 'The AlarmManagement active-alarms endpoint is unreachable.',
          }),
      });
  }

  acknowledge(alarmEventId: number): void {
    this.acknowledgingIds.update((ids) => new Set(ids).add(alarmEventId));
    this.api
      .acknowledge(alarmEventId, ACKNOWLEDGED_BY_USER_ID)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.fetch(),
        error: () => this.acknowledgingIds.update((ids) => { const next = new Set(ids); next.delete(alarmEventId); return next; }),
      });
  }

  isAcknowledging(alarmEventId: number): boolean {
    return this.acknowledgingIds().has(alarmEventId);
  }

  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get loadedGroups(): AlarmGroup[] {
    const s = this.state();
    return s.status === 'loaded' ? s.groups : [];
  }
  get totalCount(): number {
    const s = this.state();
    return s.status === 'loaded' ? s.total : 0;
  }
}
