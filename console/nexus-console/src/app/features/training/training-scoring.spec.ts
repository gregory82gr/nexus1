import { SCORING, scoreFollow, scoreHold, scoreTrip } from './training-scoring';

describe('scoreHold', () => {
  it('scores a clean hold (no excursions) at 100, marked uncalibrated', () => {
    const score = scoreHold({ outcome: 'held', excursions: 0, fractionHeld: 1 });
    expect(score).toEqual({ value: 100, verdict: 'PASS', calibrated: false });
  });

  it('deducts the named excursion penalty per band exit, never below the pass floor', () => {
    const score = scoreHold({ outcome: 'held', excursions: 2, fractionHeld: 1 });
    expect(score.value).toBe(100 - 2 * SCORING.EXCURSION_PENALTY_HOLD);

    const manyExcursions = scoreHold({ outcome: 'held', excursions: 20, fractionHeld: 1 });
    expect(manyExcursions.value).toBe(SCORING.PASS_FLOOR);
  });

  it('applies the SCRAM partial-credit multiplier and fails the drill', () => {
    const score = scoreHold({ outcome: 'scram', excursions: 0, fractionHeld: 0.5 });
    expect(score.value).toBe(Math.round(0.5 * SCORING.PARTIAL_SCRAM));
    expect(score.verdict).toBe('FAIL');
  });

  it('applies the timeout partial-credit multiplier', () => {
    const score = scoreHold({ outcome: 'timeout', excursions: 0, fractionHeld: 0.4 });
    expect(score.value).toBe(Math.round(0.4 * SCORING.PARTIAL_TIMEOUT));
  });
});

describe('scoreFollow', () => {
  it('reaches full marks at the named on-demand fraction threshold', () => {
    const score = scoreFollow({ onDemandFraction: SCORING.FOLLOW_FULL_MARKS_FRAC, excursions: 0 });
    expect(score.value).toBe(100);
    expect(score.verdict).toBe('PASS');
  });

  it('fails below the named pass mark', () => {
    const score = scoreFollow({ onDemandFraction: 0.3, excursions: 0 });
    expect(score.verdict).toBe('FAIL');
  });
});

describe('scoreTrip', () => {
  it('scores an instant on-time response near 100', () => {
    const score = scoreTrip({ outcome: 'onTime', reactionSeconds: 0, reactionWindowSeconds: 8 });
    expect(score.value).toBe(100);
    expect(score.verdict).toBe('PASS');
  });

  it('never scores an on-time response below the named pass floor', () => {
    const score = scoreTrip({ outcome: 'onTime', reactionSeconds: 8, reactionWindowSeconds: 8 });
    expect(score.value).toBe(SCORING.PASS_FLOOR);
  });

  it('scores an unplanned SCRAM (before the cue) at zero', () => {
    const score = scoreTrip({ outcome: 'unplannedScram', reactionSeconds: 0, reactionWindowSeconds: 8 });
    expect(score.value).toBe(0);
    expect(score.verdict).toBe('FAIL');
  });

  it('scores a cue that expired unanswered at zero', () => {
    const score = scoreTrip({ outcome: 'lateTimeout', reactionSeconds: 20, reactionWindowSeconds: 8 });
    expect(score.value).toBe(0);
  });

  it('every score carries calibrated: false', () => {
    expect(scoreHold({ outcome: 'held', excursions: 0, fractionHeld: 1 }).calibrated).toBe(false);
    expect(scoreFollow({ onDemandFraction: 1, excursions: 0 }).calibrated).toBe(false);
    expect(scoreTrip({ outcome: 'onTime', reactionSeconds: 1, reactionWindowSeconds: 8 }).calibrated).toBe(false);
  });
});
