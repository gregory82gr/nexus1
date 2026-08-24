import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { Plant3dComponent } from './plant-3d';
import { UnitTwinState } from '../../core/api/digital-twin-api';

describe('Plant3dComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Plant3dComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const twin: UnitTwinState = {
    unitId: 1,
    unitCode: 'UNIT-1',
    twinCode: 'TWIN-1-PK',
    twinName: 'Unit 1 Point Kinetics Model',
    modelType: 'POINT_KINETICS',
    status: 'Active',
    fidelity: 'Validated',
    isAuthoritative: true,
  };

  it('starts in the loading state', () => {
    const fixture = TestBed.createComponent(Plant3dComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne('http://localhost:5103/api/v1/digital-twin/units/1').flush([twin]);
  });

  it('derives status tone and fidelity band from the real DTO fields, not fabricated ones', () => {
    const fixture = TestBed.createComponent(Plant3dComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/digital-twin/units/1').flush([twin]);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.loadedTwin?.twinCode).toBe('TWIN-1-PK');
    expect(c.tone).toBe('ok');
    expect(c.fidelityBand).toBe(4); // 'Validated' is the fifth and highest band
  });

  it('never guesses an unrecognized status/fidelity into a known tone or band', () => {
    const oddTwin: UnitTwinState = { ...twin, status: 'Quarantined', fidelity: 'Beta' };
    const fixture = TestBed.createComponent(Plant3dComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/digital-twin/units/1').flush([oddTwin]);

    const c = fixture.componentInstance;
    expect(c.tone).toBe('unknown');
    expect(c.fidelityBand).toBeNull();
  });

  it('picks the authoritative twin when the endpoint returns more than one for a unit', () => {
    const secondary: UnitTwinState = { ...twin, twinCode: 'TWIN-1-SECONDARY', isAuthoritative: false };
    const fixture = TestBed.createComponent(Plant3dComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/digital-twin/units/1').flush([secondary, twin]);

    expect(fixture.componentInstance.loadedTwin?.twinCode).toBe('TWIN-1-PK');
  });

  it('treats an empty array as a real, non-error "no twin modeled" state, not a 404', () => {
    const fixture = TestBed.createComponent(Plant3dComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/digital-twin/units/1').flush([]);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.loadedTwin).toBeNull();
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(Plant3dComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/digital-twin/units/1').error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });

  it('renders the fallback rather than crashing when three.js/WebGL cannot start (jsdom has no real WebGL context)', async () => {
    const fixture = TestBed.createComponent(Plant3dComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/digital-twin/units/1').flush([twin]);

    fixture.detectChanges(); // renders the template, including #stage -- triggers ngAfterViewInit
    await fixture.whenStable(); // waits out the dynamic import('three') + WebGLRenderer attempt

    // This is real, not simulated: jsdom genuinely has no WebGL context,
    // so TwinScene's construction throws and the component's own
    // try/catch (plant-3d.ts's ngAfterViewInit) sets this signal --
    // exercising the exact fallback branch a restricted-egress or
    // headless environment would hit live.
    expect(fixture.componentInstance.unavailable()).toBe(true);
  });
});
