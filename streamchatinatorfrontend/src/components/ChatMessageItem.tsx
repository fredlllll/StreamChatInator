import type { FrontEndEventData, ChatMessageData } from "../types";

type ChatMessageItemProps = {
    event: FrontEndEventData<ChatMessageData>;
};

function ChatMessageItem({ event }: ChatMessageItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message">
            {message.isBroadcaster && <span className="badge">STREAMER</span>}
            <span className="username" style={{ color: message.hexColor || "#888" }}>
                {message.displayName}
            </span>
            <span className="colon">: </span>
            <span className="text">{message.message}</span>
        </div>
    );
}

export default ChatMessageItem;