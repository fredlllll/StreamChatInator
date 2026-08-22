// Parses Twitch's emote position tags ("id:start-end,start-end/...") into
// positioned emotes within a message. Lives outside the component tree because
// it's pure logic (and keeps the component file fast-refresh friendly).

export class Emote {
    readonly id: string;
    readonly text: string;
    readonly startIndex: number;
    readonly endIndex: number;

    constructor(id: string, text: string, startIndex: number, endIndex: number) {
        this.id = id;
        this.text = text;
        this.startIndex = startIndex;
        this.endIndex = endIndex;
    }
}

// Constructing a Segmenter isn't free and messages render constantly; one
// instance is stateless and reusable.
const graphemeSegmenter = new Intl.Segmenter();

export function extractEmotes(
    rawEmoteSetString: string | null | undefined,
    message: string | null | undefined
): Emote[] {
    if (!rawEmoteSetString || !message) {
        return [];
    }

    // Segment message by grapheme clusters (mirrors C# StringInfo behavior)
    const graphemes: string[] = Array.from(
        graphemeSegmenter.segment(message),
        (s: Intl.SegmentData) => s.segment
    );

    const list: Emote[] = [];
    const emoteSets: string[] = rawEmoteSetString.split('/');

    for (const emoteSet of emoteSets) {
        if (!emoteSet) continue;

        const colonIndex = emoteSet.indexOf(':');
        if (colonIndex === -1) continue;

        const emoteId = emoteSet.slice(0, colonIndex);
        const positionPairs: string[] = emoteSet.slice(colonIndex + 1).split(',');

        for (const pair of positionPairs) {
            if (!pair) continue;

            const dashIndex = pair.indexOf('-');
            if (dashIndex === -1) continue;

            const num2 = parseInt(pair.slice(0, dashIndex), 10);
            const num3 = parseInt(pair.slice(dashIndex + 1), 10);

            if (Number.isNaN(num2) || Number.isNaN(num3)) continue;

            // Replicates C# stringInfo.SubstringByTextElements(0, num2 + 1).Length - 1
            const sub1 = graphemes.slice(0, num2 + 1).join('');
            const num4 = sub1.length - 1;

            // Replicates C# stringInfo.SubstringByTextElements(num2, num3 - num2 + 1)
            const text = graphemes.slice(num2, num3 + 1).join('');

            const emoteEndIndex = num4 + text.length - 1;

            list.push(new Emote(emoteId, text, num4, emoteEndIndex));
        }
    }

    return list;
}
