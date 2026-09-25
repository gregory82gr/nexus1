// Renders docs/guide/nexus1-programmers-guide.html to docs/NEXUS-1-Programmers-Guide.pdf.
//
// Uses the Playwright Chromium the console's E2E suite already installs, so no new tool:
//   cd console/nexus-console && npm install          (once, if node_modules is absent)
//   node docs/guide/build-guide-pdf.mjs              (from the repo root)
import { createRequire } from 'node:module';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const repo = path.resolve(here, '..', '..');
const require = createRequire(path.join(repo, 'console', 'nexus-console', 'package.json'));
const { chromium } = require('@playwright/test');

const source = path.join(here, 'nexus1-programmers-guide.html');
const output = path.join(repo, 'docs', 'NEXUS-1-Programmers-Guide.pdf');

const browser = await chromium.launch();
try {
  const page = await browser.newPage();
  await page.goto(pathToFileURL(source).href, { waitUntil: 'load' });
  await page.emulateMedia({ media: 'print' });
  await page.pdf({
    path: output,
    format: 'A4',
    printBackground: true,
    preferCSSPageSize: false,
    margin: { top: '17mm', bottom: '18mm', left: '16mm', right: '16mm' },
    displayHeaderFooter: true,
    headerTemplate: '<div></div>',
    footerTemplate:
      '<div style="width:100%;font-family:Segoe UI,Arial,sans-serif;font-size:7.5pt;color:#6e7781;padding:0 16mm;display:flex;justify-content:space-between">' +
      '<span>NEXUS-1 Programmer\'s Guide · describes commit 1fbada7</span>' +
      '<span><span class="pageNumber"></span> / <span class="totalPages"></span></span></div>',
  });
  console.log(`Wrote ${output}`);
} finally {
  await browser.close();
}
