import eventComponents from "./eventComponents";
import type { EventEnvelope } from "../types";

type ChatEventItemProps = {
    event: EventEnvelope;
};

function ChatEventItem({ event }: ChatEventItemProps) {
    const Component = eventComponents[event.type];

    if (!Component) {
        return <div className="unknown-event">Unhandled event type: {event.type}</div>;
    }

    return <Component event={event} />;
}

export default ChatEventItem;