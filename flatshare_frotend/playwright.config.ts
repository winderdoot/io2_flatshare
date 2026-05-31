import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',

  fullyParallel: true,

  forbidOnly: !!process.env.CI,

  retries: process.env.CI ? 2 : 0,

  workers: process.env.CI ? 1 : undefined,

  reporter: 'html',

  use: {
    baseURL: 'http://127.0.0.1:5173',
    trace: 'on-first-retry',
    video: 'retain-on-failure',
    screenshot: 'only-on-failure',
    launchOptions: {
      slowMo: 50,
    },
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],

  webServer: [
    // {
    //   command: 'dotnet run --configuration Release -- CI',
    //   url: 'http://localhost:7047',
    //   timeout: 120000,
    //   reuseExistingServer: !process.env.CI,
    //   cwd: '../flatshare_server/flatshare_server',
    // },
    {
      command: 'npm run dev',
      url: 'http://localhost:5173',
      timeout: 120000,
      reuseExistingServer: !process.env.CI,
    }
  ],
});