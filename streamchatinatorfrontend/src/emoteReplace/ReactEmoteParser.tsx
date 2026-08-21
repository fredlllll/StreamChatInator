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

    /**
     * `keyBase` disambiguates keys when one message is parsed in multiple
     * segments: two segments could otherwise emit the same code at the same
     * token index and produce duplicate React keys. Callers pass a value
     * unique to the segment (e.g. its character offset in the full message).
     */
    public parse(text: string, keyBase: number | string = ""): ReactNode[] {
        const tokens = text.split(/(\s+)/);

        return tokens.flatMap((token, index) => {
            if (!token) return [token];

            // Exact match first: covers codes that contain non-word characters.
            if (this.emotes.has(token)) {
                return [this.renderEmote(token, index, keyBase)];
            }

            // Otherwise try to match a word part surrounded by punctuation,
            // e.g. "PogChamp!" -> code "PogChamp", keeping the punctuation as text.
            const match = token.match(/^([^\p{L}\p{N}_]*)([\p{L}\p{N}_]+)([^\p{L}\p{N}_]*)$/u);
            if (match && this.emotes.has(match[2])) {
                const nodes: ReactNode[] = [];
                if (match[1]) nodes.push(match[1]);
                nodes.push(this.renderEmote(match[2], index, keyBase));
                if (match[3]) nodes.push(match[3]);
                return nodes;
            }

            return [token];
        });
    }

    private renderEmote(code: string, index: number, keyBase: number | string): ReactNode {
        return (
            <img
                key={`${keyBase}-${code}-${index}`}
                src={this.emotes.get(code)!}
                alt={code}
                title={code}
                className="inline-emote"
            />
        );
    }
}