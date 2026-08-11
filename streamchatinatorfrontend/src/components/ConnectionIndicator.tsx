import { useChatConnection, type ChatContextType  } from "../ChatContext";

function ConnectionIndicator() {
    const ctx: ChatContextType = useChatConnection();
    return (
        <span>{!ctx.twitchConnected && "🔴"}{ctx.twitchConnected && "🟢"}</span>
  );
}

export default ConnectionIndicator;