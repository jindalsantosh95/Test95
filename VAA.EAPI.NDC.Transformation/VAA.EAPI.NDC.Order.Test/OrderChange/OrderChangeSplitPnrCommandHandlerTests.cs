using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderChange;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper.Order;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderChange
{
    public class OrderChangeSplitPnrCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderChangeSplitPnrCommandHandler>> _logger;
        private readonly Mock<IHostEnvironment> _hostEnvironment;
        private readonly OrderChangeSplitPnrCommandHandler _handler;
        private readonly Mock<IBrandMSCommand> _command;
        private readonly Mock<IBrandMSPTCCommand> _commandPTC;
        private readonly OrderDataMSQueryHandler _orderDataMS;
        private readonly Order.Infrastructure.Services.OrderSearchMS.OrderSearchMSCommandHandler _orderSearchMSCommandHandler;
        private readonly PaymentMSRetrieveQueryHandler _paymentMSRetrieveQueryHandler;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly Mock<ILogger<SplitPnrRepoCommandHandler>> _loggerSplitPnr;
        private readonly SplitPnrRepoCommandHandler _splitPnrRepoCommandHandler;
        private readonly Mock<ITrackAnalyticsEventCommandHandler> _trackAnalyticsEventCommandHandler;
        private readonly Mock<ILogger<TrackAnalyticsEventCommandHandler>> _tracklogger;
        private readonly SplitPnrPaymentCommandHandler _splitPnrPaymentCommand;
        private readonly PublishSplitOrderHistoryCommandHandler _publishSplitOrderHistory;

        public OrderChangeSplitPnrCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderChangeSplitPnrCommandHandler>>();
            _tracklogger = new Mock<ILogger<TrackAnalyticsEventCommandHandler>>();
            _hostEnvironment = new Mock<IHostEnvironment>();
            _command = new Mock<IBrandMSCommand>();
            _commandPTC = new Mock<IBrandMSPTCCommand>();
            _httpConnector = new Mock<IHttpConnector>();
            _orderDataMS = new OrderDataMSQueryHandler(_httpConnector.Object);
            _orderSearchMSCommandHandler = new Order.Infrastructure.Services.OrderSearchMS.OrderSearchMSCommandHandler(_httpConnector.Object);
            _splitPnrPaymentCommand = new SplitPnrPaymentCommandHandler(_httpConnector.Object);
            _paymentMSRetrieveQueryHandler = new PaymentMSRetrieveQueryHandler(_httpConnector.Object);
            _publishOrderDocMSCommandHandler = new PublishOrderDocMSCommandHandler(_httpConnector.Object);
            _publishSplitOrderHistory = new PublishSplitOrderHistoryCommandHandler(_httpConnector.Object);
            _validationMSCommandHandler = new ValidationMSCommandHandler(_httpConnector.Object);
            _trackAnalyticsEventCommandHandler = new Mock<ITrackAnalyticsEventCommandHandler>();
            _loggerSplitPnr = new Mock<ILogger<SplitPnrRepoCommandHandler>>();
            _splitPnrRepoCommandHandler = new SplitPnrRepoCommandHandler(_httpConnector.Object, _loggerSplitPnr.Object);
            _handler = new OrderChangeSplitPnrCommandHandler(_logger.Object, _orderDataMS, _validationMSCommandHandler, _orderSearchMSCommandHandler, _paymentMSRetrieveQueryHandler, _splitPnrRepoCommandHandler, _publishOrderDocMSCommandHandler, _hostEnvironment.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object, _splitPnrPaymentCommand, _publishSplitOrderHistory);
        }


        [Fact]
        public void When_LoggerObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderChangeSplitPnrCommandHandler(null, _orderDataMS, _validationMSCommandHandler, _orderSearchMSCommandHandler, _paymentMSRetrieveQueryHandler, _splitPnrRepoCommandHandler, _publishOrderDocMSCommandHandler, _hostEnvironment.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object, _splitPnrPaymentCommand, _publishSplitOrderHistory));
        }

        [Fact]
        public void When_HostEnvironmentObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderChangeSplitPnrCommandHandler(_logger.Object, _orderDataMS, _validationMSCommandHandler, _orderSearchMSCommandHandler, _paymentMSRetrieveQueryHandler, _splitPnrRepoCommandHandler, _publishOrderDocMSCommandHandler, null, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object, _splitPnrPaymentCommand, _publishSplitOrderHistory));
        }

        [Fact]
        public async Task ValidationMSTests()
        {
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            _mediator.Setup(x => x.Send(It.IsAny<ValidationMSCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Order.Infrastructure.Model.ValidationMS.ValidationRS()
            {
                Error = new List<ErrorDetails>() { new() { Code = "G009" }, new() { Code = "10144" }, new() { Code = "10146" }, new() { Code = "10147" }, new() { Code = "10140" }, new() { Code = "10152" }, new() { Code = "10153" }, new() { Code = "10145" } }
            });

            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\SplitPnrReq");
            var augmentationPoint = Mock.Of<AugmentationPoint>();
            var subscriptionKey = "1234";

            //Act
            var result = await _handler.Handle(request, augmentationPoint, subscriptionKey, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
        }

        [Fact]
        public async Task SplitPNRRequestValidationsTests()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\SplitPnrReq");

            request.DistributionChain.First().Ordinal = null;
            request.Request.Order.OrderID = null;
            request.AugmentationPoint.Any = null;
            var augmentationPoint = Mock.Of<AugmentationPoint>();
            var subscriptionKey = "1234";

            //Act
            var result = await _handler.Handle(request, augmentationPoint, subscriptionKey, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task Handler_Exception()
        {
            //Arrange
            _hostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\SplitPnrReq");
            var augmentationPoint = Mock.Of<AugmentationPoint>();
            var subscriptionKey = "1234";

            //Act
            var result = await _handler.Handle(request, augmentationPoint, subscriptionKey, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
        }

        [Fact]
        public void SplitPNR_ResponseMapper_Tests()
        {
            var parentDoc = TestDataProvider.GetTestData<OrderDocument>("\\NDCOrder\\OrderChange\\ParentDoc");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderCreate\\FareTypeOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, false, false, false, false, parentDoc, parentDoc);

            var response = ((OrderViewResponseType[])result.Items).First();
            var paymentResponse = ((PaymentFunctionsOrderType[])result.PaymentFunctions).First();

            Assert.NotNull(response);
            Assert.Equal("VS932ROVOOF25",response.Order.First().OrderID);
            Assert.NotNull(paymentResponse);
            Assert.Equal("81384764", paymentResponse.PaymentProcessingSummary.PaymentID);
            Assert.Equal("123456", response.DataLists.ContactInfoList.First().Phone.First().PhoneNumber);
            Assert.Equal("44", response.DataLists.ContactInfoList.First().Phone.First().CountryDialingCode);
        }
    }
}
