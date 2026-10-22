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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers.UpdatePhoneNumber;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.AzureStorage;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class UpdatePhoneNumberNotificationHandlerTest : IClassFixture<HttpClientFactoryFixture>
    {
        private const string GroupId =
            "213979b1-744e-4365-9256-12049a9f0696";

        private const string HeadOfGroupCustomerId =
            "355f45f8-1c19-41db-9057-4a9a0585012d";

        private const string HoHManagedFalseCustomerId =
            "2c157c4b-22b5-4488-9e94-520c1038a45d";

        private const string HoHManagedTrueCustomerId =
            "3d268d5c-33c6-5599-af05-630d2149b56e";

        private const string ServiceBusConnectionString =
            "testConnectionString";

        private const string ServiceBusSource =
            "testSource";

        private readonly IServiceProvider _serviceProvider;

        public UpdatePhoneNumberNotificationHandlerTest(
            HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigureSettings();

            _serviceProvider = BuildServiceProvider(
                httpClientFactoryFixture);
        }

        [Fact]
        public async Task Handle_Should_Throw_ArgumentNullException_When_Request_Is_Null()
        {
            // Arrange
            var handler = GetHandler();

            // Act
            var action = () => handler.Handle(
                null!,
                CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task Handle_Should_Update_PhoneNumber_When_HoHManaged_Is_False()
        {
            // Arrange
            var handler = GetHandler();

            var request = CreateValidRequest(
                HoHManagedFalseCustomerId);

            // Act
            var result = await handler.Handle(
                request,
                CancellationToken.None);

            // Assert
            AssertSuccessfulResponse(result);
        }

        [Fact]
        public async Task Handle_Should_Update_PhoneNumber_When_HoHManaged_Is_True()
        {
            // Arrange
            var handler = GetHandler();

            var request = CreateValidRequest(
                HoHManagedTrueCustomerId);

            // Act
            var result = await handler.Handle(
                request,
                CancellationToken.None);

            // Assert
            AssertSuccessfulResponse(result);
        }

        [Theory]
        [InlineData(HoHManagedFalseCustomerId)]
        [InlineData(HoHManagedTrueCustomerId)]
        public async Task Handle_Should_Return_OK_Response_For_Valid_Request(
            string customerId)
        {
            // Arrange
            var handler = GetHandler();

            var request = CreateValidRequest(customerId);

            // Act
            var result = await handler.Handle(
                request,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.Equal(Status.OK, result.Value.Status);
        }

        [Theory]
        [InlineData(HoHManagedFalseCustomerId)]
        [InlineData(HoHManagedTrueCustomerId)]
        public async Task Handle_Should_Return_NonEmpty_Response_Value(
            string customerId)
        {
            // Arrange
            var handler = GetHandler();

            var request = CreateValidRequest(customerId);

            // Act
            var result = await handler.Handle(
                request,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.False(
                string.IsNullOrWhiteSpace(result.Value.Value));
        }

        private IUpdatePhoneNumberCommand GetHandler()
        {
            return _serviceProvider
                .GetRequiredService<IUpdatePhoneNumberCommand>();
        }

        private static IServiceProvider BuildServiceProvider(
            HttpClientFactoryFixture fixture)
        {
            var messageBusFactory = new MessageBusFactory();

            var messageBus = messageBusFactory.GetClient(
                ServiceBusConnectionString,
                ServiceBusSource);

            return new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(fixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton(
                    typeof(ITableStorageService<>),
                    typeof(Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<IMessageBus>(messageBus)
                .AddSingleton<
                    IRequestHandler<EventRequestInsertCommand, bool>,
                    EventStoreRequestInsertCommandHandler>()
                .AddSingleton<
                    IRequestHandler<EventStoreRequestDeleteCommand, bool>,
                    EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<
                    IAzureServiceBusTopicClient,
                    AzureServiceBusTopicClient>()
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton(CreateGroupDocumentServiceQueryHandler)
                .AddSingleton<
                    IUpdatePhoneNumberCommand,
                    UpdatePhoneNumberNotificationHandler>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton(CreateAzureStorageCommandHandler)
                .BuildServiceProvider();
        }

        private static GroupDocumentServiceQueryHandler
            CreateGroupDocumentServiceQueryHandler(
                IServiceProvider serviceProvider)
        {
            var httpConnector =
                serviceProvider.GetRequiredService<IHttpConnector>();

            var logger = serviceProvider.GetRequiredService<
                ILogger<GroupDocumentServiceQueryHandler>>();

            var handlerMock =
                new Mock<GroupDocumentServiceQueryHandler>(
                    httpConnector,
                    logger);

            handlerMock
                .Setup(handler => handler.Handle(
                    It.IsAny<GroupDocumentServiceQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    Result.Success(
                        CreateGroupDocumentResponse()));

            return handlerMock.Object;
        }

        private static AzureStorageCommandHandler
            CreateAzureStorageCommandHandler(
                IServiceProvider serviceProvider)
        {
            var handlerMock =
                new Mock<AzureStorageCommandHandler>();

            handlerMock
                .Setup(handler => handler.Handle(
                    It.IsAny<AzureStorageCommand>(),
                    It.IsAny<CancellationToken>()));

            return handlerMock.Object;
        }

        private static Response<HouseHoldGroup>
            CreateGroupDocumentResponse()
        {
            return new Response<HouseHoldGroup>
            {
                Status = Status.OK,
                IgnoreNullValues = false,
                Value = CreateHouseHoldGroup()
            };
        }

        private static HouseHoldGroup CreateHouseHoldGroup()
        {
            var currentDate = DateTime.UtcNow;

            return new HouseHoldGroup
            {
                GroupId = GroupId,
                Status = "Active",
                GroupTypeCode = "Household",
                HeadOfGroupCustomerId = HeadOfGroupCustomerId,
                GroupHistory = new List<GroupHistory>
                {
                    CreateGroupHistory(
                        historyId:
                            "13489c1d-6019-4a3b-8b9f-1eec57ddddd8",
                        customerId:
                            HoHManagedFalseCustomerId,
                        hoHManaged: false,
                        createdOn: currentDate),

                    CreateGroupHistory(
                        historyId:
                            "24567c2e-7120-5b4c-9ca0-2ffd68eeee99",
                        customerId:
                            HoHManagedTrueCustomerId,
                        hoHManaged: true,
                        createdOn: currentDate.AddDays(-10))
                }
            };
        }

        private static GroupHistory CreateGroupHistory(
            string historyId,
            string customerId,
            bool hoHManaged,
            DateTime createdOn)
        {
            return new GroupHistory
            {
                HistoryId = historyId,
                CreatedOn = createdOn,
                CreatedBy = "test.user@virgin.com",
                LastUpdatedOn = createdOn,
                LastUpdatedBy = "Customer API",
                StartDate = createdOn,

                // Both records must be active so the handler can
                // exercise both HoHManaged branches.
                ExitDate = null,

                SourceIdentifier = string.Empty,
                SourceSystem = string.Empty,
                HoHManaged = hoHManaged,
                CustomerId = customerId
            };
        }

        private static UpdatePhoneNumberNotificationEventRequest
            CreateValidRequest(
                string customerId)
        {
            return new UpdatePhoneNumberNotificationEventRequest
            {
                HOHCustomerId = customerId,
                GroupID = GroupId,
                FcNumber = "1502743865",
                InitiatedBy = "Customer API",
                Phone = CreatePhone()
            };
        }

        private static Phone CreatePhone()
        {
            return new Phone
            {
                PhoneType = "Mobile",
                Usage = "Business",
                CountryCode = "44",
                PhoneNumber = "9106543133",
                PreferredFlag = "Y",
                Description = "Retest Phone"
            };
        }

        private static void AssertSuccessfulResponse(
            Result<Response<string>> result)
        {
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.False(
                string.IsNullOrWhiteSpace(result.Value.Value));
        }

        private static void ConfigureSettings()
        {
            ConfigReader.Set(
                "microservicedbkey",
                "xxx");

            ConfigReader.Set(
                "microserviceeventstoretablename",
                "test");

            ConfigReader.Set(
                "loyaltyMbrMgmt:AppId",
                "123");

            ConfigReader.Set(
                "loyaltyMbrMgmt:ClientSecret",
                "DUMMY");

            ConfigReader.Set(
                "customerTxnMS:ResourceId",
                "dummy123");

            ConfigReader.Set(
                "loyaltyMbrMgmt:OcpSubID",
                "123");

            ConfigReader.Set(
                "azureADEndpoint",
                "https://dummy.com/123/");

            ConfigReader.Set(
                "customermsurl",
                "https://dummy.com/123/");

            ConfigReader.Set(
                "CustomerTransactionTopicName",
                "customertransactioneventmessages");

            ConfigReader.Set(
                "customer:appid",
                "1234");

            ConfigReader.Set(
                "servicebusconnectionstring",
                ServiceBusConnectionString);
        }
    }
}