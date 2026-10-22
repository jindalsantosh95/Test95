using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignAmexUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAmexUpdate;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignAmexUpdateRequestCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        public CampaignAmexUpdateRequestCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("microservicedbkey", "DefaultEndpointsProtocol=https;\r\nAccountName=dummyaccount;\r\nAccountKey=dummyaccountkey1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef;\r\nTableEndpoint=https://dummyaccount.table.core.windows.net/;\r\n");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<ICampaignAmexUpdateRequestCommand, CampaignAmexUpdateRequestCommandHandler>()
                                                      .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))

                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task CampaignAmexUpdateRequestCommandHandler_When_UniqueID_NotFound_FailureScenario()
        {
            var handler = _serviceProvider.GetService<ICampaignAmexUpdateRequestCommand>()!;
            var request = new CampaginAmexUpdateRequest
            {
                AmexUniqueId = "12345",
                FCnumber = "FC001"
            };
            var result = await handler.Handle(request, CancellationToken.None);

            Assert.Equal(Status.Error, result.Value.Status);
            Assert.Equal("Not Found", result.Value.Errors.FirstOrDefault().Message);
            Assert.Equal("404", result.Value.Errors.FirstOrDefault().Code);
            Assert.Equal("Amex PCM Campaign UniqueId not present in table", result.Value.Value);
        }        

        [Fact]
        public async Task CampaignAmexUpdateRequestCommandHandler_UpdatesFCnumberInStorage()
        {
            var handler = _serviceProvider.GetService<ICampaignAmexUpdateRequestCommand>()!;
            var tableService = _serviceProvider.GetService<ITableStorageService<CampaignUniqueIdTableEntity>>()!;

            var entity = new CampaignUniqueIdTableEntity
            {
                PartitionKey = "PCM",
                RowKey = "UID-UPDATE",
                UniqueId = "UID-UPDATE",
                FCNumber = null,
                UsedTimestamp=null 
            };

            await tableService.InsertOrMergeAsync(entity);

            var request = new CampaginAmexUpdateRequest
            {
                AmexUniqueId = "UID-UPDATE",
                FCnumber = "FC999" 
            };

            var result = await handler.Handle(request, CancellationToken.None);

            var updatedEntity = await tableService.RetrieveAsync("PCM", "UID-UPDATE");

            Assert.NotNull(updatedEntity);
            Assert.Equal("FC999", updatedEntity.FCNumber);
            Assert.Equal("UID-UPDATE", updatedEntity.UniqueId);
            Assert.Equal("PCM", updatedEntity.PartitionKey);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("Amex PCM Campaign updated successfully", result.Value.Value);
        }

        [Fact]
        public async Task CampaignAmexUpdateRequestCommandHandler_RemoveDuplicateInStorage()
        {
            var handler = _serviceProvider.GetService<ICampaignAmexUpdateRequestCommand>()!;
            var tableService = _serviceProvider.GetService<ITableStorageService<CampaignUniqueIdTableEntity>>()!;

            var entity = new CampaignUniqueIdTableEntity
            {
                PartitionKey = "PCM",
                RowKey = "UID-UPDATE",
                UniqueId = "uid-update",
                FCNumber = null,
                UsedTimestamp = null
            };

            await tableService.InsertOrMergeAsync(entity);

            var request = new CampaginAmexUpdateRequest
            {
                AmexUniqueId = "uid-update",
                FCnumber = "FC999"
            };

            var result = await handler.Handle(request, CancellationToken.None);
            
            var updatedEntity = await tableService.RetrieveAsync("PCM", "UID-UPDATE");
            var duplicateEntity = await tableService.RetrieveAsync("PCM", "uid-update");

            Assert.NotNull(updatedEntity);
            Assert.Equal("PCM", updatedEntity.PartitionKey);
            Assert.Equal("UID-UPDATE", updatedEntity.RowKey);
            Assert.Equal("uid-update", updatedEntity.UniqueId);
            Assert.Equal("FC999", updatedEntity.FCNumber);
            Assert.Equal(1, ((VAA.EAPI.Core.Xunit.Azure.TableStorageService<VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAmexUpdate.CampaignUniqueIdTableEntity>)tableService).Count);
        }

        [Fact]
        public async Task CampaignAmexUpdateRequestCommandHandler_ThrowsExpectedException()
        {
            var handler = _serviceProvider.GetService<ICampaignAmexUpdateRequestCommand>()!;
            var request = new CampaginAmexUpdateRequest
            {
                AmexUniqueId = "",
                FCnumber = ""
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(request, CancellationToken.None));
        }
    }
}
