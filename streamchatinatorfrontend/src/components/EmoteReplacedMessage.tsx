import type { ReactEmoteParser } from "../emoteReplace/ReactEmoteParser";
import { extractEmotes } from "../emoteReplace/emotePositions";
import { useEmoteParser } from "../useEmoteParser";

function EmoteReplacedMessage({ emotes, text }: { emotes: string|null, text:string }) {
    const emoteParser: ReactEmoteParser = useEmoteParser();

    // Slice out native Twitch emotes by their positions first; the plain-text
    // segments left in between go through the external (BTTV/7TV/FFZ) parser.
    // Each text segment is parsed with its character offset in the full
    // message as `keyBase`, so identical codes at the same token index in
    // different segments can't collide on React keys.
    const rendered: React.ReactNode[] = [];
    const emoteList = extractEmotes(emotes, text).sort((a, b) => a.startIndex - b.startIndex);

    let lastIndex = 0;
    for (const emote of emoteList) {
        if (emote.startIndex > lastIndex) {
            rendered.push(...emoteParser.parse(text.slice(lastIndex, emote.startIndex), lastIndex));
        }

        rendered.push(
            <img
                key={`twitch-${emote.id}-${emote.startIndex}`}
                src={`https://static-cdn.jtvnw.net/emoticons/v2/${emote.id}/default/light/2.0`}
                alt={emote.text}
                title={emote.text} /* Displays native browser tooltip on hover */
                className="inline-emote"
            />
        );

        lastIndex = emote.endIndex + 1;
    }

    if (lastIndex < text.length) {
        rendered.push(...emoteParser.parse(text.slice(lastIndex), lastIndex));
    }
    if (rendered.length === 0) {
        rendered.push(text);
    }

    return <span className="text">{rendered}</span>;
}

export default EmoteReplacedMessage;