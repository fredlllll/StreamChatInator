import { Suspense, lazy, useEffect, useState } from "react";
import { createFilter, updateFilter, getFilterById } from "../api/filtersApi";
import { useNavigate, useParams } from "react-router-dom";
import { FILTER_TEMPLATE, ensureFunction, compileFilterSource } from "../editor/compileFilterSource";

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
                setCode(ensureFunction(filter.code));
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
        }
        navigate("/filters");
    }

    if (loading) return <p>Loading filter...</p>;
    if (notFound) return <p>Filter not found.</p>;

    return (
        <div>
            <h2>{creating ? "New filter" : "Edit filter"}</h2>

            <form onSubmit={handleSubmit}>
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
                    The filter is a TypeScript function(<code>eventData</code>). Type{" "}
                    <code>eventData.</code> to see the available fields, and{" "}
                    <code>eventData.chatEventType === "</code> for the list of event types.
                </p>

                <div>
                    <button type="submit">{creating ? "Create" : "Save"}</button>
                    <button type="button" onClick={() => navigate("/filters")}>Cancel</button>
                </div>
            </form>
        </div>
    );
}

export default FilterEditorPage;