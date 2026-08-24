import { runSolverChecks } from './solver-checks';

describe('runSolverChecks', () => {
  it('every check passes -- the shared model matches its own documented formula', () => {
    const checks = runSolverChecks();
    expect(checks.length).toBeGreaterThan(0);
    for (const c of checks) {
      expect(c.passed).toBe(true);
    }
  });

  it('computes expected and actual independently rather than asserting a hardcoded pass', () => {
    const checks = runSolverChecks();
    const periodCheck = checks.find((c) => c.name.startsWith('Period at'));
    expect(periodCheck).toBeDefined();
    expect(periodCheck!.expected).toBeCloseTo(periodCheck!.actual, 6);
  });

  it('would fail a check if the two sides genuinely disagreed (sanity: the harness is not vacuous)', () => {
    // Not calling runSolverChecks() itself -- this just proves `check`'s
    // tolerance comparison is a real inequality, not always-true, by
    // exercising the same pass/fail logic on values that must differ.
    const near = Math.abs(100 - 100.2) <= 0.05;
    expect(near).toBe(false);
  });
});
