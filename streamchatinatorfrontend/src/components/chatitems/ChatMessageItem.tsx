import type { FrontEndEventData, ChatEventChatMessage } from "../../types";
import ChatBadges from "../ChatBadges";
import EmoteReplacedMessage from "../EmoteReplacedMessage";

type ChatMessageItemProps = {
    event: FrontEndEventData<ChatEventChatMessage>;
};

function ChatMessageItem({ event }: ChatMessageItemProps) {
    const message = event.chatEventData;
    return (
        <div className="chat-message">
            <ChatBadges event={event} />
            <span className="username" style={{ color: message.hexColor || "#888" }}>
                {message.displayName}
            </span>
            <span className="colon">: </span>
            <EmoteReplacedMessage emotes={message.emotes} text={message.message} />
        </div>
    );
}

export default ChatMessageItem;