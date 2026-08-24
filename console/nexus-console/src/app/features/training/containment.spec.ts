import { readdirSync, readFileSync, statSync } from 'fs';
import { join } from 'path';

// Architectural, not behavioural -- Ch. 9's own test shape. This does not
// run the application; it reads the source tree and asserts a dependency
// does not exist. Worth writing exactly because the rule it protects
// (the training simulator must never touch real-plant state) is
// important, invisible at runtime, and easy to violate by accident with
// a helpful autocomplete import.
const FEATURE_DIR = join(__dirname);
const FORBIDDEN_IMPORT_FRAGMENTS = ['core/state/plant-state', 'core/api/reactor-fleet-api', 'core/api/overview-api', 'core/api/digital-twin-api'];

function collectSourceFiles(dir: string): string[] {
  const files: string[] = [];
  for (const entry of readdirSync(dir)) {
    if (entry.endsWith('.spec.ts')) continue;
    const fullPath = join(dir, entry);
    if (statSync(fullPath).isDirectory()) {
      files.push(...collectSourceFiles(fullPath));
    } else if (entry.endsWith('.ts') || entry.endsWith('.html')) {
      files.push(fullPath);
    }
  }
  return files;
}

describe('training feature containment', () => {
  it('has no import path from training into any real-plant state or API module', () => {
    const files = collectSourceFiles(FEATURE_DIR);
    expect(files.length).toBeGreaterThan(0);

    const offenders: string[] = [];
    for (const file of files) {
      const content = readFileSync(file, 'utf8');
      for (const fragment of FORBIDDEN_IMPORT_FRAGMENTS) {
        if (content.includes(fragment)) {
          offenders.push(`${file} imports "${fragment}"`);
        }
      }
    }
    expect(offenders).toEqual([]);
  });
});
