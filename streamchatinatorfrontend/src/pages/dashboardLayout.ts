import { Model, type IJsonModel, type IJsonTabNode } from "flexlayout-react";
import type { EventFilter } from "../types";

export const STORAGE_KEY = "streamchatinator.layout.v1";

export function filterToTab(filter: EventFilter): IJsonTabNode {
    return {
        type: "tab",
        component: "filter",
        name: filter.name,
        config: { filterId: filter.id },
    };
}

export function modelForFilters(filters: EventFilter[]): IJsonModel {
    const tabs: IJsonTabNode[] = filters.map(filterToTab);
    if (tabs.length === 0) {
        tabs.push({
            type: "tab",
            component: "welcome",
            name: "Getting started",
        });
    }
    return {
        global: {
            tabSetEnableDeleteWhenEmpty: false,
            tabEnableRename: false,
        },
        borders: [],
        layout: {
            type: "row",
            children: [
                {
                    type: "tabset",
                    children: tabs,
                },
            ],
        },
    };
}

export function loadModel(): Model {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) {
        try {
            return Model.fromJson(JSON.parse(saved) as IJsonModel);
        } catch {
            // fall through to default
        }
    }
    return Model.fromJson(modelForFilters([]));
}