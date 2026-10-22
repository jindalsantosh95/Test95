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
using PnrDataCreationTestCases = VAA.EAPI.Automation.Trips.DataProvider.LoyaltyAccountsScenarios;

namespace VAA.EAPI.Automation.Trips.PNRDataSetup
{
    public class PnrsForPartnerAirlinesAccounts : IClassFixture<TokenFixture>
    {
        public static string env = Config.Environment;
        private TestConfig _testConfig;
        private readonly TokenFixture _tokenFixture;
        private readonly LoyaltyServiceHelper loyaltyServiceHelper;

        public PnrsForPartnerAirlinesAccounts(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _tokenFixture.AddTokenForVRedFromKeyVault("vred");
            _tokenFixture.AddTokenForAzureB2CFromKeyVault("azureGraphB2C");
            loyaltyServiceHelper = new LoyaltyServiceHelper(tokenFixture);
        }

        [Theory]
        [MemberData(nameof(PnrDataCreationTestCases.LoyaltyTestCases), true , MemberType = typeof(PnrDataCreationTestCases))]
        [Trait("Category", Traits.MobileAppPnrsCreationForPartnerAirlines)]
        public async Task PnrDataCreationForAllAirlinesWithLoyaltyAccounts(LocationType origin, LocationType destination, JourneyType journeyType, int adults, int youngAdults, int children, int infants, FlightType airline, FareFamilyType fareFamily, CabinName cabinName, bool seats, string paymentType, string paxFirstName, string paxLastName, string fQTProgramme)
        {
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                #region Test Configuration Setup
                _testConfig = new TestConfig();

                if (origin.ToString() == "LHR" && destination.ToString() == "SVD")
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
                _testConfig.Search.Destination = destination;
                _testConfig.Search.JourneyType = journeyType;
                _testConfig.Search.Passengers.AddPassengers(adults: adults, youngAdults: youngAdults, children: children, infants: infants);
                _testConfig.Search.FlightType = airline;
                _testConfig.Search.FareFamilyType = fareFamily;
                _testConfig.Search.CabinName = cabinName;
                _testConfig.Internal.IsSeats = seats;
                _testConfig.Internal.Bearer = "";
                _testConfig.Book.Passenger.FQTProgramme = fQTProgramme; 

                string b2cToken = string.Empty;
                #endregion

                #region Search and Basket Setup
                GraphQLHelper.OfferSearchGraphQL(_testConfig);

                GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(_testConfig.Internal.BasketId, out string sectorId, out HttpResponseMessage retrieveBasketGQLResponse, b2cToken: b2cToken, firstName: paxFirstName, lastName: paxLastName);

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

                GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
                #endregion

                #region Payment Processing
                string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(_testConfig.Internal.BasketId);
                HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest, b2cToken).EnsureGQLResponseSuccess();
                GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

                // selectedPayWithPointsOption field from the payment response. If this field has a value (not null), it means the booking is an award booking paid with points. Otherwise, it's a revenue booking paid with credit card
                JObject paymentRetGraphqlResult = JsonConvert.DeserializeObject<JObject>(await paymentRetGraphqlResponse.Content.ReadAsStringAsync());
                bool isAwardSearch = paymentRetGraphqlResult.SelectToken("$.data.bookOrderPaymentReference.summary.selectedPayWithPointsOption")?.Value<string>() != null;
                _testConfig.Book.Order.IsAward = isAwardSearch;

                if (paymentType == "PayPal")
                {
                    await HostedPaymentAutomation.SubmitPayPalAsync(
                        _testConfig,
                        expectedTotalPrice: totalSeatPrice);
                }
                else
                {
                    _testConfig.Book.Payment = new Payment(CardType.Visa);
                    // Previous browser flow: HostedPaymentAutomation.SubmitCardAsync(_testConfig, expectedTotalPrice: totalSeatPrice);
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

                #region Post-Booking Data setups
                if (origin.ToString() == "LHR" && destination.ToString() == "SVD" || origin.ToString() == "LHR" && destination.ToString() == "GND")
                {
                    if (journeyType == JourneyType.OneWay)
                        _testConfig.Search.JourneyType = JourneyType.OneWayWithStopover;
                    else if (journeyType == JourneyType.Return)
                        _testConfig.Search.JourneyType = JourneyType.ReturnWithStopover;
                }

                //Azure storage data
                // Update journey type to include "ConnectingFlights" before writing to storage only for DEL->MIA or LHR->AMD

                if (origin.ToString() == "LHR" && destination.ToString() == "AMD" || origin.ToString() == "DEL" && destination.ToString() == "MIA")
                {
                    if (journeyType == JourneyType.OneWay)
                        _testConfig.Search.JourneyType = JourneyType.OneWayWithConnectingFlight;
                    else if (journeyType == JourneyType.Return)
                        _testConfig.Search.JourneyType = JourneyType.ReturnWithConnectingFlight;
                }
                #endregion

                #region Azure Storage
                await WriteToAzureStorage.AddLoyaltyAccountPNRsForMobileAppToAzureStorageTable(_testConfig);
                #endregion

                Assert.True(true);
            }, 1);
        }
    }
}
