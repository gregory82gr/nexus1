import { statusTone } from './context-status';

describe('statusTone', () => {
  it('maps the three real ASP.NET Core HealthStatus values exactly', () => {
    expect(statusTone('Healthy')).toBe('ok');
    expect(statusTone('Degraded')).toBe('warn');
    expect(statusTone('Unhealthy')).toBe('crit');
  });

  it('never guesses a tone for an unrecognized status string', () => {
    expect(statusTone('SomethingElse')).toBe('unknown');
  });
});
