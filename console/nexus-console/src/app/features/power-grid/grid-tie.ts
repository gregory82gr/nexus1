import { UnitSignalReading } from '../../core/api/instrumentation-api';

// Ch. 21's own root-cause finding is a fabricated RELATIONSHIP, not a
// fabricated value: the book's original source computes grid frequency
// from local turbine RPM (hz = 50 + (rpm-3000)/60) -- physically
// backwards, since a synchronized grid has ONE shared frequency that
// every connected turbine tracks, not one that any single turbine sets.
// The book's own fix is structural: turbineSpeedRpm stays a real, local
// measurement; gridFrequencyHz becomes a separate, unconnected field
// (it needs external point-of-common-coupling telemetry this plant does
// not have); phaseAngleDeg/breakerClosed/inSync were hardcoded constants
// in the book's own source, so they carry no computed value at all.
//
// GUARD, matching the book's own: no function in this file, or anywhere
// in this feature, may derive gridFrequencyHz (or phaseAngleDeg,
// breakerClosed, inSync) from turbineSpeedRpm or from each other. The
// absence of that function is the fix, not a detail below it -- if a
// future change adds one, it has broken the one thing this chapter is
// about.
export type TurbineSpeedReading =
  | { source: 'measured'; rpm: number; timestampUtc: string | null }
  | { source: 'no-signal' };

export interface GridTie {
  turbineSpeedRpm: TurbineSpeedReading;
  gridFrequencyHz: { source: 'awaiting-telemetry' };
  phaseAngleDeg: { source: 'no-source' };
  breakerClosed: { source: 'no-source' };
  inSync: { source: 'no-source' };
}

// The one real category seeded for this slice (Instrumentation's own
// generic Signal/Measurement model, extended the same way NEUTRONICS was
// for the Reactor cluster -- a new SignalCategory + Signal row, not new
// domain code). Checked directly before this slice: active power,
// reactive power, generator voltage, and power factor have never been
// seeded, tested, or live-verified anywhere in this system either -- they
// are declared absent alongside the grid-tie fields, not shown as if
// real.
const TURBINE_CATEGORY_CODE = 'TURBINE';

export function buildGridTie(signals: readonly UnitSignalReading[]): GridTie {
  const turbineSignal = signals.find((s) => s.categoryCode === TURBINE_CATEGORY_CODE);
  const turbineSpeedRpm: TurbineSpeedReading =
    turbineSignal && turbineSignal.latestValue !== null
      ? { source: 'measured', rpm: turbineSignal.latestValue, timestampUtc: turbineSignal.latestTimestampUtc }
      : { source: 'no-signal' };

  return {
    turbineSpeedRpm,
    gridFrequencyHz: { source: 'awaiting-telemetry' },
    phaseAngleDeg: { source: 'no-source' },
    breakerClosed: { source: 'no-source' },
    inSync: { source: 'no-source' },
  };
}
