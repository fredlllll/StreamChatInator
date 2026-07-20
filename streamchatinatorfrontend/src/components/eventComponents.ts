import type { ComponentType } from "react";
import ChatMessageItem from "./ChatMessageItem";
import type { EventEnvelope } from "../types";

const eventComponents: Record<string, ComponentType<{ event: EventEnvelope<any> }>> = {
    ChatMessage: ChatMessageItem,
    // Ban: BanEventItem,
    // Timeout: TimeoutEventItem,
};

export default eventComponents;