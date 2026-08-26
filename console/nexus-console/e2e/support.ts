import { Page } from '@playwright/test';

export async function runCommand(page: Page, command: string): Promise<void> {
  const input = page.locator('.cmd-input');
  await input.click();
  await input.fill(command);
  await input.press('Enter');
}
