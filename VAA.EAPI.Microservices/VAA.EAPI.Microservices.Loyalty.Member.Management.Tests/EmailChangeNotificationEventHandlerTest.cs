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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers.EmailUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.EmailUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class EmailChangeNotificationEventHandlerTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        public EmailChangeNotificationEventHandlerTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("microservicedbkey", "xxx");
            ConfigReader.Set("microserviceeventstoretablename", "test");
            ConfigReader.Set("blobconnstringforbulkupload", "xxx");
            ConfigReader.Set("loyaltyMbrMgmt:AppId", "123");
            ConfigReader.Set("loyaltyMbrMgmt:ClientSecret", "DUMMY");
            ConfigReader.Set("customerTxnMS:ResourceId", "dummy123");
            ConfigReader.Set("loyaltyMbrMgmt:OcpSubID", "123");
            ConfigReader.Set("azureADEndpoint", "https://dummy.com/123/");
            ConfigReader.Set("customermsurl", "https://dummy.com/123/");
            ConfigReader.Set("CustomerTransactionTopicName", "customertransactioneventmessages");
            ConfigReader.Set("microservicedbkey", "xxx");
            ConfigReader.Set("DummyEmailDomain", "U18.virgin.com");
            ConfigReader.Set("customer:appid", "1234");
            ConfigReader.Set("servicebusconnectionstring", "testConnectionString");

            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
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
                                                                               CustomerId = "355f45f8-1c19-41db-9057-4a9a0585012d",
                                                                               FirstName = "John",
                                                                               LastName = "Smith",
                                                                               LoyaltyMemberNumber = "1501489817",
                                                                               EmailAddress = "xyuz@yyy.com"
                                                                           }
                                                                       })));

                                                          return customhandler.Object;
                                                      })
                                                      .AddSingleton<ICustomerProvider>(sp =>
                                                      {
                                                          var customerProvider =
                                                              new Mock<ICustomerProvider>();

                                                          customerProvider
                                                              .Setup(x => x.GetCustomerSearchDocument(
                                                                  It.IsAny<string>()))
                                                              .ReturnsAsync(
                                                                  new CustomerSearch
                                                                  {
                                                                      CustomerId = "355f45f8-1c19-41db-9057-4a9a0585012d",
                                                                      FirstName = "John",
                                                                      LastName = "Smith",
                                                                      LoyaltyMemberNumber = "1501489817",
                                                                      EmailAddress = "xyuz@yyy.com"
                                                                  });

                                                          return customerProvider.Object;
                                                      })
                                                      .AddSingleton<FlyingClubMemberDocumentServiceQueryHandler>((sp) =>
                                                      {
                                                          var httpConnector = sp.GetService<IHttpConnector>()!;
                                                          var logger = sp.GetService<ILogger<FlyingClubMemberDocumentServiceQueryHandler>>()!;

                                                          var flyingClubMemberhandler = new Mock<FlyingClubMemberDocumentServiceQueryHandler>(httpConnector, logger);

                                                          flyingClubMemberhandler.Setup(Object => Object.Handle(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>()))
                                                                       .ReturnsAsync((Result.Success(new Response<FlyingClubMember>
                                                                       {
                                                                           Status = Status.OK,
                                                                           IgnoreNullValues = false,
                                                                           Value = new FlyingClubMember()
                                                                           {
                                                                               CustomerId = "2c157c4b-22b5-4488-9e94-520c1038a45d",
                                                                               GroupId = "213979b1-744e-4365-9256-12049a9f0696",
                                                                               HeadOfGroupMemberNumber = "355f45f8-1c19-41db-9057-4a9a0585012d",
                                                                               Status = "Active",
                                                                               MemberNumber = "1501489817",
                                                                               MemberType = "Household"
                                                                           }
                                                                       }
                                                                       )));

                                                          return flyingClubMemberhandler.Object;

                                                      })
                                                      .AddSingleton<GroupDocumentServiceQueryHandler>((sp) =>
                                                      {
                                                          var httpConnector = sp.GetService<IHttpConnector>()!;
                                                          var logger = sp.GetService<ILogger<GroupDocumentServiceQueryHandler>>()!;

                                                          var groupDocHandler = new Mock<GroupDocumentServiceQueryHandler>(httpConnector, logger);
                                                          groupDocHandler.Setup(Object => Object.Handle(It.IsAny<GroupDocumentServiceQuery>(), It.IsAny<CancellationToken>()))
                                                                      .ReturnsAsync((Result.Success(new Response<HouseHoldGroup>
                                                                      {
                                                                          Status = Status.OK,
                                                                          IgnoreNullValues = false,
                                                                          Value = new HouseHoldGroup()
                                                                          {
                                                                              GroupId = "213979b1-744e-4365-9256-12049a9f0696",
                                                                              Status = "Active",
                                                                              GroupTypeCode = "Household",
                                                                              HeadOfGroupCustomerId = "355f45f8-1c19-41db-9057-4a9a0585012d",
                                                                              GroupHistory = new List<GroupHistory>
                                                                              {
                                                                                new GroupHistory
                                                                                {
                                                                                    HistoryId = "13489c1d-6019-4a3b-8b9f-1eec57ddddd8",
                                                                                    CreatedOn = DateTime.UtcNow,
                                                                                    CreatedBy = "aby.lyui@fly.gmail.com",
                                                                                    LastUpdatedOn = DateTime.UtcNow,
                                                                                    LastUpdatedBy = "Customer API",
                                                                                    StartDate = DateTime.UtcNow,
                                                                                    ExitDate = null,
                                                                                    SourceIdentifier = string.Empty,
                                                                                    SourceSystem = string.Empty,
                                                                                    HoHManaged = false,
                                                                                    CustomerId = "2c157c4b-22b5-4488-9e94-520c1038a45d"
                                                                                },
                                                                                new GroupHistory
                                                                                {
                                                                                    HistoryId = "24567c2e-7120-5b4c-9ca0-2ffd68eeee99",
                                                                                    CreatedOn = DateTime.UtcNow.AddDays(-10),
                                                                                    CreatedBy = "test.user@virgin.com",
                                                                                    LastUpdatedOn = DateTime.UtcNow.AddDays(-5),
                                                                                    LastUpdatedBy = "Customer API",
                                                                                    StartDate = DateTime.UtcNow.AddDays(-10),
                                                                                    ExitDate = DateTime.UtcNow.AddDays(-1),
                                                                                    SourceIdentifier = string.Empty,
                                                                                    SourceSystem = string.Empty,
                                                                                    HoHManaged = true,
                                                                                    CustomerId = "3d268d5c-33c6-5599-af05-630d2149b56e"
                                                                                }
                                                                              }

                                                                          }

                                                                      }
                                                                      )));

                                                          return groupDocHandler.Object;

                                                      })
                                                      .AddSingleton<IEmailChangeNotificationCommand, EmailChangeNotificationEventHandler>()
                                                      .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                                                      .AddSingleton<AzureStorageCommandHandler>((sp) =>
                                                      {
                                                          var azureStorageCommandHandler = new Mock<AzureStorageCommandHandler>();

                                                          azureStorageCommandHandler.Setup(x => x.Handle(It.IsAny<AzureStorageCommand>(), It.IsAny<CancellationToken>()));

                                                          return azureStorageCommandHandler.Object;
                                                      })
                                                      .BuildServiceProvider();
        }
        [Fact]
        public async Task Handle_Should_Return_Failure_When_Request_Is_Null()
        {
            var handler = _serviceProvider.GetService<IEmailChangeNotificationCommand>()!;

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                handler.Handle(null, CancellationToken.None));
        }
        [Fact]
        public async Task Handle_Should_Update_HoHManaged_To_False_For_Non_Dummy_Email()
        {
            var _handler = _serviceProvider.GetService<IEmailChangeNotificationCommand>()!;

            var request = GetValidRequest();

            request.NewEmailAddress =
                "test@gmail.com";

            var result =
                await _handler.Handle(
                    request,
                    CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("Accepted", result.Value.Status.ToString());
            Assert.NotNull(result.Value);
            Assert.False(string.IsNullOrWhiteSpace(result.Value.Value));
            Assert.NotNull(result.Value);
            Assert.False(string.IsNullOrWhiteSpace(result.Value.Value));
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
        }
        [Fact]
        public async Task Handle_Should_Update_HoHManaged_To_True_For_Dummy_Email()
        {
            var _handler = _serviceProvider.GetService<IEmailChangeNotificationCommand>()!;
            var _serviceBusHandler = _serviceProvider.GetService<ServiceBusTopicMessageEventCommand>();
            var request = GetValidRequest();

            request.NewEmailAddress =
                "test@U18.virgin.com";

            var result =
                await _handler.Handle(
                    request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("Accepted", result.Value.Status.ToString());
            Assert.NotNull(result.Value);
            Assert.False(string.IsNullOrWhiteSpace(result.Value.Value));
            Assert.NotNull(result.Value);
            Assert.False(string.IsNullOrWhiteSpace(result.Value.Value));
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
        }
        private static EmailChangeEvent GetValidRequest() =>
        new()
        {
            CustomerId = "2c157c4b-22b5-4488-9e94-520c1038a45d",
            MemberId = "1501489817",
            OldEmailAddress = "old@test.com",
            NewEmailAddress = "new@test.com",
            InitiatedBy = "Customer API"
        };
    }
}
