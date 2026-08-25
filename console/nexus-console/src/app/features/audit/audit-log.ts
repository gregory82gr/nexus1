import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuditApi, ComplianceReview } from '../../core/api/audit-api';
import { ChainEntry, chainEntries, verifyChain } from './hash-chain';

// Audit & Compliance (Ch. 30) -- the console's most specific integrity
// claim, tested against its own source. The book's fictional screen
// claims "append-only, hash-chained ... each seal references the
// previous," but its seal function is two calls to Math.random(),
// formatted to look like a truncated hash -- it references nothing.
//
// Checked this backend directly before assuming any real chain exists:
// no hash-chain, seal, or tamper-evidence mechanism exists anywhere,
// server-side, in any context. Real SHA-256 hashing does exist --
// AuditEvidenceRecord.EnvelopeSha256Hex, a genuine content hash computed
// once per record -- but every one is isolated; nothing references a
// PREVIOUS record's hash. There is no chain to expose, only real,
// individually-hashed evidence records to chain FOR THE FIRST TIME,
// client-side -- see hash-chain.ts's own doc comment for why that is
// still honest and how it's labeled.
//
// SCOPING, real and structural, not a screen-design choice: both real
// endpoints (Audit evidence, Compliance review) are keyed by an opaque
// RootCause analysis id -- RootCause stays out-of-process (ADR-001), and
// neither context has a UnitId or a fleet-wide listing anywhere. There
// is no "give me everything" endpoint to page through (confirmed: the
// unique index on SourceAnalysisId means at most one evidence record and
// one compliance review exist per analysis id). So this screen uses the
// same manually-keyed lookup pattern as Mission Readiness (Ch. 19):
// looking up an analysis id fetches its one real evidence record (if
// any) and its one real compliance review (if any), and each newly
// found evidence record is appended to a growing, session-local list
// that the hash chain is computed and re-verified over -- a real,
// multi-entry chain built entirely from real per-analysis lookups, not
// a fabricated fleet-wide feed the backend doesn't have.
//
// NO ACTOR/ACTION COLUMN: checked directly (see the investigation this
// slice's own evidence report carries) -- no context in this solution
// exposes a real "who did this, what did they do" record reachable
// today. The closest real candidates (AlarmManagement's acknowledgment
// fields, Security's role/permission grant fields, EventManagement's
// timeline-entry actor field) either have no read path at all or drop
// the actor field at the DTO/projection level. Omitted entirely rather
// than filled with an invented name.
//
// The chain's own label never says "tamper-proof" -- only "verifies
// locally, not anchored." A party who controls this browser session
// also controls the only copy of the chain computed here; it can prove
// the displayed list is internally self-consistent, never that nothing
// was altered before this session saw it. A real anchored trail needs
// server-side seals, which this backend has no endpoint for -- out of
// scope, the same boundary the book itself names.
type LookupState = { status: 'idle' } | { status: 'loading' } | { status: 'error'; message: string };

@Component({
  selector: 'nx-audit-log',
  standalone: true,
  templateUrl: './audit-log.html',
  styleUrl: './audit-log.scss',
})
export class AuditLogComponent {
  private readonly api = inject(AuditApi);
  private readonly destroyRef = inject(DestroyRef);

  // Pre-filled with a known real analysis id from live evidence sessions
  // (not a placeholder guess) so the screen shows real chained data
  // without requiring the operator to already know an id on first load.
  readonly lookupAnalysisId = signal('639224028038165230');
  readonly lookupState = signal<LookupState>({ status: 'idle' });
  readonly complianceReviews = signal<readonly ComplianceReview[]>([]);
  readonly lastLookedUpId = signal<string | null>(null);
  readonly chain = signal<readonly ChainEntry[]>([]);
  readonly verification = signal<{ ok: boolean; brokenAt: number | null } | null>(null);

  private readonly seenAnalysisIds = new Set<string>();

  onLookupIdInput(value: string): void {
    this.lookupAnalysisId.set(value);
  }

  // Two independent requests, not a single combined one -- each has its
  // own real failure mode (Audit and Compliance are separate physical
  // databases/contexts), and this project's own established pattern
  // keeps independent real calls independent rather than coupling their
  // success/failure through an operator like forkJoin.
  lookup(): void {
    const id = this.lookupAnalysisId().trim();
    if (!id) return;

    this.lookupState.set({ status: 'loading' });
    this.lastLookedUpId.set(id);

    this.api
      .getComplianceReviews(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (reviews) => this.complianceReviews.set(reviews),
        error: () =>
          this.lookupState.set({
            status: 'error',
            message: 'The Compliance review endpoint is unreachable.',
          }),
      });

    this.api
      .getEvidence(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (evidence) => {
          this.lookupState.set({ status: 'idle' });
          if (evidence.length > 0 && !this.seenAnalysisIds.has(id)) {
            this.seenAnalysisIds.add(id);
            const raw = [
              ...this.chain().map((c) => ({ analysisId: c.analysisId, envelopeSha256Hex: c.envelopeSha256Hex, recordedAtUtc: c.recordedAtUtc })),
              ...evidence.map((e) => ({ analysisId: id, envelopeSha256Hex: e.envelopeSha256Hex, recordedAtUtc: e.recordedAtUtc })),
            ];
            chainEntries(raw).then((chained) => {
              this.chain.set(chained);
              verifyChain(chained).then((v) => this.verification.set(v));
            });
          }
        },
        error: () =>
          this.lookupState.set({
            status: 'error',
            message: 'The Audit evidence endpoint is unreachable.',
          }),
      });
  }

  get errorMessage(): string {
    const s = this.lookupState();
    return s.status === 'error' ? s.message : '';
  }
  get chainLabel(): string {
    const v = this.verification();
    if (!v) return '';
    return v.ok ? 'chain verifies locally, not anchored' : `chain broken — see entry ${v.brokenAt}`;
  }
}
