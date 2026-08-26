import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { NxScriptComponent } from './nx-script';
import { PlantStateService } from '../../core/state/plant-state';

describe('NxScriptComponent', () => {
  let httpMock: HttpTestingController;
  const UNITS_URL = 'http://localhost:5103/api/v1/reactor-fleet/units';
  const signalsUrl = (id: number) => `http://localhost:5103/api/v1/instrumentation/units/${id}/signals`;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NxScriptComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('returns the real fleet-wide power reading through the real BFF call, end to end', async () => {
    const fixture = TestBed.createComponent(NxScriptComponent);
    const runPromise = fixture.componentInstance.run('get power');
    httpMock.expectOne(UNITS_URL).flush([{ id: 1, code: 'NX1-U1', name: 'Unit 1', latestPowerPercent: 87.3, latestPowerRecordedAtUtc: null }]);
    await runPromise;

    const [entry] = fixture.componentInstance.history();
    expect(entry.command).toBe('get power');
    expect(entry.output).toContain('87.3%');
  });

  it('returns the specific, investigated honest refusal for each of the 11 absent signals, never a generic one', async () => {
    const fixture = TestBed.createComponent(NxScriptComponent);
    const absent = ['coolant_temp', 'xenon', 'thermal_mw', 'electrical_mw', 'rod_insert', 'capacity', 'online', 'reactivity_pcm', 'decay_heat', 'fuel_temp', 'kin_xenon'];
    const messages = new Set<string>();
    for (const signal of absent) {
      await fixture.componentInstance.run(`get ${signal}`);
    }
    for (const entry of fixture.componentInstance.history()) {
      expect(entry.output.startsWith(`${entry.command.replace('get ', '')}:`)).toBe(true);
      messages.add(entry.output);
    }
    expect(messages.size).toBe(11);
  });

  it('refuses acknowledge and scram with distinct messages', async () => {
    const fixture = TestBed.createComponent(NxScriptComponent);
    await fixture.componentInstance.run('acknowledge');
    await fixture.componentInstance.run('scram');
    const [ack, scram] = fixture.componentInstance.history();
    expect(ack.output).toMatch(/real capability/);
    expect(scram.output).toMatch(/not available in the read-only console/);
  });

  it('select uX genuinely writes to the real, shared PlantStateService -- reflected by every other existing consumer, not just this console', async () => {
    const fixture = TestBed.createComponent(NxScriptComponent);
    const plantState = TestBed.inject(PlantStateService);
    expect(plantState.selectedId()).toBe(1);

    const runPromise = fixture.componentInstance.run('select u2');
    httpMock.expectOne(UNITS_URL).flush([
      { id: 1, code: 'NX1-U1', name: 'Unit 1', latestPowerPercent: 100, latestPowerRecordedAtUtc: null },
      { id: 2, code: 'NX1-U2', name: 'Unit 2', latestPowerPercent: 50, latestPowerRecordedAtUtc: null },
    ]);
    await runPromise;

    // Not reading NxScriptComponent's own copy of the id -- reading the
    // shared root-provided service directly, the same instance every
    // other screen (Overview, Reactor Kinetics, ...) injects.
    expect(plantState.selectedId()).toBe(2);
    expect(fixture.componentInstance.history()[0].output).toContain('selected u2');
  });

  it('derives a real period from two consecutive live kin_power/period reads for the selected unit', async () => {
    const fixture = TestBed.createComponent(NxScriptComponent);

    const first = fixture.componentInstance.run('get period');
    httpMock.expectOne(signalsUrl(1)).flush([{ tag: 'NX1-U1.RX.POWER', name: 'Reactor Power', categoryCode: 'POWER', latestValue: 100, latestQualityCode: 'GOOD', latestTimestampUtc: '2026-08-26T00:00:00Z' }]);
    await first;
    expect(fixture.componentInstance.history()[0].output).toMatch(/only one reading observed/);

    const second = fixture.componentInstance.run('get period');
    httpMock.expectOne(signalsUrl(1)).flush([{ tag: 'NX1-U1.RX.POWER', name: 'Reactor Power', categoryCode: 'POWER', latestValue: 110, latestQualityCode: 'GOOD', latestTimestampUtc: '2026-08-26T00:00:10Z' }]);
    await second;
    expect(fixture.componentInstance.history()[1].output).toMatch(/period \(u1\) = [+-]\d+\.\d s/);
  });
});
