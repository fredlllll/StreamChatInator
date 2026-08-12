import type { FrontEndEventData, ChatEventPrimePaidSubscriber } from "../../types";
import UserNoticeItem, { type InfoChip } from "./UserNoticeItem";

type PrimePaidSubscriberItemProps = {
    event: FrontEndEventData<ChatEventPrimePaidSubscriber>;
};

function PrimePaidSubscriberItem({ event }: PrimePaidSubscriberItemProps) {
    const data = event.chatEventData;

    const chips: InfoChip[] = [];
    if (data.msgParamSubPlanName) {
        chips.push({ label: data.msgParamSubPlanName, title: `Subscription plan: ${data.msgParamSubPlanName}` });
    }

    return (
        <UserNoticeItem event={event} pill="Prime Sub" chips={chips}>
            {data.resubMessage ? <div className="notice-subtext">{data.resubMessage}</div> : null}
        </UserNoticeItem>
    );
}

export default PrimePaidSubscriberItem;
