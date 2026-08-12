import type { FrontEndEventData, ChatEventContinuedGiftedSubscription } from "../../types";
import UserNoticeItem, { type InfoChip } from "./UserNoticeItem";

type ContinuedGiftedSubscriptionItemProps = {
    event: FrontEndEventData<ChatEventContinuedGiftedSubscription>;
};

function ContinuedGiftedSubscriptionItem({ event }: ContinuedGiftedSubscriptionItemProps) {
    const data = event.chatEventData;

    const chips: InfoChip[] = [];
    if (data.msgParamSenderName) {
        chips.push({ label: `from ${data.msgParamSenderName}`, title: `Gift originally from ${data.msgParamSenderName}` });
    }
    if (data.msgParamPromoName) {
        chips.push({
            label: `promo: ${data.msgParamPromoName}`,
            title: `Subscriptions promo: ${data.msgParamPromoName}`,
        });
    }
    if (data.msgParamPromoGiftTotal > 0) {
        chips.push({
            label: `${data.msgParamPromoGiftTotal} gifts`,
            title: `Gifts given during this promo: ${data.msgParamPromoGiftTotal}`,
        });
    }

    return <UserNoticeItem event={event} pill="Gift Upgrade" chips={chips} />;
}

export default ContinuedGiftedSubscriptionItem;
