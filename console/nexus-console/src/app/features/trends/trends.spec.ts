import { TestBed } from '@angular/core/testing';
import { TrendsComponent } from './trends';

describe('TrendsComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [TrendsComponent] }).compileComponents();
  });

  it('names the real persistence layer, never PostgreSQL or TimescaleDB, in its own corrected claim', () => {
    const fixture = TestBed.createComponent(TrendsComponent);
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;
    // The old, false claim is shown struck through for honest contrast
    // (same "was: ..." pattern used across this console). The corrected
    // description below it may explicitly DENY postgres/timescale ("no
    // PostgreSQL, no TimescaleDB") -- that's the honest correction -- but
    // must never assert this stack is BACKED BY either one.
    const correctedText = el.querySelector('.storage-note .sub')?.textContent ?? '';
    expect(correctedText).not.toMatch(/backed by.*(postgres|timescale)/i);
    expect(correctedText).toMatch(/SQL Server/);
    expect(el.querySelector('.was')?.textContent).toMatch(/postgres/i);
  });

  it('never renders a computed availability percentage, since no transition log exists', () => {
    const fixture = TestBed.createComponent(TrendsComponent);
    fixture.detectChanges();
    const text: string = fixture.nativeElement.textContent;
    expect(text).not.toMatch(/\d+(\.\d)?%/);
  });

  it('labels the availability gap NO SOURCE on its own status pill, never "insufficient history"', () => {
    const fixture = TestBed.createComponent(TrendsComponent);
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;
    const pillTexts = Array.from(el.querySelectorAll('.pill')).map((p) => p.textContent);
    expect(pillTexts.some((t) => /no source/i.test(t ?? ''))).toBe(true);
    expect(pillTexts.some((t) => /insufficient history/i.test(t ?? ''))).toBe(false);
  });

  it('does not repeat RootCause case history or power-snapshot history as a substitute panel', () => {
    const fixture = TestBed.createComponent(TrendsComponent);
    fixture.detectChanges();
    const text: string = fixture.nativeElement.textContent;
    expect(text).toMatch(/NO OTHER TREND PANEL SHOWN/);
  });
});
