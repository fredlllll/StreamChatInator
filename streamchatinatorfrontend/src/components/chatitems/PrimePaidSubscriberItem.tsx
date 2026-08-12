import type { FrontEndEventData, ChatEventPrimePaidSubscriber } from "../../types";
import UserNoticeItem from "./UserNoticeItem";

type PrimePaidSubscriberItemProps = {
    event: FrontEndEventData<ChatEventPrimePaidSubscriber>;
};

function PrimePaidSubscriberItem({ event }: PrimePaidSubscriberItemProps) {
    const data = event.chatEventData;

    const chips: string[] = [];
    if (data.msgParamSubPlanName) chips.push(data.msgParamSubPlanName);

    return (
        <UserNoticeItem event={event} pill="Prime Sub" chips={chips}>
            {data.resubMessage ? <div className="notice-subtext">{data.resubMessage}</div> : null}
        </UserNoticeItem>
    );
}

export default PrimePaidSubscriberItem;
