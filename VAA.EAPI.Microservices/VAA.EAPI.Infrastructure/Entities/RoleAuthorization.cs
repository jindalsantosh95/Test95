using System;

namespace VAA.EAPI.Infrastructure.Entities
{
    /// <summary>
    /// Attribute to be used to add Role Authorization list to Endpoints
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RoleAuthorizationAttribute : Attribute
    {
        public string[] Roles { get; }
        public RoleAuthorizationAttribute(params string[] roles)
        {
            Roles = roles;
        }
    }
}
