using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.Search.Helpers;

namespace VAA.EAPI.Automation.Book.Provisioning.Pnr
{
    // End-to-end PNR creation engine. Consolidates the flow duplicated across
    // VAA.EAPI.Automation.Trips/PNRDataSetup/PnrDataCreation.cs and the OCI
    // PnrsCreation tests.
    //
    // Behaviour preserved from the Trips flow:
    //   - Sold-out: when ExecuteOrderCreate returns false because the response
    //     carries an accepted PnrSellError, this method returns null. Callers
    //     should treat null as "no row produced tonight" and report green —
    //     this is a nightly data-seeding job, not a real test.
    //   - SVD route -> Sunday departure; GND route -> Saturday departure.
    //   - LHR<->AMD and DEL->MIA rewrite to *WithConnectingFlight journey types
    //     before sinks fire.
    //   - LHR<->SVD and LHR<->GND rewrite to *WithStopover variants.
    //   - LoggedInUser path picks a random tier (Red/Silver/Gold) unless the
    //     scenario sets PreferredTier; falls back to Red on failure.
    //
    // Diagnostics: each stage is named (PnrStage) so a thrown
    // PnrProvisioningException identifies where the flow broke — useful when
    // a downstream interface (Delta HPP, payment status, order management) is
    // intermittently flaky.
    public sealed class PnrProvisioner
    {
        private static readonly string[] LoyaltyTierRotation = { "Red", "Silver", "Gold" };
        private const int LoyaltyPointsRequired = 100000;

        private readonly LoyaltyServiceHelper _loyaltyHelper;
        private readonly int _sinkMaxRetries;
        private readonly PnrProvisionerOptions _options;

        public PnrProvisioner(
            TokenFixture tokenFixture,
            int sinkMaxRetries = 3,
            PnrProvisionerOptions? options = null)
        {
            // Prime tokens BEFORE constructing LoyaltyServiceHelper — its constructor
            // captures tokenFixture.GetToken("vred") into a private field, so the
            // helper would silently use an empty bearer if the cache wasn't warm yet.
            tokenFixture.AddTokenForVRedFromKeyVault("vred");
            tokenFixture.AddTokenForAzureB2CFromKeyVault("azureGraphB2C");

            _loyaltyHelper = new LoyaltyServiceHelper(tokenFixture);
            _sinkMaxRetries = sinkMaxRetries;
            _options = options ?? PnrProvisionerOptions.ExistingConsumerDefaults;
        }

        // Booking is attempted ONCE (no retry) — retrying past HppSubmit could
        // double-charge / create duplicate PNRs. Each sink is retried
        // independently because sink writes are idempotent (re-writing the same
        // PNR row is safe) and the failure modes (transient Azure Storage
        // blips) are exactly what retry is for.
        public async Task<CreatedPnr?> CreateAndStoreAsync(PnrScenario scenario, params IPnrSink[] sinks)
        {
            CreatedPnr? result = await CreateInternalAsync(scenario);
            if (result is null)
                return null;

            try
            {
                foreach (IPnrSink sink in sinks)
                    await RetryHelper.RunWithRetryAsync(() => sink.WriteAsync(result), _sinkMaxRetries);
            }
            catch (Exception exception)
            {
                throw new PnrProvisioningException(PnrStage.Storage, scenario, exception, result);
            }

            return result;
        }

        private async Task<CreatedPnr?> CreateInternalAsync(PnrScenario scenario)
        {
            PnrStage stage = PnrStage.ConfigSetup;
            try
            {
                TestConfig testConfig = BuildTestConfig(scenario);

                stage = PnrStage.LoyaltyAuth;
                string b2cToken = string.Empty;
                string originalTierCode = string.Empty;
                string originalFcNumber = string.Empty;
                if (scenario.LoggedInUser)
                {
                    (b2cToken, originalTierCode, originalFcNumber) = await AcquireLoyaltyAsync(scenario, testConfig);
                }

                stage = PnrStage.OfferSearch;
                if (scenario.JourneyType == JourneyType.MultiCity)
                    ExecuteMultiCityOfferSearch(testConfig, scenario);
                else
                    GraphQLHelper.OfferSearchGraphQL(testConfig);

                decimal? insuranceQuotePrice = null;
                decimal? insuranceDecisionPrice = null;
                if (scenario.IncludeInsurance)
                {
                    stage = PnrStage.Insurance;
                    (insuranceQuotePrice, insuranceDecisionPrice) = await AddInsuranceAsync(testConfig, b2cToken);
                }

                stage = PnrStage.BasketSetup;
                (string partnerFqtProgramme, string partnerFqtNumber) = ParseFrequentFlyerProgramme(scenario);
                GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(
                    testConfig.Internal.BasketId,
                    out string sectorId,
                    out HttpResponseMessage retrieveBasketGQLResponse,
                    b2cToken: b2cToken,
                    firstName: scenario.LeadFirstName ?? string.Empty,
                    lastName: scenario.LeadLastName ?? string.Empty,
                    partnerFqtProgramme: partnerFqtProgramme,
                    partnerFqtNumber: partnerFqtNumber);

                if (scenario.IncludeInsurance)
                {
                    decimal selectedInsurancePrice = await ValidateInsurancePriceAsync(
                        retrieveBasketGQLResponse,
                        insuranceQuotePrice,
                        insuranceDecisionPrice);
                    testConfig.Book.Cost.InsurancePrice = (double)selectedInsurancePrice;
                }

                double totalSeatPrice = 0.0;
                if (scenario.Seats)
                {
                    stage = PnrStage.SeatSelection;
                    bool seatMapOk = GraphQLHelper.RetrieveSeatMapGraphQL(
                        testConfig.Internal.BasketId,
                        sectorId,
                        scenario.FareFamily,
                        out HttpResponseMessage _,
                        out string _,
                        b2cToken: b2cToken);

                    if (seatMapOk)
                    {
                        GraphQLHelper.SelectSeatsForAllSectors(
                            testConfig.Internal.BasketId,
                            scenario.FareFamily,
                            retrieveBasketGQLResponse,
                            scenario.CabinName,
                            out totalSeatPrice,
                            scenario.SeatProductName ?? string.Empty);
                    }
                }

                stage = PnrStage.PaymentRetrieve;
                string paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(testConfig.Internal.BasketId);
                HttpResponseMessage paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest, b2cToken).EnsureGQLResponseSuccess();
                GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, testConfig);

                stage = PnrStage.HppSubmit;
                if (!scenario.PaymentType.Is("PayPal")
                    && _options.CardPaymentMode == CardPaymentAutomationMode.Api)
                {
                    testConfig.Book.Payment = new Payment(CardType.Visa);
                    await ApiHostedPaymentAutomation.SubmitCardAsync(testConfig);
                }
                else if (_options.UsePlaywrightForHpp && scenario.PaymentType.Is("PayPal"))
                {
                    // Book.Payment is initialised in TestConfig's default ctor with
                    // PaymentType = "Card"; sinks branch on this value to decide
                    // whether to store card details or the literal "PayPal", so the
                    // PayPal path must overwrite it before sinks fire.
                    testConfig.Book.Payment.PaymentType = "PayPal";
                    testConfig.Book.Order.ChildPRI = await HostedPaymentAutomation.SubmitPayPalAsync(
                        testConfig,
                        usePlaywright: true,
                        headless: _options.HeadlessHpp,
                        expectedTotalPrice: totalSeatPrice,
                        rewardBooking: testConfig.Search.IsRewardSearch);
                }
                else if (_options.UsePlaywrightForHpp)
                {
                    testConfig.Book.Payment = new Payment(CardType.Visa);
                    await HostedPaymentAutomation.SubmitCardAsync(
                        testConfig,
                        usePlaywright: true,
                        headless: _options.HeadlessHpp,
                        expectedTotalPrice: totalSeatPrice,
                        rewardBooking: testConfig.Search.IsRewardSearch);
                }
                else
                {
                    using NavigateToHostedPaymentPage hpp = new NavigateToHostedPaymentPage(testConfig.Book.Order.HppUrl);
                    hpp.ValidateHPPLoaded(totalSeatPrice, testConfig.Search.IsRewardSearch);
                    if (scenario.PaymentType.Is("PayPal"))
                    {
                        testConfig.Book.Payment.PaymentType = "PayPal";
                        hpp.SubmitPayPal(testConfig);
                        testConfig.Book.Order.ChildPRI = hpp.GetChildPri();
                    }
                    else
                    {
                        testConfig.Book.Payment = new Payment(CardType.Visa);
                        hpp.SubmitHppCardDetails(testConfig);
                    }
                }

                stage = PnrStage.PaymentStatus;
                string paymentReference = scenario.PaymentType.Is("PayPal")
                    ? testConfig.Book.Order.ChildPRI
                    : testConfig.Book.Order.PRI;
                string paymentStatusGraphqlRequest = scenario.PaymentType.Is("PayPal")
                    ? GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(testConfig.Internal.BasketId, paymentReference, "PayPal")
                    : GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(testConfig.Internal.BasketId, paymentReference);
                HttpResponseMessage paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, b2cToken).EnsureGQLResponseSuccess();
                dynamic paymentStatusResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());

                Xunit.Assert.True(
                    paymentStatusResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS",
                    $"Payment status is not success — \n\tRequest: {paymentStatusGraphqlRequest}\n\tResponse: {paymentStatusResult}");

                stage = PnrStage.OrderCreate;
                bool orderCreated = await GraphQLHelper.ExecuteOrderCreate(testConfig, b2cToken);
                if (!orderCreated)
                    return null; // PnrSellError — see header comment

                stage = PnrStage.PostBooking;
                ApplyPostBookingAdjustments(testConfig, scenario, originalTierCode, originalFcNumber);

                return new CreatedPnr(
                    PnrCode: testConfig.Book.Order.PNR,
                    BasketId: testConfig.Internal.BasketId,
                    ScenarioName: scenario.Name,
                    CreatedAtUtc: DateTime.UtcNow,
                    LeadPassengerFirstName: testConfig.Book.Order.LeadPassengerFirstName,
                    LeadPassengerLastName: testConfig.Book.Order.LeadPassengerLastName)
                { Config = testConfig };
            }
            catch (Exception ex) when (ex is not PnrProvisioningException)
            {
                throw new PnrProvisioningException(stage, scenario, ex);
            }
        }

        private static TestConfig BuildTestConfig(PnrScenario scenario)
        {
            TestConfig testConfig = new TestConfig();

            testConfig.Search.DepartureDate = ResolveDepartureDate(scenario);
            testConfig.Search.ReturnDate = DataGenerator.ReturnDate(testConfig.Search.DepartureDate);
            testConfig.Search.Origin = scenario.Origin;
            testConfig.Search.Destination = scenario.Destination;
            testConfig.Search.JourneyType = scenario.JourneyType;
            testConfig.Search.Passengers.AddPassengers(
                adults: scenario.Adults,
                youngAdults: scenario.YoungAdults,
                children: scenario.Children,
                infants: scenario.Infants);
            testConfig.Search.FlightType = scenario.Airline;
            testConfig.Search.FareFamilyType = scenario.FareFamily;
            testConfig.Search.CabinName = scenario.CabinName;
            testConfig.Search.InboundFlightType = scenario.InboundAirline ?? scenario.Airline;
            testConfig.Search.InboundFareFamilyType = scenario.InboundFareFamily ?? scenario.FareFamily;
            testConfig.Search.RequiredLegOperatingAirlineCode = scenario.RequiredLegOperatingAirlineCode ?? string.Empty;
            testConfig.Search.InboundRequiredLegOperatingAirlineCode = scenario.InboundRequiredLegOperatingAirlineCode ?? string.Empty;
            testConfig.Search.IsRewardSearch =
                scenario.FareFamily == FareFamilyType.AWARDECONOMY ||
                scenario.FareFamily == FareFamilyType.AWARDBUSINESSFIRST;

            testConfig.Internal.IsSeats = scenario.Seats;
            testConfig.Internal.IsLoggedInUser = scenario.LoggedInUser;
            if (!scenario.FrequentFlyerProgramme.IsNullOrWhiteSpace())
                testConfig.Book.Passenger.FQTProgramme = scenario.FrequentFlyerProgramme;

            // OCI sinks use Internal.TestCaseName as the storage-table
            // PartitionKey and call .ToString() on it; a null value throws.
            // Keep the unique scenario name for diagnostics while allowing
            // OCI manifests to retain their established shared lookup keys.
            testConfig.Internal.TestCaseName = scenario.StoragePartitionKey ?? scenario.Name;

            ApplyOfferSelection(testConfig, scenario);

            if (scenario.SameDayReturn)
                testConfig.Search.ReturnDate = testConfig.Search.DepartureDate;

            return testConfig;
        }

        private static void ApplyOfferSelection(TestConfig testConfig, PnrScenario scenario)
        {
            switch (scenario.OfferSelection)
            {
                case PnrOfferSelection.Any:
                    testConfig.OfferFilterCriteria.AllOffersWithoutFilter = true;
                    break;
                case PnrOfferSelection.Direct:
                    testConfig.OfferFilterCriteria.DirectFlight = true;
                    break;
                case PnrOfferSelection.DirectWithStop:
                    testConfig.OfferFilterCriteria.DirectWithStop = true;
                    break;
                case PnrOfferSelection.OneStop:
                    testConfig.OfferFilterCriteria.OneStop = true;
                    break;
                case PnrOfferSelection.TwoStops:
                    testConfig.OfferFilterCriteria.TwoStops = true;
                    break;
                case PnrOfferSelection.LongLayover:
                    testConfig.OfferFilterCriteria.LongLayover = true;
                    testConfig.OfferFilterCriteria.LongLayoverDuration = scenario.LongLayoverHours
                        ?? throw new InvalidOperationException("LongLayoverHours is required for LongLayover selection.");
                    break;
                case PnrOfferSelection.FareFamilyAndAirline:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario.OfferSelection));
            }
        }

        private static void ExecuteMultiCityOfferSearch(TestConfig testConfig, PnrScenario scenario)
        {
            LocationType[] locations = scenario.MultiCityLocations.ToArray();
            if (locations.Length is not 6 and not 8)
                throw new InvalidOperationException("Multi-city provisioning requires 6 or 8 airport entries for 3 or 4 slices.");

            testConfig.Search.Origin = locations[0];
            testConfig.Search.Destination = locations[1];
            int sliceCount = locations.Length / 2;
            string request = OfferSearchHelper.MultiCityGraphqlRequest(
                locations,
                numberOfAdults: scenario.Adults,
                numberOfChildren: scenario.Children,
                numberOfYouths: scenario.YoungAdults,
                numberOfInfants: scenario.Infants,
                rewardSearch: testConfig.Search.IsRewardSearch,
                departureDate: testConfig.Search.DepartureDate);
            HttpResponseMessage response = GraphQLEndpoint.OfferSearch(request, b2cToken: testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            testConfig.Internal.BasketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(response);
            string offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(response);

            for (int tripIndex = 1; tripIndex < sliceCount; tripIndex++)
            {
                request = OfferSearchHelper.MultiCityGraphqlRequest(
                    offerId: offerId,
                    basketId: testConfig.Internal.BasketId,
                    currentTripIndexId: tripIndex.ToString(CultureInfo.InvariantCulture));
                response = GraphQLEndpoint.OfferSearch(request, b2cToken: testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
                offerId = OfferSearchHelper.ReturnRandomOfferIdFromGQLResponse(response);
            }

            string offerPriceRequest = OfferSearchHelper.OfferPriceNewGQLRequest(
                testConfig,
                offerId,
                currentTripIndexId: sliceCount);
            GraphQLEndpoint.OfferPrice(offerPriceRequest, testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
        }

        private static async Task<(decimal quotePrice, decimal decisionPrice)> AddInsuranceAsync(TestConfig testConfig, string b2cToken)
        {
            string quoteRequest = GraphQLRequestBuilder.InsuranceQuote(testConfig.Internal.BasketId);
            HttpResponseMessage quoteResponse = GraphQLEndpoint.Book(quoteRequest, b2cToken).EnsureGQLResponseSuccess();
            JObject quote = JObject.Parse(await quoteResponse.Content.ReadAsStringAsync());

            HttpResponseMessage decisionResponse = GraphQLEndpoint.InsuranceDecision(testConfig.Internal.BasketId, true, b2cToken).EnsureGQLResponseSuccess();
            JObject decision = JObject.Parse(await decisionResponse.Content.ReadAsStringAsync());

            decimal quotePrice = quote.SelectToken("$.data.InsuranceQuote[0].products[0].totalPrice")?.Value<decimal>()
                ?? throw new InvalidOperationException("Insurance quote did not contain a total price.");
            decimal decisionPrice = decision.SelectToken("$.data.insuranceDecision.summary.insurancePrice")?.Value<decimal>()
                ?? throw new InvalidOperationException("Insurance decision did not contain a price.");
            return (quotePrice, decisionPrice);
        }

        private static async Task<decimal> ValidateInsurancePriceAsync(
            HttpResponseMessage basketResponse,
            decimal? quotePrice,
            decimal? decisionPrice)
        {
            JObject basket = JObject.Parse(await basketResponse.Content.ReadAsStringAsync());
            decimal basketPrice = basket.SelectToken("$.data.retrieveBasket.summary.insurancePrice")?.Value<decimal>()
                ?? throw new InvalidOperationException("Basket did not contain the selected insurance price.");
            Xunit.Assert.Equal(quotePrice, basketPrice);
            Xunit.Assert.Equal(decisionPrice, basketPrice);
            return basketPrice;
        }

        private static string ResolveDepartureDate(PnrScenario scenario)
        {
            if (scenario.UseSeasonalSchedule)
            {
                DateTime firstAvailableDate = SeasonalRouteDateResolver.Resolve(
                    scenario.Origin.ToString(),
                    scenario.Destination.ToString(),
                    DateTime.UtcNow.Date.AddDays(1));
                DateTime seedDate = firstAvailableDate.AddDays(scenario.DepartureOffsetDays ?? 0);
                return SeasonalRouteDateResolver.Resolve(
                    scenario.Origin.ToString(),
                    scenario.Destination.ToString(),
                    seedDate).ToString("yyyy-MM-ddT00:00:00", CultureInfo.InvariantCulture);
            }

            // DepartureOffsetDays mirrors OCIHelper.GetDateFromTodayPlusDays which
            // uses the long form "yyyy-MM-ddT00:00:00" — keep parity with that
            // helper so offer-search and downstream request shapes match.
            if (scenario.DepartureOffsetDays.HasValue)
                return DateTime.UtcNow.AddDays(scenario.DepartureOffsetDays.Value).ToString("yyyy-MM-ddT00:00:00", CultureInfo.InvariantCulture);

            // Trips' existing `Oci` scenario flag uses the short form by
            // convention; preserve that to avoid changing legacy scenario
            // behaviour during the consolidation.
            if (scenario.Oci)
            {
                DateTime tripDate = DataGenerator.RandomDateBetweenTwoDates(DateTime.Now, DateTime.Now.AddDays(3));
                return tripDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            // SVD flies on Sundays only; GND on Saturdays only (LHR routes).
            if (scenario.Origin.ToString().Is("LHR") && scenario.Destination.ToString().Is("SVD"))
                return DataGenerator.GetDepartureDate(DayOfWeek.Sunday.ToString());

            if (scenario.Origin.ToString().Is("LHR") && scenario.Destination.ToString().Is("GND"))
                return DataGenerator.GetDepartureDate(DayOfWeek.Saturday.ToString());

            return DataGenerator.DepartureDate();
        }

        private async Task<(string b2cToken, string tierCode, string fcNumber)> AcquireLoyaltyAsync(PnrScenario scenario, TestConfig testConfig)
        {
            string selectedTier = scenario.PreferredTier
                ?? LoyaltyTierRotation[Random.Shared.Next(LoyaltyTierRotation.Length)];

            Dictionary<string, string> loyaltyDetails;
            int loyaltyPointsRequired = scenario.MinimumLoyaltyPoints ?? LoyaltyPointsRequired;
            try
            {
                loyaltyDetails = await _loyaltyHelper.GetLoyaltyAccountDetails(selectedTier, loyaltyPointsRequired);

                if (string.IsNullOrEmpty(loyaltyDetails["rowKey"]) || string.IsNullOrEmpty(loyaltyDetails["tier"]))
                    throw new InvalidOperationException("Empty loyalty details returned");
            }
            catch
            {
                // Fallback to Red tier if selected tier fails or returns empty.
                selectedTier = "Red";
                loyaltyDetails = await _loyaltyHelper.GetLoyaltyAccountDetails(selectedTier, loyaltyPointsRequired);
            }

            string fcMembershipNumber = loyaltyDetails["rowKey"];

            testConfig.Book.Order.PreBookingFCPoints = loyaltyDetails["points"].ToString();
            testConfig.Book.Order.FqtTierCode = selectedTier;

            string b2cToken = B2CTokenHelper.GenerateLoginB2CToken(fcMembershipNumber, "Virgin123");
            Xunit.Assert.True(!b2cToken.IsNullOrEmpty(), "B2C token is null");

            testConfig.Internal.Bearer = b2cToken;

            return (b2cToken, selectedTier, fcMembershipNumber);
        }

        private static (string programme, string number) ParseFrequentFlyerProgramme(PnrScenario scenario)
        {
            if (scenario.FrequentFlyerProgramme.IsNullOrWhiteSpace())
                return (string.Empty, string.Empty);

            string[] parts = scenario.FrequentFlyerProgramme.Split('-', 2, StringSplitOptions.TrimEntries);
            return parts.Length == 2 ? (parts[0], parts[1]) : (string.Empty, string.Empty);
        }

        private static void ApplyPostBookingAdjustments(TestConfig testConfig, PnrScenario scenario, string originalTierCode, string originalFcNumber)
        {
            if (!scenario.LoggedInUser)
            {
                if (string.IsNullOrEmpty(testConfig.Book.Order.FqtTierCode))
                    testConfig.Book.Order.FqtTierCode = "Glass";
                if (string.IsNullOrEmpty(testConfig.Book.Order.FlyingClubNumber))
                    testConfig.Book.Order.FlyingClubNumber = "N/A";
                if (string.IsNullOrEmpty(testConfig.Book.Order.FqtProgramme))
                    testConfig.Book.Order.FqtProgramme = "N/A";
            }
            else
            {
                testConfig.Book.Order.FqtTierCode = originalTierCode;
                testConfig.Book.Order.FlyingClubNumber = originalFcNumber;
            }

            // SVD/GND stopover rewrite; AMD/MIA connecting-flight rewrite.
            // Order matters: stopover check first.
            string origin = scenario.Origin.ToString();
            string destination = scenario.Destination.ToString();
            bool isStopoverRoute = origin.Is("LHR") && (destination.Is("SVD") || destination.Is("GND"));
            bool isConnectingRoute = (origin.Is("LHR") && destination.Is("AMD")) || (origin.Is("DEL") && destination.Is("MIA"));

            if (isStopoverRoute)
            {
                if (scenario.JourneyType == JourneyType.OneWay)
                    testConfig.Search.JourneyType = JourneyType.OneWayWithStopover;
                else if (scenario.JourneyType == JourneyType.Return)
                    testConfig.Search.JourneyType = JourneyType.ReturnWithStopover;
            }
            else if (isConnectingRoute)
            {
                if (scenario.JourneyType == JourneyType.OneWay)
                    testConfig.Search.JourneyType = JourneyType.OneWayWithConnectingFlight;
                else if (scenario.JourneyType == JourneyType.Return)
                    testConfig.Search.JourneyType = JourneyType.ReturnWithConnectingFlight;
            }
        }
    }
}
