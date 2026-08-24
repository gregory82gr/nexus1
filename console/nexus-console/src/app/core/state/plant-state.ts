import { Injectable, signal } from '@angular/core';

// core/state/plant-state.ts -- selectedId is the only writer (the book's
// own Deep Dive 7 framing). Minimal today: a sensible default (the first
// real seeded unit, id 1) rather than the book's full cross-screen
// selector, since Plant Fleet's own Select action is still local, visual-
// only state (named as deferred in fleet.ts) -- nothing writes into this
// signal yet. Wiring Plant Fleet's Select to call `select()` here is the
// natural next increment, not built today.
@Injectable({ providedIn: 'root' })
export class PlantStateService {
  readonly selectedId = signal(1);

  select(unitId: number): void {
    this.selectedId.set(unitId);
  }
}
