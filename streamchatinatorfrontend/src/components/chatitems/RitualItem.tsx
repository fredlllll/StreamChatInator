import type { FrontEndEventData, ChatEventRitual } from "../../types";
import UserNoticeItem from "./UserNoticeItem";

type RitualItemProps = {
    event: FrontEndEventData<ChatEventRitual>;
};

function RitualItem({ event }: RitualItemProps) {
    const data = event.chatEventData;

    const chips: string[] = [];
    if (data.msgParamRitualName) chips.push(data.msgParamRitualName);

    return (
        <UserNoticeItem event={event} pill="Ritual" chips={chips}>
            {data.message ? <div className="notice-subtext">{data.message}</div> : null}
        </UserNoticeItem>
    );
}

export default RitualItem;
