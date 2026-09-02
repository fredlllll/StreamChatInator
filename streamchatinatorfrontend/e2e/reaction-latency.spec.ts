import { test, expect, type Page } from "playwright/test";

// Standalone self-loop test: measure how fast the frontend reacts to new
// messages. Uses the real render path (ChatProvider -> Virtuoso -> ChatMessage
// components) via the harness page; no backend required.

const HARNESS = "/harness.html";

async function gotoHarness(page: Page) {
    await page.goto(HARNESS);
    // Wait until the injection API is installed. The Virtuoso list mounts on
    // the first injected event (ChatEventList renders an empty div until then),
    // so tests that need the real list drive it with __sci.measure()/inject().
    await page.waitForFunction(() => (window as any).__sci && typeof (window as any).__sci.inject === "function");
    const api = await page.evaluate(() => (window as any).__sci);
    expect(api.MAX_LIVE_EVENTS).toBeGreaterThan(0);
    await page.evaluate(() => (window as any).__sci.clear());
}

test.describe("reaction latency", () => {
    test("single message appears promptly", async ({ page }) => {
        await gotoHarness(page);

        // Measure 30 single-message deliveries, one at a time, and record the
        // DOM round-trip for each (timing happens inside the page).
        const latenciesUs: number[] = [];
        for (let i = 0; i < 30; i++) {
            const { latencyUs } = await page.evaluate(() =>
                (window as any).__sci.measure(1)
            );
            latenciesUs.push(latencyUs);
        }

        const avgUs = latenciesUs.reduce((a, b) => a + b, 0) / latenciesUs.length;
        const p95 = latenciesUs.sort((a, b) => a - b)[Math.floor(0.95 * latenciesUs.length)];
        console.log(`\n[reaction] single-message DOM latency: avg ${avgUs.toFixed(0)}us, ` +
            `p95 ${p95}us, min ${Math.min(...latenciesUs)}us, max ${Math.max(...latenciesUs)}us`);

        // The last rendered message should be present in the DOM.
        const lastSeq = await page.evaluate(() => (window as any).__sci.lastSeq);
        expect(lastSeq).toBe(30);
        // The list is virtualised so it won't render all 30 rows at once, but
        // there must be at least a visible slice.
        const mounted = await page.evaluate(() => (window as any).__sci.renderedCount);
        expect(mounted).toBeGreaterThan(1);

        // Sanity ceiling: on a healthy dev/prod machine a single React row
        // must render within ~250ms (this catches catastrophic regressions, not
        // fine-grained perf).
        expect(avgUs).toBeLessThan(250_000);
    });

    test("burst throughput - how many messages can land together", async ({ page }) => {
        await gotoHarness(page);

        // A "burst" = a batch of rows delivered in a single React commit, e.g.
        // the tail-end of a gift-sub bomb or a reconnect catch-up window.
        const burstSizes = [10, 100, 500, 1000, 5000];
        const results: { size: number; latencyMs: number; rendered: number }[] = [];

        for (const size of burstSizes) {
            const { latencyUs, renderedCount } = await page.evaluate((n) =>
                (window as any).__sci.measure(n), size);
            results.push({ size, latencyMs: latencyUs / 1000, rendered: renderedCount });
            console.log(`[burst] ${size} msgs in one commit: ${(latencyUs / 1000).toFixed(1)}ms ` +
                `to last-row render (mounted=${renderedCount})`);
        }

        // All delivered messages visible on screen (well under 20k cap here).
        const lastSeq = await page.evaluate(() => (window as any).__sci.lastSeq);
        const total = burstSizes.reduce((a, b) => a + b, 0);
        expect(lastSeq).toBe(total);

        // Each burst must complete without a multiple-second stall.
        for (const r of results) {
            expect(r.latencyMs / r.size).toBeLessThan(50); // <50ms per message at the extremes
        }
    });

    test("sustained streaming under a busy chat rate", async ({ page }) => {
        await gotoHarness(page);

        // Simulate a fast chat: push messages in chunks of 5 with a tiny inter-
        // chunk gap (per-message React commits + a frame between chunks, like
        // sequential SignalR deliveries over a real network). streamTrack records
        // the max DOM backlog the renderer ever reaches.
        const perChunk = 5;
        const total = 2000;
        const { totalMs, maxBacklog } = await page.evaluate(
            ({ count, chunk }) => (window as any).__sci.streamTrack(count, chunk, 1),
            { count: total, chunk: perChunk }
        );

        const final = await page.evaluate(() => {
            const sci = (window as any).__sci;
            return { lastSeq: sci.lastSeq, injected: sci.lastInjectedSeq };
        });

        console.log(`\n[stream] ${total} msgs @ ${perChunk}/chunk: ` +
            `${totalMs.toFixed(0)}ms wall (~${(total / (totalMs / 1000)).toFixed(0)} msg/s), ` +
            `max render backlog=${maxBacklog}`);

        // Every injected message eventually landed in the DOM (nothing lost).
        expect(final.lastSeq).toBe(total);
        // At 5 msgs per ~frame the renderer should basically keep up; a backlog
        // of the whole stream would indicate a serious stall.
        expect(maxBacklog).toBeLessThan(200);
    });
});