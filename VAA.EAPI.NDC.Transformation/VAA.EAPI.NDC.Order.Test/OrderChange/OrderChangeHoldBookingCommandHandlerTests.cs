using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderChange;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Mapper.Order;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderChange
{
    public class OrderChangeHoldBookingCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly OrderChangeHoldBookingCommandHandler _handler;
        private readonly IHostEnvironment _hostEnvironment;
        
        public OrderChangeHoldBookingCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };

            _serviceProvider = new ServiceCollection().AddLogging()
                 .AddMediatR(cfg =>
                 {
                     cfg.RegisterServicesFromAssemblyContaining<OrderChangeHoldBookingCommandHandler>();
                 })

                 .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHostEnvironment>(_hostEnvironment)
                .AddSingleton<IBrandMSCommand, BrandMSCommandHandler>()
                .AddSingleton<IBrandMSPTCCommand, BrandMSCommandHandlerPTC>()
                .AddSingleton<PaymentIFGMSAuthorizeCommandHandler>()
                .AddSingleton<PaymentIFGMSOrderCommandHandler>()
                .AddSingleton<OrderDataMSQueryHandler>()
                .AddSingleton<OrderSearchMSCommandHandler>()
                .AddSingleton<ManageCustomerRepoCommandHandler>()
                .AddSingleton<ICreatePaymentDocumentCommand, CreatePaymentDocumentCommandHandler>()
                .AddSingleton<PublishOrderDocMSCommandHandler>()
                .AddSingleton<PaymentIFGMSAuthorizeCancelCommandHandler>()
                .AddSingleton<IFGAgentFopLookUpCommandHandler>()
                .AddSingleton<AirportDetailsQueryHandler>()
                .AddSingleton<ISendOrderEmailCommand, SendOrderEmailCommandHandler>()
                .AddSingleton<ICurrencyConversionQuery, CurrencyConversionQueryHandler>()
                .AddSingleton<ICheckAgentStatusQuery, CheckAgentStatusQueryHandler>()
                .AddSingleton<ITrackAnalyticsEventCommandHandler>(new Mock<ITrackAnalyticsEventCommandHandler>().Object)
                .AddSingleton<ITransactionCommand, TransactionCommandHandler>()
                .AddSingleton<PaymentMSRetrieveQueryHandler>()
                .AddSingleton<IARCOrderDataMSQuery, ARCOrderDataMSQueryHandler>()
                .AddSingleton<OrderChangeHoldBookingCommandHandler>()
               .BuildServiceProvider();

            _handler = _serviceProvider.GetService<OrderChangeHoldBookingCommandHandler>();

            ConfigReader.Set("CreatePaymentDocumentUrl", "https://Testresources.virginatlantic.com/DEV/orderpaymentmicroservice/CreatePaymentDocument");
            ConfigReader.Set("PaymentMSSubscriptionKey", "Test_PaymentMSSubscriptionKey");
            ConfigReader.Set("ReferenceMSUrl", "https://Testresources.virginatlantic.com/dev/ReferenceMicroservice/");
            ConfigReader.Set("ReferenceMSSubscriptionKey", "Test_ReferenceMSSubscriptionKey");
        }

        [Fact]
        public void Handler_ShouldBeResolved_From_ServiceProvider()
        {
            var handler = _serviceProvider.GetService<OrderChangeHoldBookingCommandHandler>();
            Assert.NotNull(handler);
        }

        [Fact]
        public async Task OrderChangeHoldBookingRequestValidationsTests()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeHoldBookingReq");

            request.DistributionChain.First().Ordinal = "3";
            request.Request.Order.OrderID = null;

            //Act
            var result = await _handler.Handle(new OrderChangeHoldBookingCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task ValidationMSTests()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReqSSR");
            ConfigReader.Set("OrderDataMSEndpoint", "https://Testresources.virginatlantic.com/dev/orderdata/orders/");
            ConfigReader.Set("OrderDataMSSubscriptionKey", "Test_OrderDataMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/ValidationSubsKey");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");

            request.DistributionChain.First().ParticipatingOrg.OrgID = "91281035";

            //Act
            var result = await _handler.Handle(new OrderChangeHoldBookingCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task Handler_Exception()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");
            ConfigReader.Set("OrderDataMSEndpoint", "https://Testresources.virginatlantic.com/dev/orderdata/orders/");
            ConfigReader.Set("OrderDataMSSubscriptionKey", "Test_OrderDataMSSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderChangeHoldBookingCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
        }

        [Fact]
        public async Task Handler_ShouldReturnError_WhenIncorrectCardNumberIsPassed()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeHoldBookingReq");
            ((PaymentProcessingPaymentCardType)request.Request.PaymentFunctions.First().PaymentProcessingDetails.PaymentMethod.Item).CardNumber = "111122233334ad4ert@#";

            //Act
            var result = await _handler.Handle(new OrderChangeHoldBookingCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "708");
        }

        [Fact]
        public async Task CreatePaymentDocumentCommandHanlder_ShouldReturnSuccess()
        {
            var _createPaymentDocumentCommand = _serviceProvider.GetService<ICreatePaymentDocumentCommand>();

            //Arrange
            var request = new PaymentDocRequest()
            {
                Type = "PaymentDoc",
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
        public void USDCurrencyMappings_ShouldReturn_Success_Tests()
        {
            var orderDocument = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\OrderDocUSCurrency");
            var result = OrderResponseMapper.MapOrderResponse(orderDocument, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            Assert.NotNull(response.Order.First().OrderItem.First());
            Assert.Equal("USD", response.Order.First().TotalPrice.TotalAmount.CurCode.ToString());
            Assert.Equal("0.00", response.Order.First().TotalPrice.TotalAmount.Value.ToString());
            Assert.True(result.PaymentFunctions.All(x => x.PaymentProcessingSummary.Amount.CurCode == "USD"));
        }

        [Fact]
        public async Task TranscationAPIShouldReturnSuccess_ARC()
        {
            var _transactionCommandHandler = _serviceProvider.GetService<ITransactionCommand>();

            var transcationRequest = TestDataProvider.GetTestData<TransactionServiceRequest>("NDCOrder\\OrderChange\\TransactionRequest");

            ConfigReader.Set("ARCPaymentMSUrl", "https://Testresources.virginatlantic.com/dev/ARCPayment/");
            ConfigReader.Set("ARCPaymentMSSubscriptionKey", "Test_ARCPaymentMSSubscriptionKey");

            var result = await _transactionCommandHandler.Handle(transcationRequest);

            Assert.NotNull(result);
            Assert.Equal("f4da1759-68ba-49fb-bd5e-3bfc278f196b", result.RequestId);
            Assert.Equal("SUCCESS", result.Status);

        }

        [Fact]
        public void ARC_ResponseMapper_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderCreate\\ArcOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            Assert.NotNull(response.Order.First().OrderItem.First());
            Assert.Equal("177.00", response.Order.First().OrderItem.First().Price.BaseAmount.Value.ToString());
            Assert.Equal("USD", response.Order.First().OrderItem.First().Price.BaseAmount.CurCode.ToString());
        }

        [Fact]
        public async Task OrderChangeShouldReturnErrorWhenSameSsrRequested()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReqSSR");

            ConfigReader.Set("OrderDataMSEndpoint", "https://Testresources.virginatlantic.com/dev/orderdata/orders/");
            ConfigReader.Set("OrderDataMSSubscriptionKey", "Test_OrderDataMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");
            ConfigReader.Set("ConfigurationMSBSPMarketCurrencyEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/BSPMarketCurrency");
            ConfigReader.Set("ConfigurationMSSubscriptionKey", "Test_ConfigurationMSSubscriptionKey"); 
            ConfigReader.Set("FlightServicesMSUrl", "https://Testresources.virginatlantic.com/dev/flightservicesmicroservice/flightServiceOffer/");
            ConfigReader.Set("FlightServicesMSSubscriptionKey", "Test_FlightServicesMSSubscriptionKey");
            ConfigReader.Set("OrderSearchMSUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderSearch");
            ConfigReader.Set("OrderSearchMSSubscriptionKey", "Test_OrderSearchMSSubscriptionKey");
            ConfigReader.Set("BrandMSUrl", "https://Testresources.virginatlantic.com/dev/brandmicroservice/brand");
            ConfigReader.Set("BrandMSSubscriptionKey", "Test_BrandMSSubscriptionKey");
            ConfigReader.Set("IsIfgIntegrated", "false");
            ConfigReader.Set("ManageCustomermsUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderManagement/order/customers");
            ConfigReader.Set("OrderChangeMSSubscriptionKey", "Test_OrderChangeMSSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderChangeHoldBookingCommand() { OrderChangeRQ = request }, new CancellationToken());

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.NotNull(result);
            Assert.Contains(items, x => x.Code == "185");
        }
    }
}

