import { Suspense, lazy, useEffect, useState } from "react";
import type { EventFilter } from "../types";
import { getFilters, createFilter, updateFilter, deleteFilter } from "../api/filtersApi";
import { Link } from "react-router-dom";

const FilterCodeEditor = lazy(() => import("../editor/FilterCodeEditor"));

function FiltersPage() {
    const [filters, setFilters] = useState<EventFilter[]>([]);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [name, setName] = useState("");
    const [code, setCode] = useState("");

    async function refresh() {
        setFilters(await getFilters());
    }

    useEffect(() => {
        refresh();
    }, []);

    function startEdit(filter: EventFilter) {
        setEditingId(filter.id);
        setName(filter.name);
        setCode(filter.code);
    }

    function startNew() {
        setEditingId(null);
        setName("");
        setCode('return eventData.chatEventType === "ChatMessage" && eventData.chatEventData.username === "someviewer";');
    }

    async function handleSubmit(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();
        if (editingId) {
            await updateFilter(editingId, name, code);
        } else {
            await createFilter(name, code);
            startNew(); // only reset to blank after creating something new, not after editing
        }
        await refresh();
    }

    async function handleDelete(id: string) {
        await deleteFilter(id);
        if (editingId === id) startNew();
        await refresh();
    }

    return (
        <div>
            <h2>Filters</h2>

            <ul>
                {filters.map((f) => (
                    <li key={f.id}>
                        {f.name}
                        <Link to={`/view/${f.id}`}>View</Link>
                        <Link to={`/dashboard?add=${f.id}`}>Dock</Link>
                        <button type="button" onClick={() => navigator.clipboard.writeText(`${window.location.origin}/view/${f.id}`)} >
                            Copy link
                        </button>
                        <button type="button" onClick={() => startEdit(f)}>Edit</button>
                        <button type="button" onClick={() => handleDelete(f.id)}>Delete</button>
                    </li>
                ))}
            </ul>

            <form onSubmit={handleSubmit}>
                <h3>{editingId ? "Edit filter" : "New filter"}</h3>

                <input
                    type="text"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="Filter name"
                    required
                />

                <Suspense fallback={<div>Loading code editor...</div>}>
                    <FilterCodeEditor value={code} onChange={setCode} />
                </Suspense>
                <p>
                    The code runs as <code>function(eventData)</code>. Type{" "}
                    <code>eventData.</code> to see available fields, and{" "}
                    <code>eventData.chatEventType === "</code> for the list of event types.
                </p>

                <div>
                    <button type="submit">{editingId ? "Save" : "Create"}</button>
                    <button type="button" onClick={startNew}>+ New Filter</button>
                    {editingId && <button type="button" onClick={startNew}>Cancel</button>}
                </div>
            </form>
        </div>
    );
}

export default FiltersPage;