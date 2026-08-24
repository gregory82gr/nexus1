import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AssetConditionComponent } from './asset-condition';
import { UnitAssetCondition } from '../../core/api/maintenance-api';

describe('AssetConditionComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssetConditionComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const assets: UnitAssetCondition[] = [
    { assetCode: 'ASSET-UNIT1-001', name: 'Primary Coolant Pump', category: 'MECHANICAL', status: 'IN_SERVICE', isSafetyRelated: true, latestAssessedAtUtc: '2026-08-22T10:00:00', latestConditionGrade: 'GOOD', latestHealthScorePercent: 91, latestRemainingUsefulLifeDays: 1750 },
    { assetCode: 'ASSET-UNIT1-002', name: 'Backup Feedwater Valve', category: 'MECHANICAL', status: 'IN_SERVICE', isSafetyRelated: false, latestAssessedAtUtc: null, latestConditionGrade: null, latestHealthScorePercent: null, latestRemainingUsefulLifeDays: null },
  ];

  it('starts in the loading state', () => {
    const fixture = TestBed.createComponent(AssetConditionComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne('http://localhost:5103/api/v1/maintenance/units/1/assets').flush(assets);
  });

  it('groups the real asset list by Category and counts assessed assets honestly', () => {
    const fixture = TestBed.createComponent(AssetConditionComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/maintenance/units/1/assets').flush(assets);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.loadedGroups).toHaveLength(1);
    expect(c.loadedGroups[0].category).toBe('MECHANICAL');
    expect(c.assessedCount()).toBe(1); // only one of the two has a condition assessment
    expect(c.totalCount).toBe(2);
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(AssetConditionComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/maintenance/units/1/assets').error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });
});
