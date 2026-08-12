import type { FrontEndEventData, ChatEventRitual } from "../../types";
import UserNoticeItem, { type InfoChip } from "./UserNoticeItem";

type RitualItemProps = {
    event: FrontEndEventData<ChatEventRitual>;
};

function RitualItem({ event }: RitualItemProps) {
    const data = event.chatEventData;

    const chips: InfoChip[] = [];
    if (data.msgParamRitualName) {
        chips.push({ label: data.msgParamRitualName, title: `Ritual: ${data.msgParamRitualName}` });
    }

    return (
        <UserNoticeItem event={event} pill="Ritual" chips={chips}>
            {data.message ? <div className="notice-subtext">{data.message}</div> : null}
        </UserNoticeItem>
    );
}

export default RitualItem;
