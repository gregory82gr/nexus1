// Pure mapping from the real FindingSeverity.Code text to a visual tone --
// same conservative discipline as plant-3d's statusTone and
// reactor-kinetics' power-signal: Severity is a free-text lookup-table
// code (this codebase seeds different subsets per context), so an
// unrecognized string maps to 'unknown', never guessed into ok/warn/crit.
export type SeverityTone = 'ok' | 'warn' | 'crit' | 'unknown';

const CRIT_KEYWORDS = ['critical', 'severe', 'high'];
const WARN_KEYWORDS = ['medium', 'moderate'];
const OK_KEYWORDS = ['low', 'minor'];

export function severityTone(severity: string): SeverityTone {
  const s = severity.toLowerCase();
  if (CRIT_KEYWORDS.some((k) => s.includes(k))) return 'crit';
  if (WARN_KEYWORDS.some((k) => s.includes(k))) return 'warn';
  if (OK_KEYWORDS.some((k) => s.includes(k))) return 'ok';
  return 'unknown';
}
