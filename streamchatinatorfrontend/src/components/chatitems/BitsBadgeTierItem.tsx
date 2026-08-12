import type { FrontEndEventData, ChatEventBitsBadgeTier } from "../../types";
import UserNoticeItem from "./UserNoticeItem";

type BitsBadgeTierItemProps = {
    event: FrontEndEventData<ChatEventBitsBadgeTier>;
};

function BitsBadgeTierItem({ event }: BitsBadgeTierItemProps) {
    const data = event.chatEventData;

    return <UserNoticeItem event={event} pill="Bits Badge" chips={[`${data.msgParamThreshold} bits`]} />;
}

export default BitsBadgeTierItem;
