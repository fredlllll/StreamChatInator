import type { EventFilter, FrontEndEventData } from "../types";

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
        body: JSON.stringify({ name, filterType: "Chat", code, codeJs }),
    });
    if (!res.ok) throw new Error("Failed to create filter");
    return res.json();
}

export async function updateFilter(id: string, name: string, code: string, codeJs: string): Promise<void> {
    const res = await fetch(`${BASE_URL}/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name, filterType: "Chat", code, codeJs }),
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

export interface HistoryResponse {
    events: FrontEndEventData[];
    nextCursor: string;
    hasMore: boolean;
}

export async function getFilterHistory(filterId: string, before: string, take = 50): Promise<HistoryResponse> {
    const params = new URLSearchParams({ before, take: String(take) });
    const res = await fetch(`${BASE_URL}/${filterId}/messages?${params}`);
    if (!res.ok) throw new Error("Failed to load history");
    return res.json();
}