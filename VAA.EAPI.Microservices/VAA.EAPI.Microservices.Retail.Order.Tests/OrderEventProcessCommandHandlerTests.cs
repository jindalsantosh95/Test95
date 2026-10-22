using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.MetaPartners;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderCreate;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order.Remarks;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.OrderCreate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Tests
{
    public class OrderEventProcessCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderEventProcessCommandHandler>> _logger;
        private readonly Mock<IServiceBusTopicMessageEventCommandHandler> _serviceBusTopicMessageEventCommandHandler;
        private readonly Mock<ITrackAnalyticsEventCommandHandler> _trackAnalyticsEventCommandHandler;
        private readonly OrderEventProcessCommandHandler _handler;

        public OrderEventProcessCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                                                      .AddSingleton<IServiceBusTopicMessageEventCommandHandler, ServiceBusTopicMessageEventCommandHandler>()
                                                      .AddSingleton<IMetaPartnerCommandHandler, MetaPartnerCommandHandler>()
                                                      .AddSingleton<IMetaPartnerEventCommandHandler, MetaPartnerEventCommandHandler>()
                                                      .BuildServiceProvider();

            var metaPartnerEventCommandHandler = _serviceProvider.GetService<IMetaPartnerEventCommandHandler>()!;

            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderEventProcessCommandHandler>>();
            _serviceBusTopicMessageEventCommandHandler = new Mock<IServiceBusTopicMessageEventCommandHandler>();
            _trackAnalyticsEventCommandHandler = new Mock<ITrackAnalyticsEventCommandHandler>();
            _handler = new OrderEventProcessCommandHandler(_mediator.Object, _logger.Object, metaPartnerEventCommandHandler, _serviceBusTopicMessageEventCommandHandler.Object, _trackAnalyticsEventCommandHandler.Object);

            OrderTestsCommon.CreateTestEnvironmentSettings();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public async Task HandlerTest_Success()
        {
            //Arrange            
            var request = TestDataProvider.GetTestData<string>("OrderMS\\OrderMsRequest");
            var response = TestDataProvider.GetTestData<OrderResponse>("OrderMS\\OrderMsResponse");

            _mediator.Setup(m => m.Send(It.IsAny<OrderCreateServiceCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<OrderResponse> { Value = response });
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);

            //Act
            var result = await _handler.Handle(new OrderEventProcessCommand { Id = "123", Data = request }, System.Threading.CancellationToken.None);

            //Assert
            Assert.Equal(true, result.IsSuccess);
        }

        [Fact]
        public async Task Handle_WhenOrderCreateServiceCommandThrows_ReturnsFailureWithoutThrowing()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<string>("OrderMS\\OrderMsRequest");

            _mediator.Setup(m => m.Send(It.IsAny<OrderCreateServiceCommand>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new InvalidOperationException("Simulated order create failure"));
            _trackAnalyticsEventCommandHandler.Setup(handler => handler.Handle(It.IsAny<TrackAnalyticsEventCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(new OrderEventProcessCommand { Id = "123", Data = request }, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Order event processing encountered an error.", result.Error);
            _trackAnalyticsEventCommandHandler.Verify(handler => handler.Handle(
                It.Is<TrackAnalyticsEventCommand>(command => command.Subject == Constants.Application.OrderCreateEventSubject),
                CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_FeatureFlagEnabled_MetaPartnerIdPresent_CallsMetaPartnerEventHandler()
        {
            // Arrange
            ConfigReader.Set(Constants.Environment.EnableMetaPartnerEvent, "true");

            var metaPartnerEventCommandHandler = _serviceProvider.GetService<IMetaPartnerEventCommandHandler>()!;
            var messageBusFactory = (MessageBusFactory)_serviceProvider.GetService<IMessageBusFactory>()!;
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), Constants.Application.MetaPartnerEventHandlerTopicName);

            var handler = new OrderEventProcessCommandHandler(_mediator.Object, _logger.Object, metaPartnerEventCommandHandler, _serviceBusTopicMessageEventCommandHandler.Object, _trackAnalyticsEventCommandHandler.Object);

            var orderCreateRequest = TestDataProvider.GetTestData<OrderCreateRequest>("OrderMS\\OrderMsRequest");
            orderCreateRequest.MetaPartner = new System.Collections.Generic.Dictionary<string, string> { { "MetaPartnerId", "SkyScanner" } };

            var response = TestDataProvider.GetTestData<OrderResponse>("OrderMS\\OrderMsResponse");

            _mediator.Setup(m => m.Send(It.IsAny<OrderCreateServiceCommand>(), CancellationToken.None)).ReturnsAsync(new Response<OrderResponse> { Value = response });
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), CancellationToken.None)).ReturnsAsync(true);

            // Act
            await handler.Handle(new OrderEventProcessCommand { Id = "123", Data = VAA.EAPI.Common.Serialisation.Json.Serialize(orderCreateRequest) }, CancellationToken.None);

            // Assert
            Assert.Equal(1, messageBus.Delivered);
        }

        [Theory]
        [InlineData(false, "SkyScanner")] // flag disabled, id present  -> NOT called
        [InlineData(true, null)]           // flag enabled, id null      -> NOT called
        [InlineData(true, "")]             // flag enabled, id empty     -> NOT called
        public async Task Handle_MetaPartnerEventHandler_NotCalled(bool featureFlagEnabled, string metaPartnerId)
        {
            // Arrange
            ConfigReader.Set(Constants.Environment.EnableMetaPartnerEvent, featureFlagEnabled.ToString());

            var metaPartnerEventCommandHandler = _serviceProvider.GetService<IMetaPartnerEventCommandHandler>()!;
            var messageBusFactory = (MessageBusFactory)_serviceProvider.GetService<IMessageBusFactory>()!;
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), Constants.Application.MetaPartnerEventHandlerTopicName);

            var handler = new OrderEventProcessCommandHandler(_mediator.Object, _logger.Object, metaPartnerEventCommandHandler, _serviceBusTopicMessageEventCommandHandler.Object, _trackAnalyticsEventCommandHandler.Object);

            var orderCreateRequest = TestDataProvider.GetTestData<OrderCreateRequest>("OrderMS\\OrderMsRequest");
            orderCreateRequest.MetaPartner = new System.Collections.Generic.Dictionary<string, string> { { "MetaPartnerId", metaPartnerId } };

            var response = TestDataProvider.GetTestData<OrderResponse>("OrderMS\\OrderMsResponse");

            _mediator.Setup(m => m.Send(It.IsAny<OrderCreateServiceCommand>(), CancellationToken.None)).ReturnsAsync(new Response<OrderResponse> { Value = response });
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), CancellationToken.None)).ReturnsAsync(true);

            // Act
            await handler.Handle(new OrderEventProcessCommand { Id = "123", Data = VAA.EAPI.Common.Serialisation.Json.Serialize(orderCreateRequest) }, CancellationToken.None);

            // Assert
            Assert.Equal(0, messageBus.Delivered);
        }

        [Fact]
        public async Task Handle_WhenVoucherApplied_PublishesVoucherRedemptionRemarkEvent()
        {
            // Arrange
            var orderCreateRequest = TestDataProvider.GetTestData<OrderCreateRequest>("OrderMS\\OrderMsRequest");
            orderCreateRequest.VoucherApplied = true;
            orderCreateRequest.VoucherDetails = new VAA.EAPI.Microservices.Retail.Order.Domain.Order.VoucherDetails
            {
                VoucherOwnerFcAccount = "FC123456",
                RedemptionType = "UPGRADE",
                TotalPointsUsed = 120000,
                AppliedVouchers = new[]
                {
                    new VoucherRedemption { VoucherId = "VC123", PointsUsed = 90000 }
                }
            };

            var response = TestDataProvider.GetTestData<OrderResponse>("OrderMS\\OrderMsResponse");
            ServiceBusTopicMessageEventCommand publishedCommand = null;

            _mediator.Setup(m => m.Send(It.IsAny<OrderCreateServiceCommand>(), CancellationToken.None)).ReturnsAsync(new Response<OrderResponse> { Value = response });
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), CancellationToken.None)).ReturnsAsync(true);
            _serviceBusTopicMessageEventCommandHandler.Setup(handler => handler.Handle(It.IsAny<ServiceBusTopicMessageEventCommand>(), CancellationToken.None))
                .Callback<ServiceBusTopicMessageEventCommand, CancellationToken>((command, _) => publishedCommand = command)
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _handler.Handle(new OrderEventProcessCommand { Id = "123", Data = VAA.EAPI.Common.Serialisation.Json.Serialize(orderCreateRequest) }, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(publishedCommand);
            Assert.Equal(Constants.Application.OrderChangeServiceBusTopicName, publishedCommand.ServiceBusTopicName);
            Assert.NotEqual("123", publishedCommand.Message.Id);

            var remarkRequest = Assert.IsType<AddSpecialRemarkRequest>(publishedCommand.Message.Data);
            Assert.Equal("FC123456", remarkRequest.VoucherDetails.LoyaltyNumber);
            Assert.Equal("UPGRADE", remarkRequest.VoucherDetails.RedemptionType);
            Assert.Equal(120000, remarkRequest.VoucherDetails.TotalPoints);
            Assert.Collection(remarkRequest.VoucherDetails.Vouchers,
                voucher =>
                {
                    Assert.Equal("VC123", voucher.VoucherNumber);
                    Assert.Equal(90000, voucher.VoucherPointsApplied);
                });
        }

    }
}
