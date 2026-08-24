// Shared reactor model -- deliberately NOT the book's own six-delayed-
// neutron-group + xenon + thermal-feedback point-kinetics model. This is
// a single-group proportional model:
//
//   dP/dt = P * rho * GAIN     (rho = reactivity, in pcm)
//
// which is exponential growth/decay at a rate set by reactivity -- the
// same structural relationship real point-kinetics uses for reactor
// period (constant doubling/halving time for a constant reactivity,
// independent of the current power level), just without delayed
// neutrons, xenon, or thermal feedback.
//
// Lives in core/physics/ (not inside any one feature) because three
// screens share it honestly, the same way the book's own model is one
// file borrowed by both Training Mode and Reactor Kinetics: Ch. 9's
// drills simulate a whole reactor with it; Ch. 11's Kinetics screen (this
// port's features/reactor-kinetics/) applies its period formula to real
// polled telemetry instead of a simulated state; Ch. 14's Model Analysis
// (features/model-analysis/) verifies this exact implementation against
// its own documented formula, live, in the browser.
export interface ReactorState {
  powerPercent: number;
  periodSeconds: number | null; // null = critical / not changing meaningfully
  scrammed: boolean;
}

export const POINT_KINETICS = {
  // Illustrative, chosen for Training Mode's own drill pacing (a few
  // seconds to steer 10-20% of power) -- not derived from any reactor's
  // actual reactivity worth. See model-analysis's own screen for what
  // this constant does and does not let this model claim.
  REACTIVITY_GAIN_PER_PCM_PER_SEC: 0.001,
  SCRAM_DECAY_PER_SEC: 0.35,
  MIN_POWER_PERCENT: 0,
  MAX_POWER_PERCENT: 120,
} as const;

function clamp(value: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, value));
}

export function resetReactor(initialPowerPercent: number): ReactorState {
  return { powerPercent: initialPowerPercent, periodSeconds: null, scrammed: false };
}

export function scram(state: ReactorState): ReactorState {
  return { ...state, scrammed: true, periodSeconds: null };
}

// Given a reactivity (pcm) and a time step, returns the resulting
// fractional-per-second rate: dP/dt / P. Exposed on its own because
// Model Analysis needs to compare it independently against the closed-
// form period formula, not just consume advanceReactor's bundled result.
export function fractionalRate(rodPositionPcm: number): number {
  return rodPositionPcm * POINT_KINETICS.REACTIVITY_GAIN_PER_PCM_PER_SEC;
}

export function advanceReactor(state: ReactorState, rodPositionPcm: number, dtSeconds: number): ReactorState {
  if (state.scrammed) {
    const powerPercent = Math.max(
      POINT_KINETICS.MIN_POWER_PERCENT,
      state.powerPercent * Math.exp(-POINT_KINETICS.SCRAM_DECAY_PER_SEC * dtSeconds),
    );
    return { powerPercent, periodSeconds: null, scrammed: true };
  }

  const rate = fractionalRate(rodPositionPcm);
  const powerPercent = clamp(
    state.powerPercent * Math.exp(rate * dtSeconds),
    POINT_KINETICS.MIN_POWER_PERCENT,
    POINT_KINETICS.MAX_POWER_PERCENT,
  );
  const periodSeconds = rate !== 0 ? 1 / rate : null;
  return { powerPercent, periodSeconds, scrammed: false };
}

// The period formula applied the other direction: given two REAL polled
// readings of the same signal (not a simulated state), derive the
// fractional rate and period honestly from what was actually observed --
// Ch. 11's own point ("replacing naive polling with something better").
// A naive screen would show raw percent-per-poll deltas; this computes
// the textbook rate (d ln P / dt), which is well-defined and comparable
// across different poll intervals, the way a real reactor period is.
export interface TimedReading {
  value: number;
  timestampUtc: string;
}

export function deriveRateFromReadings(previous: TimedReading, current: TimedReading): number | null {
  const dtSeconds = (new Date(current.timestampUtc).getTime() - new Date(previous.timestampUtc).getTime()) / 1000;
  if (dtSeconds <= 0 || previous.value <= 0 || current.value <= 0) return null;
  return Math.log(current.value / previous.value) / dtSeconds;
}

export function periodFromRate(rate: number | null): number | null {
  if (rate === null || rate === 0) return null;
  return 1 / rate;
}
