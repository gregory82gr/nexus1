import { ActiveAlarm } from '../../core/api/alarm-management-api';

// Pure grouping over the one real fleet-wide alarm list, by the real
// Severity field -- a free-text lookup-table code (this codebase seeds
// different subsets per context), never assumed to be a fixed enum with a
// known priority order. Same "group by whatever the data actually
// reports, sorted alphabetically, never a hardcoded rank" discipline as
// zone-grouping.ts's own groupByClassification.
export interface AlarmGroup {
  severity: string;
  alarms: ActiveAlarm[];
}

export function groupBySeverity(alarms: readonly ActiveAlarm[]): AlarmGroup[] {
  const bySeverity = new Map<string, ActiveAlarm[]>();
  for (const alarm of alarms) {
    const group = bySeverity.get(alarm.severity);
    if (group) {
      group.push(alarm);
    } else {
      bySeverity.set(alarm.severity, [alarm]);
    }
  }
  return Array.from(bySeverity.entries())
    .map(([severity, groupAlarms]) => ({ severity, alarms: groupAlarms }))
    .sort((a, b) => a.severity.localeCompare(b.severity));
}
