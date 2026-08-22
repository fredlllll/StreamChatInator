import { useParams } from "react-router-dom";
import ChatEventList from "../components/ChatEventList";
import { useFilteredEvents } from "../useFilteredEvents";

function ViewPage() {
    const { filterId } = useParams<{ filterId: string }>();
    const { filter, filterLoadFailed, allEvents, hasMore, firstItemIndex, loadOlder } = useFilteredEvents(filterId);

    return (
        <div className="page">
            <div className="page-header">
                <h2>{filter ? filter.name : filterLoadFailed ? "Filter unavailable" : "Loading filter..."}</h2>
            </div>
            {filterLoadFailed ? (
                <p className="editor-hint">
                    Couldn't load this filter. It may have been deleted, or the server is unreachable.
                </p>
            ) : (
                <ChatEventList
                    firstItemIndex={firstItemIndex}
                    events={allEvents}
                    onStartReached={hasMore ? loadOlder : undefined}
                />
            )}
        </div>
    );
}

export default ViewPage;
