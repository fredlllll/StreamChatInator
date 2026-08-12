import type { FrontEndEventData, ChatEventGiftedSubscription } from "../../types";
import UserNoticeItem from "./UserNoticeItem";

type GiftedSubscriptionItemProps = {
    event: FrontEndEventData<ChatEventGiftedSubscription>;
};

function GiftedSubscriptionItem({ event }: GiftedSubscriptionItemProps) {
    const data = event.chatEventData;

    const chips: string[] = [];
    if (data.msgParamSubPlanName) chips.push(data.msgParamSubPlanName);
    if (data.msgParamMultiMonthGiftDuration > 1) chips.push(`${data.msgParamMultiMonthGiftDuration}-mo gift`);
    if (data.msgParamMonths > 1) chips.push(`${data.msgParamMonths} months`);

    return <UserNoticeItem event={event} pill="Gift Sub" chips={chips} />;
}

export default GiftedSubscriptionItem;
