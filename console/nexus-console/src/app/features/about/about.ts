import { Component } from '@angular/core';

// About (Appendix A follow-up) -- pure static scope text, same pattern as
// Help & Guide (Ch. 32): no backend call, no computed value. Content
// grounded only in this project's own real, verifiable facts (real stack,
// this console's own honest-gap discipline as observed behavior across 34
// chapters, the same advisory-only scope boundary Help & Guide states) --
// never the book's own fictional Volume III framing. Copy drafted for
// review before being treated as final.
@Component({
  selector: 'nx-about',
  standalone: true,
  templateUrl: './about.html',
  styleUrl: './about.scss',
})
export class AboutComponent {}
