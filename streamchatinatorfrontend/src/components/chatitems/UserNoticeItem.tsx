import type { CSSProperties, ReactNode } from "react";
import type { ChatUserNoticeBase, FrontEndEventData } from "../../types";
import ChatBadges from "../ChatBadges";

/** A small indicator chip. Give the chip a `title` for a longer tooltip. */
export type InfoChip = string | { label: ReactNode; title?: string };

type UserNoticeItemProps<T extends ChatUserNoticeBase> = {
    event: FrontEndEventData<T>;
    /** Short label shown in a colored pill, e.g. "Resub". */
    pill?: string;
    /** CSS color for the pill's text/border. */
    pillColor?: string;
    /** Small indicator chips appended after the message text. */
    chips?: InfoChip[];
    /** Prepend the user's colored display name + colon. */
    showUsername?: boolean;
    /** Main text; defaults to the notice's system message. */
    text?: string;
    className?: string;
    /** Inline styles applied to the root element. */
    style?: CSSProperties;
    /** Optional extra content below the message (e.g. a resub message). */
    children?: ReactNode;
};

function UserNoticeItem<T extends ChatUserNoticeBase>({
    event,
    pill,
    pillColor,
    chips,
    showUsername = false,
    text,
    className = "",
    style,
    children,
}: UserNoticeItemProps<T>) {
    const data = event.chatEventData;

    let body = text ?? data.systemMsg;
    let prefix: ReactNode = null;
    if (showUsername && data.displayName) {
        prefix = (
            <>
                <span className="username" style={{ color: data.hexColor || "#888" }}>
                    {data.displayName}
                </span>
                <span className="colon">: </span>
            </>
        );
        if (!body) body = "";
    }

    return (
        <div className={`chat-message notice-item ${className}`} style={style}>
            {pill && (
                <span
                    className="event-pill"
                    style={pillColor ? { color: pillColor, borderColor: pillColor } : undefined}
                >
                    {pill}
                </span>
            )}
            <ChatBadges event={event} />
            {prefix}
            <span className="text">{body}</span>
            {chips && chips.length > 0 && (
                <span className="event-chips">
                    {chips.map((chip, i) => {
                        const label = typeof chip === "string" ? chip : chip.label;
                        const title = typeof chip === "string" ? undefined : chip.title;
                        return (
                            <span key={i} className="event-chip" title={title}>
                                {label}
                            </span>
                        );
                    })}
                </span>
            )}
            {children}
        </div>
    );
}

export default UserNoticeItem;
