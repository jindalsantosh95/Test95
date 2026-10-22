using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Domain.Events;
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
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.HouseHoldLink;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class HouseHoldAccountLinkingCommandHandlerTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RecordingMediator _mediator;
        private readonly VAA.EAPI.Core.Xunit.ServiceBus.MessageBusFactory _messageBusFactory;

        public HouseHoldAccountLinkingCommandHandlerTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("containerNameForBulkUploadInput", "testcontainer");
            ConfigReader.Set("containernameForBulkUploadOutput", "testcontainerResponse");
            ConfigReader.Set("archiveContainerNameForBulkUpload", "testcontainerArchive");
            ConfigReader.Set("blobStorageUri", "https://tempuri.org/");
            ConfigReader.Set("containerName", "container");
            ConfigReader.Set("microservicedbkey", "xxx");
            ConfigReader.Set("microserviceeventstoretablename", "test");
            ConfigReader.Set("loyaltyMbrMgmt:AppId", "123");
            ConfigReader.Set("loyaltyMbrMgmt:ClientSecret", "DUMMY");
            ConfigReader.Set("customerTxnMS:ResourceId", "dummy123");
            ConfigReader.Set("loyaltyMbrMgmt:OcpSubID", "123");
            ConfigReader.Set("azureADEndpoint", "https://dummy.com/123/");
            ConfigReader.Set("servicebusconnectionstring", "testConnectionString");
            ConfigReader.Set("HoHLinkHeadRedEmalTemplate", "FCGOL-01");
            ConfigReader.Set("YoungFlyerEnrolmentConfirmationEmailTemplate", "vaaLoyaltyU18Parent");
            ConfigReader.Set("HoHLinkChildRedEmalTemplate", "FCGOL-01a");
            ConfigReader.Set("EmailTriggerTopicName", "enrolcustomer");
            ConfigReader.Set("CustomerTransactionTopicName", "customertransactioneventmessages");
            ConfigReader.Set("DummyEmailDomain","U18.virgin.com");

            _mediator = new RecordingMediator();
            _messageBusFactory = new VAA.EAPI.Core.Xunit.ServiceBus.MessageBusFactory();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IValidator<HouseHoldAccountLinkingCommand>, HouseHoldAccountLinkingCommandValidator>()
                                                      .AddSingleton<IMediator>(_mediator)
                                                      .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                                                      .AddSingleton<IMessageBusFactory>(_messageBusFactory)
                                                      .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                                                      .AddSingleton<IHouseHoldAccountLinkingCommand, HouseHoldAccountLinkingCommandHandler>()
                                                      .AddSingleton<FlyingClubMemberDocumentServiceQueryHandler>(sp =>
                                                          new StubFlyingClubMemberDocumentServiceQueryHandler(
                                                              sp.GetRequiredService<IHttpConnector>(),
                                                              sp.GetRequiredService<ILogger<FlyingClubMemberDocumentServiceQueryHandler>>()))
                                                      .AddSingleton<HouseHoldAccountEmailTriggerCommandHandler>()
                                                      .AddSingleton<IRequestHandler<EventRequestInsertCommand, bool>, EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<IRequestHandler<EventStoreRequestDeleteCommand, bool>, EventStoreRequestDeleteCommandHandler>()
                                                      .AddSingleton<EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                                                      .AddSingleton<AzureStorageCommandHandler, StubAzureStorageCommandHandler>()
                                                      .AddSingleton<IAzureServiceBusTopicClient, AzureServiceBusTopicClient>()
                                                      .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task HandlerShouldFailWithSameCustomerId()
        {
            // Arrange
            var handler = _serviceProvider.GetRequiredService<IHouseHoldAccountLinkingCommand>();

            var request = CreateRequest(
                headCustomerId: "8a78a494-5125-4ca7-bc88-5819a413a0fd",
                childCustomerId: "8a78a494-5125-4ca7-bc88-5819a413a0fd");

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task HandlerShouldFailWithValidationFailure()
        {
            // Arrange
            var handler = _serviceProvider.GetRequiredService<IHouseHoldAccountLinkingCommand>();

            var request = CreateRequest(
                initiatedBy: string.Empty,
                headCustomerId: "8a78a494-5125-4ca7-bc88-5819a413a0fd",
                childCustomerId: string.Empty);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task HandlerShouldReturnUpdateSuccess()
        {
            // Arrange
            var handler = _serviceProvider.GetRequiredService<IHouseHoldAccountLinkingCommand>();

            var request = CreateRequest(
                headCustomerId: "8a78a494-5125-4ca7-bc88-5819a413a0fd",
                childCustomerId: "eb82f9c8-4785-466d-bdb2-60cd8f668871");
            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value);

            Assert.Equal(
                "eb82f9c8-4785-466d-bdb2-60cd8f668871",
                request.ChildAccount.CustomerId);

            Assert.Equal(
                "8a78a494-5125-4ca7-bc88-5819a413a0fd",
                request.HeadAccount.CustomerId);

        }

        [Fact]
        public async Task HandlerShouldReturnCreateSuccess()
        {
            // Arrange
            var handler = _serviceProvider.GetRequiredService<IHouseHoldAccountLinkingCommand>();

            var request = CreateRequest(
                headCustomerId: "8a78a494-5125-4ca7-bc88-5819a413a0fd",
                childCustomerId: "eb82f9c8-4785-466d-bdb2-60cd8f668871");
            var publishedMessages = new System.Collections.Concurrent.ConcurrentBag<CloudEvent>();
            var messageBus = (VAA.EAPI.Core.Xunit.ServiceBus.MessageBus)_messageBusFactory.GetClient("testConnectionString", "householdaccount");
            messageBus.OnMessage += publishedMessages.Add;
            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.True(request.HoHManaged);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.NotNull(result.Value.Value);

            var emailEvent = Assert.Single(publishedMessages, message => message.Source == "householdaccount");
            Assert.Equal("InviteEmailEvent", emailEvent.Type);
            var emailRequest = Assert.IsType<YoungFlyerParentEmailCommand>(emailEvent.Data);
            Assert.Equal("vaaLoyaltyU18Parent", emailRequest.EmailTemplate);
            Assert.Equal(request.HeadAccount.EmailId, emailRequest.Email);
            Assert.Equal(request.ChildAccount.FirstName, emailRequest.AdobeInviteMemberRequest.FirstName);
            Assert.Equal(request.ChildAccount.LastName, emailRequest.AdobeInviteMemberRequest.LastName);
            Assert.Equal(request.ChildAccount.MemberId, emailRequest.AdobeInviteMemberRequest.MembershipNumber);
            Assert.Equal(request.HeadAccount.FirstName, emailRequest.AdobeInviteMemberRequest.FirstNameOfHouseholdLead);
            Assert.Equal(request.ChildAccount.FirstName, emailRequest.AdobeInviteMemberRequest.FirstNameOfHouseholdMember);

            Assert.Equal(
                "eb82f9c8-4785-466d-bdb2-60cd8f668871",
                request.ChildAccount.CustomerId);

            Assert.Equal(
                "8a78a494-5125-4ca7-bc88-5819a413a0fd",
                request.HeadAccount.CustomerId);

        }

        [Fact]
        public async Task HandlerShouldReturnUpdateSuccess_WhenChildHasValidEmail()
        {
            // Arrange
            var handler = _serviceProvider.GetRequiredService<IHouseHoldAccountLinkingCommand>();

            var request = CreateRequest(
                headCustomerId: "8a78a494-5125-4ca7-bc88-5819a413a0fd",
                childCustomerId: "eb82f9c8-4785-466d-bdb2-60cd8f668871");

            request.ChildAccount.EmailId = "john.smith@virgin.com";

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.False(request.HoHManaged);
        }

        [Fact]
        public async Task HandlerShouldReturnEmailTriggerError_WhenEmailTriggerThrows()
        {
            var handler = _serviceProvider.GetRequiredService<IHouseHoldAccountLinkingCommand>();
            var request = CreateRequest();
            request.ChildAccount.EmailId = "john.smith@virgin.com";
            ConfigReader.Set("ServiceBusConnectionString", string.Empty);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Contains(Constants.ErrorCodes.HoHEmailTriggerError, result.Error);
            ConfigReader.Set("ServiceBusConnectionString", "testConnectionString");
        }

        [Theory]
        [InlineData("@U18.virgin.com")]
        [InlineData("child@")]
        public async Task HandlerShouldRejectMalformedChildEmail(string email)
        {
            var handler = _serviceProvider.GetRequiredService<IHouseHoldAccountLinkingCommand>();
            var request = CreateRequest();
            request.ChildAccount.EmailId = email;

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(request.HoHManaged);
        }

        [Fact]
        public async Task HandlerShouldNotSetHoHManaged_WhenChildEmailHasWhitespace()
        {
            var handler = _serviceProvider.GetRequiredService<IHouseHoldAccountLinkingCommand>();
            var request = CreateRequest();
            request.ChildAccount.EmailId = "   @U18.virgin.com   ";

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(request.HoHManaged);
        }


        private static HouseHoldAccountLinkingCommand CreateRequest(
            string initiatedBy = "xyz",
            string headCustomerId = "8a78a494-5125-4ca7-bc88-5819a413a0fd",
            string childCustomerId = "eb82f9c8-4785-466d-bdb2-60cd8f668871")
        {
            return new HouseHoldAccountLinkingCommand
            {
                InitiatedBy = initiatedBy,
                HeadAccount = new Domain.HoHAccount.HouseHoldAccount
                {
                    CustomerId = headCustomerId,
                    MemberId = "1502525981",
                    EmailId = "xyz@fly.virgin.com",
                    FirstName = "first",
                    LastName = "last"
                },
                ChildAccount = new Domain.HoHAccount.HouseHoldAccount
                {
                    CustomerId = childCustomerId,
                    MemberId = "1502525999",
                    EmailId = "d3f4e8a91234456789abcdef12345678@U18.virgin.com",
                    FirstName = "child",
                    LastName = "last"
                }
            };
        }

        private sealed class StubFlyingClubMemberDocumentServiceQueryHandler(
            IHttpConnector httpConnector,
            ILogger<FlyingClubMemberDocumentServiceQueryHandler> logger)
            : FlyingClubMemberDocumentServiceQueryHandler(httpConnector, logger)
        {
            public override Task<Result<Response<FlyingClubMember>>> Handle(
                FlyingClubMemberDocumentServiceQuery request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(Result.Success(new Response<FlyingClubMember>
                {
                    Status = Status.OK,
                    IgnoreNullValues = false,
                    Value = new FlyingClubMember
                    {
                        GroupId = "234489121",
                        CreatedBy = "John",
                        CustomerId = "eb82f9c8-4785-466d-bdb2-60cd8f668871",
                        MemberNumber = "1501489817",
                        HeadOfGroupMemberNumber = "1501489818",
                        Status = "Active",
                        TierCode = "Red"
                    }
                }));
            }
        }

        private sealed class StubAzureStorageCommandHandler : AzureStorageCommandHandler
        {
            public override Task<Unit> Handle(AzureStorageCommand request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Unit.Value);
            }
        }

        private sealed class RecordingMediator : IMediator
        {
            public List<object> PublishedNotifications { get; } = [];
            public bool ThrowOnBooleanSend { get; set; }

            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
                where TNotification : INotification
            {
                PublishedNotifications.Add(notification);
                return Task.CompletedTask;
            }

            public Task Publish(object notification, CancellationToken cancellationToken = default)
            {
                PublishedNotifications.Add(notification);
                return Task.CompletedTask;
            }

            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            {
                if (typeof(TResponse) == typeof(bool))
                {
                    if (ThrowOnBooleanSend)
                    {
                        throw new InvalidOperationException("Simulated email trigger failure.");
                    }

                    return Task.FromResult((TResponse)(object)true);
                }

                throw new NotSupportedException("This test mediator only records published notifications and event-store acknowledgements.");
            }

            public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
                where TRequest : IRequest =>
                throw new NotSupportedException("This test mediator only records published notifications.");

            public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
                throw new NotSupportedException("This test mediator only records published notifications.");

            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
                Empty<TResponse>();

            public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
                Empty<object?>();

            private static async IAsyncEnumerable<T> Empty<T>()
            {
                await Task.CompletedTask;
                yield break;
            }
        }

    }
}
