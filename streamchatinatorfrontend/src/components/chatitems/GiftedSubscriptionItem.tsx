import type { FrontEndEventData, ChatEventGiftedSubscription } from "../../types";
import UserNoticeItem, { type InfoChip } from "./UserNoticeItem";

type GiftedSubscriptionItemProps = {
    event: FrontEndEventData<ChatEventGiftedSubscription>;
};

function GiftedSubscriptionItem({ event }: GiftedSubscriptionItemProps) {
    const data = event.chatEventData;

    const chips: InfoChip[] = [];
    if (data.msgParamSubPlanName) {
        chips.push({ label: data.msgParamSubPlanName, title: `Subscription plan: ${data.msgParamSubPlanName}` });
    }
    if (data.msgParamMultiMonthGiftDuration > 1) {
        chips.push({
            label: `${data.msgParamMultiMonthGiftDuration}-mo gift`,
            title: `Gifted ${data.msgParamMultiMonthGiftDuration} months of this subscription`,
        });
    }

    return <UserNoticeItem event={event} pill="Gift Sub" chips={chips} />;
}

export default GiftedSubscriptionItem;
