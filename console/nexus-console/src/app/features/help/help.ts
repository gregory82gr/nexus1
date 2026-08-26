import { Component } from '@angular/core';

// Help & Guide (Ch. 32) -- the book's own screen needs no correction:
// pure static "about this console" scope text, no backend call, no
// computed value of any kind. Content below states this port's own real
// scope (a portfolio/demonstration console layered on a real, if
// intentionally partial, .NET backend) rather than porting the book's own
// wording verbatim, since several of its claims (about data sources,
// about what's "live") would misrepresent this specific build's own,
// separately investigated scope.
@Component({
  selector: 'nx-help',
  standalone: true,
  templateUrl: './help.html',
  styleUrl: './help.scss',
})
export class HelpComponent {}
