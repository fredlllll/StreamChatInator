import { useState, useRef } from "react";
import { Virtuoso, type VirtuosoHandle } from "react-virtuoso";
import ChatEventItem from "./ChatEventItem";
import { useChatState } from "../ChatContext";
import type { FrontEndEventData } from "../types";
import type { CSSProperties } from "react";

type ChatEventListProps = {
    events: FrontEndEventData[];
    firstItemIndex: number;
    onStartReached?: () => void;
    style?: CSSProperties;
};

function ChatEventList({ events, firstItemIndex, onStartReached, style }: ChatEventListProps) {
    const virtuosoRef = useRef<VirtuosoHandle>(null);
    const [atBottom, setAtBottom] = useState(true);
    const { seenState } = useChatState();

    // Wait for data before mounting: `initialTopMostItemIndex` is only honored
    // at mount time, and with an empty list it would pin the view to the top
    // (the oldest messages) once history arrives instead of the newest.
    if (events.length === 0) {
        return <div style={{ height: "80vh", ...style }} />;
    }

    const scrollToBottom = () => {
        virtuosoRef.current?.scrollToIndex({ index: "LAST", align: "end", behavior: "auto" });
    };

    return (
        <div className="chat-event-list" style={{ height: "80vh", ...style }}>
            <Virtuoso
                ref={virtuosoRef}
                style={{ height: "100%" }}
                data={events}
                firstItemIndex={firstItemIndex}
                initialTopMostItemIndex={{ index: firstItemIndex + events.length - 1, align: "end" }}
                followOutput="smooth"
                startReached={onStartReached}
                atBottomStateChange={setAtBottom}
                itemContent={(_, event) => (
                    <ChatEventItem event={event} seen={seenState[event.eventId] ?? event.seen} />
                )}
            />
            {!atBottom && (
                <button
                    type="button"
                    className="scroll-to-bottom-btn"
                    onClick={scrollToBottom}
                    aria-label="Jump to latest messages"
                    title="Jump to latest messages"
                >
                    ↓
                </button>
            )}
        </div>
    );
}

export default ChatEventList;