import type { FrontEndEventData, ChatEventUserLeft } from "../../types";

type UserLeftItemProps = {
    event: FrontEndEventData<ChatEventUserLeft>;
};

function UserLeftItem({ event }: UserLeftItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message">
            <span className="text">{JSON.stringify(message)}</span>
        </div>
    );
}

export default UserLeftItem;