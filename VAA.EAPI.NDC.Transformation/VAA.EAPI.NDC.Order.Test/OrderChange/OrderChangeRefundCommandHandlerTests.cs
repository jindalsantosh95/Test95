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
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
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
    public class OrderChangeRefundCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly OrderChangeRefundCommandHandler _handler;
        private readonly IHostEnvironment _hostEnvironment;

        public OrderChangeRefundCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };

            _serviceProvider = new ServiceCollection().AddLogging()
                 .AddMediatR(cfg =>
                 {
                     cfg.RegisterServicesFromAssemblyContaining<OrderChangeRefundCommandHandler>();
                     cfg.RegisterServicesFromAssemblyContaining<OfferMSQueryHandler>();
                     cfg.RegisterServicesFromAssemblyContaining<ValidationMSCommandHandler>();
                 })

                 .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                 .AddSingleton<IHttpConnector, HttpConnector>()
                 .AddSingleton<IHostEnvironment>(_hostEnvironment)
                 .AddSingleton<IBrandMSCommand, BrandMSCommandHandler>()
                 .AddSingleton<IBrandMSPTCCommand, BrandMSCommandHandlerPTC>()
                 .AddSingleton<PaymentIFGMSExchangeCommandHandler>()
                 .AddSingleton<PaymentIFGMSOrderRefundCommandHandler>()
                 .AddSingleton<ICreatePaymentDocumentCommand, CreatePaymentDocumentCommandHandler>()
                 .AddSingleton<PublishOrderDocMSCommandHandler>()
                 .AddSingleton<ISendOrderEmailCommand, SendOrderEmailCommandHandler>()
                 .AddSingleton<ICurrencyConversionQuery, CurrencyConversionQueryHandler>()
                 .AddSingleton<ITrackAnalyticsEventCommandHandler>(new Mock<ITrackAnalyticsEventCommandHandler>().Object)
                 .AddSingleton<PaymentMSRetrieveQueryHandler>()
                 .AddSingleton<OrderChangeRefundCommandHandler>()
                 .AddSingleton<ValidationMSCommandHandler>()
                 .AddSingleton<OfferMSQueryHandler>()
                 .AddSingleton<OrderDataMSQueryHandler>()
                 .AddSingleton<IOrderChangeReshopHoldBookingCommand, OrderChangeReshopHoldBookingCommandHandler>()
                 .AddSingleton<SabreOrderManagementMSCommandHandler>()
                 .AddSingleton<ManageCustomerRepoCommandHandler>()
                 .AddSingleton<AirportDetailsQueryHandler>()

                .BuildServiceProvider();

            _handler = _serviceProvider.GetService<OrderChangeRefundCommandHandler>();
        }

        [Fact]
        public void Handler_ShouldBeResolved_From_ServiceProvider()
        {
            var handler = _serviceProvider.GetService<OrderChangeRefundCommandHandler>();
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
            var result = await _handler.Handle(new OrderChangeRefundCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
        }

        [Fact]
        public async Task ValidationMSTests()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeRefundReq");

            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");

            request.DistributionChain.First().ParticipatingOrg.OrgID = "91281035";

            //Act
            var result = await _handler.Handle(new OrderChangeRefundCommand() { OrderChangeRQ = request }, CancellationToken.None);

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
            var result = await _handler.Handle(new OrderChangeRefundCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "913");
        }

        [Fact]
        public async Task OfferDocValidations()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeRefundReq");

            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");

            request.Request.Order.OrderID = "VS932SXSNTY26";

            //Act
            var result = await _handler.Handle(new OrderChangeRefundCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "129");
        }

        [Fact]
        public async Task ValidateRefundUnEvenOrderChangeRQ()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeRefundReq");

            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");

            request.DistributionChain.First().ParticipatingOrg.Name = "Atlas Travel Services Limited";

            //Act
            var result = await _handler.Handle(new OrderChangeRefundCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task ValidateRefundUnEvenOrderChangeRQAgainstOfferDoc()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeRefundReq");

            ConfigReader.Set("OfferMSDocEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");

            request.DistributionChain.First().ParticipatingOrg.Name = "Atlas Travel Services Limited";

            //Act
            var result = await _handler.Handle(new OrderChangeRefundCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task ValidateGSTDetails()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeRefundReq");

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
            var result = await _handler.Handle(new OrderChangeRefundCommand() { OrderChangeRQ = request }, CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Items;
            Assert.Contains(items, x => x.Code == "368");
        }

        [Fact]
        public async Task PaymentIFGHanlder_ShouldReturnSuccess()
        {
            var exchangeHandler = _serviceProvider.GetService<PaymentIFGMSExchangeCommandHandler>();

            var request = new PaymentIFGMSExchangeCommand()
            {
                Parties = new()
                {
                    Participants = [new() { Ordinal ="1", AgencyID = "9128103", Name = "Skyscanner"}]
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

            ConfigReader.Set("PaymentIFGMSExchangeUrl", "https://Testresources.virginatlantic.com/DEV/IFGpayment/IFGPaymentOrderExchange");
            ConfigReader.Set("PaymentIFGMSSubscriptionKey", "Test_PaymentIFGMSSubscriptionKey");

            //Act 
            var result = await exchangeHandler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("27be8079-9372-4b71-b80f-3064054f7a2f",result.PaymentDocument.TrackId);
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
    }
}