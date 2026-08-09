import { Virtuoso } from "react-virtuoso";
import ChatEventItem from "./ChatEventItem";
import type { FrontEndEventData } from "../types";
import type { CSSProperties } from "react";

type ChatEventListProps = {
    events: FrontEndEventData[];
    onStartReached?: () => void;
    style?: CSSProperties;
};

function ChatEventList({ events, onStartReached, style }: ChatEventListProps) {
    return (
        <Virtuoso
            style={{ height: "80vh", ...style }}
            data={events}
            initialTopMostItemIndex={events.length - 1}
            followOutput="smooth"
            startReached={onStartReached}
            itemContent={(_, event) => <ChatEventItem event={event} />}
        />
    );
}

export default ChatEventList;