import { DecimalPipe } from '@angular/common';
import { Component } from '@angular/core';
import { POINT_KINETICS } from '../../core/physics/point-kinetics';
import { SolverCheck, runSolverChecks } from './solver-checks';

// Model Analysis (Ch. 14) -- fully client-side, no BFF dependency, per
// the explicit scope decision for this screen: build the book's own
// concept (verification of the point-kinetics solver, not validation
// against any real plant), not the real signal-quality endpoint, which
// is a different real thing (live telemetry trust) tracked separately.
// See solver-checks.ts's own doc comment for why this screen's checks
// are narrower than the book's -- our shared model has no discretization
// error to find, because it isn't numerically integrated.
@Component({
  selector: 'nx-model-analysis',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './model-analysis.html',
  styleUrl: './model-analysis.scss',
})
export class ModelAnalysisComponent {
  protected readonly checks: SolverCheck[] = runSolverChecks();
  protected readonly constants = POINT_KINETICS;
  protected readonly allPassed = this.checks.every((c) => c.passed);
}
