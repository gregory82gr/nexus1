import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { NAV, SidebarComponent, findActiveGroupLabel } from './sidebar';

@Component({ selector: 'nx-test-stub', standalone: true, template: '' })
class StubComponent {}

describe('findActiveGroupLabel (pure)', () => {
  it('finds the group containing a flat child path', () => {
    expect(findActiveGroupLabel('/core', NAV)).toBe('Reactor');
    expect(findActiveGroupLabel('/insp-overview', NAV)).toBe('Rod Inspection');
  });

  it('returns null for a top-level (non-grouped) route', () => {
    expect(findActiveGroupLabel('/overview', NAV)).toBeNull();
  });

  it('returns null when nothing matches', () => {
    expect(findActiveGroupLabel('/not-a-real-route', NAV)).toBeNull();
  });

  it('ignores query strings and fragments', () => {
    expect(findActiveGroupLabel('/core?foo=bar#frag', NAV)).toBe('Reactor');
  });
});

describe('SidebarComponent', () => {
  const testRoutes = [
    { path: 'overview', component: StubComponent },
    { path: 'core', component: StubComponent },
    { path: 'rods', component: StubComponent },
    { path: 'insp-overview', component: StubComponent },
    { path: 'ndt-methods', component: StubComponent },
  ];

  async function setup(initialUrl?: string) {
    await TestBed.configureTestingModule({
      imports: [SidebarComponent],
      providers: [provideRouter(testRoutes)],
    }).compileComponents();

    const router = TestBed.inject(Router);
    if (initialUrl) {
      await router.navigateByUrl(initialUrl);
    }
    const fixture = TestBed.createComponent(SidebarComponent);
    fixture.detectChanges();
    return { fixture, router };
  }

  it('a direct navigation onto a nested route starts with its group already open (the refresh/deep-link case)', async () => {
    const { fixture } = await setup('/core');
    const reactorGroup = fixture.componentInstance.nav.find((e) => 'label' in e && e.label === 'Reactor');
    expect(fixture.componentInstance.isOpen(reactorGroup as never)).toBe(true);
  });

  it('marks the group active even while collapsed -- the exact gap this fix addresses', async () => {
    const { fixture, router } = await setup('/core');
    const reactorGroup = fixture.componentInstance.nav.find((e) => 'label' in e && e.label === 'Reactor') as import('./sidebar').NavGroup;

    // Force it closed, simulating a user who collapsed an auto-opened group.
    fixture.componentInstance.toggle(reactorGroup);
    expect(fixture.componentInstance.isOpen(reactorGroup)).toBe(false);

    // The active indicator must not depend on the open/closed state.
    expect(fixture.componentInstance.isActiveGroup(reactorGroup)).toBe(true);
    void router; // router kept only to allow future navigation in this test if needed
  });

  it('auto-expands the target group on a later in-app navigation, without collapsing others the user opened', async () => {
    const { fixture, router } = await setup('/overview');
    const reactorGroup = fixture.componentInstance.nav.find((e) => 'label' in e && e.label === 'Reactor') as import('./sidebar').NavGroup;
    const rodGroup = fixture.componentInstance.nav.find((e) => 'label' in e && e.label === 'Rod Inspection') as import('./sidebar').NavGroup;

    fixture.componentInstance.toggle(rodGroup); // user manually opens an unrelated group
    expect(fixture.componentInstance.isOpen(rodGroup)).toBe(true);

    await router.navigateByUrl('/core');
    fixture.detectChanges();

    expect(fixture.componentInstance.isOpen(reactorGroup)).toBe(true); // newly active group opens
    expect(fixture.componentInstance.isOpen(rodGroup)).toBe(true); // manually-opened group is left alone
    expect(fixture.componentInstance.isActiveGroup(reactorGroup)).toBe(true);
    expect(fixture.componentInstance.isActiveGroup(rodGroup)).toBe(false);
  });

  it('reports no active group for a flat, non-grouped route', async () => {
    const { fixture } = await setup('/overview');
    const reactorGroup = fixture.componentInstance.nav.find((e) => 'label' in e && e.label === 'Reactor') as import('./sidebar').NavGroup;
    expect(fixture.componentInstance.isActiveGroup(reactorGroup)).toBe(false);
  });

  it('exposes a keyboard-operable, ARIA-labeled group header', async () => {
    const { fixture } = await setup('/overview');
    fixture.detectChanges();
    const header = fixture.nativeElement.querySelector('.grouphead') as HTMLElement;
    expect(header.getAttribute('tabindex')).toBe('0');
    expect(header.getAttribute('role')).toBe('button');
    expect(header.getAttribute('aria-expanded')).toBe('false');
  });
});
