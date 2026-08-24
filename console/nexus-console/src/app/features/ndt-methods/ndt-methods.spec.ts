import { TestBed } from '@angular/core/testing';
import { NdtMethodsComponent } from './ndt-methods';

describe('NdtMethodsComponent', () => {
  it('creates with no HttpClient provider at all -- genuinely no BFF dependency', async () => {
    // No provideHttpClient()/provideHttpClientTesting() here on purpose --
    // if this component tried to make an HTTP call, DI would throw
    // NullInjectorError for HttpClient, failing this test loudly.
    await TestBed.configureTestingModule({ imports: [NdtMethodsComponent] }).compileComponents();
    const fixture = TestBed.createComponent(NdtMethodsComponent);
    expect(() => fixture.detectChanges()).not.toThrow();
  });

  it('renders all six real NDT methods, not a subset', async () => {
    await TestBed.configureTestingModule({ imports: [NdtMethodsComponent] }).compileComponents();
    const fixture = TestBed.createComponent(NdtMethodsComponent);
    fixture.detectChanges();
    const rows = fixture.nativeElement.querySelectorAll('.table .row');
    expect(rows.length).toBe(6);
  });

  it('names the real physical reason magnetic particle testing does not apply to rod cladding', async () => {
    await TestBed.configureTestingModule({ imports: [NdtMethodsComponent] }).compileComponents();
    const fixture = TestBed.createComponent(NdtMethodsComponent);
    fixture.detectChanges();
    const text: string = fixture.nativeElement.textContent;
    expect(text).toContain('non-ferromagnetic');
  });
});
