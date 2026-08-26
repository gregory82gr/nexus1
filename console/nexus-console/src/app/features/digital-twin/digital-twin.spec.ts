import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { DigitalTwinComponent } from './digital-twin';

describe('DigitalTwinComponent', () => {
  let httpMock: HttpTestingController;
  const FLEET_URL = 'http://localhost:5103/api/v1/digital-twin/fleet';
  const DIVERGENCES_URL = 'http://localhost:5103/api/v1/digital-twin/divergences';
  const traceUrl = (twinCode: string) => `http://localhost:5103/api/v1/digital-twin/twins/${twinCode}/signals`;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DigitalTwinComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  function flushInitial(fixture: ReturnType<typeof TestBed.createComponent>, twins: unknown[] = [], divergences: unknown[] = []): void {
    httpMock.expectOne(FLEET_URL).flush(twins);
    httpMock.expectOne(DIVERGENCES_URL).flush(divergences);
    fixture.detectChanges();
  }

  it('renders real fleet-wide twins with model fidelity', () => {
    const fixture = TestBed.createComponent(DigitalTwinComponent);
    flushInitial(fixture, [{ unitCode: 'UNIT-1', twinCode: 'TWIN-U1-A', modelType: 'ThermalHydraulic', status: 'Active', fidelity: 'Validated' }]);

    const text: string = fixture.nativeElement.textContent;
    expect(text).toContain('UNIT-1');
    expect(text).toContain('TWIN-U1-A');
    expect(text).toContain('Validated');
  });

  it('loads the real signal trace for a twin only after it is selected', () => {
    const fixture = TestBed.createComponent(DigitalTwinComponent);
    flushInitial(fixture, [{ unitCode: 'UNIT-1', twinCode: 'TWIN-U1-A', modelType: 'ThermalHydraulic', status: 'Active', fidelity: 'Validated' }]);

    expect(fixture.nativeElement.textContent).toContain('Select an active twin');

    const button: HTMLButtonElement = fixture.nativeElement.querySelector('.twin-row');
    button.click();
    fixture.detectChanges();

    httpMock.expectOne(traceUrl('TWIN-U1-A')).flush([{ twinCode: 'TWIN-U1-A', modelVariable: 'CoreOutletTemp', signalTag: 'UNIT1-TH-014', bindingRole: 'Primary', bindingStatus: 'Bound' }]);
    fixture.detectChanges();

    const text: string = fixture.nativeElement.textContent;
    expect(text).toContain('CoreOutletTemp');
    expect(text).toContain('UNIT1-TH-014');
  });

  it('renders real fleet-wide divergences and labels the panel fleet-wide, not per-unit', () => {
    const fixture = TestBed.createComponent(DigitalTwinComponent);
    flushInitial(
      fixture,
      [],
      [{ detectedAtUtc: '2026-08-26T00:00:00Z', signalTag: 'UNIT1-TH-014', modelVariable: 'CoreOutletTemp', modeledValue: 320.1, measuredValue: 318.4, deltaValue: 1.7, severity: 'High', status: 'Open' }],
    );

    const panel: HTMLElement = fixture.nativeElement.querySelector('[data-panel="divergences"]');
    const text = panel.textContent ?? '';
    expect(text).toMatch(/FLEET-WIDE/i);
    expect(text).toMatch(/NOT PER-UNIT/i);
    expect(text).toContain('UNIT1-TH-014');
    expect(text).toContain('318.4');
  });

  it('never implies the divergences panel is scoped to a unit -- no unit code/id ever renders in that panel', () => {
    const fixture = TestBed.createComponent(DigitalTwinComponent);
    flushInitial(
      fixture,
      [],
      [{ detectedAtUtc: '2026-08-26T00:00:00Z', signalTag: 'UNIT1-TH-014', modelVariable: 'CoreOutletTemp', modeledValue: 320.1, measuredValue: 318.4, deltaValue: 1.7, severity: 'High', status: 'Open' }],
    );

    const panel: HTMLElement = fixture.nativeElement.querySelector('[data-panel="divergences"]');
    // The real OpenDivergenceDto carries no unit field at all -- confirming
    // none of "UNIT-", "unit ", or a bare unit id ever appears is a proxy
    // for "this panel never fabricates a per-unit scope it doesn't have."
    expect(panel.textContent).not.toMatch(/UNIT-\d/);
  });

  it('shows a real error state, not fake data, when the fleet endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(DigitalTwinComponent);
    httpMock.expectOne(FLEET_URL).error(new ProgressEvent('error'));
    httpMock.expectOne(DIVERGENCES_URL).flush([]);
    fixture.detectChanges();

    expect(fixture.componentInstance.fleetState().status).toBe('error');
  });
});
