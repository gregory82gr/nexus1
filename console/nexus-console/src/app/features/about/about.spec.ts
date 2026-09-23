import { TestBed } from '@angular/core/testing';
import { AboutComponent } from './about';

describe('AboutComponent', () => {
  it('renders as pure static content with no HttpClient provider required -- no backend call is even possible', async () => {
    // Deliberately no provideHttpClient()/provideHttpClientTesting() here --
    // same discipline as help.spec.ts: if this component tried to inject
    // HttpClient, TestBed.createComponent would throw.
    await TestBed.configureTestingModule({ imports: [AboutComponent] }).compileComponents();
    const fixture = TestBed.createComponent(AboutComponent);
    fixture.detectChanges();

    const text: string = fixture.nativeElement.textContent;
    expect(text).toMatch(/STATIC REFERENCE/i);
    expect(text).toMatch(/Angular 18/);
    expect(text).toMatch(/\.NET 8/);
    expect(text).toMatch(/Advisory and illustrative only/);
  });
});
