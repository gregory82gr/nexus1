import { TestBed } from '@angular/core/testing';
import { HelpComponent } from './help';

describe('HelpComponent', () => {
  it('renders as pure static content with no HttpClient provider required -- no backend call is even possible', async () => {
    // Deliberately no provideHttpClient()/provideHttpClientTesting() here:
    // if this component (or anything it renders) tried to inject HttpClient,
    // TestBed.createComponent would throw a NullInjectorError. It doesn't.
    await TestBed.configureTestingModule({ imports: [HelpComponent] }).compileComponents();
    const fixture = TestBed.createComponent(HelpComponent);
    fixture.detectChanges();

    const text: string = fixture.nativeElement.textContent;
    expect(text).toMatch(/STATIC REFERENCE/i);
    expect(text).toMatch(/NX-Script Console/i);
  });
});
