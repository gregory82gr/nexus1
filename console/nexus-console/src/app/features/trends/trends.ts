import { Component } from '@angular/core';

// Trends & History (Ch. 26) -- two concrete corrections, both investigated
// directly before writing a line of markup.
//
// 1. STORAGE NOTE: the book's own screen claims "Backed by a time-series
//    store (PostgreSQL + TimescaleDB)." Never true of this build -- this
//    solution persists exclusively through EF Core over SQL Server
//    (LocalDB in development), across every context built in this arc.
//    Corrected below to name the real stack, not the book's fictional one.
//
// 2. AVAILABILITY FIGURE: the book builds this from two retained state
//    transitions its own fictional console already produced --
//    toggleUnit() (online/offline) and recordScram() (trip, with an
//    actor). Checked solution-wide before assuming either exists here:
//
//    - ReactorFleet.Unit has no status field at all, current or
//      historical -- only Code/Name identity (ADR-003, Phase 1 slice).
//      No online/offline event of any kind is ever recorded anywhere.
//    - No scram/trip entity exists anywhere with BOTH a timestamp and an
//      actor. The one near-miss, AlarmSeverity.Trip, is a severity label
//      on an automatically-raised threshold alarm (AlarmDefinition.Evaluate)
//      -- no actor field exists for who triggered it, only who
//      acknowledged it after the fact.
//    - EventManagement models incidents/investigations/timelines linked
//      to alarms -- a real, rich context, but about a different thing
//      entirely; no SCRAM/TRIP/ONLINE/OFFLINE code was ever seeded or
//      referenced there.
//
//    This is a genuine total-absence gap, not "exists but thin": there is
//    no retention mechanism to be short on data, so this screen does NOT
//    show "insufficient history" (that phrase implies a working retention
//    mechanism whose window just hasn't filled yet). It declares NO
//    SOURCE instead, the same vocabulary Ch.6 first used for this exact
//    figure -- because that original deferral is still not honestly
//    dischargeable in this build. Building a fabricated transition log or
//    a computed percentage to fill this gap would be exactly the kind of
//    invention this project's whole discipline exists to prevent.
//
// No other real "trend" substitute exists to show instead, checked
// directly: RootCause's investigation-case history (the one BFF route an
// earlier slice's own comment labeled "Trends & History") was already
// deliberately used for AI Diagnostics (Ch. 24); ReactorFleet's real
// power-snapshot history is already shown on Overview (Ch. 6). Reusing
// either here would duplicate an existing screen, not add a new real
// capability -- so this screen carries no data panel at all, only the
// two named corrections.
@Component({
  selector: 'nx-trends',
  standalone: true,
  templateUrl: './trends.html',
  styleUrl: './trends.scss',
})
export class TrendsComponent {}
