// Pure mapping from the real DTO's free-text fields to a visual tone --
// testable without three.js, the same separation Ch. 8 itself insists on
// ("pure path math (testable without three.js)" for pathMeta/posAlong).
//
// Status and Fidelity are lookup-table names in the real schema
// (TwinModelStatus.cs / TwinFidelityLevel.cs), not a closed C# enum --
// each context seeds its own rows, and different component-test seed
// helpers in this repo already use different subsets ("ACTIVE"/"Active",
// "VALIDATED"/"Validated", "TRAINING"/"Training"). So an unrecognized
// string maps to 'unknown' here, never guessed into ok/warn/crit --
// misreading an unfamiliar status as safe would be worse than admitting
// it isn't understood.
export type StatusTone = 'ok' | 'warn' | 'crit' | 'unknown';

const OK_KEYWORDS = ['active'];
const WARN_KEYWORDS = ['draft', 'validating'];
const CRIT_KEYWORDS = ['retired', 'superseded', 'failed'];

export function statusTone(status: string): StatusTone {
  const s = status.toLowerCase();
  if (CRIT_KEYWORDS.some((k) => s.includes(k))) return 'crit';
  if (WARN_KEYWORDS.some((k) => s.includes(k))) return 'warn';
  if (OK_KEYWORDS.some((k) => s.includes(k))) return 'ok';
  return 'unknown';
}

// The declared fidelity band, in the order TwinFidelityLevel.cs's own
// summary comment states it, low to high trust: illustrative, training,
// shadow, advisory-ready, validated. Returns null for text that doesn't
// match one of those five words rather than guessing a position for it.
const FIDELITY_BAND = ['illustrative', 'training', 'shadow', 'advisory-ready', 'validated'];

export function fidelityBandIndex(fidelity: string): number | null {
  const f = fidelity.toLowerCase();
  const index = FIDELITY_BAND.findIndex((band) => f.includes(band));
  return index === -1 ? null : index;
}

// Colors keyed to status tone -- the console's own token palette
// (styles/_tokens.scss: --green/--amber/--red/--text-mute), not invented
// hex values.
export const TONE_COLOR: Record<StatusTone, number> = {
  ok: 0x3ddc84,
  warn: 0xffb000,
  crit: 0xff3b46,
  unknown: 0x3c5257,
};

// Opacity keyed to fidelity band: higher declared trust renders more
// solid, lower trust (or an unrecognized string) renders faint -- an
// operator should never read a low-fidelity or unrecognized model as
// trustworthy just because it happened to render brightly.
export function fidelityOpacity(bandIndex: number | null): number {
  if (bandIndex === null) return 0.35;
  return 0.35 + bandIndex * 0.13; // 0.35 .. 0.87 across the 5 known bands
}
