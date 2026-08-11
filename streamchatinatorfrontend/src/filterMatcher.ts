import type { FrontEndEventData } from "./types";

export function compileFilter(code: string): (eventData: FrontEndEventData) => boolean {
    let fn: (eventData: FrontEndEventData) => boolean;
    try {
        // `code` is the full compiled script defining `__matches(eventData)`.
        // Run it and call `__matches`, falling back to "pass" when missing.
        fn = new Function(
            "eventData",
            `${code}\n;return __matches ? __matches(eventData) : true;`,
        ) as (eventData: FrontEndEventData) => boolean;
    } catch (err) {
        console.error("Filter code failed to compile:", err);
        return () => false;
    }
    return (eventData) => {
        try {
            return !!fn(eventData);
        } catch (err) {
            console.error("Filter code threw an error:", err);
            return false;
        }
    };
}