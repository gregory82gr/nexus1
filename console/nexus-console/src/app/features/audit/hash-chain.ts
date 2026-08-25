// Ch. 30's own finding: the book's source claims "append-only,
// hash-chained ... each seal references the previous," but its seal
// function is `Math.random()` twice, formatted to look like a truncated
// hash -- it takes no argument, references nothing. Checked this
// backend directly before assuming any real chain exists to expose:
// confirmed (see the evidence report) that real SHA-256 hashing exists
// only as an isolated per-record content hash
// (AuditEvidenceRecord.EnvelopeSha256Hex, computed once, over that one
// record's own envelope bytes) -- nowhere does any entity reference a
// PREVIOUS record's hash. No chain exists anywhere server-side.
//
// So every seal computed here is a client-side construction from the
// start, not a re-derivation of something the server already chained.
// This is still honest, not fabricated: each seal is a real SHA-256
// (via the Web Crypto API) over a real, server-computed
// EnvelopeSha256Hex plus the real previous seal in this list -- genuine
// cryptographic chaining over genuine server data, just assembled here
// because the server never assembled it. The label on screen says
// exactly this: "chain verifies locally, not anchored" -- never
// "tamper-proof". A party who controls this same browser session also
// controls the only copy of the chain, so this can prove the displayed
// list is internally self-consistent; it cannot prove nothing was
// altered before this session ever saw it. A real anchored trail would
// need seals computed and stored server-side, which Volume III has no
// endpoint for -- out of scope here, same boundary the book itself
// draws.
export interface ChainEntry {
  analysisId: string;
  envelopeSha256Hex: string;
  recordedAtUtc: string;
  seal: string;
}

export interface ChainVerification {
  ok: boolean;
  brokenAt: number | null;
}

async function sha256Hex(input: string): Promise<string> {
  const bytes = new TextEncoder().encode(input);
  const digest = await crypto.subtle.digest('SHA-256', bytes);
  return Array.from(new Uint8Array(digest))
    .map((b) => b.toString(16).padStart(2, '0'))
    .join('');
}

// The one place a seal is ever produced. Takes the entry's own real
// server-computed content hash and the previous entry's seal (or null
// for the first entry) -- never anything else.
export async function computeSeal(envelopeSha256Hex: string, prevSeal: string | null): Promise<string> {
  return sha256Hex(envelopeSha256Hex + (prevSeal ?? ''));
}

// Computes a fresh seal for every entry, in order, chaining each one to
// the previous.
export async function chainEntries(
  raw: readonly { analysisId: string; envelopeSha256Hex: string; recordedAtUtc: string }[],
): Promise<ChainEntry[]> {
  const result: ChainEntry[] = [];
  let prevSeal: string | null = null;
  for (const entry of raw) {
    const seal = await computeSeal(entry.envelopeSha256Hex, prevSeal);
    result.push({ ...entry, seal });
    prevSeal = seal;
  }
  return result;
}

// Re-derives every seal from each entry's own envelopeSha256Hex plus the
// PREVIOUS entry's already-stored seal, and compares the fresh result to
// what each entry currently carries. A mismatch means this entry's
// content (or an earlier one's) changed since its seal was computed --
// exactly the property "any alteration breaks the chain" requires to be
// true, and exactly what random hex could never provide.
export async function verifyChain(entries: readonly ChainEntry[]): Promise<ChainVerification> {
  let prevSeal: string | null = null;
  for (let i = 0; i < entries.length; i++) {
    const recomputed = await computeSeal(entries[i].envelopeSha256Hex, prevSeal);
    if (recomputed !== entries[i].seal) {
      return { ok: false, brokenAt: i };
    }
    prevSeal = recomputed;
  }
  return { ok: true, brokenAt: null };
}
