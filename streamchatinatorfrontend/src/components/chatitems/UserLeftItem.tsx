import type { FrontEndEventData, ChatEventUserLeft } from "../../types";

type UserLeftItemProps = {
    event: FrontEndEventData<ChatEventUserLeft>;
};

function UserLeftItem({ event }: UserLeftItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message">
            <span className="text">SYSTEM: User {message.username} left</span>
        </div>
    );
}

export default UserLeftItem;