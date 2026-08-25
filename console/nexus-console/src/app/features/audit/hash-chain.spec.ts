import { ChainEntry, chainEntries, computeSeal, verifyChain } from './hash-chain';

describe('hash-chain', () => {
  const RAW_ENTRIES = [
    { analysisId: '111', envelopeSha256Hex: 'aaaa1111', recordedAtUtc: '2026-08-25T09:00:00Z' },
    { analysisId: '222', envelopeSha256Hex: 'bbbb2222', recordedAtUtc: '2026-08-25T09:05:00Z' },
    { analysisId: '333', envelopeSha256Hex: 'cccc3333', recordedAtUtc: '2026-08-25T09:10:00Z' },
  ];

  it('computes a seal that depends on the previous seal, not just the entry itself', async () => {
    const sealA = await computeSeal('bbbb2222', 'seal-of-entry-1');
    const sealB = await computeSeal('bbbb2222', 'a-different-prior-seal');
    expect(sealA).not.toBe(sealB); // same entry content, different chain position
  });

  it('produces a deterministic 64-hex-char SHA-256 seal, not a formatted random number', async () => {
    const seal = await computeSeal('aaaa1111', null);
    expect(seal).toMatch(/^[0-9a-f]{64}$/);
    const again = await computeSeal('aaaa1111', null);
    expect(again).toBe(seal); // same input, same output -- never random
  });

  it('chains real entries in order, each seal built from its own content plus the previous seal', async () => {
    const chained = await chainEntries(RAW_ENTRIES);
    expect(chained).toHaveLength(3);
    const expectedSeal0 = await computeSeal(RAW_ENTRIES[0].envelopeSha256Hex, null);
    expect(chained[0].seal).toBe(expectedSeal0);
    const expectedSeal1 = await computeSeal(RAW_ENTRIES[1].envelopeSha256Hex, expectedSeal0);
    expect(chained[1].seal).toBe(expectedSeal1);
  });

  it('verifies a genuine, untampered chain as ok', async () => {
    const chained = await chainEntries(RAW_ENTRIES);
    const result = await verifyChain(chained);
    expect(result).toEqual({ ok: true, brokenAt: null });
  });

  it('detects a tampered entry via a broken chain, reporting exactly which index broke', async () => {
    const chained = await chainEntries(RAW_ENTRIES);
    const tampered: ChainEntry[] = [...chained];
    tampered[1] = { ...tampered[1], envelopeSha256Hex: 'ffff9999' }; // content altered after its seal was computed
    const result = await verifyChain(tampered);
    expect(result.ok).toBe(false);
    expect(result.brokenAt).toBe(1);
  });

  it('propagates a break forward -- altering an early entry invalidates every seal after it', async () => {
    const chained = await chainEntries(RAW_ENTRIES);
    const tampered: ChainEntry[] = [...chained];
    tampered[0] = { ...tampered[0], envelopeSha256Hex: 'ffff9999' };
    const result = await verifyChain(tampered);
    expect(result.ok).toBe(false);
    expect(result.brokenAt).toBe(0); // the earliest broken link is reported, not a later symptom
  });
});
