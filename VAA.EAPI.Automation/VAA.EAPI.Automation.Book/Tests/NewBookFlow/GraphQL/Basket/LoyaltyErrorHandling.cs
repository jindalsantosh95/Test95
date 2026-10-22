using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.UI.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.Basket
{
    [Collection("LoyaltyErrorHandling")]
    public class LoyaltyErrorHandling
    {
        //No loyalty account details but exists in Azure B2C
        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.ATL, 2, 0, 0, 0, FareFamilyType.AWARDECONOMY, FlightType.VS)]
        public async Task NoLoyaltyAccountDetails(LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FareFamilyType fareFamilyType, FlightType flightType)
        {
            //Using SIT account for UAT and UAT for SIT to cause the error required
            var fcMembershipNumber = Config.Environment switch
            {
                var env when env.Is("dev") => "9107101231",
                var env when env.Is("sit") => "9107101231",
                var env when env.Is("uat") => "9107095094",
                _ => throw new InvalidOperationException("Unsupported Environment")
            };

            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, fareFamilyType, flightType, origin, destination, numberOfadults, numberOfteens, numberOfchildren, numberOfinfants, rewardFlag: true, out string basketId, out dynamic offerPriceResult);

            //login to AzureB2C
            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");

            Assert.True(b2cToken != null, "B2C token is null");

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Getting the validation error message from the basket
            var errorMessage = JsonConvert.DeserializeObject<dynamic>(retrieveBasketResult.errors[0].extensions.response.ToString());

            //Assert the correct error is returned
            var expectedErrorCode = "2008";
            var expectedErrorMessage = "We are facing technical difficulties. Please re-start your journey";

            Assert.True(expectedErrorCode == errorMessage[0].code.ToString(), $"Error code returned does not match the expected code '{expectedErrorCode}'.");
            Assert.True(expectedErrorMessage == errorMessage[0].message.ToString(), $"Error message returned does not match the expected code '{expectedErrorMessage}'.");
        }
    }
}