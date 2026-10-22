using Microsoft.AspNetCore.Authorization;
using static VAA.EAPI.HealthStatusApp.Services.GraphService;
using VAA.EAPI.HealthStatusApp.Services;

namespace VAA.EAPI.HealthStatusApp.Helpers
{
    public class GroupAuthorizationHandler : AuthorizationHandler<GroupRequirement>
    {
        private readonly IGraphService _graphService;

        public GroupAuthorizationHandler(IGraphService graphService)
        {
            _graphService = graphService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupRequirement requirement)
        {
            var userId = context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
                return;

            var isInGroup = await _graphService.IsUserInGroupAsync(userId, requirement.GroupIdOrName);

            if (isInGroup)
                context.Succeed(requirement);
        }
    }
}
