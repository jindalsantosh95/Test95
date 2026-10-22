using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Virgin.Models.Customer;
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
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper.Order;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderChange
{
    public class OrderChangeAddColCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly OrderChangeAddColCommandHandler _handler;
        private readonly IHostEnvironment _hostEnvironment;

        public OrderChangeAddColCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };

            _serviceProvider = new ServiceCollection().AddLogging()
                .AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<OrderChangeAddColCommandHandler>();
                    cfg.RegisterServicesFromAssemblyContaining<OfferMSQueryHandler>();
                    cfg.RegisterServicesFromAssemblyContaining<ValidationMSCommandHandler>();
                })

                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHostEnvironment>(_hostEnvironment)
                .AddSingleton<IBrandMSCommand, BrandMSCommandHandler>()
                .AddSingleton<IBrandMSPTCCommand, BrandMSCommandHandlerPTC>()
                .AddSingleton<PaymentIFGMSAuthorizeCommandHandler>()
                .AddSingleton<PaymentIFGMSExchangeCommandHandler>()
                .AddSingleton<ICreatePaymentDocumentCommand, CreatePaymentDocumentCommandHandler>()
                .AddSingleton<PublishOrderDocMSCommandHandler>()
                .AddSingleton<PaymentIFGMSAuthorizeCancelCommandHandler>()
                .AddSingleton<IFGAgentFopLookUpCommandHandler>()
                .AddSingleton<ISendOrderEmailCommand, SendOrderEmailCommandHandler>()
                .AddSingleton<ICurrencyConversionQuery, CurrencyConversionQueryHandler>()
                .AddSingleton<ICheckAgentStatusQuery, CheckAgentStatusQueryHandler>()
                .AddSingleton<ITrackAnalyticsEventCommandHandler>(new Mock<ITrackAnalyticsEventCommandHandler>().Object)
                .AddSingleton<ITransactionCommand, TransactionCommandHandler>()
                .AddSingleton<PaymentMSRetrieveQueryHandler>()
                .AddSingleton<IARCOrderDataMSQuery, ARCOrderDataMSQueryHandler>()
                .AddSingleton<OrderChangeAddColCommandHandler>()
                .AddSingleton<SabreOrderManagementMSCommandHandler>()
                .AddSingleton<ManageCustomerRepoCommandHandler>()
                .AddSingleton<AirportDetailsQueryHandler>()
                .AddSingleton<ValidationMSCommandHandler>()
                .AddSingleton<OfferMSQueryHandler>()
                .AddSingleton<OrderDataMSQueryHandler>()
                .AddSingleton<OrderSearchMSCommandHandler>()
                .AddSingleton<ReferenceMSCurrencyQueryHandler>()
                .AddSingleton<PaymentMSAuthorizeCommandHandler>()
                .AddSingleton<PaymentMSCaptureCommandHandler>()
                .AddSingleton<PaymentMSRefundCommandHandler>()
                .AddSingleton<PaymentIFGMSOrderCommandHandler>()
                .AddSingleton<IOrderChangeReshopConfirmHoldBookingCommand, OrderChangeReshopConfirmHoldBookingCommandHandler>()

                .BuildServiceProvider();

            _handler = _serviceProvider.GetService<OrderChangeAddColCommandHandler>();
        }

        [Fact]
        public void Handler_ShouldBeResolved_From_ServiceProvider()
        {
            var handler = _serviceProvider.GetService<OrderChangeAddColCommandHandler>();
            Assert.NotNull(handler);
        }

        [Fact]
        public async Task OrderChangeRequestValidationsTests()
        {
            //Arrange           
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");

            request.DistributionChain.First().Ordinal = null;
            request.Request.Order.OrderID = null;
            request.Request.DataLists.PaxList.First().PaxID = null;
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().OfferRefID = null;
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = null;
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().PaxRefID = Array.Empty<string>();

            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
          
            //Act
            var result = await _handler.Handle(new OrderChangeAddColCommand() { OrderChangeRQ = request }, CancellationToken.None);
           
            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task OrderChangeAddCol_WhenDisruptedOfferHasPaymentFunctions_Returns914_19()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");
            const string offerId = "91419000-0000-0000-0000-000000000000|paymentnotallowed001";

            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().OfferRefID = offerId;
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = offerId;
            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");

            var result = await _handler.Handle(new OrderChangeAddColCommand { OrderChangeRQ = request }, CancellationToken.None);

            var errors = (ErrorType[])result.Value.Items;
            Assert.Contains(errors, error => error.Code == "914" && error.DescText == "Please resubmit the request without payment functions. Payment can be completed at a later time");
        }

        [Fact]
        public async Task ValidationMSTests()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");

            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");

            request.DistributionChain.First().ParticipatingOrg.OrgID = "91281035";

            //Act
            var result = await _handler.Handle(new OrderChangeAddColCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task OfferDocNullValidation()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");

            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");

            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().OfferRefID = "ddf28790-f0c1-47d5-9ff6-22b1cb80c504|8r4LQnL2uQ8S0h9P771ZD1002";
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = "ddf28790-f0c1-47d5-9ff6-22b1cb80c504|8r4LQnL2uQ8S0h9P771ZD1002";

            //Act
            var result = await _handler.Handle(new OrderChangeAddColCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
        }

        [Fact]
        public async Task OfferDocValidations()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");

            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");

            request.Request.Order.OrderID = "VS932SXSNTY26";

            //Act
            var result = await _handler.Handle(new OrderChangeAddColCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "129");
        }

        [Fact]
        public async Task ValidateAddColUnEvenOrderChangeRQ()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");

            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");

            request.DistributionChain.First().ParticipatingOrg.Name = "Atlas Travel Services Limited";

            //Act
            var result = await _handler.Handle(new OrderChangeAddColCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task ValidateAddColUnEvenOrderChangeRQAgainstOfferDoc()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");

            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");

            request.DistributionChain.First().ParticipatingOrg.Name = "Atlas Travel Services Limited";

            //Act
            var result = await _handler.Handle(new OrderChangeAddColCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task ValidateGSTDetails()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");

            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");

            request.Request.DataLists.PaxList.First().Employer.Name = "";
            request.Request.DataLists.PaxList.First().Employer.SalesTaxRegistration.First().SalesTaxRegistrationID = null;
            request.Request.DataLists.PaxList.First().Employer.SalesTaxRegistration.First().PostalCode = null;
            request.Request.DataLists.PaxList.First().Employer.SalesTaxRegistration.First().EmailAddressText = null;
            request.Request.DataLists.PaxList.First().Employer.SalesTaxRegistration.First().CountryDialingCode = null;
            request.DistributionChain.First().ParticipatingOrg.Name = "Atlas Travel Services Limited";

            //Act
            var result = await _handler.Handle(new OrderChangeAddColCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task Handler_Exception()
        {

            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");

            //Act
            var result = await _handler.Handle(new OrderChangeAddColCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
        }

        [Fact]
        public async Task PaymentIFGAuthorizeHanlder_ShouldReturnSuccess()
        {
            var authorizeHandler = _serviceProvider.GetService<PaymentIFGMSAuthorizeCommandHandler>();

            var request = new PaymentIFGMSAuthorizeCommand()
            {
                Parties = new()
                {
                    Participants = [new() { Ordinal = "1", AgencyID = "9128103", Name = "Skyscanner" }]
                },
                PaymentDetails = new List<Payment>()
                {
                    new()
                    {
                        Amount = new()
                        {
                            MonetaryAmount = "122400",
                            BaseCurrency = "GBP2"
                        }
                    }
                },
                CustomerDetails = new List<Customer>()
                {
                    new()
                    {
                        CustId  ="ADULT_1",
                        Ptc  ="ADT"
                    }
                },
                OrderId = "VS932RVJLVA25"
            };

            ConfigReader.Set("PaymentIFGMSAuthorizeUrl", "https://Testresources.virginatlantic.com/DEV/IFGpayment/IFGAuthorizePayment");
            ConfigReader.Set("PaymentIFGMSSubscriptionKey", "Test_PaymentIFGMSSubscriptionKey");

            //Act
            var result = await authorizeHandler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("27be8079-9372-4b71-b80f-3064054f7a2f", result.PaymentDocument.TrackId);
        }

        [Fact]
        public void UsagentShouldReturnSuccess_ARC()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderCreate\\ArcOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            Assert.NotNull(response.Order.First().OrderItem.First());
            Assert.Equal("177.00", response.Order.First().OrderItem.First().Price.BaseAmount.Value.ToString());
            Assert.Equal("USD", response.Order.First().OrderItem.First().Price.BaseAmount.CurCode.ToString());
            Assert.Equal("557.00", response.Order.First().TotalPrice.TotalAmount.Value.ToString());
            Assert.Equal("USD", response.Order.First().TotalPrice.TotalAmount.CurCode.ToString());
        }

        [Fact]
        public void OrderChangeAddCollect_OutBoundJourneyDateChange_ShouldReturnSuccess()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\OrderChangeSeat");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            Assert.NotNull(response.Order.First().OrderItem);
            Assert.Equal("1373.98", response.Order.First().TotalPrice.TotalAmount.Value.ToString());
            Assert.Equal("GBP", response.Order.First().TotalPrice.TotalAmount.CurCode.ToString());
            Assert.Equal("SUCCESSFUL", result.PaymentFunctions.First().PaymentProcessingSummary.PaymentStatusCode);
            Assert.NotNull(response.Order.First().OrderItem.Any(x => x.Service.First().OrderServiceAssociation.Item is SeatOnLegType));
        }

        [Fact]
        public async Task TranscationAPIShouldReturnSuccess_ARC()
        {
            var transactionHandler = _serviceProvider.GetService<ITransactionCommand>();

            var transcationRequest = TestDataProvider.GetTestData<TransactionServiceRequest>("NDCOrder\\OrderChange\\TransactionRequest");

            ConfigReader.Set("ARCPaymentMSUrl", "https://Testresources.virginatlantic.com/dev/ARCPayment/");
            ConfigReader.Set("ARCPaymentMSSubscriptionKey", "Test_ARCPaymentMSSubscriptionKey");

            var result = await transactionHandler.Handle(transcationRequest);

            Assert.NotNull(result);
            Assert.Equal("f4da1759-68ba-49fb-bd5e-3bfc278f196b", result.RequestId);
            Assert.Equal("SUCCESS", result.Status);

        }

        [Fact]
        public async Task PaymentIFGCCAuthorizeHanlder_ShouldReturnSuccess()
        {
            var authorizeHandler = _serviceProvider.GetService<PaymentIFGMSAuthorizeCommandHandler>();

            var request = new PaymentIFGMSAuthorizeCommand()
            {
                Parties = new()
                {
                    Participants = [new() { Ordinal = "1", AgencyID = "9128103", Name = "Skyscanner" }]
                },
                PaymentDetails = new List<Payment>()
                {
                    new()
                    {
                        Amount = new()
                        {
                            MonetaryAmount = "122400",
                            BaseCurrency = "GBP2"
                        },
                        CardMethod = new()
                        {
                            CardType = "VI"
                        }
                    }
                },
                CustomerDetails = new List<Customer>()
                {
                    new()
                    {
                        CustId  ="ADULT_1",
                        Ptc  ="ADT"
                    }
                },
                OrderId = "VS932RVJLVA25"
            };

            ConfigReader.Set("PaymentIFGMSAuthorizeUrl", "https://Testresources.virginatlantic.com/DEV/IFGpayment/IFGCCAuthorizePayment");
            ConfigReader.Set("PaymentIFGMSSubscriptionKey", "Test_PaymentIFGMSSubscriptionKey");

            //Act
            var result = await authorizeHandler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("942ce0de-bf7a-487f-a13f-205c43b0748c", result.PaymentDocument.TrackId);
        }

        [Fact]
        public async Task ARCOrderDataMSShouldReturnSuccess()
        {
            var arcQuery = _serviceProvider.GetService<IARCOrderDataMSQuery>();
            
            var orderid = "VS932TYLCEJ26-ARC";

            ConfigReader.Set("OrderDataMSEndpoint", "https://Testresources.virginatlantic.com/dev/orderdata/orders/");
            ConfigReader.Set("OrderDataMSSubscriptionKey", "Test_OrderDataMSSubscriptionKey");

            var result = await arcQuery.Handle(orderid, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("9322520240373", result.ARCDocument.ARCTransactionHistory.First().PassengerTicketDetails.First().AccountableDocumentRefs.First().DocumentNumber);
            Assert.Equal("ADULT_1", result.ARCDocument.ARCTransactionHistory.First().PassengerTicketDetails.First().AccountableDocumentRefs.First().CustomerRefId);
            Assert.Equal("OrderCreate", result.ARCDocument.ARCTransactionHistory.First().PassengerTicketDetails.First().Type);
        }

        [Fact]
        public async Task PaymentAuthorizeShouldReturnSuccessforCard()
        {
            var authorizeHandler = _serviceProvider.GetService<PaymentIFGMSAuthorizeCommandHandler>();
            var request = TestDataProvider.GetTestData<PaymentIFGMSAuthorizeCommand>("NDCOrder\\OrderChange\\IFGPaymentAuthorizeRequest");

            ConfigReader.Set("PaymentIFGMSAuthorizeUrl", "https://Testresources.virginatlantic.com/DEV/IFGpayment/IFGCCAuthorizePaymentAgency");
            ConfigReader.Set("PaymentIFGMSSubscriptionKey", "Test_PaymentIFGMSSubscriptionKey");
            
            //Act
            var result = await authorizeHandler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("f39b0364-b302-402c-bb1c-ee1b6618c5fe", result.PaymentDocument.TrackId);
            Assert.Equal("82027111", result.PaymentDocument.TransactionId);
            Assert.Equal("36929", result.PaymentDocument.AmountAuthorized.Value.ToString());
        }

        [Fact]
        public async Task OrderChangeShouldRetrurnErrorforFalseIsPastIndicator()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangePartialFlownReq");

            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");
            ConfigReader.Set("BrandMSUrl", "https://Testresources.virginatlantic.com/dev/brandmicroservice/brand");
            ConfigReader.Set("BrandMSSubscriptionKey", "Test_BrandMSSubscriptionKey");
            ConfigReader.Set("OrderDataMSEndpoint", "https://Testresources.virginatlantic.com/dev/orderdata/orders/");
            ConfigReader.Set("OrderDataMSSubscriptionKey", "Test_OrderDataMSSubscriptionKey");
            ConfigReader.Set("IsIfgIntegrated", "false");
            ConfigReader.Set("ReferenceMSUrl", "https://Testresources.virginatlantic.com/dev/ReferenceMicroservice/");
            ConfigReader.Set("ReferenceMSSubscriptionKey", "Test_ReferenceMSSubscriptionKey");
            ConfigReader.Set("OrderChangeMSUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderManagement/order/");
            ConfigReader.Set("OrderChangeMSSubscriptionKey", "Test_OrderChangeMSSubscriptionKey");
            ConfigReader.Set("OrderSearchMSUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderSearch/OrderSearch");
            ConfigReader.Set("OrderSearchMSSubscriptionKey", "Test_OrderSearchMSSubscriptionKey"); 
            ConfigReader.Set("PaymentMSSubscriptionKey", "Test_PaymentMSSubscriptionKey"); 
            ConfigReader.Set("PaymentMSAuthorizeUrl", "https://Testresources.virginatlantic.com/dev/digital/orderpaymentmicroservice/AuthorizePayment");
            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");
            ConfigReader.Set("PaymentMSRefundUrl", "https://Testresources.virginatlantic.com/dev/digital/orderpaymentmicroservice/RefundPayment");

            //Act
            var result = await _handler.Handle(new OrderChangeAddColCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
        }

    }
}
