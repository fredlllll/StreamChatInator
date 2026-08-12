import type { FrontEndEventData, ChatEventChatMessage } from "../../types";
import ChatBadges from "../ChatBadges";
import EmoteReplacedMessage from "../EmoteReplacedMessage";

type ChatMessageItemProps = {
    event: FrontEndEventData<ChatEventChatMessage>;
};

function ChatMessageItem({ event }: ChatMessageItemProps) {
    const message = event.chatEventData;

    const hasBits = message.bits > 0;
    const chips: string[] = [];
    if (hasBits) chips.push(`${message.bits} bits`);
    if (message.isFirstMessage) chips.push("first");
    if (message.subscribedMonthCount > 0) chips.push(`${message.subscribedMonthCount} mo`);
    if (message.isSkippingSubMode) chips.push("sub-mode");

    return (
        <div className={`chat-message ${message.isHighlighted ? "highlighted" : ""}`}>
            {message.isReply && <span className="event-chip reply-chip">reply</span>}
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
                        <span key={i} className={`event-chip ${i === 0 && hasBits ? "bits" : ""}`}>
                            {chip}
                        </span>
                    ))}
                </span>
            )}
        </div>
    );
}

export default ChatMessageItem;
