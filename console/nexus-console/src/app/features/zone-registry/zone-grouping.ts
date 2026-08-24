import { ActiveRadiationZone } from '../../core/api/radiation-zones-api';

// Pure grouping over the one real zone list -- by the real Classification
// field, never by an invented "entity class" (there is no class-to-zone
// authorization concept anywhere in this domain to group by instead).
export interface ZoneGroup {
  classification: string;
  zones: ActiveRadiationZone[];
}

export function groupByClassification(zones: readonly ActiveRadiationZone[]): ZoneGroup[] {
  const byClassification = new Map<string, ActiveRadiationZone[]>();
  for (const zone of zones) {
    const group = byClassification.get(zone.classification);
    if (group) {
      group.push(zone);
    } else {
      byClassification.set(zone.classification, [zone]);
    }
  }
  return Array.from(byClassification.entries())
    .map(([classification, groupZones]) => ({ classification, zones: groupZones }))
    .sort((a, b) => a.classification.localeCompare(b.classification));
}
