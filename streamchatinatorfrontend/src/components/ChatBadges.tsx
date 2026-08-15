import { useEffect, useState } from "react";
import { useChatConnection } from "../ChatContext";
import { getBadgesCached } from "../api/badgesApi";
import type { BadgeMap, FrontEndEventData, UserFlagName, UserTypeName } from "../types";
import { USER_FLAG_BADGES, USER_TYPE_BADGES, type BadgeSlot } from "../badges/badgeDefinitions";
import { buildBadgeTitle, parseMessageBadges } from "../badges/badgeFormat";

type ChatBadgesItemProps = {
    event: FrontEndEventData<any>;
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
                    <span key={slot.set} className="badge" title={buildBadgeTitle(slot.set, slot.version)}>
                        {slot.fallback}
                    </span>
                );
            })}
        </span>
    );
}

export default ChatBadges;