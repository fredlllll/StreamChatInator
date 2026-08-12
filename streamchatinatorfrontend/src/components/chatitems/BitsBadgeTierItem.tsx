import type { FrontEndEventData, ChatEventBitsBadgeTier } from "../../types";
import UserNoticeItem from "./UserNoticeItem";

type BitsBadgeTierItemProps = {
    event: FrontEndEventData<ChatEventBitsBadgeTier>;
};

function BitsBadgeTierItem({ event }: BitsBadgeTierItemProps) {
    const data = event.chatEventData;

    const chips = [
        {
            label: `${data.msgParamThreshold} bits`,
            title: `Earned the ${data.msgParamThreshold}-bit badge tier`,
        },
    ];

    return <UserNoticeItem event={event} pill="Bits Badge" chips={chips} />;
}

export default BitsBadgeTierItem;
