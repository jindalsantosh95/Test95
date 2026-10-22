using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Offer.Application.Query.AirShopping;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Interface;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.AirShoppingMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Validations;
using VAA.EAPI.NDC.Offer.Validations.AirShopping;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.AirShopping
{
    public class AirShoppingQueryHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostEnvironment _hostEnvironment;

        public AirShoppingQueryHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };

            _serviceProvider = new ServiceCollection().AddLogging()
                .AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<AirShoppingQueryHandler>();
                    cfg.RegisterServicesFromAssemblyContaining<ValidationMSCommandHandler>();
                    cfg.RegisterServicesFromAssemblyContaining<BspMarketCurrencyQueryHandler>(); 
                    cfg.RegisterServicesFromAssemblyContaining<BspMarketPCCCurrencyQueryHandler>();
                    cfg.RegisterServicesFromAssemblyContaining<FareFamilyQueryHandler>(); 
                     cfg.RegisterServicesFromAssemblyContaining<FlexiDateSearchMSQuery>();
                })
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHostEnvironment>(_hostEnvironment)
                .AddSingleton<IitaFareTypeCode, ItaFareTypeCodeQueryHandler>()
                .AddSingleton<ITrackAnalyticsEventCommandHandler>(new Mock<ITrackAnalyticsEventCommandHandler>().Object)
                .AddSingleton<ITtlRules, TtlRulesQueryHandler>()
                .AddSingleton<ITtlEnabledCity, TtlEnabledCityQueryHandler>()
                .AddSingleton<ICorporateQuery, CorporateQueryHandler>()
                .AddTransient<IAirShoppingRequestValidations<IATA_AirShoppingRS>,AirShoppingRequestValidations<IATA_AirShoppingRS>>()
                .AddTransient<IGenericDataMapper, GenericDataMapper>()
                .AddSingleton<AirShoppingQueryHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public void Handler_ShouldBeResolved_From_ServiceProvider()
        {
            var handler = _serviceProvider.GetService<AirShoppingQueryHandler>();
            Assert.NotNull(handler);
        }

        [Fact]
        public async Task MapToErrorAirshoppingRs_Valid_Response()
        {
            //Arrange           
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\AirShopping\\AirShoppingRQ");
            request.DistributionChain.First().ParticipatingOrg = null;
            var subscriptionKey = "1234";

            //Act
            var _handler = _serviceProvider.GetService<AirShoppingQueryHandler>();
            var result = await _handler.Handle(new AirShoppingQuery(request, subscriptionKey), CancellationToken.None);

            //Assert
            var items = (ErrorType[])result.Value.Value.Items;
            Assert.Contains(items, x => x.Code == "912");
            Assert.NotNull(result);           
        }

        [Fact]
        public void Validate_ErrorFor_Null_ParticipatingOrgObject()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\AirshoppingRQ");
            request.DistributionChain[0].ParticipatingOrg = null;

            var validationErrors = RequestValidations.ValidateRequestAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Count > 0);
            Assert.Equal("912_1", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_Empty_ParticipatingOrgId()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\AirshoppingRQ");
            request.DistributionChain[0].ParticipatingOrg.OrgID = string.Empty;

            var validationErrors = RequestValidations.ValidateRequestAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Count > 0);
            Assert.Equal("912_1", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_EmptySalesAgentIdOfSalesAgent()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\AirshoppingRQ");
            request.DistributionChain[0].SalesAgent = new() { SalesAgentID = string.Empty };

            var validationErrors = RequestValidations.ValidateRequestAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Count > 0);
            Assert.Equal("912_1", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_EmptySalesBranchIdOfSalesBranch()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\AirshoppingRQ");
            request.DistributionChain[0].SalesBranch = new() { SalesBranchID = string.Empty };

            var validationErrors = RequestValidations.ValidateRequestAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Count > 0);
            Assert.Equal("912_1", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_EmptyOrdinal()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\AirshoppingRQ");
            request.DistributionChain[0].Ordinal = string.Empty;

            var validationErrors = RequestValidations.ValidateRequestAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Count > 0);
            Assert.Equal("912_24", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_ZeroOrdinal()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\AirshoppingRQ");
            request.DistributionChain[0].Ordinal = "0";

            var validationErrors = RequestValidations.ValidateRequestAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Count > 0);
            Assert.Equal("912_24", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_ErrorFor_SellerOrdinalNotStartingWithOne()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\AirshoppingRQ");
            request.DistributionChain[0].Ordinal = "2";

            var validationErrors = RequestValidations.ValidateRequestAgentDetails(request.DistributionChain);

            Assert.True(validationErrors.Count > 0);
            Assert.Equal("912_24", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_NoErrorFor_SellerOrdinalStartingWithOne()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\AirshoppingRQ");
            request.DistributionChain[0].Ordinal = "123";

            var validationErrors = RequestValidations.ValidateRequestAgentDetails(request.DistributionChain);

            Assert.Empty(validationErrors);
        }

        [Fact]
        public async Task ValidateBaggageAssociationsForOfferId()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\AirShopping\\AirshoppingBaggageRQ");
            var subscriptionKey = "1234";

            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");
            ConfigReader.Set("ConfigurationMSOcpKey", "Test_ConfigurationMSOcpKey"); 
            ConfigReader.Set("ConfigurationMSBSPMarketCurrencyEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/BSPMarketCurrency");
            ConfigReader.Set("ConfigurationMSPccCurrencyEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/PccCurrency");
            ConfigReader.Set("ConfigurationMSITAFareTypeCodeEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/ITAPaxTypeCodes");
            ConfigReader.Set("ConfigurationMSFareFamilyEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/FareFamily");
            ConfigReader.Set("ConfigurationMSTtlEnabledCityEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/TtlEnabledCity");
            ConfigReader.Set("ConfigurationMSTtlRulesEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/TtlRules");
            ConfigReader.Set("AirshoppingOfferEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offers");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");

            //Act
            var _handler = _serviceProvider.GetService<AirShoppingQueryHandler>();
            var result = await _handler.Handle(new AirShoppingQuery(request, subscriptionKey), CancellationToken.None);

            //Assert
            var items = (AirShoppingResponseType[])result.Value.Value.Items;
            Assert.NotNull(result);
            Assert.NotNull(items.First().OffersGroup.CarrierOffers.First().Offer.First().BaggageAssociations);
            Assert.Equal("BAGA1", items.First().OffersGroup.CarrierOffers.First().Offer.First().BaggageAssociations.First().BaggageAllowanceRefID.First());
        }

        [Fact]
        public async Task ValidateBaggageAssociationsForAIOperatingCarriesOfferId()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\AirShopping\\AirshoppingBaggageRQ");
            var subscriptionKey = "1234";

            ConfigReader.Set("ValidationMSUrl", "https://testresources.virginatlantic.com/dev/validationmicroservice/Agency/Validation");
            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("AgentRegistrationEnvironment", "PREPROD");
            ConfigReader.Set("ConfigurationMSOcpKey", "Test_ConfigurationMSOcpKey");
            ConfigReader.Set("ConfigurationMSBSPMarketCurrencyEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/BSPMarketCurrency");
            ConfigReader.Set("ConfigurationMSPccCurrencyEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/PccCurrency");
            ConfigReader.Set("ConfigurationMSITAFareTypeCodeEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/ITAPaxTypeCodes");
            ConfigReader.Set("ConfigurationMSFareFamilyEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/FareFamily");
            ConfigReader.Set("ConfigurationMSTtlEnabledCityEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/TtlEnabledCity");
            ConfigReader.Set("ConfigurationMSTtlRulesEndpoint", "https://Testresources.virginatlantic.com/dev/ConfigurationMicroservice/TtlRules");
            ConfigReader.Set("AirshoppingOfferEndpoint", "https://Testresources.virginatlantic.com/dev/Offermicroservice/offersAI");
            ConfigReader.Set("OfferMSSubscriptionKey", "Test_OfferMSSubscriptionKey");

            //Act
            var _handler = _serviceProvider.GetService<AirShoppingQueryHandler>();
            var result = await _handler.Handle(new AirShoppingQuery(request, subscriptionKey), CancellationToken.None);

            //Assert
            var items = (AirShoppingResponseType[])result.Value.Value.Items;
            Assert.NotNull(result);
            Assert.NotNull(items.First().OffersGroup.CarrierOffers.First().Offer.First().BaggageAssociations);
            Assert.Equal("BAGA1", items.First().OffersGroup.CarrierOffers.First().Offer.First().BaggageAssociations.First().BaggageAllowanceRefID.First());
        }
    }
}
