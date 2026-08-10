import type { BadgeMap } from "../types";

export async function getBadges(channelId: string | null): Promise<BadgeMap> {
    const params = channelId ? `?channelId=${encodeURIComponent(channelId)}` : "";
    const res = await fetch(`/api/badges${params}`);
    if (!res.ok) throw new Error("Failed to load badges");
    return res.json();
}

// Badge sets are big and change rarely; cache the latest fetch per channel
// for the lifetime of the page so every chat item shares one request.
const badgeCache = new Map<string, Promise<BadgeMap>>();

export function getBadgesCached(channelId: string | null): Promise<BadgeMap> {
    const key = channelId ?? "";
    const cached = badgeCache.get(key);
    if (cached) return cached;

    const promise = getBadges(channelId).catch(() => {
        badgeCache.delete(key);
        return {} as BadgeMap;
    });
    badgeCache.set(key, promise);
    return promise;
}