using Microsoft.AspNetCore.Mvc;
using StreamChatInator.Services.Twitch;

namespace StreamChatInator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BadgesController : ControllerBase
    {
        private readonly BadgeProviderService _badges;

        public BadgesController(BadgeProviderService badges)
        {
            _badges = badges;
        }

        [HttpGet]
        public async Task<ActionResult<Dictionary<string, Dictionary<string, BadgeDto>>>> Get([FromQuery] string? channelId)
        {
            return Ok(await _badges.GetBadgesAsync(channelId));
        }
    }
}