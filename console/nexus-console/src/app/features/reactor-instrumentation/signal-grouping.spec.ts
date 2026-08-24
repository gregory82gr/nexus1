import { groupByCategory } from './signal-grouping';
import { UnitSignalReading } from '../../core/api/instrumentation-api';

function reading(tag: string, categoryCode: string): UnitSignalReading {
  return { tag, name: tag, categoryCode, latestValue: 1, latestQualityCode: 'GOOD', latestTimestampUtc: '2026-08-24T00:00:00Z' };
}

describe('groupByCategory', () => {
  it('groups signals under their real CategoryCode, not an invented subsystem name', () => {
    const groups = groupByCategory([reading('A', 'NEUTRONICS'), reading('B', 'POWER'), reading('C', 'NEUTRONICS')]);
    expect(groups).toHaveLength(2);
    const neutronics = groups.find((g) => g.categoryCode === 'NEUTRONICS');
    expect(neutronics?.signals.map((s) => s.tag)).toEqual(['A', 'C']);
  });

  it('returns groups sorted alphabetically by category, deterministic regardless of input order', () => {
    const groups = groupByCategory([reading('A', 'VIBRATION'), reading('B', 'NEUTRONICS')]);
    expect(groups.map((g) => g.categoryCode)).toEqual(['NEUTRONICS', 'VIBRATION']);
  });

  it('returns an empty array for an empty signal list', () => {
    expect(groupByCategory([])).toEqual([]);
  });
});
