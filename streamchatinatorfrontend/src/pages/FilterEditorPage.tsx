import { Suspense, lazy, useEffect, useState } from "react";
import { createFilter, updateFilter, getFilterById, invalidateFilter } from "../api/filtersApi";
import { useNavigate, useParams } from "react-router-dom";
import { FILTER_TEMPLATE, compileFilterSource } from "../editor/compileFilterSource";

const FilterCodeEditor = lazy(() => import("../editor/FilterCodeEditor"));

function FilterEditorPage() {
    const { filterId } = useParams<{ filterId: string }>();
    const creating = !filterId;
    const navigate = useNavigate();

    const [name, setName] = useState("");
    const [code, setCode] = useState(FILTER_TEMPLATE);
    const [loading, setLoading] = useState(!creating);
    const [notFound, setNotFound] = useState(false);

    useEffect(() => {
        if (creating) return;
        let alive = true;
        getFilterById(filterId)
            .then((filter) => {
                if (!alive) return;
                setName(filter.name);
                setCode(filter.code);
                setLoading(false);
            })
            .catch(() => {
                if (!alive) return;
                setNotFound(true);
                setLoading(false);
            });
        return () => {
            alive = false;
        };
    }, [filterId, creating]);

    async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
        e.preventDefault();
        const { source, codeJs } = await compileFilterSource(code);
        if (creating) {
            await createFilter(name, source, codeJs);
        } else {
            await updateFilter(filterId, name, source, codeJs);
            invalidateFilter(filterId);
        }
        navigate("/filters");
    }

    if (loading) return <div className="page"><p>Loading filter...</p></div>;
    if (notFound) return <div className="page"><p>Filter not found.</p></div>;

    return (
        <div className="page">
            <div className="page-header">
                <h2>{creating ? "New filter" : "Edit filter"}</h2>
            </div>

            <form className="card editor-card" onSubmit={handleSubmit}>
                <div className="editor-field">
                    <label htmlFor="filter-name">Name</label>
                    <input
                        id="filter-name"
                        type="text"
                        className="input"
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        placeholder="Filter name"
                        required
                    />
                </div>

                <Suspense fallback={<div>Loading code editor...</div>}>
                    <FilterCodeEditor value={code} onChange={setCode} />
                </Suspense>
                <p className="editor-hint">
                    The filter is a TypeScript script. It must define{" "}
                    <code>__matches(eventData)</code>, plus any helper functions it likes. Type{" "}
                    <code>eventData.</code> to see the available fields, and{" "}
                    <code>eventData.chatEventType === "</code> for the list of event types.
                </p>

                <div className="editor-actions">
                    <button type="button" className="btn" onClick={() => navigate("/filters")}>Cancel</button>
                    <button type="submit" className="btn btn-primary">{creating ? "Create" : "Save"}</button>
                </div>
            </form>
        </div>
    );
}

export default FilterEditorPage;