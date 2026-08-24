import { TestBed } from '@angular/core/testing';
import { TrainingComponent } from './training';

describe('TrainingComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TrainingComponent],
    }).compileComponents();
  });

  it('creates with no drill selected and phase idle', () => {
    const fixture = TestBed.createComponent(TrainingComponent);
    fixture.detectChanges();
    expect(fixture.componentInstance.store.phase()).toBe('idle');
    expect(fixture.componentInstance.selectedDrill).toBeNull();
  });

  it('never renders a score without the UNCALIBRATED marker', () => {
    const fixture = TestBed.createComponent(TrainingComponent);
    fixture.detectChanges();
    const c = fixture.componentInstance;
    c.store.selectDrill('power-maneuver');
    // Directly force a finished state rather than driving the real timer --
    // this test is about the template's own rendering rule, not the sim.
    (c.store as unknown as { phase: { set: (v: string) => void } }).phase.set('done');
    (c.store as unknown as { score: { set: (v: unknown) => void } }).score.set({ value: 84, verdict: 'PASS', calibrated: false });
    fixture.detectChanges();

    const text: string = fixture.nativeElement.textContent;
    expect(text).toContain('84');
    expect(text).toContain('UNCALIBRATED');
  });

  it('renders each drill from the real catalogue as a selectable card', () => {
    const fixture = TestBed.createComponent(TrainingComponent);
    fixture.detectChanges();
    const cards = fixture.nativeElement.querySelectorAll('.drill-card');
    expect(cards.length).toBe(fixture.componentInstance.drills.length);
  });
});
