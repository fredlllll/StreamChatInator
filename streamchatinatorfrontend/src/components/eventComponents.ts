import type { ComponentType } from "react";
import ChatMessageItem from "./chatitems/ChatMessageItem";
import type { FrontEndEventData, ChatEventType } from "../types";
import AnnouncementItem from "./chatitems/AnnouncementItem";
import AnonGiftPaidUpgradeItem from "./chatitems/AnonGiftPaidUpgradeItem";
import BitsBadgeTierItem from "./chatitems/BitsBadgeTierItem";
import CommunityPayForwardItem from "./chatitems/CommunityPayForwardItem";
import CommunitySubscriptionItem from "./chatitems/CommunitySubscriptionItem";
import ContinuedGiftedSubscriptionItem from "./chatitems/ContinuedGiftedSubscriptionItem";
import GiftedSubscriptionItem from "./chatitems/GiftedSubscriptionItem";
import MessageClearedItem from "./chatitems/MessageClearedItem";
import NewSubscriberItem from "./chatitems/NewSubscriberItem";
import PrimePaidSubscriberItem from "./chatitems/PrimePaidSubscriberItem";
import ReSubscriberItem from "./chatitems/ReSubscriberItem";
import RitualItem from "./chatitems/RitualItem";
import StandardPayForwardItem from "./chatitems/StandardPayForwardItem";
import UserBannedItem from "./chatitems/UserBannedItem";
import UserJoinedItem from "./chatitems/UserJoinedItem";
import UserLeftItem from "./chatitems/UserLeftItem";
import UserTimedoutItem from "./chatitems/UserTimedoutItem";

const eventComponents: Record<ChatEventType, ComponentType<{ event: FrontEndEventData<any> }> | undefined> = {
    ChatMessage: ChatMessageItem,
    //TODO: the rest
    Announcement: AnnouncementItem,
    AnonGiftPaidUpgrade: AnonGiftPaidUpgradeItem,
    BitsBadgeTier: BitsBadgeTierItem,
    CommunityPayForward: CommunityPayForwardItem,
    CommunitySubscription: CommunitySubscriptionItem,
    ContinuedGiftedSubscription: ContinuedGiftedSubscriptionItem,
    GiftedSubscription: GiftedSubscriptionItem,
    MessageCleared: MessageClearedItem,
    NewSubscriber: NewSubscriberItem,
    PrimePaidSubscriber: PrimePaidSubscriberItem,
    ReSubscriber: ReSubscriberItem,
    Ritual: RitualItem,
    StandardPayForward: StandardPayForwardItem,
    UserBanned: UserBannedItem,
    UserJoined: UserJoinedItem,
    UserLeft: UserLeftItem,
    UserTimedout: UserTimedoutItem,
    None: undefined //otherwise the compiler complains
};

export default eventComponents;