using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Book.Provisioning.Pnr;
using VAA.EAPI.Automation.MyAccount.Helpers;
using Config = VAA.EAPI.Automation.Core.Config;
using PnrDataCreationTestCases = VAA.EAPI.Automation.Trips.DataProvider.SeatSelectionScenarios;

namespace VAA.EAPI.Automation.Trips.PNRDataSetup
{
    public class PnrsForSeatScenarios : IClassFixture<TokenFixture>
    {
        public static string env = Config.Environment;
        private TestConfig _testConfig;
        private readonly TokenFixture _tokenFixture;
        private readonly LoyaltyServiceHelper loyaltyServiceHelper;

        public PnrsForSeatScenarios(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _tokenFixture.AddTokenForVRedFromKeyVault("vred");
            _tokenFixture.AddTokenForAzureB2CFromKeyVault("azureGraphB2C");
            loyaltyServiceHelper = new LoyaltyServiceHelper(tokenFixture);
        }

        [Theory]
        [MemberData(nameof(PnrDataCreationTestCases.PaidSeatsTestCases), true, MemberType = typeof(PnrDataCreationTestCases))]
        [Trait("Category", Traits.MobileAppPnrsSeatsTesting)]
        public async Task MobilePnrDataCreationForSeats(LocationType origin, LocationType destination, JourneyType journeyType, int adults, int youngAdults, int children, int infants, FlightType airline, FareFamilyType fareFamily, CabinName cabinName, bool seats, bool loggedInUser)
        {
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                #region Test Configuration Setup
                _testConfig = new TestConfig();

                _testConfig.Search.DepartureDate = DataGenerator.DepartureDate();
                _testConfig.Search.ReturnDate = DataGenerator.ReturnDate(_testConfig.Search.DepartureDate);
                _testConfig.Search.Origin = origin;
                _testConfig.Search.Destination = destination;
                _testConfig.Search.JourneyType = journeyType;
                _testConfig.Search.Passengers.AddPassengers(adults: adults, youngAdults: youngAdults, children: children, infants: infants);
                _testConfig.Search.FlightType = airline;
                _testConfig.Search.FareFamilyType = fareFamily;
                _testConfig.Search.CabinName = cabinName;
                _testConfig.Internal.IsSeats = seats;
                _testConfig.Internal.IsLoggedInUser = loggedInUser;
                _testConfig.Internal.Bearer = "";

                string b2cToken = string.Empty;
                #endregion

                #region Search and Basket Setup
                GraphQLHelper.OfferSearchGraphQL(_testConfig);

                GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(_testConfig.Internal.BasketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse, b2cToken: b2cToken);

                double totalSeatPrice = 0.00;

                string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);

                GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

                JObject retrieveBasketGQLResult = JsonConvert.DeserializeObject<JObject>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

                string emailAddressFromBasketResponse = retrieveBasketGQLResult.SelectToken("$.data.retrieveBasket.contactDetails.emailAddress").ToString();

                //Adding Email from basket to TestConfig for future use in password reset for VAC creation - this is added as for DL flights email id populated from confirmation response is not matching with the one in basket response whereas for VS is matching.

                _testConfig.Book.Order.EmailFromBasket = emailAddressFromBasketResponse;

                #endregion

                #region Payment Processing
                string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
                HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest, b2cToken).EnsureGQLResponseSuccess();
                GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

                // selectedPayWithPointsOption field from the payment response. If this field has a value (not null), it means the booking is an award booking paid with points. Otherwise, it's a revenue booking paid with credit card
                JObject paymentRetGraphqlResult = JsonConvert.DeserializeObject<JObject>(await paymentRetGraphqlResponse.Content.ReadAsStringAsync());
                bool isAwardSearch = paymentRetGraphqlResult.SelectToken("$.data.bookOrderPaymentReference.summary.selectedPayWithPointsOption")?.Value<string>() != null;
                _testConfig.Book.Order.IsAward = isAwardSearch;

                _testConfig.Book.Payment = new Payment(CardType.Visa);
                // Previous browser flow: HostedPaymentAutomation.SubmitCardAsync(_testConfig, usePlaywright: true, headless: true, expectedTotalPrice: totalSeatPrice);
                await ApiHostedPaymentAutomation.SubmitCardAsync(_testConfig);

                //PostPayment status
                string paymentStatusGraphqlRequest = GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(_testConfig.Internal.BasketId, _testConfig.Book.Order.PRI);
                HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, b2cToken).EnsureGQLResponseSuccess();
                dynamic paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

                Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");
                #endregion

                #region Order Creation
                if (!await GraphQLHelper.ExecuteOrderCreate(_testConfig, b2cToken))
                    return;
                #endregion

                #region Azure Storage
                await WriteToAzureStorage.AddPNRsForSeatScenariosToAzureStorageTable(_testConfig);
                #endregion

                #region Post-Booking Data activites

                if (!loggedInUser)
                {
                    // Set default values for VAC users
                    if (string.IsNullOrEmpty(_testConfig.Book.Order.FqtTierCode))
                        _testConfig.Book.Order.FqtTierCode = "VAC";
                    if (string.IsNullOrEmpty(_testConfig.Book.Order.FlyingClubNumber))
                        _testConfig.Book.Order.FlyingClubNumber = "N/A";
                    if (string.IsNullOrEmpty(_testConfig.Book.Order.FqtProgramme))
                        _testConfig.Book.Order.FqtProgramme = "N/A";
                }

                #endregion

                Assert.True(true);
            }, 1);
        }
    }
}
