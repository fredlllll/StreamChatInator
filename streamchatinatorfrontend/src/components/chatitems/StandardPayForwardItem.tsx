import type { FrontEndEventData, ChatEventStandardPayForward } from "../../types";
import UserNoticeItem, { type InfoChip } from "./UserNoticeItem";

type StandardPayForwardItemProps = {
    event: FrontEndEventData<ChatEventStandardPayForward>;
};

function StandardPayForwardItem({ event }: StandardPayForwardItemProps) {
    const data = event.chatEventData;

    const chips: InfoChip[] = [];
    if (!data.msgParamPriorGifterAnonymous && data.msgParamPriorGifterDisplayName) {
        chips.push({
            label: `from ${data.msgParamPriorGifterDisplayName}`,
            title: `Gift originally from ${data.msgParamPriorGifterDisplayName}`,
        });
    }
    if (data.msgParamRecipientDisplayName) {
        chips.push({
            label: `to ${data.msgParamRecipientDisplayName}`,
            title: `Paid forward to ${data.msgParamRecipientDisplayName}`,
        });
    }

    return <UserNoticeItem event={event} pill="Pay Forward" chips={chips} />;
}

export default StandardPayForwardItem;
