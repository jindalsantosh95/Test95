using System.Security.Claims;
using VAA.EAPI.GraphQL.Rest.Middleware.Mcp;
using Xunit;

namespace VAA.EAPI.GraphQL.Rest.Tests;

public sealed class McpToolAuthorizationTests
{
    [Theory]
    [InlineData("required", "required", true)]
    [InlineData("required", "other", false)]
    [InlineData("other", "required", false)]
    [InlineData("required", null, false)]
    [InlineData(null, "required", false)]
    public void OptedInToolRequiresBothPrincipalsToHaveTheOperationRole(string? bearerRole, string? contextRole, bool allowed)
    {
        var tool = new Tool { RequiresMcpCallerContext = true, RequiresAuthentication = true, AuthorizedRoles = ["required"] };
        Assert.Equal(allowed, tool.HasRequiredCallerContext(Principal(bearerRole), Principal(contextRole)));
    }

    [Fact]
    public void ExistingToolDoesNotAcquireCallerContextRequirement()
    {
        var tool = new Tool { RequiresAuthentication = true, AuthorizedRoles = ["required"] };
        Assert.True(tool.HasRequiredCallerContext(null!, null!));
    }

    private static ClaimsPrincipal Principal(string? role) => role is null
        ? new ClaimsPrincipal(new ClaimsIdentity())
        : new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "test"));
}
