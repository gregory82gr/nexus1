import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuditLogComponent } from './audit-log';
import { AuditEvidenceRecord, ComplianceReview } from '../../core/api/audit-api';

// The component chains two sequential async Web Crypto digests
// (chainEntries then verifyChain) off the HTTP response. A single fixed
// timer proved flaky under full-suite load (real digest timing varies
// with CPU contention across the whole run) -- polling for the actual
// condition, not guessing a duration, is what's actually reliable.
async function waitForVerification(component: AuditLogComponent): Promise<void> {
  for (let i = 0; i < 200; i++) {
    if (component.verification() !== null) return;
    await new Promise((resolve) => setTimeout(resolve, 5));
  }
  throw new Error('verification() never settled within the poll window');
}

describe('AuditLogComponent', () => {
  let httpMock: HttpTestingController;
  const ANALYSIS_ID = '639223958897100060';
  const EVIDENCE_URL = `http://localhost:5103/api/v1/audit/analyses/${ANALYSIS_ID}/evidence`;
  const REVIEWS_URL = `http://localhost:5103/api/v1/compliance/analyses/${ANALYSIS_ID}/reviews`;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuditLogComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const evidence: AuditEvidenceRecord[] = [
    {
      auditEvidenceId: '78bb40a7-2946-4515-b034-ac52cd50f756',
      eventType: 'nexus1.root-cause.root-cause-verdict-issued.v1',
      schemaVersion: 1,
      correlationId: '86a5df3a-55d1-44ef-b0e0-cabf9d4d3034',
      causationId: null,
      envelopeSha256Hex: 'b4a1f70ddbf9ab74a85b06acc1e9429c5abae8c2dfa19cef1af11f160dc3203',
      occurredAtUtc: '2026-08-15T13:04:51Z',
      recordedAtUtc: '2026-08-15T13:04:51Z',
    },
  ];
  const reviews: ComplianceReview[] = [
    { complianceReviewId: 'a1b2c3d4-0000-0000-0000-000000000001', verdict: 'Loose fitting confirmed as cause.', state: 'Pending', openedAtUtc: '2026-08-15T13:05:00Z' },
  ];

  it('starts idle and, on lookup, fetches real evidence and compliance data for the entered analysis id', () => {
    const fixture = TestBed.createComponent(AuditLogComponent);
    fixture.componentInstance.onLookupIdInput(ANALYSIS_ID);
    fixture.componentInstance.lookup();
    httpMock.expectOne(EVIDENCE_URL).flush(evidence);
    httpMock.expectOne(REVIEWS_URL).flush(reviews);

    expect(fixture.componentInstance.complianceReviews()).toEqual(reviews);
  });

  it('chains real evidence records fetched across lookups and reports the chain as verified', async () => {
    const fixture = TestBed.createComponent(AuditLogComponent);
    fixture.componentInstance.onLookupIdInput(ANALYSIS_ID);
    fixture.componentInstance.lookup();
    httpMock.expectOne(EVIDENCE_URL).flush(evidence);
    httpMock.expectOne(REVIEWS_URL).flush(reviews);

    await waitForVerification(fixture.componentInstance);

    expect(fixture.componentInstance.chain()).toHaveLength(1);
    expect(fixture.componentInstance.chain()[0].envelopeSha256Hex).toBe(evidence[0].envelopeSha256Hex);
    expect(fixture.componentInstance.verification()?.ok).toBe(true);
  });

  it('does not duplicate an entry when the same analysis id is looked up twice', async () => {
    const fixture = TestBed.createComponent(AuditLogComponent);
    fixture.componentInstance.onLookupIdInput(ANALYSIS_ID);

    fixture.componentInstance.lookup();
    httpMock.expectOne(EVIDENCE_URL).flush(evidence);
    httpMock.expectOne(REVIEWS_URL).flush(reviews);
    await waitForVerification(fixture.componentInstance);

    fixture.componentInstance.lookup();
    httpMock.expectOne(EVIDENCE_URL).flush(evidence);
    httpMock.expectOne(REVIEWS_URL).flush(reviews);
    await new Promise((resolve) => setTimeout(resolve, 20)); // second lookup is a no-op; nothing new to poll for

    expect(fixture.componentInstance.chain()).toHaveLength(1);
  });

  it('never renders the word "tamper-proof" anywhere, only "verifies locally"', async () => {
    const fixture = TestBed.createComponent(AuditLogComponent);
    fixture.componentInstance.onLookupIdInput(ANALYSIS_ID);
    fixture.componentInstance.lookup();
    httpMock.expectOne(EVIDENCE_URL).flush(evidence);
    httpMock.expectOne(REVIEWS_URL).flush(reviews);
    await waitForVerification(fixture.componentInstance);
    fixture.detectChanges();

    const text: string = fixture.nativeElement.textContent;
    expect(text).not.toMatch(/tamper-proof/i);
    expect(text).toMatch(/verifies locally/i);
  });

  it('shows a real error state, not fake data, when the endpoints are unreachable', () => {
    const fixture = TestBed.createComponent(AuditLogComponent);
    fixture.componentInstance.onLookupIdInput(ANALYSIS_ID);
    fixture.componentInstance.lookup();
    httpMock.expectOne(EVIDENCE_URL).error(new ProgressEvent('error'));
    httpMock.expectOne(REVIEWS_URL).error(new ProgressEvent('error'));

    expect(fixture.componentInstance.lookupState().status).toBe('error');
  });
});
