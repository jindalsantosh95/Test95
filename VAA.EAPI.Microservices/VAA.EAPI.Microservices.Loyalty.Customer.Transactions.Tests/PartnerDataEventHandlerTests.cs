using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class PartnerDataEventHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public PartnerDataEventHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("microservicedbkey",
                "DefaultEndpointsProtocol=https;AccountName=dummyaccount;AccountKey=dummyaccountkey1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef;TableEndpoint=https://dummyaccount.table.core.windows.net/");

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IPartnerDataEventHandler, PartnerDataEventHandler>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .BuildServiceProvider();
        }

        [Fact]
        public async Task HandleAsync_ValidPartners_InsertsIntoStorage()
        {
            var handler = _serviceProvider.GetService<IPartnerDataEventHandler>()!;
            var table = _serviceProvider.GetService<ITableStorageService<PartnerDetails>>()!;

            var message = new PartnerDataMessage
            {
                Data = new PartnerDataPayload
                {
                    Partners = new List<Partner>
                    {
                        new Partner
                        {
                            PartnerId = "P001",
                            PartnerName = "Test Partner 1",
                            PartnerCode = "A",
                            PartnerType = "Airline",
                            PartnerSubType = "Primary",
                            SkyTeamPartnerFlag = 0
                        },
                        new Partner
                        {
                            PartnerId = "P002",
                            PartnerName = "Test Partner 2",
                            PartnerCode = "B",
                            PartnerType = "Airline",
                            PartnerSubType = "Global",
                            SkyTeamPartnerFlag = 1
                        }
                    }
                }
            };

            await handler.HandleAsync(message);

            var entity1 = await table.RetrieveAsync(Constants.Application.PartnerPartitionKey, "P001");
            Assert.NotNull(entity1);
            Assert.Equal("Test Partner 1", entity1.PartnerName);
            Assert.Equal("A", entity1.PartnerCode);
            Assert.Equal("Airline", entity1.PartnerType);
            Assert.Equal("Primary", entity1.PartnerSubType);
            Assert.False(entity1.SkyTeamPartnerFlag);

            var entity2 = await table.RetrieveAsync(Constants.Application.PartnerPartitionKey, "P002");
            Assert.NotNull(entity2);
            Assert.Equal("Test Partner 2", entity2.PartnerName);
            Assert.Equal("B", entity2.PartnerCode);
            Assert.Equal("Airline", entity2.PartnerType);
            Assert.Equal("Global", entity2.PartnerSubType);
            Assert.True(entity2.SkyTeamPartnerFlag);
        }

        [Fact]
        public async Task HandleAsync_PartnerIdMissing_DoesNothing()
        {
            var handler = _serviceProvider.GetService<IPartnerDataEventHandler>()!;
            var table = _serviceProvider.GetService<ITableStorageService<PartnerDetails>>()!;

            var message = new PartnerDataMessage
            {
                Data = new PartnerDataPayload
                {
                    Partners = new List<Partner>
                    {
                        new Partner
                        {
                            PartnerId = "",
                            PartnerName = "Test Partner",
                            PartnerCode = "TP",
                            PartnerType = "TestType",
                            PartnerSubType = "TestSubType",
                            SkyTeamPartnerFlag = 0
                        }
                    }
                }
            };

            await handler.HandleAsync(message);

            var entity = await table.RetrieveAsync(Constants.Application.PartnerPartitionKey, "");
            Assert.Null(entity);
        }

        [Fact]
        public async Task HandleAsync_PartnersEmpty_ThrowsException()
        {
            var handler = _serviceProvider.GetService<IPartnerDataEventHandler>()!;

            var message = new PartnerDataMessage
            {
                Data = new PartnerDataPayload
                {
                    Partners = new List<Partner>()
                }
            };

            var ex = await Assert.ThrowsAsync<Exception>(() => handler.HandleAsync(message));

            Assert.Contains("Partners", ex.Message);
        }
    }
}
