import type { FrontEndEventData, ChatEventUserBanned } from "../../types";

type UserBannedItemProps = {
    event: FrontEndEventData<ChatEventUserBanned>;
};

function UserBannedItem({ event }: UserBannedItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message">
            <span className="event-pill ban-pill">Banned</span>
            <span className="text ban-text">{message.username} was banned</span>
        </div>
    );
}

export default UserBannedItem;
