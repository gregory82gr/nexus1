import { UnitSignalReading } from '../../core/api/instrumentation-api';

// Picks the one signal this screen treats as "reactor power" for period
// derivation -- the real domain has no dedicated ReactorPower entity
// (same finding as reactor-instrumentation.ts's own doc comment), so this
// is a deliberately narrow, honest heuristic: the first signal with a
// real reading whose CategoryCode names a power-adjacent real category
// (POWER or NEUTRONICS -- both seen seeded in this codebase; flux is the
// physical proxy for power). If nothing matches, there is genuinely no
// power-like signal to derive a period from, and the screen must say so
// rather than guess from an arbitrary Tag string.
const POWER_LIKE_CATEGORY_FRAGMENTS = ['power', 'neutronics'];

export function findPowerSignal(signals: readonly UnitSignalReading[]): UnitSignalReading | null {
  return (
    signals.find(
      (s) => s.latestValue !== null && POWER_LIKE_CATEGORY_FRAGMENTS.some((fragment) => s.categoryCode.toLowerCase().includes(fragment)),
    ) ?? null
  );
}
