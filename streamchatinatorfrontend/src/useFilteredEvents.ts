import { useEffect, useState, useMemo } from "react";
import { useChatConnection } from "./ChatContext";
import { getFilterById, getFilterHistory } from "./api/filtersApi";
import { compileFilter } from "./filterMatcher";
import type { FrontEndEventData, EventFilter } from "./types";

// Global "slot" of the oldest event we currently hold. Prepending older
// batches lowers it; Virtuoso uses it as an anchor so the scroll position
// stays put after a prepend, which in turn lets `startReached` fire again on
// the next scroll-to-top (without it, scrollback only ever triggers once).
const FIRST_ITEM_INDEX_OFFSET = 1_000_000;

export function useFilteredEvents(filterId: string | undefined) {
    const [filter, setFilter] = useState<EventFilter | null>(null);
    const [history, setHistory] = useState<FrontEndEventData[]>([]);
    const [nextCursor, setNextCursor] = useState<string | null>(null);
    const [hasMore, setHasMore] = useState(true);
    const [firstItemIndex, setFirstItemIndex] = useState(FIRST_ITEM_INDEX_OFFSET);
    const [loadingOlder, setLoadingOlder] = useState(false);
    const { connectedAt, events, seenState, registerSeen } = useChatConnection();

    useEffect(() => {
        if (!filterId) return;
        getFilterById(filterId).then(setFilter);
    }, [filterId]);

    useEffect(() => {
        if (!filter || !connectedAt) return;
        getFilterHistory(filter.id, connectedAt.toISOString(), 50).then((res) => {
            setHistory([...res.events].reverse());
            setNextCursor(res.nextCursor);
            setHasMore(res.hasMore);
            setFirstItemIndex(FIRST_ITEM_INDEX_OFFSET);
            res.events.forEach((e) => registerSeen(e.eventId, e.seen));
        });
    }, [filter, connectedAt, registerSeen]);

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

    const matcher = useMemo(
        () => (filter ? compileFilter(filter.codeJs) : null),
        [filter]
    );

    // The `seen` flag on the stored envelopes is stale once the user toggles
    // it (seen state lives in ChatContext). Overlay the current seen state so
    // filters reading `eventData.seen` re-evaluate when seen changes. History
    // is only filtered server-side at request time, so re-run the matcher on
    // history too — otherwise seen/unseen changes only apply after a refresh.
    const seenOf = (e: FrontEndEventData) => seenState[e.eventId] ?? e.seen;

    const filteredLive = useMemo(
        () => (matcher ? events.filter((e) => matcher({ ...e, seen: seenOf(e) })) : []),
        [events, matcher, seenState]
    );
    const filteredHistory = useMemo(
        () => (matcher ? history.filter((e) => matcher({ ...e, seen: seenOf(e) })) : history),
        [history, matcher, seenState]
    );
    const allEvents = useMemo(
        () => [...filteredHistory, ...filteredLive],
        [filteredHistory, filteredLive]
    );

    return { filter, allEvents, hasMore, firstItemIndex, loadOlder };
}