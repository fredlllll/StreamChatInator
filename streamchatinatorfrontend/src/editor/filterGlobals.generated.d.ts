// AUTO-GENERATED from src/chatEventTypes.ts — do not edit.
// Regenerate with `npm run generate:editor-types` (also runs automatically on dev/build).
//
// Mirrors the chat event data serialized to JSON (camelCase) and passed to
// the filter script's `__matches(eventData)` — both in the browser
// (new Function: run script, then call __matches) and on the server (Jint).

// Type aliases used by the payload fields below, copied verbatim from chatEventTypes.ts.
type ChatEventType = "None" | "Announcement" | "AnonGiftPaidUpgrade" | "BitsBadgeTier" | "ChatMessage" | "CommunityPayForward" | "CommunitySubscription" | "ContinuedGiftedSubscription" | "GiftedSubscription" | "MessageCleared" | "NewSubscriber" | "PrimePaidSubscriber" | "ReSubscriber" | "Ritual" | "StandardPayForward" | "UserBanned" | "UserJoined" | "UserLeft" | "UserTimedout";
type NotSetTrueFalse = 0 | 1 | 2;
type UserFlagName = "Moderator" | "Turbo" | "Subscriber" | "Vip" | "Partner" | "Staff";
type UserTypeName = "Viewer" | "Moderator" | "GlobalModerator" | "Broadcaster" | "Admin" | "Staff";
type float = number;
type int = number;

// Chat event payload interfaces (and the base interfaces they extend), copied
// verbatim from chatEventTypes.ts, so you can cast or type against the real
// payload type, e.g. `eventData.chatEventData as ChatEventAnnouncement`.
interface Model {
    id: string;
    created: string;
    updated: string;
}

interface FrontEndEventData<T = unknown> {
    eventId: string;
    chatEventType: ChatEventType;
    seen: boolean;
    chatEventData: T;
}

interface ChatEventChatMessage extends Model {
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
    badges: string | null;
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

interface ChatUserNoticeBase extends Model {
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
    badges: string | null;
    userFlagsNames: UserFlagName[];
    userId: string;
    userType: int;
    userTypeName: UserTypeName;
}

interface ChatEventAnnouncement extends ChatUserNoticeBase {
    msgParamColor: string;
    message: string;
}

interface ChatEventAnonGiftPaidUpgrade extends ChatUserNoticeBase {
    msgParamPromoGiftTotal: int;
    msgParamPromoName: string;
}

interface ChatEventBitsBadgeTier extends ChatUserNoticeBase {
    msgParamThreshold: int;
}

interface ChatEventCommunityPayForward extends ChatUserNoticeBase {
    msgParamPriorGifterAnonymous: boolean;
    msgParamPriorGifterDisplayName: string;
    msgParamPriorGifterId: string;
    msgParamPriorGifterUserName: string;
}

interface ChatEventCommunitySubscription extends ChatUserNoticeBase {
    isAnonymous: boolean;
    msgParamGiftTheme: string;
    msgParamMassGiftCount: int;
    msgParamOriginId: string;
    msgParamSenderCount: int;
    msgParamSubPlan: int;
    msgParamSubPlanName: string;
}

interface ChatEventContinuedGiftedSubscription extends ChatUserNoticeBase {
    msgParamPromoGiftTotal: int;
    msgParamPromoName: string;
    msgParamSenderName: string;
    msgParamSenderUsername: string;
}

interface ChatEventGiftedSubscription extends ChatUserNoticeBase {
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

interface ChatEventMessageCleared extends Model {
    channel: string;
    message: string;
    targetTwitchMessageId: string;
    tmiSent: string;
}

interface ChatEventNewSubscriber extends ChatUserNoticeBase {
    msgParamCumulativeMonths: int;
    msgParamShouldShareStreak: boolean;
    msgParamStreakMonths: int;
    msgParamSubPlan: int;
    msgParamSubPlanName: string;
    resubMessage: string;
}

interface ChatEventPrimePaidSubscriber extends ChatUserNoticeBase {
    msgParamSubPlan: int;
    msgParamSubPlanName: string;
    resubMessage: string;
}

interface ChatEventReSubscriber extends ChatUserNoticeBase {
    msgParamCumulativeMonths: int;
    msgParamShouldShareStreak: boolean;
    msgParamStreakMonths: int;
    msgParamSubPlan: int;
    msgParamSubPlanName: string;
    resubMessage: string;
}

interface ChatEventRitual extends ChatUserNoticeBase {
    msgParamRitualName: string;
    message: string;
}

interface ChatEventStandardPayForward extends ChatUserNoticeBase {
    msgParamPriorGifterAnonymous: boolean;
    msgParamPriorGifterDisplayName: string;
    msgParamPriorGifterId: string;
    msgParamPriorGifterUserName: string;
    msgParamRecipientDisplayName: string | null;
    msgParamRecipientId: string | null;
    msgParamRecipientUserName: string | null;
}

interface ChatEventUserBanned extends Model {
    channel: string;
    username: string;
    roomId: string;
    targetUserId: string;
}

interface ChatEventUserJoined extends Model {
    username: string;
    channel: string;
}

interface ChatEventUserLeft extends Model {
    username: string;
    channel: string;
}

interface ChatEventUserTimedout extends Model {
    channel: string;
    timeoutDuration: string; //TimeSpan //TODO: idk how this will be serialized
    username: string;
    targetUserId: string;
}

/** Maps every ChatEventType to the payload it carries ("None" has no payload). */
type ChatEventDataByType = {
    None: unknown;
    Announcement: ChatEventAnnouncement;
    AnonGiftPaidUpgrade: ChatEventAnonGiftPaidUpgrade;
    BitsBadgeTier: ChatEventBitsBadgeTier;
    ChatMessage: ChatEventChatMessage;
    CommunityPayForward: ChatEventCommunityPayForward;
    CommunitySubscription: ChatEventCommunitySubscription;
    ContinuedGiftedSubscription: ChatEventContinuedGiftedSubscription;
    GiftedSubscription: ChatEventGiftedSubscription;
    MessageCleared: ChatEventMessageCleared;
    NewSubscriber: ChatEventNewSubscriber;
    PrimePaidSubscriber: ChatEventPrimePaidSubscriber;
    ReSubscriber: ChatEventReSubscriber;
    Ritual: ChatEventRitual;
    StandardPayForward: ChatEventStandardPayForward;
    UserBanned: ChatEventUserBanned;
    UserJoined: ChatEventUserJoined;
    UserLeft: ChatEventUserLeft;
    UserTimedout: ChatEventUserTimedout;
};

/** Payload of any chat event: `ChatEventAnnouncement | ChatEventAnonGiftPaidUpgrade | ... | ChatEventUserTimedout`. */
type ChatEventDataUnion = ChatEventDataByType[ChatEventType];

/** Every field that exists on ANY chat event payload (all optional, so they autocomplete regardless of chatEventType). */
interface ChatEventDataMerged {
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    badges?: string | null;
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
    msgParamMonths?: number;
    /** Present on: ChatEventGiftedSubscription */
    msgParamMultiMonthGiftDuration?: number;
    /** Present on: ChatEventCommunitySubscription, ChatEventGiftedSubscription */
    msgParamOriginId?: string;
    /** Present on: ChatEventCommunityPayForward, ChatEventStandardPayForward */
    msgParamPriorGifterAnonymous?: boolean;
    /** Present on: ChatEventCommunityPayForward, ChatEventStandardPayForward */
    msgParamPriorGifterDisplayName?: string;
    /** Present on: ChatEventCommunityPayForward, ChatEventStandardPayForward */
    msgParamPriorGifterId?: string;
    /** Present on: ChatEventCommunityPayForward, ChatEventStandardPayForward */
    msgParamPriorGifterUserName?: string;
    /** Present on: ChatEventAnonGiftPaidUpgrade, ChatEventContinuedGiftedSubscription */
    msgParamPromoGiftTotal?: number;
    /** Present on: ChatEventAnonGiftPaidUpgrade, ChatEventContinuedGiftedSubscription */
    msgParamPromoName?: string;
    /** Present on: ChatEventGiftedSubscription, ChatEventStandardPayForward */
    msgParamRecipientDisplayName?: string | null;
    /** Present on: ChatEventGiftedSubscription, ChatEventStandardPayForward */
    msgParamRecipientId?: string | null;
    /** Present on: ChatEventGiftedSubscription, ChatEventStandardPayForward */
    msgParamRecipientUserName?: string | null;
    /** Present on: ChatEventRitual */
    msgParamRitualName?: string;
    /** Present on: ChatEventCommunitySubscription, ChatEventGiftedSubscription */
    msgParamSenderCount?: number;
    /** Present on: ChatEventContinuedGiftedSubscription */
    msgParamSenderName?: string;
    /** Present on: ChatEventContinuedGiftedSubscription */
    msgParamSenderUsername?: string;
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
    targetTwitchMessageId?: string;
    /** Present on: ChatEventUserBanned, ChatEventUserTimedout */
    targetUserId?: string;
    /** Present on: ChatEventUserTimedout */
    timeoutDuration?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventMessageCleared, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    tmiSent?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    twitchMessageId?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventMessageCleared, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward, ChatEventUserBanned, ChatEventUserJoined, ChatEventUserLeft, ChatEventUserTimedout */
    updated?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    userFlags?: number;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    userFlagsNames?: UserFlagName[];
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    userId?: string;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    userType?: number;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward */
    userTypeName?: UserTypeName;
    /** Present on: ChatEventAnnouncement, ChatEventAnonGiftPaidUpgrade, ChatEventBitsBadgeTier, ChatEventChatMessage, ChatEventCommunityPayForward, ChatEventCommunitySubscription, ChatEventContinuedGiftedSubscription, ChatEventGiftedSubscription, ChatEventNewSubscriber, ChatEventPrimePaidSubscriber, ChatEventReSubscriber, ChatEventRitual, ChatEventStandardPayForward, ChatEventUserBanned, ChatEventUserJoined, ChatEventUserLeft, ChatEventUserTimedout */
    username?: string;
}

/**
 * The event envelope, discriminated on `chatEventType`. `chatEventData`
 * narrows to the matching payload type automatically:
 *     if (eventData.chatEventType === "ChatMessage") {
 *         eventData.chatEventData.username; // ChatEventChatMessage
 *     }
 * or cast explicitly: `eventData.chatEventData as ChatEventAnnouncement`.
 */
type ChatEventEnvelope = {
    [K in ChatEventType]: {
        eventId: string;
        chatEventType: K;
        seen: boolean;
        chatEventData: ChatEventDataByType[K];
    };
}[ChatEventType];

declare const eventData: ChatEventEnvelope;
