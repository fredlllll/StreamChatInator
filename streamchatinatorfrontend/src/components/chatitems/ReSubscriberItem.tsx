import type { FrontEndEventData, ChatEventReSubscriber } from "../../types";
import UserNoticeItem, { type InfoChip } from "./UserNoticeItem";

type ReSubscriberItemProps = {
    event: FrontEndEventData<ChatEventReSubscriber>;
};

function ReSubscriberItem({ event }: ReSubscriberItemProps) {
    const data = event.chatEventData;

    const chips: InfoChip[] = [];
    if (data.msgParamSubPlanName) {
        chips.push({ label: data.msgParamSubPlanName, title: `Subscription plan: ${data.msgParamSubPlanName}` });
    }
    if (data.msgParamShouldShareStreak && data.msgParamStreakMonths > 0) {
        chips.push({
            label: `${data.msgParamStreakMonths} streak`,
            title: `Consecutive month streak: ${data.msgParamStreakMonths}`,
        });
    }

    return (
        <UserNoticeItem event={event} pill="Resub" chips={chips}>
            {data.resubMessage ? <div className="notice-subtext">{data.resubMessage}</div> : null}
        </UserNoticeItem>
    );
}

export default ReSubscriberItem;
