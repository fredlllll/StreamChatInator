import type { FrontEndEventData, ChatEventUserBanned } from "../../types";

type UserBannedItemProps = {
    event: FrontEndEventData<ChatEventUserBanned>;
};

function UserBannedItem({ event }: UserBannedItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message">
            <span className="text">{JSON.stringify(message)}</span>
        </div>
    );
}

export default UserBannedItem;