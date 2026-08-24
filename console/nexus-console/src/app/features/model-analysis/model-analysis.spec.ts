import { TestBed } from '@angular/core/testing';
import { ModelAnalysisComponent } from './model-analysis';

describe('ModelAnalysisComponent', () => {
  it('creates with no HttpClient provider at all -- genuinely no BFF dependency', async () => {
    // No provideHttpClient() / provideHttpClientTesting() here on purpose:
    // if this component tried to make an HTTP call, DI would throw
    // NullInjectorError for HttpClient, failing this test loudly.
    await TestBed.configureTestingModule({ imports: [ModelAnalysisComponent] }).compileComponents();
    const fixture = TestBed.createComponent(ModelAnalysisComponent);
    expect(() => fixture.detectChanges()).not.toThrow();
  });

  it('runs and passes every solver check on construction', async () => {
    await TestBed.configureTestingModule({ imports: [ModelAnalysisComponent] }).compileComponents();
    const fixture = TestBed.createComponent(ModelAnalysisComponent);
    expect(fixture.componentInstance.checks.length).toBeGreaterThan(0);
    expect(fixture.componentInstance.allPassed).toBe(true);
  });

  it('renders the documented model constants, not hardcoded display values', async () => {
    await TestBed.configureTestingModule({ imports: [ModelAnalysisComponent] }).compileComponents();
    const fixture = TestBed.createComponent(ModelAnalysisComponent);
    fixture.detectChanges();
    const text: string = fixture.nativeElement.textContent;
    expect(text).toContain(String(fixture.componentInstance.constants.SCRAM_DECAY_PER_SEC));
  });
});
