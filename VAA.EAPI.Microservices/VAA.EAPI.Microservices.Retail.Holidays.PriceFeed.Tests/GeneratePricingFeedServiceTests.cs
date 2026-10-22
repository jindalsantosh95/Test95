using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Application.PricingFeed;
using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Tests
{
    public class GeneratePricingFeedServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public GeneratePricingFeedServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set(Constants.Environment.EnablePriceFeed, "true");
            ConfigReader.Set(Constants.Environment.FederatedAPIURL, "https://example.com/api/federateddata?destination={0}");
            ConfigReader.Set(Constants.Environment.TravelDestinationBlobName, "Travel_Destinations.json");
            ConfigReader.Set(Constants.Environment.BlobStorageUri, "https://teststorage.blob.core.windows.net");
            ConfigReader.Set(Constants.Environment.ContainerName, "testcontainer");
            ConfigReader.Set(Constants.Environment.SasKey, "testsaskey");
            ConfigReader.Set(Constants.Environment.VholsBlobStorageUrl, "https://testvholsstorage.blob.core.windows.net");
            ConfigReader.Set(Constants.Environment.VholsBlobContainer, "testvholscontainer");
            ConfigReader.Set(Constants.Environment.VholsBlobSasKey, "testvholssask");

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            _serviceProvider = new ServiceCollection().AddLogging()
                                                                  .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                                  .AddSingleton<IHttpConnector, HttpConnector>()
                                                                  .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                                                                  .AddSingleton<IGeneratePricingFeedService, GeneratePricingFeedService>()
                                                                  .AddSingleton<IFederatedDataService, FederatedDataService>()
                                                                  .AddSingleton<ITravelDestinations, TravelDestinations>()
                                                                  .BuildServiceProvider();

            var storageAccountUrl = ConfigReader.Get(Constants.Environment.BlobStorageUri);
            var container = ConfigReader.Get(Constants.Environment.ContainerName);
            var name = ConfigReader.Get(Constants.Environment.TravelDestinationBlobName);
            var sasKey = ConfigReader.Get(Constants.Environment.SasKey);
            var blobUri = Core.Xunit.Azure.AzureBlobService.CreateBlobUri(storageAccountUrl, container, name, sasKey);

            var azureBlobService = _serviceProvider.GetService<IAzureBlobService>()!;

            azureBlobService.DownloadBlobAsync(blobUri, name, CancellationToken.None).Wait();
        }

        [Fact]
        public async Task GeneratePricingFeedService()
        {
            var generatePricingFeedService = _serviceProvider.GetService<IGeneratePricingFeedService>()!;

            await generatePricingFeedService.PublishAsync();

            var azureBlobService = _serviceProvider.GetService<IAzureBlobService>()!;

            var storageAccountUrl = ConfigReader.Get(Constants.Environment.VholsBlobStorageUrl);
            var container = ConfigReader.Get(Constants.Environment.VholsBlobContainer);
            var sasKey = ConfigReader.Get(Constants.Environment.VholsBlobSasKey);

            var blobs = await azureBlobService.GetBlobsAsync(storageAccountUrl, container, sasKey, CancellationToken.None);

            var csvBlob = blobs.Single(x => x.EndsWith(".csv"));
            Assert.Contains("dubai", csvBlob);

            var csvContent = await azureBlobService.BlobStringAsync(csvBlob, CancellationToken.None);
            var lines = csvContent.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(49, lines.Length);
            Assert.Equal(PricingFeedCsvBuilder.Header, lines[0]);

            var data = lines[1].Split(',');
            Assert.Equal("H0374", data[0]);
            Assert.Equal("/united-arab-emirates/dubai/dubai-beach/atlantis-the-palm", data[3]); 
            Assert.Equal("1432", data[4]);
            Assert.Equal("active", data[5]);
        }


    }
}