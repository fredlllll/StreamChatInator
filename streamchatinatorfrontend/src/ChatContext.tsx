import { createContext, useCallback, useContext, useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import type { FrontEndEventData } from "./types";

export interface ChatContextType {
    events: FrontEndEventData[];
    twitchConnected: boolean;
    signalRConnectedAt: Date | null;
    channelId: string | null;
    tracking: boolean;
    seenState: Record<string, boolean>;
    // Bumped only when an existing event's seen value *flips* (a user toggle or
    // a corrected value from the server). Pure additions of brand-new events do
    // NOT bump it. Consumers use this to know when a re-filter (rather than an
    // incremental append) is required because a filter may read eventData.seen.
    seenVersion: number;
    // Bumped whenever the server purges all events, so views can drop their
    // cached history alongside the live list.
    purgeVersion: number;
    setEventSeen: (eventId: string, seen: boolean) => void;
    registerSeen: (eventId: string, seen: boolean) => void;
    undoSeen: () => void;
    canUndoSeen: boolean;
    setTracking: (enabled: boolean) => void;
}

const ChatContext = createContext<ChatContextType | undefined>(undefined);

export function ChatProvider({ children }: { children: React.ReactNode }) {
    const [events, setEvents] = useState<FrontEndEventData[]>([]);
    const [twitchConnected, setTwitchConnected] = useState<boolean>(false);
    const [signalRConnectedAt, setSignalRConnectedAt] = useState<Date | null>(null);
    const [channelId, setChannelId] = useState<string | null>(null);
    const [tracking, setTrackingState] = useState<boolean>(true);
    const [seenState, setSeenState] = useState<Record<string, boolean>>({});
    const [canUndoSeen, setCanUndoSeen] = useState(false);
    const [purgeVersion, setPurgeVersion] = useState(0);
    const [seenVersion, setSeenVersion] = useState(0);
    const connectionRef = useRef<signalR.HubConnection | null>(null);
    // Mirror of seenState for registering flips outside the state updater (side
    // effects in an updater would run twice under StrictMode). New keys (event
    // arrivals) are recorded but do not bump seenVersion.
    const seenPriorRef = useRef<Record<string, boolean>>({});
    // Stack of that user's own seen toggles, newest on top, so Ctrl+Z can
    // revert them one by one. Only setEventSeen records undo entries; seen
    // changes from broadcasts/history loads (registerSeen) do not.
    const undoStackRef = useRef<{ eventId: string; previousSeen: boolean }[]>([]);

    const registerSeen = useCallback((eventId: string, seen: boolean) => {
        setSeenState((prev) => (prev[eventId] === seen ? prev : { ...prev, [eventId]: seen }));
        const prior = seenPriorRef.current;
        const hadKey = Object.prototype.hasOwnProperty.call(prior, eventId);
        if (hadKey && prior[eventId] !== seen) {
            setSeenVersion((v) => v + 1);
        }
        seenPriorRef.current = { ...prior, [eventId]: seen };
    }, []);

    const applyEventSeen = useCallback(
        (eventId: string, seen: boolean, recordUndo: boolean) => {
            registerSeen(eventId, seen);
            if (recordUndo) {
                const previous = seenState[eventId];
                if (previous !== undefined) {
                    undoStackRef.current.push({ eventId, previousSeen: previous });
                    setCanUndoSeen(true);
                }
            }
            connectionRef.current?.invoke("SetEventSeen", eventId, seen).catch((err) => console.error("Failed to set seen:", err));
        },
        [registerSeen, seenState]
    );

    const setEventSeen = useCallback(
        (eventId: string, seen: boolean) => applyEventSeen(eventId, seen, true),
        [applyEventSeen]
    );

    const undoSeen = useCallback(() => {
        const entry = undoStackRef.current.pop();
        if (!entry) return;
        setCanUndoSeen(undoStackRef.current.length > 0);
        applyEventSeen(entry.eventId, entry.previousSeen, false);
    }, [applyEventSeen]);

    const setTracking = useCallback((enabled: boolean) => {
        connectionRef.current?.invoke("SetTracking", enabled).catch((err) => console.error("Failed to set tracking:", err));
    }, []);

    useEffect(() => {
        const onKeyDown = (e: KeyboardEvent) => {
            if (e.defaultPrevented || e.altKey) return;
            if (!(e.ctrlKey || e.metaKey)) return;
            const key = e.key.toLowerCase();
            if (key !== "z" || e.shiftKey) return;
            // Don't hijack native undo in text editors (Monaco is contentEditable).
            // Checkboxes/radios keep the undo active: the seen-toggle is an
            // <input type="checkbox"> and stays focused after clicking it.
            const target = e.target as HTMLElement | null;
            if (target?.isContentEditable || target?.tagName === "TEXTAREA" || target?.tagName === "SELECT") return;
            if (target?.tagName === "INPUT") {
                const type = (target as HTMLInputElement).type;
                if (type !== "checkbox" && type !== "radio") return;
            }
            e.preventDefault();
            undoSeen();
        };
        window.addEventListener("keydown", onKeyDown);
        return () => window.removeEventListener("keydown", onKeyDown);
    }, [undoSeen]);

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/chat")
            .withAutomaticReconnect()
            .build();
        connectionRef.current = connection;

        connection.on("ReceiveEvent", (envelope: FrontEndEventData) => {
            setEvents((prev) => [...prev, envelope]);
            registerSeen(envelope.eventId, envelope.seen);
        });

        connection.on("EventSeen", (eventId: string, seen: boolean) => {
            registerSeen(eventId, seen);
        });

        connection.on("Connection", () => {
            setTwitchConnected(true);
        });

        connection.on("NoConnection", () => {
            setTwitchConnected(false);
        });

        connection.on("ChannelId", (_channelId: string) => {
            setChannelId(_channelId);
        });

        connection.on("TrackingState", (enabled: boolean) => {
            setTrackingState(enabled);
        });

        connection.on("EventsPurged", () => {
            setEvents([]);
            setSeenState({});
            seenPriorRef.current = {};
            undoStackRef.current = [];
            setCanUndoSeen(false);
            setPurgeVersion((v) => v + 1);
            setSeenVersion((v) => v + 1);
        });

        // The history backfill anchor: set when the SignalR transport itself
        // comes up (initial start), not when the Twitch chat connects — those
        // are unrelated states.
        connection.start()
            .then(() => setSignalRConnectedAt(new Date()))
            .catch((err) => console.error("SignalR connection failed:", err));

        return () => {
            connectionRef.current = null;
            connection.stop();
        };
    }, [registerSeen]);

    return (
        <ChatContext.Provider value={{ events, twitchConnected, signalRConnectedAt, channelId, tracking, seenState, seenVersion, purgeVersion, setEventSeen, registerSeen, undoSeen, canUndoSeen, setTracking }}>
            {children}
        </ChatContext.Provider>
    );
}

export function useChatConnection(): ChatContextType {
    const context = useContext(ChatContext);
    if (!context) {
        throw new Error("useChatConnection must be used within a ChatProvider");
    }
    return context;
}