import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ZoneRegistryComponent } from './zone-registry';
import { ActiveRadiationZone } from '../../core/api/radiation-zones-api';

describe('ZoneRegistryComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ZoneRegistryComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const zones: ActiveRadiationZone[] = [{ code: 'ZONE-UNIT-1', name: 'Zone Unit 1', unitCode: 'UNIT-1', classification: 'LOW', status: 'POSTED' }];

  it('starts in the loading state and fetches the fleet-wide endpoint (no unit id)', () => {
    const fixture = TestBed.createComponent(ZoneRegistryComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne('http://localhost:5103/api/v1/radiation-monitoring/zones').flush(zones);
  });

  it('groups the real zone list by real classification', () => {
    const fixture = TestBed.createComponent(ZoneRegistryComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/radiation-monitoring/zones').flush(zones);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.loadedGroups).toHaveLength(1);
    expect(c.loadedGroups[0].classification).toBe('LOW');
    expect(c.totalCount).toBe(1);
  });

  it('accepts the route-bound focusLabel purely for page orientation, not as a data filter', () => {
    const fixture = TestBed.createComponent(ZoneRegistryComponent);
    fixture.componentInstance.focusLabel = 'Live Presence';
    httpMock.expectOne('http://localhost:5103/api/v1/radiation-monitoring/zones').flush(zones);

    expect(fixture.componentInstance.loadedGroups).toHaveLength(1);
    expect(fixture.componentInstance.focusLabel).toBe('Live Presence');
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(ZoneRegistryComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/radiation-monitoring/zones').error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });
});
