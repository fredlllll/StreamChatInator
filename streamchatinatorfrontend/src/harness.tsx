import { useEffect, useRef, useState } from "react";
import { createRoot } from "react-dom/client";
import { ChatProvider, useChatState } from "./ChatContext";
import { EmoteProvider } from "./EmoteContext";
import ChatEventList from "./components/ChatEventList";
import type { FrontEndEventData, ChatEventChatMessage } from "./types";
import "./index.css";

// Standalone frontend performance harness. Mounts the REAL render path
// (ChatProvider -> ChatEventList -> Virtuoso -> ChatEventItem -> type-specific
// message components with emote parsing) with NO backend. Playwright drives it
// through a window.__sci API to measure how fast the UI reacts to new messages
// and how it behaves once the live buffer hits its cap.
//
// The buffer/trim logic below mirrors ChatContext.tsx verbatim
// (MAX_LIVE_EVENTS and the post-cap slice(1)+push path) so the cap behavior
// tested here matches production exactly. NOTE: ChatEventList renders the last
// item at the bottom only when the Virtuoso view was initialised at the bottom;
// each panel starts there.

const MAX_LIVE_EVENTS = 20_000; // must match ChatContext.MAX_LIVE_EVENTS
const FIRST_ITEM_INDEX_OFFSET = 1_000_000; // mirrors useFilteredEvents

const USERS = ["xeno", "miko", "ryl0s", "kaede", "tavic", "noire", "odin", "vesper", "cinder", "lumen"];
const WORDS = ["pog", "LUL", "KEKW", "gg", "w", "based", "cope", "clueless", "monkaS", "pepega", "fire", "letsgo", "enjoying"];

function makeMessage(seq: number): FrontEndEventData<ChatEventChatMessage> {
    const user = USERS[seq % USERS.length];
    const words: string[] = [];
    const n = 2 + (seq % 6);
    for (let i = 0; i < n; i++) words.push(WORDS[(seq + i) % WORDS.length]);
    return {
        eventId: `evt-${seq}`,
        chatEventType: "ChatMessage",
        seen: false,
        chatEventData: {
            id: `row-${seq}`,
            created: new Date(1000 + seq).toISOString(),
            updated: new Date(1000 + seq).toISOString(),
            bits: seq % 17 === 0 ? 100 : 0,
            bitsInDollars: 0,
            emotes: null,
            customRewardId: null,
            twitchMessageId: `tw-${seq}`,
            isFirstMessage: false,
            isHighlighted: seq % 97 === 0,
            isMe: false,
            isSkippingSubMode: false,
            noisy: 0,
            subscribedMonthCount: seq % 23 === 0 ? 7 : 0,
            replyParentMessageTwitchMessageId: null,
            isReply: false,
            userId: `u-${seq % USERS.length}`,
            userFlags: 0,
            badges: null,
            username: user,
            displayName: user,
            message: `msg #${seq} ` + words.join(" "),
            hexColor: "#a0c4ff",
            isBroadcaster: false,
            tmiSent: new Date(1000 + seq).toISOString(),
            userFlagsNames: [],
            userType: 0,
            userTypeName: "Viewer",
        },
    };
}

interface SciApi {
    MAX_LIVE_EVENTS: number;
    lastSeq: number;
    renderedCount: number;
    eventsStart: number;
    injectedEventsStart: number;
    receiver: null | ((batch: FrontEndEventData[] | null) => void);
    inject: (count: number) => void;
}

// Module-scope reference to the Virtuoso handle so the injection API can keep
// the list tail pinned (scroll to newest) after injecting a batch. Without
// this, a large batch mounts only the slice around the current scroll position
// and the newest rows are never rendered for measurement.
const virtuosoHandleRef: { current: any } = { current: null };

function Harness() {
    useChatState(); // keep provider mounted + reactive to seen changes
    const [events, setEvents] = useState<FrontEndEventData[]>([]);
    // Absolute number of events ever injected (mirrors ChatContext.arrivalCountRef).
    const arrivalCountRef = useRef(0);

    // Publish current buffer geometry for the DOM/cap assertions.
    useEffect(() => {
        const api = (window as any).__sci as SciApi;
        api.eventsStart = Math.max(0, arrivalCountRef.current - events.length);
        api.renderedCount = 0; // refreshed by DomStats
    }, [events]);

    // Subscriber hook: Playwright calls __sci.receiver to inject a batch.
    useEffect(() => {
        const api = (window as any).__sci as SciApi;
        api.receiver = (batch) => {
            if (!batch || batch.length === 0) {
                arrivalCountRef.current = 0;
                setEvents([]);
                return;
            }
            arrivalCountRef.current += batch.length;
            setEvents((prev) => {
                // SAME post-cap path as ChatContext: slice(1)+push = single copy.
                let next = prev;
                for (const ev of batch) {
                    if (next.length >= MAX_LIVE_EVENTS) next = next.slice(1);
                    else next = [...next];
                    next.push(ev);
                }
                return next;
            });
        };
        return () => {
            api.receiver = null;
        };
    }, []);

    return (
        <ChatEventList
            events={events}
            firstItemIndex={FIRST_ITEM_INDEX_OFFSET}
            virtuosoRef={virtuosoHandleRef}
        />
    );
}

function DomStats() {
    // Watches the real list DOM and keeps __sci.renderedCount / __sci.lastSeq in
    // sync so Playwright can (a) wait for a specific msg to appear and (b) read
    // how many items are actually mounted. Reading ids from the rendered text is
    // chosen deliberately over hooking React internals: it exercises the true DOM.
    useEffect(() => {
        const api = (window as any).__sci as SciApi;
        const update = () => {
            const items = document.querySelectorAll(".chat-event-list .chat-event-item");
            let maxSeq = -1;
            for (const el of items) {
                const m = /msg #(\d+)/.exec(el.textContent ?? "");
                if (m) {
                    const s = parseInt(m[1], 10);
                    if (s > maxSeq) maxSeq = s;
                }
            }
            api.renderedCount = items.length;
            api.lastSeq = maxSeq;
        };
        const mo = new MutationObserver(update);
        mo.observe(document.getElementById("root")!, { childList: true, subtree: true });
        update();
        return () => mo.disconnect();
    }, []);

    return null;
}

function HarnessApp() {
    return (
        <div className="app-shell" style={{ padding: 8 }}>
            <div className="page-header">
                <h2>Frontend perf harness</h2>
                <p>
                    Real ChatProvider + Virtuoso + message render path, no backend. Drive
                    from Playwright via <code>window.__sci</code>.
                </p>
            </div>
            <Harness />
            <DomStats />
        </div>
    );
}

// Keep the Virtuoso list pinned to the tail so the newest items are always
// in the DOM. Called from inside rAF loops in the injection API; Virtuoso
// no-ops if already at bottom.
function pinTail() {
    const ref = virtuosoHandleRef.current;
    if (ref?.scrollToIndex) ref.scrollToIndex({ index: "LAST", align: "end", behavior: "auto" });
}

function installApi() {
    const api: any = {
        MAX_LIVE_EVENTS,
        lastSeq: -1,
        renderedCount: 0,
        eventsStart: 0,
        receiver: null,

        // Dispatch `count` rows in a single React batch (like several SignalR
        // envelopes coalesced into one render). Returns immediately; the caller
        // waits on __sci.lastSeq via Playwright or __sci.measure.
        inject(count: number) {
            const batch: FrontEndEventData[] = [];
            const base = this.lastInjectedSeq;
            for (let i = 0; i < count; i++) batch.push(makeMessage(base + i + 1));
            this.lastInjectedSeq = base + count;
            this.receiver?.(batch);
        },

        // Dispatch rows in `perChunk`-sized batches back-to-back (relies on
        // React's automatic batching for per-chunk renders).
        stream(count: number, perChunk: number) {
            for (let offset = 0; offset < count; offset += perChunk) {
                const size = Math.min(perChunk, count - offset);
                const batch: FrontEndEventData[] = [];
                for (let i = 0; i < size; i++) {
                    batch.push(makeMessage(this.lastInjectedSeq + 1));
                    this.lastInjectedSeq += 1;
                }
                this.receiver?.(batch);
            }
        },

        // Simulate a live channel: dispatch chunks of `perChunk` rows at a time,
        // yielding a frame between chunks, and after each chunk records how far
        // the DOM lags behind the injection cursor. Resolves with per-chunk
        // backlog stats. This is the realistic "sustained stream" profile.
        async streamTrack(count: number, perChunk: number, gapMs = 0): Promise<{
            totalMs: number;
            maxBacklog: number;
            replayed: boolean;
        }> {
            const start = performance.now();
            let maxBacklog = 0;
            for (let offset = 0; offset < count; offset += perChunk) {
                const size = Math.min(perChunk, count - offset);
                const batch: FrontEndEventData[] = [];
                for (let i = 0; i < size; i++) {
                    batch.push(makeMessage(this.lastInjectedSeq + 1));
                    this.lastInjectedSeq += 1;
                }
                this.receiver?.(batch);
                // Yield so the browser can actually paint/commit before the
                // next chunk; mimics real network pacing.
                await new Promise((r) => setTimeout(r, gapMs));
                // Pin the list tail so the newest items are mounted and
                // DomStats sees the highest seq.
                pinTail();
                const rendered = this.lastSeq;
                const injected = this.lastInjectedSeq;
                maxBacklog = Math.max(maxBacklog, injected - rendered - 1);
            }
            // Final catch-up wait so the caller can treat the stream as settled.
            const target = this.lastInjectedSeq;
            await new Promise<void>((resolve) => {
                const poll = () => {
                    pinTail();
                    if (this.lastSeq >= target) resolve();
                    else requestAnimationFrame(poll);
                };
                requestAnimationFrame(poll);
            });
            return { totalMs: performance.now() - start, maxBacklog, replayed: false };
        },

        // Dispatch `count` rows and return a promise that resolves with how long
        // it took for the LAST injected row to actually appear in the DOM
        // (round-trip through React + Virtuoso), in microseconds. Timing happens
        // in-page, so it measures the real render latency without Playwright's
        // per-frame eval overhead. The tail is kept pinned via scrollToIndex("LAST")
        // each frame so the newest items are always mounted in the DOM.
        measure(count: number): Promise<{ latencyUs: number; renderedCount: number; targetSeq: number }> {
            const start = performance.now();
            const targetSeq = this.lastInjectedSeq + count;
            this.inject(count);
            return new Promise((resolve) => {
                const poll = () => {
                    pinTail();
                    if (this.lastSeq >= targetSeq) {
                        resolve({
                            targetSeq,
                            latencyUs: Math.round((performance.now() - start) * 1000),
                            renderedCount: this.renderedCount,
                        });
                    } else {
                        requestAnimationFrame(poll);
                    }
                };
                requestAnimationFrame(poll);
            });
        },

        clear() {
            this.lastInjectedSeq = 0;
            this.receiver?.(null);
        },
    };
    api.lastInjectedSeq = 0;
    (window as any).__sci = api;
}

installApi();

createRoot(document.getElementById("root")!).render(
    <ChatProvider>
        <EmoteProvider>
            <HarnessApp />
        </EmoteProvider>
    </ChatProvider>
);
