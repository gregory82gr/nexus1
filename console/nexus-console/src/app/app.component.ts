import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from './shared/sidebar/sidebar';
import { TopbarComponent } from './shared/topbar/topbar';

@Component({
  selector: 'nx-root',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, TopbarComponent],
  template: `
    <div class="app">
      <nx-sidebar />
      <nx-topbar />
      <main class="content"><router-outlet /></main>
    </div>
  `,
  styles: `
    .app {
      display: grid;
      grid-template-columns: 236px 1fr; /* ported verbatim */
      grid-template-rows: 58px 1fr;
      height: 100vh;
      position: relative; /* anchors the scanline overlay below */
    }
    /* Bug found live (real screenshot, not just DOM text -- see this
       screen's own evidence report): with no explicit placement, CSS
       Grid's row-major auto-placement puts the three children at
       (row1,col1)=sidebar, (row1,col2)=topbar, (row2,col1)=content --
       squeezing the router outlet into the narrow 236px column under the
       sidebar and leaving the actual 1fr area in row 2 completely empty.
       Confirmed by measuring .content's own getBoundingClientRect() before
       this fix: width 236, not the ~530+ the 1fr column actually has.
       Explicit placement below is a shell-level fix -- it was never
       specific to Overview and would have hit every future screen. */
    nx-sidebar { grid-column: 1; grid-row: 1 / 3; }
    nx-topbar { grid-column: 2; grid-row: 1; }
    .content { grid-column: 2; grid-row: 2; overflow: auto; padding: 14px; }

    /* The CRT scanline overlay deferred from Ch. 2 -- it belongs on .app,
       once .app exists to attach it to. The book's own exact scanline CSS
       was not present in the chapter excerpts available for this port;
       this is a faithful, standard recreation of the effect (a repeating
       horizontal-line gradient over the whole shell), not a verified
       byte-for-byte copy. Pointer-events are disabled so it never
       intercepts a click. */
    .app::after {
      content: '';
      position: absolute;
      inset: 0;
      pointer-events: none;
      background: repeating-linear-gradient(
        0deg,
        rgba(0, 0, 0, 0.15) 0px,
        rgba(0, 0, 0, 0.15) 1px,
        transparent 1px,
        transparent 2px
      );
      opacity: 0.35;
      z-index: 1000;
    }
  `,
})
export class AppComponent {}
