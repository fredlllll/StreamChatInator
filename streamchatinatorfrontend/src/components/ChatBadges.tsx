import type { FrontEndEventData, UserFlagName, UserTypeName } from "../types";

type ChatBadgesItemProps = {
    event: FrontEndEventData<any>;
};

function ChatBadges({ event }: ChatBadgesItemProps) {
    const data = event.chatEventData;
    const userFlagsNames: UserFlagName[] | undefined = data.userFlagsNames;
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
    if (userFlagsNames) {
        isModerator ||= userFlagsNames.includes("Moderator");
        isSubscriber = userFlagsNames.includes("Subscriber") || data.isSubscriber;
        isVip = userFlagsNames.includes("Vip");
        isPartner = userFlagsNames.includes("Partner");
        isTurbo = userFlagsNames.includes("Turbo");
    }

    return (
        <span>
            {isBroadcaster && <span className="badge"> STREAMER </span>}
            {isModerator && <span className="badge"> Mod </span>}
            {isSubscriber && <span className="badge"> Sub </span>}
            {isVip && <span className="badge"> Vip </span>}
            {isPartner && <span className="badge"> Partner </span>}
            {isTurbo && <span className="badge"> Turbo </span>}
        </span>
    );
}

export default ChatBadges;