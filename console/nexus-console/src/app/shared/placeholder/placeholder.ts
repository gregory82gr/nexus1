import { Component, input } from '@angular/core';

// The two-state rule (Ch. 3): every one of the 39 routes points here until
// its own chapter builds the real screen. "stub" (route resolves, nothing
// built) and "unreachable" (route resolves, its future data source is
// already known to be down) are deliberately loud and distinct -- a
// placeholder that looked like a finished, empty panel would be a lie a
// reader could walk past.
@Component({
  selector: 'nx-placeholder',
  standalone: true,
  template: `
    <div class="panel">
      <span class="pill" [class.info]="!failed()" [class.crit]="failed()">
        {{ failed() ? 'UNREACHABLE' : 'NOT YET BUILT' }}
      </span>
      <h2>{{ title() }}</h2>
      <p class="sub">Arrives in Chapter {{ chapter() }}.</p>
    </div>
  `,
})
export class PlaceholderComponent {
  title = input.required<string>();
  chapter = input.required<number>();
  failed = input(false);
}
