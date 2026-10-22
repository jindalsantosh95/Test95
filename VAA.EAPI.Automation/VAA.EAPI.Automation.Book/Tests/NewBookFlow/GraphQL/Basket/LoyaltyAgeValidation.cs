using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.MyAccount.Fixtures;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.Basket
{
    public class LoyaltyAgeValidation : IClassFixture<LoyaltyAccountsFixture>
    {
        private readonly LoyaltyAccountsFixture loyaltyAccountsFixture;

        public LoyaltyAgeValidation(LoyaltyAccountsFixture loyaltyAccountsFixture)
        {
            this.loyaltyAccountsFixture = loyaltyAccountsFixture;
            this.loyaltyAccountsFixture.CreateLoyaltyAccount(FlyingClubMemberTiers.Gold, 100_000, DateTime.Now.AddYears(-16).AddMonths(3).ToString("yyyy-MM-dd"));
        }

        //Age Validation 16 on date of travel
        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.ATL, 2, 0, 0, 0, FareFamilyType.AWARDECONOMY, FlightType.VS)]
        public async Task _16OnDateOfTravel(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FareFamilyType fareFamilyType, FlightType flightType)
        {
            //Setting the dob variable to look for in the accounts created list
            var dob = DateTime.Now.AddYears(-16).AddMonths(3).ToString("yyyy-MM-dd");

            //Returning the Loyalty Accounts
            var accounts = loyaltyAccountsFixture.GetLoyaltyAccount(dob, 100_000);

            //Setting the departure date to be on the Loyalty Accounts 16th Birthday
            string departureDate = DateTime.Now.AddMonths(3).ToString("yyyy-MM-ddT00:00:00");

            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, fareFamilyType, flightType, origin, destination, numberOfadults, numberOfteens, numberOfchildren, numberOfinfants, rewardFlag: true, out string basketId, out dynamic offerPriceResult, departureDate: departureDate);

            string fcMembershipNumber = accounts.fcDetails;

            //login to AzureB2C
            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");

            Assert.True(b2cToken != null, "B2C token is null");

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Assert error is not returned
            Assert.False(DynamicHelper.HasProperty(retrieveBasketResult, "errors"), "Response should not have an 'errors' property.");
        }

        //Age Validation <16 on date of travel
        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 2, 0, 0, 0, FareFamilyType.AWARDECONOMY, FlightType.VS)]
        public async Task Under16OnDateOfTravel(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FareFamilyType fareFamilyType, FlightType flightType)
        {
            //Setting the dob variable to look for in the accounts created list
            var dob = DateTime.Now.AddYears(-16).AddMonths(3).ToString("yyyy-MM-dd");

            //Returning the Loyalty Accounts
            var accounts = loyaltyAccountsFixture.GetLoyaltyAccount(dob, 100_000);

            //Setting the departure date to be a day before the loyalty accounts 16th Birthday
            string departureDate = DateTime.Now.AddMonths(3).AddDays(-1).ToString("yyyy-MM-ddT00:00:00");

            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, fareFamilyType, flightType, origin, destination, numberOfadults, numberOfteens, numberOfchildren, numberOfinfants, rewardFlag: true, out string basketId, out dynamic offerPriceResult, departureDate: departureDate);

            string fcMembershipNumber = accounts.fcDetails;

            //login to AzureB2C
            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");

            Assert.True(b2cToken != null, "B2C token is null");

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Assert error is returned
            Assert.True(DynamicHelper.HasProperty(retrieveBasketResult, "errors"), "Response should have an 'errors' property.");

            //Getting the validation error message from the basket
            var errorMessage = JsonConvert.DeserializeObject<dynamic>(retrieveBasketResult.errors[0].extensions.response.ToString());

            //Assert the correct error is returned
            var expectedErrorCode = "2010";
            var expectedErrorMessage = "Under 16s cannot be the lead passenger. Please restart your journey with a logged in adult";

            Assert.True(expectedErrorCode == errorMessage[0].code.ToString(), $"Error code returned does not match the expected code '{expectedErrorCode}'.");
            Assert.True(expectedErrorMessage == errorMessage[0].message.ToString(), $"Error message returned does not match the expected code '{expectedErrorMessage}'.");
        }
    }
}