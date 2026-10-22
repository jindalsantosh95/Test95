using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request;
using VAA.EAPI.GraphQL.Domain.Security.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Security.Tests;

public sealed class APIMSubscriptionProviderTests
{
    [Fact]
    public async Task CreateSubscription_UsesCheckedInMockResponse()
    {
        ConfigReader.Set(Environment.APIMSubscriptionMSCreateUrl, "https://synthetic.example.test/apim/subscriptions");
        ConfigReader.Set(Environment.APIMSubscriptionMSOcpKey, "synthetic-key");

        var provider = new APIMSubscriptionProvider(
            new StubTokenProvider(),
            new HttpConnector(NullLogger<HttpConnector>.Instance, new HttpClientFactory()));

        var response = await provider.CreateSubscription(new SubscriptionCreateRequest
        {
            Channel = "NDC",
            Environment = "NONPROD",
            SubscriptionId = "aggregator-123",
            SubscriptionName = "Synthetic Aggregator",
            UserAccount = new UserAccountDetails
            {
                FirstName = "Synthetic",
                LastName = "Agent",
                EmailAddress = "synthetic.agent@example.test"
            }
        });

        Assert.Equal("Enabled", response.SubscriptionState);
        Assert.Single(response.SubscriptionKeys);
        Assert.Equal("17ca76a7ec334151a4e55b0d44b7769f", response.SubscriptionKeys[0].Key);
    }

    private sealed class StubTokenProvider : VAA.EAPI.GraphQL.Infrastructure.Interfaces.IOAuthTokenProvider
    {
        public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("synthetic-token");
    }
}
