import eventComponents from "./eventComponents";
import type { FrontEndEventData } from "../types";

type ChatEventItemProps = {
    event: FrontEndEventData;
};

function ChatEventItem({ event }: ChatEventItemProps) {
    const Component = eventComponents[event.chatEventType];

    if (!Component) {
        return <div className="unknown-event">Unhandled event type: {event.chatEventType}</div>;
    }

    return <Component event={event} />;
}

export default ChatEventItem;