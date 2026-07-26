import type { ComponentType } from "react";
import ChatMessageItem from "./ChatMessageItem";
import type { FrontEndEventData, ChatEventType } from "../types";

const eventComponents: Record<ChatEventType, ComponentType<{ event: FrontEndEventData<any> }> | undefined> = {
    ChatMessage: ChatMessageItem,
    //TODO: the rest
    None: undefined,
    Announcement: undefined,
    AnonGiftPaidUpgrade: undefined,
    BitsBadgeTier: undefined,
    CommunityPayForward: undefined,
    CommunitySubscription: undefined,
    ContinuedGiftedSubscription: undefined,
    GiftedSubscription: undefined,
    MessageCleared: undefined,
    NewSubscriber: undefined,
    PrimePaidSubscriber: undefined,
    ReSubscriber: undefined,
    Ritual: undefined,
    StandardPayForward: undefined,
    UserBanned: undefined,
    UserJoined: undefined,
    UserLeft: undefined,
    UserTimedout: undefined
};

export default eventComponents;