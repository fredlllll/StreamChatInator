import type { FrontEndEventData, ChatEventMessageCleared } from "../../types";

type MessageClearedItemProps = {
    event: FrontEndEventData<ChatEventMessageCleared>;
};

function MessageClearedItem({ event }: MessageClearedItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message">
            <span className="event-pill">Cleared</span>
            <span className="text cleared-text">{message.message || "Message removed"}</span>
        </div>
    );
}

export default MessageClearedItem;
