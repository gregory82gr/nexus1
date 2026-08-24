import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  OnDestroy,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DigitalTwinApi, UnitTwinState } from '../../core/api/digital-twin-api';
import { PlantStateService } from '../../core/state/plant-state';
import { TwinScene } from './twin-scene';
import { StatusTone, TONE_COLOR, fidelityBandIndex, fidelityOpacity, statusTone } from './twin-visual';

// Plant 3D View (Ch. 8), wired to Nexus1.Bff's real
// GET /api/v1/digital-twin/units/{id}.
//
// SCOPE CHANGE FROM THE BOOK, decided explicitly rather than guessed:
// Ch. 8's own Plant 3D View is a FLEET-WIDE scene (every unit rendered
// together) driven by four derived facts -- unit count, each unit's
// reactor class (used to compute steam-generator count), each unit's
// online/offline state, and its power output. None of those four exist
// in the real domain: ReactorFleet's Unit aggregate is bare Code/Name
// (ADR-003, no class, no online flag), and the real digital-twin
// endpoint is per-unit only -- there is no fleet-wide HTTP route
// (GetActiveTwinsForFleetQuery exists in the Application layer but is
// never mapped in Program.cs). Forcing the book's Figure 8.1 onto this
// data would mean inventing a reactor class and an online flag that
// don't exist anywhere real.
//
// So this screen visualizes what the endpoint actually answers -- the
// real per-unit twin BINDING, not plant geometry: ModelType, Status,
// Fidelity, and IsAuthoritative for the unit's active twin model. The
// abstract object's color follows Status (see twin-visual.ts's own
// keyword mapping, deliberately conservative -- unrecognized text renders
// neutral, never guessed into ok/warn/crit) and its opacity follows the
// declared Fidelity band (illustrative..validated, per
// TwinFidelityLevel.cs's own doc comment). Divergence/sync-drift data and
// the book's physical plant layout are both named as absent in the
// template, not silently dropped.
//
// The three.js/Angular ownership boundary itself IS ported faithfully:
// Angular owns the route, the toolbar, one empty <div #stage>, and
// ngOnDestroy; TwinScene (twin-scene.ts) owns the canvas, scene graph,
// and render loop, and disposes everything by hand on destroy -- lazy
// routes really do destroy this component, unlike the source file's
// hide-only pages.
//
// The book wraps scene creation in NgZone.runOutsideAngular() so the
// 60fps loop doesn't trigger a change-detection pass. This app is
// genuinely zoneless (Ch. 2) -- there is no zone for the render loop to
// escape, and the loop never touches a signal, so it can't trigger CD
// regardless. Omitted as genuinely unnecessary, not as an oversight.
//
// One more real-shape correction, caught by reading the endpoint's own
// prior live evidence before wiring this client rather than after: the
// endpoint returns an ARRAY (a unit can have more than one active twin;
// IsAuthoritative marks the live one) and always answers HTTP 200 -- an
// empty array means "no twin modeled for this unit," already documented
// by that evidence as "not an error," not a 404. So `twin: null` here is
// a genuine loaded state (no fault, nothing to show), distinct from
// `status: 'error'` (the endpoint itself was unreachable).
type TwinState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; twin: UnitTwinState | null };

@Component({
  selector: 'nx-plant-3d',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './plant-3d.html',
  styleUrl: './plant-3d.scss',
})
export class Plant3dComponent implements AfterViewInit, OnDestroy {
  private readonly api = inject(DigitalTwinApi);
  private readonly plantState = inject(PlantStateService);
  private readonly destroyRef = inject(DestroyRef);

  private scene?: TwinScene; // the non-Angular half -- see twin-scene.ts

  readonly stage = viewChild.required<ElementRef<HTMLDivElement>>('stage');
  readonly unavailable = signal(false);
  readonly unitId = this.plantState.selectedId;
  readonly state = signal<TwinState>({ status: 'loading' });

  constructor() {
    this.api
      .getUnitTwinStates(this.unitId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (twins) => {
          // The domain allows more than one active twin per unit;
          // IsAuthoritative is what it uses to say which one is live.
          // Falling back to the first entry when none is flagged
          // authoritative is a deliberate, named choice, not an
          // oversight -- the real endpoint has never yet returned more
          // than one row for a unit, so this branch is unexercised by
          // live data, only by the Jest spec that constructs it.
          const twin = twins.find((t) => t.isAuthoritative) ?? twins[0] ?? null;
          this.state.set({ status: 'loaded', twin });
          this.applyToScene(twin);
        },
        error: () =>
          this.state.set({
            status: 'error',
            message: 'The digital-twin endpoint is unreachable.',
          }),
      });
  }

  async ngAfterViewInit(): Promise<void> {
    let three: typeof import('three');
    try {
      three = await import('three');
    } catch {
      this.unavailable.set(true);
      return;
    }

    try {
      this.scene = new TwinScene(three, this.stage().nativeElement);
      this.scene.start();
      const current = this.state();
      if (current.status === 'loaded') {
        this.applyToScene(current.twin);
      }
    } catch {
      // Real, not hypothetical: a browser/environment without WebGL
      // support throws here (WebGLRenderer's context creation), not on
      // import -- e.g. jsdom's own test environment has no WebGL at all.
      this.unavailable.set(true);
      this.scene = undefined;
    }
  }

  ngOnDestroy(): void {
    this.scene?.destroy();
    this.scene = undefined;
  }

  private applyToScene(twin: UnitTwinState | null): void {
    if (twin === null) {
      this.scene?.setState(null);
      return;
    }
    const tone = statusTone(twin.status);
    const band = fidelityBandIndex(twin.fidelity);
    this.scene?.setState({ color: TONE_COLOR[tone], opacity: fidelityOpacity(band) });
  }

  // Narrowing helpers -- Angular's template compiler doesn't narrow a
  // discriminated union across repeated state() calls inside @switch/@case.
  get errorMessage(): string {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  }
  get loadedTwin(): UnitTwinState | null {
    const s = this.state();
    return s.status === 'loaded' ? s.twin : null;
  }
  get tone(): StatusTone | null {
    const t = this.loadedTwin;
    return t ? statusTone(t.status) : null;
  }
  get fidelityBand(): number | null {
    const t = this.loadedTwin;
    return t ? fidelityBandIndex(t.fidelity) : null;
  }
}
