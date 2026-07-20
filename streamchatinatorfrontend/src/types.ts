type int = number;
type float = number;
type NotSetTrueFalse = 0 | 1 | 2;
type UserFlagName = "Moderator" | "Turbo" | "Subscriber" | "Vip" | "Partner" | "Staff";

export interface EventEnvelope<T = unknown> {
    type: string;
    data: T;
}

export interface ChatMessageData {
    bits: int;
    bitsInDollars: float;
    customRewardId: string | null;
    emoteReplacedMessage: string | null;
    hasEmotes: boolean;
    twitchMessageId: string;
    isFirstMessage: boolean;
    isHighlighted: boolean;
    isMe: boolean;
    isSkippingSubMode: boolean;
    noisy: NotSetTrueFalse;
    subscribedMonthCount: int;
    replyParentMessageTwitchMessageId: string | null;
    isReply: boolean;
    userId: string;
    userFlags: int;
    created: string;
    updated: string;
    id: string;
    username: string;
    displayName: string;
    message: string;
    hexColor: string;
    isBroadcaster: boolean;
    tmiSent: string; // ISO date string - we'll parse it when we need to display it
    userFlagNames: UserFlagName[];
}

export interface EventFilter {
    id: string;
    name: string;
    code: string;
    created: string;
    updated: string;
}