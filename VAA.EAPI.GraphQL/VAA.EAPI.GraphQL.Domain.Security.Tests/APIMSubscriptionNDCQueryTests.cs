using HotChocolate;
using HotChocolate.AspNetCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Security.Interfaces;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Response;
using VAA.EAPI.GraphQL.Domain.Security.Models.NDC;
using VAA.EAPI.GraphQL.Domain.Security.Queries;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Security.Tests;

public sealed class APIMSubscriptionNDCQueryTests
{
    [Fact]
    public async Task GetSubscription_UsesAgencyId_WhenAgencyOnlyIsSupplied()
    {
        var provider = new StubSubscriptionQueryProvider();

        await CreateQuery(provider).GetAPIMSubscriptionNDC(Request(Agency(), null));

        Assert.Equal("agency-123", provider.Request!.SubscriptionId);
    }

    [Fact]
    public async Task GetSubscription_UsesAggregatorId_WhenAggregatorOnlyIsSupplied()
    {
        var provider = new StubSubscriptionQueryProvider();

        await CreateQuery(provider).GetAPIMSubscriptionNDC(Request(null, Aggregator()));

        Assert.Equal("aggregator-123", provider.Request!.SubscriptionId);
    }

    [Fact]
    public async Task GetSubscription_UsesCombinedId_WhenAgencyAndAggregatorAreSupplied()
    {
        var provider = new StubSubscriptionQueryProvider();

        await CreateQuery(provider).GetAPIMSubscriptionNDC(Request(Agency(), Aggregator()));

        Assert.Equal("agency-123-aggregator-123", provider.Request!.SubscriptionId);
    }

    [Fact]
    public async Task GetSubscription_UsesAggregatorIdOnly_WhenSingleKeyAggregatorIsTrue()
    {
        var provider = new StubSubscriptionQueryProvider();
        var aggregator = Aggregator();
        aggregator.SingleKeyAggregator = true;

        await CreateQuery(provider).GetAPIMSubscriptionNDC(Request(Agency(), aggregator));

        Assert.Equal("aggregator-123", provider.Request!.SubscriptionId);
    }

    [Fact]
    public async Task GetSubscription_RejectsRequest_WhenPartyDetailsAreMissing()
    {
        var provider = new StubSubscriptionQueryProvider();

        await Assert.ThrowsAsync<GraphQLRequestException>(() =>
            CreateQuery(provider).GetAPIMSubscriptionNDC(Request(null, null)));

        Assert.Null(provider.Request);
    }

    private static APIMSubscriptionNDCQuery CreateQuery(StubSubscriptionQueryProvider provider) =>
        new(LoggerFactory.Create(_ => { }).CreateLogger<APIMSubscriptionNDCQuery>(), provider);

    private static NDCSubscriptionRetrieveQueryRequest Request(
        NDCAgencyDetails? agency,
        NDCAggregatorDetails? aggregator) => new()
        {
            Channel = "NDC",
            Environment = "NONPROD",
            Party = new PartyDetails { Agency = agency, Aggregator = aggregator }
        };

    private static NDCAgencyDetails Agency() => new() { AgencyId = "agency-123", AgencyName = "Synthetic Agency" };
    private static NDCAggregatorDetails Aggregator() => new() { AggregatorId = "aggregator-123", AggregatorName = "Synthetic Aggregator" };

    private sealed class StubSubscriptionQueryProvider : IAPIMSubscriptionQueryProvider
    {
        public SubscriptionRetrieveRequest? Request { get; private set; }

        public Task<SubscriptionResponse> GetSubscription(SubscriptionRetrieveRequest request)
        {
            Request = request;
            return Task.FromResult(new SubscriptionResponse());
        }
    }
}
