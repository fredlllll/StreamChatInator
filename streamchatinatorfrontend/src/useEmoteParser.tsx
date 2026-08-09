import { useMemo } from "react";
import { useEmotes } from "./EmoteContext";
import { ReactEmoteParser } from "./emoteReplace/ReactEmoteParser";

export function useEmoteParser(): ReactEmoteParser {
    const { emotes } = useEmotes();
    return useMemo(() => new ReactEmoteParser(emotes), [emotes]);
}