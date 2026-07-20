import { useSearchParams } from "react-router-dom";
import FilterTile from "../components/FilterTile";

function DashboardPage() {
    const [searchParams] = useSearchParams();
    const filterIds = (searchParams.get("filters") ?? "").split(",").filter(Boolean);

    if (filterIds.length === 0) {
        return <p>Add ?filters=id1,id2 to the URL to see views here.</p>;
    }

    return (
        <div className="dashboard-grid">
            {filterIds.map((id) => (
                <FilterTile key={id} filterId={id} />
            ))}
        </div>
    );
}

export default DashboardPage;