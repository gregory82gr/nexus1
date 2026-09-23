import { Component, computed, inject } from '@angular/core';
import { FIXED_INCIDENT_ID, RootCauseService } from './root-cause.service';

// Root Cause (Ch. 29) -- the console's most rigorous screen: the engineered
// causal verdict for EVT-2026-0418, read from the real diagnosis pipeline
// (ADR-032..ADR-035), not a hand-authored fixture. This screen never computes a
// conclusion; it reads the one RootCauseService owns. Confidences are shown as
// illustrative. The fault-tree graph (Figure 29.1) is deliberately NOT drawn:
// the route returns the ranked verdict + citations, not the graph topology, and
// a hand-authored graph would be exactly the fabricated client-side data this
// project argues against -- so the gap is named, not faked.
@Component({
  selector: 'nx-root-cause',
  standalone: true,
  templateUrl: './root-cause.html',
  styleUrl: './root-cause.scss',
})
export class RootCauseComponent {
  private readonly rootCause = inject(RootCauseService);

  readonly incidentId = FIXED_INCIDENT_ID;
  readonly state = this.rootCause.state;
  readonly citations = this.rootCause.citations;
  readonly auditHash = this.rootCause.auditHash;

  readonly result = computed(() => {
    const s = this.state();
    return s.status === 'ready' ? s.result : null;
  });

  readonly abstainReason = computed(() => {
    const s = this.state();
    return s.status === 'abstained' ? s.reason : null;
  });

  readonly errorMessage = computed(() => {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  });

  constructor() {
    this.rootCause.load(FIXED_INCIDENT_ID);
  }

  pct(value: number | null): string {
    return value == null ? '—' : `${Math.round(value * 100)}%`;
  }
}
