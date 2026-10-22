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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Query.CustProfileMS;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models.CustProfile;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.AzureStorage;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class AddFCNumberToPNRCommandHandlerTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public AddFCNumberToPNRCommandHandlerTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            var blobUri = new Uri("https://mystorageaccount.blob.core.windows.net/mycontainer/myblob.txt");
            const string DummyConnectionString = "DefaultEndpointsProtocol=https;" + "AccountName=dummyaccount;" + "AccountKey=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=" + "TableEndpoint=https://dummyaccount.table.core.windows.net/;";

            ConfigReader.Set("AzureWebJobsScriptRoot", ".\\");
            ConfigReader.Set("LoyaltyMbrMgmt:AppId", "123");
            ConfigReader.Set("LoyaltyMbrMgmt:ClientSecret", "DUMMY");
            ConfigReader.Set("CustomerTxnMS:ResourceId", "dummy123");
            ConfigReader.Set("LoyaltyMbrMgmt:OcpSubID", "123");
            ConfigReader.Set("AzureADEndpoint", "https://dummy.com/123/");
            ConfigReader.Set("CustomerTxnMSDetailsEndpoint", "http://localhost:7071/api/{0}/{1}");
            ConfigReader.Set("CommandStatusLocation", "http://localhost:7071/api/{0}s/status/{1}");
            ConfigReader.Set("CommandStatusPollingDelayMS", "30");
            ConfigReader.Set("CommandStatusPollingRetryCount", "3");
            ConfigReader.Set("EmailTriggerTopicName", "enrolcustomer");
            ConfigReader.Set("HoHLinkHeadSilverEmalTemplate", "FCGOL-01");
            ConfigReader.Set("HoHLinkChildSilverEmalTemplate", "FCGOL-01A");
            ConfigReader.Set("BirthdayEmailTemplate", "FCGEN-19");
            ConfigReader.Set("RuleEngineTopicName", "ruleengineeventmessages");
            ConfigReader.Set("GroupDocType", "Group");
            ConfigReader.Set("CustomerTransactionTopicName", "customertransactioneventmessages");
            ConfigReader.Set("BlobConnStringForEnrolledMemberToPromotion", DummyConnectionString);
            ConfigReader.Set("ContainerNameForEnrolledMemberToPromotion", "test");
            ConfigReader.Set("CustomerMSUrl", "http://localhost:7071/api/{0}/{1}");
            ConfigReader.Set("HoHAddressUpdateEmailTemplate", "FCGOL-02");
            ConfigReader.Set("IsManualRun", "False");
            ConfigReader.Set("BirthdayDateForManualRun", "2023-09-15");
            ConfigReader.Set("ContainerNameForAddSSRResponse", "testResponse");
            ConfigReader.Set("ContainerNameForAddSSRFile", "test");
            ConfigReader.Set("ArchiveContainerNameForAddSSRFile", "testArchive");
            ConfigReader.Set("ChannelIdToAddSSR", "test");
            ConfigReader.Set("OrderMgt:ServiceBusConnectionString", "https://dummy.com/123/");
            ConfigReader.Set("OrderMgt:ServiceBusTopicName", "testteventmessage");
            ConfigReader.Set("OrderMgt:AppId", "123");
            ConfigReader.Set("OrderMgt:ManageSpecialServiceResponseUrl", "http://localhost:7071/api/{0}/{1}");
            ConfigReader.Set("FCStampingTableName", "testTable");
            ConfigReader.Set("CodeSharePartnerForStatementText", "VS,DL,AF,KL");
            ConfigReader.Set("microservicedbkey", "DefaultEndpointsProtocol=https;\r\nAccountName=dummyaccount;\r\nAccountKey=dummyaccountkey1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef;\r\nTableEndpoint=https://dummyaccount.table.core.windows.net/;\r\n");
            ConfigReader.Set("microserviceeventstoretablename", "testTable");
            ConfigReader.Set("loyaltymbrmgmtsaskey", "xxx");
            ConfigReader.Set("stampfcnumbertopnrcontainerurl", "https://mystorageaccount.blob.core.windows.net/mycontainer/");
            ConfigReader.Set("servicebusconnectionstring", "testConnstring");
            ConfigReader.Set("customertransactionmssubscriptionkey", "testKey");
            ConfigReader.Set("JwtPrincipalFeature", "1325677");

            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                                                      .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                                                      .AddSingleton<IOAuthTokenProvider, Core.Xunit.Azure.OAuthTokenProvider>()
                                                      .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                                                      .AddSingleton<IMessageBus>(azureServiceBusFactory.GetClient("DefaultEndpointsProtocol=https;AccountName=dummyaccount;AccountKey=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=;TableEndpoint=https://dummyaccount.table.core.windows.net/;", "testSource"))
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
                                                      .AddSingleton<AddFCNumberToPNRResponseQueryHandler>()

                                                      .AddSingleton<AzureStorageCommandHandler>((sp) =>
                                                      {
                                                          var azureStorageCommandHandler = new Mock<AzureStorageCommandHandler>();

                                                          azureStorageCommandHandler.Setup(x => x.Handle(It.IsAny<AzureStorageCommand>(), It.IsAny<CancellationToken>()))
                                                                                     .ReturnsAsync(Unit.Value);
                                                          return azureStorageCommandHandler.Object;
                                                      })
                                                      .AddSingleton<IAddFCNumberToPNRCommand, AddFCNumberToPNRCommandHandler>()
                                                      .AddSingleton<ICustomerProfileQuery>((sp) =>
                                                      {
                                                          var tokenProviderMock = new Mock<IOAuthTokenProvider>();
                                                          tokenProviderMock.Setup(tp => tp.AcquireTokenAsync(It.IsAny<string>()))
                                                                           .ReturnsAsync("fake-token");
                                                          var httpConnector = sp.GetService<IHttpConnector>()!;
                                                          var customerProfileHandler = new Mock<ICustomerProfileQuery>();

                                                          customerProfileHandler.Setup(Object => Object.Handle(It.IsAny<CustomerProfileRequest>(), It.IsAny<CancellationToken>()))
                                                                                .ReturnsAsync(Result.Success(new Response<CustomerProfileResponse>
                                                                                {
                                                                                    Status = Status.OK,
                                                                                    IgnoreNullValues = false,
                                                                                    Value = new CustomerProfileResponse
                                                                                    {
                                                                                        CustomerId = "123",
                                                                                        CustomerProfile = new CustomerProfile
                                                                                        {
                                                                                            Id = "d9450f37-4402-4f53-8f83-96d1216e6",
                                                                                            Preferences = new[]
                                                                                            {
                                                                                            new Preference { Active = "Y", PreferenceType = "true" },
                                                                                            new Preference { Active = "MarketingOptIn", PreferenceType = "true" },
                                                                                            new Preference { Active = "Y", PreferenceType = "PreferredPartnerFFP",PreferenceValue="DL"}
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }));
                                                          return customerProfileHandler.Object;
                                                      })
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task AddFCNumberToPNRCommandHandler_UpdatenewEntityScenario()
        {
            var handler = _serviceProvider.GetService<IAddFCNumberToPNRCommand>()!;
            var tableService = _serviceProvider.GetService<ITableStorageService<MemberDetailsToAddSSR>>()!;

            var entity = new MemberDetailsToAddSSR
            {
                PartitionKey = "TestFile",
                RowKey = Guid.NewGuid().ToString(),
                AmperityId = "d9450f37-4402-4f53-8f83-96d1216e6",
                MemberNumber = "1501489858",
                PNR = "C2MXXXJ",
                FirstName = "Jhon",
                LastName = "Smith",
                Pos = "xxx",
                OrderId = "C2MBXXX2MAR25"
            };

            await tableService.InsertOrMergeAsync(entity);

            var addFCNumberToPNRCommandRequest = new AddFCNumberToPNRCommand
            {
                FileName = "autostamp_06_05_2026",
                MemberDeatilsToAddSSR = new List<MemberDetailsToAddSSR>
                {
                    new MemberDetailsToAddSSR
                    {
                        PartitionKey = "autostamp_06_05_2026",
                        RowKey = "C2MXXJJ",
                        AmperityId = "d9450f37-4402-4f53-8f83-96d1216e6",
                        MemberNumber = "1501489858",
                        PNR = "C2MXXJJ",
                        FirstName = "Jhon",
                        LastName = "Smith",
                        Pos = "xxx",
                        OrderId = "C2MBXXX2MAR25"
                    }
                }
            };

            var result = await handler.Handle(addFCNumberToPNRCommandRequest, CancellationToken.None);

            var retrieve = await tableService.RetrieveItemsInPartitionAsync("autostamp_06_05_2026", default);

            Assert.NotNull(result);
            Assert.Equal("Success", result);
            Assert.True(retrieve.Count() == 1);
            Assert.DoesNotContain(entity, retrieve);
            Assert.Equal("1501489858", retrieve.First().MemberNumber);
            Assert.Equal("C2MXXJJ", retrieve.First().PNR);
            Assert.Equal("Jhon", retrieve.First().FirstName);
            Assert.Equal("Smith", retrieve.First().LastName);
            Assert.Equal("C2MBXXX2MAR25", retrieve.First().OrderId);
        }

        [Fact]
        public async Task AddFCNumberToPNRCommandHandler_ValidSuccessScenario()
        {
            // Arrange
            var tableService = _serviceProvider.GetRequiredService<ITableStorageService<MemberDetailsToAddSSR>>();

            var customerProfileMock = new Mock<ICustomerProfileQuery>();
            customerProfileMock
                .Setup(x => x.Handle(It.IsAny<CustomerProfileRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new Response<CustomerProfileResponse>
                {
                    Status = Status.OK,
                    IgnoreNullValues = false,
                    Value = new CustomerProfileResponse
                    {
                        CustomerId = "123",
                        CustomerProfile = new CustomerProfile
                        {
                            Id = "d9450f37-4402-4f53-8f83-96d1216e6",
                            Preferences = new[]
                            {
                        new Preference
                        {
                            Active = "Y",
                            PreferenceType = "PreferredPartnerFFP",
                            PreferenceValue = "VS"
                        }
                            }
                        }
                    }
                }));

            // Resolve all remaining constructor dependencies from existing provider
            var azureBlobService = _serviceProvider.GetRequiredService<IAzureBlobService>();
            var customerSearchDocumentServiceQueryHandler = _serviceProvider.GetRequiredService<CustomerSearchDocumentServiceQueryHandler>();
            var addFCNumberToPNRResponseQueryHandler = _serviceProvider.GetRequiredService<AddFCNumberToPNRResponseQueryHandler>();
            var eventStoreRequestInsertCommandHandler = _serviceProvider.GetRequiredService<EventStoreRequestInsertCommandHandler>();
            var eventStoreRequestDeleteCommandHandler = _serviceProvider.GetRequiredService<EventStoreRequestDeleteCommandHandler>();
            var serviceBusTopicMessageEventCommandHandler = _serviceProvider.GetRequiredService<ServiceBusTopicMessageEventCommandHandler>();
            var azureStorageCommandHandler = _serviceProvider.GetRequiredService<AzureStorageCommandHandler>();

            var handler = new AddFCNumberToPNRCommandHandler(
                azureBlobService,
                tableService,
                customerSearchDocumentServiceQueryHandler,
                customerProfileMock.Object,
                addFCNumberToPNRResponseQueryHandler,
                eventStoreRequestInsertCommandHandler,
                eventStoreRequestDeleteCommandHandler,
                serviceBusTopicMessageEventCommandHandler,
                azureStorageCommandHandler
            );

            var entity = new MemberDetailsToAddSSR
            {
                PartitionKey = "autostampTest",
                RowKey = "C2MBXXX2MAR25",
                AmperityId = "d9450f37-4402-4f53-8f83-96d1216e6",
                MemberNumber = "1501489858",
                PNR = "C2MXXXJ",
                FirstName = "Jhon",
                LastName = "Smith",
                Pos = "xxx",
                OrderId = "C2MBXXX2MAR25",
                Status = "Ingested"
            };

            await tableService.InsertOrMergeAsync(entity);

            var request = new AddFCNumberToPNRCommand
            {
                FileName = "autostampTest",
                MemberDeatilsToAddSSR = new List<MemberDetailsToAddSSR>
        {
            new MemberDetailsToAddSSR
            {
                AmperityId = "d9450f37-4402-4f53-8f83-96d1216e6",
                MemberNumber = "1501489858",
                PNR = "C2MXXXJ",
                FirstName = "Jhon",
                LastName = "Smith",
                Pos = "xxx",
                OrderId = "C2MBXXX2MAR25"
            }
        }
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            var retrieve = await tableService.RetrieveItemsInPartitionAsync("autostampTest", default);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Success", result);

            Assert.NotNull(retrieve);
            var item = retrieve.First();

            Assert.Equal("1501489858", item.MemberNumber);
            Assert.Equal("C2MXXXJ", item.PNR);
            Assert.Equal("Jhon", item.FirstName);
            Assert.Equal("Smith", item.LastName);
            Assert.Equal("C2MBXXX2MAR25", item.OrderId);            
        }
       
        [Fact]
        public async Task AddFCNumberToPNRCommandHandler_NonVSValidScenario()
        {   
            var handler = _serviceProvider.GetService<IAddFCNumberToPNRCommand>()!;
            var tableService = _serviceProvider.GetService<ITableStorageService<MemberDetailsToAddSSR>>()!;

            var addFCNumberToPNRCommandRequest = new AddFCNumberToPNRCommand
            {
                FileName = "NonVSTestFile",
                MemberDeatilsToAddSSR = new List<MemberDetailsToAddSSR>
                {
                    new MemberDetailsToAddSSR
                    {
                        PartitionKey = "NonVSTestFile",
                        RowKey = "C2MXXJJ",
                        AmperityId = "d9450f37-4402-4f53-8f83-96d1216e6",
                        MemberNumber = "1501489858",
                        PNR = "C2MXXJJ",
                        FirstName = "Jhon",
                        LastName = "Smith",
                        Pos = "xxx",
                        OrderId = "C2MBXXX2MAR25"
                    }
                }
            };

            var result = await handler.Handle(addFCNumberToPNRCommandRequest, CancellationToken.None);

            var retrieve = await tableService.RetrieveItemsInPartitionAsync("NonVSTestFile", default);

            Assert.NotNull(result);
            Assert.Equal("Success", result);
            Assert.True(retrieve.Count() == 1);
            Assert.Equal("1501489858", retrieve.First().MemberNumber);
            Assert.Equal("C2MXXJJ", retrieve.First().PNR);
            Assert.Equal("Jhon", retrieve.First().FirstName);
            Assert.Equal("Smith", retrieve.First().LastName);
            Assert.Equal("C2MBXXX2MAR25", retrieve.First().OrderId);
            Assert.Equal("Skipped", retrieve.First().Response);
        }
        
        [Fact]
        public async Task AddFCNumberToPNRCommandHandler_Request_Null_ThrowsExpectedException()
        {
            var handler = _serviceProvider.GetService<IAddFCNumberToPNRCommand>()!;
            var result = await handler.Handle(null, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Value cannot be null. (Parameter 'request')", result);
        }

        [Fact]
        public async Task AddFCNumberToPNRCommandHandler_Request_FilenameNull_ThrowsExpectedException()
        {
            var handler = _serviceProvider.GetService<IAddFCNumberToPNRCommand>()!;
            var addFCNumberToPNRCommandRequest = new AddFCNumberToPNRCommand
            {
                FileName = "",
                MemberDeatilsToAddSSR = new List<MemberDetailsToAddSSR>
                {
                    new MemberDetailsToAddSSR
                    {
                        AmperityId = "d9450f37-4402-4f53-8f83-96d1216e6",
                        MemberNumber = "1501489858",
                        PNR = "C2MBXJ",
                        FirstName = "LILACH",
                        LastName = "DAHAN",
                        Pos = "HDQ",
                        OrderId = "C2MBXXX2MAR25"
                    }
                }
            };

            var result = await handler.Handle(addFCNumberToPNRCommandRequest, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Value cannot be null. (Parameter 'FileName')", result);
        }
    }
}