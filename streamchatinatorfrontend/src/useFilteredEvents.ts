import { useEffect, useState, useMemo } from "react";
import { useChatConnection, type ChatContextType } from "./ChatContext";
import { getFilterById, getFilterHistory } from "./api/filtersApi";
import { compileFilter } from "./filterMatcher";
import type { FrontEndEventData, EventFilter } from "./types";

export function useFilteredEvents(filterId: string | undefined) {
    const [filter, setFilter] = useState<EventFilter | null>(null);
    const [history, setHistory] = useState<FrontEndEventData[]>([]);
    const [nextCursor, setNextCursor] = useState<string | null>(null);
    const [hasMore, setHasMore] = useState(true);
    const ctx: ChatContextType = useChatConnection();

    useEffect(() => {
        if (!filterId) return;
        getFilterById(filterId).then(setFilter);
    }, [filterId]);

    useEffect(() => {
        if (!filter || !ctx.connectedAt) return;
        getFilterHistory(filter.id, ctx.connectedAt.toISOString(), 50).then((res) => {
            setHistory([...res.events].reverse());
            setNextCursor(res.nextCursor);
            setHasMore(res.hasMore);
        });
    }, [filter, ctx.connectedAt]);

    async function loadOlder() {
        if (!filter || !nextCursor) return;
        const res = await getFilterHistory(filter.id, nextCursor, 50);
        setHistory((prev) => [...[...res.events].reverse(), ...prev]);
        setNextCursor(res.nextCursor);
        setHasMore(res.hasMore);
    }

    const matcher = useMemo(
        () => (filter ? compileFilter(filter.code) : null),
        [filter?.code]
    );
    const filteredLive = matcher ? ctx.events.filter((e) => matcher(e)) : [];
    const allEvents = [...history, ...filteredLive];

    return { filter, allEvents, hasMore, loadOlder };
}