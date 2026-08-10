import type { Model } from "./chatEventTypes";
export * from "./chatEventTypes";

export interface EventFilter extends Model {
    name: string;
    code: string; // TypeScript source of the filter function
    codeJs: string; // compiled JavaScript (function body) used for execution
}

export interface BadgeInfo {
    title: string;
    imageUrl: string;
    clickAction?: string | null;
    clickUrl?: string | null;
}

export type BadgeMap = Record<string, Record<string, BadgeInfo>>;