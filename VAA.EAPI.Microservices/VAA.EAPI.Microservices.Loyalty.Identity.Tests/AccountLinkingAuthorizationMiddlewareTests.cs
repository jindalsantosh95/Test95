using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests;

public sealed class AccountLinkingAuthorizationMiddlewareTests
{
    [Theory]
    [InlineData(null, false, 401)]
    [InlineData("wrong-role", true, 403)]
    [InlineData("Loyalty.Identity.AccountLinkingProof.Create", true, 0)]
    public async Task NewProofBoundaryRequiresAuthenticatedApplicationRole(string? role, bool authenticated, int status)
    {
        var context = Context(typeof(AccountLinkingController).FullName + ".CreateAsync");
        if (role is not null)
            context.Features.Set(new JwtPrincipalFeature(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], authenticated ? "test" : null)), null!));
        using var serviceProvider = new ServiceCollection().AddSingleton<AccountLinkingAuthorizationMiddleware>().BuildServiceProvider();
        var called = false;
        await serviceProvider.GetRequiredService<AccountLinkingAuthorizationMiddleware>().Invoke(context, _ => { called = true; return Task.CompletedTask; });
        Assert.Equal(status == 0, called);
        if (status != 0)
            Assert.Equal((HttpStatusCode)status, Assert.IsAssignableFrom<HttpResponseData>(context.Features.Get<IFunctionBindingsFeature>()!.InvocationResult).StatusCode);
    }

    [Theory]
    [InlineData("ValidateAsync")]
    [InlineData("IssueAsync")]
    public async Task ExistingTokenEndpointsRemainOutsideNewAuthorization(string method)
    {
        using var serviceProvider = new ServiceCollection().AddSingleton<AccountLinkingAuthorizationMiddleware>().BuildServiceProvider();
        var called = false;
        await serviceProvider.GetRequiredService<AccountLinkingAuthorizationMiddleware>().Invoke(
            Context(typeof(FlyingClubTokenController).FullName + "." + method), _ => { called = true; return Task.CompletedTask; });
        Assert.True(called);
    }

    private static TestFunctionContext Context(string entryPoint)
    {
        var context = new TestFunctionContext();
        // The shared fixture currently ignores its functionDefinition constructor argument.
        typeof(TestFunctionContext).GetField("functionDefinition", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(context, new TestFunctionDefinition("test", "test", entryPoint));
        return context;
    }
}
