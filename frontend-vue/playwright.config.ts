import { defineConfig, devices } from '@playwright/test'

const devServer = {
  command: 'pnpm dev',
  url: 'http://localhost:5173',
  reuseExistingServer: !process.env.CI,
  timeout: 120_000,
}

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: Boolean(process.env.CI),
  retries: process.env.CI ? 1 : 0,
  reporter: process.env.CI
    ? [['github'], ['html', { open: 'never', outputFolder: 'playwright-report' }]]
    : 'list',
  outputDir: 'test-results',
  webServer: [devServer],
  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:5173',
    locale: 'ru-RU',
    timezoneId: 'Europe/Moscow',
    viewport: { width: 1280, height: 900 },
    serviceWorkers: 'block',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'specs',
      testIgnore: /integration\.spec\.ts/,
      use: { ...devices['Desktop Chrome'] },
    },
  ],
})
