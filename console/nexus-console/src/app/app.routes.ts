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
  { path: 'training', title: 'Training Mode', data: { title: 'Training Mode', chapter: 9 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },

  // Reactor group (8)
  { path: 'core', title: 'Core', data: { title: 'Core', chapter: 10 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'rods', title: 'Control Rods', data: { title: 'Control Rods', chapter: 10 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'neutronics', title: 'Neutronics', data: { title: 'Neutronics', chapter: 11 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'kinetics', title: 'Reactor Kinetics', data: { title: 'Reactor Kinetics', chapter: 11 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'coolant', title: 'Coolant / TH', data: { title: 'Coolant / TH', chapter: 12 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'steam', title: 'Steam & Secondary', data: { title: 'Steam & Secondary', chapter: 13 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'analysis', title: 'Model Analysis', data: { title: 'Model Analysis', chapter: 14 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'reactor3d', title: '3D Reactor View', data: { title: '3D Reactor View', chapter: 15 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },

  // Rod Inspection group (2)
  { path: 'insp-overview', title: 'Inspection Overview', data: { title: 'Inspection Overview', chapter: 16 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'ndt-methods', title: 'NDT Methods', data: { title: 'NDT Methods', chapter: 16 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },

  // Personnel group (2)
  { path: 'personnel-overview', title: 'Personnel Overview', data: { title: 'Personnel Overview', chapter: 17 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
  { path: 'personnel-stress', title: 'Stress Test', data: { title: 'Stress Test', chapter: 17 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },

  // Plant Lifecycle group (3)
  { path: 'aging', title: 'Aging & Degradation', data: { title: 'Aging & Degradation', chapter: 18 }, loadComponent: () => import('./shared/placeholder/placeholder').then((m) => m.PlaceholderComponent) },
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
