import { Component, DestroyRef, Input, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActiveRadiationZone, RadiationZonesApi } from '../../core/api/radiation-zones-api';
import { ZoneGroup, groupByClassification } from './zone-grouping';

// Zone Access (Ch. 20) -- neither of the book's own two screens has any
// real backing anywhere in this solution, checked directly rather than
// assumed. Volume III itself has none either ("Volume III has no
// access-control endpoint; the tags, zones, and movements are
// generated"), but that alone wouldn't be new -- three prior clusters
// (Rod Inspection, Plant Lifecycle, Robotics) found the same about the
// book's own source and still had at least one real screen to build.
// This is the first cluster where BOTH book screens come up empty:
//
//  - Permissions Matrix (which entity CLASS may enter which zone): no
//    class-to-zone authorization mapping exists anywhere -- not in
//    Security (confirmed RBAC-only: roles/permissions/lock/preferences,
//    the already-known finding), not anywhere else. Checked across
//    every context's domain layer, not assumed from Security alone.
//  - Live Presence (named people, real-time zone, a violation flag, and
//    Part IV's first write -- acknowledging an access alarm): no
//    presence/badge/entry-log concept exists anywhere either. A
//    solution-wide search for anything resembling it came back empty.
//
// What DOES exist, and what this screen is built around instead:
// RadiationMonitoring.RadiationZone -- a genuine physical-zone registry
// (code, name, classification, status, optional home unit), fleet-wide,
// via a real Application-layer query
// (GetActiveRadiationZonesQuery/ActiveRadiationZoneDto) that already
// existed and was already registered, just never mapped to a BFF route
// until this slice. It is not a permissions matrix (no class/role is
// attached to a zone here) and it is not a presence view (no person or
// location is attached to a zone here either) -- it is a real zone
// catalog, named honestly as one. Both original nav entries
// (`access-presence`, `access-matrix`) point at this one component,
// the same consolidation precedent as the Reactor cluster: distinct
// nav labels over one real, honestly-labeled data source, rather than
// fabricating two different screens neither of which has anything real
// to show.
type ZonesState = { status: 'loading' } | { status: 'error'; message: string } | { status: 'loaded'; groups: ZoneGroup[]; total: number };

@Component({
  selector: 'nx-zone-registry',
  standalone: true,
  templateUrl: './zone-registry.html',
  styleUrl: './zone-registry.scss',
})
export class ZoneRegistryComponent {
  private readonly api = inject(RadiationZonesApi);
  private readonly destroyRef = inject(DestroyRef);

  @Input() focusLabel = 'Zone Access';

  readonly state = signal<ZonesState>({ status: 'loading' });

  constructor() {
    this.api
      .getActiveZones()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (zones: ActiveRadiationZone[]) => this.state.set({ status: 'loaded', groups: groupByClassification(zones), total: zones.length }),
        error: () =>
          this.state.set({
            status: 'error',
            message: 'The RadiationMonitoring zones endpoint is unreachable.',
          }),
      });
  }

  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get loadedGroups(): ZoneGroup[] {
    const s = this.state();
    return s.status === 'loaded' ? s.groups : [];
  }
  get totalCount(): number {
    const s = this.state();
    return s.status === 'loaded' ? s.total : 0;
  }
}
