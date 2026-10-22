using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.Microservices.Seats
{
    public class Seats : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;
        public Seats(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
        }

        [Trait("Category", GlobalTraits.SeatMicroserviceCiCd)]
        [Theory]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.JFK, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 2, 0, 1, 1)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.ATL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 1, 1, 0, 0)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.BOS, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 2, 0, 0, 0)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.MCO, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 2, 0, 2, 1)]
        [InlineData(JourneyType.OneWay, LocationType.LHR, LocationType.LAX, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 4, 0, 0, 0)]
        [InlineData(JourneyType.Return, LocationType.LHR, LocationType.JFK, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 1, 1, 1, 1)]
        [InlineData(JourneyType.Return, LocationType.LHR, LocationType.ATL, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 2, 2, 1, 1)]
        [InlineData(JourneyType.Return, LocationType.LHR, LocationType.BOS, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 3, 1, 0, 0)]
        [InlineData(JourneyType.Return, LocationType.LHR, LocationType.MCO, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 2, 0, 0, 2)]
        [InlineData(JourneyType.Return, LocationType.LHR, LocationType.LAX, LocationType.None, LocationType.None, LocationType.None, LocationType.None, 0, 1, 0, 0)]
        [InlineData(JourneyType.MultiCity, LocationType.IAD, LocationType.LON, LocationType.LON, LocationType.DEL, LocationType.DEL, LocationType.ATL, 2, 0, 0, 0)]
        [InlineData(JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.JNB, LocationType.JNB, LocationType.LHR, 1, 1, 0, 0)]
        [InlineData(JourneyType.MultiCity, LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.LHR, 2, 1, 1, 1)]

        public void SeatsMicroserviceValidations(JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            List<string> fareFamilies = OfferMicroserviceHelper.ReturnFareFamilies();
            int seatPaxCount = numberOfadults + numberOfyouths + numberOfchildren;

            string offerSearchRequest = OfferSearchRequestBuilderHelper.OfferSearchRequest(journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, fareFamilies: fareFamilies);

            //Post Offer Search Microservice
            HttpResponseMessage offerSearchResponse = MicroserviceEndpoints.Offers(offerSearchRequest).EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            dynamic selectedOffer = OfferMicroserviceHelper.ReturnFirstSelectedVirginOffer(offerSearchResponse);

            //Generating price request
            string offerPriceRequest = OfferSearchRequestBuilderHelper.OfferPriceRequest(journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, fareFamilies: fareFamilies, selectedOffer: selectedOffer);

            //Post Offer Price Microservice
            HttpResponseMessage offerPriceResponse = MicroserviceEndpoints.Price(offerPriceRequest).EnsureIsSuccessStatusCode();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(offerPriceResult.offers.Count > 0, "No offers have been returned for the offer id provided");

            dynamic flightSegments = OfferMicroserviceHelper.ReturnFlightSegmentsForVirginFlights(offerPriceResponse);

            int flightSegementsCount = ((IEnumerable<dynamic>)flightSegments).Count();

            string seatsRequest = OfferSearchRequestBuilderHelper.SeatsRetrievalRequest(numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, currencyCode: "GBP", channelId: "NDC", flightSegments: flightSegments);

            HttpResponseMessage seatsResponse = MicroserviceEndpoints.Seats(seatsRequest).EnsureIsSuccessStatusCode();

            JObject seatsResponsejObject = JsonConvert.DeserializeObject<JObject>(seatsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SoftAssertHelper softAssert = new();

            softAssert.True(SeatsValidator.ValidateSeatOfferId(seatsResponse, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(SeatsValidator.ValidateFlightSegmentSeatMapCount(seatsResponse, flightSegementsCount, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(SeatsValidator.ValidateSeatPtcs(offerSearchRequest, seatsResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(SeatsValidator.ValidateSeatMapData(seatsResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(SeatsValidator.ValidateCatalogueData(offerSearchRequest, seatsResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }
    }
}