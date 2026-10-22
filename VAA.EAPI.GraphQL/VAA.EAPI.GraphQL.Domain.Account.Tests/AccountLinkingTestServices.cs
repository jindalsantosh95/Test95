using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Mutations.Secure;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;
using TestHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

internal static class AccountLinkingTestServices
{
    public const string ApplicationToken = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJleHAiOjQxMDI0NDQ4MDB9.";
    private const string DelegatedToken = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJleHAiOjQxMDI0NDQ4MDB9.ZGVsZWdhdGVk";

    public static IServiceProvider Create(string identityUrl, TestHttpClientFactory? httpClientFactory = null)
    {
        Setup.CreateTestEnvironmentSettings();
        ConfigReader.Set(Environment.IdentityMsUrl, identityUrl);
        ConfigReader.Set(Environment.IdentityOcpKey, "test-key");
        ConfigReader.Set(Environment.IdentityMSAppId, "identity-app");
        SeedApplicationToken();

        httpClientFactory ??= new TestHttpClientFactory();
        return new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory>(httpClientFactory)
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>()
            .AddSingleton<IApplicationOAuthTokenProvider, OAuthTokenProvider_AAD>()
            .AddSingleton<IAccountCustomerService, CustomerService>()
            .AddSingleton<IAccountLinkingEligibilityService, AccountLinkingEligibilityService>()
            .AddSingleton<IAccountLinkingProvider, AccountLinkingProvider>()
            .AddSingleton<AccountLinkingProofMutation>()
            .AddSingleton<AccountLinkingMutation>()
            .BuildServiceProvider();
    }

    private static void SeedApplicationToken()
    {
        var clientId = ConfigReader.Get(Environment.EAPIGraphAppID);
        var resource = ConfigReader.Get(Environment.IdentityMSAppId);
        var cacheField = typeof(OAuthTokenProvider_AAD).GetField("cache", BindingFlags.NonPublic | BindingFlags.Static)!;
        var cache = (ConcurrentDictionary<string, string>)cacheField.GetValue(null)!;
        cache[$"{clientId}/{resource}/application"] = ApplicationToken;
        cache[$"{clientId}/{resource}/delegated/named-member"] = DelegatedToken;
        cache[$"{clientId}/{ConfigReader.Get(Environment.CustomerAppId)}"] = DelegatedToken;
    }

    public static string SeedExpiredDelegatedToken()
    {
        var clientId = ConfigReader.Get(Environment.EAPIGraphAppID);
        var resource = ConfigReader.Get(Environment.IdentityMSAppId);
        var key = $"{clientId}/{resource}/delegated/expired-member";
        TokenCache()[key] = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJleHAiOjF9.";
        return key;
    }

    public static bool HasToken(string key) => TokenCache().ContainsKey(key);

    private static ConcurrentDictionary<string, string> TokenCache()
    {
        var cacheField = typeof(OAuthTokenProvider_AAD).GetField("cache", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (ConcurrentDictionary<string, string>)cacheField.GetValue(null)!;
    }
}
