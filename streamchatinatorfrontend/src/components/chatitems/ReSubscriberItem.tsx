import type { FrontEndEventData, ChatEventReSubscriber } from "../../types";
import UserNoticeItem from "./UserNoticeItem";

type ReSubscriberItemProps = {
    event: FrontEndEventData<ChatEventReSubscriber>;
};

function ReSubscriberItem({ event }: ReSubscriberItemProps) {
    const data = event.chatEventData;

    const chips: string[] = [];
    if (data.msgParamSubPlanName) chips.push(data.msgParamSubPlanName);
    if (data.msgParamCumulativeMonths > 0) chips.push(`${data.msgParamCumulativeMonths} mo`);
    if (data.msgParamShouldShareStreak && data.msgParamStreakMonths > 0) {
        chips.push(`${data.msgParamStreakMonths} streak`);
    }

    return (
        <UserNoticeItem event={event} pill="Resub" chips={chips}>
            {data.resubMessage ? <div className="notice-subtext">{data.resubMessage}</div> : null}
        </UserNoticeItem>
    );
}

export default ReSubscriberItem;
