import type { FrontEndEventData, ChatEventChatMessage } from "../../types";
import ChatBadges from "../ChatBadges";
import EmoteReplacedMessage from "../EmoteReplacedMessage";

type ChatMessageItemProps = {
    event: FrontEndEventData<ChatEventChatMessage>;
};

function ChatMessageItem({ event }: ChatMessageItemProps) {
    const message = event.chatEventData;

    const hasBits = message.bits > 0;
    const chips: Array<{ label: string; title?: string }> = [];
    if (hasBits) {
        chips.push({
            label: `${message.bits} bits`,
            title: `${message.bits} bits (≈ $${message.bitsInDollars.toFixed(2)})`,
        });
    }
    if (message.isFirstMessage) {
        chips.push({ label: "first", title: "First message ever in this channel" });
    }
    if (message.isSkippingSubMode) {
        chips.push({ label: "sub-mode", title: "Used channel points to send while sub-mode was on" });
    }

    return (
        <div className={`chat-message ${message.isHighlighted ? "highlighted" : ""}`}>
            {message.isReply && (
                <span className="event-chip reply-chip" title="Replying to another message">
                    reply
                </span>
            )}
            <ChatBadges event={event} />
            <span className="username" style={{ color: message.hexColor || "#888" }}>
                {message.displayName}
            </span>
            <span className="colon">: </span>
            <span className={message.isMe ? "me-text" : undefined}>
                <EmoteReplacedMessage emotes={message.emotes} text={message.message} />
            </span>
            {chips.length > 0 && (
                <span className="event-chips">
                    {chips.map((chip, i) => (
                        <span key={i} className={`event-chip ${i === 0 && hasBits ? "bits" : ""}`} title={chip.title}>
                            {chip.label}
                        </span>
                    ))}
                </span>
            )}
        </div>
    );
}

export default ChatMessageItem;
