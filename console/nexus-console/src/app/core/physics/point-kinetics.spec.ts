import { advanceReactor, deriveRateFromReadings, fractionalRate, periodFromRate, resetReactor, scram } from './point-kinetics';

describe('advanceReactor', () => {
  it('holds power steady at reactivity zero (critical)', () => {
    const state = resetReactor(100);
    const next = advanceReactor(state, 0, 5);
    expect(next.powerPercent).toBeCloseTo(100, 5);
    expect(next.periodSeconds).toBeNull();
  });

  it('raises power on positive reactivity and lowers it on negative', () => {
    const state = resetReactor(100);
    const up = advanceReactor(state, 50, 5);
    const down = advanceReactor(state, -50, 5);
    expect(up.powerPercent).toBeGreaterThan(100);
    expect(down.powerPercent).toBeLessThan(100);
  });

  it('reports a shorter period for a larger reactivity insertion', () => {
    const state = resetReactor(100);
    const small = advanceReactor(state, 10, 1);
    const large = advanceReactor(state, 50, 1);
    expect(Math.abs(large.periodSeconds!)).toBeLessThan(Math.abs(small.periodSeconds!));
  });

  it('clamps power within the model bounds', () => {
    const state = resetReactor(119);
    const next = advanceReactor(state, 120, 100);
    expect(next.powerPercent).toBeLessThanOrEqual(120);
  });

  it('decays power exponentially toward zero once scrammed, and reports no period', () => {
    const state = scram(resetReactor(100));
    const next = advanceReactor(state, 50, 5); // rod input ignored once scrammed
    expect(next.powerPercent).toBeLessThan(100);
    expect(next.periodSeconds).toBeNull();
    expect(next.scrammed).toBe(true);
  });
});

describe('deriveRateFromReadings / periodFromRate', () => {
  it('derives the correct log-rate from two real, distinct readings', () => {
    const previous = { value: 100, timestampUtc: '2026-08-24T00:00:00Z' };
    const current = { value: 110.517, timestampUtc: '2026-08-24T00:00:10Z' }; // 100*e^0.1 at t=10s
    const rate = deriveRateFromReadings(previous, current);
    expect(rate).toBeCloseTo(0.01, 3);
    expect(periodFromRate(rate)).toBeCloseTo(100, 1);
  });

  it('returns null when the two readings share the same timestamp (no real time elapsed)', () => {
    const reading = { value: 100, timestampUtc: '2026-08-24T00:00:00Z' };
    expect(deriveRateFromReadings(reading, reading)).toBeNull();
  });

  it('returns null for a non-positive reading (log undefined)', () => {
    const previous = { value: 0, timestampUtc: '2026-08-24T00:00:00Z' };
    const current = { value: 10, timestampUtc: '2026-08-24T00:00:10Z' };
    expect(deriveRateFromReadings(previous, current)).toBeNull();
  });

  it('periodFromRate reports null (critical) at a zero or null rate, never a huge/noisy number', () => {
    expect(periodFromRate(0)).toBeNull();
    expect(periodFromRate(null)).toBeNull();
  });
});

describe('fractionalRate', () => {
  it('scales linearly with reactivity -- doubling pcm doubles the rate', () => {
    expect(fractionalRate(100)).toBeCloseTo(2 * fractionalRate(50), 12);
  });

  it('is zero at zero reactivity', () => {
    expect(fractionalRate(0)).toBe(0);
  });
});
