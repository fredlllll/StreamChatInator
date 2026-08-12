import type { FrontEndEventData, ChatEventUserJoined } from "../../types";

type UserJoinedItemProps = {
    event: FrontEndEventData<ChatEventUserJoined>;
};

function UserJoinedItem({ event }: UserJoinedItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message system-message">
            <span className="event-pill">Joined</span>
            <span className="text">{message.username} joined the chat</span>
        </div>
    );
}

export default UserJoinedItem;
