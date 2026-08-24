import { severityTone } from './severity-tone';

describe('severityTone', () => {
  it('recognizes high/critical/severe as crit', () => {
    expect(severityTone('High')).toBe('crit');
    expect(severityTone('CRITICAL')).toBe('crit');
    expect(severityTone('Severe')).toBe('crit');
  });

  it('recognizes medium/moderate as warn', () => {
    expect(severityTone('Medium')).toBe('warn');
    expect(severityTone('Moderate')).toBe('warn');
  });

  it('recognizes low/minor as ok', () => {
    expect(severityTone('Low')).toBe('ok');
    expect(severityTone('Minor')).toBe('ok');
  });

  it('never guesses an unrecognized severity into a known tone', () => {
    expect(severityTone('Deferred')).toBe('unknown');
  });
});
