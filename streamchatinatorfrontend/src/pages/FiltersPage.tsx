import { useEffect, useState } from "react";
import type { EventFilter } from "../types";
import { getFilters, deleteFilter, invalidateFilter } from "../api/filtersApi";
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
        invalidateFilter(id);
        await refresh();
    }

    return (
        <div className="page">
            <div className="page-header">
                <h2>Filters</h2>
                <Link to="/filters/new" className="btn btn-primary">+ New Filter</Link>
            </div>

            <ul className="filter-list">
                {filters.map((f) => (
                    <li key={f.id}>
                        <span className="filter-name">{f.name}</span>
                        <Link className="btn btn-sm" to={`/view/${f.id}`}>View</Link>
                        <Link className="btn btn-sm" to={`/dashboard?add=${f.id}`}>Dock</Link>
                        <button
                            type="button"
                            className="btn btn-sm"
                            onClick={() => navigator.clipboard.writeText(`${window.location.origin}/view/${f.id}`)}
                        >
                            Copy link
                        </button>
                        <Link className="btn btn-sm" to={`/filters/${f.id}/edit`}>Edit</Link>
                        <button type="button" className="btn btn-sm btn-danger" onClick={() => handleDelete(f.id)}>
                            Delete
                        </button>
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default FiltersPage;