using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Net.Http;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderCreate;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Interface;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Services.Air4OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.FlightServiceMS;
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
using VAA.EAPI.NDC.Order.Mapper.Order;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderCreate
{
    public class OrderCreateIFGCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly CallbackLogger _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
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
        private readonly ILogger<TrackAnalyticsEventCommandHandler> _trackLogger;
        private readonly RecordingLogger _trackIfgLogger;
        private readonly ILogger<SeatMSRetrieveSeatMapQueryHandler> _seatMSRetrieveSeatMapQueryHandlerLogger;
        private readonly ILogger<SeatMSSeatFulfillmentCommandHandler> _seatMSSeatFulfillmentCommandHandlerLogger;
        private readonly ILogger<OrderManagementMSCommandHandler> _orderMgmtMSLogger;
        private readonly SeatMSQueryHandler _seatMSQueryHandler;
        private readonly SeatMSRetrieveSeatMapQueryHandler _seatMSRetrieveSeatMapQueryHandler;
        private readonly SeatMSSeatFulfillmentCommandHandler _seatMSSeatFulfillmentCommandHandler;
        private readonly FlightServicesDocQueryHandler _flightServicesDocQueryHandler;
        private readonly ICreatePaymentDocumentCommand _createPaymentDocumentCommand;
        private readonly OrderCreateIFGCommandHandler _handler;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly PaymentIFGMSAuthorizeCancelCommandHandler _paymentIFGMSAuthorizeCancelCommandHandler;
        private readonly IFGAgentFopLookUpCommandHandler _iFGAgentFopLookUpCommandHandler;
        private readonly ISendOrderEmailCommand _sendOrderEmailCommand;
        private readonly ICurrencyConversionQuery _currencyConversionQuery;
        private readonly CheckAgentStatusQueryHandler _checkAgentStatusQueryHandler;
        private readonly TransactionCommandHandler _transactionCommandHandler;
        private readonly Mock<ITrackAnalyticsEventCommandHandler> _trackAnalyticsEventCommandHandler;
        private readonly PaymentMSAuthorizeCommandHandler _paymentMSAuthorizeCommandHandler;
        private readonly PaymentMSRefundCommandHandler _paymentMSRefundCommandHandler;
        private readonly PaymentMSCaptureCommandHandler _paymentMSCaptureCommandHandler;
        private readonly OrderManagementMSCommandHandler _orderManagementMSCommandHandler;
        private readonly ICityCodeQuery _cityCodeQuery;
        private readonly OrderCancelCommandHandler _orderCancelCommandHandler;
        private readonly ILogger<OrderCancelCommandHandler> _orderCancelLogger;

        public OrderCreateIFGCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = new CallbackLogger();
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };
            _httpClientFactoryFixture = httpClientFactoryFixture;
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
            _flightServicesDocQueryHandler = new FlightServicesDocQueryHandler(_httpConnector);
            _paymentIFGMSAuthorizeCancelCommandHandler = new PaymentIFGMSAuthorizeCancelCommandHandler(_httpConnector);
            _serviceProvider = new DefaultServiceProvider();
            _mediator = new Mediator(_serviceProvider);
            _trackIfgLogger = new RecordingLogger();
            _trackLogger = NullLogger<TrackAnalyticsEventCommandHandler>.Instance;
            _createPaymentDocumentCommand = new CreatePaymentDocumentCommandHandler(_httpConnector);
            _publishOrderDocMSCommandHandler = new PublishOrderDocMSCommandHandler(_httpConnector);
            _iFGAgentFopLookUpCommandHandler = new IFGAgentFopLookUpCommandHandler(_httpConnector);
            _sendOrderEmailCommand = new SendOrderEmailCommandHandler(_httpConnector);
            _currencyConversionQuery = new CurrencyConversionQueryHandler(_httpConnector);
            _checkAgentStatusQueryHandler = new CheckAgentStatusQueryHandler(_httpConnector);
            _transactionCommandHandler = new TransactionCommandHandler(_httpConnector);
            _trackAnalyticsEventCommandHandler = new Mock<ITrackAnalyticsEventCommandHandler>();
            _paymentMSAuthorizeCommandHandler = new PaymentMSAuthorizeCommandHandler(_httpConnector);
            _paymentMSCaptureCommandHandler = new PaymentMSCaptureCommandHandler(_httpConnector);
            _paymentMSRefundCommandHandler = new PaymentMSRefundCommandHandler(_httpConnector);
            _orderMgmtMSLogger = NullLogger<OrderManagementMSCommandHandler>.Instance;
            _orderManagementMSCommandHandler = new OrderManagementMSCommandHandler(_httpConnector, _orderMgmtMSLogger);
            _cityCodeQuery = new CityCodeQueryHandler(_httpConnector);
            _orderCancelLogger = NullLogger<OrderCancelCommandHandler>.Instance;
            _orderCancelCommandHandler = new OrderCancelCommandHandler(
                new HttpConnector(_logger, new CancellationCheckingHttpClientFactory(_httpConnector)), _orderCancelLogger);

            _handler = new OrderCreateIFGCommandHandler(_trackIfgLogger, _hostEnvironment, _validationMSCommandHandler, _offerMSQueryHandler, _referenceMSCurrencyQueryHandler, _airportDetailsQueryHandler, _paymentIFGMSAuthorizeCommandHandler, _paymentIFGMSOrderCommandHandler, _command, _commandPTC, _orderMSCommandHandler, _orderTicketMSCommandHandler, _trackAnalyticsEventCommandHandler.Object, _seatMSQueryHandler, _seatMSRetrieveSeatMapQueryHandler, _seatMSSeatFulfillmentCommandHandler, _flightServicesDocQueryHandler, _createPaymentDocumentCommand, _publishOrderDocMSCommandHandler, _paymentIFGMSAuthorizeCancelCommandHandler, _iFGAgentFopLookUpCommandHandler, _sendOrderEmailCommand, _currencyConversionQuery, _checkAgentStatusQueryHandler, _transactionCommandHandler, _paymentMSAuthorizeCommandHandler, _paymentMSRefundCommandHandler, _paymentMSCaptureCommandHandler, _orderManagementMSCommandHandler, _cityCodeQuery, _orderCancelCommandHandler);

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
            ConfigReader.Set("ARCPaymentMSUrl", "https://tempuri.org/");
            ConfigReader.Set("SeatMSUrl", "https://Testresources.virginatlantic.com/dev/SeatMicroservice");

            ConfigReader.Set("ReferenceMSSubscriptionKey", "Test_ReferenceMSSubscriptionKey");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("BrandMSSubscriptionKey", "Test_BrandMSSubscriptionKey");
            ConfigReader.Set("PaymentIFGMSSubscriptionKey", "Test_PaymentIFGMSSubscriptionKey");
            ConfigReader.Set("OrderMSSubscriptionKey", "Test_OrderMSSubscriptionKey");
            ConfigReader.Set("OrderTicketMSSubscriptionKey", "Test_OrderTicketMSSubscriptionKey");
            ConfigReader.Set("agentregistrationenvironment", "NONPROD");
            ConfigReader.Set("PaymentMSSubscriptionKey", "Test_PaymentMSSubscriptionKey");
            ConfigReader.Set("OrderSearchMSSubscriptionKey", "Test_OrderSearchMSSubscriptionKey");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");
            ConfigReader.Set("emailmssubscriptionkey", "Test_emailmssubscriptionkey");
            ConfigReader.Set("ARCPaymentMSSubscriptionKey", "Test_ARCPaymentMSSubscriptionKey");
            ConfigReader.Set("SeatMSSubscriptionKey", "Test_SeatMSSubscriptionKey");
        }

        [Fact]
        public async Task OrderCreateIFG_CommandHandlershouldReturnSuccess()
        {
            //Arrange

            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateIFGCashRq");
            var subscriptionKey = "1234";
            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");


            //Act
            var result = await _handler.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            var items = result.Value.Value;
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal("VS932PWVUAA25", ((result.Value.Value.Items.First()) as OrderViewResponseType).Order.First().OrderID);
            Assert.Equal("ADULT_1", ((result.Value.Value.Items.First()) as OrderViewResponseType).TicketDocInfo.First().PaxRefID);
            Assert.Equal("9322973112264", ((result.Value.Value.Items.First()) as OrderViewResponseType).TicketDocInfo.First().Ticket.First().TicketNumber);
        }

        [Theory]
        [InlineData(RollbackOutcome.Success, RollbackOutcome.Success, false)]
        [InlineData(RollbackOutcome.Exception, RollbackOutcome.Success, false)]
        [InlineData(RollbackOutcome.Success, RollbackOutcome.Exception, false)]
        [InlineData(RollbackOutcome.FailedResponse, RollbackOutcome.Success, false)]
        [InlineData(RollbackOutcome.Success, RollbackOutcome.FailedResponse, false)]
        [InlineData(RollbackOutcome.Success, RollbackOutcome.Success, true)]
        public async Task OrderCreateIFG_WhenOrderPaymentFails_AttemptsRollbackAndReportsFailures(
            RollbackOutcome pnrCancelOutcome, RollbackOutcome authorizationCancelOutcome, bool requestCancelled)
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateIFGCashRq");
            var originalOrderUrl = ConfigReader.Try("PaymentIFGMSOrderUrl");
            var originalAuthorizeCancelUrl = ConfigReader.Try("PaymentIFGMSAuthorizeCancelUrl");
            var originalOrderCancelUrl = ConfigReader.Try("OrderCancelMSUrl");
            var originalOrderCancelKey = ConfigReader.Try("OrderCancelMSSubscriptionKey");
            var originalDiagnostics = ConfigReader.Try("api-diagnostics");
            const string orderUrl = "https://test.example/ifg-order-failure";
            var orderCancelUrl = pnrCancelOutcome switch
            {
                RollbackOutcome.Exception => ":bad",
                RollbackOutcome.FailedResponse => "https://test.example/ordercancel/failure",
                _ => "https://test.example/ordercancel/ok"
            };
            var authorizeCancelUrl = authorizationCancelOutcome switch
            {
                RollbackOutcome.Exception => ":bad",
                RollbackOutcome.FailedResponse => "https://test.example/authorization-cancel/failure",
                _ => "https://test.example/authorization-cancel/ok"
            };

            try
            {
                ConfigReader.Set("PaymentIFGMSOrderUrl", orderUrl);
                ConfigReader.Set("PaymentIFGMSAuthorizeCancelUrl", authorizeCancelUrl);
                ConfigReader.Set("OrderCancelMSUrl", orderCancelUrl);
                ConfigReader.Set("OrderCancelMSSubscriptionKey", "Test_OrderCancelMSSubscriptionKey");
                if (requestCancelled)
                    ConfigReader.Set("api-diagnostics", "true");

                var paymentDocumentUrl = ConfigReader.Get("CreatePaymentDocumentUrl");
                var paymentDocumentsBefore = _httpClientFactoryFixture.Current.GetResponses(paymentDocumentUrl)
                    .Count(response => response.RequestMessage?.RequestUri?.AbsoluteUri == paymentDocumentUrl);
                var orderResponsesBefore = _httpClientFactoryFixture.Current.GetResponses(orderUrl)
                    .Count(response => response.RequestMessage?.RequestUri?.AbsoluteUri == orderUrl);
                var pnrResponsesBefore = _httpClientFactoryFixture.Current.GetResponses(orderUrl)
                    .Count(response => response.RequestMessage?.RequestUri?.AbsoluteUri == orderCancelUrl);
                var authorizationResponsesBefore = _httpClientFactoryFixture.Current.GetResponses(orderUrl)
                    .Count(response => response.RequestMessage?.RequestUri?.AbsoluteUri == authorizeCancelUrl);
                using var requestCancellation = new CancellationTokenSource();
                int? pnrResponsesAtCancellation = null;
                if (requestCancelled)
                    _logger.OnMessage = message =>
                    {
                        if (message.Contains(orderUrl, StringComparison.Ordinal))
                        {
                            pnrResponsesAtCancellation = _httpClientFactoryFixture.Current.GetResponses(orderUrl)
                                .Count(response => response.RequestMessage?.RequestUri?.AbsoluteUri == orderCancelUrl);
                            requestCancellation.Cancel();
                        }
                    };
                var result = await _handler.Handle(request, "1234", requestCancelled ? requestCancellation.Token : TestContext.Current.CancellationToken);

                if (requestCancelled)
                {
                    Assert.True(requestCancellation.IsCancellationRequested);
                    Assert.Equal(pnrResponsesBefore, pnrResponsesAtCancellation);
                }
                Assert.NotEqual(Status.OK, result.Value.Status);
                var error = Assert.IsType<ErrorType>(Assert.Single(result.Value.Value.Items));
                Assert.Equal("913", error.Code);
                Assert.DoesNotContain("Card Authorization", error.DescText);
                var paymentDocumentsAfter = _httpClientFactoryFixture.Current.GetResponses(paymentDocumentUrl)
                    .Count(response => response.RequestMessage?.RequestUri?.AbsoluteUri == paymentDocumentUrl);
                Assert.Equal(paymentDocumentsBefore, paymentDocumentsAfter);

                Assert.Equal(orderResponsesBefore + 1, _httpClientFactoryFixture.Current.GetResponses(orderUrl)
                    .Count(response => response.RequestMessage?.RequestUri?.AbsoluteUri == orderUrl));

                var cancelPnrResponses = _httpClientFactoryFixture.Current.GetResponses(orderUrl)
                    .Where(response => response.RequestMessage?.RequestUri?.AbsoluteUri == orderCancelUrl)
                    .Skip(pnrResponsesBefore).ToList();
                if (pnrCancelOutcome == RollbackOutcome.Exception)
                {
                    Assert.Empty(cancelPnrResponses);
                }
                else
                {
                    var cancelPnrRequest = Assert.Single(cancelPnrResponses).RequestMessage;
                    Assert.Equal(HttpMethod.Patch, cancelPnrRequest.Method);
                    var cancelPnr = Json.Deserialize<OrderChangeRequest>(await cancelPnrRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
                    Assert.Equal("PWVUAA", cancelPnr.RLOC);
                    Assert.True(cancelPnr.CancelPnr);
                    Assert.Equal(OrderChangeRequest.Operations.OrderCreate, cancelPnr.Operation);
                }

                var authorizeCancelResponses = _httpClientFactoryFixture.Current.GetResponses(orderUrl)
                    .Where(response => response.RequestMessage?.RequestUri?.AbsoluteUri == authorizeCancelUrl)
                    .Skip(authorizationResponsesBefore).ToList();
                if (authorizationCancelOutcome == RollbackOutcome.Exception)
                {
                    Assert.Empty(authorizeCancelResponses);
                }
                else
                {
                    var authorizeCancelRequest = Assert.Single(authorizeCancelResponses).RequestMessage;
                    Assert.Equal(HttpMethod.Post, authorizeCancelRequest.Method);
                }

                Assert.Equal(pnrCancelOutcome != RollbackOutcome.Success, _trackIfgLogger.ErrorMessages.Any(message =>
                    message.Contains("PNR", StringComparison.Ordinal) && message.Contains("after IFG order failure", StringComparison.Ordinal)));
                Assert.Equal(authorizationCancelOutcome != RollbackOutcome.Success, _trackIfgLogger.ErrorMessages.Any(message =>
                    message.Contains("IFG authorization", StringComparison.Ordinal) && message.Contains("after IFG order failure", StringComparison.Ordinal)));
            }
            finally
            {
                _logger.OnMessage = null;
                ConfigReader.Set("api-diagnostics", originalDiagnostics ?? string.Empty);
                ConfigReader.Set("PaymentIFGMSOrderUrl", originalOrderUrl ?? string.Empty);
                ConfigReader.Set("PaymentIFGMSAuthorizeCancelUrl", originalAuthorizeCancelUrl ?? string.Empty);
                ConfigReader.Set("OrderCancelMSUrl", originalOrderCancelUrl ?? string.Empty);
                ConfigReader.Set("OrderCancelMSSubscriptionKey", originalOrderCancelKey ?? string.Empty);
            }
        }

        [Theory]
        [InlineData("https://test.example/ifg-order-malformed")]
        [InlineData("https://test.example/ifg-order-null-document")]
        [InlineData("https://test.example/ifg-order-empty-errors")]
        public async Task OrderCreateIFG_WhenOrderPaymentCannotBeConfirmed_AttemptsBothRollbackActionsAndReturnsDefaultError(string orderUrl)
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateIFGCashRq");
            var originalOrderUrl = ConfigReader.Try("PaymentIFGMSOrderUrl");
            var originalAuthorizeCancelUrl = ConfigReader.Try("PaymentIFGMSAuthorizeCancelUrl");
            var originalOrderCancelUrl = ConfigReader.Try("OrderCancelMSUrl");
            var originalOrderCancelKey = ConfigReader.Try("OrderCancelMSSubscriptionKey");
            const string orderCancelUrl = "https://test.example/ordercancel/ok";
            const string authorizeCancelUrl = "https://test.example/authorization-cancel/ok";

            try
            {
                ConfigReader.Set("PaymentIFGMSOrderUrl", orderUrl);
                ConfigReader.Set("PaymentIFGMSAuthorizeCancelUrl", authorizeCancelUrl);
                ConfigReader.Set("OrderCancelMSUrl", orderCancelUrl);
                ConfigReader.Set("OrderCancelMSSubscriptionKey", "Test_OrderCancelMSSubscriptionKey");

                var pnrResponsesBefore = _httpClientFactoryFixture.Current.GetResponses(orderUrl)
                    .Count(response => response.RequestMessage?.RequestUri?.AbsoluteUri == orderCancelUrl);
                var authorizationResponsesBefore = _httpClientFactoryFixture.Current.GetResponses(orderUrl)
                    .Count(response => response.RequestMessage?.RequestUri?.AbsoluteUri == authorizeCancelUrl);
                var paymentDocumentUrl = ConfigReader.Get("CreatePaymentDocumentUrl");
                var paymentDocumentsBefore = _httpClientFactoryFixture.Current.GetResponses(paymentDocumentUrl)
                    .Count(response => response.RequestMessage?.RequestUri?.AbsoluteUri == paymentDocumentUrl);

                var result = await _handler.Handle(request, "1234", TestContext.Current.CancellationToken);

                Assert.NotEqual(Status.OK, result.Value.Status);
                var error = Assert.IsType<ErrorType>(Assert.Single(result.Value.Value.Items));
                Assert.Equal("911", error.Code);
                Assert.Equal(pnrResponsesBefore + 1, _httpClientFactoryFixture.Current.GetResponses(orderUrl)
                    .Count(response => response.RequestMessage?.RequestUri?.AbsoluteUri == orderCancelUrl));
                var authorizeCancelRequest = Assert.Single(_httpClientFactoryFixture.Current.GetResponses(orderUrl)
                    .Where(response => response.RequestMessage?.RequestUri?.AbsoluteUri == authorizeCancelUrl)
                    .Skip(authorizationResponsesBefore)).RequestMessage;
                Assert.Equal(HttpMethod.Post, authorizeCancelRequest.Method);
                Assert.Equal(paymentDocumentsBefore, _httpClientFactoryFixture.Current.GetResponses(paymentDocumentUrl)
                    .Count(response => response.RequestMessage?.RequestUri?.AbsoluteUri == paymentDocumentUrl));
            }
            finally
            {
                ConfigReader.Set("PaymentIFGMSOrderUrl", originalOrderUrl ?? string.Empty);
                ConfigReader.Set("PaymentIFGMSAuthorizeCancelUrl", originalAuthorizeCancelUrl ?? string.Empty);
                ConfigReader.Set("OrderCancelMSUrl", originalOrderCancelUrl ?? string.Empty);
                ConfigReader.Set("OrderCancelMSSubscriptionKey", originalOrderCancelKey ?? string.Empty);
            }
        }

        [Fact]
        public async Task IFGAuthorizationCancel_UsesTheCancellationToken()
        {
            var originalAuthorizeCancelUrl = ConfigReader.Try("PaymentIFGMSAuthorizeCancelUrl");
            using var cancellation = new CancellationTokenSource();

            try
            {
                ConfigReader.Set("PaymentIFGMSAuthorizeCancelUrl", "https://test.example/authorization-cancel/ok");
                cancellation.Cancel();
                var handler = new PaymentIFGMSAuthorizeCancelCommandHandler(
                    new HttpConnector(_logger, new CancellationCheckingHttpClientFactory(_httpConnector)));
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                    handler.HandleWithStatusAsync(
                        new AuthorizePaymentRequest { TrackId = "27be8079-9372-4b71-b80f-3064054f7a2f" }, cancellation.Token));
            }
            finally
            {
                ConfigReader.Set("PaymentIFGMSAuthorizeCancelUrl", originalAuthorizeCancelUrl ?? string.Empty);
            }
        }

        [Fact]
        public async Task CreatePaymentDocumentCommandHanlder_ShouldReturnSuccess()
        {
            //Arrange
            var request = new PaymentDocRequest()
            {
                Type = "PaymentRefundDoc",
                ProductType = Constants.ProductType.Ticket,
                OrderId = "VS932TIXLII25",
                Parties = new()
                {
                    Participants = [new() { Ordinal = "1", AgencyID = "9128103", Name = "Skyscanner" }]
                },
                PaymentDetails = new PaymentDetails[]
                {
                    new()
                    {
                        Amount = new()
                        {
                            MonetaryAmount = "125.34",
                            BaseCurrency = "GBP"
                        },
                        FormOfPayments = PaymentDetails.FormOfPayment.CashEnum,
                        TrackId = "b4925067-ba8f-4df7-b8a8-6b66e5cac614"
                    }
                }
            };

            //Act 
            var result = await _createPaymentDocumentCommand.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.PaymentDocument.TransactionId);
            Assert.Equal("125.34", result.PaymentDocument.Amount.ToString());
        }

        [Fact]
        public void ARC_ResponseMapper_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderCreate\\ArcOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            Assert.NotNull(response.Order.First().OrderItem.First());
            Assert.Equal("177.00", response.Order.First().OrderItem.First().Price.BaseAmount.Value.ToString());
            Assert.Equal("70J", response.Order.First().OrderItem.First().FareDetail.First().FareComponent.First().NegotiatedCode);
            Assert.Equal("USD", response.Order.First().OrderItem.First().Price.BaseAmount.CurCode.ToString());
        }

        [Fact]
        public async Task TransactionAPIShouldReturnSuccess_ARC()
        {
            var transactionRequest = TestDataProvider.GetTestData<TransactionServiceRequest>("NDCOrder\\OrderChange\\TransactionRequest");

            ConfigReader.Set("ARCPaymentMSUrl", "https://Testresources.virginatlantic.com/dev/ARCPayment/");
            ConfigReader.Set("ARCPaymentMSSubscriptionKey", "Test_ARCPaymentMSSubscriptionKey");

            var result = await _transactionCommandHandler.Handle(transactionRequest);

            Assert.NotNull(result);
            Assert.Equal("f4da1759-68ba-49fb-bd5e-3bfc278f196b", result.RequestId);
            Assert.Equal("SUCCESS", result.Status);

        }

        [Fact]
        public async Task ValidateSeatCCFop()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateIFGCashRq_WithSeat");
            var subscriptionKey = "1234";
            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");


            //Act
            var result = await _handler.Handle(request, subscriptionKey, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            var items = result.Value.Value.Items.First() as OrderViewResponseType;
            var warning = items!.Warning;
            Assert.NotEmpty(warning);
            Assert.Equal("466", warning.First().Code);
        }

        [Fact]
        public async Task OrderCreateIFG_CommandHandlerValidatingIFGAgentLookupResp()
        {
            //Arrange

            var request = new IFGAgentFopLookUpCommand()
            {
                AgentCode = "9128103",
                ClientID = "932",
                CountryCode = "GB"

            };

            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");

            //Act
            var result = await _iFGAgentFopLookUpCommandHandler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal("200", result.HttpStatus.ToString());
            Assert.Equal("AV001:Active BSP Accredited Agent with customer's TA.", result.Description);
            Assert.Equal("GB", result.BspCode);
            Assert.True(result.CreditCardAllowed);
            Assert.True(result.BspCashAllowed);

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

        [Fact]
        public async Task OrderMSHandler_ShouldReturnSuccessForDeltaPNRIssue()
        {
            //   C: \Users\571717\source\repos\VAAD1ST\eapi\VAA.EAPI.Microservices\VAA.EAPI.Microservices.Framework.Tests\TestDataSamples\NDCOrder\OrderCreate\OrderMSRequest.json
            //Arrange
            var request = TestDataProvider.GetTestData<OrderMSCommand>("NDCOrder\\OrderCreate\\OrderMSRequest");
            ConfigReader.Set("OrderMSSubscriptionKey", "Test_OrderMSSubscriptionKey");
            ConfigReader.Set("OrderMSCreateUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderMS/OrderCreate/21f4a028");

            //Act
            var result = await _orderMSCommandHandler.Handle(request, CancellationToken.None);
            //Assert
            Assert.NotNull(result);
            Assert.Equal("F002", result.Errors.First().Code);
        }

        public enum RollbackOutcome
        {
            Success,
            Exception,
            FailedResponse
        }

        private sealed class RecordingLogger : ILogger<OrderCreateIFGCommandHandler>
        {
            public List<string> ErrorMessages { get; } = new();

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                    ErrorMessages.Add(formatter(state, exception));
            }
        }

        private sealed class CallbackLogger : ILogger<HttpConnector>
        {
            public Action<string>? OnMessage { get; set; }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                OnMessage?.Invoke(formatter(state, exception));
            }
        }

        private sealed class CancellationCheckingHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpConnector _inner;

            public CancellationCheckingHttpClientFactory(HttpConnector inner) => _inner = inner;

            public HttpClient CreateClient(string name) => new(new CancellationCheckingHandler(_inner));
        }

        private sealed class CancellationCheckingHandler : HttpMessageHandler
        {
            private readonly HttpConnector _inner;

            public CancellationCheckingHandler(HttpConnector inner) => _inner = inner;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var forwarded = new HttpRequestMessage(request.Method, request.RequestUri) { Content = request.Content };
                foreach (var header in request.Headers)
                    forwarded.Headers.TryAddWithoutValidation(header.Key, header.Value);
                return _inner.SendAsync(forwarded, cancellationToken);
            }
        }
    }
}
