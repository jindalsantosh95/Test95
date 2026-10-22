using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.BulkMemberUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Vred;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.VredMs;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BulkMemberUpdate;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class BulkMemberUpdateCommandHandlerTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public BulkMemberUpdateCommandHandlerTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("containerNameForBulkUploadInput", "testcontainer");
            ConfigReader.Set("containernameForBulkUploadOutput", "testcontainerResponse");
            ConfigReader.Set("archiveContainerNameForBulkUpload", "testcontainerArchive");
            ConfigReader.Set("blobStorageUri", "https://tempuri.org/");
            ConfigReader.Set("containerName", "container");
            ConfigReader.Set("loyaltymbrmgmtsaskey", "xxx");
            ConfigReader.Set("bulkUploadBlob", "bulkupdatefile.csv");
            ConfigReader.Set("microservicedbkey", "xxx");
            ConfigReader.Set("microserviceeventstoretablename", "test");
            ConfigReader.Set("blobconnstringforbulkupload", "xxx");
            ConfigReader.Set("loyaltyMbrMgmt:AppId", "123");
            ConfigReader.Set("loyaltyMbrMgmt:ClientSecret", "DUMMY");
            ConfigReader.Set("customerTxnMS:ResourceId", "dummy123");
            ConfigReader.Set("loyaltyMbrMgmt:OcpSubID", "123");
            ConfigReader.Set("azureADEndpoint", "https://dummy.com/123/");
            ConfigReader.Set("customermsurl", "https://dummy.com/123/");
            ConfigReader.Set("customer:appid", "1234");
            ConfigReader.Set("servicebusconnectionstring", "testConnectionString");
            ConfigReader.Set("vredmssubscriptionkey", Guid.NewGuid().ToString());
            ConfigReader.Set("updatevredcustomerdetailsresponseurl", "http://tempuri.org");

            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                                                      .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                                                      .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                                                      .AddSingleton<IMessageBus>(azureServiceBusFactory.GetClient("testConnectionString", "testSource"))
                                                      .AddSingleton<IRequestHandler<EventRequestInsertCommand, bool>, EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<IRequestHandler<EventStoreRequestDeleteCommand, bool>, EventStoreRequestDeleteCommandHandler>()
                                                      .AddSingleton<IAzureServiceBusTopicClient, AzureServiceBusTopicClient>()
                                                      .AddSingleton<EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                                                      .AddSingleton<CustomerSearchDocumentServiceQueryHandler>((sp) =>
                                                      {
                                                          var httpConnector = sp.GetService<IHttpConnector>()!;
                                                          var logger = sp.GetService<ILogger<CustomerSearchDocumentServiceQueryHandler>>()!;

                                                          var customhandler = new Mock<CustomerSearchDocumentServiceQueryHandler>(httpConnector, logger);

                                                          customhandler.Setup(Object => Object.Handle(It.IsAny<CustomerSearchDocumentServiceQuery>(), It.IsAny<CancellationToken>()))
                                                                       .ReturnsAsync((Result.Success(new Response<CustomerSearch>
                                                                       {
                                                                           Status = Status.OK,
                                                                           IgnoreNullValues = false,
                                                                           Value = new CustomerSearch()
                                                                           {
                                                                               CustomerId = "234489121",
                                                                               FirstName = "John",
                                                                               LastName = "Smith",
                                                                               LoyaltyMemberNumber = "1501489817",
                                                                               EmailAddress = "xyuz@yyy.com",
                                                                           }
                                                                       })));

                                                          return customhandler.Object;
                                                      })
                                                      .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                                                      .AddSingleton<AzureStorageCommandHandler>((sp) =>
                                                      {
                                                          var azureStorageCommandHandler = new Mock<AzureStorageCommandHandler>();

                                                          azureStorageCommandHandler.Setup(x => x.Handle(It.IsAny<AzureStorageCommand>(), It.IsAny<CancellationToken>()));

                                                          return azureStorageCommandHandler.Object;
                                                      })
                                                      .AddSingleton<IVredCustomerDetailsResponseQuery>((sp) =>
                                                      {
                                                          var updateVredCustomerCommandHandler = new Mock<IVredCustomerDetailsResponseQuery>();

                                                          updateVredCustomerCommandHandler.Setup(x => x.Handle(It.IsAny<UpdateVredCustomerDetailsResponseQuery>(), It.IsAny<CancellationToken>()))
                                                                                          .ReturnsAsync(new UpdateVredCustomerDetailsRS
                                                                                          {

                                                                                              UpdateVredCustomerDetailsResponse = new UpdateVredCustomerDetailsQueryResponse
                                                                                              {
                                                                                                  ProcessingStatus = "Completed"
                                                                                              },

                                                                                              Error = Enumerable.Empty<ErrorDetails>()
                                                                                          });


                                                          return updateVredCustomerCommandHandler.Object;
                                                      })
                                                      .AddSingleton<IBulkMemberUpdateCommand, BulkMemberUpdateCommandHandler>()
                                                      .BuildServiceProvider();

            var storageAccountUrl = ConfigReader.Get("blobStorageUri");
            var container = ConfigReader.Get("containerName");
            var name = "bulkupdatefile.csv";
            var sasKey = ConfigReader.Get("loyaltymbrmgmtsaskey");
            var azureBlobService = _serviceProvider.GetService<IAzureBlobService>()!;
            var blobUri = Core.Xunit.Azure.AzureBlobService.CreateBlobUri(storageAccountUrl, container, name, sasKey);

            azureBlobService.DownloadBlobAsync(blobUri, name, CancellationToken.None)
                                .Wait();
        }

        [Fact]
        public async Task BulkMemberUpdateCommandHandler_ValidScenario_ThrowsExpectedException()
        {
            var handler = _serviceProvider.GetService<IBulkMemberUpdateCommand>()!;
            var bulkMemberUpdateRequest = new BulkMemberUpdateRequest { BlobFilePath = "xxx", MemberNotes = "", MemberStatus = "suspended", InitiatedBy = "testing@fly.com" };

            await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(bulkMemberUpdateRequest, CancellationToken.None));
        }

        [Fact]
        public async Task BulkMemberUpdateCommandHandler_NotValidScenario()
        {
            var handler = _serviceProvider.GetService<IBulkMemberUpdateCommand>()!;

            await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task BulkMemberUpdateCommandHandler_ValidScenario()
        {
            var handler = _serviceProvider.GetService<IBulkMemberUpdateCommand>()!;
            var bulkMemberUpdateRequest = new BulkMemberUpdateRequest
            {
                BlobFilePath = "https://temporg.blob.core.windows.net/bulkmemberupdate/bulkupdatefile.csv",
                MemberNotes = "test",
                MemberStatus = "suspended",
                InitiatedBy = "testing@fly.com"
            };

            var result = await handler.Handle(bulkMemberUpdateRequest, CancellationToken.None);

            Assert.NotNull(result.Value);
            Assert.False(result.IsFailure);
            Assert.True(result.IsSuccess);
            Assert.Equal("BulkUpload Processed Successfully", result.Value.Value.Message);
            Assert.Equal("OK", result.Value.Value.Status);
            Assert.True(result.Value.Errors == null);
            Assert.True(result.Value.ErrorMessage == null);
        }

        [Fact]
        public async Task BulkMemberUpdateCommandHandler_StatusNullValidScenario()
        {
            var handler = _serviceProvider.GetService<IBulkMemberUpdateCommand>()!;
            var bulkMemberUpdateRequest = new BulkMemberUpdateRequest
            {
                BlobFilePath = "https://temporg.blob.core.windows.net/bulkmemberupdate/bulkupdatefile.csv",
                MemberNotes = "test",
                MemberStatus = "",
                InitiatedBy = "testing@fly.com"
            };

            var result = await handler.Handle(bulkMemberUpdateRequest, CancellationToken.None);

            Assert.NotNull(result.Value);
            Assert.False(result.IsFailure);
            Assert.True(result.IsSuccess);
            Assert.Equal("BulkUpload Processed Successfully", result.Value.Value.Message);
            Assert.Equal("OK", result.Value.Value.Status);
            Assert.True(result.Value.Errors == null);
            Assert.True(result.Value.ErrorMessage == null);
        }
    }
}
