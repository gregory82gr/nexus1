import { Component } from '@angular/core';
import { NDT_METHODS, NdtMethodReference } from './ndt-methods-reference';

// NDT Methods (Ch. 16) -- fully client-side, no BFF call. Split out of
// asset-condition.ts's own consolidated screen on purpose: this is
// authored reference content (see ndt-methods-reference.ts's own doc
// comment), a different KIND of thing than the live asset/condition data,
// not a duplicate view over the same list.
@Component({
  selector: 'nx-ndt-methods',
  standalone: true,
  templateUrl: './ndt-methods.html',
  styleUrl: './ndt-methods.scss',
})
export class NdtMethodsComponent {
  protected readonly methods: readonly NdtMethodReference[] = NDT_METHODS;
}
