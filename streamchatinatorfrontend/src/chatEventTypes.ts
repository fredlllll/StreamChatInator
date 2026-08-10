export type int = number;
export type float = number;
export type NotSetTrueFalse = 0 | 1 | 2;
export type UserFlagName = "Moderator" | "Turbo" | "Subscriber" | "Vip" | "Partner" | "Staff";
export type ChatEventType = "None" | "Announcement" | "AnonGiftPaidUpgrade" | "BitsBadgeTier" | "ChatMessage" | "CommunityPayForward" | "CommunitySubscription" | "ContinuedGiftedSubscription" | "GiftedSubscription" | "MessageCleared" | "NewSubscriber" | "PrimePaidSubscriber" | "ReSubscriber" | "Ritual" | "StandardPayForward" | "UserBanned" | "UserJoined" | "UserLeft" | "UserTimedout";
export type UserTypeName = "Viewer" | "Moderator" | "GlobalModerator" | "Broadcaster" | "Admin" | "Staff";

export interface Model {
    id: string,
    created: string,
    updated: string;
}

export interface FrontEndEventData<T = unknown> {
    eventId: string;
    chatEventType: ChatEventType;
    seen: boolean;
    chatEventData: T;
}

export interface ChatEventChatMessage extends Model {
    bits: int;
    bitsInDollars: float;
    emotes: string | null;
    customRewardId: string | null;
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
    username: string;
    displayName: string;
    message: string;
    hexColor: string;
    isBroadcaster: boolean;
    tmiSent: string; // ISO date string - we'll parse it when we need to display it
    userFlagsNames: UserFlagName[];
    userType: int;
    userTypeName: UserTypeName;
}

export interface ChatUserNoticeBase extends Model {
    hexColor: string;
    displayName: string;
    emotes: string;
    twitchMessageId: string;
    username: string;
    msgId: string;
    roomId: string;
    systemMsg: string;
    tmiSent: string;
    userFlags: int;
    userFlagsNames: UserFlagName[];
    userId: string;
    userType: int;
    userTypeName: UserTypeName;
}

export interface ChatEventAnnouncement extends ChatUserNoticeBase {
    msgParamColor: string;
    message: string;
}

export interface ChatEventAnonGiftPaidUpgrade extends ChatUserNoticeBase {
    msgParamPromoGiftTotal: int;
    msgParamPromoName: string;
}

export interface ChatEventBitsBadgeTier extends ChatUserNoticeBase {
    msgParamThreshold: int;
}

export interface ChatEventCommunityPayForward extends ChatUserNoticeBase {
    msgParamPriorGifterAnonymous: boolean;
    msgParamPriorGifterDisplayName: string;
    msgParamPriorGifterId: string;
    msgParamPriorGifterUserName: string;
}

export interface ChatEventCommunitySubscription extends ChatUserNoticeBase {
    isAnonymous: boolean;
    msgParamGiftTheme: string;
    msgParamMassGiftCount: int;
    msgParamOriginId: string;
    msgParamSenderCount: int;
    msgParamSubPlan: int;
    msgParamSubPlanName: string;
}

export interface ChatEventContinuedGiftedSubscription extends ChatUserNoticeBase {
    msgParamPromoGiftTotal: int;
    msgParamPromoName: string;
    msgParamSenderName: string;
    msgParamSenderUsername: string;
}

export interface ChatEventGiftedSubscription extends ChatUserNoticeBase {
    isAnonymous: boolean;
    msgParamMonths: int;
    msgParamOriginId: string;
    msgParamRecipientDisplayName: string;
    msgParamRecipientId: string;
    msgParamRecipientUserName: string;
    msgParamSenderCount: int;
    msgParamSubPlan: int;
    msgParamSubPlanName: string;
    msgParamMultiMonthGiftDuration: int;
}

export interface ChatEventMessageCleared extends Model {
    channel: string;
    message: string;
    targetTwitchMessageId: string;
    tmiSent: string;
}

export interface ChatEventNewSubscriber extends ChatUserNoticeBase {
    msgParamCumulativeMonths: int;
    msgParamShouldShareStreak: boolean;
    msgParamStreakMonths: int;
    msgParamSubPlan: int;
    msgParamSubPlanName: string;
    resubMessage: string;
}

export interface ChatEventPrimePaidSubscriber extends ChatUserNoticeBase {
    msgParamSubPlan: int;
    msgParamSubPlanName: string;
    resubMessage: string;
}

export interface ChatEventReSubscriber extends ChatUserNoticeBase {
    msgParamCumulativeMonths: int;
    msgParamShouldShareStreak: boolean;
    msgParamStreakMonths: int;
    msgParamSubPlan: int;
    msgParamSubPlanName: string;
    resubMessage: string;
}

export interface ChatEventRitual extends ChatUserNoticeBase {
    msgParamRitualName: string;
    message: string;
}

export interface ChatEventStandardPayForward extends ChatUserNoticeBase {
    msgParamPriorGifterAnonymous: boolean;
    msgParamPriorGifterDisplayName: string;
    msgParamPriorGifterId: string;
    msgParamPriorGifterUserName: string;
    msgParamRecipientDisplayName: string | null;
    msgParamRecipientId: string | null;
    msgParamRecipientUserName: string | null;
}

export interface ChatEventUserBanned extends Model {
    channel: string;
    username: string;
    roomId: string;
    targetUserId: string;
}

export interface ChatEventUserJoined extends Model {
    username: string;
    channel: string;
}

export interface ChatEventUserLeft extends Model {
    username: string;
    channel: string;
}

export interface ChatEventUserTimedout extends Model {
    channel: string;
    timeoutDuration: string; //TimeSpan //TODO: idk how this will be serialized
    username: string;
    targetUserId: string;
}