import type { FrontEndEventData, ChatEventNewSubscriber } from "../../types";
import UserNoticeItem, { type InfoChip } from "./UserNoticeItem";

type NewSubscriberItemProps = {
    event: FrontEndEventData<ChatEventNewSubscriber>;
};

function NewSubscriberItem({ event }: NewSubscriberItemProps) {
    const data = event.chatEventData;

    const chips: InfoChip[] = [];
    if (data.msgParamSubPlanName) {
        chips.push({ label: data.msgParamSubPlanName, title: `Subscription plan: ${data.msgParamSubPlanName}` });
    }

    return (
        <UserNoticeItem event={event} pill="New Sub" chips={chips}>
            {data.resubMessage ? <div className="notice-subtext">{data.resubMessage}</div> : null}
        </UserNoticeItem>
    );
}

export default NewSubscriberItem;
