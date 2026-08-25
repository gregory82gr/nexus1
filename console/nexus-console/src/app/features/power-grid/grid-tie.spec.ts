import { UnitSignalReading } from '../../core/api/instrumentation-api';
import { buildGridTie } from './grid-tie';

describe('buildGridTie', () => {
  const turbineSignal: UnitSignalReading = {
    tag: 'UNIT1-TURB-001',
    name: 'Main Turbine Shaft Speed',
    categoryCode: 'TURBINE',
    latestValue: 2998.7,
    latestQualityCode: 'GOOD',
    latestTimestampUtc: '2026-08-25T09:00:00Z',
  };

  it('reads turbineSpeedRpm from the real TURBINE-category signal when present', () => {
    const tie = buildGridTie([turbineSignal]);
    expect(tie.turbineSpeedRpm).toEqual({ source: 'measured', rpm: 2998.7, timestampUtc: '2026-08-25T09:00:00Z' });
  });

  it('reports no-signal when no TURBINE-category signal is in the list', () => {
    const tie = buildGridTie([{ ...turbineSignal, categoryCode: 'POWER' }]);
    expect(tie.turbineSpeedRpm).toEqual({ source: 'no-signal' });
  });

  it('reports no-signal when the turbine signal exists but has never recorded a value', () => {
    const tie = buildGridTie([{ ...turbineSignal, latestValue: null }]);
    expect(tie.turbineSpeedRpm).toEqual({ source: 'no-signal' });
  });

  it('keeps gridFrequencyHz, phaseAngleDeg, breakerClosed, and inSync structurally unconnected to turbineSpeedRpm', () => {
    const tie = buildGridTie([turbineSignal]);
    expect(tie.gridFrequencyHz).toEqual({ source: 'awaiting-telemetry' });
    expect(tie.phaseAngleDeg).toEqual({ source: 'no-source' });
    expect(tie.breakerClosed).toEqual({ source: 'no-source' });
    expect(tie.inSync).toEqual({ source: 'no-source' });
  });

  it('never changes gridFrequencyHz when turbineSpeedRpm changes, at any RPM value', () => {
    const low = buildGridTie([{ ...turbineSignal, latestValue: 2900 }]);
    const high = buildGridTie([{ ...turbineSignal, latestValue: 3100 }]);
    expect(low.gridFrequencyHz).toEqual(high.gridFrequencyHz);
  });
});
