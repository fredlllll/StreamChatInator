import type { FrontEndEventData, UserFlagName , UserTypeName} from "../types";

type AnnouncementItemProps = {
    event: FrontEndEventData<any>;
};

function ChatBadges({ event }: AnnouncementItemProps) {
    const data = event.chatEventData;
    const userFlagNames: UserFlagName[] | undefined = data.userFlagNames;
    const userTypeName: UserTypeName | undefined = data.userTypeName;

    if (userFlagNames && userTypeName) {
        const isBroadcaster = userTypeName.includes("Broadcaster");
        const isModerator = userTypeName.includes("Moderator");
        const isSubscriber = userFlagNames.includes("Subscriber");
        const isVip = userFlagNames.includes("Vip");
        const isPartner = userFlagNames.includes("Partner");
        const isTurbo = userFlagNames.includes("Turbo");

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
}

export default ChatBadges;