import { useEffect, useState, useMemo, useRef } from "react";
import { useChatConnection } from "./ChatContext";
import { getFilterByIdCached, getFilterHistory } from "./api/filtersApi";
import { compileFilter } from "./filterMatcher";
import type { FrontEndEventData, EventFilter } from "./types";

// Global "slot" of the oldest event we currently hold. Prepending older
// batches lowers it; Virtuoso uses it as an anchor so the scroll position
// stays put after a prepend, which in turn lets `startReached` fire again on
// the next scroll-to-top (without it, scrollback only ever triggers once).
const FIRST_ITEM_INDEX_OFFSET = 1_000_000;

type Matcher = (eventData: FrontEndEventData) => boolean;

const EMPTY_LIVE_STATE = { processedLen: 0, matcher: null as Matcher | null, seenVersion: 0 };

export function useFilteredEvents(filterId: string | undefined) {
    const [filter, setFilter] = useState<EventFilter | null>(null);
    const [history, setHistory] = useState<FrontEndEventData[]>([]);
    const [nextCursor, setNextCursor] = useState<string | null>(null);
    const [hasMore, setHasMore] = useState(true);
    const [firstItemIndex, setFirstItemIndex] = useState(FIRST_ITEM_INDEX_OFFSET);
    const [loadingOlder, setLoadingOlder] = useState(false);
    const { signalRConnectedAt, events, seenState, seenVersion, registerSeen, purgeVersion } = useChatConnection();

    // Accumulated filtered live list (in arrival order) + the inputs that
    // produced it. When any of the recorded inputs change we must re-evaluate
    // from scratch; when only `events` grew we evaluate just the new arrivals
    // and append the matches. The list is replaced immutably every time it
    // changes so consumers get a fresh, un-mutated array reference.
    const liveListRef = useRef<FrontEndEventData[]>([]);
    const liveInfoRef = useRef<{ processedLen: number; matcher: Matcher | null; seenVersion: number }>({ ...EMPTY_LIVE_STATE });

    useEffect(() => {
        if (!filterId) return;
        getFilterByIdCached(filterId).then(setFilter);
    }, [filterId]);

    useEffect(() => {
        if (!filter || !signalRConnectedAt) return;
        getFilterHistory(filter.id, signalRConnectedAt.toISOString(), 50).then((res) => {
            setHistory([...res.events].reverse());
            setNextCursor(res.nextCursor);
            setHasMore(res.hasMore);
            setFirstItemIndex(FIRST_ITEM_INDEX_OFFSET);
            res.events.forEach((e) => registerSeen(e.eventId, e.seen));
        });
    }, [filter, signalRConnectedAt, registerSeen]);

    // A purge drops the events server-side; the live list is cleared by
    // ChatContext, but the cached history pages here need to go too so old
    // events don't linger until the next refresh.
    useEffect(() => {
        if (purgeVersion === 0) return;
        setHistory([]);
        setNextCursor(null);
        setHasMore(true);
        setFirstItemIndex(FIRST_ITEM_INDEX_OFFSET);
        liveListRef.current = [];
        liveInfoRef.current = { ...EMPTY_LIVE_STATE, seenVersion };
    }, [purgeVersion, seenVersion]);

    async function loadOlder() {
        // `loadingOlder` guards against overlapping requests: `startReached`
        // can re-fire in quick succession and each concurrent call would
        // otherwise fetch the same page and duplicate events.
        if (!filter || !nextCursor || loadingOlder) return;
        setLoadingOlder(true);
        try {
            const res = await getFilterHistory(filter.id, nextCursor, 50);
            setHistory((prev) => [...[...res.events].reverse(), ...prev]);
            setNextCursor(res.nextCursor);
            setHasMore(res.hasMore);
            setFirstItemIndex((i) => i - res.events.length);
            res.events.forEach((e) => registerSeen(e.eventId, e.seen));
        } finally {
            setLoadingOlder(false);
        }
    }

    // Key the compiled matcher on id+updated so every tile sharing this filter
    // reuses one compiled script, and an edit recompiles automatically.
    const matcher = useMemo(
        () => (filter ? compileFilter(filter.codeJs, `${filter.id}:${filter.updated}`) : null),
        [filter]
    );

    // The `seen` flag on the stored envelopes is stale once the user toggles
    // it (seen state lives in ChatContext). Overlay the current seen state so
    // filters reading `eventData.seen` re-evaluate when seen changes. History
    // is only filtered server-side at request time, so re-run the matcher on
    // history too — otherwise seen/unseen changes only apply after a refresh.
    const seenOf = (e: FrontEndEventData) => seenState[e.eventId] ?? e.seen;

    // ---- Incremental live filtering -----------------------------------------
    // The matcher only runs over brand-new arrivals on the hot path. A full
    // re-scan happens only when something changes the outcome for *existing*
    // events: the filter itself, a seen flip (the matcher may read
    // `eventData.seen`), or a purge/reset. Without this, every new message in
    // a long chat re-ran the compiled filter over the entire backlog (O(n) per
    // message, O(n^2) overall dilution of the scrollback too).
    const info = liveInfoRef.current;
    const needFullRescan =
        info.matcher !== matcher ||
        info.seenVersion !== seenVersion ||
        info.processedLen > events.length;

    if (needFullRescan) {
        const kept: FrontEndEventData[] = [];
        for (const e of events) {
            if (matcher ? matcher({ ...e, seen: seenOf(e) }) : false) kept.push(e);
        }
        liveListRef.current = kept;
        liveInfoRef.current = { processedLen: events.length, matcher, seenVersion };
    } else if (info.processedLen < events.length) {
        const matched: FrontEndEventData[] = [];
        for (let i = info.processedLen; i < events.length; i++) {
            const e = events[i];
            if (matcher && matcher({ ...e, seen: seenOf(e) })) matched.push(e);
        }
        liveListRef.current = [...liveListRef.current, ...matched];
        liveInfoRef.current.processedLen = events.length;
    }
    const live = liveListRef.current;

    // ---- Derived history + combined list ------------------------------------
    // History is only re-filtered when the history pages, the filter, or a
    // seen flip change — not on every live delivery. Keyed on `seenVersion`
    // (flips only), not `seenState` identity, so a brand-new event arriving
    // (which adds a new seenState key) doesn't re-scan history either. The
    // matcher reads seen through `seenOf`, which reflects the render closure's
    // `seenState`.
    const filteredHistory = useMemo(
        () => (void seenVersion, matcher ? history.filter((e) => matcher({ ...e, seen: seenOf(e) })) : history),
        // exhaustive-deps can't model a version-triggered memo, so this entry
        // is suppressed: adding seenOf/seenState would re-scan on every
        // delivery, which is exactly the cost this is avoiding.
        // eslint-disable-next-line react-hooks/exhaustive-deps
        [history, matcher, seenVersion]
    );

    const allEvents = useMemo(
        () => [...filteredHistory, ...live],
        [filteredHistory, live]
    );

    return { filter, allEvents, hasMore, firstItemIndex, loadOlder };
}