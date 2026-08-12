import type { FrontEndEventData, ChatEventCommunityPayForward } from "../../types";
import UserNoticeItem, { type InfoChip } from "./UserNoticeItem";

type CommunityPayForwardItemProps = {
    event: FrontEndEventData<ChatEventCommunityPayForward>;
};

function CommunityPayForwardItem({ event }: CommunityPayForwardItemProps) {
    const data = event.chatEventData;

    const chips: InfoChip[] = [];
    if (!data.msgParamPriorGifterAnonymous && data.msgParamPriorGifterDisplayName) {
        chips.push({
            label: `from ${data.msgParamPriorGifterDisplayName}`,
            title: `Gift originally from ${data.msgParamPriorGifterDisplayName}`,
        });
    }

    return <UserNoticeItem event={event} pill="Pay Forward" chips={chips} />;
}

export default CommunityPayForwardItem;
