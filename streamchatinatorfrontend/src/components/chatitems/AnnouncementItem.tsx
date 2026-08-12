import type { FrontEndEventData, ChatEventAnnouncement } from "../../types";
import UserNoticeItem from "./UserNoticeItem";

const ANNOUNCEMENT_COLORS: Record<string, string> = {
    PRIMARY: "#7c3aed",
    BLUE: "#1f6feb",
    GREEN: "#1a7f37",
    ORANGE: "#d97706",
    PURPLE: "#8250df",
};

type AnnouncementItemProps = {
    event: FrontEndEventData<ChatEventAnnouncement>;
};

function AnnouncementItem({ event }: AnnouncementItemProps) {
    const data = event.chatEventData;

    const pillColor = ANNOUNCEMENT_COLORS[data.msgParamColor] ?? data.msgParamColor;

    return (
        <UserNoticeItem
            event={event}
            className="announcement"
            pill="Announcement"
            pillColor={pillColor}
            showUsername
            text={data.message}
            style={{ borderLeftColor: pillColor }}
        />
    );
}

export default AnnouncementItem;
