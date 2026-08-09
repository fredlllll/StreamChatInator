import { useEffect, useState } from "react";
import { useChatConnection, type ChatContextType } from "./ChatContext";
import { ReactEmoteParser } from "./emoteReplace/ReactEmoteParser";
import { EmoteFetcher } from "./emoteReplace/EmoteFetcher";

export function useEmoteParser(): ReactEmoteParser {
    const [emoteParser, setEmoteParser] = useState<ReactEmoteParser>();
    const ctx: ChatContextType = useChatConnection();

    const emoteFetcher = new EmoteFetcher();
    const parser = new ReactEmoteParser(emoteFetcher);

    const globalFetch = Promise.all([
        emoteFetcher.fetchBTTVEmotes(),
        emoteFetcher.fetchSevenTVEmotes(),
        emoteFetcher.fetchFFZEmotes()
    ]);

    useEffect(() => {
        if (ctx.channelId) {
            Promise.all([
                emoteFetcher.fetchBTTVEmotes(ctx.channelId),
                emoteFetcher.fetchSevenTVEmotes(ctx.channelId),
                emoteFetcher.fetchFFZEmotes(ctx.channelId)
            ]);
        }
    }, [ctx.channelId])

    setEmoteParser(parser);

    return emoteParser;
}