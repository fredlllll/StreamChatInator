import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { Actions, DockLocation, Layout, Model, type ILayoutApi, type TabNode } from "flexlayout-react";
import "flexlayout-react/style/dark.css";
import FilterTile from "../components/FilterTile";
import WelcomeHint from "../components/WelcomeHint";
import { getFilters } from "../api/filtersApi";
import type { EventFilter } from "../types";
import { STORAGE_KEY, filterToTab, loadModel, modelForFilters } from "./dashboardLayout";

function DashboardPage() {
    const [searchParams, setSearchParams] = useSearchParams();
    const urlFilterIds = useMemo(
        () => (searchParams.get("filters") ?? "").split(",").filter(Boolean),
        [searchParams]
    );
    const addFilterIds = useMemo(
        () => (searchParams.get("add") ?? "").split(",").filter(Boolean),
        [searchParams]
    );

    const [availableFilters, setAvailableFilters] = useState<EventFilter[]>([]);
    const [selectedFilterId, setSelectedFilterId] = useState("");
    const [filtersLoadFailed, setFiltersLoadFailed] = useState(false);
    const [model, setModelState] = useState<Model>(loadModel);
    const modelRef = useRef(model);
    const layoutRef = useRef<ILayoutApi>(null);

    function setModel(newModel: Model) {
        modelRef.current = newModel;
        setModelState(newModel);
    }

    const onModelChange = useCallback((newModel: Model) => {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(newModel.toJson()));
    }, []);

    function refreshFilters() {
        setFiltersLoadFailed(false);
        getFilters()
            .then(setAvailableFilters)
            .catch(() => setFiltersLoadFailed(true));
    }

    useEffect(() => {
        refreshFilters();
    }, []);

    useEffect(() => {
        if (urlFilterIds.length === 0 && addFilterIds.length === 0) return;
        let cancelled = false;
        getFilters()
            .then((filters) => {
                if (cancelled) return;
                if (urlFilterIds.length > 0) {
                    const selected = filters.filter((f) => urlFilterIds.includes(f.id));
                    if (selected.length > 0) {
                        setModel(Model.fromJson(modelForFilters(selected)));
                        return;
                    }
                }
                if (addFilterIds.length > 0) {
                    const toAdd = filters.filter((f) => addFilterIds.includes(f.id));
                    toAdd.forEach(addFilterTab);
                    // Consume ?add= so reloads don't dock the same tab again.
                    setSearchParams(
                        (prev) => {
                            const next = new URLSearchParams(prev);
                            next.delete("add");
                            return next;
                        },
                        { replace: true }
                    );
                }
            })
            .catch((err) => console.error("Failed to load filters for URL seeding:", err));
        return () => {
            cancelled = true;
        };
    }, [urlFilterIds, addFilterIds, setSearchParams]);

    const factory = useCallback((node: TabNode) => {
        switch (node.getComponent()) {
            case "filter":
                return <FilterTile key={node.getId()} filterId={node.getConfig().filterId} />;
            case "welcome":
                return <WelcomeHint />;
            default:
                return <div>Unknown panel</div>;
        }
    }, []);

    function addFilterTab(filter: EventFilter) {
        const api = layoutRef.current;
        if (api && api.addTabToActiveTabSet(filterToTab(filter))) return;

        const ts = modelRef.current.getActiveTabset() ?? modelRef.current.getFirstTabSet();
        if (ts) {
            modelRef.current.doAction(Actions.addTab(filterToTab(filter), ts.getId(), DockLocation.CENTER, -1));
        } else {
            // No tabset left (shouldn't happen, but recover by rebuilding from scratch).
            setModel(Model.fromJson(modelForFilters([filter])));
        }
    }

    function resetLayout() {
        localStorage.removeItem(STORAGE_KEY);
        setModel(Model.fromJson(modelForFilters([])));
    }

    return (
        <div className="workspace-page">
            <div className="workspace-toolbar">
                <select
                    className="input"
                    value={selectedFilterId}
                    onChange={(e) => setSelectedFilterId(e.target.value)}
                    aria-label="Add a filter view"
                >
                    <option value="">Add a filter view...</option>
                    {availableFilters.map((f) => (
                        <option key={f.id} value={f.id}>
                            {f.name}
                        </option>
                    ))}
                </select>
                <button type="button" className="btn btn-primary" disabled={!selectedFilterId} onClick={() => {
                    const filter = availableFilters.find((f) => f.id === selectedFilterId);
                    if (filter) addFilterTab(filter);
                    setSelectedFilterId("");
                }}>
                    + Add view
                </button>
                <button type="button" className="btn" onClick={resetLayout}>Reset layout</button>
                {filtersLoadFailed && (
                    <>
                        <span className="workspace-param-hint">Couldn't load filters.</span>
                        <button type="button" className="btn" onClick={refreshFilters}>Retry</button>
                    </>
                )}
                {urlFilterIds.length > 0 && (
                    <span className="workspace-param-hint">Seeded from ?filters= in the URL - remove it to keep your docked layout across reloads.</span>
                )}
            </div>
            <div className="workspace-layout">
                <Layout ref={layoutRef} model={model} factory={factory} onModelChange={onModelChange} />
            </div>
        </div>
    );
}

export default DashboardPage;