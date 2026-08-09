import type { FrontEndEventData, ChatEventAnnouncement } from "../../types";
import ChatBadges from "../ChatBadges";
import EmoteReplacedMessage from "../EmoteReplacedMessage";

type AnnouncementItemProps = {
    event: FrontEndEventData<ChatEventAnnouncement>;
};

function AnnouncementItem({ event }: AnnouncementItemProps) {
    const message = event.chatEventData;
    return (
        <div className="chat-message announcement" style={{ "borderColor": message.msgParamColor }}>
            <span className="badge">Announcement</span>
            <ChatBadges event={event} />
            <span className="username" style={{ color: message.hexColor || "#888" }}>
                {message.displayName}
            </span>
            <span className="colon">: </span>
            <EmoteReplacedMessage emotes={message.emotes} text={message.message} />
        </div>
    );
}

export default AnnouncementItem;