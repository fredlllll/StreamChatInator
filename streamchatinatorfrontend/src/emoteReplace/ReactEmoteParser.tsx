import type { ReactNode } from "react";
import type { EmoteFetcher } from "./EmoteFetcher";

export class ReactEmoteParser {
    public fetcher: EmoteFetcher;

    public constructor(fetcher: EmoteFetcher) {
        this.fetcher = fetcher;
    }

    public parse(text: string): ReactNode[] {
        const tokens = text.split(/(\s+)/);

        return tokens.map((token, index) => {
            const emote = this.fetcher.emotes.get(token);
            if (!emote) {
                return token;
            }

            return (
                <img
                    key={emote.id}
                    src={(emote as any).toLink(0)} //because toLink is not in the d.ts files of the library
                    alt={emote.code}
                    title={emote.code}
                    className="inline-emote"
                />
            );
        });
    }
}