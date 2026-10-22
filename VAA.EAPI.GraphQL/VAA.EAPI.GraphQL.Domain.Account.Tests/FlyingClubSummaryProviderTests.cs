using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using TestHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class FlyingClubSummaryProviderTests
{
    [Fact]
    public async Task SummaryUsesTierDetailsAndForwardsTheApplicationToken()
    {
        const string root = FlyingClubSummaryTestServices.Root;
        const string applicationToken = FlyingClubSummaryTestServices.ApplicationToken;
        using var serviceProvider = FlyingClubSummaryTestServices.Create();
        var response = await serviceProvider.GetRequiredService<IFlyingClubAccountSummaryProvider>()
            .GetAsync("synthetic-linked-token");
        Assert.Equal("Gold", response.Tier);
        Assert.Equal(400, response.TierPoints);
        Assert.Equal(1000, response.VirginPoints);
        var requests = ((TestHttpClientFactory)serviceProvider.GetRequiredService<IHttpClientFactory>()).GetResponses(root);
        var validation = Assert.Single(requests, response => response.RequestMessage!.RequestUri!.AbsolutePath.EndsWith("/application-token/validate")).RequestMessage!;
        Assert.Contains("synthetic-linked-token", await validation.Content!.ReadAsStringAsync());
        Assert.Equal(applicationToken, validation.Headers.Authorization!.Parameter);
        Assert.Single(requests, response => response.RequestMessage!.RequestUri!.AbsolutePath == "/summary/transactions/flyingClubMember/summary-customer");
    }

    [Fact]
    public async Task CancellationDuringReadRetryPropagates()
    {
        using var services = FlyingClubSummaryTestServices.Create(customerUrl: FlyingClubSummaryTestServices.Root + "/customer-retry/{memberid}");
        using var cancellation = new CancellationTokenSource();
        var provider = services.GetRequiredService<IFlyingClubAccountSummaryProvider>();
        var task = provider.GetAsync("synthetic-linked-token", cancellation.Token);
        var factory = (TestHttpClientFactory)services.GetRequiredService<IHttpClientFactory>();
        Assert.Single(factory.GetResponses(FlyingClubSummaryTestServices.Root),
            response => response.RequestMessage!.RequestUri!.AbsolutePath.Contains("/customer-retry/"));

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.Single(factory.GetResponses(FlyingClubSummaryTestServices.Root),
            response => response.RequestMessage!.RequestUri!.AbsolutePath.Contains("/customer-retry/"));
    }

    [Theory]
    [InlineData("unauthorized")]
    [InlineData("forbidden")]
    public async Task DownstreamAuthorizationFailureIsTechnicalAndNotRetried(string scenario)
    {
        using var services = FlyingClubSummaryTestServices.Create(customerUrl: FlyingClubSummaryTestServices.Root + "/customer-" + scenario + "/{memberid}");
        var provider = services.GetRequiredService<IFlyingClubAccountSummaryProvider>();

        var error = await Assert.ThrowsAsync<Exception>(() => provider.GetAsync("synthetic-linked-token"));

        Assert.Equal("ACCOUNT_SUMMARY_DEPENDENCY_UNAVAILABLE", error.Message);
        var factory = (TestHttpClientFactory)services.GetRequiredService<IHttpClientFactory>();
        Assert.Single(factory.GetResponses(FlyingClubSummaryTestServices.Root),
            response => response.RequestMessage!.RequestUri!.AbsolutePath.Contains("/customer-" + scenario + "/"));
    }
}
