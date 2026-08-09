import { useChatConnection, type ChatContextType  } from "../ChatContext";

function ConnectionIndicator() {
    const ctx: ChatContextType = useChatConnection();
    return (
        <span>{!ctx.connected && "🔴"}{ctx.connected && "🟢"}</span>
  );
}

export default ConnectionIndicator;