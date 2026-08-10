// AUTO-GENERATED from src/types.ts — do not edit.
// Regenerate with `npm run generate:editor-types` (also runs automatically on dev/build).
//
// Mirrors the chat event data serialized to JSON (camelCase) and passed to
// the filter code as `eventData` — both in the browser
// (new Function("eventData", code)) and on the server (Jint:
// function __matches(eventData) { ... }).

// Type aliases used by the payload fields below, copied verbatim from types.ts.
type ChatEventType = "None" | "Announcement" | "AnonGiftPaidUpgrade" | "BitsBadgeTier" | "ChatMessage" | "CommunityPayForward" | "CommunitySubscription" | "ContinuedGiftedSubscription" | "GiftedSubscription" | "MessageCleared" | "NewSubscriber" | "PrimePaidSubscriber" | "ReSubscriber" | "Ritual" | "StandardPayForward" | "UserBanned" | "UserJoined" | "UserLeft" | "UserTimedout";
type NotSetTrueFalse = 0 | 1 | 2;
type UserFlagName = "Moderator" | "Turbo" | "Subscriber" | "Vip" | "Partner" | "Staff";
type UserTypeName = "Viewer" | "Moderator" | "GlobalModerator" | "Broadcaster" | "Admin" | "Staff";
type float = number;
type int = number;

interface ChatEventEnvelope {
    eventId: string;
    chatEventType: ChatEventType;
    seen: boolean;
    /** Payload of the event. Present on one of: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventMessageCleared, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward, ChatEventUserBanned, ChatEventUserJoined, ChatEventUserLeft, ChatEventUserTimedout. */
    chatEventData: ChatEventDataMerged;
}

/** Every field that exists on ANY chat event payload (all optional, so they autocomplete regardless of chatEventType). */
interface ChatEventDataMerged {
    /** Present on: ChatEventChatMessage */
    bits?: number;
    /** Present on: ChatEventChatMessage */
    bitsInDollars?: number;
    /** Present on: ChatEventMessageCleared, ChatEventUserBanned, ChatEventUserJoined, ChatEventUserLeft, ChatEventUserTimedout */
    channel?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventMessageCleared, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward, ChatEventUserBanned, ChatEventUserJoined, ChatEventUserLeft, ChatEventUserTimedout */
    created?: string;
    /** Present on: ChatEventChatMessage */
    customRewardId?: string | null;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    displayName?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    emotes?: string | null;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    hexColor?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventMessageCleared, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward, ChatEventUserBanned, ChatEventUserJoined, ChatEventUserLeft, ChatEventUserTimedout */
    id?: string;
    /** Present on: ChatEventCommunitySubscription, ChatEventGiftedSubscription */
    isAnonymous?: boolean;
    /** Present on: ChatEventChatMessage */
    isBroadcaster?: boolean;
    /** Present on: ChatEventChatMessage */
    isFirstMessage?: boolean;
    /** Present on: ChatEventChatMessage */
    isHighlighted?: boolean;
    /** Present on: ChatEventChatMessage */
    isMe?: boolean;
    /** Present on: ChatEventChatMessage */
    isReply?: boolean;
    /** Present on: ChatEventChatMessage */
    isSkippingSubMode?: boolean;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    login?: string;
    /** Present on: ChatEventAnnouncement, ChatEventChatMessage, ChatEventMessageCleared, ChatEventRitual */
    message?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    msgId?: string;
    /** Present on: ChatEventAnnouncement */
    msgParamColor?: string;
    /** Present on: ChatEventNewSubscriber, ChatEventReSubscriber */
    msgParamCumulativeMonths?: number;
    /** Present on: ChatEventCommunitySubscription */
    msgParamGiftTheme?: string;
    /** Present on: ChatEventCommunitySubscription */
    msgParamMassGiftCount?: number;
    /** Present on: ChatEventGiftedSubscription */
    msgParamMonths?: string;
    /** Present on: ChatEventGiftedSubscription */
    msgParamMultiMonthGiftDuration?: number;
    /** Present on: ChatEventCommunitySubscription, ChatEventGiftedSubscription */
    msgParamOriginId?: string;
    /** Present on: ChatEventCommunityPayForward, ChatEventStandardPayForward */
    msgParamPriorGifterAnonymous?: boolean;
    /** Present on: ChatEventCommunityPayForward, ChatEventStandardPayForward */
    msgParamPriorGifterDisplayName?: string;
    /** Present on: ChatEventCommunityPayForward, ChatEventStandardPayForward */
    msgParamPriorGifterId?: string | number;
    /** Present on: ChatEventCommunityPayForward, ChatEventStandardPayForward */
    msgParamPriorGifterUserName?: string;
    /** Present on: ChatEventAnonGiftPaidUpgrade, ChatEventContinuedGiftedSubscription */
    msgParamPromoGiftTotal?: number;
    /** Present on: ChatEventAnonGiftPaidUpgrade, ChatEventContinuedGiftedSubscription */
    msgParamPromoName?: string;
    /** Present on: ChatEventGiftedSubscription, ChatEventStandardPayForward */
    msgParamRecipientDisplayName?: string | null;
    /** Present on: ChatEventGiftedSubscription, ChatEventStandardPayForward */
    msgParamRecipientId?: string | number | null;
    /** Present on: ChatEventGiftedSubscription, ChatEventStandardPayForward */
    msgParamRecipientUserName?: string | null;
    /** Present on: ChatEventRitual */
    msgParamRitualName?: string;
    /** Present on: ChatEventCommunitySubscription, ChatEventGiftedSubscription */
    msgParamSenderCount?: number;
    /** Present on: ChatEventContinuedGiftedSubscription */
    msgParamSenderLogin?: string;
    /** Present on: ChatEventContinuedGiftedSubscription */
    msgParamSenderName?: string;
    /** Present on: ChatEventNewSubscriber, ChatEventReSubscriber */
    msgParamShouldShareStreak?: boolean;
    /** Present on: ChatEventNewSubscriber, ChatEventReSubscriber */
    msgParamStreakMonths?: number;
    /** Present on: ChatEventCommunitySubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber */
    msgParamSubPlan?: number;
    /** Present on: ChatEventCommunitySubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber */
    msgParamSubPlanName?: string;
    /** Present on: ChatEventBitsBadgeTier */
    msgParamThreshold?: number;
    /** Present on: ChatEventChatMessage */
    noisy?: NotSetTrueFalse;
    /** Present on: ChatEventChatMessage */
    replyParentMessageTwitchMessageId?: string | null;
    /** Present on: ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber */
    resubMessage?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward, ChatEventUserBanned */
    roomId?: string;
    /** Present on: ChatEventChatMessage */
    subscribedMonthCount?: number;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    systemMsg?: string;
    /** Present on: ChatEventMessageCleared */
    targetMessageId?: string;
    /** Present on: ChatEventUserBanned, ChatEventUserTimedout */
    targetUserId?: string;
    /** Present on: ChatEventUserTimedout */
    timeoutDuration?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventMessageCleared, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    tmiSent?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    twitchId?: string;
    /** Present on: ChatEventChatMessage */
    twitchMessageId?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventMessageCleared, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward, ChatEventUserBanned, ChatEventUserJoined, ChatEventUserLeft, ChatEventUserTimedout */
    updated?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    userFlags?: number;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    userFlagsNames?: UserFlagName[];
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    userId?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    userType?: number;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    userTypeName?: UserTypeName;
    /** Present on: ChatEventChatMessage, ChatEventUserBanned, ChatEventUserJoined, ChatEventUserLeft, ChatEventUserTimedout */
    username?: string;
}

declare const eventData: ChatEventEnvelope;
