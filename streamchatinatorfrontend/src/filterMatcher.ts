export function compileFilter(code: string): (eventType: string, eventData: unknown) => boolean {
    const fn = new Function("eventType", "eventData", code) as (eventType: string, eventData: unknown) => boolean;
    return (eventType, eventData) => {
        try {
            return !!fn(eventType, eventData);
        } catch (err) {
            console.error("Filter code threw an error:", err);
            return false;
        }
    };
}