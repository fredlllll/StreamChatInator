using StreamChatInator.Database.Models;
using System.Text.Json;

namespace StreamChatInator
{
    /// <summary>Shapes a chat event (index row + detail rows) into the payload sent to the frontend.</summary>
    public static class FrontEndEventMapper
    {
        public static FrontEndEventData ToFrontendData(ChatEvent chatEvent, Model eventData)
        {
            return new FrontEndEventData
            {
                EventId = chatEvent.Id,
                ChatEventType = chatEvent.ChatEventType,
                Seen = chatEvent.Seen,
                ChatEventData = eventData
            };
        }

        public static FrontEndEventData ToFrontendData(ChatEvent chatEvent, Model eventData, Model eventSubData)
        {
            return new FrontEndEventData
            {
                EventId = chatEvent.Id,
                ChatEventType = chatEvent.ChatEventType,
                Seen = chatEvent.Seen,
                ChatEventData = ObjectMerger.MergeObjects(JsonNamingPolicy.CamelCase, eventSubData, eventData)
            };
        }
    }
}