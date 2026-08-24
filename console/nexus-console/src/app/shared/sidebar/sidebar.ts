import { Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

export interface NavEntry {
  path: string;
  label: string;
  icon: string;
}
export interface NavGroup {
  label: string;
  children: NavEntry[];
}

// Ported from index.html's own #nav, group for group, in its own declared
// order (Ch. 1's sitemap is the visual authority; this book does not
// redesign it). Twenty flat entries and six grouped sections -- twenty-six
// top-level rows, thirty-nine routes once every group's children are
// counted.
export const NAV: (NavEntry | NavGroup)[] = [
  { path: 'overview', label: 'Overview', icon: '▣' },
  { path: 'fleet', label: 'Plant Fleet', icon: '☷' },
  { path: 'plant3d', label: 'Plant 3D View', icon: '▦' },
  { path: 'training', label: 'Training Mode', icon: '◔' },
  {
    label: 'Reactor',
    children: [
      { path: 'core', label: 'Core', icon: '' },
      { path: 'rods', label: 'Control Rods', icon: '' },
      { path: 'neutronics', label: 'Neutronics', icon: '' },
      { path: 'kinetics', label: 'Reactor Kinetics', icon: '' },
      { path: 'analysis', label: 'Model Analysis', icon: '' },
      { path: 'reactor3d', label: '3D Reactor View', icon: '' },
      { path: 'steam', label: 'Steam & Secondary', icon: '' },
      { path: 'coolant', label: 'Coolant / TH', icon: '' },
    ],
  },
  {
    label: 'Rod Inspection',
    children: [
      { path: 'insp-overview', label: 'Inspection Overview', icon: '' },
      { path: 'ndt-methods', label: 'NDT Methods', icon: '' },
    ],
  },
  {
    label: 'Personnel',
    children: [
      { path: 'personnel-overview', label: 'Overview', icon: '' },
      { path: 'personnel-stress', label: 'Stress Test', icon: '' },
    ],
  },
  {
    label: 'Plant Lifecycle',
    children: [
      { path: 'aging', label: 'Aging & Degradation', icon: '' },
      { path: 'decommissioning', label: 'Decommissioning', icon: '' },
      { path: 'waste', label: 'Waste & Spent Fuel', icon: '' },
    ],
  },
  {
    label: 'Robotics & Vehicles',
    children: [
      { path: 'robotics-overview', label: 'Fleet Overview', icon: '' },
      { path: 'robotics-readiness', label: 'Mission Readiness', icon: '' },
    ],
  },
  {
    label: 'Zone Access',
    children: [
      { path: 'access-presence', label: 'Live Presence', icon: '' },
      { path: 'access-matrix', label: 'Permissions Matrix', icon: '' },
    ],
  },
  { path: 'power', label: 'Power & Grid', icon: '' },
  { path: 'rad', label: 'Radiation / Safety', icon: '' },
  { path: 'alarms', label: 'Alarms & Events', icon: '' },
  { path: 'ai', label: 'AI Diagnostics', icon: '' },
  { path: 'rlopt', label: 'Optimization (RL)', icon: '' },
  { path: 'deps', label: 'System Dependencies', icon: '' },
  { path: 'components', label: 'Component Registry', icon: '' },
  { path: 'twin', label: 'Digital Twin', icon: '' },
  { path: 'trends', label: 'Trends & History', icon: '' },
  { path: 'incident', label: 'Incident Analysis', icon: '' },
  { path: 'rcgraph', label: 'Root Cause Graph', icon: '' },
  { path: 'audit', label: 'Compliance / Audit', icon: '' },
  { path: 'sec', label: 'Security / Services', icon: '' },
  { path: 'console', label: 'NX-Script Console', icon: '' },
  { path: 'help', label: 'Help & Guide', icon: '' },
  { path: 'about', label: 'About', icon: '' },
];

function isGroup(entry: NavEntry | NavGroup): entry is NavGroup {
  return 'children' in entry;
}

@Component({
  selector: 'nx-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class SidebarComponent {
  nav = NAV;
  isGroup = isGroup;

  // Nothing mutates NAV itself, so it isn't a service -- only which groups
  // are open is component state.
  private openGroups = signal(new Set<string>());

  isOpen(entry: NavGroup): boolean {
    return this.openGroups().has(entry.label);
  }

  toggle(entry: NavGroup): void {
    const next = new Set(this.openGroups());
    next.has(entry.label) ? next.delete(entry.label) : next.add(entry.label);
    this.openGroups.set(next);
  }
}
