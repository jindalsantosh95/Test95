using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Reference.Application.Query;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Reference.Tests
{
    public class CurrencyExchangeRateQueryHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public CurrencyExchangeRateQueryHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("BlobStorageUri", "https://tempuri.org/");
            ConfigReader.Set("ContainerName", "container");
            ConfigReader.Set("SasKey", "xxx");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                                                      .AddSingleton<CurrencyExchangeRateQueryHandler>()
                                                      .BuildServiceProvider();

            var storageAccountUrl = ConfigReader.Get("BlobStorageUri");
            var container = ConfigReader.Get("ContainerName");
            var name = "CurrencyConversion.json";
            var sasKey = ConfigReader.Get("SasKey");
            var blobUri = Core.Xunit.Azure.AzureBlobService.CreateBlobUri(storageAccountUrl, container, name, sasKey);

            var azureBlobService = _serviceProvider.GetService<IAzureBlobService>()!;

            azureBlobService.DownloadBlobAsync(blobUri, name, CancellationToken.None)
                            .Wait();
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            var handler = _serviceProvider.GetService<CurrencyExchangeRateQueryHandler>()!;

            var result = await handler.Handle();

            Assert.NotNull(result); 
            Assert.True(result.UsdToGbpExchangeRate > 0);
            Assert.Equal("0.73", result.UsdToGbpExchangeRate.ToString());
            Assert.NotNull(result.UpdateTimestamp);
        }
    }
}
