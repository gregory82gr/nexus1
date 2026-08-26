import { UnitSignalReading } from '../../core/api/instrumentation-api';
import { UnitSummary } from '../../core/api/reactor-fleet-api';
import { TimedReading } from '../../core/physics/point-kinetics';
import { evaluateCommand, EvaluatorDeps } from './command-evaluator';
import { parseCommand } from './command-parser';
import { SIGNAL_CATALOG } from './signal-catalog';

function makeDeps(overrides: Partial<EvaluatorDeps> & { units?: UnitSummary[]; signals?: Record<number, UnitSignalReading[]> } = {}): EvaluatorDeps {
  const units: UnitSummary[] = overrides.units ?? [{ id: 1, code: 'NX1-U1', name: 'Unit 1', latestPowerPercent: 100.1, latestPowerRecordedAtUtc: '2026-08-26T00:00:00Z' }];
  const signalsByUnit = overrides.signals ?? {
    1: [{ tag: 'NX1-U1.RX.POWER', name: 'Reactor Power', categoryCode: 'POWER', latestValue: 100.1, latestQualityCode: 'GOOD', latestTimestampUtc: '2026-08-26T00:00:00Z' }],
  };
  let selected = 1;
  return {
    fetchFleetUnits: async () => units,
    fetchUnitSignals: async (unitId: number) => signalsByUnit[unitId] ?? [],
    selectedUnitId: () => selected,
    selectUnit: (id: number) => {
      selected = id;
    },
    lastKineticsReading: new Map<number, TimedReading>(),
    ...overrides,
  };
}

describe('evaluateCommand', () => {
  it('returns the real per-unit power value for a bare get', async () => {
    const result = await evaluateCommand(parseCommand('get power'), makeDeps());
    expect(result).toContain('100.1%');
  });

  it('returns the real fleet-wide array for get fleet.power', async () => {
    const deps = makeDeps({
      units: [
        { id: 1, code: 'NX1-U1', name: 'Unit 1', latestPowerPercent: 100.1, latestPowerRecordedAtUtc: null },
        { id: 2, code: 'NX1-U2', name: 'Unit 2', latestPowerPercent: null, latestPowerRecordedAtUtc: null },
      ],
    });
    const result = await evaluateCommand(parseCommand('get fleet.power'), deps);
    expect(result).toContain('NX1-U1: 100.1%');
    expect(result).toContain('NX1-U2: no reading yet');
  });

  it('computes a real aggregate over fleet.power, excluding units with no reading', async () => {
    const deps = makeDeps({
      units: [
        { id: 1, code: 'NX1-U1', name: 'Unit 1', latestPowerPercent: 100, latestPowerRecordedAtUtc: null },
        { id: 2, code: 'NX1-U2', name: 'Unit 2', latestPowerPercent: 50, latestPowerRecordedAtUtc: null },
        { id: 3, code: 'NX1-U3', name: 'Unit 3', latestPowerPercent: null, latestPowerRecordedAtUtc: null },
      ],
    });
    const result = await evaluateCommand(parseCommand('mean(fleet.power)'), deps);
    expect(result).toContain('75.0%');
    expect(result).toContain('from 2 of 3 units reporting');
  });

  it('refuses aggregation over a point-kinetics signal, since there is no fleet-wide array for it', async () => {
    const result = await evaluateCommand(parseCommand('sum(fleet.period)'), makeDeps());
    expect(result).toMatch(/per-unit only/);
  });

  it('returns kin_power as the real live Instrumentation reading for the selected unit', async () => {
    const result = await evaluateCommand(parseCommand('get kin_power'), makeDeps());
    expect(result).toContain('100.1');
    expect(result).toContain('NX1-U1.RX.POWER');
  });

  it('reports insufficient data on the first period reading, then derives a real rate on the second', async () => {
    const deps = makeDeps({
      signals: { 1: [{ tag: 'T', name: 'T', categoryCode: 'POWER', latestValue: 100, latestQualityCode: 'GOOD', latestTimestampUtc: '2026-08-26T00:00:00Z' }] },
    });
    const first = await evaluateCommand(parseCommand('get period'), deps);
    expect(first).toMatch(/only one reading observed/);

    deps.fetchUnitSignals = async () => [{ tag: 'T', name: 'T', categoryCode: 'POWER', latestValue: 200, latestQualityCode: 'GOOD', latestTimestampUtc: '2026-08-26T00:00:05Z' }];
    const second = await evaluateCommand(parseCommand('get period'), deps);
    expect(second).toMatch(/period \(u1\) = [+-]?\d+\.\d s/);
  });

  it('refuses a point-kinetics signal requested for a unit other than the selected one', async () => {
    const deps = makeDeps({
      units: [
        { id: 1, code: 'NX1-U1', name: 'Unit 1', latestPowerPercent: 100, latestPowerRecordedAtUtc: null },
        { id: 2, code: 'NX1-U2', name: 'Unit 2', latestPowerPercent: 50, latestPowerRecordedAtUtc: null },
      ],
    });
    const result = await evaluateCommand(parseCommand('get u2.period'), deps);
    expect(result).toMatch(/only available for the currently selected unit \(u1\)/);
  });

  it('honestly refuses each of the 11 absent signals with its own specific reason, never a generic message', async () => {
    const absentSignals = SIGNAL_CATALOG.filter((s) => !s.real);
    expect(absentSignals).toHaveLength(11);
    const messages = new Set<string>();
    for (const signal of absentSignals) {
      const result = await evaluateCommand(parseCommand(`get ${signal.name}`), makeDeps());
      expect(result).toBe(`${signal.name}: ${signal.absenceReason}`);
      messages.add(result);
    }
    expect(messages.size).toBe(11);
  });

  it('writes to the real PlantStateService-backed select, validated against the real fleet list', async () => {
    const deps = makeDeps({
      units: [
        { id: 1, code: 'NX1-U1', name: 'Unit 1', latestPowerPercent: 100, latestPowerRecordedAtUtc: null },
        { id: 2, code: 'NX1-U2', name: 'Unit 2', latestPowerPercent: 50, latestPowerRecordedAtUtc: null },
      ],
    });
    const result = await evaluateCommand(parseCommand('select u2'), deps);
    expect(result).toContain('selected u2');
    expect(deps.selectedUnitId()).toBe(2);
  });

  it('refuses select for a unit that does not exist in the real fleet, rather than writing an invalid selection', async () => {
    const deps = makeDeps();
    const result = await evaluateCommand(parseCommand('select u99'), deps);
    expect(result).toMatch(/not a real unit/);
    expect(deps.selectedUnitId()).toBe(1);
  });

  it('refuses acknowledge with a message distinct from the design-only verb refusals', async () => {
    const ack = await evaluateCommand(parseCommand('acknowledge'), makeDeps());
    const scram = await evaluateCommand(parseCommand('scram'), makeDeps());
    expect(ack).toMatch(/real capability/);
    expect(scram).not.toMatch(/real capability/);
    expect(scram).toMatch(/not available in the read-only console/);
  });

  it('reports an unrecognized identifier distinctly from a recognized-but-absent one', async () => {
    const result = await evaluateCommand(parseCommand('get warp_factor'), makeDeps());
    expect(result).toMatch(/unknown identifier/);
  });
});
