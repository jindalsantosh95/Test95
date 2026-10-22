using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.DataProvider;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.Basket
{
    [Collection("BasketRedemptionGraphQL")]
    public class BasketRedemptionGraphQL : IClassFixture<ReportFixture>, IClassFixture<TokenFixture>
    {
        private ReportFixture reportFixture;
        private readonly TokenFixture tokenFixture;
        private static Random rnd = new Random();

        public BasketRedemptionGraphQL(ReportFixture reportFixture, TokenFixture tokenFixture)
        {
            this.reportFixture = reportFixture;
            this.tokenFixture = tokenFixture;
            this.tokenFixture.AddTokenForVRedFromKeyVault("vred");
        }

        #region Redemption OneWay

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.ATL, FareFamilyType.AWARDECONOMY, FlightType.VS, "Red", 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.BOS, FareFamilyType.AWARDCOMFORTPLUSPREMIUMECONOMY, FlightType.VS, "Silver", 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.AWARDBUSINESSFIRST, FlightType.VS, "Gold", 1, 1, 1, 1)]
        [InlineData(LocationType.ATL, LocationType.LHR, FareFamilyType.AWARDECONOMY, FlightType.DL, "Silver", 1, 1, 1, 1)]
        public async Task Basket_FlyingClublogin_OneWay(LocationType origin, LocationType destination, FareFamilyType fareFamily, FlightType flightType, string tierCode, int adults, int teens, int child, int infants)
        {
            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, fareFamily, flightType, origin, destination, adults, teens, child, infants, rewardFlag: true, out string basketId, out dynamic offerPriceResult);

            var offerPriceMembershipPoints = offerPriceResult.data.searchSummary.price.awardPoints.Value;

            string fcMembershipNumber = string.Empty;
            Dictionary<string, string> loyaltyDetails = new Dictionary<string, string>();

            int maxAttempts = 5;
            int attempt = 0;
            int fqTPointsBeforeRedeem = 0;
            while (attempt < maxAttempts)
            {
                loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tierCode, int.Parse(offerPriceMembershipPoints), false);

                fcMembershipNumber = loyaltyDetails["rowKey"];

                var vRedResponseAPI = MicroserviceEndpoint.Vred(tokenFixture.GetToken("vred"), fcMembershipNumber).EnsureIsSuccessStatusCode();

                dynamic vRedResultAPI = JsonConvert.DeserializeObject<dynamic>(await vRedResponseAPI.Content.ReadAsStringAsync());

                int vredPoints = (int)vRedResultAPI.balance.usable.Value;

                await VAA.EAPI.Automation.MyAccount.DataProvider.LoyaltyAccountUpdates.UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, vredPoints);

                if (loyaltyDetails["points"] == vredPoints.ToString() && vredPoints >= int.Parse(offerPriceMembershipPoints))
                    break;

                attempt++;
            }
            fqTPointsBeforeRedeem = int.Parse(loyaltyDetails["points"]);

            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");

            Assert.True(!b2cToken.IsNullOrEmpty(), "B2C token is null");

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            var softAssert = new SoftAssertHelper();

            //MemberShip points valdation with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value, $"Validation failed: isAwardSearch is not True for redemption points scenario {retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value}");

            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.membershipPoints.Value != null, $"Validation failed: membershipPoints field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, retrieveBasketResult, out string message), $"Validation failed:  {message}");

            //Validate FC point with basket membership points
            softAssert.True(GraphQLAssertion.ValidateFcPointElibility(retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate Basket FC customer profile
            softAssert.True(retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value == "VS", $"Validation failed: The Basket FQTProgramme - {retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value} is not equal to FQTProgramme VS");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubNumber(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubTierCode(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubPoints(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubFirstName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubLastName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate Flight price with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.flightPrice.Value != null, $"flightPrice field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, retrieveBasketResult, out string errorMessage), errorMessage);

            softAssert.AssertAll();

            //passenger update in basket
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId, false, "", b2cToken);

            GraphQLHelper.ContactBasketUpdateGraqphQL(out string emailId, out string phoneNumber, out string altPhoneNumber, out bool acceptPost, out bool acceptTelephone, out bool acceptEmail, out bool acceptTextMsg, out string countryCode);

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            var contactSoftAssert = new SoftAssertHelper();

            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == basketId, "Contact Basket Validation failed: Basket ID dont match");

            //MemberShip points valdation with offer price data
            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.summary.isAwardSearch.Value, "Contact Basket Validation failed: isAwardSearch is not True for redemption points scenario");

            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.summary.membershipPoints.Value != null, $"Contact Basket Validation failed: membershipPoints field is null  \n Response : {contactBasketGQLResult}");

            contactSoftAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, contactBasketGQLResult, out message), $"Contact Basket Validation failed: {message}");

            //Validate Flight price with offer price data
            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.summary.flightPrice.Value != null, $"Contact Basket Validation failed:  flightPrice field is null  \n Response : {contactBasketGQLResult}");

            contactSoftAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, contactBasketGQLResult, out errorMessage), $"Contact Basket Validation failed:  {errorMessage}");

            contactSoftAssert.AssertAll();
            //Term and Condition Graph QL
            var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);

            var tncBasketGQLResponse = GraphQLEndpoint.Book(tncBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic tncBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await tncBasketGQLResponse.Content.ReadAsStringAsync());

            var tncBasketSoftAssert = new SoftAssertHelper();

            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.basketId.Value == basketId, "Basket ID dont match");

            //MemberShip points valdation with offer price data
            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.summary.isAwardSearch.Value, "Term and condition Basket Validation failed: isAwardSearch is not True for redemption points scenario");

            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.summary.membershipPoints.Value != null, $"Term and condition Basket Validation failed: membershipPoints field is null  \n Response : {tncBasketGQLResult}");

            tncBasketSoftAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, tncBasketGQLResult, out message), $"Term and condition Basket Validation failed:  {message}");

            //Validate Flight price with offer price data
            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.summary.flightPrice.Value != null, $"Term and condition Basket Validation failed: flightPrice field is null  \n Response : {tncBasketGQLResult}");

            tncBasketSoftAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, tncBasketGQLResult, out errorMessage), $"Term and condition Basket Validation failed:  {errorMessage}");

            tncBasketSoftAssert.AssertAll();

            //Retrieve Basket Graph ql request
            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            softAssert = new SoftAssertHelper();

            //MemberShip points valdation with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value, $"Validation failed: isAwardSearch is not True for redemption points scenario {retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value}");

            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.membershipPoints.Value != null, $"Validation failed: membershipPoints field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate FC point with basket membership points
            softAssert.True(GraphQLAssertion.ValidateFcPointElibility(retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate Basket FC customer profile
            softAssert.True(retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value == "VS", $"Validation failed: The Basket FQTProgramme - {retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value} is not equal to FQTProgramme VS");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubNumber(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubTierCode(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubPoints(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubFirstName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubLastName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate Flight price with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.flightPrice.Value != null, $"flightPrice field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, retrieveBasketResult, out errorMessage), errorMessage);

            softAssert.True(retrieveBasketResult.data.retrieveBasket.validation.Value == "VALID", "Retreieve Basket Valdation Failed : Validation is Invalid");

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.ATL, FareFamilyType.AWARDECONOMY, FlightType.VS, "Red", 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.BOS, FareFamilyType.AWARDCOMFORTPLUSPREMIUMECONOMY, FlightType.VS, "Silver", 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.SFO, FareFamilyType.AWARDBUSINESSFIRST, FlightType.VS, "Gold", 1, 1, 1, 1)]
        [InlineData(LocationType.ATL, LocationType.LHR, FareFamilyType.AWARDECONOMY, FlightType.DL, "Silver", 1, 1, 1, 1)]
        public async Task Basket_FCLogin_InsufficientPoints_OneWay_NegativeTest(LocationType origin, LocationType destination, FareFamilyType fareFamily, FlightType flightType, string tierCode, int adults, int teens, int child, int infants)
        {
            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, fareFamily, flightType, origin, destination, adults, teens, child, infants, rewardFlag: true, out string basketId, out dynamic offerPriceResult);

            var offerPriceMembershipPoints = offerPriceResult.data.searchSummary.price.awardPoints.Value;

            string fcMembershipNumber = string.Empty;
            Dictionary<string, string> loyaltyDetails = new Dictionary<string, string>();

            int maxAttempts = 5;
            int attempt = 0;
            int fqTPointsBeforeRedeem = 0;
            while (attempt < maxAttempts)
            {
                loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tierCode, int.Parse(offerPriceMembershipPoints), true);

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

            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");

            Assert.True(!b2cToken.IsNullOrEmpty(), "B2C token is null");

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            var softAssert = new SoftAssertHelper();

            //MemberShip points valdation with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value, $"Validation failed: isAwardSearch is not True for redemption points scenario {retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value}");

            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.membershipPoints.Value != null, $"Validation failed: membershipPoints field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, retrieveBasketResult, out string message), $"Validation failed:  {message}");

            //Validate FC point with basket membership points
            softAssert.True(GraphQLAssertion.ValidateFcPointsEligibility_NegativeTest_InsufficientPoints(retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate Basket FC customer profile
            softAssert.True(retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value == "VS", $"Validation failed: The Basket FQTProgramme - {retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value} is not equal to FQTProgramme VS");

            softAssert.True(retrieveBasketResult.data.retrieveBasket.people[0].profilePassenger.Value, $"Validation failed: The Basket Profile passenger flag is false - {retrieveBasketResult.data.retrieveBasket.people[0].Value} suppose to be True VS");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubNumber(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubTierCode(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubPoints(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubFirstName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubLastName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate Flight price with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.flightPrice.Value != null, $"flightPrice field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, retrieveBasketResult, out string errorMessage), errorMessage);

            softAssert.AssertAll();

            //passenger update in basket
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId, false, "", b2cToken);

            //Retrieve Basket Graph ql request
            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            var people = (JArray)retrieveBasketResult.data.retrieveBasket.people;

            foreach (var item in people)
            {
                string passengerId = item["id"].ToString();
                string type = item["type"].ToString();
                string firstName = item["firstName"].ToString();
                string lastName = item["lastName"].ToString();
                string gender = item["gender"].ToString();
                string dob = item["dateOfBirth"].ToString();
                var parsedDate = DateTime.Parse(dob).ToString("yyyy-MM-dd");
                string fqtProgramme = item["membershipDetails"]["fqtProgramme"].ToString();
                string fqtNumber = item["membershipDetails"]["fqtNumber"].ToString();

                if (passengerId == "0" && !b2cToken.IsNullOrWhiteSpace() && firstName.IsNullOrWhiteSpace()) continue;

                var updatePassengerRequest = GraphQLRequestBuilder.UpdatePassengerRequestGQL(basketId, passengerId, firstName, lastName, parsedDate, gender, type, fqtProgramme, fqtNumber);

                var updatePassengerResponseGQL = GraphQLEndpoint.Book(updatePassengerRequest, b2cToken).EnsureGQLResponseSuccess();

                dynamic passengerBasket = JsonConvert.DeserializeObject<dynamic>(await updatePassengerResponseGQL.Content.ReadAsStringAsync());

                Assert.True(GraphQLAssertion.ValidatePassengerBasketMembershipDetails(passengerBasket, basketId, passengerId, out message), $"Basket Validation failed in passegner : \n{message}");

                var passengerSoftAssert = new SoftAssertHelper();

                //MemberShip points valdation with offer price data
                passengerSoftAssert.True(passengerBasket.data.updatePeople.summary.isAwardSearch.Value, "Passenger Basket Validation failed: isAwardSearch is not True for redemption points scenario");

                passengerSoftAssert.True(passengerBasket.data.updatePeople.summary.membershipPoints.Value != null, $"Passenger Basket Validation failed: membershipPoints field is null  \n Response : {passengerBasket}");

                passengerSoftAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, passengerBasket, out message), $"Passenger Basket Validation failed: {message}");

                //Validate Flight price with offer price data
                passengerSoftAssert.True(passengerBasket.data.updatePeople.summary.flightPrice.Value != null, $"Passenger Basket Validation failed: flightPrice field is null  \n Response : {passengerBasket}");

                passengerSoftAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, passengerBasket, out errorMessage), $"Passenger Basket Validation failed:  {errorMessage}");

                passengerSoftAssert.AssertAll();
            }

            GraphQLHelper.ContactBasketUpdateGraqphQL(out string emailId, out string phoneNumber, out string altPhoneNumber, out bool acceptPost, out bool acceptTelephone, out bool acceptEmail, out bool acceptTextMsg, out string countryCode);

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            var contactSoftAssert = new SoftAssertHelper();

            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == basketId, "Contact Basket Validation failed: Basket ID dont match");

            //MemberShip points valdation with offer price data
            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.summary.isAwardSearch.Value, "Contact Basket Validation failed: isAwardSearch is not True for redemption points scenario");

            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.summary.membershipPoints.Value != null, $"Contact Basket Validation failed: membershipPoints field is null  \n Response : {contactBasketGQLResult}");

            contactSoftAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, contactBasketGQLResult, out message), $"Contact Basket Validation failed: {message}");

            //Validate Flight price with offer price data
            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.summary.flightPrice.Value != null, $"Contact Basket Validation failed:  flightPrice field is null  \n Response : {contactBasketGQLResult}");

            contactSoftAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, contactBasketGQLResult, out errorMessage), $"Contact Basket Validation failed:  {errorMessage}");

            contactSoftAssert.AssertAll();
            //Term and Condition Graph QL
            var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);

            var tncBasketGQLResponse = GraphQLEndpoint.Book(tncBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic tncBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await tncBasketGQLResponse.Content.ReadAsStringAsync());

            var tncBasketSoftAssert = new SoftAssertHelper();

            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.basketId.Value == basketId, "Basket ID dont match");

            //MemberShip points valdation with offer price data
            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.summary.isAwardSearch.Value, "Term and condition Basket Validation failed: isAwardSearch is not True for redemption points scenario");

            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.summary.membershipPoints.Value != null, $"Term and condition Basket Validation failed: membershipPoints field is null  \n Response : {tncBasketGQLResult}");

            tncBasketSoftAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, tncBasketGQLResult, out message), $"Term and condition Basket Validation failed:  {message}");

            //Validate Flight price with offer price data
            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.summary.flightPrice.Value != null, $"Term and condition Basket Validation failed: flightPrice field is null  \n Response : {tncBasketGQLResult}");

            tncBasketSoftAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, tncBasketGQLResult, out errorMessage), $"Term and condition Basket Validation failed:  {errorMessage}");

            tncBasketSoftAssert.AssertAll();

            //Retrieve Basket Graph ql request
            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            softAssert = new SoftAssertHelper();

            //MemberShip points valdation with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value, $"Validation failed: isAwardSearch is not True for redemption points scenario {retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value}");

            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.membershipPoints.Value != null, $"Validation failed: membershipPoints field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate FC point with basket membership points
            softAssert.True(GraphQLAssertion.ValidateFcPointsEligibility_NegativeTest_InsufficientPoints(retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate Basket FC customer profile
            softAssert.True(retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value == "VS", $"Validation failed: The Basket FQTProgramme - {retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value} is not equal to FQTProgramme VS");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubNumber(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubTierCode(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubPoints(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubFirstName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubLastName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate Flight price with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.flightPrice.Value != null, $"flightPrice field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, retrieveBasketResult, out errorMessage), errorMessage);

            softAssert.True(retrieveBasketResult.data.retrieveBasket.validation.Value == "VALID", "Retreieve Basket Valdation Failed : Validation is Invalid");

            softAssert.AssertAll();
        }

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.LHR, LocationType.ATL, FareFamilyType.AWARDECONOMY, "Red", 1, 1, 1, 1)]
        public async Task RedemptionBasket_LeadPassenger_PositiveValidation(LocationType origin, LocationType destination, FareFamilyType fareFamily, string tierCode, int adults, int teens, int child, int infants)
        {
            var departureDate = OfferSearchHelper.DepartureDate();

            //generating offer search graph ql request.
            var offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(JourneyType.OneWay, departureDate, locationOne: origin, locationTwo: destination, numberOfAdults: adults, numberOfChildren: child, numberOfYouths: teens, numberOfInfants: infants, rewardSearch: true);

            //post search GRAPHQL endpoint.
            var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "").EnsureGQLResponseSuccess();

            if (GraphQlAssertions.ValidateOfferSearchGraphqlResponse(offerSearchResponse)) return;

            dynamic offerSearchResult = JsonConvert.DeserializeObject<dynamic>(await offerSearchResponse.Content.ReadAsStringAsync());

            //get basket id from search graphql response body
            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);

            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(offerSearchResponse.ReadResponseAsString(), fareFamily, FlightType.VS);

            Assert.True(summaryLinks.Any(), $"Summary link not returned for {fareFamily} and flight type {FlightType.VS}");

            //getting offer id from summary link
            string offerId = summaryLinks[0].Split('/').LastOrDefault();

            //generating offer price graph ql request.
            var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(JourneyType.OneWay, basketId, offerId);

            //post price GRAPHQL endpoint.
            var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureGQLResponseSuccess();

            dynamic offerPriceResult = JsonConvert.DeserializeObject<dynamic>(await offerPriceResponse.Content.ReadAsStringAsync());

            Assert.True(GraphQlAssertions.ValidateOfferIdExistsInSummaryResponse(offerPriceResult, out string summaryOffer), summaryOffer);

            var offerPriceMembershipPoints = offerPriceResult.data.searchSummary.price.awardPoints.Value;

            //login to AzureB2C
            Dictionary<string, string> loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tierCode, int.Parse(offerPriceMembershipPoints));

            string fcMembershipNumber = loyaltyDetails["rowKey"];

            var vRedResponse = MicroserviceEndpoint.Vred(tokenFixture.GetToken("vred"), fcMembershipNumber).EnsureIsSuccessStatusCode();

            dynamic vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

            int points = (int)vRedResult.balance.usable.Value;

            await  LoyaltyAccountUpdates.UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, points);

            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");

            Assert.True(!b2cToken.IsNullOrEmpty(), "B2C token is null");

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            var softAssert = new SoftAssertHelper();

            //MemberShip points valdation with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value, $"Validation failed: isAwardSearch is not True for redemption points scenario {retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value}");

            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.membershipPoints.Value != null, $"Validation failed: membershipPoints field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, retrieveBasketResult, out string message), $"Validation failed:  {message}");

            //Validate FC point with basket membership points
            softAssert.True(GraphQLAssertion.ValidateFcPointElibility(retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate Basket FC customer profile
            softAssert.True(retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value == "VS", $"Validation failed: The Basket FQTProgramme - {retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value} is not equal to FQTProgramme VS");

            softAssert.True(retrieveBasketResult.data.retrieveBasket.people[0].profilePassenger.Value, $"Validation failed: The Basket Profile passenger flag is false - {retrieveBasketResult.data.retrieveBasket.people[0].Value} suppose to be True VS");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubNumber(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubTierCode(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubPoints(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubFirstName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubLastName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate Flight price with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.flightPrice.Value != null, $"flightPrice field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, retrieveBasketResult, out string errorMessage), errorMessage);

            softAssert.AssertAll();

            //passenger update in basket
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId, false, "", b2cToken);

            GraphQLHelper.ContactBasketUpdateGraqphQL(out string emailId, out string phoneNumber, out string altPhoneNumber, out bool acceptPost, out bool acceptTelephone, out bool acceptEmail, out bool acceptTextMsg, out string countryCode);

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            var contactSoftAssert = new SoftAssertHelper();

            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == basketId, "Contact Basket Validation failed: Basket ID dont match");

            //MemberShip points valdation with offer price data
            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.summary.isAwardSearch.Value, "Contact Basket Validation failed: isAwardSearch is not True for redemption points scenario");

            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.summary.membershipPoints.Value != null, $"Contact Basket Validation failed: membershipPoints field is null  \n Response : {contactBasketGQLResult}");

            contactSoftAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, contactBasketGQLResult, out message), $"Contact Basket Validation failed: {message}");

            //Validate Flight price with offer price data
            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.summary.flightPrice.Value != null, $"Contact Basket Validation failed:  flightPrice field is null  \n Response : {contactBasketGQLResult}");

            contactSoftAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, contactBasketGQLResult, out errorMessage), $"Contact Basket Validation failed:  {errorMessage}");

            contactSoftAssert.AssertAll();
            //Term and Condition Graph QL
            var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);

            var tncBasketGQLResponse = GraphQLEndpoint.Book(tncBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic tncBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await tncBasketGQLResponse.Content.ReadAsStringAsync());

            var tncBasketSoftAssert = new SoftAssertHelper();

            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.basketId.Value == basketId, "Basket ID dont match");

            //MemberShip points valdation with offer price data
            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.summary.isAwardSearch.Value, "Term and condition Basket Validation failed: isAwardSearch is not True for redemption points scenario");

            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.summary.membershipPoints.Value != null, $"Term and condition Basket Validation failed: membershipPoints field is null  \n Response : {tncBasketGQLResult}");

            tncBasketSoftAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, tncBasketGQLResult, out message), $"Term and condition Basket Validation failed:  {message}");

            //Validate Flight price with offer price data
            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.summary.flightPrice.Value != null, $"Term and condition Basket Validation failed: flightPrice field is null  \n Response : {tncBasketGQLResult}");

            tncBasketSoftAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, tncBasketGQLResult, out errorMessage), $"Term and condition Basket Validation failed:  {errorMessage}");

            tncBasketSoftAssert.AssertAll();

            //Retrieve Basket Graph ql request
            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            softAssert = new SoftAssertHelper();

            //MemberShip points valdation with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value, $"Validation failed: isAwardSearch is not True for redemption points scenario {retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value}");

            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.membershipPoints.Value != null, $"Validation failed: membershipPoints field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate FC point with basket membership points
            softAssert.True(GraphQLAssertion.ValidateFcPointElibility(retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate Basket FC customer profile
            softAssert.True(retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value == "VS", $"Validation failed: The Basket FQTProgramme - {retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value} is not equal to FQTProgramme VS");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubNumber(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubTierCode(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubPoints(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubFirstName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubLastName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");

            //Validate Flight price with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.flightPrice.Value != null, $"flightPrice field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, retrieveBasketResult, out errorMessage), errorMessage);

            softAssert.True(retrieveBasketResult.data.retrieveBasket.validation.Value == "VALID", "Retreieve Basket Valdation Failed : Validation is Invalid");

            softAssert.AssertAll();
        }

        [Fact]
        [Trait("Category", Traits.BookGraphQLRegression)]
        public async Task PartialPWP_CoBrandEligibility_BasketUnder20K()
        {
            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, fareFamily: FareFamilyType.ECON, flightType: FlightType.VS, origin: LocationType.JFK, destination: LocationType.LHR, adults: 1, teens: 1, child: 1, infants: 1, rewardFlag: false, out string basketId, out dynamic offerPriceResult);

            string fcMembershipNumber = string.Empty;
            Dictionary<string, string> loyaltyDetails = new Dictionary<string, string>();

            //Get minium required points expected for points converter
            int passengerCount = 4;
            var minimumRequiredPoints = passengerCount * 3000;

            int maxAttempts = 5;
            int attempt = 0;
            int fqTPointsBeforeBooking = 0;
            while (attempt < maxAttempts)
            {
                loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tier: "Red", points: 100000, countryCode: "US");

                fcMembershipNumber = loyaltyDetails["rowKey"];

                var vRedResponseAPI = MicroserviceEndpoint.Vred(tokenFixture.GetToken("vred"), fcMembershipNumber).EnsureIsSuccessStatusCode();

                dynamic vRedResultAPI = JsonConvert.DeserializeObject<dynamic>(await vRedResponseAPI.Content.ReadAsStringAsync());

                int vredPoints = (int)vRedResultAPI.balance.usable.Value;

                await  LoyaltyAccountUpdates.UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, vredPoints);

                if (loyaltyDetails["points"] == vredPoints.ToString() && vredPoints <= 100000 && int.Parse(loyaltyDetails["points"]) >= minimumRequiredPoints)
                    break;

                attempt++;
            }

            Assert.True(int.Parse(loyaltyDetails["points"]) >= minimumRequiredPoints, $"FC account : {loyaltyDetails["rowKey"]}  points : {loyaltyDetails["points"]}  is not greater than {minimumRequiredPoints}");

            fqTPointsBeforeBooking = int.Parse(loyaltyDetails["points"]);

            //login to AzureB2C
            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");
            Assert.True(!b2cToken.IsNullOrEmpty(), "B2C token is null");

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //passenger update in basket
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId, false, "", b2cToken);
            GraphQLHelper.ContactBasketUpdateGraqphQL(out string emailId, out string phoneNumber, out string altPhoneNumber, out bool acceptPost, out bool acceptTelephone, out bool acceptEmail, out bool acceptTextMsg, out string countryCode);

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);
            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            //Term and Condition Graph QL
            var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);
            var tncBasketGQLResponse = GraphQLEndpoint.Book(tncBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            dynamic tncBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await tncBasketGQLResponse.Content.ReadAsStringAsync());

            //point selection in HPP page
            var pointToCurrencyConverterRequest = GraphQLRequestBuilder.PointToCurrencyConverter(basketId);

            var pointToCurrencyConverterResponse = GraphQLEndpoint.Book
                (pointToCurrencyConverterRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic pointToCurrencyConverterResult = JsonConvert.DeserializeObject<dynamic>(await pointToCurrencyConverterResponse.Content.ReadAsStringAsync());

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await tncBasketGQLResponse.Content.ReadAsStringAsync());

            //calculate points array list expected
            var pointsArrayListCount = fqTPointsBeforeBooking / minimumRequiredPoints;

            string passengerMesssage = $"passengerCount: ADT: {1}, Teen: {1}, Child:{1}, infant: {1} \n Minimum points : {minimumRequiredPoints} \n";
            string flyingClubMessage = $"FCMembershipNumber: {fcMembershipNumber}\n points in FC: {fqTPointsBeforeBooking}\n";

            Assert.True(GraphQLAssertion.ValidateCurrencyPayToCurrencyConverter(pointToCurrencyConverterResponse, retrieveBasketGQLResponse, out string errorMessage), $" {passengerMesssage} Basket ID : {basketId} \n {flyingClubMessage} Pay with points Currency Validation: {errorMessage} \n\tResponse : \n\t{pointToCurrencyConverterResult} ");

            //Select
            string selectedPoints = GraphQLHelper.PointsSelectionInCurrencyConverter(pointToCurrencyConverterResponse);

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.data.updateTnC.summary.totalPrice.Value;
            decimal amountToPayDecimal = (decimal)amountToPay;

            //Payment retrieve - PRI
            var paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(basketId, selectedPoints);
            var paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest, b2cToken).EnsureGQLResponseSuccess();

            GraphQLAssertion.ValidatePaymentRetreiveGQL(retrieveBasketGQLResponse, paymentRetGraphqlResponse);
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, out string hppUrl, out string pri);

            //Retrieve Basket Graph ql request
            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());
            Assert.True(GraphQLAssertion.CheckCoBrandEligibiltyStatusForPayWithPoints(retrieveBasketGQLResponse, amountToPayDecimal), $"Co-Brand Eligibility status is not correct {retrieveBasketResult}");
        }

        [Fact]
        [Trait("Category", Traits.BookGraphQLRegression)]
        public async Task PartialPWP_CoBrandEligibility_BasketGreaterThan20K()
        {
            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, fareFamily: FareFamilyType.ECON, flightType: FlightType.VS, origin: LocationType.JFK, destination: LocationType.LHR, adults: 6, teens: 1, child: 1, infants: 1, rewardFlag: false, out string basketId, out dynamic offerPriceResult);

            string fcMembershipNumber = string.Empty;
            Dictionary<string, string> loyaltyDetails = new Dictionary<string, string>();

            //Get minium required points expected for points converter
            int passengerCount = 9;
            var minimumRequiredPoints = passengerCount * 3000;

            int maxAttempts = 5;
            int attempt = 0;
            int fqTPointsBeforeBooking = 0;
            while (attempt < maxAttempts)
            {
                loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tier: "Red", points: 100000, countryCode: "US");

                fcMembershipNumber = loyaltyDetails["rowKey"];

                var vRedResponseAPI = MicroserviceEndpoint.Vred(tokenFixture.GetToken("vred"), fcMembershipNumber).EnsureIsSuccessStatusCode();

                dynamic vRedResultAPI = JsonConvert.DeserializeObject<dynamic>(await vRedResponseAPI.Content.ReadAsStringAsync());

                int vredPoints = (int)vRedResultAPI.balance.usable.Value;

                await  LoyaltyAccountUpdates.UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, vredPoints);

                if (loyaltyDetails["points"] == vredPoints.ToString() && vredPoints <= 100000 && int.Parse(loyaltyDetails["points"]) >= minimumRequiredPoints)
                    break;

                attempt++;
            }

            Assert.True(int.Parse(loyaltyDetails["points"]) >= minimumRequiredPoints, $"FC account : {loyaltyDetails["rowKey"]}  points : {loyaltyDetails["points"]}  is not greater than {minimumRequiredPoints}");

            fqTPointsBeforeBooking = int.Parse(loyaltyDetails["points"]);

            //login to AzureB2C
            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");
            Assert.True(!b2cToken.IsNullOrEmpty(), "B2C token is null");

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //passenger update in basket
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId, false, "", b2cToken);
            GraphQLHelper.ContactBasketUpdateGraqphQL(out string emailId, out string phoneNumber, out string altPhoneNumber, out bool acceptPost, out bool acceptTelephone, out bool acceptEmail, out bool acceptTextMsg, out string countryCode);

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);
            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            //Term and Condition Graph QL
            var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);
            var tncBasketGQLResponse = GraphQLEndpoint.Book(tncBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            dynamic tncBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await tncBasketGQLResponse.Content.ReadAsStringAsync());

            //point selection in HPP page
            var pointToCurrencyConverterRequest = GraphQLRequestBuilder.PointToCurrencyConverter(basketId);

            var pointToCurrencyConverterResponse = GraphQLEndpoint.Book
                (pointToCurrencyConverterRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic pointToCurrencyConverterResult = JsonConvert.DeserializeObject<dynamic>(await pointToCurrencyConverterResponse.Content.ReadAsStringAsync());

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await tncBasketGQLResponse.Content.ReadAsStringAsync());

            //calculate points array list expected
            var pointsArrayListCount = fqTPointsBeforeBooking / minimumRequiredPoints;

            string passengerMesssage = $"passengerCount: ADT: {6}, Teen: {1}, Child:{1}, infant: {1} \n Minimum points : {minimumRequiredPoints} \n";
            string flyingClubMessage = $"FCMembershipNumber: {fcMembershipNumber}\n points in FC: {fqTPointsBeforeBooking}\n";

            Assert.True(GraphQLAssertion.ValidateCurrencyPayToCurrencyConverter(pointToCurrencyConverterResponse, retrieveBasketGQLResponse, out string errorMessage), $" {passengerMesssage} Basket ID : {basketId} \n {flyingClubMessage} Pay with points Currency Validation: {errorMessage} \n\tResponse : \n\t{pointToCurrencyConverterResult} ");

            //Select
            string selectedPoints = GraphQLHelper.PointsSelectionInCurrencyConverter(pointToCurrencyConverterResponse);

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.data.updateTnC.summary.totalPrice.Value;
            decimal amountToPayDecimal = (decimal)amountToPay;

            //Payment retrieve - PRI
            var paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(basketId, selectedPoints);
            var paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest, b2cToken).EnsureGQLResponseSuccess();

            GraphQLAssertion.ValidatePaymentRetreiveGQL(retrieveBasketGQLResponse, paymentRetGraphqlResponse);
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, out string hppUrl, out string pri);

            //Retrieve Basket Graph ql request
            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());
            Assert.True(GraphQLAssertion.CheckCoBrandEligibiltyStatusForPayWithPoints(retrieveBasketGQLResponse, amountToPayDecimal), $"Co-Brand Eligibility status is not correct {retrieveBasketResult}");
        }

        [Fact]
        [Trait("Category", Traits.BookGraphQLRegression)]
        public async Task FullPWP_CoBrandEligibility_BasketUnder20K()
        {
            GraphQLHelper.OfferSearchGraphQL(JourneyType.OneWay, fareFamily: FareFamilyType.ECON, flightType: FlightType.VS, origin: LocationType.JFK, destination: LocationType.LHR, adults: 1, teens: 1, child: 1, infants: 1, rewardFlag: false, out string basketId, out dynamic offerPriceResult);

            string fcMembershipNumber = string.Empty;
            Dictionary<string, string> loyaltyDetails = new Dictionary<string, string>();

            //Get minium required points expected for points converter
            int passengerCount = 4;
            var minimumRequiredPoints = passengerCount * 3000;

            int maxAttempts = 5;
            int attempt = 0;
            int fqTPointsBeforeBooking = 0;
            while (attempt < maxAttempts)
            {
                loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tier: "Red", points: 1900000, countryCode: "US");

                fcMembershipNumber = loyaltyDetails["rowKey"];

                var vRedResponseAPI = MicroserviceEndpoint.Vred(tokenFixture.GetToken("vred"), fcMembershipNumber).EnsureIsSuccessStatusCode();

                dynamic vRedResultAPI = JsonConvert.DeserializeObject<dynamic>(await vRedResponseAPI.Content.ReadAsStringAsync());

                int vredPoints = (int)vRedResultAPI.balance.usable.Value;

                await  LoyaltyAccountUpdates.UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, vredPoints);

                if (loyaltyDetails["points"] == vredPoints.ToString() && vredPoints <= 1900000 && int.Parse(loyaltyDetails["points"]) >= minimumRequiredPoints)
                    break;

                attempt++;
            }

            Assert.True(int.Parse(loyaltyDetails["points"]) >= minimumRequiredPoints, $"FC account : {loyaltyDetails["rowKey"]}  points : {loyaltyDetails["points"]}  is not greater than {minimumRequiredPoints}");

            fqTPointsBeforeBooking = int.Parse(loyaltyDetails["points"]);

            //login to AzureB2C
            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");
            Assert.True(!b2cToken.IsNullOrEmpty(), "B2C token is null");

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //passenger update in basket
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId, false, "", b2cToken);
            GraphQLHelper.ContactBasketUpdateGraqphQL(out string emailId, out string phoneNumber, out string altPhoneNumber, out bool acceptPost, out bool acceptTelephone, out bool acceptEmail, out bool acceptTextMsg, out string countryCode);

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);
            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            //Term and Condition Graph QL
            var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);
            var tncBasketGQLResponse = GraphQLEndpoint.Book(tncBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            dynamic tncBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await tncBasketGQLResponse.Content.ReadAsStringAsync());

            //point selection in HPP page
            var pointToCurrencyConverterRequest = GraphQLRequestBuilder.PointToCurrencyConverter(basketId);

            var pointToCurrencyConverterResponse = GraphQLEndpoint.Book
                (pointToCurrencyConverterRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic pointToCurrencyConverterResult = JsonConvert.DeserializeObject<dynamic>(await pointToCurrencyConverterResponse.Content.ReadAsStringAsync());

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await tncBasketGQLResponse.Content.ReadAsStringAsync());

            //calculate points array list expected
            var pointsArrayListCount = fqTPointsBeforeBooking / minimumRequiredPoints;

            string passengerMesssage = $"passengerCount: ADT: {1}, Teen: {1}, Child:{1}, infant: {1} \n Minimum points : {minimumRequiredPoints} \n";
            string flyingClubMessage = $"FCMembershipNumber: {fcMembershipNumber}\n points in FC: {fqTPointsBeforeBooking}\n";

            Assert.True(GraphQLAssertion.ValidateCurrencyPayToCurrencyConverter(pointToCurrencyConverterResponse, retrieveBasketGQLResponse, out string errorMessage), $" {passengerMesssage} Basket ID : {basketId} \n {flyingClubMessage} Pay with points Currency Validation: {errorMessage} \n\tResponse : \n\t{pointToCurrencyConverterResult} ");

            //Select
            string selectedPoints = GraphQLHelper.FullPointsSelectionInCurrencyConverter(pointToCurrencyConverterResponse);

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.data.updateTnC.summary.totalPrice.Value;
            decimal amountToPayDecimal = (decimal)amountToPay;

            //Payment retrieve - PRI
            var paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(basketId, selectedPoints);
            var paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest, b2cToken).EnsureGQLResponseSuccess();

            GraphQLAssertion.ValidatePaymentRetreiveGQL(retrieveBasketGQLResponse, paymentRetGraphqlResponse);

            //Retrieve Basket Graph ql request
            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());
            Assert.False(GraphQLAssertion.CheckCoBrandEligibiltyStatusForPayWithPoints(retrieveBasketGQLResponse, amountToPayDecimal), $"Co-Brand Eligibility status is not correct {retrieveBasketResult}");
        }

        #endregion Redemption OneWay

        #region Redemption Return

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [InlineData(LocationType.JFK, LocationType.DEL, FareFamilyType.AWARDECONOMY, FlightType.VS, "Red", 1, 1, 1, 1)]
        [InlineData(LocationType.ATL, LocationType.LHR, FareFamilyType.AWARDECONOMY, FlightType.VS, "Silver", 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.AWARDBUSINESSFIRST, FlightType.VS, "Gold", 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.JFK, FareFamilyType.AWARDECONOMY, FlightType.DL, "Red", 1, 1, 1, 1)]
        [InlineData(LocationType.ATL, LocationType.LHR, FareFamilyType.AWARDECONOMY, FlightType.DL, "Silver", 1, 1, 1, 1)]
        [InlineData(LocationType.LHR, LocationType.MCO, FareFamilyType.AWARDBUSINESSFIRST, FlightType.DL, "Gold", 1, 1, 1, 1)]
        public async Task Basket_FCloginValidation_Return(LocationType origin, LocationType destination, FareFamilyType fareFamily, FlightType flightType, string tierCode, int adults, int teens, int child, int infants)
        {
            GraphQLHelper.OfferSearchGraphQL(JourneyType.Return, fareFamily, flightType, origin, destination, adults, teens, child, infants, rewardFlag: true, out string basketId, out dynamic offerPriceResult);

            var offerPriceMembershipPoints = offerPriceResult.data.searchSummary.price.awardPoints.Value;

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

            //login to AzureB2C
            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");

            Assert.True(!b2cToken.IsNullOrEmpty(), "B2C token is null");

            //Retrieve Basket Graph ql request
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            var retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            var softAssert = new SoftAssertHelper();

            //MemberShip points valdation with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value, $"Retrieve Basket Validation failed: isAwardSearch is not True for redemption points scenario {retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value}");

            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.membershipPoints.Value != null, $"Retrieve Basket Validation failed: membershipPoints field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, retrieveBasketResult, out string message), $"Retrieve Basket Validation failed:  {message}");

            //Validate FC point with basket membership points
            softAssert.True(GraphQLAssertion.ValidateFcPointElibility(retrieveBasketResult, out message), $"Retrieve Basket Validation failed:  {message}");

            //Validate Basket FC customer profile
            softAssert.True(retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value == "VS", $"Retrieve Basket Validation failed: The Basket FQTProgramme - {retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value} is not equal to FQTProgramme VS");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubNumber(loyaltyDetails, retrieveBasketResult, out message), $"Retrieve Basket Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubTierCode(loyaltyDetails, retrieveBasketResult, out message), $"Retrieve Basket Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubPoints(loyaltyDetails, retrieveBasketResult, out message), $"Retrieve Basket Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubFirstName(loyaltyDetails, retrieveBasketResult, out message), $"Retrieve Basket Validation failed:  {message}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubLastName(loyaltyDetails, retrieveBasketResult, out message), $"Retrieve Basket Validation failed:  {message}");

            //Validate Flight price with offer price data
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.flightPrice.Value != null, $"Retrieve Basket Validation failed: flightPrice field is null  \n Response : {retrieveBasketResult}");

            softAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, retrieveBasketResult, out string errorMessage), errorMessage);

            softAssert.AssertAll();

            //passenger update in basket
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId, false, "", b2cToken);

            GraphQLHelper.ContactBasketUpdateGraqphQL(out string emailId, out string phoneNumber, out string altPhoneNumber, out bool acceptPost, out bool acceptTelephone, out bool acceptEmail, out bool acceptTextMsg, out string countryCode);

            //Contact Basket Graph QL
            var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(basketId, emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

            var contactSoftAssert = new SoftAssertHelper();

            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == basketId, "Contact Basket Validation failed: Basket ID dont match");

            //MemberShip points valdation with offer price data
            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.summary.isAwardSearch.Value, "Contact Basket Validation failed: isAwardSearch is not True for redemption points scenario");

            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.summary.membershipPoints.Value != null, $"Contact Basket Validation failed: membershipPoints field is null  \n Response : {contactBasketGQLResult}");

            contactSoftAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, contactBasketGQLResult, out message), $"Contact Basket Validation failed: {message}");

            //Validate Flight price with offer price data
            contactSoftAssert.True(contactBasketGQLResult.data.updateContactDetails.summary.flightPrice.Value != null, $"Contact Basket Validation failed:  flightPrice field is null  \n Response : {contactBasketGQLResult}");

            contactSoftAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, contactBasketGQLResult, out errorMessage), $"Contact Basket Validation failed:  {errorMessage}");

            contactSoftAssert.AssertAll();

            //Term and Condition Graph QL
            var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);

            var tncBasketGQLResponse = GraphQLEndpoint.Book(tncBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            var tncBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await tncBasketGQLResponse.Content.ReadAsStringAsync());

            var tncBasketSoftAssert = new SoftAssertHelper();

            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.basketId.Value == basketId, "Basket ID dont match");

            //MemberShip points valdation with offer price data
            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.summary.isAwardSearch.Value, "Term and condition Basket Validation failed: isAwardSearch is not True for redemption points scenario");

            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.summary.membershipPoints.Value != null, $"Term and condition Basket Validation failed: membershipPoints field is null  \n Response : {tncBasketGQLResult}");

            tncBasketSoftAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, tncBasketGQLResult, out message), $"Term and condition Basket Validation failed:  {message}");

            //Validate Flight price with offer price data
            tncBasketSoftAssert.True(tncBasketGQLResult.data.updateTnC.summary.flightPrice.Value != null, $"Term and condition Basket Validation failed: flightPrice field is null  \n Response : {tncBasketGQLResult}");

            tncBasketSoftAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, tncBasketGQLResult, out errorMessage), $"Term and condition Basket Validation failed:  {errorMessage}");

            tncBasketSoftAssert.AssertAll();

            //Retrieve Basket Graph ql request
            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);

            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

            retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            var retrieveBasketAssert = new SoftAssertHelper();

            //MemberShip points valdation with offer price data
            retrieveBasketAssert.True(retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value, $"Retrieve Basket Validation failed: isAwardSearch is not True for redemption points scenario {retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value}");

            retrieveBasketAssert.True(retrieveBasketResult.data.retrieveBasket.summary.membershipPoints.Value != null, $"Retrieve Basket Validation failed: membershipPoints field is null  \n Response : {retrieveBasketResult}");

            retrieveBasketAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, retrieveBasketResult, out message), $"Retrieve Basket Validation failed:  {message}");

            //Validate FC point with basket membership points
            retrieveBasketAssert.True(GraphQLAssertion.ValidateFcPointElibility(retrieveBasketResult, out message), $"Retrieve Basket Validation failed:  {message}");

            //Validate Basket FC customer profile
            retrieveBasketAssert.True(retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value == "VS", $"Retrieve Basket Validation failed: The Basket FQTProgramme - {retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value} is not equal to FQTProgramme VS");

            retrieveBasketAssert.True(GraphQLAssertion.ValidateBasketFlyingClubNumber(loyaltyDetails, retrieveBasketResult, out message), $"Retrieve Basket Validation failed:  {message}");

            retrieveBasketAssert.True(GraphQLAssertion.ValidateBasketFlyingClubTierCode(loyaltyDetails, retrieveBasketResult, out message), $"Retrieve Basket Validation failed:  {message}");

            retrieveBasketAssert.True(GraphQLAssertion.ValidateBasketFlyingClubPoints(loyaltyDetails, retrieveBasketResult, out message), $"Retrieve Basket Validation failed:  {message}");

            retrieveBasketAssert.True(GraphQLAssertion.ValidateBasketFlyingClubFirstName(loyaltyDetails, retrieveBasketResult, out message), $"Retrieve Basket Validation failed:  {message}");

            retrieveBasketAssert.True(GraphQLAssertion.ValidateBasketFlyingClubLastName(loyaltyDetails, retrieveBasketResult, out message), $"Retrieve Basket Validation failed:  {message}");

            //Validate Flight price with offer price data
            retrieveBasketAssert.True(retrieveBasketResult.data.retrieveBasket.summary.flightPrice.Value != null, $"Retrieve Basket Validation failed: flightPrice field is null  \n Response : {retrieveBasketResult}");

            retrieveBasketAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, retrieveBasketResult, out errorMessage), errorMessage);

            softAssert.True(retrieveBasketResult.data.retrieveBasket.validation.Value == "VALID", "Retreieve Basket Valdation Failed : Validation is Invalid");

            retrieveBasketAssert.AssertAll();
        }

        #endregion Redemption Return
    }
}