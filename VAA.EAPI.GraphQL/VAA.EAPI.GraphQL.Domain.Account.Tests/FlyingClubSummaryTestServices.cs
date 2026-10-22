using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;
using TestHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

internal static class FlyingClubSummaryTestServices
{
    public const string Root = "https://testresources.virginatlantic.com/summary";
    public const string ApplicationToken = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJleHAiOjQxMDI0NDQ4MDB9.";

    public static ServiceProvider Create(string? identityUrl = null, string? customerUrl = null)
    {
        const string root = Root;
        Setup.CreateTestEnvironmentSettings();

        ConfigReader.Set(Environment.IdentityMsUrl, identityUrl ?? root + "/identity");
        ConfigReader.Set(Environment.CustomerUrlGetByMemNum, customerUrl ?? root + "/customer/{memberid}");
        ConfigReader.Set(Environment.CustTransactionMsUrl, root + "/transactions");
        ConfigReader.Set(Environment.VredMsUrl, root + "/points");
        return new ServiceCollection().AddLogging()
            .AddSingleton<IHttpClientFactory, TestHttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IOAuthTokenProvider, SummaryApplicationTokenProvider>()
            .AddSingleton<IApplicationOAuthTokenProvider, SummaryApplicationTokenProvider>()
            .AddSingleton<IFlyingClubTokenProvider, FlyingClubTokenProvider>()
            .AddSingleton<IAccountCustomerService, CustomerService>()
            .AddSingleton(typeof(ICustomerTransactionQueryProvider<>), typeof(CustomerTransactionQueryProvider<>))
            .AddSingleton<IVredPointProvider, VredPointProvider>()
            .AddSingleton<IFlyingClubAccountSummaryProvider, FlyingClubAccountSummaryProvider>()
            .AddSingleton<VAA.EAPI.GraphQL.Domain.Account.Queries.FlyingClubAccountSummaryQuery>()
            .BuildServiceProvider();
    }

    // MSAL owns its HTTP stack, so the connector mock-file harness cannot intercept token acquisition.
    // Keep that SDK boundary deterministic while exercising all summary HTTP services unchanged.
    private sealed class SummaryApplicationTokenProvider : IOAuthTokenProvider, IApplicationOAuthTokenProvider
    {
        public Task<string> AcquireTokenAsync(string resource) => Task.FromResult(ApplicationToken);

        public Task<string> AcquireApplicationTokenAsync(string resource) => Task.FromResult(ApplicationToken);
    }
}
