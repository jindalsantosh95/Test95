using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Holidays.Application;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents;

namespace VAA.EAPI.Microservices.Retail.Holidays.Tests;

public sealed class RetrieveDocumentDownloadContractTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly IServiceProvider _serviceProvider;

    public RetrieveDocumentDownloadContractTests(HttpClientFactoryFixture httpClientFactoryFixture)
    {
        ConfigReader.Set("Holidays:AuthorizationProvider", "AZURE");

        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IOAuthTokenProvider, Core.Xunit.Azure.OAuthTokenProvider>()
            .AddSingleton<IRetrieveDocumentService, RetrieveDocumentService>()
            .BuildServiceProvider();
    }

    [Fact]
    public void RetrieveDocumentService_IsRegistered()
    {
        Assert.IsType<RetrieveDocumentService>(_serviceProvider.GetRequiredService<IRetrieveDocumentService>());
    }
}
