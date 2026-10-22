using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.DataProvider;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.Basket
{
    [Collection("EndToEndRewardSearch")]
    public class ValidateBasket : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;
        private static Random rnd = new Random();

        public ValidateBasket(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
            this.tokenFixture.AddTokenForVRedFromKeyVault("vred");
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.LHR, LocationType.ATL, "Red", 1, 1, 1, 1, FlightScenarioType.Virgin)]
        [InlineData(LocationType.LHR, LocationType.ATL, "Silver", 1, 1, 1, 1, FlightScenarioType.NonVirgin)]
        [InlineData(LocationType.LHR, LocationType.ATL, "Silver", 1, 1, 1, 1, FlightScenarioType.Virgin)]
        [InlineData(LocationType.DEL, LocationType.LHR, "Gold", 1, 1, 1, 1, FlightScenarioType.Virgin)]
        public async Task OneWay(LocationType origin, LocationType destination, string tierCode, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FlightScenarioType flightScenarioType)
        {
            WebBFFHelper.OfferSearchBFF(JourneyType.OneWay, flightScenarioType, origin, destination, numberOfadults, numberOfteens, numberOfchildren, numberOfinfants, rewardFlag: true, out string basketId, out HttpResponseMessage summaryBFFPayload);

            //login to AzureB2C
            var offerPriceMembershipPoints = "5000";
            string fcMembershipNumber = string.Empty;
            Dictionary<string, string> loyaltyDetails = new Dictionary<string, string>();

            int maxAttempts = 5;
            int attempt = 0;
            int fqTPointsBeforeRedeem = 0;
            while (attempt < maxAttempts)
            {
                loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tierCode, int.Parse(offerPriceMembershipPoints));

                fcMembershipNumber = loyaltyDetails["rowKey"];

                var vRedResponseAPI = MicroserviceEndpoint.Vred(tokenFixture.GetToken("vred"), fcMembershipNumber).EnsureIsSuccessStatusCode();

                dynamic vRedResultAPI = JsonConvert.DeserializeObject<dynamic>(await vRedResponseAPI.Content.ReadAsStringAsync());

                int vredPoints = (int)vRedResultAPI.balance.usable.Value;

                await  LoyaltyAccountUpdates.UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, vredPoints);

                if (loyaltyDetails["points"] == vredPoints.ToString() && vredPoints >= int.Parse(offerPriceMembershipPoints))
                    break;

                attempt++;
            }
            fqTPointsBeforeRedeem = int.Parse(loyaltyDetails["points"]);

            fcMembershipNumber = loyaltyDetails["rowKey"];

            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");

            HttpResponseMessage fcMemberDetailsResponse = WebBFFEndpoint.GetMemberDetailsBFF(b2cToken).EnsureIsSuccessStatusCode();

            dynamic fcMemberResult = JsonConvert.DeserializeObject<dynamic>(await fcMemberDetailsResponse.Content.ReadAsStringAsync());

            Assert.True(WebBFFAssertions.ValidateFCMemberDetails(fcMembershipNumber, tierCode, fcMemberResult, out string fcDetails), fcDetails);

            Assert.True(!b2cToken.IsNullOrEmpty(), "B2C token is null");

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId, b2cToken).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            var retrieveBasketAssert = new SoftAssertHelper();

            retrieveBasketAssert.True(getBasket.summary.isAwardSearch.Value, $"RetrieveBasketAssert Validation failed. Reasons: isAwardSearch is not True for redemption points scenario \n Basket Response : {getBasket}");

            retrieveBasketAssert.True(getBasket.summary.membershipPoints.Value != null, $"Retrieve Basket Validation failed. Reasons: membershipPoints field is null  \n Response : {getBasket}");

            retrieveBasketAssert.True(BasketBFFAssertion.ValidateBasketMembershipPoints(summaryBFFPayload, getBasket, out string message), $"Retrieve Basket Validation failed. Reasons: " + message);

            retrieveBasketAssert.True(BasketBFFAssertion.ValidateBasketFlightPrice(summaryBFFPayload, getBasket, out message), $"Retrieve Basket Validation failed. Reasons: " + message);

            retrieveBasketAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.JFK, LocationType.LHR, "Red", 1, 1, 1, 1, FlightScenarioType.Virgin)]
        [InlineData(LocationType.LHR, LocationType.ATL, "Silver", 1, 1, 1, 1, FlightScenarioType.NonVirgin)]
        [InlineData(LocationType.DEL, LocationType.LHR, "Gold", 1, 1, 1, 1, FlightScenarioType.Virgin)]
        public async Task Return(LocationType origin, LocationType destination, string tierCode, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FlightScenarioType flightScenarioType)
        {
            WebBFFHelper.OfferSearchBFF(JourneyType.Return, flightScenarioType, origin, destination, numberOfadults, numberOfteens, numberOfchildren, numberOfinfants, rewardFlag: true, out string basketId, out HttpResponseMessage summaryBFFPayload);

            //login to AzureB2C
            Dictionary<string, string> loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tierCode, 2000);

            string fcMembershipNumber = loyaltyDetails["rowKey"];

            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");

            HttpResponseMessage fcMemberDetailsResponse = WebBFFEndpoint.GetMemberDetailsBFF(b2cToken).EnsureIsSuccessStatusCode();

            dynamic fcMemberResult = JsonConvert.DeserializeObject<dynamic>(await fcMemberDetailsResponse.Content.ReadAsStringAsync());

            Assert.True(WebBFFAssertions.ValidateFCMemberDetails(fcMembershipNumber, tierCode, fcMemberResult, out string fcDetails), fcDetails);

            Assert.True(!b2cToken.IsNullOrEmpty(), "B2C token is null");

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId, b2cToken).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            //Assertion of Retrieve Basket
            var retrieveBasketAssert = new SoftAssertHelper();

            retrieveBasketAssert.True(getBasket.summary.isAwardSearch.Value, $"Retrieve Basket Validation failed. Reasons: isAwardSearch is not True for redemption points scenario \n Basket Response : {getBasket}");

            retrieveBasketAssert.True(getBasket.summary.membershipPoints.Value != null, $"Retrieve Basket Validation failed. Reasons: membershipPoints field is null  \n Response : {getBasket}");

            retrieveBasketAssert.True(BasketBFFAssertion.ValidateBasketMembershipPoints(summaryBFFPayload, getBasket, out string message), $"Retrieve Basket Validation failed. Reasons: " + message);

            retrieveBasketAssert.True(BasketBFFAssertion.ValidateBasketFlightPrice(summaryBFFPayload, getBasket, out message), $"Retrieve Basket Validation failed. Reasons: " + message);

            retrieveBasketAssert.AssertAll();
        }
    }
}
