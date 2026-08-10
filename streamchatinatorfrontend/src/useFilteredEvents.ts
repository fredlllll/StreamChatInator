import { useEffect, useState, useMemo } from "react";
import { useChatConnection } from "./ChatContext";
import { getFilterById, getFilterHistory } from "./api/filtersApi";
import { compileFilter } from "./filterMatcher";
import type { FrontEndEventData, EventFilter } from "./types";

export function useFilteredEvents(filterId: string | undefined) {
    const [filter, setFilter] = useState<EventFilter | null>(null);
    const [history, setHistory] = useState<FrontEndEventData[]>([]);
    const [nextCursor, setNextCursor] = useState<string | null>(null);
    const [hasMore, setHasMore] = useState(true);
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
            res.events.forEach((e) => registerSeen(e.eventId, e.seen));
        });
    }, [filter, connectedAt, registerSeen]);

    async function loadOlder() {
        if (!filter || !nextCursor) return;
        const res = await getFilterHistory(filter.id, nextCursor, 50);
        setHistory((prev) => [...[...res.events].reverse(), ...prev]);
        setNextCursor(res.nextCursor);
        setHasMore(res.hasMore);
        res.events.forEach((e) => registerSeen(e.eventId, e.seen));
    }

    const matcher = useMemo(
        () => (filter ? compileFilter(filter.codeJs) : null),
        [filter]
    );

    // The `seen` flag on the stored envelopes is stale once the user toggles
    // it (seen state lives in ChatContext). Overlay the current seen state so
    // filters reading `eventData.seen` re-evaluate when seen changes.
    const seenOf = (e: FrontEndEventData) => seenState[e.eventId] ?? e.seen;

    const filteredLive = useMemo(
        () => (matcher ? events.filter((e) => matcher({ ...e, seen: seenOf(e) })) : []),
        [events, matcher, seenState]
    );
    const allEvents = useMemo(
        () => [...history.map((e) => ({ ...e, seen: seenOf(e) })), ...filteredLive],
        [history, filteredLive, seenState]
    );

    return { filter, allEvents, hasMore, loadOlder };
}