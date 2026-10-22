using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderCreate;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Interface;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Services.Air4OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderTicketMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderCreate
{
    public class OrderCreateEasyPayCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly HttpConnector _httpConnector;
        private readonly IMediator _mediator;
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly OfferMSQueryHandler _offerMSQueryHandler;
        private readonly ReferenceMSCurrencyQueryHandler _referenceMSCurrencyQueryHandler;
        private readonly AirportDetailsQueryHandler _airportDetailsQueryHandler;
        private readonly PaymentIFGMSAuthorizeCommandHandler _paymentIFGMSAuthorizeCommandHandler;
        private readonly PaymentIFGMSOrderCommandHandler _paymentIFGMSOrderCommandHandler;
        private readonly IBrandMSCommand _command;
        private readonly IBrandMSPTCCommand _commandPTC;
        private readonly OrderMSCommandHandler _orderMSCommandHandler;
        private readonly OrderTicketMSCommandHandler _orderTicketMSCommandHandler;
        private readonly IServiceProvider _serviceProvider;
        private readonly Mock<ITrackAnalyticsEventCommandHandler> _trackAnalyticsEventCommandHandler;
        private readonly ILogger<TrackAnalyticsEventCommandHandler> _trackLogger;
        private readonly ILogger<OrderCreateEasyPayCommandHandler> _trackEasyPayLogger;
        private readonly ILogger<SeatMSRetrieveSeatMapQueryHandler> _seatMSRetrieveSeatMapQueryHandlerLogger;
        private readonly ILogger<SeatMSSeatFulfillmentCommandHandler> _seatMSSeatFulfillmentCommandHandlerLogger;
        private readonly SeatMSQueryHandler _seatMSQueryHandler;
        private readonly SeatMSRetrieveSeatMapQueryHandler _seatMSRetrieveSeatMapQueryHandler;
        private readonly SeatMSSeatFulfillmentCommandHandler _seatMSSeatFulfillmentCommandHandler;
        private readonly OrderCreateEasyPayCommandHandler _handler;
        private readonly PaymentIFGMSAuthorizeCancelCommandHandler _paymentIFGMSAuthorizeCancelCommandHandler;
        private readonly ICreatePaymentDocumentCommand _createPaymentDocumentCommand;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly IFGAgentFopLookUpCommandHandler _iFGAgentFopLookUpCommandHandler;
        private readonly ISendOrderEmailCommand _sendOrderEmailCommand;
        private readonly PaymentMSAuthorizeCommandHandler _paymentMSAuthorizeCommandHandler;
        private readonly PaymentMSRefundCommandHandler _paymentMSRefundCommandHandler;
        private readonly PaymentMSCaptureCommandHandler _paymentMSCaptureCommandHandler;
        private readonly OrderManagementMSCommandHandler _orderManagementMSCommandHandler;
        private readonly ILogger<OrderManagementMSCommandHandler> _orderMgmtMSLogger;
        private readonly ICityCodeQuery _cityCodeQuery;
        private readonly OrderCancelCommandHandler _orderCancelCommandHandler;
        private readonly ILogger<OrderCancelCommandHandler> _orderCancelLogger;

        public OrderCreateEasyPayCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _validationMSCommandHandler = new ValidationMSCommandHandler(_httpConnector);
            _offerMSQueryHandler = new OfferMSQueryHandler(_httpConnector);
            _referenceMSCurrencyQueryHandler = new ReferenceMSCurrencyQueryHandler(_httpConnector);
            _airportDetailsQueryHandler = new AirportDetailsQueryHandler(_httpConnector);
            _paymentIFGMSAuthorizeCommandHandler = new PaymentIFGMSAuthorizeCommandHandler(_httpConnector);
            _paymentIFGMSOrderCommandHandler = new PaymentIFGMSOrderCommandHandler(_httpConnector);
            _command = new BrandMSCommandHandler(_httpConnector);
            _commandPTC = new BrandMSCommandHandlerPTC(_httpConnector);
            _orderMSCommandHandler = new OrderMSCommandHandler(_httpConnector);
            _orderTicketMSCommandHandler = new OrderTicketMSCommandHandler(_httpConnector);
            _seatMSQueryHandler = new SeatMSQueryHandler(_httpConnector);
            _seatMSRetrieveSeatMapQueryHandlerLogger = NullLogger<SeatMSRetrieveSeatMapQueryHandler>.Instance;
            _seatMSRetrieveSeatMapQueryHandler = new SeatMSRetrieveSeatMapQueryHandler(_httpConnector, _seatMSRetrieveSeatMapQueryHandlerLogger);
            _seatMSSeatFulfillmentCommandHandlerLogger = NullLogger<SeatMSSeatFulfillmentCommandHandler>.Instance;
            _seatMSSeatFulfillmentCommandHandler = new SeatMSSeatFulfillmentCommandHandler(_httpConnector, _seatMSSeatFulfillmentCommandHandlerLogger);
            _paymentIFGMSAuthorizeCancelCommandHandler = new PaymentIFGMSAuthorizeCancelCommandHandler(_httpConnector);
            _serviceProvider = new DefaultServiceProvider();
            _mediator = new Mediator(_serviceProvider);
            _trackEasyPayLogger = NullLogger<OrderCreateEasyPayCommandHandler>.Instance;
            _trackLogger = NullLogger<TrackAnalyticsEventCommandHandler>.Instance;
            _trackAnalyticsEventCommandHandler = new Mock<ITrackAnalyticsEventCommandHandler>();
            _createPaymentDocumentCommand = new CreatePaymentDocumentCommandHandler(_httpConnector);
            _publishOrderDocMSCommandHandler = new PublishOrderDocMSCommandHandler(_httpConnector);
            _iFGAgentFopLookUpCommandHandler = new IFGAgentFopLookUpCommandHandler(_httpConnector);
            _sendOrderEmailCommand = new SendOrderEmailCommandHandler(_httpConnector);
            _paymentMSAuthorizeCommandHandler = new PaymentMSAuthorizeCommandHandler(_httpConnector);
            _paymentMSCaptureCommandHandler = new PaymentMSCaptureCommandHandler(_httpConnector);
            _paymentMSRefundCommandHandler = new PaymentMSRefundCommandHandler(_httpConnector);
            _orderMgmtMSLogger = NullLogger<OrderManagementMSCommandHandler>.Instance;
            _orderManagementMSCommandHandler = new OrderManagementMSCommandHandler(_httpConnector, _orderMgmtMSLogger);
            _cityCodeQuery = new CityCodeQueryHandler(_httpConnector);
            _orderCancelLogger = NullLogger<OrderCancelCommandHandler>.Instance; 
            _orderCancelCommandHandler =   new OrderCancelCommandHandler(_httpConnector, _orderCancelLogger);

            _handler = new OrderCreateEasyPayCommandHandler(_trackEasyPayLogger, _hostEnvironment, _validationMSCommandHandler, _offerMSQueryHandler, _referenceMSCurrencyQueryHandler, _airportDetailsQueryHandler, _paymentIFGMSAuthorizeCommandHandler, _paymentIFGMSOrderCommandHandler, _command, _commandPTC, _orderMSCommandHandler, _orderTicketMSCommandHandler, _seatMSQueryHandler, _seatMSRetrieveSeatMapQueryHandler, _seatMSSeatFulfillmentCommandHandler, _trackAnalyticsEventCommandHandler.Object, _paymentIFGMSAuthorizeCancelCommandHandler, _createPaymentDocumentCommand, _publishOrderDocMSCommandHandler, _iFGAgentFopLookUpCommandHandler, _sendOrderEmailCommand, _paymentMSAuthorizeCommandHandler, _paymentMSCaptureCommandHandler, _paymentMSRefundCommandHandler, _orderManagementMSCommandHandler, _cityCodeQuery, _orderCancelCommandHandler);

            ConfigReader.Set("ReferenceMSUrl", "https://Testresources.virginatlantic.com/dev/ReferenceMicroservice/");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("BrandMSUrl", "https://Testresources.virginatlantic.com/dev/brandmicroservice/brand");
            ConfigReader.Set("PaymentIFGMSAuthorizeUrl", "https://Testresources.virginatlantic.com/DEV/IFGpayment/IFGAuthorizePayment");
            ConfigReader.Set("OrderMSCreateUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderMS/OrderCreate");
            ConfigReader.Set("OrderTicketMSUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderDocument/order/ticket");
            ConfigReader.Set("PaymentIFGMSOrderUrl", "https://Testresources.virginatlantic.com/DEV/IFGpayment/IFGOrderPayment");
            ConfigReader.Set("CreatePaymentDocumentUrl", "https://Testresources.virginatlantic.com/DEV/orderpaymentmicroservice/CreatePaymentDocument");
            ConfigReader.Set("PublishOrderDocUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/PublishOrderDoc");
            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("EmailMSSendOrderEmailUrl", "https://Testresources.virginatlantic.com/dev/emailmicroservice/SendOrderEmail");
            ConfigReader.Set("emailmssubscriptionkey", "Test_emailmssubscriptionkey");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");
            ConfigReader.Set("ReferenceMSSubscriptionKey", "Test_ReferenceMSSubscriptionKey");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("BrandMSSubscriptionKey", "Test_BrandMSSubscriptionKey");
            ConfigReader.Set("PaymentIFGMSSubscriptionKey", "Test_PaymentIFGMSSubscriptionKey");
            ConfigReader.Set("OrderMSSubscriptionKey", "Test_OrderMSSubscriptionKey");
            ConfigReader.Set("OrderTicketMSSubscriptionKey", "Test_OrderTicketMSSubscriptionKey");
            ConfigReader.Set("PaymentMSSubscriptionKey", "Test_PaymentMSSubscriptionKey");
            ConfigReader.Set("OrderSearchMSSubscriptionKey", "Test_OrderSearchMSSubscriptionKey");
            ConfigReader.Set("agentregistrationenvironment", "NONPROD");
            
            
        }

        [Fact]
        public async Task OrderCreateEasyPay_CommandHandlershouldReturnSuccess()
        {
            //Arrange

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateEasyPayReq");
            var subscriptionKey = "1234";
            bool isEasyPay = true;

            //Act
            var result = await _handler.Handle(request, isEasyPay, subscriptionKey, CancellationToken.None);

            //Assert
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932PWVUAA25", ((result.Value.Value.Items.First()) as OrderViewResponseType).Order.First().OrderID);
            Assert.Equal("176397686334041277", items.PaymentFunctions.First().PaymentProcessingSummary.PaymentID);
        }

        [Fact]
        public async Task CreatePaymentDocumentCommandHandler_ShouldReturnSuccess()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<PaymentDocRequest>("NDCOrder\\OrderCreate\\CreatePaymentDocumentEasyPay");

            ConfigReader.Set("CreatePaymentDocumentUrl", "https://Testresources.virginatlantic.com/DEV/orderpaymentmicroservice/CreatePaymentDocumentEasyPay");

            //Act 
            var result = await _createPaymentDocumentCommand.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.PaymentDocument.TransactionId);
            Assert.Equal("495.39", result.PaymentDocument.Amount.ToString());
            Assert.Equal("177677579874828458", result.PaymentDocument.TransactionId);
            Assert.Equal("VS932RXNCES26", result.PaymentDocument.OrderID);
            Assert.Equal("EasyPay", result.PaymentDocument.FormOfPayment);
        }

        [Fact]
        public async Task CityCodeQueryHandlerShouldReturnSuccess()
        {
            //Arrange
            string countryCode = "GB";
            ConfigReader.Set("ConfigurationMSCityCodeEndpoint", "https://Testresources.virginatlantic.com/Dev/ConfigurationMicroservice/CityCode");
            ConfigReader.Set("ConfigurationMSSubscriptionKey", "Test_ConfigurationMSSubscriptionKey");

            //Act
            var result = await _cityCodeQuery.Handle(countryCode, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("LON", result.CityCode);
            Assert.Equal("LONNDC", result.NDCCityCodeLocation);
            Assert.Equal("BSPGB_United Kingdom", result.BSPMarket);
        }
    }
}