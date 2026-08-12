import type { FrontEndEventData, ChatEventAnonGiftPaidUpgrade } from "../../types";
import UserNoticeItem from "./UserNoticeItem";

type AnonGiftPaidUpgradeItemProps = {
    event: FrontEndEventData<ChatEventAnonGiftPaidUpgrade>;
};

function AnonGiftPaidUpgradeItem({ event }: AnonGiftPaidUpgradeItemProps) {
    const data = event.chatEventData;

    const chips: string[] = [];
    if (data.msgParamPromoName) chips.push(`promo: ${data.msgParamPromoName}`);
    if (data.msgParamPromoGiftTotal > 0) chips.push(`${data.msgParamPromoGiftTotal} gifts`);

    return <UserNoticeItem event={event} pill="Gift Upgrade" chips={chips} />;
}

export default AnonGiftPaidUpgradeItem;
