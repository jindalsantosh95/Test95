using Microsoft.AspNetCore.Authorization;

namespace VAA.EAPI.HealthStatusApp.Helpers
{
    public class GroupRequirement : IAuthorizationRequirement
    {
        public string GroupIdOrName { get; }

        public GroupRequirement(string groupIdOrName)
        {
            GroupIdOrName = groupIdOrName;
        }
    }
}
