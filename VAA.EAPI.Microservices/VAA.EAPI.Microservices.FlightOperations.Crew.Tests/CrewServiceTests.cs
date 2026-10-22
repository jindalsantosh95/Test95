using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Azure;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Infrastructure.Services.Dispatcher;
using VAA.EAPI.Microservices.FlightOperations.Crew.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Crew.Application.Models;
using VAA.EAPI.Microservices.FlightOperations.Crew.Application.Service;
using Xunit;

namespace VAA.EAPI.Microservices.FlightOperations.Crew.Tests
{
    public sealed class CrewServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public CrewServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("CrewService:AzureBlobService:StorageAccountUrl", "https://tempuri.org/");
            ConfigReader.Set("CrewService:AzureBlobService:Container", "imports");
            ConfigReader.Set("CrewService:AzureBlobService:SasKey", "sasKey");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                                                      .AddSingleton<ICrewService, CrewService>()
                                                      .BuildServiceProvider();

            var storageAccountUrl = ConfigReader.Get("CrewService:AzureBlobService:StorageAccountUrl");
            var container = ConfigReader.Get("CrewService:AzureBlobService:Container");
            var name = "master_crew_.csv";
            var sasKey = ConfigReader.Get("CrewService:AzureBlobService:SasKey");
            var blobUri = Core.Xunit.Azure.AzureBlobService.CreateBlobUri(storageAccountUrl, container, name, sasKey);

            var azureBlobService = _serviceProvider.GetService<IAzureBlobService>()!;

            azureBlobService.DownloadBlobAsync(blobUri, name, CancellationToken.None)
                            .Wait();
        }

        [Fact]
        public async Task CrewServiceRoster()
        {
            var crewService = _serviceProvider.GetService<ICrewService>()!;

            var response = await crewService.RosterAsync();

            Assert.NotNull(response.Value);
            Assert.NotNull(response.Value.Value);

            var flightCrewRosterResponse = response.Value.Value;

            Assert.Single(flightCrewRosterResponse);
            Assert.Equal("012345", flightCrewRosterResponse.First().Id);
            Assert.Equal("John", flightCrewRosterResponse.First().FirstName);
            Assert.Equal("Doe", flightCrewRosterResponse.First().LastName);
            Assert.Equal("johndoe@tempuri.org", flightCrewRosterResponse.First().Email);
            Assert.Equal("LHR", flightCrewRosterResponse.First().Base);
        }
    }
}
