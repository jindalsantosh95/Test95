using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.Basket
{
    [Collection("BasketBFFRedemption")]
    public class BasketBFFRedemption : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;

        public BasketBFFRedemption(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
            this.tokenFixture.AddTokenForVRedFromKeyVault("vred");
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData(LocationType.JFK, LocationType.LHR, "Red", 1, 1, 1, 1, FlightScenarioType.Virgin)]
        [InlineData(LocationType.LHR, LocationType.ATL, "Silver", 1, 1, 1, 1, FlightScenarioType.NonVirgin)]
        [InlineData(LocationType.DEL, LocationType.LHR, "Gold", 1, 1, 1, 1, FlightScenarioType.Virgin)]
        public async Task BasketSummaryFCLoginValidationReturn(LocationType origin, LocationType destination, string tierCode, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FlightScenarioType flightScenarioType)
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

            Assert.True(getBasket.summary.isAwardSearch.Value, $"isAwardSearch is not True for redemption points scenario \n Basket Response : {getBasket}");

            Assert.True(getBasket.summary.membershipPoints.Value != null, $"membershipPoints field is null  \n Response : {getBasket}");

            Assert.True(BasketBFFAssertion.ValidateBasketMembershipPoints(summaryBFFPayload, getBasket, out string message), message);

            Assert.True(BasketBFFAssertion.ValidateBasketFlightPrice(summaryBFFPayload, getBasket, out message), message);
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.JFK, LocationType.LHR, "Red", 1, 1, 1, 1, FlightScenarioType.Virgin)]
        [InlineData(LocationType.LHR, LocationType.ATL, "Silver", 1, 1, 1, 1, FlightScenarioType.NonVirgin)]
        [InlineData(LocationType.DEL, LocationType.LHR, "Gold", 1, 1, 1, 1, FlightScenarioType.Virgin)]
        public async Task BasketSummaryFCLoginValidationOneWay(LocationType origin, LocationType destination, string tierCode, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FlightScenarioType flightScenarioType)
        {
            WebBFFHelper.OfferSearchBFF(JourneyType.OneWay, flightScenarioType, origin, destination, numberOfadults, numberOfteens, numberOfchildren, numberOfinfants, rewardFlag: true, out string basketId, out HttpResponseMessage summaryBFFPayload);

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

            Assert.True(getBasket.summary.isAwardSearch.Value, $"isAwardSearch is not True for redemption points scenario \n Basket Response : {getBasket}");

            Assert.True(getBasket.summary.membershipPoints.Value != null, $"membershipPoints field is null  \n Response : {getBasket}");

            Assert.True(BasketBFFAssertion.ValidateBasketMembershipPoints(summaryBFFPayload, getBasket, out string message), message);

            Assert.True(BasketBFFAssertion.ValidateBasketFlightPrice(summaryBFFPayload, getBasket, out message), message);
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.JFK, LocationType.LHR, "Red", 1, 1, 1, 1, FlightScenarioType.Virgin)]
        [InlineData(LocationType.LHR, LocationType.ATL, "Silver", 1, 1, 1, 1, FlightScenarioType.NonVirgin)]
        [InlineData(LocationType.DEL, LocationType.LHR, "Gold", 1, 1, 1, 1, FlightScenarioType.Virgin)]
        public async Task BasketSummaryFCLoginValidationInsufficientPointsOneWay(LocationType origin, LocationType destination, string tierCode, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FlightScenarioType flightScenarioType)
        {
            WebBFFHelper.OfferSearchBFF(JourneyType.OneWay, flightScenarioType, origin, destination, numberOfadults, numberOfteens, numberOfchildren, numberOfinfants, rewardFlag: true, out string basketId, out HttpResponseMessage summaryBFFPayload);

            var offerPriceResponse = JObject.Parse(summaryBFFPayload.ReadResponseAsString());
            var offerPriceMembershipPoints = offerPriceResponse.SelectToken(".price.awardPoints").ToString();

            Dictionary<string, string> loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tierCode, int.Parse(offerPriceMembershipPoints), true);

            string fcMembershipNumber = loyaltyDetails["rowKey"];

            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");

            HttpResponseMessage fcMemberDetailsResponse = WebBFFEndpoint.GetMemberDetailsBFF(b2cToken).EnsureIsSuccessStatusCode();

            dynamic fcMemberResult = JsonConvert.DeserializeObject<dynamic>(await fcMemberDetailsResponse.Content.ReadAsStringAsync());

            Assert.True(WebBFFAssertions.ValidateFCMemberDetails(fcMembershipNumber, tierCode, fcMemberResult, out string fcDetails), fcDetails);

            Assert.True(!b2cToken.IsNullOrEmpty(), "B2C token is null");
            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId, b2cToken).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            var softAssert = new SoftAssertHelper();

            softAssert.True(getBasket.summary.isAwardSearch.Value, $"isAwardSearch is not True for redemption points scenario \n Basket Response : {getBasket}");
            softAssert.True(getBasket.summary.membershipPoints.Value != null, $"membershipPoints field is null  \n Response : {getBasket}");
            softAssert.True(BasketBFFAssertion.ValidateBasketMembershipPoints(summaryBFFPayload, getBasket, out string message), message);
            softAssert.True(BasketBFFAssertion.ValidateBasketFlightPrice(summaryBFFPayload, getBasket, out message), message);
            softAssert.True(BasketBFFAssertion.ValidateFCPointsEligibilityNegativeTestInsufficientPoints(getBasket, out message), message);

            softAssert.AssertAll();
        }
    }
}