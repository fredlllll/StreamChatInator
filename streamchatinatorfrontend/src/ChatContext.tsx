import { createContext, useContext, useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import type { FrontEndEventData } from "./types";

export interface ChatContextType {
    events: FrontEndEventData[];
    connected: boolean;
    connectedAt: Date | null;
    channelId: string | null;
}

const ChatContext = createContext<ChatContextType | undefined>(undefined);

export function ChatProvider({ children }: { children: React.ReactNode }) {
    const [events, setEvents] = useState<FrontEndEventData[]>([]);
    const [connected, setConnected] = useState<boolean>(false);
    const [connectedAt, setConnectedAt] = useState<Date | null>(null);
    const [channelId, setChannelId] = useState<string | null>(null);

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/chat")
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveEvent", (envelope: FrontEndEventData) => {
            setEvents((prev) => [...prev, envelope]);
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
            connection.stop();
        };
    }, []);

    return (
        <ChatContext.Provider value={{ events, connected, connectedAt, channelId }}>
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