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

const USER_TYPE_BADGES: Partial<Record<UserTypeName, BadgeSlot>> = {
    Broadcaster: { set: "broadcaster", version: "1", fallback: "Streamer" },
    Moderator: { set: "moderator", version: "1", fallback: "Mod" },
    GlobalModerator: { set: "global_mod", version: "1", fallback: "Global Mod" },
    Admin: { set: "admin", version: "1", fallback: "Admin" },
    Staff: { set: "staff", version: "1", fallback: "Staff" },
};

const USER_FLAG_BADGES: Partial<Record<UserFlagName, BadgeSlot>> = {
    Moderator: { set: "moderator", version: "1", fallback: "Mod" },
    Subscriber: { set: "subscriber", version: "1", fallback: "Sub" },
    Vip: { set: "vip", version: "1", fallback: "Vip" },
    Partner: { set: "partner", version: "1", fallback: "Partner" },
    Turbo: { set: "turbo", version: "1", fallback: "Turbo" },
    Staff: { set: "staff", version: "1", fallback: "Staff" },
};

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
    if (userTypeName && USER_TYPE_BADGES[userTypeName]) {
        slots.push(USER_TYPE_BADGES[userTypeName]);
    }
    if (userFlagsNames) {
        for (const flag of userFlagsNames) {
            const slot = USER_FLAG_BADGES[flag];
            if (slot && !slots.some((s) => s.set === slot.set)) {
                slots.push(slot);
            }
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