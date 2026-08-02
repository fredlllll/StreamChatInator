import type { FrontEndEventData, ChatEventContinuedGiftedSubscription } from "../../types";
import ChatBadges from "../ChatBadges";

type ContinuedGiftedSubscriptionItemProps = {
    event: FrontEndEventData<ChatEventContinuedGiftedSubscription>;
};

function ContinuedGiftedSubscriptionItem({ event }: ContinuedGiftedSubscriptionItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message">
            <ChatBadges event={event} />
            <span className="username" style={{ color: message.hexColor || "#888" }}>
                {message.displayName}
            </span>
            <span className="colon">: </span>
            <span className="text">JSON.stringify(message)</span>
        </div>
    );
}

export default ContinuedGiftedSubscriptionItem;