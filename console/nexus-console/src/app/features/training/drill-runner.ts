import { DrillDefinition } from './drills';
import { DrillScore, scoreFollow, scoreHold, scoreTrip } from './training-scoring';
import { ReactorState } from '../../core/physics/point-kinetics';

// Pure per-tick reducer -- takes the current drill progress and the
// reactor state just produced by core/physics/point-kinetics.ts's advanceReactor, and
// decides whether the drill is still running or has just finished (and
// with what score). No signals, no timer, no framework: a spec can drive
// this tick by tick.
export interface HoldProgress {
  kind: 'hold';
  elapsedSeconds: number;
  inBandSeconds: number;
  excursions: number;
  wasInBand: boolean;
}
export interface FollowProgress {
  kind: 'follow';
  elapsedSeconds: number;
  onDemandSeconds: number;
  excursions: number;
  wasOnDemand: boolean;
}
export interface TripProgress {
  kind: 'trip';
  elapsedSeconds: number;
  cueFired: boolean;
}
export type DrillProgress = HoldProgress | FollowProgress | TripProgress;

export type DrillTickResult = { progress: DrillProgress; done: false } | { progress: DrillProgress; done: true; score: DrillScore };

export function initProgress(drill: DrillDefinition): DrillProgress {
  switch (drill.kind) {
    case 'hold':
      return { kind: 'hold', elapsedSeconds: 0, inBandSeconds: 0, excursions: 0, wasInBand: false };
    case 'follow':
      return { kind: 'follow', elapsedSeconds: 0, onDemandSeconds: 0, excursions: 0, wasOnDemand: false };
    case 'trip':
      return { kind: 'trip', elapsedSeconds: 0, cueFired: false };
  }
}

export function advanceDrill(
  drill: DrillDefinition,
  progress: DrillProgress,
  reactor: ReactorState,
  dtSeconds: number,
  scramPressedThisTick: boolean,
): DrillTickResult {
  if (drill.kind === 'hold' && progress.kind === 'hold') {
    return advanceHold(drill, progress, reactor, dtSeconds);
  }
  if (drill.kind === 'follow' && progress.kind === 'follow') {
    return advanceFollow(drill, progress, reactor, dtSeconds);
  }
  if (drill.kind === 'trip' && progress.kind === 'trip') {
    return advanceTrip(drill, progress, dtSeconds, scramPressedThisTick);
  }
  throw new Error(`Drill kind "${drill.kind}" does not match progress kind "${progress.kind}".`);
}

function advanceHold(drill: DrillDefinition, progress: HoldProgress, reactor: ReactorState, dtSeconds: number): DrillTickResult {
  const elapsedSeconds = progress.elapsedSeconds + dtSeconds;
  const inBand = Math.abs(reactor.powerPercent - drill.targetPercent!) <= drill.tolerancePercent!;
  const excursions = progress.wasInBand && !inBand ? progress.excursions + 1 : progress.excursions;
  const inBandSeconds = inBand ? progress.inBandSeconds + dtSeconds : progress.inBandSeconds;
  const next: HoldProgress = { kind: 'hold', elapsedSeconds, inBandSeconds, excursions, wasInBand: inBand };
  const fractionHeld = drill.holdSecondsRequired! > 0 ? Math.min(1, inBandSeconds / drill.holdSecondsRequired!) : 0;

  if (reactor.scrammed) {
    return { progress: next, done: true, score: scoreHold({ outcome: 'scram', excursions, fractionHeld }) };
  }
  if (drill.undershootFloorPercent !== undefined && reactor.powerPercent < drill.undershootFloorPercent) {
    return { progress: next, done: true, score: scoreHold({ outcome: 'limit', excursions, fractionHeld }) };
  }
  if (inBandSeconds >= drill.holdSecondsRequired!) {
    return { progress: next, done: true, score: scoreHold({ outcome: 'held', excursions, fractionHeld: 1 }) };
  }
  if (elapsedSeconds >= drill.timeLimitSeconds) {
    return { progress: next, done: true, score: scoreHold({ outcome: 'timeout', excursions, fractionHeld }) };
  }
  return { progress: next, done: false };
}

function currentFollowTarget(drill: DrillDefinition, elapsedSeconds: number): number {
  const schedule = drill.schedule!;
  let target = schedule[0].targetPercent;
  for (const step of schedule) {
    if (step.atSeconds <= elapsedSeconds) target = step.targetPercent;
  }
  return target;
}

function advanceFollow(drill: DrillDefinition, progress: FollowProgress, reactor: ReactorState, dtSeconds: number): DrillTickResult {
  const elapsedSeconds = progress.elapsedSeconds + dtSeconds;
  const target = currentFollowTarget(drill, elapsedSeconds);
  const onDemand = Math.abs(reactor.powerPercent - target) <= drill.tolerancePercent!;
  const excursions = progress.wasOnDemand && !onDemand ? progress.excursions + 1 : progress.excursions;
  const onDemandSeconds = onDemand ? progress.onDemandSeconds + dtSeconds : progress.onDemandSeconds;
  const next: FollowProgress = { kind: 'follow', elapsedSeconds, onDemandSeconds, excursions, wasOnDemand: onDemand };

  if (reactor.scrammed || elapsedSeconds >= drill.totalSeconds!) {
    const onDemandFraction = drill.totalSeconds! > 0 ? onDemandSeconds / drill.totalSeconds! : 0;
    return { progress: next, done: true, score: scoreFollow({ onDemandFraction, excursions }) };
  }
  return { progress: next, done: false };
}

function advanceTrip(drill: DrillDefinition, progress: TripProgress, dtSeconds: number, scramPressedThisTick: boolean): DrillTickResult {
  const elapsedSeconds = progress.elapsedSeconds + dtSeconds;
  const cueFired = progress.cueFired || elapsedSeconds >= drill.cueAtSeconds!;
  const next: TripProgress = { kind: 'trip', elapsedSeconds, cueFired };

  if (scramPressedThisTick) {
    if (!cueFired) {
      return { progress: next, done: true, score: scoreTrip({ outcome: 'unplannedScram', reactionSeconds: 0, reactionWindowSeconds: drill.reactionWindowSeconds! }) };
    }
    const reactionSeconds = elapsedSeconds - drill.cueAtSeconds!;
    const outcome = reactionSeconds <= drill.reactionWindowSeconds! ? 'onTime' : 'lateTimeout';
    return { progress: next, done: true, score: scoreTrip({ outcome, reactionSeconds, reactionWindowSeconds: drill.reactionWindowSeconds! }) };
  }
  if (cueFired && elapsedSeconds >= drill.cueAtSeconds! + drill.reactionWindowSeconds!) {
    return {
      progress: next,
      done: true,
      score: scoreTrip({ outcome: 'lateTimeout', reactionSeconds: drill.reactionWindowSeconds!, reactionWindowSeconds: drill.reactionWindowSeconds! }),
    };
  }
  return { progress: next, done: false };
}
