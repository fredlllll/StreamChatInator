import type { FrontEndEventData, ChatEventCommunitySubscription } from "../../types";
import UserNoticeItem from "./UserNoticeItem";

type CommunitySubscriptionItemProps = {
    event: FrontEndEventData<ChatEventCommunitySubscription>;
};

function CommunitySubscriptionItem({ event }: CommunitySubscriptionItemProps) {
    const data = event.chatEventData;

    const chips: string[] = [];
    if (data.msgParamSubPlanName) chips.push(data.msgParamSubPlanName);
    if (data.msgParamMassGiftCount > 0) chips.push(`${data.msgParamMassGiftCount} subs`);
    if (data.msgParamSenderCount > 0) chips.push(`${data.msgParamSenderCount} total`);

    return <UserNoticeItem event={event} pill="Mass Gift" chips={chips} />;
}

export default CommunitySubscriptionItem;
