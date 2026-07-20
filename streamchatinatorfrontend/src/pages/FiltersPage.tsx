import { useEffect, useState } from "react";
import type { EventFilter } from "../types";
import { getFilters, createFilter, updateFilter, deleteFilter } from "../api/filtersApi";
import { Link } from "react-router-dom";

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
        setCode("");
    }

    async function handleSubmit(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();
        if (editingId) {
            await updateFilter(editingId, name, code);
        } else {
            await createFilter(name, code);
        }
        startNew();
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

                <textarea
                    value={code}
                    onChange={(e) => setCode(e.target.value)}
                    rows={10}
                    style={{ width: "100%", fontFamily: "monospace" }}
                    placeholder='return eventType === "ChatMessage" && eventData.username === "someviewer";'
                />

                <div>
                    <button type="submit">{editingId ? "Save" : "Create"}</button>
                    {editingId && <button type="button" onClick={startNew}>Cancel</button>}
                </div>
            </form>
        </div>
    );
}

export default FiltersPage;