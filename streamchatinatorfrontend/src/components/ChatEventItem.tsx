import { memo } from "react";
import eventComponents from "./eventComponents";
import { useChatActions } from "../ChatContext";
import type { FrontEndEventData } from "../types";
import { formatEventTime, formatFullTime } from "../util";

type ChatEventItemProps = {
    event: FrontEndEventData;
    // Resolved by the list (which already tracks seenState), so this component
    // doesn't have to subscribe to it — seenState changes on every arrival,
    // which would defeat the memo below.
    seen: boolean;
};

// Memoized over primitive props + stable refs from ChatActionsContext: when a
// new message arrives only the items whose own seen flag changed re-render,
// instead of every visible item in every tile.
function ChatEventItem({ event, seen }: ChatEventItemProps) {
    const { setEventSeen } = useChatActions();
    const Component = eventComponents[event.chatEventType];

    if (!Component) {
        return <div className="unknown-event">Unhandled event type: {event.chatEventType}</div>;
    }

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

export default memo(ChatEventItem);
