using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.DataProvider;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Automation.OCI.Constants;
using VAA.EAPI.Automation.OCI.Helpers;

namespace VAA.EAPI.Automation.OCI.Tests.PnrsCreation
{
    [Collection("RedemptionPNRCreation")]
    public class RedemptionPNRCreationTest : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly LoyaltyServiceHelper loyaltyServiceHelper;
        protected readonly TestConfig _testConfig;

        protected virtual Task WritePnrToStorage(TestConfig config)
            => WriteToAzureStorage.AddOCIPNRToAzureStorageTable(config);

        public RedemptionPNRCreationTest(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
            this.tokenFixture.AddTokenForVRedFromKeyVault("vred");
            loyaltyServiceHelper = new LoyaltyServiceHelper(tokenFixture);
            _testConfig = new TestConfig();
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreation)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetRedemptionPNRCreationData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public Task RedemptionPNRCreationForOneway(LocationType origin, LocationType destination, string tierCode, int departureOffset)
        {
            return CreateRedemptionPnr(origin, destination, tierCode, departureOffset, JourneyType.OneWay, "OneWayScenarioRedemption");
        }

        [Theory]
        [Trait("Category", Traits.OCIPNRCreation)]
        [MemberData(nameof(OCIPnrDataCreationTestCases.GetRedemptionPNRCreationData), MemberType = typeof(OCIPnrDataCreationTestCases))]
        public Task RedemptionPNRCreationForReturn(LocationType origin, LocationType destination, string tierCode, int departureOffset)
        {
            return CreateRedemptionPnr(origin, destination, tierCode, departureOffset, JourneyType.Return, "ReturnScenarioRedemption");
        }

        private async Task CreateRedemptionPnr(LocationType origin, LocationType destination, string tierCode, int departureOffset, JourneyType journeyType, string testCaseName)
        {
            _testConfig.Search.IsRewardSearch = true;
            _testConfig.Search.Origin = origin;
            _testConfig.Search.Destination = destination;
            _testConfig.Search.JourneyType = journeyType;
            _testConfig.Search.FareFamilyType = FareFamilyType.AWARDECONOMY;
            _testConfig.Search.Passengers.AddPassengers(adults: 1, 1, 1, 1);
            _testConfig.Search.DepartureDate = OCIHelper.GetDateFromTodayPlusDays(departureOffset);

            if (journeyType == JourneyType.Return)
            {
                _testConfig.Search.InboundFareFamilyType = FareFamilyType.AWARDECONOMY;
                _testConfig.Search.ReturnDate = DataGenerator.ReturnDate(_testConfig.Search.DepartureDate);
            }

            _testConfig.Internal.TestCaseName = testCaseName;

            Dictionary<string, string> loyaltyDetails = await loyaltyServiceHelper.GetLoyaltyAccountDetails(tierCode, 1000000);

            string fcMembershipNumber = loyaltyDetails["rowKey"];

            _testConfig.Internal.FlyingClubNumber = fcMembershipNumber;

            //login to AzureB2C
            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, Vault.Default.GetSecretValue("FCLoginGenericPwd"));

            Assert.True(!b2cToken.IsNullOrEmpty(), "B2C token is null");

            _testConfig.Internal.Bearer = b2cToken;

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);

            //Retrieve Basket Graph ql request
            string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken beforeInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Passenger Basket update
            GraphQLRequestBuilder.PassengerBasketGraphQLRequest(beforeInsuranceBasketResponse, _testConfig.Internal.BasketId, b2cToken: _testConfig.Internal.Bearer);

            //Contact Basket GraphQL
            string contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
            GraphQLEndpoint.Book(contactBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();

            //Terms and Condition GraphQL
            string tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(_testConfig.Internal.BasketId);
            GraphQLEndpoint.Book(tncBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();

            retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);
            retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            JToken afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            //Payment retrieve - PRI
            string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
            HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            _testConfig.Book.Payment = new Payment(CardType.Visa);

            await HostedPaymentAutomation.SubmitCardAsync(_testConfig);

            //Post Payment Status
            string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
            HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            //Order Create
            if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, _testConfig.Internal.Bearer))
                return;

            //Assert PNR data in order document
            Assert.True(GraphQLAssertion.ValidatePnrOrderDocument(_testConfig.Book.Order.PNR, out string info), $"Order Validation:  {info}");

            //update point back to Test storage account
            HttpResponseMessage vRedResponse = Microservice.Vred(tokenFixture.GetToken("vred"), fcMembershipNumber).EnsureIsSuccessStatusCode();

            dynamic vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

            int points = (int)vRedResult.balance.usable.Value;

            await LoyaltyAccountUpdates.UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, points);

            //Azure storage data
            await WritePnrToStorage(_testConfig);
        }
    }
}
