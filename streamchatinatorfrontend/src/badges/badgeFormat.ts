import { BADGE_LABELS, BADGE_TITLES, type BadgeSlot } from "./badgeDefinitions";

/** Tooltip text for a fallback badge: full name, plus months for subscribers. */
export function buildBadgeTitle(set: string, version: string): string {
    const base = BADGE_TITLES[set] ?? set;
    if (set === "subscriber" && /^\d+$/.test(version) && +version > 0) {
        return `${base} (${version} months)`;
    }
    return base;
}

// Parses the JSON badge array Twitch sends in the message's badge tag:
//     [{"set":"broadcaster","version":"1"},{"set":"subscriber","version":"24"}]
export function parseMessageBadges(raw: string | null | undefined): BadgeSlot[] {
    if (!raw) return [];
    try {
        const parsed: unknown = JSON.parse(raw);
        if (!Array.isArray(parsed)) return [];
        const slots: BadgeSlot[] = [];
        for (const badge of parsed) {
            if (badge == null || typeof badge !== "object") continue;
            const b = badge as { set?: unknown; version?: unknown };
            if (typeof b.set !== "string" || b.set.length === 0) continue;
            const version = typeof b.version === "string" && b.version.length > 0 ? b.version : "0";
            slots.push({ set: b.set, version, fallback: BADGE_LABELS[b.set] ?? b.set });
        }
        return slots;
    } catch {
        return [];
    }
}