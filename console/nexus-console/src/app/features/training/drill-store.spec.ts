import { DrillStore } from './drill-store';

describe('DrillStore', () => {
  beforeEach(() => jest.useFakeTimers());
  afterEach(() => jest.useRealTimers());

  it('starts idle with no drill selected', () => {
    const store = new DrillStore();
    expect(store.phase()).toBe('idle');
    expect(store.selectedDrill).toBeNull();
  });

  it('runs the selected drill to completion over simulated time (timeout path)', () => {
    const store = new DrillStore();
    store.selectDrill('power-maneuver'); // 100% -> 80%, hold 25s, 90s time limit
    store.start();
    expect(store.phase()).toBe('running');

    // Rod left at critical (0): power never approaches the 80% target, so
    // the drill runs out its own time limit rather than completing the
    // hold -- a deterministic way to exercise "runs to completion"
    // without depending on the sim's exact trajectory.
    jest.advanceTimersByTime(91_000);
    expect(store.phase()).toBe('done');
    expect(store.score()).not.toBeNull();
    expect(store.score()?.calibrated).toBe(false);
  });

  it('reaches a passing, full-marks score when steered onto target and held there', () => {
    const store = new DrillStore();
    store.selectDrill('power-maneuver'); // 100% -> 80%, hold 25s
    store.start();

    // -50 pcm reactivity: power decays toward 80% in a few seconds
    // (exp(-0.05 * t) = 0.8 at t ~= 4.46s), then rod returns to critical
    // to hold there for the required 25s.
    store.setRodPosition(-50);
    jest.advanceTimersByTime(4_460);
    store.setRodPosition(0);
    jest.advanceTimersByTime(25_500);

    expect(store.phase()).toBe('done');
    expect(store.score()).toEqual({ value: 100, verdict: 'PASS', calibrated: false });
  });

  it('stops ticking once frozen, and resumes from where it left off', () => {
    const store = new DrillStore();
    store.selectDrill('power-maneuver');
    store.start();
    jest.advanceTimersByTime(2_000);
    const powerAtFreeze = store.reactor().powerPercent;

    store.freeze();
    expect(store.phase()).toBe('frozen');
    jest.advanceTimersByTime(5_000); // no ticks should occur while frozen
    expect(store.reactor().powerPercent).toBe(powerAtFreeze);

    store.resume();
    expect(store.phase()).toBe('running');
  });

  it('aborts back to idle without producing a score', () => {
    const store = new DrillStore();
    store.selectDrill('power-maneuver');
    store.start();
    jest.advanceTimersByTime(1_000);
    store.abort();
    expect(store.phase()).toBe('idle');
    expect(store.score()).toBeNull();
  });

  it('stops the interval on destroy, so no further ticks fire after the route/component is gone', () => {
    const store = new DrillStore();
    store.selectDrill('power-maneuver');
    store.start();
    jest.advanceTimersByTime(1_000);
    const powerAtDestroy = store.reactor().powerPercent;

    store.ngOnDestroy();
    jest.advanceTimersByTime(10_000);
    expect(store.reactor().powerPercent).toBe(powerAtDestroy);
  });

  it('a fresh store never resumes an old drill\'s state -- each instance is its own sandbox', () => {
    const first = new DrillStore();
    first.selectDrill('power-maneuver');
    first.start();
    jest.advanceTimersByTime(2_000);
    first.ngOnDestroy();

    const second = new DrillStore();
    expect(second.phase()).toBe('idle');
    expect(second.reactor().powerPercent).toBe(100);
  });
});
