using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.FlyingClubToken;
using VAA.EAPI.GraphQL.Domain.Account.Mutations.Secure;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;
using TestHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class FlyingClubTokenMutationTests
{
    private const string IdentityUrl = "https://testresources.virginatlantic.com/flying-club/identity";
    private readonly IServiceProvider _serviceProvider;

    public FlyingClubTokenMutationTests()
    {
        Setup.CreateTestEnvironmentSettings();
        ConfigReader.Set(Environment.IdentityMsUrl, IdentityUrl);
        ConfigReader.Set(Environment.IdentityOcpKey, "test-key");
        SeedApplicationToken();

        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory, TestHttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>()
            .AddSingleton<IApplicationOAuthTokenProvider, OAuthTokenProvider_AAD>()
            .AddSingleton<IFlyingClubTokenProvider, FlyingClubTokenProvider>()
            .AddSingleton<FlyingClubTokenMutation>()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task IssueAsync_ReturnsIdentityResponse_WhenAccountNumberIsValid()
    {
        var mutation = _serviceProvider.GetRequiredService<FlyingClubTokenMutation>();

        var response = await mutation.IssueAsync(new FlyingClubTokenRequest
        {
            FlyingClubAccountNumber = " 1234567890 "
        });

        Assert.Equal("signed.jwt.value", response.AccessToken);
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal(300, response.ExpiresIn);

        var responses = ((TestHttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>()).GetResponses(IdentityUrl);
        var request = responses.Single(x =>
            x.RequestMessage!.RequestUri!.ToString() == $"{IdentityUrl}/application-token").RequestMessage!;
        var body = Json.Deserialize<IdentityFlyingClubTokenRequest>(await request.Content!.ReadAsStringAsync());
        Assert.Equal("1234567890", body.FlyingClubAccountNumber);
        Assert.Equal(ConfigReader.Get(Environment.EAPIGraphAppID), body.CallerApplicationId);
    }

    [Fact]
    public async Task LegacyRegistrationDoesNotRequireApplicationTokenService()
    {
        using var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory, TestHttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>()
            .AddSingleton<IFlyingClubTokenProvider, FlyingClubTokenProvider>()
            .BuildServiceProvider();
        var provider = services.GetRequiredService<IFlyingClubTokenProvider>();
        var response = await provider.IssueAsync(new FlyingClubTokenRequest { FlyingClubAccountNumber = "1234567890" });
        Assert.Equal("signed.jwt.value", response.AccessToken);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ABC123")]
    [InlineData("123456789012345678901")]
    public async Task IssueAsync_ThrowsValidationException_WhenAccountNumberIsInvalid(string? accountNumber)
    {
        var mutation = _serviceProvider.GetRequiredService<FlyingClubTokenMutation>();

        await Assert.ThrowsAsync<RequestValidationException>(() => mutation.IssueAsync(new FlyingClubTokenRequest
        {
            FlyingClubAccountNumber = accountNumber
        }));
    }

    private static void SeedApplicationToken()
    {
        var clientId = ConfigReader.Get(Environment.EAPIGraphAppID);
        var resource = ConfigReader.Get(Environment.IdentityMSAppId);
        var cacheField = typeof(OAuthTokenProvider_AAD).GetField("cache", BindingFlags.NonPublic | BindingFlags.Static)!;
        var cache = (ConcurrentDictionary<string, string>)cacheField.GetValue(null)!;
        cache[$"{clientId}/{resource}"] = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJleHAiOjQxMDI0NDQ4MDB9.";
        cache[$"{clientId}/{resource}/application"] = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJleHAiOjQxMDI0NDQ4MDB9.";
    }

    private sealed class IdentityFlyingClubTokenRequest
    {
        public string FlyingClubAccountNumber { get; set; } = string.Empty;

        public string CallerApplicationId { get; set; } = string.Empty;
    }
}
