import type { FrontEndEventData, ChatEventUserTimedout } from "../../types";

type UserTimedoutItemProps = {
    event: FrontEndEventData<ChatEventUserTimedout>;
};

function UserTimedoutItem({ event }: UserTimedoutItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message">
            <span className="text">{JSON.stringify(message)}</span>
        </div>
    );
}

export default UserTimedoutItem;