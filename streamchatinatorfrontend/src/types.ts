import type { FrontEndEventData, Model } from "./chatEventTypes";
export * from "./chatEventTypes";

export interface EventFilter extends Model {
    name: string;
    code: string; // TypeScript source of the filter script (defines __matches)
    codeJs: string; // compiled JavaScript (full script) used for execution
}

export interface BadgeInfo {
    title: string;
    imageUrl: string;
    clickAction?: string | null;
    clickUrl?: string | null;
}

export type BadgeMap = Record<string, Record<string, BadgeInfo>>;

export interface HistoryResponse {
    events: FrontEndEventData[];
    nextCursor: string;
    hasMore: boolean;
}