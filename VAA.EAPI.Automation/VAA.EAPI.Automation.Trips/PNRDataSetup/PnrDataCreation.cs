using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Constants;
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
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Book.Provisioning.Pnr;
using VAA.EAPI.Automation.MyAccount.Helpers;
using Config = VAA.EAPI.Automation.Core.Config;
using PnrDataAwardScenarios = VAA.EAPI.Automation.Trips.DataProvider.PnrDataAwardScenarios;
using PnrDataCreationTestCases = VAA.EAPI.Automation.Trips.DataProvider.PnrDataScenarios;

namespace VAA.EAPI.Automation.Trips.PNRDataSetup
{
    public class PnrDataCreation : IClassFixture<TokenFixture>
    {
        public static string env = Config.Environment;
        private TestConfig _testConfig;
        private readonly TokenFixture _tokenFixture;
        private readonly LoyaltyServiceHelper loyaltyServiceHelper;

        public PnrDataCreation(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForVRedFromKeyVault("vred");

            _tokenFixture.AddTokenForAzureB2CFromKeyVault("azureGraphB2C");

            loyaltyServiceHelper = new LoyaltyServiceHelper(tokenFixture);
        }


        [Theory]
        [MemberData(nameof(GetLoyaltyTestCases))]
        [Trait("Category", Traits.PNRsWithFlyingClubMembers)]
        public async Task LoyaltyPnrDataCreation(LocationType origin, LocationType destination, JourneyType journeyType, int adults, int youngAdults, int children, int infants, FlightType airline, FareFamilyType fareFamily, CabinName cabinName, bool seats, bool loggedInUser, string paymentType, bool oci)
        {
            await ExecutePnrCreation(origin, destination, journeyType, adults, youngAdults, children, infants, airline, fareFamily, cabinName, seats, loggedInUser, paymentType, oci);
        }

        [Theory]
        [MemberData(nameof(GetGlassTierTestCases))]
        [Trait("Category", Traits.PNRsWithGlassTierDetails)]
        public async Task GlassTierPnrDataCreation(LocationType origin, LocationType destination, JourneyType journeyType, int adults, int youngAdults, int children, int infants, FlightType airline, FareFamilyType fareFamily, CabinName cabinName, bool seats, bool loggedInUser, string paymentType, bool oci)
        {
            await ExecutePnrCreation(origin, destination, journeyType, adults, youngAdults, children, infants, airline, fareFamily, cabinName, seats, loggedInUser, paymentType, oci);
        }

        [Theory]
        [MemberData(nameof(GetAwardTestCases))]
        [Trait("Category", Traits.PNRsWithAwardBookings)]
        public async Task AwardPnrDataCreation(LocationType origin, LocationType destination, JourneyType journeyType, int adults, int youngAdults, int children, int infants, FlightType airline, FareFamilyType fareFamily, CabinName cabinName, bool seats, bool loggedInUser, string paymentType, bool oci)
        {
            await ExecutePnrCreation(origin, destination, journeyType, adults, youngAdults, children, infants, airline, fareFamily, cabinName, seats, loggedInUser, paymentType, oci);
        }

        public static IEnumerable<object[]> GetLoyaltyTestCases()
        {
            return PnrDataCreationTestCases.TestCases().Where(testCase => (bool)testCase[11] == true);
        }

        public static IEnumerable<object[]> GetGlassTierTestCases()
        {
            return PnrDataCreationTestCases.TestCases().Where(testCase => (bool)testCase[11] == false);
        }
        public static IEnumerable<object[]> GetAwardTestCases()
        {
            return PnrDataAwardScenarios.AwardTestCases();
        }

        private async Task ExecutePnrCreation(LocationType origin, LocationType destination, JourneyType journeyType, int adults, int youngAdults, int children, int infants, FlightType airline, FareFamilyType fareFamily, CabinName cabinName, bool seats, bool loggedInUser, string paymentType, bool oci)
        {
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                #region Test Configuration Setup

                _testConfig = new TestConfig();

                if (oci)
                {
                    DateTime tripDate = DataGenerator.RandomDateBetweenTwoDates(DateTime.Now, DateTime.Now.AddDays(3));
                    _testConfig.Search.DepartureDate = tripDate.ToString("yyyy-MM-dd");
                }
                // Custom departure date logic for LHR->SVD and LHR->GND
                else if (origin.ToString() == "LHR" && destination.ToString() == "SVD")
                {
                    // this route flies on a Sunday 
                    string dayOfTheWeek = DayOfWeek.Sunday.ToString();
                    _testConfig.Search.DepartureDate = DataGenerator.GetDepartureDate(dayOfTheWeek);
                }
                else if (origin.ToString() == "LHR" && destination.ToString() == "GND")
                {
                    // this route flies on a Saturday
                    string dayOfTheWeek = DayOfWeek.Saturday.ToString();
                    _testConfig.Search.DepartureDate = DataGenerator.GetDepartureDate(dayOfTheWeek);
                }
                else
                {
                    _testConfig.Search.DepartureDate = DataGenerator.DepartureDate();
                }

                _testConfig.Search.ReturnDate = DataGenerator.ReturnDate(_testConfig.Search.DepartureDate);
                _testConfig.Search.Origin = origin;
                _testConfig.Search.IsRewardSearch = fareFamily is FareFamilyType.AWARDECONOMY or FareFamilyType.AWARDBUSINESSFIRST;
                _testConfig.Search.Destination = destination;
                _testConfig.Search.JourneyType = journeyType;
                _testConfig.Search.Passengers.AddPassengers(adults: adults, youngAdults: youngAdults, children: children, infants: infants);
                _testConfig.Search.FlightType = airline;
                _testConfig.Search.FareFamilyType = fareFamily;
                _testConfig.Search.CabinName = cabinName;
                _testConfig.Internal.IsSeats = seats;
                _testConfig.Internal.IsLoggedInUser = loggedInUser;
                _testConfig.Internal.Bearer = "";
                #endregion

                #region Loyalty Account Setup
                string b2cToken = string.Empty;
                string originalTierCode = string.Empty;
                string originalFcNumber = string.Empty;

                if (loggedInUser)
                {
                    int pointsRequired = 100000;

                    // Randomize tier selection: Red, Silver, or Gold
                    string[] tiers = { "Red", "Silver", "Gold" };
                    Random random = new();
                    string selectedTier = tiers[random.Next(tiers.Length)];

                    Dictionary<string, string> loyaltyDetails;
                    try
                    {
                        loyaltyDetails = await loyaltyServiceHelper.GetLoyaltyAccountDetails(selectedTier, pointsRequired);

                        // Check if critical values are empty/null
                        if (string.IsNullOrEmpty(loyaltyDetails["rowKey"]) || string.IsNullOrEmpty(loyaltyDetails["tier"]))
                        {
                            throw new InvalidOperationException("Empty loyalty details returned");
                        }
                    }
                    catch
                    {
                        // Fallback to Red tier if selected tier fails or returns empty values
                        selectedTier = "Red";
                        loyaltyDetails = await loyaltyServiceHelper.GetLoyaltyAccountDetails(selectedTier, pointsRequired);
                    }

                    string fcMembershipNumber = loyaltyDetails["rowKey"];
                    string loyaltyTierCode = loyaltyDetails["tier"];
                    _testConfig.Book.Order.PreBookingFCPoints = loyaltyDetails["points"].ToString();
                    _testConfig.Book.Order.FqtTierCode = selectedTier;

                    // Store original values for restoration later
                    originalTierCode = selectedTier;
                    originalFcNumber = fcMembershipNumber;

                    b2cToken = await GenerateLoginB2CTokenFromApi(fcMembershipNumber, loyaltyDetails);

                    _testConfig.Internal.Bearer = b2cToken;
                }
                #endregion

                #region Search and Basket Setup

                GraphQLHelper.OfferSearchGraphQL(_testConfig);

                GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(_testConfig.Internal.BasketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse, b2cToken: b2cToken);

                double totalSeatPrice = 0.00;

                if (seats)
                {
                    bool responseBool = GraphQLHelper.RetrieveSeatMapGraphQL(_testConfig.Internal.BasketId, sectorId, fareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message, b2cToken: b2cToken);

                    if (responseBool)
                    {
                        GraphQLHelper.SelectSeatsForAllSectors(_testConfig.Internal.BasketId, fareFamily, retrieveBasketGQLResponse, cabinName, out totalSeatPrice);
                    }
                }

                string retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(_testConfig.Internal.BasketId);

                retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();

                JObject retrieveBasketGQLResult = JsonConvert.DeserializeObject<JObject>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

                string emailAddressFromBasketResponse = retrieveBasketGQLResult.SelectToken("$.data.retrieveBasket.contactDetails.emailAddress").ToString();

                #endregion

                #region Payment Processing

                string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
                HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest, b2cToken).EnsureGQLResponseSuccess();
                GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

                JObject paymentRetGraphqlResult = JsonConvert.DeserializeObject<JObject>(await paymentRetGraphqlResponse.Content.ReadAsStringAsync());
                bool isAwardSearch = paymentRetGraphqlResult.SelectToken("$.data.bookOrderPaymentReference.summary.selectedPayWithPointsOption")?.Value<string>() != null;

                if (paymentType == "PayPal")
                {
                    await HostedPaymentAutomation.SubmitPayPalAsync(
                        _testConfig,
                        expectedTotalPrice: totalSeatPrice,
                        rewardBooking: _testConfig.Search.IsRewardSearch);
                }
                else
                {
                    _testConfig.Book.Payment = new Payment(CardType.Visa);
                    // Previous browser flow: HostedPaymentAutomation.SubmitCardAsync(_testConfig, expectedTotalPrice: totalSeatPrice, rewardBooking: _testConfig.Search.IsRewardSearch);
                    await ApiHostedPaymentAutomation.SubmitCardAsync(_testConfig);
                }

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

                #region Post-Booking Data activites

                if (!loggedInUser)
                {
                    // Set default values for VAC users
                    if (string.IsNullOrEmpty(_testConfig.Book.Order.FqtTierCode))
                        _testConfig.Book.Order.FqtTierCode = "Glass";
                    if (string.IsNullOrEmpty(_testConfig.Book.Order.FlyingClubNumber))
                        _testConfig.Book.Order.FlyingClubNumber = "N/A";
                    if (string.IsNullOrEmpty(_testConfig.Book.Order.FqtProgramme))
                        _testConfig.Book.Order.FqtProgramme = "N/A";
                }
                else
                {
                    // Restore original loyalty details from login setup
                    _testConfig.Book.Order.FqtTierCode = originalTierCode;
                    _testConfig.Book.Order.FlyingClubNumber = originalFcNumber;
                }

                if ((origin == LocationType.LHR && destination == LocationType.SVD) || (origin == LocationType.LHR && destination == LocationType.GND))
                {
                    if (journeyType == JourneyType.OneWay)
                        _testConfig.Search.JourneyType = JourneyType.OneWayWithStopover;
                    else if (journeyType == JourneyType.Return)
                        _testConfig.Search.JourneyType = JourneyType.ReturnWithStopover;
                }

                //Azure storage data
                // Update journey type to include "ConnectingFlights" before writing to storage only for DEL->MIA or LHR->AMD

                if ((origin == LocationType.LHR && destination == LocationType.AMD) || (origin == LocationType.DEL && destination == LocationType.MIA))
                {
                    if (journeyType == JourneyType.OneWay)
                        _testConfig.Search.JourneyType = JourneyType.OneWayWithConnectingFlight;
                    else if (journeyType == JourneyType.Return)
                        _testConfig.Search.JourneyType = JourneyType.ReturnWithConnectingFlight;
                }
                #endregion

                #region Azure Storage

                await WriteToAzureStorage.AddPNRsForMobileAppToAzureStorageTable(_testConfig);

                if (loggedInUser) // for loggedinuser -award bookings - This minimal change needed is to conditionally write to the seat scenarios table only when it's an award booking.
                {
                    await WriteToAzureStorage.AddPNRsForSeatScenariosToAzureStorageTable(_testConfig);
                }

                #endregion

                Assert.True(true);
            }, 1);
        }

        private static async Task<string> GenerateLoginB2CTokenFromApi(string fcMembershipNumber, Dictionary<string, string> loyaltyDetails)
        {
            Assert.True(!fcMembershipNumber.IsNullOrWhiteSpace(), "FC membership number is required to generate a B2C token.");

            string email = loyaltyDetails.TryGetValue("email", out string loyaltyEmail) ? loyaltyEmail : string.Empty;
            HttpResponseMessage signInResponse = MyAccount.Requests.Endpoint.GraphQL
                .AccountMemberSignIn(env, email, fcMembershipNumber, Vault.Default.GetSecretValue("MYA-Password"))
                .EnsureGQLResponseSuccess();

            JObject signInResult = JObject.Parse(await signInResponse.Content.ReadAsStringAsync());
            string accessToken = signInResult.SelectToken("data.accountMemberSignIn.accessToken")?.ToString();

            Assert.True(
                !accessToken.IsNullOrWhiteSpace(),
                $"B2C token is null from API sign-in. Environment={env}, FcMembershipNumberSupplied={!fcMembershipNumber.IsNullOrWhiteSpace()}, EmailSupplied={!email.IsNullOrWhiteSpace()}");

            return accessToken;
        }
    }
}
