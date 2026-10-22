using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Reference.Application;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Reference.Tests
{
    public class ReferenceDataCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public ReferenceDataCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("BlobStorageUri", "https://tempuri.org/");
            ConfigReader.Set("ContainerName", "container");
            ConfigReader.Set("SasKey", "xxx");
            ConfigReader.Set("FQTProgramsBlob", "FQTProgrammes.json");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                                                      .AddSingleton<ReferenceDataCommandHandler>()
                                                      .BuildServiceProvider();

            var storageAccountUrl = ConfigReader.Get("BlobStorageUri");
            var container = ConfigReader.Get("ContainerName");
            var name = "FQTProgrammes.json";
            var sasKey = ConfigReader.Get("SasKey");
            var blobUri = Core.Xunit.Azure.AzureBlobService.CreateBlobUri(storageAccountUrl, container, name, sasKey);

            var azureBlobService = _serviceProvider.GetService<IAzureBlobService>()!;

            azureBlobService.DownloadBlobAsync(blobUri, name, CancellationToken.None)
                            .Wait();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ReferenceDataCommandHandler(null));
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            var handler = _serviceProvider.GetService<ReferenceDataCommandHandler>()!;

            ReferenceDataCommand cmd = new()
            {
                ReferenceDataType = ReferenceDataTypeEnum.FQTPROGS
            };

            var result = await handler.Handle(cmd, CancellationToken.None);

            var output = Json.Deserialize<IEnumerable<FQTProgram>>(result);

            Assert.Equal(25, output.Count()); // Sample response has 25 elements
        }
    }
}
