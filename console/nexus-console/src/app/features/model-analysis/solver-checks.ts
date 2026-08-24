import { POINT_KINETICS, advanceReactor, fractionalRate, resetReactor, scram } from '../../core/physics/point-kinetics';

// Model Analysis (Ch. 14) -- fully client-side, no BFF call, per the
// explicit scope decision for this screen. The book's own Model Analysis
// verifies a numerically-INTEGRATED six-group solver against closed-form
// references (asymptotic period vs. the inhour equation, xenon
// half-lives, etc.) -- i.e. it checks for integration/discretization
// error. Our shared model (core/physics/point-kinetics.ts) has no such
// error to check: advanceReactor evaluates the exact analytic
// exponential every call, it does not numerically integrate an ODE. So
// this screen verifies something still real and still worth checking --
// that the CODE correctly implements its OWN documented formula -- and
// says plainly, in its own header, that it is not the same kind of
// verification the book's six-group solver needed. Same discipline
// either way: verification (does the code match its own equations, ever
// answerable without any plant data) is not validation (do the equations
// match a real reactor, which nothing on this screen or any other
// attempts, since this model's constants are illustrative, not
// commissioning data from any real unit).
export interface SolverCheck {
  name: string;
  expected: number;
  actual: number;
  toleranceAbs: number;
  passed: boolean;
  note: string;
}

function check(name: string, expected: number, actual: number, toleranceAbs: number, note: string): SolverCheck {
  return { name, expected, actual, toleranceAbs, passed: Math.abs(expected - actual) <= toleranceAbs, note };
}

// Every value here is computed live by calling the real functions this
// app actually runs (Training Mode's drills, Reactor Kinetics' period
// display) -- nothing is a pre-computed literal presented as a check.
export function runSolverChecks(): SolverCheck[] {
  const checks: SolverCheck[] = [];

  const critical = advanceReactor(resetReactor(100), 0, 10);
  checks.push(check('Critical at ρ = 0 pcm', 100, critical.powerPercent, 1e-9, 'Power must not drift with zero reactivity.'));

  const rho1 = 50;
  const held = advanceReactor(resetReactor(100), rho1, 1);
  checks.push(
    check(`Period at ρ = ${rho1} pcm equals 1 / (ρ · GAIN)`, 1 / fractionalRate(rho1), held.periodSeconds ?? NaN, 1e-9, "The model's own defining formula, computed live."),
  );

  const periodAt25 = 1 / fractionalRate(25);
  const periodAt50 = 1 / fractionalRate(50);
  checks.push(check('Doubling ρ halves the period', periodAt25 / 2, periodAt50, 1e-9, 'Direct consequence of a linear reactivity-to-rate model.'));

  const rho2 = 30;
  const dt = 4;
  const grown = advanceReactor(resetReactor(100), rho2, dt);
  checks.push(
    check(
      `Power at ρ = ${rho2} pcm, t = ${dt}s matches P0 · e^(ρ·GAIN·t)`,
      100 * Math.exp(fractionalRate(rho2) * dt),
      grown.powerPercent,
      1e-6,
      'The exact exponential is evaluated directly -- no numerical integration happens here, so there is no discretization error to find (unlike the book\'s own RK4-integrated six-group model).',
    ),
  );

  const halfLifeSeconds = Math.log(2) / POINT_KINETICS.SCRAM_DECAY_PER_SEC;
  const decayed = advanceReactor(scram(resetReactor(100)), 0, halfLifeSeconds);
  checks.push(
    check(
      'SCRAM decay halves power at its own documented half-life',
      50,
      decayed.powerPercent,
      0.05,
      `Half-life = ln(2) / ${POINT_KINETICS.SCRAM_DECAY_PER_SEC} ≈ ${halfLifeSeconds.toFixed(2)}s.`,
    ),
  );

  return checks;
}
