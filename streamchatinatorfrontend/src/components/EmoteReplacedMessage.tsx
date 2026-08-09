import { useChatConnection, type ChatContextType } from "../ChatContext";
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
    const emoteList = extractEmotes(emotes, text);
    const ctx: ChatContextType = useChatConnection();
    const emoteParser: ReactEmoteParser = useEmoteParser();

    if (!emoteList || emoteList.length === 0) {
        return <span className="text">{text}</span>;
    }

    // Sort emotes linearly by start position to safely slice the string
    const sortedEmotes = [...emoteList].sort((a, b) => a.startIndex - b.startIndex);

    const elements: React.ReactNode[] = [];
    let lastIndex = 0;

    sortedEmotes.forEach((emote) => {
        // 1. Add plain text prior to the emote
        if (emote.startIndex > lastIndex) {
            elements.push(text.slice(lastIndex, emote.startIndex));
        }

        // 2. Add the emote image
        const emoteUrl = `https://static-cdn.jtvnw.net/emoticons/v1/${emote.id}/1.0`;
        elements.push(
            <img
                key={`emote-${emote.id}-${emote.startIndex}`}
                src={emoteUrl}
                alt={emote.text}
                title={emote.text} /* Displays native browser tooltip on hover */
                className="inline-emote"
            />
        );

        // 3. Move cursor past the end of the current emote
        lastIndex = emote.endIndex + 1;
    });

    // 4. Append remaining text after the final emote
    if (lastIndex < text.length) {
        elements.push(text.slice(lastIndex));
    }

    //TODO: use the external emotes here as we can still work with the bare string messages in between
    const newElements: React.ReactNode[] = [];
    elements.forEach((value, index, array) => {
        if (isString(value)) {
            var parsed = emoteParser.parse(value);
            parsed.forEach((x) => {
                newElements.push(x);
            });
        } else {
            newElements.push(value);
        }
    });

    return <span className="text">{newElements}</span>;
}

export default EmoteReplacedMessage;