import ChatEventList from "./ChatEventList";
import { useFilteredEvents } from "../useFilteredEvents";

type FilterTileProps = {
    filterId: string;
};

function FilterTile({ filterId }: FilterTileProps) {
    const { filter, allEvents, hasMore, loadOlder } = useFilteredEvents(filterId);

    return (
        <div className="filter-tile">
            <h3>{filter ? filter.name : "..."}</h3>
            <ChatEventList events={allEvents} onStartReached={hasMore ? loadOlder : undefined} />
        </div>
    );
}

export default FilterTile;