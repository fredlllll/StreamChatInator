import type { FrontEndEventData, ChatEventUserTimedout } from "../../types";
import { formatTimeSpan } from "../../util";

type UserTimedoutItemProps = {
    event: FrontEndEventData<ChatEventUserTimedout>;
};

function UserTimedoutItem({ event }: UserTimedoutItemProps) {
    const message = event.chatEventData;

    const duration = formatTimeSpan(message.timeoutDuration);

    return (
        <div className="chat-message">
            <span className="event-pill ban-pill">Timeout</span>
            <span className="text">
                {message.username} was timed out{duration ? ` for ${duration}` : ""}
            </span>
        </div>
    );
}

export default UserTimedoutItem;
