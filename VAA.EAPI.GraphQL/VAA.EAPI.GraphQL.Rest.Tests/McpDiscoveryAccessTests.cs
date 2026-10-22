using System.Security.Claims;
using VAA.EAPI.GraphQL.Rest.Middleware.Mcp;
using Xunit;

namespace VAA.EAPI.GraphQL.Rest.Tests;

public sealed class McpDiscoveryAccessTests
{
    [Theory]
    [InlineData(false, null, true, "required", false)]
    [InlineData(true, null, true, "required", false)]
    [InlineData(true, "other", true, "required", false)]
    [InlineData(true, "required", true, "required", true)]
    [InlineData(false, null, true, null, false)]
    [InlineData(true, null, true, null, true)]
    [InlineData(false, null, false, null, true)]
    public void DiscoveryAccessHonorsAuthenticationAndRoles(bool authenticated, string? role, bool requiresAuthentication, string? requiredRole, bool expected)
    {
        var tool = new Tool
        {
            RequiresAuthentication = requiresAuthentication,
            AuthorizedRoles = requiredRole is null ? [] : [requiredRole]
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            role is null ? [] : [new Claim(ClaimTypes.Role, role)], authenticated ? "test" : null));
        Assert.Equal(expected, tool.CanAccess(principal));
    }
}
