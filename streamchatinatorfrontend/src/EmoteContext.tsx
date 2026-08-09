import { createContext, useContext, useEffect, useRef, useState } from "react";
import { useChatConnection } from "./ChatContext";

export interface EmoteInfo {
    code: string;
    url: string;
}

interface EmoteContextValue {
    emotes: ReadonlyMap<string, string>;
    ready: boolean;
}

const EmoteContext = createContext<EmoteContextValue>({ emotes: new Map(), ready: false });

const cache = new Map<string, Promise<EmoteInfo[]>>();

function fetchEmoteList(channelKey: string): Promise<EmoteInfo[]> {
    const existing = cache.get(channelKey);
    if (existing) return existing;

    const url =
        channelKey === "global"
            ? "/api/emotes"
            : `/api/emotes?channelId=${encodeURIComponent(channelKey)}`;

    const promise = fetch(url)
        .then((response) => {
            if (!response.ok) {
                throw new Error(`emote fetch failed: ${response.status}`);
            }
            return response.json() as Promise<EmoteInfo[]>;
        })
        .catch((error: unknown) => {
            cache.delete(channelKey);
            throw error;
        });

    cache.set(channelKey, promise);
    return promise;
}

export function EmoteProvider({ children }: { children: React.ReactNode }) {
    const ctx = useChatConnection();
    const channelKey = ctx.channelId ?? "global";
    const [emotes, setEmotes] = useState<ReadonlyMap<string, string>>(new Map());
    const [ready, setReady] = useState(false);
    const fetchVersionRef = useRef(0);

    useEffect(() => {
        const version = ++fetchVersionRef.current;
        let cancelled = false;
        setReady(false);

        Promise.all([fetchEmoteList("global"), fetchEmoteList(channelKey)])
            .then(([globalList, channelList]) => {
                if (cancelled || version !== fetchVersionRef.current) return;

                const map = new Map<string, string>();
                for (const emote of channelList) map.set(emote.code, emote.url);
                for (const emote of globalList) {
                    if (!map.has(emote.code)) map.set(emote.code, emote.url);
                }

                setEmotes(map);
                setReady(true);
            })
            .catch((error: unknown) => console.error("Failed to load emotes:", error));

        return () => {
            cancelled = true;
        };
    }, [channelKey]);

    return <EmoteContext.Provider value={{ emotes, ready }}>{children}</EmoteContext.Provider>;
}

export function useEmotes(): EmoteContextValue {
    return useContext(EmoteContext);
}