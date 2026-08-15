namespace StreamChatInator.Services
{
    /// <summary>
    /// One external emote provider (BTTV/7TV/FFZ). Channel id is null for the
    /// global emote set; a provider that is unreachable yields an empty list
    /// rather than throwing so chat keeps working.
    /// </summary>
    public interface IEmoteFetcher
    {
        Task<List<EmoteDto>> FetchAsync(string? channelId);
    }
}