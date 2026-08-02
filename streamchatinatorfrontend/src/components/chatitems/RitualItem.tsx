import type { FrontEndEventData, ChatEventRitual } from "../../types";
import ChatBadges from "../ChatBadges";

type RitualItemProps = {
    event: FrontEndEventData<ChatEventRitual>;
};

function RitualItem({ event }: RitualItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message">
            <ChatBadges event={event} />
            <span className="username" style={{ color: message.hexColor || "#888" }}>
                {message.displayName}
            </span>
            <span className="colon">: </span>
            <span className="text">{JSON.stringify(message)}</span>
        </div>
    );
}

export default RitualItem;