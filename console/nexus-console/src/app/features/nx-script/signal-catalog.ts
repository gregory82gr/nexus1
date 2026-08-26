// The Phase-0 demo (gregory82gr.github.io/Nexus-1-phase-0, a pure
// client-side simulation, not connected to this backend) names 14
// signals across two tiers: dual-tier (fleet-wide, any unit) and
// point-kinetics (selected unit only). Checked each one directly against
// this real backend before writing this table (Ch. 32 investigation
// report): only power (dual-tier) and period/kin_power (point-kinetics)
// have any real backing. The other 11 are total absences -- each still
// gets a catalog entry (so the interpreter recognizes the identifier
// rather than calling it unknown) but resolves to its own specific,
// investigated gap, never a generic "not tracked" or a fabricated value.
export type SignalTier = 'fleet' | 'kinetics';

export interface SignalDef {
  readonly name: string;
  readonly tier: SignalTier;
  readonly real: boolean;
  readonly absenceReason?: string;
}

export const SIGNAL_CATALOG: readonly SignalDef[] = [
  { name: 'power', tier: 'fleet', real: true },
  {
    name: 'coolant_temp',
    tier: 'fleet',
    real: false,
    absenceReason: 'no temperature-category signal is tracked anywhere in this backend',
  },
  {
    name: 'xenon',
    tier: 'fleet',
    real: false,
    absenceReason: 'no xenon signal or concept exists anywhere in this backend',
  },
  {
    name: 'thermal_mw',
    tier: 'fleet',
    real: false,
    absenceReason: 'no MWe/thermal-power rating field exists anywhere in this backend -- only a percent-of-rated reading is tracked',
  },
  {
    name: 'electrical_mw',
    tier: 'fleet',
    real: false,
    absenceReason:
      'no electrical-power field is tracked anywhere in this backend (active power, reactive power, generator voltage, and power factor are all absent)',
  },
  {
    name: 'rod_insert',
    tier: 'fleet',
    real: false,
    absenceReason:
      'no control-rod position telemetry exists anywhere in this backend -- rod position exists only as a Training Mode simulation input, never a real reading',
  },
  {
    name: 'capacity',
    tier: 'fleet',
    real: false,
    absenceReason: 'no installed-capacity or capacity-factor field exists anywhere in this backend',
  },
  {
    name: 'online',
    tier: 'fleet',
    real: false,
    absenceReason: 'no per-unit operating-status flag exists anywhere in this backend',
  },
  { name: 'period', tier: 'kinetics', real: true },
  {
    name: 'reactivity_pcm',
    tier: 'kinetics',
    real: false,
    absenceReason: 'reactivity is not derived from real telemetry anywhere in this backend -- it exists only as a Training Mode simulation input',
  },
  {
    name: 'decay_heat',
    tier: 'kinetics',
    real: false,
    absenceReason: 'no decay-heat concept exists anywhere in this backend',
  },
  {
    name: 'fuel_temp',
    tier: 'kinetics',
    real: false,
    absenceReason: 'no temperature-category signal is tracked anywhere in this backend',
  },
  { name: 'kin_power', tier: 'kinetics', real: true },
  {
    name: 'kin_xenon',
    tier: 'kinetics',
    real: false,
    absenceReason: 'no xenon signal or concept exists anywhere in this backend',
  },
];

export function findSignal(name: string): SignalDef | undefined {
  return SIGNAL_CATALOG.find((s) => s.name === name);
}

export function absenceRefusal(signal: SignalDef): string {
  return `${signal.name}: ${signal.absenceReason}`;
}

export function offUnitRefusal(signalName: string, selectedUnitId: number, requestedUnitId: number): string {
  return `${signalName}: point-kinetics signals are only available for the currently selected unit (u${selectedUnitId}) -- run 'select u${requestedUnitId}' to switch, or drop the unit qualifier.`;
}

// The entire real BFF route table (checked directly, Program.cs) has
// exactly one write endpoint: POST alarm-management/alarms/{id}/acknowledge.
// Recognized-then-refused with a message distinct from the design-only
// refusals below, since it names a real capability this console chooses
// not to expose, not an absent one.
export const REAL_BUT_UNEXPOSED_VERBS: readonly string[] = ['acknowledge'];

// These correspond to nothing real anywhere in this backend (scram exists
// only as a Training Mode simulation action) -- refused as a pure design
// statement, the same spirit as the book's own read-only premise.
export const DESIGN_REFUSED_VERBS: readonly string[] = ['set', 'scram', 'hold', 'step', 'wait'];

export function verbRefusal(verb: string): string {
  if (REAL_BUT_UNEXPOSED_VERBS.includes(verb)) {
    return `act verb '${verb}' is a real capability (alarm acknowledgement) but is not exposed in this read-only console.`;
  }
  return `act verb '${verb}' is not available in the read-only console.`;
}
