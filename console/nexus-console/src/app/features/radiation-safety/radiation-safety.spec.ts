import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { RadiationSafetyComponent } from './radiation-safety';
import { UnitRadiationSafety } from '../../core/api/radiation-safety-api';
import { PlantStateService } from '../../core/state/plant-state';

describe('RadiationSafetyComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RadiationSafetyComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const safety: UnitRadiationSafety = {
    unitId: 1,
    monitors: [
      { monitorCode: 'RM-CONT-1', monitorName: 'Containment Interior Monitor', monitorStatus: 'OPERATIONAL', latestValue: 14.2, engineeringUnitSymbol: 'uSv/h', quality: 'GOOD', latestReadingAtUtc: '2026-08-25T11:00:00Z' },
      { monitorCode: 'RM-AUX-1', monitorName: 'Aux Building Monitor', monitorStatus: 'OPERATIONAL', latestValue: 0.42, engineeringUnitSymbol: 'uSv/h', quality: 'GOOD', latestReadingAtUtc: '2026-08-25T10:00:00Z' },
    ],
    zones: [],
  };

  it('starts in the loading state and fetches the per-unit radiation-safety endpoint', () => {
    const fixture = TestBed.createComponent(RadiationSafetyComponent);
    const unitId = TestBed.inject(PlantStateService).selectedId();
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne(`http://localhost:5103/api/v1/radiation-monitoring/units/${unitId}`).flush(safety);
  });

  it('shows each monitor as its own independent reading, not derived from another', () => {
    const fixture = TestBed.createComponent(RadiationSafetyComponent);
    const unitId = TestBed.inject(PlantStateService).selectedId();
    httpMock.expectOne(`http://localhost:5103/api/v1/radiation-monitoring/units/${unitId}`).flush(safety);

    const monitors = fixture.componentInstance.loadedMonitors;
    expect(monitors).toHaveLength(2);
    expect(monitors[0].latestValue).toBe(14.2);
    expect(monitors[1].latestValue).toBe(0.42);
  });

  it('shows a real no-reading state for a monitor with no value, not a fabricated one', () => {
    const noReading: UnitRadiationSafety = {
      unitId: 1,
      monitors: [{ monitorCode: 'RM-STACK-1', monitorName: 'Stack Effluent Monitor', monitorStatus: 'OPERATIONAL', latestValue: null, engineeringUnitSymbol: null, quality: null, latestReadingAtUtc: null }],
      zones: [],
    };
    const fixture = TestBed.createComponent(RadiationSafetyComponent);
    const unitId = TestBed.inject(PlantStateService).selectedId();
    httpMock.expectOne(`http://localhost:5103/api/v1/radiation-monitoring/units/${unitId}`).flush(noReading);

    expect(fixture.componentInstance.loadedMonitors[0].latestValue).toBeNull();
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(RadiationSafetyComponent);
    const unitId = TestBed.inject(PlantStateService).selectedId();
    httpMock.expectOne(`http://localhost:5103/api/v1/radiation-monitoring/units/${unitId}`).error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });
});
