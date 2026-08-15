import type { EventFilter, HistoryResponse } from "../types";

const BASE_URL = "/api/filters";

export async function getFilters(): Promise<EventFilter[]> {
    const res = await fetch(BASE_URL);
    if (!res.ok) throw new Error("Failed to load filters");
    return res.json();
}

export async function createFilter(name: string, code: string, codeJs: string): Promise<EventFilter> {
    const res = await fetch(BASE_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name, code, codeJs }),
    });
    if (!res.ok) throw new Error("Failed to create filter");
    return res.json();
}

export async function updateFilter(id: string, name: string, code: string, codeJs: string): Promise<void> {
    const res = await fetch(`${BASE_URL}/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name, code, codeJs }),
    });
    if (!res.ok) throw new Error("Failed to update filter");
}

export async function deleteFilter(id: string): Promise<void> {
    const res = await fetch(`${BASE_URL}/${id}`, { method: "DELETE" });
    if (!res.ok) throw new Error("Failed to delete filter");
}

export async function getFilterById(id: string): Promise<EventFilter> {
    const res = await fetch(`${BASE_URL}/${id}`);
    if (!res.ok) throw new Error("Failed to load filter");
    return res.json();
}

// Every dashboard tile / view for the same filter fetches the same row; cache
// the latest fetch per id for the lifetime of the page so they share one
// request. Call invalidateFilter after create/update/delete so edits aren't stale.
const filterCache = new Map<string, Promise<EventFilter>>();

export function getFilterByIdCached(id: string): Promise<EventFilter> {
    const cached = filterCache.get(id);
    if (cached) return cached;

    const promise = getFilterById(id).catch((err) => {
        filterCache.delete(id);
        throw err;
    });
    filterCache.set(id, promise);
    return promise;
}

export function invalidateFilter(id: string): void {
    filterCache.delete(id);
}

export async function getFilterHistory(filterId: string, before: string, take = 50): Promise<HistoryResponse> {
    const params = new URLSearchParams({ before, take: String(take) });
    const res = await fetch(`${BASE_URL}/${filterId}/messages?${params}`);
    if (!res.ok) throw new Error("Failed to load history");
    return res.json();
}