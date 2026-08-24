import { Injectable, signal } from '@angular/core';

// Personnel (Ch. 17) is department-scoped, not unit-scoped -- there is no
// connection at all between ReactorFleet.Unit and Organization's
// hierarchy (ADR-017), so the topbar's existing unit selector doesn't
// apply here. A separate, minimal state service rather than reusing
// PlantStateService: conflating "selected unit" and "selected
// department" would be semantically wrong even though both are just an
// int today. Defaults to 1, the one real seeded department
// ("Operations Department") from the roster's own proven evidence.
@Injectable({ providedIn: 'root' })
export class DepartmentStateService {
  readonly selectedId = signal(1);

  select(departmentId: number): void {
    this.selectedId.set(departmentId);
  }
}
