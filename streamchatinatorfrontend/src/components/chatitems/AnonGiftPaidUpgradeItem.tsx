import type { FrontEndEventData, ChatEventAnonGiftPaidUpgrade } from "../../types";
import UserNoticeItem, { type InfoChip } from "./UserNoticeItem";

type AnonGiftPaidUpgradeItemProps = {
    event: FrontEndEventData<ChatEventAnonGiftPaidUpgrade>;
};

function AnonGiftPaidUpgradeItem({ event }: AnonGiftPaidUpgradeItemProps) {
    const data = event.chatEventData;

    const chips: InfoChip[] = [];
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

export default AnonGiftPaidUpgradeItem;
