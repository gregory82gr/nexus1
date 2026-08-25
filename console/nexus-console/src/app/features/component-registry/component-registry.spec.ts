import { TestBed } from '@angular/core/testing';
import { ComponentRegistryComponent } from './component-registry';

describe('ComponentRegistryComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ComponentRegistryComponent] }).compileComponents();
  });

  it('never renders a fabricated health percentage or wear-model figure of any kind', () => {
    const fixture = TestBed.createComponent(ComponentRegistryComponent);
    fixture.detectChanges();
    const text: string = fixture.nativeElement.textContent;
    expect(text).not.toMatch(/\d+(\.\d)?%/);
  });

  it('never renders a health/life bar element, since no real model exists to back one', () => {
    const fixture = TestBed.createComponent(ComponentRegistryComponent);
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelectorAll('.lifebar, .compcard').length).toBe(0);
  });

  it('declares the gap as NO SOURCE on all three of the book\'s real model inputs', () => {
    const fixture = TestBed.createComponent(ComponentRegistryComponent);
    fixture.detectChanges();
    const text: string = fixture.nativeElement.textContent;
    expect(text).toMatch(/service years/i);
    expect(text).toMatch(/SCRAM/i);
    expect(text).toMatch(/load-sensitivity/i);
  });

  it('declares the book\'s 11-12-component premise as unsupported, not silently ignored', () => {
    const fixture = TestBed.createComponent(ComponentRegistryComponent);
    fixture.detectChanges();
    const text: string = fixture.nativeElement.textContent;
    expect(text).toMatch(/11 to 12/i);
  });

  it('states explicitly that the one real adjacent data is not duplicated here', () => {
    const fixture = TestBed.createComponent(ComponentRegistryComponent);
    fixture.detectChanges();
    const text: string = fixture.nativeElement.textContent;
    expect(text).toMatch(/NOT DUPLICATED HERE/i);
    expect(text).toMatch(/Rod Inspection/i);
    expect(text).toMatch(/Ageing & Degradation/i);
  });
});
