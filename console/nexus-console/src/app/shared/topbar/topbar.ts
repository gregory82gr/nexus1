import { Component } from '@angular/core';

// Three controls the topbar draws but does not yet operate (Ch. 3's own
// honest boundary): the clock is a fixed timestamp until Ch. 4 gives it a
// signal; the unit selector lists the fleet but selecting does nothing
// until Ch. 7 makes selection global state; the alarm badge is hardcoded
// to the source file's own "3" until Ch. 20 connects the real alarm feed.
// Drawn now so the layout is final and never reflowed later.
@Component({
  selector: 'nx-topbar',
  standalone: true,
  templateUrl: './topbar.html',
  styleUrl: './topbar.scss',
})
export class TopbarComponent {
  readonly clockPlaceholder = '00:00:00';
  readonly unitPlaceholder = 'Unit 1 – PWR-900';
  readonly alarmBadgeCount = 3;
}
