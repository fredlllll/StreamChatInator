import ChatEventList from "./ChatEventList";
import { useFilteredEvents } from "../useFilteredEvents";

type FilterTileProps = {
    filterId: string;
};

function FilterTile({ filterId }: FilterTileProps) {
    const { filter, filterLoadFailed, allEvents, hasMore, firstItemIndex, loadOlder } = useFilteredEvents(filterId);

    return (
        <div className="filter-tile">
            <h3>{filter ? filter.name : filterLoadFailed ? "unavailable" : "..."}</h3>
            <div className="filter-tile-list">
                {filterLoadFailed ? (
                    <p className="editor-hint">Couldn't load this filter.</p>
                ) : (
                    <ChatEventList
                        style={{ height: "100%" }}
                        firstItemIndex={firstItemIndex}
                        events={allEvents}
                        onStartReached={hasMore ? loadOlder : undefined}
                    />
                )}
            </div>
        </div>
    );
}

export default FilterTile;
