import { fidelityBandIndex, fidelityOpacity, statusTone, TONE_COLOR } from './twin-visual';

describe('statusTone', () => {
  it('recognizes the codebase-wide "Active" convention as ok', () => {
    expect(statusTone('Active')).toBe('ok');
    expect(statusTone('ACTIVE')).toBe('ok');
  });

  it('recognizes draft/validating as warn', () => {
    expect(statusTone('Draft')).toBe('warn');
    expect(statusTone('Validating')).toBe('warn');
  });

  it('recognizes retired/superseded/failed as crit', () => {
    expect(statusTone('Retired')).toBe('crit');
    expect(statusTone('Superseded')).toBe('crit');
    expect(statusTone('Failed')).toBe('crit');
  });

  it('never guesses an unrecognized status into ok/warn/crit', () => {
    expect(statusTone('Quarantined')).toBe('unknown');
  });
});

describe('fidelityBandIndex', () => {
  it('places the five documented bands in low-to-high trust order', () => {
    expect(fidelityBandIndex('Illustrative')).toBe(0);
    expect(fidelityBandIndex('Training')).toBe(1);
    expect(fidelityBandIndex('Shadow')).toBe(2);
    expect(fidelityBandIndex('Advisory-Ready')).toBe(3);
    expect(fidelityBandIndex('Validated')).toBe(4);
  });

  it('returns null for text that matches none of the five bands, rather than guessing a position', () => {
    expect(fidelityBandIndex('Beta')).toBeNull();
  });
});

describe('fidelityOpacity', () => {
  it('renders an unrecognized band faint, not confidently solid', () => {
    expect(fidelityOpacity(null)).toBe(0.35);
  });

  it('renders higher trust bands more solid', () => {
    expect(fidelityOpacity(0)).toBeLessThan(fidelityOpacity(4));
  });
});

describe('TONE_COLOR', () => {
  it('uses the console token palette, not invented hex values', () => {
    expect(TONE_COLOR.ok).toBe(0x3ddc84);
    expect(TONE_COLOR.warn).toBe(0xffb000);
    expect(TONE_COLOR.crit).toBe(0xff3b46);
    expect(TONE_COLOR.unknown).toBe(0x3c5257);
  });
});
