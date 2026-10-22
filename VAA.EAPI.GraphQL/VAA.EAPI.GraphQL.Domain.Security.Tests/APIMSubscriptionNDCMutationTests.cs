using HotChocolate.AspNetCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request;
using VAA.EAPI.GraphQL.Domain.Security.Models.NDC;
using VAA.EAPI.GraphQL.Domain.Security.Mutations;
using VAA.EAPI.GraphQL.Domain.Security.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Security.Tests;

public sealed class APIMSubscriptionNDCMutationTests
{
    [Fact]
    public async Task Create_UsesAggregatorIdOnly_ForSingleKeyAggregator()
    {
        var factory = new HttpClientFactory();
        var response = await CreateMutation(factory).CreateAPIMSubscriptionNDC(CreateRequest(true));

        Assert.Equal("Enabled", response.SubscriptionState);
        Assert.Single(response.SubscriptionKeys);
        var requestBody = await GetRequestBody(factory, "https://synthetic.example.test/apim/subscriptions");
        Assert.Contains("\"subscriptionId\":\"aggregator-123\"", requestBody);
        Assert.Contains("\"subscriptionName\":\"Synthetic Aggregator\"", requestBody);
    }

    [Fact]
    public async Task Create_RejectsAgencyDetails_ForSingleKeyAggregator()
    {
        var request = CreateRequest(true);
        request.Party.Agency = Agency();

        await Assert.ThrowsAsync<GraphQLRequestException>(() => CreateMutation(new HttpClientFactory()).CreateAPIMSubscriptionNDC(request));
    }

    [Fact]
    public async Task Create_RejectsAggregatorOnlyRequest_WhenSingleKeyAggregatorIsNotTrue()
    {
        var request = CreateRequest(false);

        await Assert.ThrowsAsync<GraphQLRequestException>(() => CreateMutation(new HttpClientFactory()).CreateAPIMSubscriptionNDC(request));
    }

    [Fact]
    public async Task Create_UsesAgencyAndAggregatorId_ForRegularAggregatorRequest()
    {
        var request = CreateRequest(false);
        request.Party.Agency = Agency();

        var factory = new HttpClientFactory();
        var response = await CreateMutation(factory).CreateAPIMSubscriptionNDC(request);

        Assert.Equal("Enabled", response.SubscriptionState);
        var requestBody = await GetRequestBody(factory, "https://synthetic.example.test/apim/subscriptions");
        Assert.Contains("\"subscriptionId\":\"agency-123-aggregator-123\"", requestBody);
        Assert.Contains("\"subscriptionName\":\"Synthetic Agency-Synthetic Aggregator\"", requestBody);
    }

    [Fact]
    public async Task Update_UsesAggregatorSubscriptionId()
    {
        var factory = new HttpClientFactory();
        var response = await CreateMutation(factory).UpdateAPIMSubscriptionNDC(new NDCSubscriptionUpdateMutationRequest
        {
            Channel = "NDC", Environment = "NONPROD", Enabled = true,
            Party = new PartyDetails { Agency = Agency(), Aggregator = Aggregator(true) }
        });

        Assert.Equal("Enabled", response.SubscriptionState);
        Assert.NotEmpty(factory.GetResponses("https://synthetic.example.test/apim/subscriptions/aggregator-123"));
    }

    [Fact]
    public async Task Update_RejectsAggregatorOnlyRequest_WhenSingleKeyAggregatorIsNotTrue()
    {
        await Assert.ThrowsAsync<GraphQLRequestException>(() => CreateMutation(new HttpClientFactory()).UpdateAPIMSubscriptionNDC(new NDCSubscriptionUpdateMutationRequest
        {
            Channel = "NDC", Environment = "NONPROD", Enabled = true,
            Party = new PartyDetails { Aggregator = Aggregator() }
        }));
    }

    [Fact]
    public async Task Delete_UsesAgencyAndAggregatorSubscriptionId()
    {
        var factory = new HttpClientFactory();
        var response = await CreateMutation(factory).DeleteAPIMSubscriptionNDC(new NDCSubscriptionDeleteMutationRequest
        {
            Channel = "NDC", Environment = "NONPROD",
            Party = new PartyDetails { Agency = Agency(), Aggregator = Aggregator() }
        });

        Assert.Equal("Enabled", response.SubscriptionState);
        Assert.NotEmpty(factory.GetResponses("https://synthetic.example.test/apim/subscriptions/agency-123-aggregator-123"));
    }

    [Fact]
    public async Task Delete_UsesAggregatorSubscriptionId_ForSingleKeyAggregator()
    {
        var factory = new HttpClientFactory();
        var response = await CreateMutation(factory).DeleteAPIMSubscriptionNDC(new NDCSubscriptionDeleteMutationRequest
        {
            Channel = "NDC", Environment = "NONPROD",
            Party = new PartyDetails { Agency = Agency(), Aggregator = Aggregator(true) }
        });

        Assert.Equal("Enabled", response.SubscriptionState);
        Assert.NotEmpty(factory.GetResponses("https://synthetic.example.test/apim/subscriptions/aggregator-123"));
    }

    [Fact]
    public async Task Delete_RejectsAggregatorOnlyRequest_WhenSingleKeyAggregatorIsNotTrue()
    {
        await Assert.ThrowsAsync<GraphQLRequestException>(() => CreateMutation(new HttpClientFactory()).DeleteAPIMSubscriptionNDC(new NDCSubscriptionDeleteMutationRequest
        {
            Channel = "NDC", Environment = "NONPROD",
            Party = new PartyDetails { Aggregator = Aggregator() }
        }));
    }

    private static APIMSubscriptionNDCMutation CreateMutation(HttpClientFactory factory)
    {
        ConfigReader.Set(Environment.APIMSubscriptionMSCreateUrl, "https://synthetic.example.test/apim/subscriptions");
        ConfigReader.Set(Environment.APIMSubscriptionMSUpdateUrl, "https://synthetic.example.test/apim/subscriptions/{subscriptionid}");
        ConfigReader.Set(Environment.APIMSubscriptionMSDeleteUrl, "https://synthetic.example.test/apim/subscriptions/{subscriptionid}");
        ConfigReader.Set(Environment.APIMSubscriptionMSOcpKey, "synthetic-key");
        var provider = new APIMSubscriptionProvider(new StubTokenProvider(), new HttpConnector(NullLogger<HttpConnector>.Instance, factory));
        return new APIMSubscriptionNDCMutation(NullLogger<APIMSubscriptionNDCMutation>.Instance, provider);
    }

    private static async Task<string> GetRequestBody(HttpClientFactory factory, string uri)
    {
        var response = Assert.Single(factory.GetResponses(uri));
        return await response.RequestMessage!.Content!.ReadAsStringAsync();
    }

    private static NDCSubscriptionCreateMutationRequest CreateRequest(bool singleKeyAggregator) => new()
    {
        Channel = "NDC", Environment = "NONPROD",
        UserAccount = new UserAccountDetails { FirstName = "Synthetic", LastName = "Agent", EmailAddress = "synthetic.agent@example.test" },
        Party = new PartyDetails { Aggregator = Aggregator(singleKeyAggregator) }
    };

    private static NDCAgencyDetails Agency() => new() { AgencyId = "agency-123", AgencyName = "Synthetic Agency" };
    private static NDCAggregatorDetails Aggregator(bool singleKeyAggregator = false) => new() { AggregatorId = "aggregator-123", AggregatorName = "Synthetic Aggregator", SingleKeyAggregator = singleKeyAggregator };

    private sealed class StubTokenProvider : VAA.EAPI.GraphQL.Infrastructure.Interfaces.IOAuthTokenProvider
    {
        public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("synthetic-token");
    }
}
