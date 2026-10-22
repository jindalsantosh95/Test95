using MediatR;
using Moq;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Offer.Application.Query.AirShopping;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.Clid;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.TTL;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.AirShopping.Mapper
{
    public class AirshoppingRQMapperTests
    {
        private readonly Mock<IMediator> _mediator;
        public AirshoppingRQMapperTests()
        {
            _mediator = new Mock<IMediator>();
        }

        [Fact]
        public async Task MapOfferRequestSuccess()
        {
            //Arrange
            var fareFamilies = TestDataProvider.GetTestData<IEnumerable<FareFamilies>>("NDCOffer\\Airshopping\\FareFamily");
            var itaPaxTypeCode = TestDataProvider.GetTestData<IEnumerable<ItaFareTypeCodeData>>("NDCOffer\\Airshopping\\FareCodeTypeList");
            var ttlRules = TestDataProvider.GetTestData<IEnumerable<TtlRulesData>>("NDCOffer\\Airshopping\\TTLRules");
            var configMSRS = TestDataProvider.GetTestData<IEnumerable<CorporateData>>("NDCOffer\\Airshopping\\ConfigMSRS")?.FirstOrDefault();
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\Return2ADT1CHD1GBE");
            var obj = new AirShoppingQuery(request, "testsubscriptionkey");

            var currencydata = new BspMarketCurrencyData() { CountryCode = "GB", LocalCurrencyCode = "" };
            var agencydetails = new AgencyValidateResponse
            {
                AgencyDetails = new()
                {
                    AccountCode = "",
                    CountryCode = "GB",
                    RequestAgencyId = "91281035",
                    RequestAgencyIdType = "IATA",
                    RequestOrgName = "TRAVEL REPUBLIC.CO.UK"
                }
            };
            var bspMarket = new Infrastructure.Model.ConfigMS.BspMarket.PccCurrencyData();

            //Act 
            var result = AirShoppingRQMapper.MapOfferRequest(obj, currencydata, bspMarket, fareFamilies, itaPaxTypeCode, agencydetails, itaPaxTypeCode.First().FareTypeCode, itaPaxTypeCode.First().FareSubTypeCode, ttlRules, configMSRS);
            Assert.NotNull(result);
            Assert.Equal("91281035", result.Parties.Participants.First().AgencyID);
            Assert.Equal("758", result.FareTypeRequest.FareTypeCode);
            Assert.Equal("Seat Only", result.FareTypeRequest.FareSubTypeCode);
        }

        [Fact]
        public async Task MapCalendarOfferRequestSuccess()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\Return2ADT1CHD1GBE");
            var configMSRS = TestDataProvider.GetTestData<IEnumerable<CorporateData>>("NDCOffer\\Airshopping\\ConfigMSRS")?.FirstOrDefault();
            var obj = new AirShoppingQuery(request, "testsubscriptionkey");
            var currencydata = new BspMarketCurrencyData() { CountryCode = "GB", LocalCurrencyCode = "" };
           
            var agencydetails = new AgencyValidateResponse
            {
                AgencyDetails = new()
                {
                    AccountCode = "",
                    CountryCode = "GB",
                    RequestAgencyId = "91281035",
                    RequestAgencyIdType = "IATA",
                    RequestOrgName = "TRAVEL REPUBLIC.CO.UK"
                }
            };
            var fareFamilies = TestDataProvider.GetTestData<IEnumerable<FareFamilies>>("NDCOffer\\Airshopping\\FareFamily");
            IEnumerable<ItaFareTypeCodeData> itaPaxTypeCode = [];
            var bspMarket = new Infrastructure.Model.ConfigMS.BspMarket.PccCurrencyData();

            //Act
            var result = AirShoppingRQMapper.MapCalendarOfferRequest(obj, currencydata, bspMarket, fareFamilies, itaPaxTypeCode, agencydetails, "", null, configMSRS);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Parties.Participants.First().Ordinal);
            Assert.Equal("Skyscanner", result.Parties.Participants.First().Name);
            Assert.Equal("ATL", result.FlightsearchRequest.SearchOriginDestinations.First().Destination);
            Assert.Equal("LHR", result.FlightsearchRequest.SearchOriginDestinations.First().Origin);
        }

        [Fact]
        public async Task GetPartiesSuccess()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\Return2ADT1CHD1GBE");
            var configMSRS = TestDataProvider.GetTestData<IEnumerable<CorporateData>>("NDCOffer\\Airshopping\\ConfigMSRS")?.FirstOrDefault();
            var obj = new AirShoppingQuery(request, "testsubscriptionkey");
            var bspMarket = new Infrastructure.Model.ConfigMS.BspMarket.PccCurrencyData();
           
            var agencydetails = new AgencyValidateResponse
            {
                AgencyDetails = new()
                {
                    AccountCode = "NDC11",
                    CountryCode = "GB",
                    RequestAgencyId = "91281035",
                    RequestAgencyIdType = "IATA",
                    RequestOrgName = "TRAVEL REPUBLIC.CO.UK"
                }
            };
            string fareTypeCode = Constants.Application.PrivateFareTypeCode;

            //Act
            var result = AirShoppingRQMapper.GetParties(obj.AirShoppingRQ, bspMarket, agencydetails, fareTypeCode, configMSRS);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("Skyscanner", result.Participants.First().Name);
            Assert.Equal("91281035", result.Participants.First().AgencyID);
            Assert.Equal("1", result.Participants.First().Ordinal);
            Assert.Equal("NDC11", result.Participants.First().AccountCode);
        }


        [Fact]
        public void CalculateTTL_InvalidFareTypeCode_ReturnsEmpty()
        {
            var rules = new List<TtlRulesData>
            {
                new() { FareTypeCode = 758, FareSubTypeCode = "Tour Operator", MinDepartureDate = 0, MaxDepartureDate = 10 }
            };

            var result = AirShoppingRQMapper.CalculateTTL(DateTime.UtcNow.AddDays(5), "abc", "Tour Operator", rules);
            Assert.Empty(result);
        }

        [Fact]
        public void CalculateTTL_SelectsRuleWithinDepartureWindow()
        {
                    var ttlRules = new List<TtlRulesData>
            {
                new() { FareTypeCode = 758, FareSubTypeCode = "Tour Operator", MinDepartureDate = 1, MaxDepartureDate = 10, TTLBeforeDeparture = 5 }
            };
            var departureDate = DateTime.UtcNow.AddDays(5);

            var ttlInfo = AirShoppingRQMapper.CalculateTTL(departureDate, "758", "Tour Operator", ttlRules).FirstOrDefault();

            Assert.NotNull(ttlInfo);
            Assert.Equal(departureDate.AddDays(-5).ToString(Constants.Application.ZuluDateTimeFormat), ttlInfo.TtlDate);
        }

        [Theory]
        [InlineData("5", "189C", true, "13EC", false, "ECONOMY-DELIGHT-LIGHT")]
        [InlineData("4", "13EC", false, "", false, "COMFORT-PLUS-PREMIUM-LIGHT")]
        [InlineData("2", "13EC", false, "", false, "BUS-FIRST-LIGHT")]
        public void PopulateFareFamily_ReturnsConfiguredLightBrandForPublishedFare(string cabinCode, string taxonomyCode, bool includeInd, string additionalTaxonomyCode, bool additionalIncludeInd, string expectedFareBrand)
        {
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\Return2ADT1CHD1GBE");
            var fareFamilies = TestDataProvider.GetTestData<IEnumerable<FareFamilies>>("NDCOffer\\Airshopping\\FareFamily");

            request.Request.OfferCriteria.FareCriteria.First().FareTypeCode = ["70J"];
            request.Request.OfferCriteria.ServiceCriteria = string.IsNullOrEmpty(additionalTaxonomyCode)
                ? [new ServiceCriteriaType { IncludeInd = includeInd, TaxonomyCode = [taxonomyCode] }]
                : [
                    new ServiceCriteriaType { IncludeInd = includeInd, TaxonomyCode = [taxonomyCode] },
                    new ServiceCriteriaType { IncludeInd = additionalIncludeInd, TaxonomyCode = [additionalTaxonomyCode] }
                ];

            var result = AirShoppingRQMapper.PopulateFareFamily([new CabinType { CabinCode = cabinCode }], request, fareFamilies);

            Assert.Equal(expectedFareBrand, Assert.Single(result));
        }

        [Fact]
        public void PopulateFareFamily_RejectsMultipleTaxonomiesWithoutAConfiguredCombination()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\Return2ADT1CHD1GBE");
            var fareFamilies = TestDataProvider.GetTestData<IEnumerable<FareFamilies>>("NDCOffer\\Airshopping\\FareFamily");

            request.Request.OfferCriteria.FareCriteria.First().FareTypeCode = ["70J"];
            request.Request.OfferCriteria.ServiceCriteria =
            [
                new ServiceCriteriaType { IncludeInd = false, TaxonomyCode = ["189C"] },
                new ServiceCriteriaType { IncludeInd = true, TaxonomyCode = ["13EC"] }
            ];

            Assert.Throws<Exception>(() => AirShoppingRQMapper.PopulateFareFamily([new CabinType { CabinCode = "5" }], request, fareFamilies));
        }

        [Theory]
        [InlineData("731", "ECONOMY-DELIGHT|COMFORT-PLUS-PREMIUM-ECON|BUS-FIRST")]
        [InlineData("730", "ECON-DELIGHT-FLEXIBLE|COMFORT-PLUS-PREMIUM-FLEXIBLE|BUS-FIRST-FLEXIBLE")]
        [InlineData("", "ECONOMY-DELIGHT|ECON-DELIGHT-FLEXIBLE|COMFORT-PLUS-PREMIUM-ECON|COMFORT-PLUS-PREMIUM-FLEXIBLE|BUS-FIRST|BUS-FIRST-FLEXIBLE")]
        public void PopulateFareFamily_ReturnsConfiguredLegacyFareFamiliesWithoutCabin(string fareTypeCode, string expectedFareBrands)
        {
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\Return2ADT1CHD1GBE");
            var fareFamilies = TestDataProvider.GetTestData<IEnumerable<FareFamilies>>("NDCOffer\\Airshopping\\FareFamily");

            request.Request.OfferCriteria.FareCriteria.First().FareTypeCode = [fareTypeCode];
            request.Request.OfferCriteria.ServiceCriteria =
            [
                new ServiceCriteriaType { IncludeInd = true, TaxonomyCode = ["189C"] },
                new ServiceCriteriaType { IncludeInd = true, TaxonomyCode = ["13EC"] }
            ];

            var result = AirShoppingRQMapper.PopulateFareFamily(null, request, fareFamilies);

            Assert.Equal(expectedFareBrands.Split('|'), result);
        }

        [Theory]
        [InlineData("189C", true, "ECONOMY-DELIGHT|ECON-DELIGHT-FLEXIBLE")]
        [InlineData("13EC", false, "ECONOMY-LIGHT")]
        public void PopulateFareFamily_KeepsLegacyEconomyMappingsForSingleTaxonomy(string taxonomyCode, bool includeInd, string expectedFareBrands)
        {
            var request = TestDataProvider.GetXmlTestData<IATA_AirShoppingRQ>("NDCOffer\\Airshopping\\Return2ADT1CHD1GBE");
            var fareFamilies = TestDataProvider.GetTestData<IEnumerable<FareFamilies>>("NDCOffer\\Airshopping\\FareFamily");

            request.Request.OfferCriteria.FareCriteria.First().FareTypeCode = ["70J"];
            request.Request.OfferCriteria.ServiceCriteria = [new ServiceCriteriaType { IncludeInd = includeInd, TaxonomyCode = [taxonomyCode] }];

            var result = AirShoppingRQMapper.PopulateFareFamily([new CabinType { CabinCode = "5" }], request, fareFamilies);

            Assert.Equal(expectedFareBrands.Split('|'), result);
        }

    }
}
