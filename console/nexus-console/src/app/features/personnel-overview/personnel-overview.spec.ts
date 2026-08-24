import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { PersonnelOverviewComponent } from './personnel-overview';
import { DepartmentRosterEntry } from '../../core/api/organization-api';

describe('PersonnelOverviewComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PersonnelOverviewComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const roster: DepartmentRosterEntry[] = [
    { personId: 1, displayName: 'Alex Rivera', personnelNumber: null, positionTitle: 'Reactor Operator', isSafetyCriticalPosition: true, applicationUserId: null, startDate: '2026-01-01', isPrimary: true },
    { personId: 2, displayName: 'Jordan Chen', personnelNumber: null, positionTitle: 'Shift Supervisor', isSafetyCriticalPosition: true, applicationUserId: 42, startDate: '2026-02-01', isPrimary: true },
  ];

  it('starts in the loading state and fetches department 1 by default', () => {
    const fixture = TestBed.createComponent(PersonnelOverviewComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne('http://localhost:5103/api/v1/organization/departments/1/roster').flush(roster);
  });

  it('aggregates the real roster into counts, never exposing a name anywhere in component state', () => {
    const fixture = TestBed.createComponent(PersonnelOverviewComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/organization/departments/1/roster').flush(roster);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.summary?.totalCount).toBe(2);
    expect(c.summary?.safetyCriticalCount).toBe(2);
    expect(c.summary?.positions).toHaveLength(2);
    expect(JSON.stringify(c.state())).not.toContain('Alex Rivera');
  });

  it('re-fetches for a new department id entered in the picker', () => {
    const fixture = TestBed.createComponent(PersonnelOverviewComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/organization/departments/1/roster').flush(roster);

    fixture.componentInstance.onDepartmentIdInput('2');
    expect(fixture.componentInstance.departmentState.selectedId()).toBe(2);
    httpMock.expectOne('http://localhost:5103/api/v1/organization/departments/2/roster').flush([]);

    expect(fixture.componentInstance.summary?.totalCount).toBe(0);
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(PersonnelOverviewComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/organization/departments/1/roster').error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });
});
