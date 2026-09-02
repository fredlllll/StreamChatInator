import { defineConfig } from "playwright/test";

export default defineConfig({
    testDir: "./e2e",
    timeout: 180_000,
    retries: 0,
    workers: 1, // perf measurements are sensitive to contention
    use: {
        baseURL: "http://localhost:53401",
        headless: true,
        viewport: { width: 1280, height: 800 },
        trace: "off",
    },
    reporter: [["list"]],
});