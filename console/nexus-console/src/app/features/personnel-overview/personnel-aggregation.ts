import { DepartmentRosterEntry } from '../../core/api/organization-api';

// Ch. 17's own central argument, applied here: the operational question
// ("does each sector meet its minimum complement, is every critical role
// covered?") needs counts and role coverage, not names. The real roster
// DTO carries DisplayName, PersonId, ApplicationUserId, PersonnelNumber,
// StartDate -- this deliberately drops every one of them. Only position
// title and count survive into what the screen renders, even though the
// raw data has far more identifying detail available. This is not a data
// gap being worked around; it's the book's own minimization principle
// applied to real data that happens to support more than it should show.
export interface PositionAggregate {
  positionTitle: string;
  count: number;
  anySafetyCritical: boolean;
}

export interface RosterSummary {
  totalCount: number;
  safetyCriticalCount: number;
  positions: PositionAggregate[];
}

export function aggregateRoster(entries: readonly DepartmentRosterEntry[]): RosterSummary {
  const totalCount = entries.length;
  const safetyCriticalCount = entries.filter((e) => e.isSafetyCriticalPosition === true).length;

  const byTitle = new Map<string, { count: number; anySafetyCritical: boolean }>();
  for (const entry of entries) {
    const title = entry.positionTitle ?? 'Unspecified position';
    const existing = byTitle.get(title);
    if (existing) {
      existing.count += 1;
      existing.anySafetyCritical = existing.anySafetyCritical || entry.isSafetyCriticalPosition === true;
    } else {
      byTitle.set(title, { count: 1, anySafetyCritical: entry.isSafetyCriticalPosition === true });
    }
  }

  const positions: PositionAggregate[] = Array.from(byTitle.entries())
    .map(([positionTitle, v]) => ({ positionTitle, count: v.count, anySafetyCritical: v.anySafetyCritical }))
    .sort((a, b) => a.positionTitle.localeCompare(b.positionTitle));

  return { totalCount, safetyCriticalCount, positions };
}
