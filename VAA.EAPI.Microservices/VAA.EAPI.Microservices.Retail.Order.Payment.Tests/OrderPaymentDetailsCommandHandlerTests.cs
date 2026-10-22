using Microsoft.Extensions.Logging;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;
using Xunit;
using MediatR;
using Moq;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4Payment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4TokenGenerator;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class OrderPaymentDetailsCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IHttpConnector _httpConnectorMock;
        private readonly Mock<IMediator> _mediator;
        private readonly ILogger<OrderPaymentDetailsCommandHandler> _logger;
        private readonly ILogger<Air4PaymentDetailsQueryHandler> _paymentDetailslogger;
        private readonly ILogger<Air4FormsOfPaymentsQueryHandler> _formOfPaymentlogger;

        private readonly IAir4TokenGenerator _tokenGenerator;
        private readonly ICachedChannel _cachedChannel;
        private readonly ITableStorageService<Channel> _tableStorageService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _cachedChannelMemoryCache;
        private readonly OrderPaymentDetailsCommandHandler _handler;
        private readonly Air4PaymentDetailsQueryHandler _paymentDetailshandler;
        private readonly Air4FormsOfPaymentsQueryHandler _formOfPaymenthandler;

        public OrderPaymentDetailsCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            PRIGenerationEnvironmentSetup.CreateTestEnvironmentSettings();
            _serviceProvider = new ServiceCollection().AddMemoryCache()
                                                   .BuildServiceProvider();


            _cachedChannelMemoryCache = _serviceProvider.GetService<IMemoryCache>();
          

            var httplogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<OrderPaymentDetailsCommandHandler>.Instance;
            _paymentDetailslogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<Air4PaymentDetailsQueryHandler>.Instance;
            _formOfPaymentlogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<Air4FormsOfPaymentsQueryHandler>.Instance;
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<OrderPaymentDetailsCommandHandler>.Instance;

            _mediator = new Mock<IMediator>();
            _tableStorageService = new VAA.EAPI.Core.Xunit.Azure.TableStorageService<Channel>();
            _tableStorageService.InsertOrMergeAsync(new()
            {
                RowKey = "Mobile",
                ChannelId = "Mobile",
                ChannelName = "channelName",
                DlAgentId = "dlAgent",
                DlApplicationId = "channelName",
                DlPaymentobjectName = "dlPaymentObjectName",
                DlApplicationName = "dlApplicationName",
                DlPaymentPurchasePath = "dlPaymentPurchasePath",
                DlPoolType = "dlPoolType",
                DlRoleIndicator = ""
            });
            _httpConnectorMock = new HttpConnector(httplogger, httpClientFactoryFixture.Current);
            _tokenGenerator = new Air4TokenGenerator(_httpConnectorMock);
            _cachedChannel = new CachedChannel(_tableStorageService, _cachedChannelMemoryCache);

            _handler = new OrderPaymentDetailsCommandHandler(
                _mediator.Object,
                _logger
            );

            _paymentDetailshandler = new Air4PaymentDetailsQueryHandler(_paymentDetailslogger, _httpConnectorMock, _tokenGenerator, _cachedChannel);

            _formOfPaymenthandler = new Air4FormsOfPaymentsQueryHandler(_formOfPaymentlogger, _httpConnectorMock, _tokenGenerator,_cachedChannel);

            // Set a mock correlation ID in AsyncContext
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public async Task Handler_GetPaymentDetailsCardWithChannel_ExecuteSuccessfully()
        {
            //Arrange
            var channel = "Mobile";
            var paymentReferenceId = "C5D05CBF6BC84CA3A4F6FB7567932008";

            var air4PaymentDetailsResponse =await _paymentDetailshandler.Handle(new() { Channel = channel, PaymentReferenceId = paymentReferenceId }, CancellationToken.None);

             _mediator.Setup(x => x.Send(It.IsAny<Air4PaymentDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(air4PaymentDetailsResponse);

            //Arrange & Act
            var result = await _handler.Handle(new() { Channel = channel, PaymentReferenceId = paymentReferenceId, PaymentType = Domain.Enums.PaymentType.Card }, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);            
        }

        [Fact]
        public async Task Handler_GetPaymentDetailsCardWithIncorrectChannel_ReturnsSuccessOnDefaultValues()
        {
            //Arrange
            var channel = "NoChannel";
            var paymentReferenceId = "C5D05CBF6BC84CA3A4F6FB7567932008";

            var air4PaymentDetailsResponse = await _paymentDetailshandler.Handle(new() { Channel = channel, PaymentReferenceId = paymentReferenceId }, CancellationToken.None);

            _mediator.Setup(x => x.Send(It.IsAny<Air4PaymentDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(air4PaymentDetailsResponse);

            //Arrange & Act
            var result = await _handler.Handle(new() { Channel = channel, PaymentReferenceId = paymentReferenceId, PaymentType = Domain.Enums.PaymentType.Card }, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_GetPaymentDetailsCard_ExecuteSuccessfully()
        {
            //Arrange           
            var paymentReferenceId = "C5D05CBF6BC84CA3A4F6FB7567932008";

            var air4PaymentDetailsResponse = await _paymentDetailshandler.Handle(new() { PaymentReferenceId = paymentReferenceId }, CancellationToken.None);

            _mediator.Setup(x => x.Send(It.IsAny<Air4PaymentDetailsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(air4PaymentDetailsResponse);

            //Arrange & Act
            var result = await _handler.Handle(new() { PaymentReferenceId = paymentReferenceId, PaymentType = Domain.Enums.PaymentType.Card }, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_GetPaymentDetailsAPM_ExecuteSuccessfully()
        {
            //Arrange
            var paymentReferenceId = "A7F9036892164490B90DCD4D36A9469E";

            var air4FormsOfPaymentsResponse = await _formOfPaymenthandler.Handle(new() { PaymentReferenceId = paymentReferenceId }, CancellationToken.None);

            _mediator.Setup(x => x.Send(It.IsAny<Air4FormsOfPaymentsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(air4FormsOfPaymentsResponse);

            //Arrange & Act
            var result = await _handler.Handle(new() { PaymentReferenceId = paymentReferenceId, PaymentType = Domain.Enums.PaymentType.APM }, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
        }
    }
}
