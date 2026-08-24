import { groupByCategory } from './asset-grouping';
import { UnitAssetCondition } from '../../core/api/maintenance-api';

function asset(assetCode: string, category: string): UnitAssetCondition {
  return {
    assetCode,
    name: assetCode,
    category,
    status: 'IN_SERVICE',
    isSafetyRelated: false,
    latestAssessedAtUtc: null,
    latestConditionGrade: null,
    latestHealthScorePercent: null,
    latestRemainingUsefulLifeDays: null,
  };
}

describe('groupByCategory', () => {
  it('groups assets under their real Category, not an invented rod-type or NDT taxonomy', () => {
    const groups = groupByCategory([asset('A', 'MECHANICAL'), asset('B', 'ELECTRICAL'), asset('C', 'MECHANICAL')]);
    expect(groups).toHaveLength(2);
    const mechanical = groups.find((g) => g.category === 'MECHANICAL');
    expect(mechanical?.assets.map((a) => a.assetCode)).toEqual(['A', 'C']);
  });

  it('returns groups sorted alphabetically, deterministic regardless of input order', () => {
    const groups = groupByCategory([asset('A', 'MECHANICAL'), asset('B', 'ELECTRICAL')]);
    expect(groups.map((g) => g.category)).toEqual(['ELECTRICAL', 'MECHANICAL']);
  });

  it('returns an empty array for an empty asset list', () => {
    expect(groupByCategory([])).toEqual([]);
  });
});
