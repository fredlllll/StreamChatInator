import type { FrontEndEventData, ChatEventMessageCleared } from "../../types";

type MessageClearedItemProps = {
    event: FrontEndEventData<ChatEventMessageCleared>;
};

function MessageClearedItem({ event }: MessageClearedItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message">
            <span className="text">{JSON.stringify(message)}</span>
        </div>
    );
}

export default MessageClearedItem;