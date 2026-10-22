using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Fixtures;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.Features.PayWithPoints
{
    [Collection("ErrorHandling")]
    public class ErrorHandling : IClassFixture<TokenFixture>, IClassFixture<LoyaltyAccountsFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly LoyaltyAccountsFixture loyaltyAccountsFixture;

        public ErrorHandling(TokenFixture tokenFixture, LoyaltyAccountsFixture loyaltyAccountsFixture)
        {
            this.tokenFixture = tokenFixture;
            this.tokenFixture.AddTokenForVRedFromKeyVault("vred");
            this.loyaltyAccountsFixture = loyaltyAccountsFixture;
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 0, 0, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 1, 0, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 1, 1, 1)]
        public async Task NotEnoughPointsPerPerson(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants)
        {
            // Get minium required points expected for points converter
            int passengerCount = numberOfadults + numberOfchildren + numberOfinfants + numberOfteens;
            var requiredPoints = passengerCount * 3000;

            // Create loyalty account
            var dob = DateTime.Now.AddYears(-18).ToString("yyyy-MM-dd");
            loyaltyAccountsFixture.CreateLoyaltyAccount(FlyingClubMemberTiers.Gold, requiredPoints - 1, dob);
            var account = loyaltyAccountsFixture.GetLoyaltyAccount(dob, requiredPoints - 1);
            Assert.True(requiredPoints - 1 == account.points, $"Loyalty account has the wrong points value. Acutal: {account.points}, Expected: {requiredPoints - 1}");

            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, FareFamilyType.ECON, FlightType.VS, origin, destination, numberOfadults, numberOfteens, numberOfchildren, numberOfinfants, rewardFlag: false, out string basketId, out _);

            //login to AzureB2C
            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(account.fcDetails, "Virgin123");

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //passenger update in basket
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId, false, "", b2cToken);
            GraphQLHelper.ContactBasketUpdateGraqphQL(out string emailId, out string phoneNumber, out string altPhoneNumber, out bool acceptPost, out bool acceptTelephone, out bool acceptEmail, out bool acceptTextMsg, out string countryCode);

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);
            GraphQLEndpoint.Book(contactBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            //Term and Condition Graph QL
            var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            //point selection in HPP page
            var pointToCurrencyConverterResponse = GraphQLEndpoint.Book(basketId, b2cToken).EnsureIsSuccessStatusCode();
            dynamic pointToCurrencyConverterResult = JsonConvert.DeserializeObject<dynamic>(await pointToCurrencyConverterResponse.Content.ReadAsStringAsync());

            //Assert the correct error is returned
            var expectedErrorCode = "2011";
            var expectedErrorMessage = "Unfortunately you wont be able to pay with points. The minimum amount required is 3000 points per passenger";

            Assert.True(expectedErrorCode == pointToCurrencyConverterResult.errors[0].extensions.code.ToString(), $"Error code returned does not match the expected code '{expectedErrorCode}'.");
            Assert.True(expectedErrorMessage == pointToCurrencyConverterResult.errors[0].extensions.message.ToString(), $"Error message returned does not match the expected code '{expectedErrorMessage}'.");
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 0, 0, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 1, 0, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 1, 1, 0)]
        [InlineData(LocationType.LHR, LocationType.JFK, 1, 1, 1, 1)]
        public async Task SufficientPointsNoError(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants)
        {
            // Get minium required points expected for points converter
            int passengerCount = numberOfadults + numberOfchildren + numberOfinfants + numberOfteens;
            var requiredPoints = passengerCount * 3000;

            // Create loyalty account
            var dob = DateTime.Now.AddYears(-18).ToString("yyyy-MM-dd");
            loyaltyAccountsFixture.CreateLoyaltyAccount(FlyingClubMemberTiers.Gold, requiredPoints, dob);
            var account = loyaltyAccountsFixture.GetLoyaltyAccount(dob, requiredPoints);
            Assert.True(requiredPoints == account.points, "Loyalty account has the wrong points value.");

            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, FareFamilyType.ECON, FlightType.VS, origin, destination, numberOfadults, numberOfteens, numberOfchildren, numberOfinfants, rewardFlag: false, out string basketId, out _);

            //login to AzureB2C
            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(account.fcDetails, "Virgin123");

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //passenger update in basket
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId, false, "", b2cToken);
            GraphQLHelper.ContactBasketUpdateGraqphQL(out string emailId, out string phoneNumber, out string altPhoneNumber, out bool acceptPost, out bool acceptTelephone, out bool acceptEmail, out bool acceptTextMsg, out string countryCode);

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);
            GraphQLEndpoint.Book(contactBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            //Term and Condition Graph QL
            var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            //point selection in HPP page
            var pointToCurrencyConverterResponse = GraphQLEndpoint.Book(basketId, b2cToken).EnsureGQLResponseSuccess();
            dynamic pointToCurrencyConverterResult = JsonConvert.DeserializeObject<dynamic>(await pointToCurrencyConverterResponse.Content.ReadAsStringAsync());

            //Assert error is not returned
            Assert.False(DynamicHelper.HasProperty(pointToCurrencyConverterResult, "errors"), "Response should not have an 'errors' property.");
        }
    }
}