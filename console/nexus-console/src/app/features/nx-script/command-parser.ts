import { DESIGN_REFUSED_VERBS, REAL_BUT_UNEXPOSED_VERBS } from './signal-catalog';

// Pure syntax layer only -- deliberately does not check the signal
// catalog. An identifier the interpreter has never heard of (`get foo`)
// is a genuinely different situation from a recognized-but-absent one
// (`get coolant_temp`): the former is a syntax-level "unknown identifier"
// the evaluator raises after a catalog lookup, never a fabricated
// "not tracked" gap message for something not even in the vocabulary.
export type CommandScope = 'bare' | 'fleet' | { readonly unit: number };

export type ParsedCommand =
  | { readonly kind: 'get'; readonly scope: CommandScope; readonly signal: string }
  | { readonly kind: 'aggregate'; readonly fn: 'sum' | 'mean' | 'max' | 'min'; readonly scope: CommandScope; readonly signal: string }
  | { readonly kind: 'select'; readonly unit: number }
  | { readonly kind: 'verb'; readonly verb: string }
  | { readonly kind: 'error'; readonly message: string }
  | { readonly kind: 'unknown'; readonly raw: string };

const GET_RE = /^get\s+(?:fleet\.([a-z_]+)|u(\d+)\.([a-z_]+)|([a-z_]+))$/;
const AGGREGATE_RE = /^(sum|mean|max|min)\(\s*(?:fleet\.([a-z_]+)|u(\d+)\.([a-z_]+)|([a-z_]+))\s*\)$/;
const SELECT_RE = /^select\s+u(\d+)$/;
const ALL_VERBS = [...REAL_BUT_UNEXPOSED_VERBS, ...DESIGN_REFUSED_VERBS];

function scopeFromMatch(fleetSignal: string | undefined, unitToken: string | undefined, unitSignal: string | undefined, bareSignal: string | undefined): { scope: CommandScope; signal: string } {
  if (fleetSignal) return { scope: 'fleet', signal: fleetSignal };
  if (unitToken && unitSignal) return { scope: { unit: Number(unitToken) }, signal: unitSignal };
  return { scope: 'bare', signal: bareSignal! };
}

export function parseCommand(rawInput: string): ParsedCommand {
  const raw = rawInput.trim();
  if (raw.length === 0) {
    return { kind: 'error', message: 'empty command.' };
  }
  const normalized = raw.toLowerCase();
  const firstToken = normalized.split(/\s+/)[0];

  if (ALL_VERBS.includes(firstToken)) {
    return { kind: 'verb', verb: firstToken };
  }

  const selectMatch = SELECT_RE.exec(normalized);
  if (selectMatch) {
    return { kind: 'select', unit: Number(selectMatch[1]) };
  }

  const aggregateMatch = AGGREGATE_RE.exec(normalized);
  if (aggregateMatch) {
    const [, fn, fleetSignal, unitToken, unitSignal, bareSignal] = aggregateMatch;
    if (!fleetSignal) {
      return { kind: 'error', message: 'aggregation only applies to fleet.<signal> arrays, e.g. sum(fleet.power).' };
    }
    return { kind: 'aggregate', fn: fn as 'sum' | 'mean' | 'max' | 'min', ...scopeFromMatch(fleetSignal, unitToken, unitSignal, bareSignal) };
  }

  const getMatch = GET_RE.exec(normalized);
  if (getMatch) {
    const [, fleetSignal, unitToken, unitSignal, bareSignal] = getMatch;
    return { kind: 'get', ...scopeFromMatch(fleetSignal, unitToken, unitSignal, bareSignal) };
  }

  return { kind: 'unknown', raw };
}
