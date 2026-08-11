import { useEffect, useState } from "react";
import { useChatConnection } from "../ChatContext";
import { getBadgesCached } from "../api/badgesApi";
import type { BadgeMap, FrontEndEventData, UserFlagName, UserTypeName } from "../types";

type ChatBadgesItemProps = {
    event: FrontEndEventData<any>;
};

type BadgeSlot = {
    set: string;
    version: string;
    fallback: string;
};

// Fallback labels shown (as text) when a badge's image can't be found in the
// fetched badge map. The message's `badges` tag is the ground truth; the
// flag/type fallbacks below are only used for badges Twitch didn't put there.
const BADGE_LABELS: Record<string, string> = {
    broadcaster: "Streamer",
    moderator: "Mod",
    vip: "Vip",
    subscriber: "Sub",
    founder: "Founder",
    global_mod: "Global Mod",
    admin: "Admin",
    staff: "Staff",
    turbo: "Turbo",
    partner: "Partner",
    bits: "Bits",
    bits_charity: "Bits",
    bits_leaderboard: "Bits Leader",
    prediction: "Prediction",
    predictions: "Prediction",
    sub_gift_leaderboard: "Sub Gifter",
    sub_gifter: "Sub Gifter",
    premium: "Prime",
    no_audio: "No Audio",
    no_video: "No Video",
    uploader: "Uploader",
};

const USER_TYPE_BADGES: Partial<Record<UserTypeName, BadgeSlot>> = {
    Broadcaster: { set: "broadcaster", version: "1", fallback: "Streamer" },
    Moderator: { set: "moderator", version: "1", fallback: "Mod" },
    GlobalModerator: { set: "global_mod", version: "1", fallback: "Global Mod" },
    Admin: { set: "admin", version: "1", fallback: "Admin" },
    Staff: { set: "staff", version: "1", fallback: "Staff" },
};

const USER_FLAG_BADGES: Partial<Record<UserFlagName, BadgeSlot>> = {
    Moderator: { set: "moderator", version: "1", fallback: "Mod" },
    Subscriber: { set: "subscriber", version: "0", fallback: "Sub" },
    Vip: { set: "vip", version: "1", fallback: "Vip" },
    Partner: { set: "partner", version: "1", fallback: "Partner" },
    Turbo: { set: "turbo", version: "1", fallback: "Turbo" },
    Staff: { set: "staff", version: "1", fallback: "Staff" },
};

// Parses the JSON badge array Twitch sends in the message's badge tag:
//     [{"set":"broadcaster","version":"1"},{"set":"subscriber","version":"24"}]
function parseMessageBadges(raw: string | null | undefined): BadgeSlot[] {
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

function ChatBadges({ event }: ChatBadgesItemProps) {
    const { channelId } = useChatConnection();
    const [badges, setBadges] = useState<BadgeMap | null>(null);

    useEffect(() => {
        let cancelled = false;
        getBadgesCached(channelId).then((loaded) => {
            if (!cancelled) setBadges(loaded);
        });
        return () => {
            cancelled = true;
        };
    }, [channelId]);

    const data = event.chatEventData;
    const userFlagsNames: UserFlagName[] | undefined = data.userFlagsNames;
    const userTypeName: UserTypeName | undefined = data.userTypeName;

    const slots: BadgeSlot[] = [];
    const seenSets = new Set<string>();
    for (const slot of parseMessageBadges(data.badges)) {
        slots.push(slot);
        seenSets.add(slot.set);
    }
    const addFallbackSlot = (slot: BadgeSlot | undefined) => {
        if (slot && !seenSets.has(slot.set)) {
            slots.push(slot);
            seenSets.add(slot.set);
        }
    };
    if (userTypeName) {
        addFallbackSlot(USER_TYPE_BADGES[userTypeName]);
    }
    if (userFlagsNames) {
        for (const flag of userFlagsNames) {
            addFallbackSlot(USER_FLAG_BADGES[flag]);
        }
    }
    // Broadcasters auto-moderate their own chat, so the mod badge is redundant
    // next to the broadcaster one (Twitch hides it too).
    const visibleSlots = slots.some((s) => s.set === "broadcaster")
        ? slots.filter((s) => s.set !== "moderator")
        : slots;

    return (
        <span>
            {visibleSlots.map((slot) => {
                const badge = badges?.[slot.set]?.[slot.version];
                if (badge) {
                    const img = (
                        <img
                            key={slot.set}
                            className="chat-badge-image"
                            src={badge.imageUrl}
                            alt={slot.fallback}
                            title={badge.title}
                        />
                    );
                    if (badge.clickUrl) {
                        return (
                            <a
                                key={slot.set}
                                className="chat-badge-link"
                                href={badge.clickUrl}
                                target="_blank"
                                rel="noreferrer"
                                title={badge.title}
                            >
                                {img}
                            </a>
                        );
                    }
                    return img;
                }
                return (
                    <span key={slot.set} className="badge">
                        {slot.fallback}
                    </span>
                );
            })}
        </span>
    );
}

export default ChatBadges;