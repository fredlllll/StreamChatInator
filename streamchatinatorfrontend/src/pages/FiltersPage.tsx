import { useEffect, useState } from "react";
import type { EventFilter } from "../types";
import { getFilters, deleteFilter } from "../api/filtersApi";
import { Link } from "react-router-dom";

function FiltersPage() {
    const [filters, setFilters] = useState<EventFilter[]>([]);

    async function refresh() {
        setFilters(await getFilters());
    }

    useEffect(() => {
        refresh();
    }, []);

    async function handleDelete(id: string) {
        await deleteFilter(id);
        await refresh();
    }

    return (
        <div>
            <h2>Filters</h2>

            <Link to="/filters/new">+ New Filter</Link>

            <ul>
                {filters.map((f) => (
                    <li key={f.id}>
                        {f.name}
                        <Link to={`/view/${f.id}`}>View</Link>
                        <Link to={`/dashboard?add=${f.id}`}>Dock</Link>
                        <button type="button" onClick={() => navigator.clipboard.writeText(`${window.location.origin}/view/${f.id}`)} >
                            Copy link
                        </button>
                        <Link to={`/filters/${f.id}/edit`}>Edit</Link>
                        <button type="button" onClick={() => handleDelete(f.id)}>Delete</button>
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default FiltersPage;