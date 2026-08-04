import type { FrontEndEventData, ChatEventReSubscriber } from "../../types";
import ChatBadges from "../ChatBadges";

type ReSubscriberItemProps = {
    event: FrontEndEventData<ChatEventReSubscriber>;
};

function ReSubscriberItem({ event }: ReSubscriberItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message system-message">
            <span className="text">{message.systemMsg}</span>
            <br/>
            <ChatBadges event={event} />
            <span className="username" style={{ color: message.hexColor || "#888" }}>
                {message.displayName}
            </span>
            <span className="colon">: </span>
            <span className="text">{message.resubMessage}</span>
        </div>
    );
}

export default ReSubscriberItem;