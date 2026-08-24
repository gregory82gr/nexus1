import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ReactorInstrumentationComponent } from './reactor-instrumentation';
import { UnitSignalReading } from '../../core/api/instrumentation-api';

describe('ReactorInstrumentationComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReactorInstrumentationComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const signals: UnitSignalReading[] = [
    { tag: 'UNIT1-NI-001', name: 'Neutron Flux Channel 1', categoryCode: 'NEUTRONICS', latestValue: 99.2, latestQualityCode: 'GOOD', latestTimestampUtc: '2026-08-24T10:00:00' },
    { tag: 'UNIT1-NI-002', name: 'Neutron Flux Channel 2', categoryCode: 'NEUTRONICS', latestValue: null, latestQualityCode: null, latestTimestampUtc: null },
  ];

  it('starts in the loading state', () => {
    const fixture = TestBed.createComponent(ReactorInstrumentationComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne('http://localhost:5103/api/v1/instrumentation/units/1/signals').flush(signals);
  });

  it('groups the real signal list by CategoryCode and counts reporting signals honestly', () => {
    const fixture = TestBed.createComponent(ReactorInstrumentationComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/instrumentation/units/1/signals').flush(signals);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.loadedGroups).toHaveLength(1);
    expect(c.loadedGroups[0].categoryCode).toBe('NEUTRONICS');
    expect(c.reportingCount()).toBe(1); // only one of the two has a reading
    expect(c.totalCount).toBe(2);
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(ReactorInstrumentationComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/instrumentation/units/1/signals').error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });

  it('accepts the route-bound focusLabel purely for page orientation, not as a data filter', () => {
    const fixture = TestBed.createComponent(ReactorInstrumentationComponent);
    fixture.componentInstance.focusLabel = 'Coolant / TH';
    httpMock.expectOne('http://localhost:5103/api/v1/instrumentation/units/1/signals').flush(signals);

    // Same signal list regardless of which nav entry set the label.
    expect(fixture.componentInstance.loadedGroups).toHaveLength(1);
    expect(fixture.componentInstance.focusLabel).toBe('Coolant / TH');
  });
});
