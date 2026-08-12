import type { FrontEndEventData, ChatEventUserLeft } from "../../types";

type UserLeftItemProps = {
    event: FrontEndEventData<ChatEventUserLeft>;
};

function UserLeftItem({ event }: UserLeftItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message system-message">
            <span className="event-pill">Left</span>
            <span className="text">{message.username} left the chat</span>
        </div>
    );
}

export default UserLeftItem;
