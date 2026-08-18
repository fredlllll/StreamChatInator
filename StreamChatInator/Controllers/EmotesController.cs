using Microsoft.AspNetCore.Mvc;
using StreamChatInator.Services.Emotes;

namespace StreamChatInator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmotesController : ControllerBase
    {
        private readonly EmoteProviderService _emotes;

        public EmotesController(EmoteProviderService emotes)
        {
            _emotes = emotes;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<EmoteDto>>> Get([FromQuery] string? channelId)
        {
            return Ok(await _emotes.GetEmotesAsync(channelId));
        }
    }
}