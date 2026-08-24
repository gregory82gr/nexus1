import { aggregateRoster } from './personnel-aggregation';
import { DepartmentRosterEntry } from '../../core/api/organization-api';

function entry(overrides: Partial<DepartmentRosterEntry> = {}): DepartmentRosterEntry {
  return {
    personId: 1,
    displayName: 'Someone Real',
    personnelNumber: null,
    positionTitle: 'Reactor Operator',
    isSafetyCriticalPosition: true,
    applicationUserId: null,
    startDate: '2026-01-01',
    isPrimary: true,
    ...overrides,
  };
}

describe('aggregateRoster', () => {
  it('counts total and safety-critical roster size, from real entries', () => {
    const summary = aggregateRoster([
      entry({ personId: 1, isSafetyCriticalPosition: true }),
      entry({ personId: 2, isSafetyCriticalPosition: false }),
    ]);
    expect(summary.totalCount).toBe(2);
    expect(summary.safetyCriticalCount).toBe(1);
  });

  it('groups by position title, never by name -- the roster entry itself is not retained', () => {
    const summary = aggregateRoster([
      entry({ personId: 1, displayName: 'Alex Rivera', positionTitle: 'Reactor Operator' }),
      entry({ personId: 2, displayName: 'Jordan Chen', positionTitle: 'Shift Supervisor' }),
      entry({ personId: 3, displayName: 'Sam Okafor', positionTitle: 'Reactor Operator' }),
    ]);
    expect(summary.positions).toEqual([
      { positionTitle: 'Reactor Operator', count: 2, anySafetyCritical: true },
      { positionTitle: 'Shift Supervisor', count: 1, anySafetyCritical: true },
    ]);
    // No property on any returned object carries a name or person id.
    const serialized = JSON.stringify(summary);
    expect(serialized).not.toContain('Alex Rivera');
    expect(serialized).not.toContain('Jordan Chen');
  });

  it('falls back to an honest label, not a blank, for a null position title', () => {
    const summary = aggregateRoster([entry({ positionTitle: null })]);
    expect(summary.positions[0].positionTitle).toBe('Unspecified position');
  });

  it('returns an empty summary for an empty roster', () => {
    const summary = aggregateRoster([]);
    expect(summary).toEqual({ totalCount: 0, safetyCriticalCount: 0, positions: [] });
  });
});
