import type { FrontEndEventData, ChatEventCommunitySubscription } from "../../types";
import UserNoticeItem, { type InfoChip } from "./UserNoticeItem";

type CommunitySubscriptionItemProps = {
    event: FrontEndEventData<ChatEventCommunitySubscription>;
};

function CommunitySubscriptionItem({ event }: CommunitySubscriptionItemProps) {
    const data = event.chatEventData;

    const chips: InfoChip[] = [];
    if (data.msgParamSubPlanName) {
        chips.push({ label: data.msgParamSubPlanName, title: `Subscription plan: ${data.msgParamSubPlanName}` });
    }
    if (data.msgParamMassGiftCount > 0) {
        chips.push({
            label: `${data.msgParamMassGiftCount} subs`,
            title: `Subs gifted to the community: ${data.msgParamMassGiftCount}`,
        });
    }
    if (data.msgParamSenderCount > 0) {
        chips.push({
            label: `${data.msgParamSenderCount} total`,
            title: `Total subs gifted by the sender: ${data.msgParamSenderCount}`,
        });
    }

    return <UserNoticeItem event={event} pill="Mass Gift" chips={chips} />;
}

export default CommunitySubscriptionItem;
