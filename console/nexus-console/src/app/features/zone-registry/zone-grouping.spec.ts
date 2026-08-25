import { groupByClassification } from './zone-grouping';
import { ActiveRadiationZone } from '../../core/api/radiation-zones-api';

function zone(code: string, classification: string): ActiveRadiationZone {
  return { code, name: code, unitCode: null, classification, status: 'POSTED' };
}

describe('groupByClassification', () => {
  it('groups zones under their real Classification, not an invented entity-class taxonomy', () => {
    const groups = groupByClassification([zone('A', 'LOW'), zone('B', 'HIGH'), zone('C', 'LOW')]);
    expect(groups).toHaveLength(2);
    const low = groups.find((g) => g.classification === 'LOW');
    expect(low?.zones.map((z) => z.code)).toEqual(['A', 'C']);
  });

  it('returns groups sorted alphabetically, deterministic regardless of input order', () => {
    const groups = groupByClassification([zone('A', 'LOW'), zone('B', 'HIGH')]);
    expect(groups.map((g) => g.classification)).toEqual(['HIGH', 'LOW']);
  });

  it('returns an empty array for an empty zone list', () => {
    expect(groupByClassification([])).toEqual([]);
  });
});
