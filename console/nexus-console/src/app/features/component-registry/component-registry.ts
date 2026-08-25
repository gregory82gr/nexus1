import { Component } from '@angular/core';

// Component Registry (Ch. 28) -- the book's own finding here is
// different in kind from every prior gap chapter: the SOURCE's wear
// model is real (health computed from real accumulated service years, a
// real penalty per SCRAM cycle, and a load-sensitivity term), and the
// bug is purely a disclosure-placement one (the "illustrative,
// accelerated model" note lives on one summary panel, never repeated on
// the dozens of individual component cards where a reader actually
// reads the numbers). The book's own fix is to KEEP every health bar and
// hoist the disclosure to the container level -- the same structural
// move Ch.27 made for a tab bar, generalized from a fixed set of tabs to
// an unbounded @for of cards.
//
// Checked directly, on all three of the book's real inputs, before
// assuming that premise carries over to THIS backend -- it does not, on
// every one of them:
//
// 1. Service years: `Maintenance.Asset.CommissionedAtUtc` exists as a
//    schema field, but is nullable and left unpopulated in the one asset
//    this solution has ever seeded -- real schema, not real data, and
//    not even projected by either existing Maintenance BFF DTO.
// 2. SCRAM-cycle penalty: `AlarmManagement.AlarmSeverity.Trip` exists as
//    an enum value, but has zero real occurrences anywhere in this
//    codebase -- not one AlarmEvent, in any test or any live evidence
//    session, has ever been raised with Trip severity. A count today
//    would honestly always be zero.
// 3. Load-sensitivity term: nothing in Maintenance ties to any
//    runtime/load data for this purpose at all.
// 4. The combining formula itself: every Maintenance write path checked
//    (RecordAssetConditionCommandHandler, RecordDegradationCommandHandler)
//    is pass-through persistence -- it validates and stores a value
//    someone/something already computed elsewhere; neither computes
//    anything from multiple inputs. AssetCondition.HealthScorePercent is
//    human-assessed; ActiveDegradationCaseDto.TrendPoints is a bare
//    COUNT(*), not a rate or a score.
// 5. The book's own "11 to 12 tracked components per unit" premise is
//    itself unsupported: this solution's real seed data is exactly ONE
//    asset per unit (a feedwater pump), and `AssetComponent` (the actual
//    sub-component entity below an asset) is never populated by any
//    code path in this solution.
//
// So this is not Ch.28's own situation (a real model, mis-disclosed) --
// using the book's own three-way framework (relabel / remove / hoist),
// there is no real value here to hoist a disclosure for, and no
// independently-mislabeled real value to relabel. It is closer to a
// total-absence gap, the same shape as Ch.26's availability finding: the
// retention/computation mechanism itself does not exist, not merely a
// thin instance of it.
//
// No substitute panel is built either, checked directly: the one real
// adjacent data (AssetCondition.HealthScorePercent, DegradationRecord's
// mechanism/severity/trend-point count) already has two honest homes --
// Rod Inspection (Ch. 16) and Ageing & Degradation (Ch. 18) -- both
// drawing on the exact same single real asset this investigation found.
// Showing it a third time here, framed as a "registry," would duplicate
// an existing screen's real capability rather than add a new one.
@Component({
  selector: 'nx-component-registry',
  standalone: true,
  templateUrl: './component-registry.html',
  styleUrl: './component-registry.scss',
})
export class ComponentRegistryComponent {}
