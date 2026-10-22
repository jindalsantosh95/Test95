using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Helpers;

namespace VAA.EAPI.Automation.NDC.Tests.Microservices.Offer
{
    public class Offer : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;

        public static string schemaFilePath = @".\Tests\Microservices\Schemas\Schema.json";

        public Offer(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
        }

        [Trait("Category", "OfferMS_CICD")]
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
        public void OfferMicroeserviceValidations(JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants)
        {
            List<string> fareFamilies = OfferMicroserviceHelper.ReturnFareFamilies();

            List<string> expectedCabinList = OfferMicroserviceHelper.ReturnExpectedCabins();

            //Generating offer search request
            string offerSearchRequest = OfferSearchRequestBuilderHelper.OfferSearchRequest(journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, cabinTypeCode: "4", numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, fareFamilies: fareFamilies);

            //Post offer search micro service
            HttpResponseMessage offerSearchResponse = Requests.Endpoints.MicroserviceEndpoints.Offers(offerSearchRequest).EnsureIsSuccessStatusCode();

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(offerSearchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            
            SoftAssertHelper softAssert = new();

            softAssert.True(offerSearchResult.offers.Count > 0, "No offers have been returned for the search details provided");

            softAssert.True(OfferValidator.ValidatePointOfSale(offerSearchRequest, offerSearchResponse, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferValidator.ValidatePartiesParticipants(offerSearchRequest, offerSearchResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));
            
            softAssert.True(OfferValidator.ValidateCabinNames(offerSearchResult, expectedCabinList, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferValidator.ValidateBaggageWeightAndDimensions(offerSearchResult, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferValidator.ValidateFareComponent(offerSearchResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferValidator.ValidateOriginAndDestination(offerSearchRequest, offerSearchResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferValidator.ValidatePaxList(offerSearchRequest, offerSearchResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferValidator.ValidateJourneyPaxList(offerSearchRequest, offerSearchResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferValidator.ValidateOfferExpirationTTL(offerSearchResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferValidator.ValidateTotalTaxInLocalCurrency(offerSearchRequest, offerSearchResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferValidator.ValidatePriceClassList(offerSearchResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.True(OfferValidator.ValidateJourneyRefIds(offerSearchResponse, out result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            softAssert.AssertAll();
        }
    }
}
