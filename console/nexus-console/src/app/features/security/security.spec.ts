import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { SecurityComponent } from './security';
import { ContextHealthResult } from '../../core/api/security-api';

describe('SecurityComponent', () => {
  let httpMock: HttpTestingController;
  const HEALTH_URL = 'http://localhost:5103/health/contexts';

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SecurityComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('starts loading and fetches the real per-context health endpoint', () => {
    const fixture = TestBed.createComponent(SecurityComponent);
    expect(fixture.componentInstance.contextHealthState().status).toBe('loading');
    httpMock.expectOne(HEALTH_URL).flush([]);
  });

  it('reflects a genuinely different per-context result in the rendered LED class (ok vs crit), proving it is not hardcoded', () => {
    const results: ContextHealthResult[] = [
      { contextName: 'Audit', status: 'Healthy', durationMs: 4.2 },
      { contextName: 'Compliance', status: 'Unhealthy', durationMs: 5012.7 },
    ];
    const fixture = TestBed.createComponent(SecurityComponent);
    httpMock.expectOne(HEALTH_URL).flush(results);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    const auditLed = el.querySelector('[data-svc="Audit"] .led')!;
    const complianceLed = el.querySelector('[data-svc="Compliance"] .led')!;
    expect(auditLed.classList.contains('ok')).toBe(true);
    expect(auditLed.classList.contains('crit')).toBe(false);
    expect(complianceLed.classList.contains('crit')).toBe(true);
    expect(complianceLed.classList.contains('ok')).toBe(false);
  });

  it('renders real context names and real per-check durations, not placeholders', () => {
    const results: ContextHealthResult[] = [{ contextName: 'ReactorFleet', status: 'Healthy', durationMs: 3.14159 }];
    const fixture = TestBed.createComponent(SecurityComponent);
    httpMock.expectOne(HEALTH_URL).flush(results);
    fixture.detectChanges();

    const text: string = fixture.nativeElement.textContent;
    expect(text).toContain('ReactorFleet');
    expect(text).toContain('3.1ms');
  });

  it('never claims service-level monitoring -- the panel explicitly labels itself DB connectivity only', () => {
    const fixture = TestBed.createComponent(SecurityComponent);
    httpMock.expectOne(HEALTH_URL).flush([]);
    fixture.detectChanges();

    const text: string = fixture.nativeElement.textContent;
    expect(text).toMatch(/DB CONNECTIVITY CHECK/i);
    expect(text).toMatch(/NOT SERVICE-LEVEL MONITORING/i);
  });

  it('renders OT Security Posture and Network Zones with no LED and no status claim anywhere', () => {
    const fixture = TestBed.createComponent(SecurityComponent);
    httpMock.expectOne(HEALTH_URL).flush([]);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    const otPanel = el.querySelector('[data-panel="ot-posture"]')!;
    const zonesPanel = el.querySelector('[data-panel="network-zones"]')!;
    expect(otPanel.querySelectorAll('.led').length).toBe(0);
    expect(zonesPanel.querySelectorAll('.led').length).toBe(0);
    // "Zero-Trust" appears once, in quotes, in the explanatory prose
    // contrasting this panel with the book's own tag -- it must never
    // appear as an actual badge/pill element, only as that one quoted
    // mention.
    const zeroTrustBadges = Array.from(otPanel.querySelectorAll('.pill, .tag, .led')).filter((e) => /zero-trust/i.test(e.textContent ?? ''));
    expect(zeroTrustBadges).toHaveLength(0);
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(SecurityComponent);
    httpMock.expectOne(HEALTH_URL).error(new ProgressEvent('error'));

    expect(fixture.componentInstance.contextHealthState().status).toBe('error');
  });
});
