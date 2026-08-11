using Microsoft.AspNetCore.Authorization;
using StreamChatInator.Services;

namespace StreamChatInator.Auth
{
    /// <summary>
    /// Passes when LAN access is disabled, or when the caller is authenticated.
    /// This is what makes PIN gating opt-out: with Auth:Enabled=false every
    /// request succeeds without a cookie.
    /// </summary>
    public class LanAccessRequirement : IAuthorizationRequirement { }

    public class LanAccessHandler : AuthorizationHandler<LanAccessRequirement>
    {
        private readonly LanAccessService _lanAccess;

        public LanAccessHandler(LanAccessService lanAccess)
        {
            _lanAccess = lanAccess;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, LanAccessRequirement requirement)
        {
            if (!_lanAccess.Enabled || context.User.Identity?.IsAuthenticated == true)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
