using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Search.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.Basket
{
    public class LoyaltyErrorHandling
    {
        //No loyalty account details but exists in Azure B2C
        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.ATL, 2, 0, 0, 0)]
        public async Task NoLoyaltyAccountDetails(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants)
        {
            //Using SIT account for UAT and UAT for SIT to cause the error required
            var fcMembershipNumber = Config.Environment switch
            {
                var env when env.Is("dev") => "9107101231",
                var env when env.Is("sit") => "9107101231",
                var env when env.Is("uat") => "9107095094",
                _ => throw new InvalidOperationException("Unsupported Environment")
            };

            var offerSearchRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin, destination, adults: numberOfadults, teens: numberOfteens, child: numberOfchildren, infants: numberOfinfants, rewardFlag: true);
            var offerSearchResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchRequest).EnsureIsSuccessStatusCode();
            var response = offerSearchResponse.ReadResponseAsString();

            var flightSummaryUrl = OfferSearchHelper.FindSummaryLinkForFirstFlightReturnedInResults(response);
            WebBFFHelper.SummaryURL(flightSummaryUrl, out string basketId, out string slice, out string offerId);
            flightSummaryUrl = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            WebBFFEndpoint.SummaryBFF(flightSummaryUrl).EnsureIsSuccessStatusCode();

            var b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");
            Assert.True(b2cToken != null, "B2C token is null");

            //Get the basket from the BFF
            var getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId, b2cToken);

            Assert.True(getBasketResponseBff.StatusCode == System.Net.HttpStatusCode.InternalServerError);

            var basketResponse = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            var expectedErrorCode = "2008";
            var expectedErrorMessage = "We are facing technical difficulties. Please re-start your journey";

            Assert.True(expectedErrorCode == basketResponse[0].code.ToString(), $"Error code returned does not match the expected code '{expectedErrorCode}'.");
            Assert.True(expectedErrorMessage == basketResponse[0].message.ToString(), $"Error message returned does not match the expected code '{expectedErrorMessage}'.");
        }
    }
}