import { UnitSignalReading } from '../../core/api/instrumentation-api';

// Pure grouping over the one real signal list -- by the real CategoryCode
// field, never by the book's six subsystem names, since no CategoryCode
// value resembling "CORE"/"RODS"/"COOLANT"/"STEAM" exists anywhere in the
// real domain (checked directly before writing this: the only seeded
// category values found in this codebase are generic measurement types
// like POWER, VIBRATION, NEUTRONICS). Grouping by whatever category the
// data actually reports is honest; grouping by the screen's own name
// would mean inventing a mapping the backend doesn't have.
export interface SignalGroup {
  categoryCode: string;
  signals: UnitSignalReading[];
}

export function groupByCategory(signals: readonly UnitSignalReading[]): SignalGroup[] {
  const byCategory = new Map<string, UnitSignalReading[]>();
  for (const signal of signals) {
    const group = byCategory.get(signal.categoryCode);
    if (group) {
      group.push(signal);
    } else {
      byCategory.set(signal.categoryCode, [signal]);
    }
  }
  return Array.from(byCategory.entries())
    .map(([categoryCode, groupSignals]) => ({ categoryCode, signals: groupSignals }))
    .sort((a, b) => a.categoryCode.localeCompare(b.categoryCode));
}
