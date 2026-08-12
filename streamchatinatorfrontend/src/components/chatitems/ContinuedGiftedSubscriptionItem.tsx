import type { FrontEndEventData, ChatEventContinuedGiftedSubscription } from "../../types";
import UserNoticeItem from "./UserNoticeItem";

type ContinuedGiftedSubscriptionItemProps = {
    event: FrontEndEventData<ChatEventContinuedGiftedSubscription>;
};

function ContinuedGiftedSubscriptionItem({ event }: ContinuedGiftedSubscriptionItemProps) {
    const data = event.chatEventData;

    const chips: string[] = [];
    if (data.msgParamSenderName) chips.push(`from ${data.msgParamSenderName}`);
    if (data.msgParamPromoName) chips.push(`promo: ${data.msgParamPromoName}`);
    if (data.msgParamPromoGiftTotal > 0) chips.push(`${data.msgParamPromoGiftTotal} gifts`);

    return <UserNoticeItem event={event} pill="Gift Upgrade" chips={chips} />;
}

export default ContinuedGiftedSubscriptionItem;
