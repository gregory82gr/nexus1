import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ReactorKineticsComponent } from './reactor-kinetics';
import { UnitSignalReading } from '../../core/api/instrumentation-api';

const URL = 'http://localhost:5103/api/v1/instrumentation/units/1/signals';

function powerSignal(value: number, timestampUtc: string): UnitSignalReading {
  return { tag: 'NI-001', name: 'Neutron Flux Channel 1', categoryCode: 'NEUTRONICS', latestValue: value, latestQualityCode: 'GOOD', latestTimestampUtc: timestampUtc };
}

describe('ReactorKineticsComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    jest.useFakeTimers();
    await TestBed.configureTestingModule({
      imports: [ReactorKineticsComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    jest.useRealTimers();
  });

  it('polls immediately on creation and shows critical (no period) on the first sample alone', () => {
    const fixture = TestBed.createComponent(ReactorKineticsComponent);
    httpMock.expectOne(URL).flush([powerSignal(100, '2026-08-24T00:00:00Z')]);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.powerSignal?.latestValue).toBe(100);
    expect(c.periodSeconds()).toBeNull();
  });

  it('derives a real period from two distinct polled readings, not from a simulation', () => {
    const fixture = TestBed.createComponent(ReactorKineticsComponent);
    httpMock.expectOne(URL).flush([powerSignal(100, '2026-08-24T00:00:00Z')]);

    jest.advanceTimersByTime(5000);
    httpMock.expectOne(URL).flush([powerSignal(110.517, '2026-08-24T00:00:10Z')]); // 100*e^0.1 at t=10s

    const c = fixture.componentInstance;
    expect(c.periodSeconds()).toBeCloseTo(100, 0);
    expect(c.pollCount()).toBe(2);
  });

  it('stays critical (null period) when the polled value never changes -- the real dev DB does not update on its own', () => {
    const fixture = TestBed.createComponent(ReactorKineticsComponent);
    httpMock.expectOne(URL).flush([powerSignal(100, '2026-08-24T00:00:00Z')]);
    jest.advanceTimersByTime(5000);
    httpMock.expectOne(URL).flush([powerSignal(100, '2026-08-24T00:00:00Z')]); // identical -- no new measurement

    expect(fixture.componentInstance.periodSeconds()).toBeNull();
  });

  it('reports NO SOURCE, not a guess, when no power-like signal is present', () => {
    const fixture = TestBed.createComponent(ReactorKineticsComponent);
    httpMock.expectOne(URL).flush([{ tag: 'VIB-1', name: 'Bearing Vibration', categoryCode: 'VIBRATION', latestValue: 4.2, latestQualityCode: 'GOOD', latestTimestampUtc: '2026-08-24T00:00:00Z' }]);

    expect(fixture.componentInstance.powerSignal).toBeNull();
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(ReactorKineticsComponent);
    httpMock.expectOne(URL).error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });
});
