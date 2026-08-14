using Microsoft.AspNetCore.Authorization;
using StreamChatInator.Services;

namespace StreamChatInator.Auth
{
    /// <summary>
    /// Passes when access control is disabled, or when the caller is authenticated.
    /// This is what makes PIN gating opt-out: with Auth:Enabled=false every
    /// request succeeds without a cookie.
    /// </summary>
    public class AccessControlRequirement : IAuthorizationRequirement { }

    public class AccessControlHandler : AuthorizationHandler<AccessControlRequirement>
    {
        private readonly AccessControlService _accessControl;

        public AccessControlHandler(AccessControlService accessControl)
        {
            _accessControl = accessControl;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessControlRequirement requirement)
        {
            if (!_accessControl.Enabled || context.User.Identity?.IsAuthenticated == true)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
