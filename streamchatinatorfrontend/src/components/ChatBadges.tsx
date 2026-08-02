import type { FrontEndEventData, UserFlagName, UserTypeName } from "../types";

type AnnouncementItemProps = {
    event: FrontEndEventData<any>;
};

function ChatBadges({ event }: AnnouncementItemProps) {
    const data = event.chatEventData;
    const userFlagNames: UserFlagName[] | undefined = data.userFlagNames;
    const userTypeName: UserTypeName | undefined = data.userTypeName;

    let isBroadcaster = false;
    let isModerator = false;
    let isSubscriber = false;
    let isVip = false;
    let isPartner = false;
    let isTurbo = false;

    if (userTypeName) {
        isBroadcaster = userTypeName.includes("Broadcaster");
        isModerator = userTypeName.includes("Moderator");
    }
    else if (userFlagNames) {
        isSubscriber = userFlagNames.includes("Subscriber") || data.isSubscriber;
        isVip = userFlagNames.includes("Vip");
        isPartner = userFlagNames.includes("Partner");
        isTurbo = userFlagNames.includes("Turbo");
    }

    return (
        <div>
            {isBroadcaster && <span className="badge"> STREAMER </span>}
            {isModerator && <span className="badge"> Mod </span>}
            {isSubscriber && <span className="badge"> Sub </span>}
            {isVip && <span className="badge"> Vip </span>}
            {isPartner && <span className="badge"> Partner </span>}
            {isTurbo && <span className="badge"> Turbo </span>}
        </div>
    );
}

export default ChatBadges;