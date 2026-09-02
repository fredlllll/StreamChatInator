import { test, expect, type Page } from "playwright/test";

// Buffer-cap behavior: how the frontend behaves once the live buffer fills to
// MAX_LIVE_EVENTS (20_000). The harness mirrors ChatContext's exact trim logic
// (slice(1)+push) so the cap behavior matches production.

const HARNESS = "/harness.html";
const MAX_LIVE_EVENTS = 20_000;

async function gotoHarness(page: Page) {
    await page.goto(HARNESS);
    await page.waitForFunction(() => (window as any).__sci && typeof (window as any).__sci.inject === "function");
    await page.evaluate(() => (window as any).__sci.clear());
}

// Fill the buffer to `count` rows, keeping the Virtuoso tail pinned so the
// newest rows are actually mounted (streamTrack does this internally).
async function fill(page: Page, count: number, chunk = 2000) {
    await page.evaluate(
        ({ c, ch }) => (window as any).__sci.streamTrack(c, ch, 0),
        { c: count, ch: chunk }
    );
}

test.describe("buffer cap at 20k", () => {
    test("buffer stays at the cap and oldest events get trimmed", async ({ page }) => {
        await gotoHarness(page);
        const api = await page.evaluate(() => (window as any).__sci);
        expect(api.MAX_LIVE_EVENTS).toBe(MAX_LIVE_EVENTS);

        // Fill the buffer to exactly the cap (tail pinned throughout).
        await fill(page, MAX_LIVE_EVENTS);

        const state = await page.evaluate(() => {
            const sci = (window as any).__sci;
            return { lastSeq: sci.lastSeq, injected: sci.lastInjectedSeq, start: sci.eventsStart };
        });
        expect(state.lastSeq).toBe(MAX_LIVE_EVENTS);
        // Buffer holds exactly the cap; no trimming yet (eventsStart = 0).
        expect(state.start).toBe(0);

        // Push ONE more message: the old head (msg #1) is trimmed, buffer is
        // exactly [2..20001] still, and eventsStart ticks up to 1.
        await page.evaluate(() => (window as any).__sci.inject(1));
        await page.waitForFunction(() =>
            (window as any).__sci.lastSeq >= (window as any).__sci.lastInjectedSeq
        );
        const after = await page.evaluate(() => {
            const sci = (window as any).__sci;
            return { start: sci.eventsStart, injected: sci.lastInjectedSeq };
        });
        expect(after.injected).toBe(MAX_LIVE_EVENTS + 1);
        expect(after.start).toBe(1); // one row trimmed off the front
    });

    test("renders fine while full - new messages still stream in at the bottom", async ({ page }) => {
        await gotoHarness(page);

        // Fill to the cap, then keep streaming past it with a small per-chunk
        // gap so the browser gets a chance to paint (realistic pacing).
        const { maxBacklog } = await page.evaluate(
            ({ fillCount, extra, chunk }) =>
                (window as any).__sci.streamTrack(fillCount + extra, chunk, 1),
            { fillCount: MAX_LIVE_EVENTS, extra: 5000, chunk: 500 }
        );

        const end = await page.evaluate(() => {
            const sci = (window as any).__sci;
            return { lastSeq: sci.lastSeq, injected: sci.lastInjectedSeq, start: sci.eventsStart };
        });
        expect(end.lastSeq).toBe(MAX_LIVE_EVENTS + 5000);
        expect(end.start).toBe(5000); // exactly (injected - cap) rows trimmed

        // The renderer caught up fully (streamTrack resolves once the tail is
        // mounted); the observed peak backlog stays bounded well under the
        // whole 25k stream (i.e. it streams, it doesn't dump everything at once).
        console.log(`[cap] while full: peak render backlog = ${maxBacklog} over 500-row chunks`);
        expect(maxBacklog).toBeLessThan(5000);

        // Virtualized: only a slice is mounted, not all 25k.
        const mounted = await page.evaluate(() => (window as any).__sci.renderedCount);
        expect(mounted).toBeLessThan(5000);
        expect(mounted).toBeGreaterThan(0);
    });

    test("newest still renders at the bottom once past the cap", async ({ page }) => {
        await gotoHarness(page);

        // Fill far past the cap (tail pinned).
        await fill(page, MAX_LIVE_EVENTS + 3000);

        const state = await page.evaluate(() => {
            const sci = (window as any).__sci;
            return { start: sci.eventsStart, injected: sci.lastInjectedSeq, last: sci.lastSeq };
        });
        expect(state.start).toBe(3000); // rows 1..3000 trimmed off the front
        expect(state.injected).toBe(MAX_LIVE_EVENTS + 3000);
        expect(state.last).toBe(state.injected);

        // The newest row is mounted in the DOM (tail pinned proof).
        await expect(page.locator(".chat-event-list .chat-event-item").last()).toContainText(
            "msg #" + (MAX_LIVE_EVENTS + 3000)
        );
        // And the list stays virtualized (doesn't mount all 23k).
        const mounted = await page.evaluate(() => (window as any).__sci.renderedCount);
        expect(mounted).toBeLessThan(5000);
        expect(mounted).toBeGreaterThan(0);
    });
});