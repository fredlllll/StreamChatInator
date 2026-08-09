import { createContext, useCallback, useContext, useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import type { FrontEndEventData } from "./types";

export interface ChatContextType {
    events: FrontEndEventData[];
    connected: boolean;
    connectedAt: Date | null;
    channelId: string | null;
    seenState: Record<string, boolean>;
    setEventSeen: (eventId: string, seen: boolean) => void;
    registerSeen: (eventId: string, seen: boolean) => void;
}

const ChatContext = createContext<ChatContextType | undefined>(undefined);

export function ChatProvider({ children }: { children: React.ReactNode }) {
    const [events, setEvents] = useState<FrontEndEventData[]>([]);
    const [connected, setConnected] = useState<boolean>(false);
    const [connectedAt, setConnectedAt] = useState<Date | null>(null);
    const [channelId, setChannelId] = useState<string | null>(null);
    const [seenState, setSeenState] = useState<Record<string, boolean>>({});
    const connectionRef = useRef<signalR.HubConnection | null>(null);

    const registerSeen = useCallback((eventId: string, seen: boolean) => {
        setSeenState((prev) => (prev[eventId] === seen ? prev : { ...prev, [eventId]: seen }));
    }, []);

    const setEventSeen = useCallback(
        (eventId: string, seen: boolean) => {
            registerSeen(eventId, seen);
            connectionRef.current?.invoke("SetEventSeen", eventId, seen).catch((err) => console.error("Failed to set seen:", err));
        },
        [registerSeen]
    );

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
            setConnected(true);
        });

        connection.on("NoConnection", () => {
            setConnected(false);
        });

        connection.on("ChannelId", (_channelId: string) => {
            setChannelId(_channelId);
        });

        connection
            .start()
            .then(() => {
                setConnected(true);
                setConnectedAt(new Date());
            })
            .catch((err) => console.error("SignalR connection failed:", err));

        return () => {
            connectionRef.current = null;
            connection.stop();
        };
    }, [registerSeen]);

    return (
        <ChatContext.Provider value={{ events, connected, connectedAt, channelId, seenState, setEventSeen, registerSeen }}>
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