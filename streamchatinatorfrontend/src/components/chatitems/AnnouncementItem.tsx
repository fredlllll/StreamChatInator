import type { FrontEndEventData, ChatEventAnnouncement } from "../../types";
import ChatBadges from "../ChatBadges";

type AnnouncementItemProps = {
    event: FrontEndEventData<ChatEventAnnouncement>;
};

function AnnouncementItem({ event }: AnnouncementItemProps) {
    const message = event.chatEventData;

    return (
        <div className="chat-message">
            <ChatBadges event={event} />
            <span className="badge">Announcement</span>
            <span className="username" style={{ color: message.hexColor || "#888" }}>
                {message.displayName}
            </span>
            <span className="colon">: </span>
            <span className="text">{JSON.stringify(message)}</span>
        </div>
    );
}

export default AnnouncementItem;