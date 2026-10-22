using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.MyAccount.Fixtures;
using VAA.EAPI.Automation.Search.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.Basket
{
    public class LoyaltyAgeValidation : IClassFixture<LoyaltyAccountsFixture>
    {
        private readonly LoyaltyAccountsFixture loyaltyAccountsFixture;

        public LoyaltyAgeValidation(LoyaltyAccountsFixture loyaltyAccountsFixture)
        {
            this.loyaltyAccountsFixture = loyaltyAccountsFixture;
            this.loyaltyAccountsFixture.CreateLoyaltyAccount(FlyingClubMemberTiers.Gold, 100_000, DateTime.Now.AddYears(-16).AddMonths(4).ToString("yyyy-MM-dd"));
        }

        //Age Validation 16 on date of travel
        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.ATL, 2, 0, 0, 0)]
        public void _16OnDateOfTravel(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants)
        {
            //Setting the dob variable to look for in the accounts created list
            var dob = DateTime.Now.AddYears(-16).AddMonths(4).ToString("yyyy-MM-dd");

            //Returning the Loyalty Accounts
            var accounts = loyaltyAccountsFixture.GetLoyaltyAccount(dob, 100_000);
            var fcMembershipNumber = accounts.fcDetails;

            //Setting the departure date to be on the Loyalty Accounts 16th Birthday
            var departureDate = DateTime.Now.AddMonths(4).ToString("yyyy-MM-dd");

            var offerSearchRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin, destination, adults: numberOfadults, teens: numberOfteens, child: numberOfchildren, infants: numberOfinfants, departureDate: departureDate, rewardFlag: true);
            var offerSearchResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchRequest).EnsureIsSuccessStatusCode();
            var response = offerSearchResponse.ReadResponseAsString();

            var flightSummaryUrl = OfferSearchHelper.FindSummaryLinkForFirstFlightReturnedInResults(response);
            WebBFFHelper.SummaryURL(flightSummaryUrl, out string basketId, out string slice, out string offerId);
            flightSummaryUrl = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            WebBFFEndpoint.SummaryBFF(flightSummaryUrl).EnsureIsSuccessStatusCode();

            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");
            Assert.True(b2cToken != null, "B2C token is null");

            WebBFFEndpoint.GetMemberDetailsBFF(b2cToken).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            var getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId, b2cToken);

            Assert.True(getBasketResponseBff.StatusCode == System.Net.HttpStatusCode.OK);
        }

        //Age Validation <16 on date of travel
        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.ATL, 2, 0, 0, 0)]
        public async Task Under16OnDateOfTravel(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants)
        {
            //Setting the dob variable to look for in the accounts created list
            var dob = DateTime.Now.AddYears(-16).AddMonths(4).ToString("yyyy-MM-dd");

            //Returning the Loyalty Accounts
            var accounts = loyaltyAccountsFixture.GetLoyaltyAccount(dob, 100_000);
            var fcMembershipNumber = accounts.fcDetails;

            //Setting the departure date to be a month before the loyalty accounts 18th Birthday
            var departureDate = DateTime.Now.AddMonths(4).AddDays(-1).ToString("yyyy-MM-dd");

            var offerSearchRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin, destination, adults: numberOfadults, teens: numberOfteens, child: numberOfchildren, infants: numberOfinfants, departureDate: departureDate, rewardFlag: true);
            var offerSearchResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchRequest).EnsureIsSuccessStatusCode();
            var response = offerSearchResponse.ReadResponseAsString();

            var flightSummaryUrl = OfferSearchHelper.FindSummaryLinkForFirstFlightReturnedInResults(response);
            WebBFFHelper.SummaryURL(flightSummaryUrl, out string basketId, out string slice, out string offerId);
            flightSummaryUrl = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            WebBFFEndpoint.SummaryBFF(flightSummaryUrl).EnsureIsSuccessStatusCode();

            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");
            Assert.True(b2cToken != null, "B2C token is null");

            WebBFFEndpoint.GetMemberDetailsBFF(b2cToken).EnsureIsSuccessStatusCode();

            //Get the basket from the BFF
            var getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId, b2cToken);

            Assert.True(getBasketResponseBff.StatusCode == System.Net.HttpStatusCode.BadRequest);

            var basketResponse = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            var expectedErrorCode = "2010";
            var expectedErrorMessage = "Under 16s cannot be the lead passenger. Please restart your journey with a logged in adult";

            Assert.True(expectedErrorCode == basketResponse[0].code.ToString(), $"Error code returned does not match the expected code '{expectedErrorCode}'.");
            Assert.True(expectedErrorMessage == basketResponse[0].message.ToString(), $"Error message returned does not match the expected code '{expectedErrorMessage}'.");
        }
    }
}
