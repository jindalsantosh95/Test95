using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Offer.Application.Query.OrderQuote;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote;
using VAA.EAPI.NDC.Offer.Application.Query.OrderReshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderQuoteRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Validations.OrderQuote;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderQuote
{
    public class OrderQuoteQueryHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostEnvironment _hostEnvironment;

        public OrderQuoteQueryHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };

            _serviceProvider = new ServiceCollection().AddLogging()
                .AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<OrderQuoteQueryHandler>();
                })
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHostEnvironment>(_hostEnvironment)
                .AddSingleton<ITrackAnalyticsEventCommandHandler>(new Mock<ITrackAnalyticsEventCommandHandler>().Object)
                .AddTransient<IGenericDataMapper, GenericDataMapper>()
                .AddSingleton<IBrandMSPTCCommand, BrandMSCommandHandlerPTC>()
                .AddSingleton<IBrandMSCommand, BrandMSCommandHandler>()
                .AddSingleton<OrderQuoteQueryHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public void Handler_ShouldBeResolved_From_ServiceProvider()
        {
            var handler = _serviceProvider.GetService<OrderQuoteQueryHandler>();
            Assert.NotNull(handler);
        }


        [Fact]
        public async Task MapToErrorOrderQuoteRs_Valid_Response()
        {
            //Arrange           
            var request = TestDataProvider.GetXmlTestData<IATA_OrderQuoteRQ>("NDCOffer\\OrderQuote\\OrderQuoteRQ");
            request.DistributionChain.First().ParticipatingOrg = null;
            var subscriptionKey = "1234";

            //Act
            var _handler = _serviceProvider.GetService<OrderQuoteQueryHandler>();
            var result = await _handler.Handle(new OrderQuoteQuery(request, subscriptionKey), CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "914");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task OrderQuoteResponse_Valid_Response()
        {
            //Arrange           
            var request = TestDataProvider.GetXmlTestData<IATA_OrderQuoteRQ>("NDCOffer\\OrderQuote\\OrderQuoteRQBaggage");
            var subscriptionKey = "1234";

            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");
            ConfigReader.Set("ConfigurationMSOcpKey", "Test_ConfigurationMSOcpKey");
            ConfigReader.Set("ConfigurationMSBSPMarketCurrencyEndpoint", "https://Testresources.virginatlantic.com/sit/ConfigurationMicroservice/BSPMarketCurrency");
            ConfigReader.Set("ConfigurationMSUpsellOffersEndpoint", "https://Testresources.virginatlantic.com/sit/ConfigurationMicroservice/UpsellOffers");
            ConfigReader.Set("OfferSearchEndPoint", "https://Testresources.virginatlantic.com/sit/Offermicroservice/offers/");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");
            ConfigReader.Set("CheckAgentTktAuthority", "false"); 
            ConfigReader.Set("OrderDataMSEndpoint", "https://Testresources.virginatlantic.com/sit/orderdata/orders/");
            ConfigReader.Set("OrderDataMSSubscriptionKey", "Test_OrderDataMSSubscriptionKey");
            ConfigReader.Set("OrderQuoteEndpoint", "https://testresources.virginatlantic.com/sit/OfferMicroservice/Order/Reprice");
            ConfigReader.Set("BrandMSSubscriptionKey", "Test_BrandMSSubscriptionKey");
            ConfigReader.Set("BrandMSUrl", "https://testresources.virginatlantic.com/sit/brandmicroservice/brand");

            //Act
            var _handler = _serviceProvider.GetService<OrderQuoteQueryHandler>();
            var result = await _handler.Handle(new OrderQuoteQuery(request, subscriptionKey), CancellationToken.None);

            //Assert
            var items = (OrderReshopResponseType[])result.Value.Value.Items;
            var requotedOffer = items.First().ReshopResults.Item as ReshopOffersType;
            Assert.NotNull(result);
            Assert.NotNull(requotedOffer);
            Assert.NotNull(requotedOffer.RequotedOffer.BaggageAssociations);
            Assert.Equal("BAGA1", requotedOffer.RequotedOffer.BaggageAssociations.First().BaggageAllowanceRefID.First());

        }

        [Fact]
        public async Task OrderQuoteResponse_DoesNotReturnRefundWarning_WhenReshopIsDisrupted()
        {
            var offerResponse = TestDataProvider.GetTestData<Infrastructure.Model.OfferMS.OfferResponse>("NDCOffer\\OrderQuote\\OfferResponse");
            var pricing = offerResponse.Offers.First(offer => offer.Selected).OfferItems.First().OfferitemPricing.First();
            pricing.RefundPrice = [new RefundPrice { Amount = pricing.SalePrice.First().Amount }];

            Assert.Contains(
                OrderQuoteResponseValidations.ValidateQuoteRefundOffer(offerResponse, _hostEnvironment),
                warning => warning.Code == "745");

            ConfigReader.Set("ConfigurationMSOcpKey", "Test_ConfigurationMSOcpKey");
            ConfigReader.Set("ConfigurationMSBSPMarketCurrencyEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/BSPMarketCurrency");
            ConfigReader.Set("ConfigurationMSBSPMarketPaymentEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/BSPMarketPayment");

            var validationResponse = new ValidationRS
            {
                ValidateResponse = new AgencyValidateResponse
                {
                    AgencyDetails = new AgencyDetails { CountryCode = "GB", RequestAgencyIdType = "IATA" },
                    AgencyPermissions = new AgencyPermissions()
                }
            };

            var handler = _serviceProvider.GetRequiredService<OrderQuoteQueryHandler>();
            var result = await handler.OrderQuoteResponse(
                isHoldBookingReshop: false,
                isReshopDisruption: true,
                offerResponse: offerResponse,
                orderRepriceQuery: new OrderRepriceQuery { IsHeldBooking = false },
                offerDocument: offerResponse,
                cancellationToken: CancellationToken.None,
                analyticsEventCommand: new TrackAnalyticsEventCommand(),
                validationMSRS: validationResponse);

            var orderReshopResponse = Assert.IsType<OrderReshopResponseType>(result.Value.Value.Items.Single());
            Assert.DoesNotContain(orderReshopResponse.Warning ?? [], warning => warning.Code == "745");
        }
    }
}
