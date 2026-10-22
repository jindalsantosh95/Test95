using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Search.Tests.NewBookFlow.Microservices.OfferSearch;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.Microservices.Book
{
    public class SeatLopa : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;
    private readonly TestConfig _testConfig;

        public SeatLopa(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _testConfig = new TestConfig();
            _testConfig.Search.Passengers.AddPassengers(adults: 0, 1, 1, 1);
        }

        [Theory]
        [Trait("Category", Traits.BookMicroserviceRegression)]
        [InlineData("351", "L5G", "VIRGIN ATLANTIC A350-1000 with booth")]
        [InlineData("351", "L5H", "VIRGIN ATLANTIC A350-1000 with Loft")]
        [InlineData("333", "L3D", "VIRGIN ATLANTIC A330-300")]
        [InlineData("339", "L39", "VIRGIN ATLANTIC A330- neo")]
        [InlineData("789", "L9E", "VIRGIN ATLANTIC B787-900")]
        public async Task Lopa_Connectivity_Virgin(string fleetTypeCode, string equipmentCode, string aircraftModel)
        {
            //Seat Lopa template for Virgin flights
            HttpResponseMessage lopaResponse = MicroserviceEndpoint.SeatLopa(fleetTypeCode, equipmentCode).EnsureIsSuccessStatusCode();

            dynamic lopaResponseResult = JsonConvert.DeserializeObject<dynamic>(await lopaResponse.Content.ReadAsStringAsync());

            Assert.NotNull(lopaResponseResult);

            //Assertion method
            MicroserviceAssertions.LopaSeatAssertions(lopaResponseResult, equipmentCode, fleetTypeCode);
        }

        [Fact]
        [Trait("Category", Traits.BookMicroserviceRegression)]
        public async Task Lopa_E2E_withOffer_Virgin()
        {
            //Generating offer search request
            var offerSearchRequest = Search.Tests.NewBookFlow.Microservices.MicroserviceRequestBuilder.OfferSearchRequest(_testConfig);

            // post offer search micro service
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoint.OfferSearch(offerSearchRequest);
            if (OfferSearchAssertions.CheckIfStatusCodeIsUnprocessableEntity(offerSearchResponse)) return;

            offerSearchResponse.EnsureIsSuccessStatusCode();
            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(await offerSearchResponse.Content.ReadAsStringAsync());
            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            //Get offer for Virgin
            _testConfig.Internal.OfferId = OfferSearchHelper.ReturnOfferIdForMarketingCarrierAndCabinNameDirectOnly(offerSearchResponse, FlightType.VS, _testConfig.Search.CabinName); ;
            Assert.True(!_testConfig.Internal.OfferId.IsNullOrEmpty(), $"No offer matched with flight type->{FlightType.VS} and cabin name->{_testConfig.Search.CabinName}");
            //Generating price request
            var priceRequest = Search.Tests.NewBookFlow.Microservices.MicroserviceRequestBuilder.PriceRequest(_testConfig);

            // post price micro service
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoint.Price(priceRequest).EnsureIsSuccessStatusCode();
            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerPriceResult.offers.Count > 0, "No offers have been returned for the offer id provided");
            Assert.True(OfferSearchAssertions.upsellOfferValidation(offerPriceResponse, out string upsellOffers), upsellOffers);

            //Get Offer
            var offerResponse = MicroserviceEndpoint.GetOffer(_testConfig.Internal.OfferId).EnsureIsSuccessStatusCode();
            dynamic offerResponseResult = JsonConvert.DeserializeObject<dynamic>(await offerResponse.Content.ReadAsStringAsync());
            string fleetTypeCode_Offer = offerResponseResult.dataList.flightSegments[0].airCraft.Value;

            NBFMircoserviceHelper.FindEquipmentCode(fleetTypeCode_Offer, out string equipmentCode);
            Assert.NotNull(equipmentCode);

            //Seat Lopa template for Virgin flights
            HttpResponseMessage lopaResponse = MicroserviceEndpoint.SeatLopa(fleetTypeCode_Offer, equipmentCode).EnsureIsSuccessStatusCode();
            dynamic lopaResponseResult = JsonConvert.DeserializeObject<dynamic>(await lopaResponse.Content.ReadAsStringAsync());
            Assert.NotNull(lopaResponseResult);

            //Assertion method
            MicroserviceAssertions.LopaSeatAssertions(lopaResponseResult, equipmentCode, fleetTypeCode_Offer);
        }
    }
}