import type { ReactEmoteParser } from "../emoteReplace/ReactEmoteParser";
import { useEmoteParser } from "../useEmoteParser";
import { isString } from "../util";

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

export function extractEmotes(
    rawEmoteSetString: string | null | undefined,
    message: string | null | undefined
): Emote[] {
    if (!rawEmoteSetString || !message) {
        return [];
    }

    // Segment message by grapheme clusters (mirrors C# StringInfo behavior)
    const segmenter = new Intl.Segmenter();
    const graphemes: string[] = Array.from(
        segmenter.segment(message),
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

type EmoteReplacedMessageProps = {
    emotes: string|null,
    text:string,
};
function EmoteReplacedMessage({ emotes, text }: EmoteReplacedMessageProps) {
    const emoteParser: ReactEmoteParser = useEmoteParser();

    // Slice out native Twitch emotes by their positions first; the plain-text
    // segments left in between go through the external (BTTV/7TV/FFZ) parser.
    const elements: React.ReactNode[] = [];
    const emoteList = extractEmotes(emotes, text).sort((a, b) => a.startIndex - b.startIndex);

    let lastIndex = 0;
    emoteList.forEach((emote) => {
        if (emote.startIndex > lastIndex) {
            elements.push(text.slice(lastIndex, emote.startIndex));
        }

        elements.push(
            <img
                key={`twitch-${emote.id}-${emote.startIndex}`}
                src={`https://static-cdn.jtvnw.net/emoticons/v1/${emote.id}/1.0`}
                alt={emote.text}
                title={emote.text} /* Displays native browser tooltip on hover */
                className="inline-emote"
            />
        );

        lastIndex = emote.endIndex + 1;
    });

    if (lastIndex < text.length) {
        elements.push(text.slice(lastIndex));
    }
    if (elements.length === 0) {
        elements.push(text);
    }

    const rendered: React.ReactNode[] = [];
    elements.forEach((value) => {
        if (isString(value)) {
            rendered.push(...emoteParser.parse(value));
        } else {
            rendered.push(value);
        }
    });

    return <span className="text">{rendered}</span>;
}

export default EmoteReplacedMessage;