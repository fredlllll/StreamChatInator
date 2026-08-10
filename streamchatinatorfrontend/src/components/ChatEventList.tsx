import { Virtuoso } from "react-virtuoso";
import ChatEventItem from "./ChatEventItem";
import type { FrontEndEventData } from "../types";
import type { CSSProperties } from "react";

type ChatEventListProps = {
    events: FrontEndEventData[];
    firstItemIndex: number;
    onStartReached?: () => void;
    style?: CSSProperties;
};

function ChatEventList({ events, firstItemIndex, onStartReached, style }: ChatEventListProps) {
    // Wait for data before mounting: `initialTopMostItemIndex` is only honored
    // at mount time, and with an empty list it would pin the view to the top
    // (the oldest messages) once history arrives instead of the newest.
    if (events.length === 0) {
        return <div style={{ height: "80vh", ...style }} />;
    }

    return (
        <Virtuoso
            style={{ height: "80vh", ...style }}
            data={events}
            firstItemIndex={firstItemIndex}
            initialTopMostItemIndex={{ index: firstItemIndex + events.length - 1, align: "end" }}
            followOutput="smooth"
            startReached={onStartReached}
            itemContent={(_, event) => <ChatEventItem event={event} />}
        />
    );
}

export default ChatEventList;