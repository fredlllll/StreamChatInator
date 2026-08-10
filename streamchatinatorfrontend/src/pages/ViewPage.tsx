import { useParams } from "react-router-dom";
import ChatEventList from "../components/ChatEventList";
import { useFilteredEvents } from "../useFilteredEvents";

function ViewPage() {
    const { filterId } = useParams<{ filterId: string }>();
    const { filter, allEvents, hasMore, loadOlder } = useFilteredEvents(filterId);

    return (
        <div className="page">
            <div className="page-header">
                <h2>{filter ? filter.name : "Loading filter..."}</h2>
            </div>
            <ChatEventList events={allEvents} onStartReached={hasMore ? loadOlder : undefined} />
        </div>
    );
}

export default ViewPage;