import { findPowerSignal } from './power-signal';
import { UnitSignalReading } from '../../core/api/instrumentation-api';

function reading(tag: string, categoryCode: string, latestValue: number | null = 1): UnitSignalReading {
  return { tag, name: tag, categoryCode, latestValue, latestQualityCode: 'GOOD', latestTimestampUtc: '2026-08-24T00:00:00Z' };
}

describe('findPowerSignal', () => {
  it('picks a NEUTRONICS-category signal as the power proxy when nothing is explicitly labeled POWER', () => {
    const signal = findPowerSignal([reading('VIB-1', 'VIBRATION'), reading('NI-001', 'NEUTRONICS')]);
    expect(signal?.tag).toBe('NI-001');
  });

  it('prefers whichever power-like signal appears first, without guessing from the Tag text', () => {
    const signal = findPowerSignal([reading('PWR-1', 'POWER')]);
    expect(signal?.tag).toBe('PWR-1');
  });

  it('skips a power-like signal with no real reading', () => {
    const signal = findPowerSignal([reading('NI-002', 'NEUTRONICS', null), reading('NI-003', 'NEUTRONICS', 50)]);
    expect(signal?.tag).toBe('NI-003');
  });

  it('returns null, not a guess, when nothing power-like is present', () => {
    expect(findPowerSignal([reading('VIB-1', 'VIBRATION')])).toBeNull();
  });
});
