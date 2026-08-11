import type { FrontEndEventData } from "./types";

// Compiled matchers are shared by every tile/remount that uses the same filter.
// Keying on filter id + updated timestamp means an edit produces a new key and
// a recompile automatically. Bounded so a long editing session can't grow it.
const matcherCache = new Map<string, (eventData: FrontEndEventData) => boolean>();
const MATCHER_CACHE_LIMIT = 100;

function cacheCompiled(key: string, fn: (eventData: FrontEndEventData) => boolean) {
    matcherCache.set(key, fn);
    if (matcherCache.size > MATCHER_CACHE_LIMIT) {
        const oldest = matcherCache.keys().next().value;
        if (oldest !== undefined) matcherCache.delete(oldest);
    }
}

export function compileFilter(
    code: string,
    cacheKey?: string,
): (eventData: FrontEndEventData) => boolean {
    if (cacheKey) {
        const cached = matcherCache.get(cacheKey);
        if (cached) return cached;
    }

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
        fn = () => true;
    }

    const wrapped = (eventData: FrontEndEventData) => {
        try {
            return !!fn(eventData);
        } catch (err) {
            console.error("Filter code threw an error:", err);
            return true;
        }
    };

    if (cacheKey) cacheCompiled(cacheKey, wrapped);
    return wrapped;
}
