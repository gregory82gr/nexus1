import { Component, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { InstrumentationApi } from '../../core/api/instrumentation-api';
import { ReactorFleetApi } from '../../core/api/reactor-fleet-api';
import { TimedReading } from '../../core/physics/point-kinetics';
import { PlantStateService } from '../../core/state/plant-state';
import { evaluateCommand, EvaluatorDeps } from './command-evaluator';
import { parseCommand } from './command-parser';

export interface HistoryEntry {
  readonly command: string;
  readonly output: string;
}

// NX-Script Console (Ch. 32) -- the book's own finding here is narrower
// than every prior gap chapter: this screen is a read-only query
// interpreter over already-loaded console state, and its only correction
// is disclosure (the `select` side effect is console-wide, previously
// only a developer comment, never on-screen -- see the label in
// nx-script.html). The vocabulary itself needed real investigation
// (Ch. 32's own report): of the Phase-0 demo's 14 named signals, only
// `power` (fleet-wide, ReactorFleet.UnitSummaryDto.LatestPowerPercent)
// and `period`/`kin_power` (per-unit, Instrumentation's real POWER/
// NEUTRONICS-category signal, the same one Reactor Kinetics already
// polls) have any real backing. The other 11 are total absences --
// see signal-catalog.ts for the specific, investigated reason each one
// is refused rather than answered. `select` writes through the real,
// already-existing (if previously unwritten-to) PlantStateService --
// see command-evaluator.ts and plant-state.ts.
@Component({
  selector: 'nx-script',
  standalone: true,
  templateUrl: './nx-script.html',
  styleUrl: './nx-script.scss',
})
export class NxScriptComponent {
  private readonly reactorFleetApi = inject(ReactorFleetApi);
  private readonly instrumentationApi = inject(InstrumentationApi);
  private readonly plantState = inject(PlantStateService);

  private readonly lastKineticsReading = new Map<number, TimedReading>();

  readonly history = signal<HistoryEntry[]>([]);
  readonly selectedUnitId = this.plantState.selectedId;
  readonly running = signal(false);

  private readonly deps: EvaluatorDeps = {
    fetchFleetUnits: () => firstValueFrom(this.reactorFleetApi.getUnits()),
    fetchUnitSignals: (unitId: number) => firstValueFrom(this.instrumentationApi.getSignals(unitId)),
    selectedUnitId: () => this.plantState.selectedId(),
    selectUnit: (unitId: number) => this.plantState.select(unitId),
    lastKineticsReading: this.lastKineticsReading,
  };

  async run(rawCommand: string): Promise<void> {
    const command = rawCommand.trim();
    if (command.length === 0) return;

    this.running.set(true);
    let output: string;
    try {
      output = await evaluateCommand(parseCommand(command), this.deps);
    } catch {
      output = 'the backend is unreachable for this command.';
    }
    this.running.set(false);
    this.history.update((h) => [...h, { command, output }]);
  }
}
