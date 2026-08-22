/**
 * Extracts the event timestamp as a `Date`, preferring the Twitch server
 * timestamp (`tmiSent`) and falling back to the event's database `created`
 * time. Both are ISO 8601 strings.
 */
function eventTimestamp(data: unknown): Date | null {
    if (data == null || typeof data !== "object") return null;
    const record = data as Record<string, unknown>;
    const iso =
        typeof record.tmiSent === "string" && record.tmiSent
            ? record.tmiSent
            : typeof record.created === "string" && record.created
              ? record.created
              : null;
    if (!iso) return null;
    const date = new Date(iso);
    return Number.isNaN(date.getTime()) ? null : date;
}

/** Formats an event's timestamp as a compact HH:MM in the viewer's local time. */
export function formatEventTime(data: unknown): string {
    const date = eventTimestamp(data);
    return date ? date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }) : "";
}

/** Formats an event's timestamp in full (date + time) for tooltips. */
export function formatFullTime(data: unknown): string {
    const date = eventTimestamp(data);
    return date ? date.toLocaleString([], { dateStyle: "medium", timeStyle: "short" }) : "";
}

function formatDuration(parts: { days: number; hours: number; minutes: number; seconds: number }): string {
    const out: string[] = [];
    if (parts.days) out.push(`${parts.days}d`);
    if (parts.hours) out.push(`${parts.hours}h`);
    if (parts.minutes) out.push(`${parts.minutes}m`);
    if (parts.seconds || out.length === 0) out.push(`${parts.seconds}s`);
    return out.join(" ");
}

/**
 * Formats a timeout duration into a compact "1h 30m" style string.
 * Handles the two ways .NET's TimeSpan can be serialized to JSON:
 * ISO 8601 durations ("PT1H30M10S") and the "c" constant format ("1.01:30:10").
 */
export function formatTimeSpan(value: string | number): string {
    if (typeof value === "number") {
        const total = Math.floor(value);
        return formatDuration({
            days: Math.floor(total / 86400),
            hours: Math.floor((total % 86400) / 3600),
            minutes: Math.floor((total % 3600) / 60),
            seconds: total % 60,
        });
    }

    const s = (value ?? "").trim();
    if (!s) return "";

    const iso = /^P(?:(\d+)D)?(?:T(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?)?$/.exec(s);
    if (iso) {
        return formatDuration({
            days: +(iso[1] ?? 0),
            hours: +(iso[2] ?? 0),
            minutes: +(iso[3] ?? 0),
            seconds: +(iso[4] ?? 0),
        });
    }

    // "c" format: [d.]hh:mm:ss[.fffffff]
    const segments = s.split(".")[0].split(":");
    if (segments.length === 3 || segments.length === 4) {
        const nums = segments.map((part) => parseInt(part, 10) || 0);
        const seconds = nums.pop() ?? 0;
        const minutes = nums.pop() ?? 0;
        const hours = nums.pop() ?? 0;
        const days = nums.length ? nums.pop() ?? 0 : 0;
        return formatDuration({ days, hours, minutes, seconds });
    }

    return s;
}
