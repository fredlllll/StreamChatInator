import type { FrontEndEventData, ChatEventUserJoined } from "../../types";

type UserJoinedItemProps = {
    event: FrontEndEventData<ChatEventUserJoined>;
};

function UserJoinedItem({ event }: UserJoinedItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message">
            <span className="text">{JSON.stringify(message)}</span>
        </div>
    );
}

export default UserJoinedItem;