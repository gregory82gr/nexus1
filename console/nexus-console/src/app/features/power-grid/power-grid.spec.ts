import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { PowerGridComponent } from './power-grid';
import { UnitSignalReading } from '../../core/api/instrumentation-api';
import { PlantStateService } from '../../core/state/plant-state';

describe('PowerGridComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PowerGridComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const turbineSignal: UnitSignalReading = {
    tag: 'UNIT1-TURB-001',
    name: 'Main Turbine Shaft Speed',
    categoryCode: 'TURBINE',
    latestValue: 2998.7,
    latestQualityCode: 'GOOD',
    latestTimestampUtc: '2026-08-25T09:00:00Z',
  };

  it('starts in the loading state and fetches signals for the selected unit', () => {
    const fixture = TestBed.createComponent(PowerGridComponent);
    const unitId = TestBed.inject(PlantStateService).selectedId();
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne(`http://localhost:5103/api/v1/instrumentation/units/${unitId}/signals`).flush([turbineSignal]);
  });

  it('builds a GridTie with the real turbine speed reading once signals load', () => {
    const fixture = TestBed.createComponent(PowerGridComponent);
    const unitId = TestBed.inject(PlantStateService).selectedId();
    httpMock.expectOne(`http://localhost:5103/api/v1/instrumentation/units/${unitId}/signals`).flush([turbineSignal]);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.tie?.turbineSpeedRpm).toEqual({ source: 'measured', rpm: 2998.7, timestampUtc: '2026-08-25T09:00:00Z' });
  });

  it('keeps gridFrequencyHz, phaseAngleDeg, breakerClosed, and inSync as no-source/awaiting-telemetry regardless of turbine speed', () => {
    const fixture = TestBed.createComponent(PowerGridComponent);
    const unitId = TestBed.inject(PlantStateService).selectedId();
    httpMock.expectOne(`http://localhost:5103/api/v1/instrumentation/units/${unitId}/signals`).flush([turbineSignal]);

    const tie = fixture.componentInstance.tie!;
    expect(tie.gridFrequencyHz).toEqual({ source: 'awaiting-telemetry' });
    expect(tie.phaseAngleDeg).toEqual({ source: 'no-source' });
    expect(tie.breakerClosed).toEqual({ source: 'no-source' });
    expect(tie.inSync).toEqual({ source: 'no-source' });
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(PowerGridComponent);
    const unitId = TestBed.inject(PlantStateService).selectedId();
    httpMock.expectOne(`http://localhost:5103/api/v1/instrumentation/units/${unitId}/signals`).error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });
});
