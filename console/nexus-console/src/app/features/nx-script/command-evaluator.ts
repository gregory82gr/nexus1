import { UnitSignalReading } from '../../core/api/instrumentation-api';
import { UnitSummary } from '../../core/api/reactor-fleet-api';
import { TimedReading, deriveRateFromReadings, periodFromRate } from '../../core/physics/point-kinetics';
import { findPowerSignal } from '../reactor-kinetics/power-signal';
import { ParsedCommand } from './command-parser';
import { absenceRefusal, findSignal, offUnitRefusal, verbRefusal } from './signal-catalog';

// Deliberately plain functions (fetchFleetUnits/fetchUnitSignals), not
// the Angular services themselves -- lets this whole evaluator run
// against fixed fixture data in a unit test, with no TestBed/HTTP
// harness, the same separation reactor-kinetics.ts keeps between
// core/physics/point-kinetics.ts (pure) and its own component (I/O).
export interface EvaluatorDeps {
  fetchFleetUnits(): Promise<UnitSummary[]>;
  fetchUnitSignals(unitId: number): Promise<UnitSignalReading[]>;
  selectedUnitId(): number;
  selectUnit(unitId: number): void;
  // Keyed by unit id, not a single field, because NX-Script (unlike the
  // fixed-unit Reactor Kinetics screen) can select a different unit
  // between two `get period` calls -- comparing readings across a unit
  // switch would derive a rate from two different reactors, not a real
  // period. Cleared implicitly by simply never mixing readings across keys.
  readonly lastKineticsReading: Map<number, TimedReading>;
}

async function resolveUnit(unitId: number, deps: EvaluatorDeps): Promise<UnitSummary | null> {
  const units = await deps.fetchFleetUnits();
  return units.find((u) => u.id === unitId) ?? null;
}

async function evaluateKinetics(signalName: 'period' | 'kin_power', unitId: number, deps: EvaluatorDeps): Promise<string> {
  const signals = await deps.fetchUnitSignals(unitId);
  const powerSignal = findPowerSignal(signals);
  if (!powerSignal || powerSignal.latestValue === null || !powerSignal.latestTimestampUtc) {
    return `${signalName}: no power-like signal is currently reporting for u${unitId}.`;
  }

  const current: TimedReading = { value: powerSignal.latestValue, timestampUtc: powerSignal.latestTimestampUtc };

  if (signalName === 'kin_power') {
    deps.lastKineticsReading.set(unitId, current);
    return `kin_power (u${unitId}, ${powerSignal.tag}) = ${powerSignal.latestValue}`;
  }

  const previous = deps.lastKineticsReading.get(unitId);
  deps.lastKineticsReading.set(unitId, current);
  if (!previous) {
    return `period: only one reading observed so far for u${unitId} -- run 'get period' again in a few seconds to derive a rate from two real readings.`;
  }

  const rate = deriveRateFromReadings(previous, current);
  if (rate === null) {
    return `period: cannot derive a rate from the two most recent readings for u${unitId} (need two distinct, positive, time-separated readings) -- try again shortly.`;
  }

  const period = periodFromRate(rate);
  if (period === null) {
    return `period (u${unitId}) = critical (rate ~0, no measurable period)`;
  }
  return `period (u${unitId}) = ${period >= 0 ? '+' : ''}${period.toFixed(1)} s`;
}

async function evaluateGet(parsed: Extract<ParsedCommand, { kind: 'get' }>, deps: EvaluatorDeps): Promise<string> {
  const signal = findSignal(parsed.signal);
  if (!signal) {
    return `unknown identifier '${parsed.signal}' -- not part of this console's signal vocabulary.`;
  }
  if (!signal.real) {
    return absenceRefusal(signal);
  }

  if (signal.tier === 'kinetics') {
    if (parsed.scope === 'fleet') {
      return `${signal.name}: point-kinetics signals are per-unit only -- fleet.${signal.name} is not defined.`;
    }
    let targetUnitId: number;
    if (parsed.scope === 'bare') {
      targetUnitId = deps.selectedUnitId();
    } else {
      const unit = await resolveUnit(parsed.scope.unit, deps);
      if (!unit) {
        return `u${parsed.scope.unit} is not a real unit in this fleet.`;
      }
      if (unit.id !== deps.selectedUnitId()) {
        return offUnitRefusal(signal.name, deps.selectedUnitId(), unit.id);
      }
      targetUnitId = unit.id;
    }
    return evaluateKinetics(signal.name as 'period' | 'kin_power', targetUnitId, deps);
  }

  // signal.tier === 'fleet' and real: only 'power'.
  if (parsed.scope === 'fleet') {
    const units = await deps.fetchFleetUnits();
    const parts = units.map((u) => `${u.code}: ${u.latestPowerPercent !== null ? u.latestPowerPercent + '%' : 'no reading yet'}`);
    return `fleet.power = [${parts.join(', ')}]`;
  }
  const targetUnitId = parsed.scope === 'bare' ? deps.selectedUnitId() : parsed.scope.unit;
  const unit = await resolveUnit(targetUnitId, deps);
  if (!unit) {
    return `u${targetUnitId} is not a real unit in this fleet.`;
  }
  return `power (u${unit.id}/${unit.code}) = ${unit.latestPowerPercent !== null ? unit.latestPowerPercent + '%' : 'no reading yet'}`;
}

async function evaluateAggregate(parsed: Extract<ParsedCommand, { kind: 'aggregate' }>, deps: EvaluatorDeps): Promise<string> {
  const signal = findSignal(parsed.signal);
  if (!signal) {
    return `unknown identifier '${parsed.signal}' -- not part of this console's signal vocabulary.`;
  }
  if (!signal.real) {
    return absenceRefusal(signal);
  }
  if (signal.tier === 'kinetics') {
    return `${signal.name}: point-kinetics signals are per-unit only -- there is no fleet-wide array to aggregate.`;
  }

  const units = await deps.fetchFleetUnits();
  const values = units.map((u) => u.latestPowerPercent).filter((v): v is number => v !== null);
  if (values.length === 0) {
    return `${parsed.fn}(fleet.${signal.name}): no units currently report a reading to aggregate.`;
  }

  let result: number;
  switch (parsed.fn) {
    case 'sum':
      result = values.reduce((a, b) => a + b, 0);
      break;
    case 'mean':
      result = values.reduce((a, b) => a + b, 0) / values.length;
      break;
    case 'max':
      result = Math.max(...values);
      break;
    case 'min':
      result = Math.min(...values);
      break;
  }
  return `${parsed.fn}(fleet.${signal.name}) = ${result.toFixed(1)}% (from ${values.length} of ${units.length} units reporting)`;
}

export async function evaluateCommand(parsed: ParsedCommand, deps: EvaluatorDeps): Promise<string> {
  switch (parsed.kind) {
    case 'get':
      return evaluateGet(parsed, deps);
    case 'aggregate':
      return evaluateAggregate(parsed, deps);
    case 'select': {
      const unit = await resolveUnit(parsed.unit, deps);
      if (!unit) {
        return `u${parsed.unit} is not a real unit in this fleet.`;
      }
      deps.selectUnit(unit.id);
      return `selected u${unit.id} (${unit.code}).`;
    }
    case 'verb':
      return verbRefusal(parsed.verb);
    case 'error':
      return parsed.message;
    case 'unknown':
      return `unrecognized command: '${parsed.raw}'`;
  }
}
