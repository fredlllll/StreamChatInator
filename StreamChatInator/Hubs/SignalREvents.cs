namespace StreamChatInator.Hubs
{
    /// <summary>
    /// Event names broadcast to SignalR clients. These are part of the
    /// frontend contract: the values must match the frontend's
    /// <c>connection.on(...)</c> handlers in ChatContext.tsx.
    /// </summary>
    public static class SignalREvents
    {
        public const string ChannelId = nameof(ChannelId);
        public const string Connection = nameof(Connection);
        public const string NoConnection = nameof(NoConnection);
        public const string TrackingState = nameof(TrackingState);
        public const string EventSeen = nameof(EventSeen);
        public const string ReceiveEvent = nameof(ReceiveEvent);
        public const string EventsPurged = nameof(EventsPurged);
    }
}
