import { UnitAssetCondition } from '../../core/api/maintenance-api';

// Pure grouping over the one real asset list -- by the real Category
// field (same discipline as reactor-instrumentation's own
// groupByCategory: group by what the data actually reports, never by an
// invented taxonomy like "rod type" or "NDT method," neither of which
// exists anywhere in this domain).
export interface AssetGroup {
  category: string;
  assets: UnitAssetCondition[];
}

export function groupByCategory(assets: readonly UnitAssetCondition[]): AssetGroup[] {
  const byCategory = new Map<string, UnitAssetCondition[]>();
  for (const asset of assets) {
    const group = byCategory.get(asset.category);
    if (group) {
      group.push(asset);
    } else {
      byCategory.set(asset.category, [asset]);
    }
  }
  return Array.from(byCategory.entries())
    .map(([category, groupAssets]) => ({ category, assets: groupAssets }))
    .sort((a, b) => a.category.localeCompare(b.category));
}
