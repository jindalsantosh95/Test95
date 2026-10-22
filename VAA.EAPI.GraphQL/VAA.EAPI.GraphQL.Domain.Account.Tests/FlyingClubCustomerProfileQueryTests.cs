using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Queries;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;
using TestHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class FlyingClubCustomerProfileQueryTests
{
    private const string IdentityUrl = "https://testresources.virginatlantic.com/flying-club/identity";
    private const string CustomerUrl = "https://testresources.virginatlantic.com/flying-club/customer/loyalty/{memberid}";
    private const string TransactionsUrl = "https://testresources.virginatlantic.com/flying-club/customertransactions/";
    private readonly IServiceProvider _serviceProvider;

    public FlyingClubCustomerProfileQueryTests()
    {
        Setup.CreateTestEnvironmentSettings();
        ConfigReader.Set(Environment.IdentityMsUrl, IdentityUrl);
        ConfigReader.Set(Environment.IdentityOcpKey, "test-key");
        ConfigReader.Set(Environment.CustomerUrlGetByMemNum, CustomerUrl);
        ConfigReader.Set(Environment.CustTransactionMsUrl, TransactionsUrl);
        SeedApplicationTokens();

        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory, TestHttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>()
            .AddSingleton<IFlyingClubTokenProvider, FlyingClubTokenProvider>()
            .AddSingleton<IAccountCustomerService, CustomerService>()
            .AddSingleton(typeof(ICustomerTransactionQueryProvider<>), typeof(CustomerTransactionQueryProvider<>))
            .AddSingleton<IFlyingClubCustomerProfileProvider, FlyingClubCustomerProfileProvider>()
            .AddSingleton<FlyingClubCustomerProfileQuery>()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task GetAsync_ReturnsCustomerProfileAndAccount_FromMockedDependencies()
    {
        AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };
        AsyncContext.Current.Context.Features.Set(new FlyingClubTokenFeature("signed.jwt.value"));
        var query = _serviceProvider.GetRequiredService<FlyingClubCustomerProfileQuery>();

        var response = await query.GetAsync();

        Assert.Equal("12345678-abcd-4321-efab-987654321000", response.Customer.CustomerId);
        Assert.Equal("John", response.Customer.FirstName);
        Assert.Equal("Leisure", response.Profile.ReasonForTravel);
        Assert.Equal("1234567890", response.Account.MemberNumber);
        Assert.Equal("GOLD", response.Account.TierCode);
    }

    [Fact]
    public async Task GetAsync_RejectsRequestWithoutBearerToken()
    {
        AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };
        var query = _serviceProvider.GetRequiredService<FlyingClubCustomerProfileQuery>();

        await Assert.ThrowsAsync<RequestValidationException>(() => query.GetAsync());
    }

    [Fact]
    public async Task GetAsync_RejectsNormalJwtFeature_WhenBrokerTokenFeatureIsMissing()
    {
        AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };
        AsyncContext.Current.Context.Features.Set(new JwtPrincipalFeature(
            new ClaimsPrincipal(),
            new AuthToken { Token = "forged.broker.token" }));
        var query = _serviceProvider.GetRequiredService<FlyingClubCustomerProfileQuery>();

        await Assert.ThrowsAsync<RequestValidationException>(() => query.GetAsync());
    }

    private static void SeedApplicationTokens()
    {
        var clientId = ConfigReader.Get(Environment.EAPIGraphAppID);
        var resources = new[]
        {
            ConfigReader.Get(Environment.IdentityMSAppId),
            ConfigReader.Get(Environment.CustomerAppId),
            ConfigReader.Get(Environment.CustTransAppId)
        };
        var cacheField = typeof(OAuthTokenProvider_AAD).GetField("cache", BindingFlags.NonPublic | BindingFlags.Static)!;
        var cache = (ConcurrentDictionary<string, string>)cacheField.GetValue(null)!;
        foreach (var resource in resources)
            cache[$"{clientId}/{resource}/application"] = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJleHAiOjQxMDI0NDQ4MDB9.";
    }
}
