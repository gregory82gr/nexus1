import { Routes } from '@angular/router';

// Thirty-nine routes (Appendix A's real screen count -- the book's own
// running "map slot" counter reads 33 because six grouped sections are
// tracked as one chapter-topic each; the route table itself has all
// thirty-nine leaf paths), in the sitemap's own declared order, each path
// identical to index.html's original data-p value -- a deliberate
// compatibility decision, not laziness.
//
// Named gap: every entry points at the same shared PlaceholderComponent
// (via withComponentInputBinding, route `data` -> `title`/`chapter`
// inputs) rather than at thirty-nine separate per-feature files under
// features/. The book's own abridged listing shows each route eventually
// importing its own feature file once that screen is built; since nothing
// is built yet in this chapter, thirty-nine near-identical one-line
// wrapper files would be mechanical duplication for zero behavioral
// difference over one shared component -- features/ stays empty here,
// consistent with Ch. 2's own "only styles/ is populated" discipline,
// and gets populated screen by screen starting with Plant Fleet.
export const routes: Routes = [
  { path: '', redirectTo: 'overview', pathMatch: 'full' },

  // Plant Overview (Ch. 6) -- the second screen with a real backend call.
  { path: 'overview', title: 'Plant Overview', loadComponent: () => import('./features/overview/overview').then((m) => m.OverviewComponent) },
  // Plant Fleet (Ch. 7) -- the first screen with a real, wired-up backend
  // call, replacing the placeholder.
  { path: 'fleet', title: 'Plant Fleet', loadComponent: () => import('./features/fleet/fleet').then((m) => m.FleetComponent) },
  // Plant 3D View (Ch. 8) -- reshaped around the real per-unit digital-twin
  // endpoint rather than the book's fleet-wide physical plant scene; see
  // features/plant-3d/plant-3d.ts's own doc comment for the full reasoning.
  { path: 'plant3d', title: 'Plant 3D View', loadComponent: () => import('./features/plant-3d/plant-3d').then((m) => m.Plant3dComponent) },
  // Training Mode (Ch. 9) -- genuinely self-contained, confirmed before
  // building: no BFF call anywhere in the feature, no shared service or
  // type with any real-plant screen (features/training/drill-store.ts's
  // own doc comment, and containment.spec.ts).
  { path: 'training', title: 'Training Mode', loadComponent: () => import('./features/training/training').then((m) => m.TrainingComponent) },

  // Reactor group (8). Core, Control Rods, Neutronics, Coolant/TH, and
  // Steam & Secondary all point at the same ReactorInstrumentationComponent
  // -- consolidated after investigation found the real backend has one
  // generic signal feed behind all five, not five distinct groupings (see
  // features/reactor-instrumentation/reactor-instrumentation.ts's own doc
  // comment). `focusLabel` is bound via withComponentInputBinding() from
  // each route's own `data` -- orientation only, not a data filter.
  { path: 'core', title: 'Core', data: { focusLabel: 'Core' }, loadComponent: () => import('./features/reactor-instrumentation/reactor-instrumentation').then((m) => m.ReactorInstrumentationComponent) },
  { path: 'rods', title: 'Control Rods', data: { focusLabel: 'Control Rods' }, loadComponent: () => import('./features/reactor-instrumentation/reactor-instrumentation').then((m) => m.ReactorInstrumentationComponent) },
  { path: 'neutronics', title: 'Neutronics', data: { focusLabel: 'Neutronics' }, loadComponent: () => import('./features/reactor-instrumentation/reactor-instrumentation').then((m) => m.ReactorInstrumentationComponent) },
  // Reactor Kinetics stays its own real screen -- not a different backend
  // source (same one signals endpoint), but genuine client-side work the
  // others don't need: deriving reactor period from real polled readings
  // via core/physics/point-kinetics.ts, instead of a raw percent delta.
  { path: 'kinetics', title: 'Reactor Kinetics', loadComponent: () => import('./features/reactor-kinetics/reactor-kinetics').then((m) => m.ReactorKineticsComponent) },
  { path: 'coolant', title: 'Coolant / TH', data: { focusLabel: 'Coolant / TH' }, loadComponent: () => import('./features/reactor-instrumentation/reactor-instrumentation').then((m) => m.ReactorInstrumentationComponent) },
  { path: 'steam', title: 'Steam & Secondary', data: { focusLabel: 'Steam & Secondary' }, loadComponent: () => import('./features/reactor-instrumentation/reactor-instrumentation').then((m) => m.ReactorInstrumentationComponent) },
  // Model Analysis (Ch. 14) -- built as the book intends: fully
  // client-side solver verification, no BFF call. The real signal-quality
  // endpoint stays unused here; it's a genuinely different real thing
  // (live telemetry trust), tracked separately for a future, honestly
  // named screen rather than shoehorned into this one.
  { path: 'analysis', title: 'Model Analysis', loadComponent: () => import('./features/model-analysis/model-analysis').then((m) => m.ModelAnalysisComponent) },
  { path: 'reactor3d', title: '3D Reactor View', data: { title: '3D Reactor View', chapter: 15 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },

  // Rod Inspection group (2)
  // Rod Inspection cluster (Ch. 16) -- Inspection Overview, NDT Methods,
  // and Rod Type/Film all map to Maintenance's one real generic
  // asset/condition endpoint; Rod Type/Film is not built at all (nothing
  // real to show -- see asset-condition.ts's own doc comment). NDT
  // Methods is genuinely static reference content, not a duplicate view
  // over the live list, so it gets its own component.
  { path: 'insp-overview', title: 'Inspection Overview', loadComponent: () => import('./features/asset-condition/asset-condition').then((m) => m.AssetConditionComponent) },
  { path: 'ndt-methods', title: 'NDT Methods', loadComponent: () => import('./features/ndt-methods/ndt-methods').then((m) => m.NdtMethodsComponent) },

  // Personnel group (2)
  // Personnel (Ch. 17) -- department-scoped (core/state/department-state.ts),
  // deliberately rendering less than the real data allows: no names are
  // fetched into either screen's own component state, matching the
  // book's own minimization argument. Sector Roster (the book's "one
  // screen that needs names") is not built -- it needs a real route
  // guard, and none exists yet in this console.
  { path: 'personnel-overview', title: 'Personnel Overview', loadComponent: () => import('./features/personnel-overview/personnel-overview').then((m) => m.PersonnelOverviewComponent) },
  { path: 'personnel-stress', title: 'Stress Test', loadComponent: () => import('./features/absence-stress-test/absence-stress-test').then((m) => m.AbsenceStressTestComponent) },

  // Plant Lifecycle group (3)
  // Plant Lifecycle (Ch. 18). Ageing & Degradation is real (real
  // DegradationRecord/DegradationTrendPoint data, unlike the book's own
  // fully-generated source) and gets a real screen. Decommissioning and
  // Waste & Spent Fuel are not built at all -- checked directly, neither
  // has any entity anywhere in Maintenance's domain, a total-absence
  // gap like Security's own zone-access finding, not missing fields.
  { path: 'aging', title: 'Aging & Degradation', loadComponent: () => import('./features/ageing-degradation/ageing-degradation').then((m) => m.AgeingDegradationComponent) },
  { path: 'decommissioning', title: 'Decommissioning', data: { title: 'Decommissioning', chapter: 18 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'waste', title: 'Waste & Spent Fuel', data: { title: 'Waste & Spent Fuel', chapter: 18 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },

  // Robotics & Vehicles group (2)
  { path: 'robotics-overview', title: 'Robotics Fleet Overview', data: { title: 'Robotics Fleet Overview', chapter: 19 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'robotics-readiness', title: 'Mission Readiness', data: { title: 'Mission Readiness', chapter: 19 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },

  // Zone Access group (2)
  { path: 'access-presence', title: 'Live Presence', data: { title: 'Live Presence', chapter: 20 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'access-matrix', title: 'Permissions Matrix', data: { title: 'Permissions Matrix', chapter: 20 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },

  // remaining flat screens (16)
  { path: 'power', title: 'Power & Grid', data: { title: 'Power & Grid', chapter: 21 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'rad', title: 'Radiation / Safety', data: { title: 'Radiation / Safety', chapter: 22 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'alarms', title: 'Alarms & Events', data: { title: 'Alarms & Events', chapter: 23 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'ai', title: 'AI Diagnostics', data: { title: 'AI Diagnostics', chapter: 24 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'rlopt', title: 'Optimization (RL)', data: { title: 'Optimization (RL)', chapter: 25 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'trends', title: 'Trends & History', data: { title: 'Trends & History', chapter: 26 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'deps', title: 'System Dependencies', data: { title: 'System Dependencies', chapter: 27 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'components', title: 'Component Registry', data: { title: 'Component Registry', chapter: 28 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  // Digital Twin: never individually audited by the book's own Appendix A --
  // chapter number below is a placed-near-neighbor judgment call, not an
  // asserted fact.
  { path: 'twin', title: 'Digital Twin', data: { title: 'Digital Twin', chapter: 28 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'incident', title: 'Incident Analysis', data: { title: 'Incident Analysis', chapter: 29 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'rcgraph', title: 'Root Cause Graph', data: { title: 'Root Cause Graph', chapter: 29 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'audit', title: 'Compliance / Audit', data: { title: 'Compliance / Audit', chapter: 30 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'sec', title: 'Security / Services', data: { title: 'Security / Services', chapter: 31 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'console', title: 'NX-Script Console', data: { title: 'NX-Script Console', chapter: 32 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'help', title: 'Help & Guide', data: { title: 'Help & Guide', chapter: 32 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  // About: also never individually audited (Appendix A) -- same judgment-call caveat as Digital Twin.
  { path: 'about', title: 'About', data: { title: 'About', chapter: 32 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },

  { path: '**', redirectTo: 'overview' },
];
