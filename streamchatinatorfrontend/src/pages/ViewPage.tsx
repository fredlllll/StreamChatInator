import { useParams } from "react-router-dom";
import ChatEventList from "../components/ChatEventList";
import { useFilteredEvents } from "../useFilteredEvents";

function ViewPage() {
    const { filterId } = useParams<{ filterId: string }>();
    const { filter, allEvents, hasMore, loadOlder } = useFilteredEvents(filterId);

    return (
        <div>
            <h2>{filter ? filter.name : "Loading filter..."}</h2>
            <ChatEventList events={allEvents} onStartReached={hasMore ? loadOlder : undefined} />
        </div>
    );
}

export default ViewPage;