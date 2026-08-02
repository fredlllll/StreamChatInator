import type { FrontEndEventData, ChatEventGiftedSubscription } from "../../types";
import ChatBadges from "../ChatBadges";

type GiftedSubscriptionItemProps = {
    event: FrontEndEventData<ChatEventGiftedSubscription>;
};

function GiftedSubscriptionItem({ event }: GiftedSubscriptionItemProps) {
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

export default GiftedSubscriptionItem;