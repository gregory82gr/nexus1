import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MaintenanceApi, UnitAssetCondition } from '../../core/api/maintenance-api';
import { PlantStateService } from '../../core/state/plant-state';
import { AssetGroup, groupByCategory } from './asset-grouping';

// Rod Inspection cluster (Ch. 16) -- Inspection Overview, NDT Methods, and
// Rod Type/Film, three of the book's screens, all reading Maintenance's
// real GET /api/v1/maintenance/units/{id}/assets. Investigated before
// building, not assumed: Maintenance's domain model has no rod-specific
// entity anywhere -- Asset/AssetCondition are entirely generic (any
// maintainable equipment item, generic category/status/grade lookups).
// NDT Methods and Rod Type/Film have nothing real to map to at all, not
// missing fields on an otherwise rod-shaped model (see
// UnitAssetConditionDto's own doc comment).
//
// A stronger reason than the Reactor cluster's own consolidation applies
// here: the book's OWN source material states outright that "Volume III
// has no inspection endpoint, no NDT results, no rod inventory, and no
// verdicts" -- its Rod Inspection module is entirely generated:
// synthetic radiograph pixels, synthetic indications placed by the
// console, and a verdict computed from those placed indications. This
// port has real backend data (unlike the book's own source), but not
// real inspection/radiograph data -- so building a convincing radiograph
// viewer or an acceptance verdict from asset-condition data would be
// exactly the fabrication Ch. 16 itself spends a full chapter warning
// against (the image "marker that must survive the screenshot," because
// a viewer who screenshots a fake film and shares it gets something
// indistinguishable from a real one). So this screen (serving the
// Inspection Overview route) shows the real generic asset/condition list,
// honestly grouped by the real Category field, and NO radiograph, NO NDT
// verdict, and NO acceptance decision is rendered anywhere -- there is
// nothing real to compute one from. Rod Type/Film is not built at all,
// named here as a real gap rather than a fabricated screen. NDT Methods
// gets its own separate, genuinely static reference screen
// (features/ndt-methods/) instead -- the book itself treats that table as
// authored reference content needing no provenance marker, the same class
// as Model Analysis's own model-constants panel, not something to
// consolidate into this live data view.
type AssetsState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; groups: AssetGroup[]; total: number };

@Component({
  selector: 'nx-asset-condition',
  standalone: true,
  templateUrl: './asset-condition.html',
  styleUrl: './asset-condition.scss',
})
export class AssetConditionComponent {
  private readonly api = inject(MaintenanceApi);
  private readonly plantState = inject(PlantStateService);
  private readonly destroyRef = inject(DestroyRef);

  readonly unitId = this.plantState.selectedId;
  readonly state = signal<AssetsState>({ status: 'loading' });

  readonly assessedCount = computed(() => {
    const s = this.state();
    if (s.status !== 'loaded') return 0;
    return s.groups.reduce((count, g) => count + g.assets.filter((a) => a.latestConditionGrade !== null).length, 0);
  });

  constructor() {
    this.api
      .getAssetConditions(this.unitId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (assets: UnitAssetCondition[]) =>
          this.state.set({ status: 'loaded', groups: groupByCategory(assets), total: assets.length }),
        error: () =>
          this.state.set({
            status: 'error',
            message: 'The Maintenance asset-conditions endpoint is unreachable.',
          }),
      });
  }

  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get loadedGroups(): AssetGroup[] {
    const s = this.state();
    return s.status === 'loaded' ? s.groups : [];
  }
  get totalCount(): number {
    const s = this.state();
    return s.status === 'loaded' ? s.total : 0;
  }
}
