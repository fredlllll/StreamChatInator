import eventComponents from "./eventComponents";
import { useChatConnection } from "../ChatContext";
import type { FrontEndEventData } from "../types";
import { formatEventTime, formatFullTime } from "../util";

type ChatEventItemProps = {
    event: FrontEndEventData;
};

function ChatEventItem({ event }: ChatEventItemProps) {
    const { seenState, setEventSeen } = useChatConnection();
    const Component = eventComponents[event.chatEventType];

    if (!Component) {
        return <div className="unknown-event">Unhandled event type: {event.chatEventType}</div>;
    }

    const seen = seenState[event.eventId] ?? event.seen;
    const time = formatEventTime(event.chatEventData);

    return (
        <div className={`chat-event-item ${seen ? "seen" : "unseen"}`}>
            <label className="seen-toggle" title={seen ? "Mark as unseen" : "Mark as seen"}>
                <input
                    type="checkbox"
                    checked={seen}
                    onChange={(e) => setEventSeen(event.eventId, e.target.checked)}
                />
            </label>
            <Component event={event} />
            {time && (
                <span className="event-time" title={formatFullTime(event.chatEventData)}>
                    {time}
                </span>
            )}
        </div>
    );
}

export default ChatEventItem;