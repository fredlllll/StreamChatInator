import type { FrontEndEventData } from "./types";

export function compileFilter(code: string): (eventData: FrontEndEventData) => boolean {
    const fn = new Function("eventData", code) as (eventData: FrontEndEventData) => boolean;
    return (eventData) => {
        try {
            return !!fn(eventData);
        } catch (err) {
            console.error("Filter code threw an error:", err);
            return false;
        }
    };
}