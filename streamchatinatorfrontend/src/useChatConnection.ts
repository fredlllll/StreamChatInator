import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import type { FrontEndEventData } from "./types";

export function useChatConnection() {
    const [events, setEvents] = useState<FrontEndEventData[]>([]);
    const [connected, setConnected] = useState<boolean>(true);
    const [connectedAt, setConnectedAt] = useState<Date | null>(null);

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

        connection
            .start()
            .then(() => setConnectedAt(new Date()))
            .catch((err) => console.error("SignalR connection failed:", err));

        return () => {
            connection.stop();
        };
    }, []); // empty array = connect once, when this hook's component mounts

    return { events, connected, connectedAt };
}