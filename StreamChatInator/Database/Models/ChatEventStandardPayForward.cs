using TwitchLib.Client.Models;

namespace StreamChatInator.Database.Models
{
    public class ChatEventStandardPayForward : ModelWithUserNoticeBase
    {
        public required bool MsgParamPriorGifterAnonymous { get; set; }

        public required string MsgParamPriorGifterDisplayName { get; set; }

        public required string MsgParamPriorGifterId { get; set; }

        public required string MsgParamPriorGifterUserName { get; set; }

        public required string? MsgParamRecipientDisplayName { get; set; }

        public required string? MsgParamRecipientId { get; set; }

        public required string? MsgParamRecipientUserName { get; set; }

        public static ChatEventStandardPayForward FromStandardPayForward(StandardPayForward standardPayForward, string chatUserNoticeBaseId)
        {
            return new ChatEventStandardPayForward()
            {
                Id = GetNewId<ChatEventStandardPayForward>(),
                MsgParamPriorGifterAnonymous = standardPayForward.MsgParamPriorGifterAnonymous,
                MsgParamPriorGifterDisplayName = standardPayForward.MsgParamPriorGifterDisplayName,
                MsgParamPriorGifterId = standardPayForward.MsgParamPriorGifterId.ToString(),
                MsgParamPriorGifterUserName = standardPayForward.MsgParamPriorGifterUserName,
                MsgParamRecipientDisplayName = standardPayForward.MsgParamRecipientDisplayName,
                MsgParamRecipientId = standardPayForward.MsgParamRecipientId?.ToString(),
                MsgParamRecipientUserName = standardPayForward.MsgParamRecipientUserName,
                ChatUserNoticeBaseId = chatUserNoticeBaseId,
            };
        }
    }
}
