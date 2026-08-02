import { useChatConnection } from "../useChatConnection";

function ConnectionIndicator() {
    const { events, connected, connectedAt } = useChatConnection();
    return (
        <span>{!connected && "🔴"}{connected && "🟢"}</span>
  );
}

export default ConnectionIndicator;