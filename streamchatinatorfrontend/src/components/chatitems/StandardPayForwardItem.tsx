import type { FrontEndEventData, ChatEventStandardPayForward } from "../../types";
import UserNoticeItem from "./UserNoticeItem";

type StandardPayForwardItemProps = {
    event: FrontEndEventData<ChatEventStandardPayForward>;
};

function StandardPayForwardItem({ event }: StandardPayForwardItemProps) {
    const data = event.chatEventData;

    const chips: string[] = [];
    if (!data.msgParamPriorGifterAnonymous && data.msgParamPriorGifterDisplayName) {
        chips.push(`from ${data.msgParamPriorGifterDisplayName}`);
    }
    if (data.msgParamRecipientDisplayName) {
        chips.push(`to ${data.msgParamRecipientDisplayName}`);
    }

    return <UserNoticeItem event={event} pill="Pay Forward" chips={chips} />;
}

export default StandardPayForwardItem;
