using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
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
    public class OrderChangeEvenExchangeCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly OrderChangeEvenExchangeCommandHandler _handler;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ReshopDisruptionCommandStub _orderChangeReshopDisruptionCommand;

        public OrderChangeEvenExchangeCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };
            _orderChangeReshopDisruptionCommand = new ReshopDisruptionCommandStub();

           _serviceProvider = new ServiceCollection().AddLogging()
                .AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<OrderChangeEvenExchangeCommandHandler>();
                })

                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHostEnvironment>(_hostEnvironment)
                .AddSingleton<IBrandMSCommand, BrandMSCommandHandler>()
                .AddSingleton<IBrandMSPTCCommand, BrandMSCommandHandlerPTC>()
                .AddSingleton<PaymentIFGMSAuthorizeCommandHandler>()
                .AddSingleton<PaymentIFGMSExchangeCommandHandler>()
                .AddSingleton<OrderDataMSQueryHandler>()
                .AddSingleton<OrderSearchMSCommandHandler>()
                .AddSingleton<ManageCustomerRepoCommandHandler>()
                .AddSingleton<ICreatePaymentDocumentCommand, CreatePaymentDocumentCommandHandler>()
                .AddSingleton<PublishOrderDocMSCommandHandler>()
                .AddSingleton<PaymentIFGMSAuthorizeCancelCommandHandler>()
                .AddSingleton<IFGAgentFopLookUpCommandHandler>()
                .AddSingleton<ValidationMSCommandHandler>()
                .AddSingleton<AirportDetailsQueryHandler>()
                .AddSingleton<ISendOrderEmailCommand, SendOrderEmailCommandHandler>()
                .AddSingleton<ICurrencyConversionQuery, CurrencyConversionQueryHandler>()
                .AddSingleton<ICheckAgentStatusQuery, CheckAgentStatusQueryHandler>()
                .AddSingleton<ITrackAnalyticsEventCommandHandler, TrackAnalyticsEventCommandHandlerStub>()
                .AddSingleton<ITransactionCommand, TransactionCommandHandler>()
                .AddSingleton<PaymentMSRetrieveQueryHandler>()
                .AddSingleton<IARCOrderDataMSQuery, ARCOrderDataMSQueryHandler>()
                .AddSingleton<IOrderChangeReshopDisruptionCommand>(_orderChangeReshopDisruptionCommand)
                .AddSingleton<OrderChangeEvenExchangeCommandHandler>() 
               .BuildServiceProvider();

            _handler = _serviceProvider.GetService<OrderChangeEvenExchangeCommandHandler>();
        }

        [Fact]
        public void Handler_ShouldBeResolved_From_ServiceProvider()
        {
            var handler = _serviceProvider.GetService<OrderChangeEvenExchangeCommandHandler>();
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
            var result = await _handler.Handle(new OrderChangeEvenExchangeCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task OrderChangeEvenExchange_WhenDisruptedOfferHasOTPayment_ContinuesOnTheExistingOrderChangeFlow()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");
            const string offerId = "91419000-0000-0000-0000-000000000000|paymentnotallowed001";

            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().OfferRefID = offerId;
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = offerId;
            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");

            var result = await _handler.Handle(new OrderChangeEvenExchangeCommand { OrderChangeRQ = request }, CancellationToken.None);

            var errors = (ErrorType[])result.Value.Items;
            Assert.Contains(errors, error => error.Code == "913");
            Assert.DoesNotContain(errors, error => error.Code == "914");
            Assert.Equal(0, _orderChangeReshopDisruptionCommand.CallCount);
        }

        [Fact]
        public async Task OrderChangeEvenExchange_WhenDisruptedOfferHasNoPaymentFunctionsAndInvalidOffer_ReturnsInvalidOfferError()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");
            const string offerId = "91419000-0000-0000-0000-000000000000|paymentnotallowed001";

            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().OfferRefID = offerId;
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = offerId;
            request.Request.PaymentFunctions = null;
            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");

            var result = await _handler.Handle(new OrderChangeEvenExchangeCommand { OrderChangeRQ = request }, CancellationToken.None);

            var errors = Assert.IsType<ErrorType[]>(result.Value.Items);
            Assert.Contains(errors, error => error.Code == "913");
            Assert.Equal(0, _orderChangeReshopDisruptionCommand.CallCount);
        }

        [Fact]
        public async Task OrderChangeEvenExchange_WhenDisruptedOfferHasMixedPaymentFunctions_ReturnsDisruptionPaymentError()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");
            const string offerId = "91419000-0000-0000-0000-000000000000|paymentnotallowed001";

            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().OfferRefID = offerId;
            ((AcceptSelectedQuotedOfferListType)request.Request.ChangeOrderChoice.Item).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID = offerId;
            var nonOtPaymentFunction = Json.Deserialize<PaymentFunctionsChangeType>(Json.Serialize(request.Request.PaymentFunctions.First()));
            nonOtPaymentFunction.PaymentProcessingDetails.Amount.Value = 1;
            request.Request.PaymentFunctions = request.Request.PaymentFunctions.Append(nonOtPaymentFunction).ToArray();
            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");

            var result = await _handler.Handle(new OrderChangeEvenExchangeCommand { OrderChangeRQ = request }, CancellationToken.None);

            var errors = Assert.IsType<ErrorType[]>(result.Value.Items);
            Assert.Contains(errors, error => error.Code == "914");
            Assert.Equal(0, _orderChangeReshopDisruptionCommand.CallCount);
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
            var result = await _handler.Handle(new OrderChangeEvenExchangeCommand() { OrderChangeRQ = request }, CancellationToken.None);

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

            ((request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.First().OfferRefID) = "ddf28790-f0c1-47d5-9ff6-22b1cb80c504|8r4LQnL2uQ8S0h9P771ZD1002";
            ((request.Request.ChangeOrderChoice.Item as AcceptSelectedQuotedOfferListType).SelectedPricedOffer.First().SelectedOfferItem.First().OfferItemRefID) = "ddf28790-f0c1-47d5-9ff6-22b1cb80c504|8r4LQnL2uQ8S0h9P771ZD1002";

            //Act
            var result = await _handler.Handle(new OrderChangeEvenExchangeCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "913");
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
            var result = await _handler.Handle(new OrderChangeEvenExchangeCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "129");
        }

        [Fact]
        public async Task ValidateEvenOrderChangeRQ()
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
            var result = await _handler.Handle(new OrderChangeEvenExchangeCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task ValidateEvenOrderChangeRQAgainstOfferDoc()
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
            var result = await _handler.Handle(new OrderChangeEvenExchangeCommand() { OrderChangeRQ = request }, CancellationToken.None);

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
            var result = await _handler.Handle(new OrderChangeEvenExchangeCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
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
        public void Exchange_ResponseMapper_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderCreate\\FareTypeOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            Assert.NotNull(response.Order.First().OrderItem.First().FareDetail.First().FareComponent.First());
            Assert.Equal("Marine", response.Order.First().OrderItem.First().FareDetail.First().FareComponent.First().FareTypeCode);
            Assert.Equal("758", response.Order.First().OrderItem.First().FareDetail.First().FareComponent.First().NegotiatedCode);
            Assert.Equal("IY9VKDJA", response.Order.First().OrderItem.First().FareDetail.First().FareComponent.First().FareBasisCode);

        }

        [Fact]
        public void OrderChangeEvenExchange_OutBoundJourneyDateChange_ShouldReturnSuccess()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\OrderChangeSeat");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            Assert.NotNull(response.Order.First().OrderItem);
            Assert.Equal("1373.98", response.Order.First().TotalPrice.TotalAmount.Value.ToString());
            Assert.Equal("GBP", response.Order.First().TotalPrice.TotalAmount.CurCode.ToString());
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
        public async Task OrderChangeEvenExchangeCommandHandlerShouldReturnErrorForFlightNoop()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReqFlightNoop");

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
            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");
            ConfigReader.Set("OrderChangeMSUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderManagement/order/");
            ConfigReader.Set("OrderChangeMSSubscriptionKey", "Test_OrderChangeMSSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderChangeEvenExchangeCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "114");
        }

        [Fact]
        public async Task OrderChangeEvenExchangeCommandHandlerShouldReturnErrorForIAmAndCancelSegments()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReqCancelSegments");

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
            ConfigReader.Set("IFGAgentFopLookUpUrl", "https://Testresources.virginatlantic.com/DEV/validationmicroservice/IFGAgentFopLookUp");
            ConfigReader.Set("IFGAgentFOPLookUpSubscriptionKey", "Test_IFGAgentFOPLookUpSubscriptionKey");
            ConfigReader.Set("OrderChangeMSUrl", "https://Testresources.virginatlantic.com/dev/NdcOrderManagement/order/");
            ConfigReader.Set("OrderChangeMSSubscriptionKey", "Test_OrderChangeMSSubscriptionKey");

            //Act
            var result = await _handler.Handle(new OrderChangeEvenExchangeCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "911");
        }

        private sealed class ReshopDisruptionCommandStub : IOrderChangeReshopDisruptionCommand
        {
            public int CallCount { get; private set; }
            public IATA_OrderChangeRQ LastRequest { get; private set; }

            public Task<CSharpFunctionalExtensions.Result<Response<IATA_OrderViewRS>>> Handle(IATA_OrderChangeRQ request, string subscriptionKey, CancellationToken cancellationToken)
            {
                CallCount++;
                LastRequest = request;
                return Task.FromResult(CSharpFunctionalExtensions.Result.Success(new Response<IATA_OrderViewRS> { Status = Status.OK }));
            }
        }

        private sealed class TrackAnalyticsEventCommandHandlerStub : ITrackAnalyticsEventCommandHandler
        {
            public Task<bool> Handle(TrackAnalyticsEventCommand analyticsEventCommand, CancellationToken cancellationToken)
            {
                return Task.FromResult(true);
            }
        }
    }
}
