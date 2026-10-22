using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;
using TestHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class AccountLinkingProviderTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly TestHttpClientFactory _httpClientFactory;

    public AccountLinkingProviderTests(HttpClientFactoryFixture fixture)
    {
        Setup.CreateTestEnvironmentSettings();
        ConfigReader.Set(Environment.IdentityOcpKey, "test-key");
        ConfigReader.Set(Environment.IdentityMSAppId, "identity-app");
        _httpClientFactory = fixture.Current;
    }

    [Fact]
    public async Task CreateAsync_ForwardsContractAndReturnsResponse()
    {
        const string baseUrl = "https://testresources.virginatlantic.com/account-linking-success";
        var provider = CreateProvider(baseUrl);

        var response = await provider.CreateAsync(new CreateLinkingProofRequest
        {
            FlyingClubAccountNumber = "1234567890",
            TransactionId = "transaction", ClientId = "alexa-client",
            Resource = "vaa-flying-club", Scope = "account:read"
        });

        Assert.Equal("proof-value", response.Proof);
        Assert.Equal(DateTime.Parse("2026-10-02T12:02:00Z").ToUniversalTime(), response.ExpiresAtUtc);
        var request = _httpClientFactory.GetResponses(baseUrl).Single().RequestMessage!;
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal(AccountLinkingTestServices.ApplicationToken, request.Headers.Authorization?.Parameter);
        Assert.Equal("test-key", request.Headers.GetValues("Ocp-Apim-Subscription-Key").Single());
        var payload = Json.Deserialize<CreateLinkingProofRequest>(await request.Content!.ReadAsStringAsync());
        Assert.Equal("1234567890", payload.FlyingClubAccountNumber);
        Assert.Equal("transaction", payload.TransactionId);
        Assert.Equal("account:read", payload.Scope);
    }

    [Fact]
    public async Task CreateAsync_MapsDownstreamBadRequest()
    {
        var provider = CreateProvider("https://testresources.virginatlantic.com/account-linking-bad");

        var exception = await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => provider.CreateAsync(new CreateLinkingProofRequest()));

        var error = Assert.Single(exception.Errors);
        Assert.Equal("ACCOUNT_LINKING_TRANSACTION_INVALID", error.Code);
        Assert.Equal(400, error.Extensions!["httpStatusCode"]);
    }

    [Fact]
    public async Task CreateAsync_RemovesExpiredDelegatedTokensFromSharedCache()
    {
        var expiredKey = AccountLinkingTestServices.SeedExpiredDelegatedToken();
        var provider = CreateProvider("https://testresources.virginatlantic.com/account-linking-success");

        await provider.CreateAsync(new CreateLinkingProofRequest
        {
            FlyingClubAccountNumber = "1234567890",
            TransactionId = "transaction", ClientId = "alexa-client",
            Resource = "vaa-flying-club", Scope = "account:read"
        });

        Assert.False(AccountLinkingTestServices.HasToken(expiredKey));
    }

    [Fact]
    public async Task CreateAsync_MapsRealHttpClientTimeoutToRetryableDependencyError()
    {
        var previous = ConfigReader.Try(Environment.HttpClientTimeOutInMilliSeconds);
        try
        {
            ConfigReader.Set(Environment.HttpClientTimeOutInMilliSeconds, "100");
            var factory = new TestHttpClientFactory();
            var services = AccountLinkingTestServices.Create("https://testresources.virginatlantic.com/account-linking-timeout", factory);
            var provider = services.GetRequiredService<IAccountLinkingProvider>();
            var exception = await Assert.ThrowsAsync<HotChocolate.GraphQLException>(() => provider.CreateAsync(new CreateLinkingProofRequest()));
            var error = Assert.Single(exception.Errors);
            Assert.Equal("ACCOUNT_LINKING_DEPENDENCY_UNAVAILABLE", error.Code);
            Assert.Equal(503, error.Extensions!["httpStatusCode"]);
            Assert.Equal(503, error.Extensions["transportStatusCode"]);
            Assert.Equal(true, error.Extensions["retryable"]);
        }
        finally { ConfigReader.Set(Environment.HttpClientTimeOutInMilliSeconds, previous); }
    }

    private AccountLinkingProvider CreateProvider(string baseUrl)
    {
        ConfigReader.Set(Environment.IdentityMsUrl, baseUrl);
        return (AccountLinkingProvider)AccountLinkingTestServices.Create(baseUrl, _httpClientFactory)
            .GetRequiredService<IAccountLinkingProvider>();
    }
}
