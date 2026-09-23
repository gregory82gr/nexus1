import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FIXED_INCIDENT_ID, RootCauseService } from '../root-cause/root-cause.service';

// Incident Analysis (Ch. 29) -- the console's simplest root-cause view. Per
// Ch.29's own correction, it authors NO hypothesis list of its own and makes no
// "confirmed by an engineer" claim (a provenance this build cannot support).
// It reads the SAME RootCauseService the Root Cause screen reads, so the two can
// never disagree about the same incident -- there is exactly one place the answer
// is produced. Because these screens are new (Ch.29 was previously deferred),
// this correctness is inherited by construction: there was never a local CAND
// fixture here to retire.
@Component({
  selector: 'nx-incident-summary',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './incident-summary.html',
  styleUrl: './incident-summary.scss',
})
export class IncidentSummaryComponent {
  private readonly rootCause = inject(RootCauseService);

  readonly incidentId = FIXED_INCIDENT_ID;
  readonly state = this.rootCause.state;
  readonly topCause = this.rootCause.topCause;
  readonly seeFullGraphLink = '/rcgraph';

  readonly topPct = computed(() => {
    const cause = this.topCause();
    return cause ? `${Math.round(cause.weight * 100)}%` : '';
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
}
