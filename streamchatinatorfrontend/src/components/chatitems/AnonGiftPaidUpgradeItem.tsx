import type { FrontEndEventData, ChatEventAnonGiftPaidUpgrade } from "../../types";
import ChatBadges from "../ChatBadges";

type AnonGiftPaidUpgradeItemProps = {
    event: FrontEndEventData<ChatEventAnonGiftPaidUpgrade>;
};

function AnonGiftPaidUpgradeItem({ event }: AnonGiftPaidUpgradeItemProps) {
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

export default AnonGiftPaidUpgradeItem;