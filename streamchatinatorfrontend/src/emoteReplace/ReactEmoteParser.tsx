import type { ReactNode } from "react";

/**
 * Replaces emote codes in a plain-text segment with image tags, using
 * a code -> CDN url map produced by the backend EmoteProviderService.
 */
export class ReactEmoteParser {
    private readonly emotes: ReadonlyMap<string, string>;

    public constructor(emotes: ReadonlyMap<string, string> = new Map()) {
        this.emotes = emotes;
    }

    public parse(text: string): ReactNode[] {
        const tokens = text.split(/(\s+)/);

        return tokens.map((token, index) => {
            const url = this.emotes.get(token);
            if (!url) {
                return token;
            }

            return (
                <img
                    key={`${token}-${index}`}
                    src={url}
                    alt={token}
                    title={token}
                    className="inline-emote"
                />
            );
        });
    }
}