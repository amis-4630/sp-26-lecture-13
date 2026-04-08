import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
    testDir: "./e2e",
    timeout: 30_000,
    expect: { timeout: 5_000 },
    fullyParallel: false,
    retries: 0,
    use: {
        baseURL: "http://localhost:5173",
        trace: "on-first-retry",
    },
    projects: [
        {
            name: "chromium",
            use: { ...devices["Desktop Chrome"] },
        },
    ],
    webServer: [
        {
            command:
                "dotnet run --project ../backend/Buckeye.Lending.Api --urls http://localhost:5000",
            url: "http://localhost:5000/openapi/v1.json",
            reuseExistingServer: true,
            timeout: 30_000,
        },
        {
            command: "npm run dev",
            url: "http://localhost:5173",
            reuseExistingServer: true,
            timeout: 15_000,
        },
    ],
});
