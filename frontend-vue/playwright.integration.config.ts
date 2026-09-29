import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './e2e',
  testMatch: /integration\.spec\.ts/,
  fullyParallel: true,
  forbidOnly: Boolean(process.env.CI),
  retries: 0,
  reporter: process.env.CI
    ? [['github'], ['html', { open: 'never', outputFolder: 'playwright-report' }]]
    : 'list',
  outputDir: 'test-results',
  use: {
    baseURL: process.env.E2E_BASE_URL,
    locale: 'ru-RU',
    timezoneId: 'Europe/Moscow',
    viewport: { width: 1280, height: 900 },
    serviceWorkers: 'block',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'integration',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
})
