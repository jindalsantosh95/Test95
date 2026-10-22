using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Search.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.Basket
{
    //[Collection("BasketSummaryValidation")]
    public class BasketFlyingClubBff
    {
        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 0, 0, 0, "VS", "9999998025", "Five", "Axistest")] // This account works in SIT and UAT....
        [InlineData(LocationType.LHR, LocationType.ATL, 1, 0, 0, 0, "AR", "9106599849", "Aerolineas", "Argentinas")]

        public async Task BasketFlyingClubValidation(LocationType origin, LocationType destination, int adults, int teens, int child, int infants, string fcProgram, string fcNumber, string fcFirstName, string fcLastName)
        {
            //Generating offer search BFF request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: adults, teens: teens, child: child, infants: infants);

            //Post search BFF endpoint
            HttpResponseMessage offerSearchBFFResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFRequest).EnsureIsSuccessStatusCode();

            var result = offerSearchBFFResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Getting the link url for the SummaryBff
            var url = OfferSearchHelper.FindSummaryLinkForSpecifiedAirlineCodeAndCabinNameForOneWay(result, FlightScenarioType.Virgin);

            //Getting basketId from the response url
            WebBFFHelper.SummaryURL(url, out string basketId, out string slice, out string offerId);

            //Need to build url for Summary link from returned url
            url = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(url).EnsureIsSuccessStatusCode();

            dynamic summaryBFFPayload = summaryBffResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            string peopleLink = getBasket["people"][0]["link"].ToString();
            string firstName = fcFirstName;
            string lastName = fcLastName;
            string dateOfBirth = "2000-10-10";
            string gender = "Male";
            string type = "ADULT";
            string fqtProgram = fcProgram;
            string flyNumber = fcNumber;
            var passenegerReq = WebBFFHelper.UpdatePassengerRequestBff(firstName, lastName, dateOfBirth, gender, type, fqtProgram, flyNumber);

            var updatePassengerResponse_BFF = WebBFFEndpoint.UpdatePassengerBFF(basketId, peopleLink, passenegerReq).EnsureIsSuccessStatusCode();

            dynamic updatePassengerResult = JsonConvert.DeserializeObject<dynamic>(await updatePassengerResponse_BFF.Content.ReadAsStringAsync());

            string fqtValidityStatus = updatePassengerResult.people[0]["membershipDetails"]["fqtValidityStatus"].Value.ToString();

            if (fqtProgram == "VS")
            {
                Assert.True(fqtValidityStatus.Is("VALID"), $"VALID was not returned as expected for Basket");
            }
            else
                Assert.True(fqtValidityStatus.Is("UNCHECKED"), $"UNCHECKED was not returned as expected for Basket");
        }
    }
}