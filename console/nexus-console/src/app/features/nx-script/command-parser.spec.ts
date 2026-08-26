import { parseCommand } from './command-parser';

describe('parseCommand', () => {
  it('parses a bare get', () => {
    expect(parseCommand('get power')).toEqual({ kind: 'get', scope: 'bare', signal: 'power' });
  });

  it('parses a fleet-scoped get', () => {
    expect(parseCommand('get fleet.power')).toEqual({ kind: 'get', scope: 'fleet', signal: 'power' });
  });

  it('parses a unit-scoped get', () => {
    expect(parseCommand('get u2.period')).toEqual({ kind: 'get', scope: { unit: 2 }, signal: 'period' });
  });

  it('parses an aggregate over fleet.power', () => {
    expect(parseCommand('sum(fleet.power)')).toEqual({ kind: 'aggregate', fn: 'sum', scope: 'fleet', signal: 'power' });
    expect(parseCommand('mean(fleet.power)')).toEqual({ kind: 'aggregate', fn: 'mean', scope: 'fleet', signal: 'power' });
  });

  it('rejects an aggregate that is not fleet-scoped as a syntax error, not a silent bare aggregation', () => {
    const result = parseCommand('sum(power)');
    expect(result.kind).toBe('error');
  });

  it('parses select', () => {
    expect(parseCommand('select u2')).toEqual({ kind: 'select', unit: 2 });
  });

  it('recognizes the real-but-unexposed act verb distinctly from the design-refused ones', () => {
    expect(parseCommand('acknowledge')).toEqual({ kind: 'verb', verb: 'acknowledge' });
    expect(parseCommand('scram')).toEqual({ kind: 'verb', verb: 'scram' });
  });

  it('treats an unrecognized command as unknown, not a signal lookup', () => {
    expect(parseCommand('frobnicate the reactor')).toEqual({ kind: 'unknown', raw: 'frobnicate the reactor' });
  });

  it('rejects empty input as an error, not a silent no-op', () => {
    expect(parseCommand('   ')).toEqual({ kind: 'error', message: 'empty command.' });
  });

  it('is case-insensitive', () => {
    expect(parseCommand('GET FLEET.POWER')).toEqual({ kind: 'get', scope: 'fleet', signal: 'power' });
  });
});
