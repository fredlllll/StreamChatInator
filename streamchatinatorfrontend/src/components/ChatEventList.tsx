import { Virtuoso } from "react-virtuoso";
import ChatEventItem from "./ChatEventItem";
import type { FrontEndEventData } from "../types";

type ChatEventListProps = {
    events: FrontEndEventData[];
    onStartReached?: () => void;
};

function ChatEventList({ events, onStartReached }: ChatEventListProps) {
    return (
        <Virtuoso
            style={{ height: "80vh" }}
            data={events}
            initialTopMostItemIndex={events.length - 1}
            followOutput="smooth"
            startReached={onStartReached}
            itemContent={(_, event) => <ChatEventItem event={event} />}
        />
    );
}

export default ChatEventList;