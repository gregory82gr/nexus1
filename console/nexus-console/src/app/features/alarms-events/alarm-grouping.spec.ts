import { ActiveAlarm } from '../../core/api/alarm-management-api';
import { groupBySeverity } from './alarm-grouping';

describe('groupBySeverity', () => {
  const alarm = (severity: string, alarmEventId: number): ActiveAlarm => ({
    alarmEventId,
    unitId: 1,
    message: 'test alarm',
    severity,
    raisedAtUtc: '2026-08-25T09:00:00Z',
  });

  it('groups alarms by their real severity field', () => {
    const groups = groupBySeverity([alarm('Critical', 1), alarm('High', 2), alarm('Critical', 3)]);
    expect(groups).toHaveLength(2);
    expect(groups.find((g) => g.severity === 'Critical')?.alarms).toHaveLength(2);
    expect(groups.find((g) => g.severity === 'High')?.alarms).toHaveLength(1);
  });

  it('sorts groups alphabetically by severity text, never a hardcoded priority rank', () => {
    const groups = groupBySeverity([alarm('High', 1), alarm('Critical', 2)]);
    expect(groups.map((g) => g.severity)).toEqual(['Critical', 'High']);
  });

  it('returns an empty list for an empty alarm list', () => {
    expect(groupBySeverity([])).toEqual([]);
  });
});
