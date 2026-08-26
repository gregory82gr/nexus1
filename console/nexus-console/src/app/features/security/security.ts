import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ContextHealthResult, SecurityApi } from '../../core/api/security-api';
import { StatusTone, statusTone } from './context-status';

// Security & Services (Ch. 31) -- the book's fictional screen has three
// panels. Network Zones is honest static documentation, kept as-is. The
// other two, Microservice Health (6 services, all green) and OT Security
// Posture (6 controls, all green under a "Zero-Trust" tag), have a
// different kind of finding from every prior chapter: not a wrong
// computation, no computation at all -- literal <span class="led ok">
// markup with no renderSecurity() anywhere.
//
// Checked directly before assuming the book's own honest fix (2 rows
// genuinely live from real telRate/twinLag signals, the other 4 "not
// monitored") carries over -- it does not, on both signals:
//
// 1. No telemetry ingestion RATE (messages/second) exists anywhere.
//    Real outbox metrics exist (pending count, oldest-message age) but
//    are counts/ages, not rates -- and even those aren't reachable from
//    this BFF at all (it registers neither AddNexusMessaging nor
//    AddNexusObservability, confirmed directly in Program.cs).
// 2. No digital-twin sync-lag or staleness field exists anywhere in
//    DigitalTwin's domain -- confirmed via IActiveTwinFinder's own
//    "four-hop join" doc comment (divergence data was found and
//    explicitly excluded from the one twin-state route that exists).
//
// So neither of the book's own "genuinely live" rows has real data here
// either -- reported to the user, who chose a genuinely different real
// thing over declaring total absence: this backend already has real,
// live DbContextHealthCheck<T> reachability checks per composed
// context, feeding /health/ready -- but only as an aggregate plain-text
// status, never a per-context breakdown. GET /health/contexts (new, this
// slice) is that breakdown: real per-context database CONNECTIVITY
// reachability, named "Context Health," never "Microservice Health" --
// deliberately not the book's own vocabulary, so a reader can never
// mistake a green light here for message-processing health, queue
// depth, or business-logic health, none of which this backend can
// compute for anything. The label on screen says this explicitly.
//
// OT Security Posture and Network Zones carry no backend call at all --
// checked directly (solution-wide grep for HSM/MFA/IDS/segmentation/
// zero-trust/audit-streaming): zero real backing for any of the 6 OT
// controls, exactly as the book itself found. Both render as static
// documentation, Network Zones' own already-honest format -- no LEDs,
// no "Zero-Trust" tag, a plain list of design facts.
type ContextHealthState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'loaded'; results: ContextHealthResult[] };

@Component({
  selector: 'nx-security',
  standalone: true,
  templateUrl: './security.html',
  styleUrl: './security.scss',
})
export class SecurityComponent {
  private readonly api = inject(SecurityApi);
  private readonly destroyRef = inject(DestroyRef);

  readonly contextHealthState = signal<ContextHealthState>({ status: 'loading' });

  constructor() {
    this.api
      .getContextHealth()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (results) => this.contextHealthState.set({ status: 'loaded', results }),
        error: () =>
          this.contextHealthState.set({
            status: 'error',
            message: 'The /health/contexts endpoint is unreachable.',
          }),
      });
  }

  get contextHealthErrorMessage(): string {
    const s = this.contextHealthState();
    return s.status === 'error' ? s.message : '';
  }
  get loadedContextHealth(): ContextHealthResult[] {
    const s = this.contextHealthState();
    return s.status === 'loaded' ? s.results : [];
  }

  toneOf(status: string): StatusTone {
    return statusTone(status);
  }
}
