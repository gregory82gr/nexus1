import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { FleetComponent } from './fleet';
import { UnitSummary } from '../../core/api/reactor-fleet-api';

describe('FleetComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FleetComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('starts in the loading state', () => {
    const fixture = TestBed.createComponent(FleetComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne('http://localhost:5103/api/v1/reactor-fleet/units').flush([]);
  });

  it('shows real data once the fleet endpoint resolves, honoring the known-gap fields', () => {
    const units: UnitSummary[] = [
      { id: 1, code: 'UNIT-1', name: 'Demonstrator Unit 1', latestPowerPercent: 92, latestPowerRecordedAtUtc: '2026-08-24T09:00:00' },
      { id: 2, code: 'UNIT-2', name: 'Demonstrator Unit 2', latestPowerPercent: null, latestPowerRecordedAtUtc: null },
    ];

    const fixture = TestBed.createComponent(FleetComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/reactor-fleet/units').flush(units);

    const state = fixture.componentInstance.state();
    expect(state.status).toBe('loaded');
    expect(fixture.componentInstance.reportingCount()).toBe(1);
    expect(fixture.componentInstance.totalCount()).toBe(2);
  });

  it('shows an error state, not fake data, when the request fails', () => {
    const fixture = TestBed.createComponent(FleetComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/reactor-fleet/units').flush(
      { message: 'down' },
      { status: 503, statusText: 'Service Unavailable' },
    );

    const state = fixture.componentInstance.state();
    expect(state.status).toBe('error');
  });

  it('marks a unit as selected on click, and only that one', () => {
    const fixture = TestBed.createComponent(FleetComponent);
    httpMock
      .expectOne('http://localhost:5103/api/v1/reactor-fleet/units')
      .flush([{ id: 1, code: 'UNIT-1', name: 'Unit 1', latestPowerPercent: 90, latestPowerRecordedAtUtc: null }]);

    fixture.componentInstance.select({ id: 1, code: 'UNIT-1', name: 'Unit 1', latestPowerPercent: 90, latestPowerRecordedAtUtc: null });
    expect(fixture.componentInstance.selectedId()).toBe(1);
  });
});
