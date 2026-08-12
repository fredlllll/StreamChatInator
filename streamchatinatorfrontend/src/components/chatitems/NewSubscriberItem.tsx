import type { FrontEndEventData, ChatEventNewSubscriber } from "../../types";
import UserNoticeItem from "./UserNoticeItem";

type NewSubscriberItemProps = {
    event: FrontEndEventData<ChatEventNewSubscriber>;
};

function NewSubscriberItem({ event }: NewSubscriberItemProps) {
    const data = event.chatEventData;

    const chips: string[] = [];
    if (data.msgParamSubPlanName) chips.push(data.msgParamSubPlanName);
    if (data.msgParamCumulativeMonths > 0) chips.push(`${data.msgParamCumulativeMonths} mo`);

    return (
        <UserNoticeItem event={event} pill="New Sub" chips={chips}>
            {data.resubMessage ? <div className="notice-subtext">{data.resubMessage}</div> : null}
        </UserNoticeItem>
    );
}

export default NewSubscriberItem;
