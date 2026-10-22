using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mutations.Secure;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Rest.FunctionHost;
using VAA.EAPI.GraphQL.Rest.Middleware;
using Xunit;

namespace VAA.EAPI.GraphQL.Rest.Tests;

public sealed class CallerContextRouteTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("wrong-role", false)]
    [InlineData(VAA.EAPI.GraphQL.Domain.Account.Constants.Application.AccountLinkingProofRedeemRole, true)]
    public async Task MarkedRouteRequiresCallerContextBeforeReadingArguments(string? contextRole, bool allowed)
    {
        const string role = VAA.EAPI.GraphQL.Domain.Account.Constants.Application.AccountLinkingProofRedeemRole;
        var services = new ServiceCollection();
        using var provider = services.AddLogging().AddSingleton<IServiceCollection>(services)
            .AddSingleton<IHttpClientFactory, VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>()
            .AddSingleton<IApplicationOAuthTokenProvider, OAuthTokenProvider_AAD>()
            .AddSingleton<IAccountLinkingProvider, AccountLinkingProvider>()
            .AddSingleton<AccountLinkingMutation>().AddSingleton<Router>().BuildServiceProvider();
        var context = new TestFunctionContext();
        context.Features.Set(new JwtPrincipalFeature(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "test")), null!));
        if (contextRole is not null)
            context.Features.Set(new McpCallerContextFeature(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, contextRole)], "test"))));
        var previous = AsyncContext.Current;
        try
        {
            AsyncContext.Current = new AsyncContext { Context = context };
            var router = provider.GetRequiredService<Router>();
            var mapping = router.Routes.SelectMany(group => group.Value).Single(route => route.Name == "loyaltyRedeemLinkingProof");
            if (allowed)
                await Assert.ThrowsAsync<InvalidOperationException>(() => router.Invoke("{}", mapping));
            else
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => router.Invoke("{}", mapping));
        }
        finally { AsyncContext.Current = previous; }
    }
}
