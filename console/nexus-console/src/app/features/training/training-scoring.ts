// Drill scoring constants.
//
// PROVENANCE: authored for the NEXUS-1 demonstrator, carried across from
// the source file and the book's own port unchanged. NOT derived from any
// operator-training standard, NOT reviewed by a training authority. They
// produce a plausible-feeling number and nothing more -- see the banner
// rendered above every drill. Changing them would imply the new ones were
// better founded, and they would not be, so they are reused verbatim.
export const SCORING = {
  EXCURSION_PENALTY_HOLD: 8, // points lost per band exit
  EXCURSION_PENALTY_FOLLOW: 4, // ditto, load-follow drills
  PASS_FLOOR: 40, // a completed run never scores below this
  FOLLOW_FULL_MARKS_FRAC: 0.85, // on-demand fraction that earns 100
  FOLLOW_PASS_MARK: 60, // score at or above which a follow passes
  PARTIAL_SCRAM: 45, // credit multiplier, drill ended by SCRAM
  PARTIAL_LIMIT: 40, // ditto, ended by over/undershoot
  PARTIAL_TIMEOUT: 60, // ditto, ran out of time
} as const;

function clamp(value: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, value));
}

// The field worth noticing is calibrated: false. It travels with the
// score into every consumer -- the objective panel, and any future log
// entry -- rather than living only in a comment.
export interface DrillScore {
  value: number;
  verdict: 'PASS' | 'FAIL';
  calibrated: false;
}

export interface HoldRun {
  outcome: 'held' | 'scram' | 'timeout' | 'limit';
  excursions: number;
  fractionHeld: number; // used only for the partial-credit outcomes
}

export function scoreHold(run: HoldRun): DrillScore {
  const penalty = run.excursions * SCORING.EXCURSION_PENALTY_HOLD;
  if (run.outcome === 'held') {
    return { value: clamp(100 - penalty, SCORING.PASS_FLOOR, 100), verdict: 'PASS', calibrated: false };
  }
  const multiplier =
    run.outcome === 'scram' ? SCORING.PARTIAL_SCRAM : run.outcome === 'timeout' ? SCORING.PARTIAL_TIMEOUT : SCORING.PARTIAL_LIMIT;
  return { value: Math.max(0, Math.round(run.fractionHeld * multiplier)), verdict: 'FAIL', calibrated: false };
}

export interface FollowRun {
  onDemandFraction: number; // fraction of the drill spent within tolerance of the current target
  excursions: number;
}

export function scoreFollow(run: FollowRun): DrillScore {
  const penalty = run.excursions * SCORING.EXCURSION_PENALTY_FOLLOW;
  const value = clamp(Math.round((run.onDemandFraction / SCORING.FOLLOW_FULL_MARKS_FRAC) * 100) - penalty, 0, 100);
  return { value, verdict: value >= SCORING.FOLLOW_PASS_MARK ? 'PASS' : 'FAIL', calibrated: false };
}

// Reactor Trip Response has no scoring source in the book's own excerpted
// scoring.ts (only scoreHold's source was available for this port) -- the
// live demo names the drill and its "scored on reaction time" premise,
// but not its formula. This function is this port's own extension of the
// same named-constants/calibrated:false discipline to that shape, not a
// literal port: it reuses PASS_FLOOR as the minimum score for any
// on-time response, consistent with that constant's own documented
// meaning ("a completed run never scores below this"), and introduces no
// other new constant.
export interface TripRun {
  outcome: 'onTime' | 'lateTimeout' | 'unplannedScram';
  reactionSeconds: number; // meaningful only when outcome is 'onTime'
  reactionWindowSeconds: number;
}

export function scoreTrip(run: TripRun): DrillScore {
  if (run.outcome !== 'onTime') {
    // Late (cue expired unanswered) and unplanned (SCRAM before the cue)
    // both end the drill without a legitimate response -- scored 0 rather
    // than borrowing a partial-credit multiplier that has no stated
    // meaning for a timing drill.
    return { value: 0, verdict: 'FAIL', calibrated: false };
  }
  const promptness = clamp(1 - run.reactionSeconds / run.reactionWindowSeconds, 0, 1);
  const value = Math.round(SCORING.PASS_FLOOR + promptness * (100 - SCORING.PASS_FLOOR));
  return { value, verdict: 'PASS', calibrated: false };
}
