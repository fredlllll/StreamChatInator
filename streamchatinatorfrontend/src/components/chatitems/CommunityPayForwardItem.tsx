import type { FrontEndEventData, ChatEventCommunityPayForward } from "../../types";
import UserNoticeItem from "./UserNoticeItem";

type CommunityPayForwardItemProps = {
    event: FrontEndEventData<ChatEventCommunityPayForward>;
};

function CommunityPayForwardItem({ event }: CommunityPayForwardItemProps) {
    const data = event.chatEventData;

    const chips: string[] = [];
    if (!data.msgParamPriorGifterAnonymous && data.msgParamPriorGifterDisplayName) {
        chips.push(`from ${data.msgParamPriorGifterDisplayName}`);
    }

    return <UserNoticeItem event={event} pill="Pay Forward" chips={chips} />;
}

export default CommunityPayForwardItem;
