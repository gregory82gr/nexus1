import { DrillDefinition } from './drills';
import { advanceDrill, initProgress } from './drill-runner';
import { ReactorState } from '../../core/physics/point-kinetics';

function reactorAt(powerPercent: number, scrammed = false): ReactorState {
  return { powerPercent, periodSeconds: null, scrammed };
}

const holdDrill: DrillDefinition = {
  id: 'test-hold',
  name: 'Test Hold',
  difficulty: 'CORE',
  description: '',
  kind: 'hold',
  initialPowerPercent: 100,
  targetPercent: 80,
  tolerancePercent: 2,
  holdSecondsRequired: 10,
  timeLimitSeconds: 60,
};

describe('advanceDrill (hold)', () => {
  it('completes with a full-marks score once the hold requirement is met with no excursions', () => {
    let progress = initProgress(holdDrill);
    let result = advanceDrill(holdDrill, progress, reactorAt(80), 10, false);
    expect(result.done).toBe(true);
    if (result.done) {
      expect(result.score).toEqual({ value: 100, verdict: 'PASS', calibrated: false });
    }
  });

  it('counts an excursion when power leaves the band after entering it', () => {
    let progress = initProgress(holdDrill);
    let step = advanceDrill(holdDrill, progress, reactorAt(80), 2, false); // enters band
    progress = step.progress as typeof progress;
    step = advanceDrill(holdDrill, progress, reactorAt(90), 2, false); // leaves band -- excursion
    progress = step.progress as typeof progress;
    expect((progress as { excursions: number }).excursions).toBe(1);
  });

  it('ends the drill immediately on an operator SCRAM, scored as a partial-credit fail', () => {
    const progress = initProgress(holdDrill);
    const result = advanceDrill(holdDrill, progress, reactorAt(50, true), 1, false);
    expect(result.done).toBe(true);
    if (result.done) {
      expect(result.score.verdict).toBe('FAIL');
    }
  });

  it('ends the drill on undershoot past the named floor, when one is declared', () => {
    const drillWithFloor: DrillDefinition = { ...holdDrill, targetPercent: 50, undershootFloorPercent: 40 };
    const progress = initProgress(drillWithFloor);
    const result = advanceDrill(drillWithFloor, progress, reactorAt(35), 1, false);
    expect(result.done).toBe(true);
  });

  it('times out and scores a partial-credit fail if the hold is never completed', () => {
    let progress = initProgress(holdDrill);
    const result = advanceDrill(holdDrill, progress, reactorAt(60), 60, false);
    expect(result.done).toBe(true);
    if (result.done) {
      expect(result.score.verdict).toBe('FAIL');
    }
  });
});

const followDrill: DrillDefinition = {
  id: 'test-follow',
  name: 'Test Follow',
  difficulty: 'INTERMEDIATE',
  description: '',
  kind: 'follow',
  initialPowerPercent: 100,
  tolerancePercent: 3,
  schedule: [
    { atSeconds: 0, targetPercent: 100 },
    { atSeconds: 10, targetPercent: 80 },
  ],
  totalSeconds: 20,
  timeLimitSeconds: 20,
};

describe('advanceDrill (follow)', () => {
  it('tracks on-demand time against the current schedule step, not a fixed target', () => {
    let progress = initProgress(followDrill);
    let step = advanceDrill(followDrill, progress, reactorAt(100), 5, false); // on-demand for step 1
    progress = step.progress as typeof progress;
    step = advanceDrill(followDrill, progress, reactorAt(80), 15, false); // on-demand for step 2, completes drill
    expect(step.done).toBe(true);
    if (step.done) {
      // fully on-demand throughout -> full marks
      expect(step.score.value).toBe(100);
    }
  });

  it('finishes early and scores proportionally if the operator SCRAMs mid-drill', () => {
    const progress = initProgress(followDrill);
    const result = advanceDrill(followDrill, progress, reactorAt(50, true), 5, false);
    expect(result.done).toBe(true);
  });
});

const tripDrill: DrillDefinition = {
  id: 'test-trip',
  name: 'Test Trip',
  difficulty: 'CORE',
  description: '',
  kind: 'trip',
  initialPowerPercent: 100,
  cueAtSeconds: 10,
  reactionWindowSeconds: 5,
  timeLimitSeconds: 30,
};

describe('advanceDrill (trip)', () => {
  it('scores zero for a SCRAM pressed before the cue fires', () => {
    const progress = initProgress(tripDrill);
    const result = advanceDrill(tripDrill, progress, reactorAt(100, true), 5, true);
    expect(result.done).toBe(true);
    if (result.done) expect(result.score.value).toBe(0);
  });

  it('scores a high mark for a prompt SCRAM right after the cue', () => {
    let progress = initProgress(tripDrill);
    let step = advanceDrill(tripDrill, progress, reactorAt(100), 10, false); // cue fires
    progress = step.progress as typeof progress;
    step = advanceDrill(tripDrill, progress, reactorAt(100, true), 0.1, true); // SCRAM immediately after
    expect(step.done).toBe(true);
    if (step.done) expect(step.score.value).toBeGreaterThan(90);
  });

  it('scores zero if the reaction window expires with no SCRAM', () => {
    let progress = initProgress(tripDrill);
    let step = advanceDrill(tripDrill, progress, reactorAt(100), 10, false); // cue fires
    progress = step.progress as typeof progress;
    step = advanceDrill(tripDrill, progress, reactorAt(100), 6, false); // window (5s) expires unanswered
    expect(step.done).toBe(true);
    if (step.done) expect(step.score.value).toBe(0);
  });
});
