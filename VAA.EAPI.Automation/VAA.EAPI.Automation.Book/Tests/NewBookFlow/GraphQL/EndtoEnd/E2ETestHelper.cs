using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Provisioning.Pnr;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.DataProvider;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.Search.Helpers;
using VAA.EAPI.Automation.Search.Tests.NewBookFlow.GraphQL.OfferSearch;
using Xunit.Abstractions;
using MyAccountGraphQL = VAA.EAPI.Automation.MyAccount.Requests.Endpoint.GraphQL;
using SearchGraphQLEndpoint = VAA.EAPI.Automation.Search.Requests.Endpoints.GraphQLEndpoint;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.EndtoEnd
{
    public class E2ETestHelper
    {
        private readonly TestConfig _testConfig;
        private readonly TokenFixture _tokenFixture;
        private readonly LoyaltyServiceHelper loyaltyServiceHelper;
        private readonly ITestOutputHelper _output;
        private static readonly ConcurrentDictionary<string, string> RequiredLegOperatingAirlineDepartureDateCache = new ConcurrentDictionary<string, string>();
        private string synchronyApplyCardUrl = ConfigReader.Get("SynchronyApplyCardUrl");

        public E2ETestHelper(TestConfig testConfig, TokenFixture tokenFixture, LoyaltyServiceHelper loyaltyServiceHelper, ITestOutputHelper output = null)
        {
            _testConfig = testConfig;
            _tokenFixture = tokenFixture;
            this.loyaltyServiceHelper = loyaltyServiceHelper;
            _output = output;
        }

        public void ConfigureTestConfig(E2EScenario scenario)
        {
            if (!scenario.IsMultiCity)
            {
                _testConfig.Search.Origin = scenario.Origin;
                _testConfig.Search.Destination = scenario.Destination;
            }

            _testConfig.Search.FareFamilyType = scenario.FareFamily;
            _testConfig.Search.FlightType = scenario.FlightType;
            _testConfig.Search.RequiredLegOperatingAirlineCode = string.Empty;
            _testConfig.Search.InboundRequiredLegOperatingAirlineCode = string.Empty;
            _testConfig.Search.Passengers = new Passengers();
            _testConfig.Search.Passengers.AddPassengers(adults: scenario.Adults, scenario.Teens, scenario.Children, scenario.Infants);
            _testConfig.Search.JourneyType = scenario.JourneyType;

            if (scenario.JourneyType != JourneyType.OneWay)
            {
                _testConfig.Search.InboundFareFamilyType = scenario.FareFamily;
                _testConfig.Search.InboundFlightType = scenario.InboundFlightType ?? scenario.FlightType;
            }

            if (!scenario.DepartureDate.IsNullOrWhiteSpace())
            {
                _testConfig.Search.DepartureDate = scenario.DepartureDate;
                _testConfig.Search.ReturnDate = OfferSearchHelper.ReturnDate(scenario.DepartureDate);
                _testConfig.Search.RequiredLegOperatingAirlineCode = scenario.RequiredLegOperatingAirlineCode ?? string.Empty;
            }
            else if (!scenario.RequiredLegOperatingAirlineCode.IsNullOrWhiteSpace())
            {
                _testConfig.Search.DepartureDate = ResolveRequiredLegOperatingAirlineDepartureDate(scenario);
                _testConfig.Search.ReturnDate = OfferSearchHelper.ReturnDate(_testConfig.Search.DepartureDate);
            }

            _testConfig.Internal.TestCaseName = scenario.TestName;

            if (scenario.CabinName.HasValue)
                _testConfig.Book.Seat.CabinName = scenario.CabinName.Value;

            if (!string.IsNullOrEmpty(scenario.SeatProductName))
                _testConfig.Book.Seat.SeatProductName = scenario.SeatProductName;

            if (scenario.PaymentMethod == PaymentMethod.PayPal)
                _testConfig.Book.Payment.PaymentType = "PayPal";

            if (!string.IsNullOrEmpty(scenario.PaymentCountry))
                _testConfig.Book.Payment.CardDetails.Country = scenario.PaymentCountry;

            if (!string.IsNullOrEmpty(scenario.State))
                _testConfig.Book.Payment.CardDetails.State = scenario.State;

            _testConfig.Book.Payment.CardDetails.Postcode = ResolveBillingPostcode(scenario, _testConfig.Book.Payment.CardDetails.Postcode);

            if (!string.IsNullOrEmpty(scenario.CardNumber))
                _testConfig.Book.Payment.CardDetails.CardNumber = scenario.CardNumber;

            if (scenario.Is3DS)
                _testConfig.Book.Payment.CardDetails.IsThreeDs = true;

            if (scenario.PaymentCardType.HasValue)
                _testConfig.Book.Payment = new Payment(scenario.PaymentCardType.Value);

            if (scenario.UseNearDepartureDate)
            {
                _testConfig.Search.DepartureDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddT00:00:00", CultureInfo.InvariantCulture);
                _testConfig.Search.ReturnDate = OfferSearchHelper.ReturnDate(_testConfig.Search.DepartureDate);
            }

            if (scenario.IsRewardSearch)
                _testConfig.Search.IsRewardSearch = true;

            if (scenario.IsCoBrand)
            {
                _testConfig.Book.CoBrand.LastName = scenario.CoBrandLastName;
                _testConfig.Book.CoBrand.FirstName = scenario.CoBrandFirstName;
            }

        }

        private string ResolveRequiredLegOperatingAirlineDepartureDate(E2EScenario scenario)
        {
            string cacheKey = BuildRequiredLegOperatingAirlineDepartureDateCacheKey(scenario);

            List<string> attempts = new List<string>();
            DateTime startDate = DateTime.Today.AddDays(21);
            DateTime endDate = DateTime.Today.AddDays(42);
            List<DateTime> candidateDates = new List<DateTime>();

            if (RequiredLegOperatingAirlineDepartureDateCache.TryGetValue(cacheKey, out string cachedDepartureDate))
            {
                _output?.WriteLine($"Required leg operating airline cached date will be retried first: Test={scenario.TestName}, Carrier={scenario.RequiredLegOperatingAirlineCode}, JourneyType={scenario.JourneyType}, Route={scenario.Origin}-{scenario.Destination}, DepartureDate={cachedDepartureDate}");
                candidateDates.Add(DateTime.ParseExact(cachedDepartureDate, "yyyy-MM-ddT00:00:00", CultureInfo.InvariantCulture));
            }

            for (DateTime candidateDate = startDate; candidateDate <= endDate; candidateDate = candidateDate.AddDays(1))
            {
                if (!candidateDates.Contains(candidateDate))
                {
                    candidateDates.Add(candidateDate);
                }
            }

            foreach (DateTime candidateDate in candidateDates)
            {
                string departureDate = candidateDate.ToString("yyyy-MM-ddT00:00:00", CultureInfo.InvariantCulture);
                _testConfig.Search.DepartureDate = departureDate;
                _testConfig.Search.ReturnDate = OfferSearchHelper.ReturnDate(departureDate);
                ResetRequiredLegOperatingAirlineProbeState();

                string offerSearchGQLRequest = OfferSearchHelper.OfferSearchGQLRequest(_testConfig);

                try
                {
                    using HttpResponseMessage offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, string.Empty, _testConfig.Internal.Bearer).EnsureIsSuccessStatusCode();
                    string searchResponseContent = offerSearchResponse.ReadResponseAsString();
                    string[] fareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForFareFamilyAndLegOperatingAirline(
                        searchResponseContent,
                        _testConfig.Search.FareFamilyType,
                        _testConfig.Search.FlightType,
                        scenario.RequiredLegOperatingAirlineCode,
                        "fare");

                    if (!fareLinks.Any())
                    {
                        fareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForFareFamilyAndLegOperatingAirline(
                            searchResponseContent,
                            _testConfig.Search.FareFamilyType,
                            _testConfig.Search.FlightType,
                            scenario.RequiredLegOperatingAirlineCode,
                            "summary");
                    }

                    if (fareLinks.Any())
                    {
                        _testConfig.Search.RequiredLegOperatingAirlineCode = scenario.RequiredLegOperatingAirlineCode;
                        _testConfig.Search.InboundRequiredLegOperatingAirlineCode = string.Empty;
                        RequiredLegOperatingAirlineDepartureDateCache.TryAdd(cacheKey, departureDate);
                        _output?.WriteLine($"Required leg operating airline date selected: Test={scenario.TestName}, Carrier={scenario.RequiredLegOperatingAirlineCode}, Direction=outbound, JourneyType={scenario.JourneyType}, Route={scenario.Origin}-{scenario.Destination}, DepartureDate={departureDate}");
                        ResetRequiredLegOperatingAirlineProbeState(clearRequiredAirlineDirection: false);
                        return departureDate;
                    }

                    if (scenario.JourneyType == JourneyType.Return
                        && TryFindRequiredInboundLegOperatingAirline(
                            scenario,
                            offerSearchResponse,
                            searchResponseContent))
                    {
                        _testConfig.Search.RequiredLegOperatingAirlineCode = string.Empty;
                        _testConfig.Search.InboundRequiredLegOperatingAirlineCode = scenario.RequiredLegOperatingAirlineCode;
                        RequiredLegOperatingAirlineDepartureDateCache.TryAdd(cacheKey, departureDate);
                        _output?.WriteLine($"Required leg operating airline date selected: Test={scenario.TestName}, Carrier={scenario.RequiredLegOperatingAirlineCode}, Direction=inbound, JourneyType={scenario.JourneyType}, Route={scenario.Origin}-{scenario.Destination}, DepartureDate={departureDate}");
                        ResetRequiredLegOperatingAirlineProbeState(clearRequiredAirlineDirection: false);
                        return departureDate;
                    }

                    attempts.Add($"{departureDate}: no matching {scenario.RequiredLegOperatingAirlineCode} leg offer returned on either direction");
                }
                catch (Exception ex)
                {
                    attempts.Add($"{departureDate}: {ex.Message}");
                }
            }

            throw new InvalidOperationException($"No offer with required leg operating airline '{scenario.RequiredLegOperatingAirlineCode}' was found for {scenario.TestName} between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}. Attempts: {string.Join(" | ", attempts)}");
        }

        private bool TryFindRequiredInboundLegOperatingAirline(
            E2EScenario scenario,
            HttpResponseMessage offerSearchResponse,
            string searchResponseContent)
        {
            string[] outboundLinks = OfferSearchHelper.FindAllFareLinksForSpecificCabinAndAirline(
                searchResponseContent,
                _testConfig.Search.FareFamilyType,
                _testConfig.Search.FlightType);

            if (!outboundLinks.Any())
            {
                outboundLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(
                    searchResponseContent,
                    _testConfig.Search.FareFamilyType,
                    _testConfig.Search.FlightType);
            }

            if (!outboundLinks.Any())
            {
                return false;
            }

            string basketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);
            string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(outboundLinks, randomOffer: false);
            _testConfig.Internal.BasketId = basketId;
            _testConfig.Internal.OfferId = offerId;

            string returnRequest = OfferSearchHelper.OfferSearchGQLRequest(_testConfig);
            using HttpResponseMessage returnResponse = GraphQLEndpoint
                .OfferSearch(returnRequest, basketId, _testConfig.Internal.Bearer)
                .EnsureGQLResponseSuccess();
            string returnResponseContent = returnResponse.ReadResponseAsString();
            string[] inboundLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForFareFamilyAndLegOperatingAirline(
                returnResponseContent,
                _testConfig.Search.InboundFareFamilyType,
                _testConfig.Search.InboundFlightType,
                scenario.RequiredLegOperatingAirlineCode,
                "summary");

            return inboundLinks.Any();
        }

        private void ResetRequiredLegOperatingAirlineProbeState(bool clearRequiredAirlineDirection = true)
        {
            _testConfig.Internal.BasketId = string.Empty;
            _testConfig.Internal.OfferId = string.Empty;

            if (clearRequiredAirlineDirection)
            {
                _testConfig.Search.RequiredLegOperatingAirlineCode = string.Empty;
                _testConfig.Search.InboundRequiredLegOperatingAirlineCode = string.Empty;
            }
        }

        private static string BuildRequiredLegOperatingAirlineDepartureDateCacheKey(E2EScenario scenario)
        {
            return string.Join(
                "|",
                Config.Environment,
                scenario.JourneyType,
                scenario.Origin,
                scenario.Destination,
                scenario.FareFamily,
                scenario.FlightType,
                scenario.RequiredLegOperatingAirlineCode);
        }

        private static string ResolveBillingPostcode(E2EScenario scenario, string defaultPostcode)
        {
            if (!scenario.Postcode.IsNullOrWhiteSpace())
            {
                return scenario.Postcode;
            }

            if (scenario.PaymentCountry.Is("United States"))
            {
                if (scenario.State.Is("Alaska"))
                {
                    return "99501";
                }

                return "12345";
            }

            return defaultPostcode;
        }

        public async Task<(string b2cToken, string fcMembershipNumber, Dictionary<string, string> loyaltyDetails, int fqTPointsBeforeBooking)> SetupLoyaltyAndLogin(E2EScenario scenario)
        {
            string fcMembershipNumber;
            Dictionary<string, string> loyaltyDetails;
            int fqTPointsBeforeBooking = 0;

            if (scenario.ValidateVoucherConfirmation)
            {
                loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyVoucherAccountDetailsFromStorageTable(
                    tierCode: scenario.LoyaltyTierCode,
                    environmentOverride: ConfigReader.Get("SearchRewardVoucherAccountEnvironment", Config.Environment),
                    voucherType: scenario.VoucherType,
                    minimumPoints: scenario.VoucherMinimumPoints ?? 300000,
                    minimumVoucherExpiryDate: DateTime.UtcNow.Date.AddMonths(10),
                    exactVoucherCount: scenario.ExactVoucherCount,
                    requireAllVouchersMatchType: scenario.RequireAllVouchersMatchType);
                Assert.False(
                    loyaltyDetails.ContainsKey("error"),
                    loyaltyDetails.TryGetValue("error", out string voucherAccountError)
                        ? voucherAccountError
                        : "A voucher-backed loyalty account could not be selected.");
                fcMembershipNumber = loyaltyDetails["rowKey"];
                fqTPointsBeforeBooking = Convert.ToInt32(loyaltyDetails["points"]);
                _output?.WriteLine(
                    $"Voucher account selected: Scenario={scenario.TestName}, Tier={scenario.LoyaltyTierCode}, Member={loyaltyDetails.GetValueOrDefault("maskedRowKey")}");
            }
            else if (scenario.BookingType == BookingType.PayWithPoints && !scenario.IsCoBrand)
            {
                if (scenario.UseExplicitSearchOverload)
                {
                    // PayWithPointOneWay: search first, then find loyalty account
                    return (null, null, null, 0); // will be set up after search
                }

                // PWP with insurance tests: search first, loyalty after
                return (null, null, null, 0);
            }

            if (scenario.IsCoBrand)
            {
                string lastNameForLookup = scenario.SynchronyApplicationStatus == "APPROVED" ? "Approval" :
                    scenario.CoBrandLastName == "Refer" ? "Refer" :
                    scenario.CoBrandLastName == "Decline" ? "Decline" :
                    scenario.CoBrandLastName;

                if (scenario.BookingType == BookingType.PayWithPoints)
                {
                    int passengerCount = 4;
                    var minimumRequiredPoints = passengerCount * 3000;
                    loyaltyDetails = await loyaltyServiceHelper.GetLoyaltyAccountDetailsForCoBrand(scenario.LoyaltyTierCode, "Approval", scenario.MinLoyaltyPoints, minimumRequiredPoints);
                }
                else if (scenario.IsRewardSearch)
                    loyaltyDetails = await loyaltyServiceHelper.GetLoyaltyAccountDetailsForCoBrand(scenario.LoyaltyTierCode, lastNameForLookup, 1000000);
                else if (scenario.Insurance == InsuranceAction.Accept && !scenario.IsRewardSearch && scenario.SynchronyApplicationStatus == "APPROVED" && !scenario.UseSynchronyCardPayment)
                    loyaltyDetails = await loyaltyServiceHelper.GetLoyaltyAccountDetailsForCoBrand(scenario.LoyaltyTierCode, lastNameForLookup, 100000);
                else if (scenario.UseSynchronyCardPayment && scenario.Insurance == InsuranceAction.Accept)
                    loyaltyDetails = await loyaltyServiceHelper.GetLoyaltyAccountDetailsForCoBrand("Red", "Approval", 1000000);
                else
                    loyaltyDetails = await loyaltyServiceHelper.GetLoyaltyAccountDetailsForCoBrand(scenario.LoyaltyTierCode, lastNameForLookup, 100000);

                fcMembershipNumber = loyaltyDetails["rowKey"];
                fqTPointsBeforeBooking = Convert.ToInt32(loyaltyDetails["points"]);
            }
            else if (scenario.BookingType == BookingType.Redemption)
            {
                if (scenario.Insurance != InsuranceAction.None)
                {
                    int pointsRequired = 1000000;
                    string tierCode = Passenger.GetRandomTier();
                    loyaltyDetails = ShouldBypassVRedBalanceRefreshInUat(scenario)
                        ? RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tierCode, pointsRequired)
                        : await loyaltyServiceHelper.GetLoyaltyAccountDetails(tierCode, pointsRequired);
                    fcMembershipNumber = loyaltyDetails["rowKey"];
                    _testConfig.Book.Order.PreBookingFCPoints = loyaltyDetails["points"].ToString();
                }
                else
                {
                    loyaltyDetails = ShouldBypassVRedBalanceRefreshInUat(scenario)
                        ? RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(scenario.LoyaltyTierCode, scenario.MinLoyaltyPoints)
                        : await loyaltyServiceHelper.GetLoyaltyAccountDetails(scenario.LoyaltyTierCode, scenario.MinLoyaltyPoints);
                    fcMembershipNumber = loyaltyDetails["rowKey"];
                    fqTPointsBeforeBooking = Convert.ToInt32(loyaltyDetails["points"]);
                }
            }
            else if (scenario.FlyingClubPassenger)
            {
                // FlyingClub seat tests don't need loyalty setup here - handled in UpdateBasketDetails
                return (null, null, null, 0);
            }
            else if (scenario.IsLoggedIn)
            {
                loyaltyDetails = await loyaltyServiceHelper.GetLoyaltyAccountDetails(
                    scenario.LoyaltyTierCode,
                    scenario.MinLoyaltyPoints);
                fcMembershipNumber = loyaltyDetails["rowKey"];
                fqTPointsBeforeBooking = Convert.ToInt32(loyaltyDetails["points"]);
            }
            else
            {
                loyaltyDetails = new Dictionary<string, string>();
                fcMembershipNumber = string.Empty;
            }

            // Voucher fixtures are keyed and validated by Flying Club number. Match the proven
            // Search voucher flow and avoid a stale storage-table email resolving the wrong identity.
            Dictionary<string, string> signInDetails = scenario.ValidateVoucherConfirmation ? null : loyaltyDetails;
            string b2cToken = await GenerateLoginB2CTokenFromApi(fcMembershipNumber, signInDetails);

            if (scenario.IsCoBrand)
                _testConfig.Internal.Bearer = b2cToken;

            return (b2cToken, fcMembershipNumber, loyaltyDetails, fqTPointsBeforeBooking);
        }

        public Task ApplyVoucherSelection(E2EScenario scenario, string basketId, string b2cToken, JToken offerPriceResult)
        {
            Assert.False(scenario.VoucherRedemptionType.IsNullOrWhiteSpace(), "voucherRedemptionType is required when validateVoucherConfirmation is enabled.");

            JArray redemptionOptions = offerPriceResult
                .SelectTokens("$..voucherDetails.redemptionOptions")
                .OfType<JArray>()
                .FirstOrDefault();
            Assert.True(
                redemptionOptions != null && redemptionOptions.Count > 0,
                $"Search Summary returned no voucher redemption options for {scenario.VoucherRedemptionType}. ResponsePreview={BuildJsonPreview(offerPriceResult)}");
            JObject selectedOption = redemptionOptions
                .Children<JObject>()
                .FirstOrDefault(option =>
                    option.SelectToken("redemptionType")?.ToString().Is(scenario.VoucherRedemptionType) == true &&
                    (!scenario.ExpectedAppliedVoucherCount.HasValue ||
                     option.SelectTokens("appliedVouchers[*].voucherId").Count() == scenario.ExpectedAppliedVoucherCount.Value));
            Assert.True(
                selectedOption != null,
                $"Search Summary returned no {scenario.VoucherRedemptionType} option applying the expected {scenario.ExpectedAppliedVoucherCount?.ToString() ?? "any"} voucher(s). Options={redemptionOptions}");

            List<string> selectedVoucherIds = selectedOption
                .SelectTokens("appliedVouchers[*].voucherId")
                .Select(voucherId => voucherId.ToString())
                .Where(voucherId => !voucherId.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            Assert.NotEmpty(selectedVoucherIds);

            string selectionRequest = OfferSearchHelper.SetVoucherRedemptionSelectionGQLRequest(basketId, selectedVoucherIds);
            JObject selectionResponse = JObject.Parse(SearchGraphQLEndpoint
                .SetVoucherRedemptionSelection(selectionRequest, b2cToken)
                .EnsureGQLResponseSuccess()
                .ReadResponseAsString());
            Assert.True(
                GraphQlAssertions.ValidateRewardVoucherSelectionMutationResponse(selectionResponse, selectedOption, selectedVoucherIds, out string selectionMessage),
                $"{scenario.VoucherRedemptionType} voucher selection was not persisted. {selectionMessage}");

            return Task.CompletedTask;
        }

        public Task<(string basketId, dynamic offerPriceResult)> PerformSearch(E2EScenario scenario, string b2cToken)
        {
            dynamic offerPriceResult = null;
            string basketId = null;

            if (scenario.IsMultiCity)
            {
                PerformMultiCitySearch(scenario);
                basketId = _testConfig.Internal.BasketId;
                return Task.FromResult((basketId, offerPriceResult));
            }

            _testConfig.Search.IsRewardSearch = scenario.IsRewardSearch;
            _testConfig.OfferFilterCriteria.VSFlightRandomOffer = scenario.RandomOffer;
            if (!string.IsNullOrEmpty(b2cToken))
                _testConfig.Internal.Bearer = b2cToken;

            offerPriceResult = GraphQLHelper.OfferSearchGraphQL(
                _testConfig,
                includeVoucherDetails: scenario.ValidateVoucherConfirmation,
                includeVoucherBasketDetails: scenario.ValidateVoucherConfirmation);
            basketId = _testConfig.Internal.BasketId;
            return Task.FromResult((basketId, offerPriceResult));
        }

        public void PerformMultiCitySearch(E2EScenario scenario)
        {
            int maxAttempts = 5;
            int attempt = 0;
            bool success = false;

            while (attempt < maxAttempts && !success)
            {
                try
                {
                    attempt++;

                    var offerSearchGQLRequest = OfferSearchHelper.MultiCityGraphqlRequest(scenario.MultiCityLocations, _testConfig.Search.Passengers.Adults.Count);
                    var offerSearchResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLRequest, "");

                    if (!offerSearchResponse.IsGQLResponseSuccess())
                    {
                        if (attempt == maxAttempts)
                            throw new Exception($"offerSearchResponse{offerSearchResponse.ReadResponseAsString()}");
                        else
                            continue;
                    }

                    _testConfig.Internal.BasketId = OfferSearchHelper.GetBasketIdFromGQLResponseBody(offerSearchResponse);
                    var offerSearchResult = offerSearchResponse.ReadResponseAsString();
                    var fareLinks = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchResult, FlightType.VS);

                    if (!fareLinks.Any())
                    {
                        if (attempt == maxAttempts)
                            throw new Exception($"Slice - 0: No direct flights were found matching the requested criteria. Response: {offerSearchResult}");
                        else
                            continue;
                    }

                    string offerId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinks, randomOffer: true);

                    var offerSearchGQLSliceOneRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: offerId, basketId: _testConfig.Internal.BasketId, currentTripIndexId: "1");
                    var offerSearchSliceOneResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceOneRequest, "");

                    if (!offerSearchSliceOneResponse.IsGQLResponseSuccess())
                    {
                        if (attempt == maxAttempts)
                            throw new Exception($"offerSearchSliceOneResponse : {offerSearchSliceOneResponse.ReadResponseAsString()}");
                        else
                            continue;
                    }

                    var offerSearchSliceOneResult = offerSearchSliceOneResponse.ReadResponseAsString();
                    var fareLinksSliceOne = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchSliceOneResult, FlightType.VS);

                    if (!fareLinksSliceOne.Any())
                    {
                        if (attempt == maxAttempts)
                            throw new Exception($"Slice - 1: No direct flights were found matching the requested criteria. Response: {offerSearchSliceOneResult}");
                        else
                            continue;
                    }

                    string sliceOneOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(fareLinksSliceOne, randomOffer: true);
                    var offerSearchGQLSliceTwoRequest = OfferSearchHelper.MultiCityGraphqlRequest(offerId: sliceOneOfferId, basketId: _testConfig.Internal.BasketId, currentTripIndexId: "2");
                    var offerSearchSliceTwoResponse = GraphQLEndpoint.OfferSearch(offerSearchGQLSliceTwoRequest, "");

                    if (!offerSearchSliceTwoResponse.IsGQLResponseSuccess())
                    {
                        if (attempt == maxAttempts)
                            throw new Exception($"offerSearchSliceTwoResponse : {offerSearchSliceTwoResponse.ReadResponseAsString()}");
                        else
                            continue;
                    }

                    var offerSearchSliceTwoResult = offerSearchSliceTwoResponse.ReadResponseAsString();
                    var summaryLinksSliceTwo = OfferSearchHelper.FindAllFareOrSummaryLinksForSpecificAirlineAndDirectOnly(offerSearchSliceTwoResult, FlightType.VS, "summary");

                    if (!summaryLinksSliceTwo.Any())
                    {
                        if (attempt == maxAttempts)
                            throw new Exception($"Slice - 2: No direct flights were found matching the requested criteria. Response: {offerSearchSliceTwoResult}");
                        else
                            continue;
                    }

                    string sliceTwoOfferId = OfferSearchHelper.GetFirstOrRandomOfferIdFromFareOrSummaryLinks(summaryLinksSliceTwo, randomOffer: true);
                    _testConfig.Search.JourneyType = JourneyType.MultiCity;
                    var offerPriceGQLRequest = OfferSearchHelper.OfferPriceNewGQLRequest(_testConfig, sliceTwoOfferId);
                    var offerPriceResponse = GraphQLEndpoint.OfferPrice(offerPriceGQLRequest).EnsureIsSuccessStatusCode();

                    if (!GraphQlAssertions.ValidateOriginAndDestinationInSummary(offerSearchGQLRequest, offerPriceResponse, out string originAndDestination))
                        throw new Exception(originAndDestination);

                    success = true;
                }
                catch (Exception ex)
                {
                    if (attempt == maxAttempts)
                        throw new Exception($"All {maxAttempts} attempts failed. Last error: {ex.Message}", ex);
                }
            }
        }

        public async Task<(HttpResponseMessage response, dynamic result)> RetrieveBasket(string basketId, string b2cToken = null)
        {
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
            var retrieveBasketGQLResponse = (b2cToken != null)
                ? GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess()
                : GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            dynamic retrieveBasketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());
            return (retrieveBasketGQLResponse, retrieveBasketResult);
        }

        public void ValidateExpectedLegOperatingAirline(E2EScenario scenario, object response, string responseName)
        {
            string expectedOperatingAirlineCode = ResolveExpectedLegOperatingAirlineCode(scenario);
            if (expectedOperatingAirlineCode.IsNullOrWhiteSpace())
            {
                return;
            }

            if (response == null)
            {
                throw new InvalidOperationException(
                    $"Expected leg operating airline check failed for {responseName}. Response was null. ExpectedCarrier={expectedOperatingAirlineCode}");
            }

            JToken responseToken = response as JToken ?? JToken.FromObject(response);
            JToken[] legTokens = GetLegTokens(responseToken);

            Assert.True(
                legTokens.Any(),
                $"Expected leg operating airline check failed for {responseName}. No leg details were returned. ExpectedCarrier={expectedOperatingAirlineCode}, ResponsePreview={BuildJsonPreview(responseToken)}");

            string[] returnedCarrierCodes = legTokens
                .Select(GetLegOperatingCarrierCode)
                .Where(code => !code.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(code => code)
                .ToArray();

            Assert.True(
                returnedCarrierCodes.Any(code => code.Is(expectedOperatingAirlineCode)),
                $"Expected leg operating airline '{expectedOperatingAirlineCode}' was not returned in {responseName}. Returned leg operators: {BuildLegOperatingCarrierSummary(legTokens)}");

            _output?.WriteLine(
                $"Validated expected leg operating airline in {responseName}: ExpectedCarrier={expectedOperatingAirlineCode}, ReturnedCarriers={string.Join(",", returnedCarrierCodes)}");
        }

        private static string ResolveExpectedLegOperatingAirlineCode(E2EScenario scenario)
        {
            if (scenario.IsMultiCity)
            {
                return string.Empty;
            }

            if (!scenario.RequiredLegOperatingAirlineCode.IsNullOrWhiteSpace())
            {
                return scenario.RequiredLegOperatingAirlineCode;
            }

            return scenario.FlightType.ToString();
        }

        private static JToken[] GetLegTokens(JToken responseToken)
        {
            List<JToken> legTokens = new List<JToken>();

            foreach (JToken legsToken in responseToken.SelectTokens("$..legs"))
            {
                AddTokenOrChildren(legTokens, legsToken);
            }

            foreach (JToken flightLegToken in responseToken.SelectTokens("$..flightLeg"))
            {
                AddTokenOrChildren(legTokens, flightLegToken);
            }

            foreach (JToken legToken in responseToken.SelectTokens("$..leg"))
            {
                AddTokenOrChildren(legTokens, legToken);
            }

            return legTokens.ToArray();
        }

        private static void AddTokenOrChildren(List<JToken> tokens, JToken token)
        {
            if (token is JArray tokenArray)
            {
                foreach (JToken childToken in tokenArray.Children())
                {
                    tokens.Add(childToken);
                }

                return;
            }

            tokens.Add(token);
        }

        private static string GetLegOperatingCarrierCode(JToken legToken)
        {
            string operatingAirlineCode = legToken.SelectToken("operatingAirline.code")?.ToString();
            if (!operatingAirlineCode.IsNullOrWhiteSpace())
            {
                return operatingAirlineCode;
            }

            string operatingAirlineValue = legToken.SelectToken("operatingAirline")?.ToString();
            if (!operatingAirlineValue.IsNullOrWhiteSpace())
            {
                return operatingAirlineValue;
            }

            string operatingAirlineCodeValue = legToken.SelectToken("operatingAirlineCode")?.ToString();
            if (!operatingAirlineCodeValue.IsNullOrWhiteSpace())
            {
                return operatingAirlineCodeValue;
            }

            string operatingCarrierCarrierCode = legToken.SelectToken("operatingCarrier.carrierCode")?.ToString();
            if (!operatingCarrierCarrierCode.IsNullOrWhiteSpace())
            {
                return operatingCarrierCarrierCode;
            }

            string operatingCarrierCode = legToken.SelectToken("operatingCarrier.code")?.ToString();
            if (!operatingCarrierCode.IsNullOrWhiteSpace())
            {
                return operatingCarrierCode;
            }

            string operatingCarrierCodeValue = legToken.SelectToken("operatingCarrierCode")?.ToString();
            if (!operatingCarrierCodeValue.IsNullOrWhiteSpace())
            {
                return operatingCarrierCodeValue;
            }

            JToken operatingCarrier = legToken.SelectToken("operatingCarrier");
            return operatingCarrier?.Type == JTokenType.String ? operatingCarrier.ToString() : string.Empty;
        }

        private static string BuildLegOperatingCarrierSummary(IEnumerable<JToken> legTokens)
        {
            string[] legSummaries = legTokens
                .Select(BuildLegOperatingCarrierSummary)
                .Where(summary => !summary.IsNullOrWhiteSpace())
                .ToArray();

            return legSummaries.Any() ? string.Join(", ", legSummaries) : "none";
        }

        private static string BuildLegOperatingCarrierSummary(JToken legToken)
        {
            string origin = legToken.SelectToken("origin.code")?.ToString()
                ?? legToken.SelectToken("originAirportCode")?.ToString()
                ?? legToken.SelectToken("originAirport.airportCode")?.ToString()
                ?? "?";
            string destination = legToken.SelectToken("destination.code")?.ToString()
                ?? legToken.SelectToken("destinationAirportCode")?.ToString()
                ?? legToken.SelectToken("destinationAirport.airportCode")?.ToString()
                ?? "?";
            string carrierCode = GetLegOperatingCarrierCode(legToken);
            string carrierName = legToken.SelectToken("operatingAirline.name")?.ToString()
                ?? legToken.SelectToken("operatingAirlineName")?.ToString()
                ?? legToken.SelectToken("operatingCarrier.codeShareAirline")?.ToString()
                ?? string.Empty;

            return carrierName.IsNullOrWhiteSpace()
                ? $"{origin}-{destination}:{carrierCode}"
                : $"{origin}-{destination}:{carrierCode}:{carrierName}";
        }

        private static string BuildJsonPreview(JToken responseToken)
        {
            string responseText = responseToken.ToString(Formatting.None);
            return responseText.Length > 2000 ? responseText[..2000] : responseText;
        }

        public void ValidateRedemptionBasket(dynamic offerPriceResult, dynamic retrieveBasketResult, Dictionary<string, string> loyaltyDetails, string basketId)
        {
            var softAssert = new SoftAssertHelper();

            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value, $"Validation failed: isAwardSearch is not True for redemption points scenario {retrieveBasketResult.data.retrieveBasket.summary.isAwardSearch.Value}");
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.membershipPoints.Value != null, $"Validation failed: membershipPoints field is null  \n Response : {retrieveBasketResult}");
            softAssert.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, retrieveBasketResult, out string message), $"Validation failed:  {message}");
            softAssert.True(GraphQLAssertion.ValidateFcPointElibility(retrieveBasketResult, out message), $"Validation failed:  {message}");
            softAssert.True(retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value == "VS", $"Validation failed: The Basket FQTProgramme - {retrieveBasketResult.data.retrieveBasket.customerProfile.membershipDetails.fqtProgramme.Value} is not equal to FQTProgramme VS");
            softAssert.True(retrieveBasketResult.data.retrieveBasket.people[0].profilePassenger.Value, $"Validation failed: The Basket Profile passenger flag is false - {retrieveBasketResult.data.retrieveBasket.people[0].Value} suppose to be True VS");
            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubNumber(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");
            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubTierCode(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");
            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubPoints(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");
            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubFirstName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");
            softAssert.True(GraphQLAssertion.ValidateBasketFlyingClubLastName(loyaltyDetails, retrieveBasketResult, out message), $"Validation failed:  {message}");
            softAssert.True(retrieveBasketResult.data.retrieveBasket.summary.flightPrice.Value != null, $"flightPrice field is null  \n Response : {retrieveBasketResult}");
            softAssert.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, retrieveBasketResult, out string errorMessage), errorMessage);
            softAssert.AssertAll();
        }

        public async Task<(JToken insuranceQuoteResult, JToken insuranceDecisionResult)> HandleInsurance(E2EScenario scenario, string basketId, string b2cToken)
        {
            var insuranceQuoteRequest = GraphQLRequestBuilder.InsuranceQuote(basketId);
            var insuranceQuoteResponse = (b2cToken != null && scenario.IsCoBrand)
                ? GraphQLEndpoint.Book(insuranceQuoteRequest, b2cToken).EnsureGQLResponseSuccess()
                : GraphQLEndpoint.Book(insuranceQuoteRequest).EnsureGQLResponseSuccess();
            var insuranceQuoteResult = JsonConvert.DeserializeObject<JToken>(await insuranceQuoteResponse.Content.ReadAsStringAsync());

            bool accept = scenario.Insurance == InsuranceAction.Accept;
            HttpResponseMessage insuranceDecisionResponse;
            if (b2cToken != null && (scenario.BookingType == BookingType.Redemption || scenario.IsCoBrand))
                insuranceDecisionResponse = GraphQLEndpoint.InsuranceDecision(basketId, accept, b2cToken).EnsureGQLResponseSuccess();
            else
                insuranceDecisionResponse = GraphQLEndpoint.InsuranceDecision(basketId, accept).EnsureGQLResponseSuccess();

            var insuranceDecisionResult = JsonConvert.DeserializeObject<JToken>(await insuranceDecisionResponse.Content.ReadAsStringAsync());

            if (scenario.Insurance == InsuranceAction.Reject && scenario.ExpectFraudCheck)
            {
                bool insuranceDecision = insuranceDecisionResult.SelectToken("data.insuranceDecision.isInsuranceAdded").Value<bool>();
                Assert.True(!insuranceDecision, $"\n Insurance insuranceDecision is not true : {insuranceDecision} \n Insurance Decision Response:{insuranceDecisionResult}  ");
            }

            return (insuranceQuoteResult, insuranceDecisionResult);
        }

        public async Task<(string sectorId, HttpResponseMessage retrieveBasketResponse)> UpdateBasketDetails(E2EScenario scenario, dynamic retrieveBasketResult, string basketId, string b2cToken, dynamic offerPriceResult)
        {
            string sectorId = null;
            HttpResponseMessage retrieveBasketResponse = null;

            if (scenario.UseUpdateBasketShortcut)
            {
                if (scenario.FlyingClubPassenger)
                    GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out sectorId, out retrieveBasketResponse, true, scenario.FlyingClubTier);
                else
                    GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out sectorId, out retrieveBasketResponse);
                return (sectorId, retrieveBasketResponse);
            }

            // Passenger Basket update
            if (scenario.BookingType == BookingType.Redemption && !scenario.IsCoBrand)
            {
                GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId, false, "", b2cToken);

                var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
                var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
                dynamic contactBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await contactBasketGQLResponse.Content.ReadAsStringAsync());

                // Validate contact basket for redemption
                if (scenario.ValidateRedemptionBasket)
                {
                    var softAssertContact = new SoftAssertHelper();
                    softAssertContact.True(contactBasketGQLResult.data.updateContactDetails.basketId.Value == basketId, "Contact Validation: Basket ID dont match");
                    softAssertContact.True(contactBasketGQLResult.data.updateContactDetails.summary.isAwardSearch.Value, "Contact Validation: isAwardSearch is not True for redemption points scenario");
                    softAssertContact.True(contactBasketGQLResult.data.updateContactDetails.summary.membershipPoints.Value != null, $"Contact Validation: membershipPoints field is null  \n Response : {contactBasketGQLResult}");
                    softAssertContact.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, contactBasketGQLResult, out string message), $"Contact Validation:  {message}");
                    softAssertContact.True(contactBasketGQLResult.data.updateContactDetails.summary.flightPrice.Value != null, $" Contact Validation:  flightPrice field is null  \n Response : {contactBasketGQLResult}");
                    softAssertContact.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, contactBasketGQLResult, out string errorMessage), $"Contact Validation:  {errorMessage}");
                    softAssertContact.AssertAll();
                }

                var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);
                var tncBasketGQLResponse = GraphQLEndpoint.Book(tncBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
                dynamic tncBasketGQLResult = JsonConvert.DeserializeObject<dynamic>(await tncBasketGQLResponse.Content.ReadAsStringAsync());

                // Validate T&C for redemption
                if (scenario.ValidateRedemptionBasket)
                {
                    var softAssertTerms = new SoftAssertHelper();
                    softAssertTerms.True(tncBasketGQLResult.data.updateTnC.basketId.Value == basketId, "Term&Condition Validation: Basket ID dont match");
                    softAssertTerms.True(tncBasketGQLResult.data.updateTnC.summary.isAwardSearch.Value, "Term&Condition Validation: isAwardSearch is not True for redemption points scenario");
                    softAssertTerms.True(tncBasketGQLResult.data.updateTnC.summary.membershipPoints.Value != null, $"Term&Condition Validation: membershipPoints field is null  \n Response : {tncBasketGQLResult}");
                    softAssertTerms.True(GraphQLAssertion.ValidateBasketMembershipPoints(offerPriceResult, tncBasketGQLResult, out string message), $"Term&Condition Validation:  {message}");
                    softAssertTerms.True(tncBasketGQLResult.data.updateTnC.summary.flightPrice.Value != null, $"Term&Condition Validation: flightPrice field is null  \n Response : {tncBasketGQLResult}");
                    softAssertTerms.True(GraphQLAssertion.ValidateBasketFlightPrice(offerPriceResult, tncBasketGQLResult, out string errorMessage), $"Term&Condition Validation: {errorMessage}");
                    softAssertTerms.AssertAll();
                }
            }
            else if (scenario.BookingType == BookingType.PayWithPoints && !scenario.IsCoBrand)
            {
                GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId, false, "", b2cToken ?? "");
                var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
                if (b2cToken != null)
                    GraphQLEndpoint.Book(contactBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
                else
                    GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();
                var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);
                if (b2cToken != null)
                    GraphQLEndpoint.Book(tncBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
                else
                    GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();
            }
            else if (scenario.IsCoBrand)
            {
                GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId, b2cToken: _testConfig.Internal.Bearer);
                var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
                GraphQLEndpoint.Book(contactBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
                var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);
                GraphQLEndpoint.Book(tncBasketGQLRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            }
            else
            {
                // Standard revenue flow
                GraphQLRequestBuilder.PassengerBasketGraphQLRequest(retrieveBasketResult, basketId);
                var contactBasketGQLRequest = GraphQLRequestBuilder.ContactBasketGraphQLRequest(_testConfig);
                var contactBasketGQLResponse = GraphQLEndpoint.Book(contactBasketGQLRequest).EnsureGQLResponseSuccess();

                if (scenario.ExpectFraudCheck)
                {
                    var contactBasketGQLResult = JsonConvert.DeserializeObject<JToken>(await contactBasketGQLResponse.Content.ReadAsStringAsync());
                    Assert.True(contactBasketGQLResult.SelectToken("data.updateContactDetails.basketId").Value<string>() == basketId, "Contact Basket ID dont match");
                }

                var tncBasketGQLRequest = GraphQLRequestBuilder.TnCBasketGQLRequest(basketId);
                var tncBasketGQLResponse = GraphQLEndpoint.Book(tncBasketGQLRequest).EnsureGQLResponseSuccess();

                if (scenario.ExpectFraudCheck)
                {
                    var tncBasketGQLResult = JsonConvert.DeserializeObject<JToken>(await tncBasketGQLResponse.Content.ReadAsStringAsync());
                    Assert.True(tncBasketGQLResult.SelectToken("data.updateTnC.basketId").Value<string>() == basketId, "Contact Basket ID dont match");
                }
            }

            // Retrieve basket after updates to get sectorId (also needed for paid/free seat scenarios)
            if ((scenario.SeatMode != SeatMode.None || scenario.HasPaidSeat || scenario.HasFreeSeat) && !scenario.UseUpdateBasketShortcut)
            {
                var (resp, result) = await RetrieveBasket(basketId, b2cToken);
                retrieveBasketResponse = resp;
                dynamic basketResult = result;
                sectorId = basketResult.data.retrieveBasket.summary.slices[0].sectors[0].sectorId.Value;
            }

            return (sectorId, retrieveBasketResponse);
        }

        public async Task<JToken> ValidateInsurancePrices(E2EScenario scenario, string basketId, string b2cToken, JToken insuranceQuoteResult, JToken insuranceDecisionResult)
        {
            // Retrieve basket after insurance
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
            HttpResponseMessage retrieveBasketGQLResponse;
            if (b2cToken != null && (scenario.BookingType == BookingType.Redemption || scenario.IsCoBrand || scenario.BookingType == BookingType.PayWithPoints))
                retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            else
                retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();

            var afterInsuranceBasketResponse = JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());

            if (scenario.Insurance == InsuranceAction.Accept)
            {
                _testConfig.Book.Cost.InsurancePrice = (double)afterInsuranceBasketResponse.SelectToken("data.retrieveBasket.summary.insurancePrice").Value<decimal>();
                double insuranceDecisionPrice = (double)insuranceDecisionResult.SelectToken("data.insuranceDecision.summary.insurancePrice").Value<decimal>();
                double insuranceQuotePrice = (double)insuranceQuoteResult.SelectToken("data.InsuranceQuote[0].products[0].totalPrice").Value<decimal>();

                Assert.True(_testConfig.Book.Cost.InsurancePrice == insuranceQuotePrice, $"\nBasket Insurance price : {_testConfig.Book.Cost.InsurancePrice} is not equal to Insurance Quote :{insuranceQuotePrice}  ");
                Assert.True(_testConfig.Book.Cost.InsurancePrice == insuranceDecisionPrice, $"\nBasket Insurance price : {_testConfig.Book.Cost.InsurancePrice} is not equal to Insurance decision :{insuranceDecisionPrice}  ");
            }
            else if (scenario.Insurance == InsuranceAction.Reject && !scenario.ExpectFraudCheck)
            {
                bool insuranceDecision = insuranceDecisionResult.SelectToken("data.insuranceDecision.isInsuranceAdded").Value<bool>();
                Assert.True(!insuranceDecision, $"\n Insurance insuranceDecision is not true : {insuranceDecision} \n Insurance Decision Response:{insuranceDecisionResult}  ");
            }

            return afterInsuranceBasketResponse;
        }

        public async Task<(double totalSeatPrice, int basketAssignedSeatsCount)> HandleSeatSelection(E2EScenario scenario, string basketId, string b2cToken, string sectorId, HttpResponseMessage retrieveBasketGQLResponse)
        {
            dynamic basketResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());
            double totalFlightPrice = GraphQLHelper.GetBasketSummaryTotalPrice(basketResult);

            if (scenario.SeatMode == SeatMode.AllSectors)
            {
                return await HandleAllSectorsSeatSelection(scenario, basketId, sectorId, retrieveBasketGQLResponse, totalFlightPrice);
            }

            if (scenario.SeatMode == SeatMode.OneWay)
            {
                return await HandleOneWaySeatSelection(scenario, basketId, sectorId, retrieveBasketGQLResponse, totalFlightPrice);
            }

            if (scenario.SeatMode == SeatMode.ClearDown)
            {
                int basketAssignedSeatsCount = await HandleClearDownSeatSelection(scenario, basketId, sectorId, retrieveBasketGQLResponse);
                return (0, basketAssignedSeatsCount);
            }

            return (0, 0);
        }

        private async Task<(double totalSeatPrice, int basketAssignedSeatsCount)> HandleAllSectorsSeatSelection(E2EScenario scenario, string basketId, string sectorId, HttpResponseMessage retrieveBasketGQLResponse, double totalFlightPrice)
        {
            if (!scenario.UseUpdateBasketShortcut)
            {
                if (scenario.PaymentMethod == PaymentMethod.PayPal && scenario.JourneyType == JourneyType.Return)
                {
                    GraphQLAssertion.ValidateSectorIdInBothSegment(retrieveBasketGQLResponse, _testConfig);
                }
                else
                {
                    GraphQLAssertion.ValidateSectorIdInBothSegment(retrieveBasketGQLResponse);
                }
            }

            string firstSectorId = ResolveFirstSectorIdForSeatValidation(scenario, retrieveBasketGQLResponse, sectorId);
            CabinName cabinName = scenario.CabinName ?? CabinName.VSCL;

            double totalSeatPrice;
            try
            {
                Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, firstSectorId, scenario.FareFamily, out HttpResponseMessage _, out string message), message);

                GraphQLHelper.SelectSeatsForAllSectors(basketId, scenario.FareFamily, retrieveBasketGQLResponse, cabinName, out totalSeatPrice, scenario.SeatProductName);

                Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, firstSectorId, scenario.FareFamily, out HttpResponseMessage _, out message), message);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(BuildAllSectorsSeatSelectionDiagnostic(scenario, basketId, sectorId, firstSectorId, cabinName, retrieveBasketGQLResponse), ex);
            }

            HttpResponseMessage retrieveBasketAfterGQLResponse = GraphQLEndpoint.Book(GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId)).EnsureGQLResponseSuccess();
            dynamic retrieveBasketAfterResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketAfterGQLResponse.Content.ReadAsStringAsync());
            ValidateBasketAfterSeatSelection(basketId, totalFlightPrice, totalSeatPrice, retrieveBasketAfterResult);

            int basketAssignedSeatsCount = GraphQLHelper.ReturnAssignedSeats(retrieveBasketAfterGQLResponse).Length;
            return (totalSeatPrice, basketAssignedSeatsCount);
        }

        private async Task<(double totalSeatPrice, int basketAssignedSeatsCount)> HandleOneWaySeatSelection(E2EScenario scenario, string basketId, string sectorId, HttpResponseMessage retrieveBasketGQLResponse, double totalFlightPrice)
        {
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, scenario.FareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message), message);
            Assert.True(GraphQLAssertion.SeatMapLegendValidation(retrieveSeatMapGQLResponse, scenario.FareFamily, out string result), result);

            dynamic retrieveBasket = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());
            dynamic retrieveSeatMapResult = JsonConvert.DeserializeObject<dynamic>(await retrieveSeatMapGQLResponse.Content.ReadAsStringAsync());
            string seatOfferId = GraphQLHelper.FindSeatOfferID(retrieveSeatMapResult);
            JArray people = (JArray)retrieveBasket.data.retrieveBasket.people;

            if (!GraphQLHelper.FindAvailableSeat(retrieveSeatMapGQLResponse, _testConfig))
            {
                _output.WriteLine($"No available seat found in cabin {scenario.CabinName ?? CabinName.VSCL}; continuing without seat selection.");
                return (0, 0);
            }

            if (!scenario.UseExplicitSearchOverload)
            {
                _testConfig.Book.Seat.SeatOfferId = seatOfferId;
            }

            bool useConfigSeatOfferId = !scenario.UseExplicitSearchOverload;
            double totalSeatPrice = await SelectSeatsForPeople(
                basketId,
                sectorId,
                people,
                seatOfferId,
                useConfigSeatOfferId,
                true);

            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, scenario.FareFamily, out HttpResponseMessage _, out message), message);

            HttpResponseMessage retrieveBasketAfterGQLResponse = GraphQLEndpoint.Book(GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId)).EnsureGQLResponseSuccess();
            dynamic retrieveBasketAfterResult = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketAfterGQLResponse.Content.ReadAsStringAsync());
            ValidateBasketAfterSeatSelection(basketId, totalFlightPrice, totalSeatPrice, retrieveBasketAfterResult);

            int basketAssignedSeatsCount = GraphQLHelper.ReturnAssignedSeats(retrieveBasketAfterGQLResponse).Length;
            return (totalSeatPrice, basketAssignedSeatsCount);
        }

        private async Task<int> HandleClearDownSeatSelection(E2EScenario scenario, string basketId, string sectorId, HttpResponseMessage retrieveBasketGQLResponse)
        {
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, scenario.FareFamily, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message), message);

            dynamic retrieveBasket = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());
            dynamic retrieveSeatMapResult = JsonConvert.DeserializeObject<dynamic>(await retrieveSeatMapGQLResponse.Content.ReadAsStringAsync());
            string seatOfferId = GraphQLHelper.FindSeatOfferID(retrieveSeatMapResult);
            JArray people = (JArray)retrieveBasket.data.retrieveBasket.people;

            if (!GraphQLHelper.FindAvailableSeat(retrieveSeatMapGQLResponse, _testConfig))
            {
                _output.WriteLine($"No available seat found in cabin {scenario.CabinName ?? CabinName.VSCL}; continuing without seat selection.");
                return 0;
            }

            await SelectSeatsForPeople(
                basketId,
                sectorId,
                people,
                seatOfferId,
                false,
                false);

            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, scenario.FareFamily, out HttpResponseMessage _, out message), message);

            GraphQLHelper.UpdateBasketWithPassengerContactAndTnCs(basketId, out sectorId, out HttpResponseMessage _);
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, scenario.FareFamily, out HttpResponseMessage retrieveSeatAfterBasketUpdateGQLResponse, out message), message);

            JObject jsonObject = JObject.Parse(retrieveSeatAfterBasketUpdateGQLResponse.ReadResponseAsString());
            string[] seats = jsonObject.SelectTokens("$.data.retrieveSeatMap.flightPassengerSeatMap[*].seatNumber").Select(x => x.ToString()).ToArray();
            foreach (string seat in seats)
            {
                Assert.True(seat == "", "Seat number should be empty after basket update clears seats.");
            }

            return 0;
        }

        private async Task<double> SelectSeatsForPeople(string basketId, string sectorId, JArray people, string seatOfferId, bool useConfigSeatOfferId, bool validateSeatAmountCurrency)
        {
            double totalSeatPrice = 0;
            int seatEligiblePassengerCount = people.Count(item => !item["type"].ToString().Is("INFANT"));
            int selectedPassengerCount = 0;

            foreach (JToken item in people)
            {
                if (item["type"].ToString().Is("INFANT"))
                {
                    continue;
                }

                int passengerId = Convert.ToInt32(item["id"]);
                string selectedSeatOfferId = useConfigSeatOfferId ? _testConfig.Book.Seat.SeatOfferId : seatOfferId;

                string selectSeatRequest = GraphQLRequestBuilder.SeatSelectionGQLRequest(
                    basketId,
                    sectorId,
                    selectedSeatOfferId,
                    passengerId.ToString(),
                    _testConfig.Book.Seat.SeatNumber,
                    _testConfig.Book.Seat.Amount,
                    _testConfig.Book.Seat.Currency);

                HttpResponseMessage seatSelectionGQLResponse = GraphQLEndpoint.Book(selectSeatRequest).EnsureGQLResponseSuccess();
                dynamic selectSeatGQLResult = JsonConvert.DeserializeObject<dynamic>(await seatSelectionGQLResponse.Content.ReadAsStringAsync());

                Assert.NotNull(selectSeatGQLResult.data.selectSeat.flightPassengerSeatMap[passengerId].seatNumber.Value);
                Assert.True(
                    selectSeatGQLResult.data.selectSeat.flightPassengerSeatMap[passengerId].seatNumber == _testConfig.Book.Seat.SeatNumber,
                    $" Selected Seat number dont match with assginged SeatNumber for passenger {passengerId} \n  Passenger Seat Number = {selectSeatGQLResult.data.selectSeat.flightPassengerSeatMap[passengerId].seatNumber} \n Selected Seat Number = {_testConfig.Book.Seat.SeatNumber}");

                Assert.True(GraphQLAssertion.ValidateColumnSeatSelected(seatSelectionGQLResponse, _testConfig.Book.Seat.SeatNumber, out string message), message);
                if (validateSeatAmountCurrency)
                {
                    Assert.True(
                        GraphQLAssertion.ValidateColumnSeatAmountCurrencySelected(
                            seatSelectionGQLResponse,
                            _testConfig.Book.Seat.SeatNumber,
                            _testConfig.Book.Seat.Amount,
                            _testConfig.Book.Seat.Currency,
                            out message),
                        message);
                }

                totalSeatPrice += _testConfig.Book.Seat.Amount;
                selectedPassengerCount++;
                if (selectedPassengerCount < seatEligiblePassengerCount)
                {
                    if (!GraphQLHelper.FindAvailableSeat(seatSelectionGQLResponse, _testConfig))
                    {
                        _output.WriteLine("No further available seats found; continuing with the seats already selected.");
                        break;
                    }
                }
            }

            return totalSeatPrice;
        }

    private const double PriceComparisonTolerance = 0.01;

    private static void ValidateBasketAfterSeatSelection(string basketId, double totalFlightPrice, double totalSeatPrice, dynamic retrieveBasketAfterResult)
    {
        Assert.True(retrieveBasketAfterResult.data.retrieveBasket.basketId.Value == basketId, "Basket ID dont match");
        double expectedTotalPrice = totalFlightPrice + totalSeatPrice;
        double actualTotalPrice = GraphQLHelper.GetBasketSummaryTotalPrice(retrieveBasketAfterResult);
        double priceDifference = Math.Abs(expectedTotalPrice - actualTotalPrice);

        Assert.True(
            priceDifference <= PriceComparisonTolerance,
            $" Total price is not valid -----\n Total Seat+flight price : {expectedTotalPrice} \n Total price from Basket : {actualTotalPrice} \n Difference : {priceDifference} \n Tolerance : {PriceComparisonTolerance}");
    }

        private string ResolveFirstSectorIdForSeatValidation(E2EScenario scenario, HttpResponseMessage retrieveBasketGQLResponse, string sectorId)
        {
            if (!sectorId.IsNullOrWhiteSpace())
            {
                return sectorId;
            }

            if (!(_testConfig.Book.Seat.SectorId.IsNullOrWhiteSpace()))
            {
                return _testConfig.Book.Seat.SectorId;
            }

            if (scenario.PaymentMethod == PaymentMethod.PayPal && scenario.JourneyType == JourneyType.Return)
            {
                if (_testConfig.Book.Seat.SectorOutbound is { Length: > 0 } && !_testConfig.Book.Seat.SectorOutbound[0].IsNullOrWhiteSpace())
                {
                    return _testConfig.Book.Seat.SectorOutbound[0];
                }
            }

            JObject retrieveBasketResult = JObject.Parse(retrieveBasketGQLResponse.ReadResponseAsString());
            string firstBasketSectorId = retrieveBasketResult
                .SelectTokens("$.data.retrieveBasket.summary.slices[*].sectors[*].sectorId")
                .Select(token => token.ToString())
                .FirstOrDefault(id => !id.IsNullOrWhiteSpace());

            Assert.True(!firstBasketSectorId.IsNullOrWhiteSpace(), "No sectorId available in retrieveBasket response for seat selection.");
            return firstBasketSectorId;
        }

        public async Task HandlePaidSeatForInsuranceTests(E2EScenario scenario, string basketId, string b2cToken, HttpResponseMessage retrieveBasketGQLResponse)
        {
            GraphQLHelper.GetBasketOutBoundSectorId(retrieveBasketGQLResponse, _testConfig);

            var sectorId = _testConfig.Internal.OutboundSectorIds[0];
            Assert.True(GraphQLHelper.RetrieveSeatMapGraphQL(basketId, sectorId, _testConfig.Search.FareFamilyType, out HttpResponseMessage retrieveSeatMapGQLResponse, out string message, b2cToken ?? ""), message);

            dynamic retrieveBasket = JsonConvert.DeserializeObject<dynamic>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());
            dynamic retrieveSeatMapResult = JsonConvert.DeserializeObject<dynamic>(await retrieveSeatMapGQLResponse.Content.ReadAsStringAsync());
            string seatOfferId = GraphQLHelper.FindSeatOfferID(retrieveSeatMapResult);
            var people = (JArray)retrieveBasket.data.retrieveBasket.people;

            _testConfig.Book.Seat.SeatProductName = scenario.SeatProductName;
            _testConfig.Book.Seat.CabinName = scenario.CabinName ?? CabinName.VSCL;
            if (!GraphQLHelper.FindAvailableSeat(retrieveSeatMapGQLResponse, _testConfig))
            {
                _output.WriteLine($"No available paid seat found in cabin {scenario.CabinName ?? CabinName.VSCL}; continuing without seat selection.");
                return;
            }

            int seatEligiblePassengerCount = people.Count(item => !item["type"].ToString().Is("INFANT"));
            int selectedPassengerCount = 0;
            foreach (var item in people)
            {
                if (item["type"].ToString().Is("INFANT"))
                {
                    continue;
                }

                int passengerId = Convert.ToInt32(item["id"]);
                var selectSeatReq = GraphQLRequestBuilder.SeatSelectionGQLRequest(basketId, sectorId, seatOfferId, passengerId.ToString(), _testConfig.Book.Seat.SeatNumber, _testConfig.Book.Seat.Amount, _testConfig.Book.Seat.Currency);
                var seatSelectionGQLResponse = GraphQLEndpoint.Book(selectSeatReq, b2cToken ?? "").EnsureGQLResponseSuccess();

                GraphQLAssertion.SeatSelectionValidation(seatSelectionGQLResponse, passengerId, _testConfig.Book.Seat.SeatNumber);
                Assert.True(GraphQLAssertion.ValidateColumnSeatSelected(seatSelectionGQLResponse, _testConfig.Book.Seat.SeatNumber, out message), message);
                Assert.True(GraphQLAssertion.ValidateColumnSeatAmountCurrencySelected(seatSelectionGQLResponse, _testConfig.Book.Seat.SeatNumber, _testConfig.Book.Seat.Amount, _testConfig.Book.Seat.Currency, out message), message);

                selectedPassengerCount++;
                if (selectedPassengerCount < seatEligiblePassengerCount)
                {
                    if (!GraphQLHelper.FindAvailableSeat(seatSelectionGQLResponse, _testConfig))
                    {
                        _output.WriteLine("No further available paid seats found; continuing with the seats already selected.");
                        break;
                    }
                }
            }
        }

        public async Task<(string selectedPoints, string b2cToken, string fcMembershipNumber)> HandlePointsConverter(string basketId, string b2cToken, int fqTPointsBeforeBooking, E2EScenario scenario, string fcMembershipNumber, HttpResponseMessage retrieveBasketGQLResponse)
        {
            // For PWP tests that need loyalty setup after search
            Dictionary<string, string> loyaltyDetails = null;
            if (b2cToken == null && !scenario.IsCoBrand)
            {
                int passengerCount = scenario.Adults + scenario.Children + scenario.Infants + scenario.Teens;
                int minimumRequiredPoints = passengerCount * 3000;

                if (scenario.UseRetryHelperForLoyalty)
                {
                    loyaltyDetails = new Dictionary<string, string>();
                    await RetryHelper.RunWithRetryAsync(async () =>
                    {
                        loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(scenario.LoyaltyTierCode, int.Parse(scenario.PayWithPointsAmount), false);
                        Assert.True(loyaltyDetails is not null, "Loyalty lookup returned null dictionary.");
                        Assert.True(loyaltyDetails.TryGetValue("rowKey", out fcMembershipNumber) && !fcMembershipNumber.IsNullOrWhiteSpace(),
                            $"Loyalty lookup did not return 'rowKey'. Tier: {scenario.LoyaltyTierCode}, Points threshold: {scenario.PayWithPointsAmount}, Error: {(loyaltyDetails.TryGetValue("error", out string retryError) ? retryError : "none")}");
                        Assert.True(loyaltyDetails.TryGetValue("points", out string loyaltyPointsValue) && !loyaltyPointsValue.IsNullOrWhiteSpace(),
                            $"Loyalty lookup did not return 'points'. Tier: {scenario.LoyaltyTierCode}, Points threshold: {scenario.PayWithPointsAmount}");
                        int loyatlyPointsInt = int.Parse(loyaltyPointsValue);
                        if (!ShouldBypassVRedBalanceRefreshInUat(scenario))
                        {
                            System.Net.Http.HttpResponseMessage vRedResponseAPI = MicroserviceEndpoint.Vred(GetRequiredVRedToken("Book E2E pay-with-points retry VRed balance lookup"), fcMembershipNumber).EnsureIsSuccessStatusCode();
                            dynamic vRedResultAPI = JsonConvert.DeserializeObject<dynamic>(await vRedResponseAPI.Content.ReadAsStringAsync());
                            int vredPoints = (int)vRedResultAPI.balance.usable.Value;
                            if (vredPoints.ToString() != loyaltyPointsValue)
                                await LoyaltyAccountUpdates.UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, vredPoints);
                            loyatlyPointsInt = vredPoints;
                            loyaltyDetails["points"] = vredPoints.ToString();
                        }

                        Assert.True(loyatlyPointsInt >= minimumRequiredPoints);
                    }, 3);
                }
                else
                {
                    int maxAttempts = 5;
                    int attempt = 0;
                    while (attempt < maxAttempts)
                    {
                        loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(scenario.LoyaltyTierCode, int.Parse(scenario.PayWithPointsAmount), false);
                        Assert.True(loyaltyDetails is not null, "Loyalty lookup returned null dictionary.");
                        Assert.True(loyaltyDetails.TryGetValue("rowKey", out fcMembershipNumber) && !fcMembershipNumber.IsNullOrWhiteSpace(),
                            $"Loyalty lookup did not return 'rowKey'. Tier: {scenario.LoyaltyTierCode}, Points threshold: {scenario.PayWithPointsAmount}, Error: {(loyaltyDetails.TryGetValue("error", out string lookupError) ? lookupError : "none")}");
                        Assert.True(loyaltyDetails.TryGetValue("points", out string loyaltyPointsValue) && !loyaltyPointsValue.IsNullOrWhiteSpace(),
                            $"Loyalty lookup did not return 'points'. Tier: {scenario.LoyaltyTierCode}, Points threshold: {scenario.PayWithPointsAmount}");
                        int vredPoints = int.Parse(loyaltyPointsValue);
                        if (!ShouldBypassVRedBalanceRefreshInUat(scenario))
                        {
                            System.Net.Http.HttpResponseMessage vRedResponseAPI = MicroserviceEndpoint.Vred(GetRequiredVRedToken("Book E2E pay-with-points VRed balance lookup"), fcMembershipNumber).EnsureIsSuccessStatusCode();
                            dynamic vRedResultAPI = JsonConvert.DeserializeObject<dynamic>(await vRedResponseAPI.Content.ReadAsStringAsync());
                            vredPoints = (int)vRedResultAPI.balance.usable.Value;
                            await LoyaltyAccountUpdates.UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, vredPoints);
                            loyaltyDetails["points"] = vredPoints.ToString();
                        }

                        if (ShouldBypassVRedBalanceRefreshInUat(scenario) && int.Parse(loyaltyPointsValue) >= minimumRequiredPoints)
                            break;

                        if (loyaltyPointsValue == vredPoints.ToString() && vredPoints <= int.Parse(scenario.PayWithPointsAmount) && int.Parse(loyaltyPointsValue) >= minimumRequiredPoints)
                            break;
                        attempt++;
                    }
                    Assert.True(int.Parse(loyaltyDetails["points"]) >= minimumRequiredPoints, $"FC account : {loyaltyDetails["rowKey"]}  points : {loyaltyDetails["points"]}  is not greater than {minimumRequiredPoints}");
                }

                fqTPointsBeforeBooking = int.Parse(loyaltyDetails["points"]);
                b2cToken = await GenerateLoginB2CTokenFromApi(fcMembershipNumber, loyaltyDetails);

                var retrieveBasketRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
                GraphQLEndpoint.Book(retrieveBasketRequest, b2cToken).EnsureGQLResponseSuccess();
            }

            var pointToCurrencyConverterRequest = GraphQLRequestBuilder.PointToCurrencyConverter(basketId);
            var pointToCurrencyConverterResponse = GraphQLEndpoint.Book(pointToCurrencyConverterRequest, b2cToken ?? _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            dynamic pointToCurrencyConverterResult = JsonConvert.DeserializeObject<dynamic>(await pointToCurrencyConverterResponse.Content.ReadAsStringAsync());

            if (retrieveBasketGQLResponse != null)
                Assert.True(GraphQLAssertion.ValidateCurrencyPayToCurrencyConverter(pointToCurrencyConverterResponse, retrieveBasketGQLResponse, out string errorMessage), $"Pay with points Currency Validation: {errorMessage} \n\tResponse : \n\t{pointToCurrencyConverterResult} ");

            string selectedPoints;
            if (scenario.FullPointsSelection)
            {
                selectedPoints = GraphQLHelper.FullPointsSelectionInCurrencyConverter(pointToCurrencyConverterResponse);
                Assert.True(!selectedPoints.IsNullOrEmpty(), " Selected Points is empty or Null");
            }
            else
            {
                selectedPoints = GraphQLHelper.PointsSelectionInCurrencyConverter(pointToCurrencyConverterResponse);
            }

            return (selectedPoints, b2cToken, fcMembershipNumber);
        }

        public async Task<string> HandleCoBrand(E2EScenario scenario, string basketId, string b2cToken)
        {
            var paymentRetGraphqlRequest = GraphQLRequestBuilder.RetrievePaymentGQLRequest(basketId);
            var paymentRetGraphqlResponse = GraphQLEndpoint.Book(paymentRetGraphqlRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);

            var synchronyTokenRequest = GraphQLRequestBuilder.SynchronyToken(basketId);
            var synchronyTokenResponse = GraphQLEndpoint.Book(synchronyTokenRequest, _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            var synchronyTokenResult = JsonConvert.DeserializeObject<JToken>(await synchronyTokenResponse.Content.ReadAsStringAsync());
            string synchronyToken = synchronyTokenResult.SelectToken("$..token").ToString();
            Assert.True(!synchronyToken.IsNullOrEmpty(), $"Synchrony token not exist in response: {synchronyTokenResult}");

            if (synchronyApplyCardUrl.IsNullOrWhiteSpace()
                || !Uri.TryCreate(synchronyApplyCardUrl, UriKind.Absolute, out _))
            {
                throw new InvalidOperationException(
                    "SynchronyApplyCardUrl is missing or invalid. Configure the SIT Synchrony application base URL before running co-brand journeys.");
            }

            string synchronyUrl = synchronyApplyCardUrl + $"{synchronyToken}";
            using var synchronyApplyPage = new SynchronyApplyPage(synchronyUrl);
            synchronyApplyPage.ApplySynchronyVirginRedRewardsMasterCard(_testConfig);

            var reteriveCoBrandApplicationStatusRequest = GraphQLRequestBuilder.RetrieveCoBrandApplicationStatus(basketId, synchronyToken);
            var reteriveCoBrandApplicationStatusResponse = await RetrieveCoBrandApplicationStatusWithRetry(reteriveCoBrandApplicationStatusRequest);
            var reteriveCoBrandApplicationStatusResult = JsonConvert.DeserializeObject<JToken>(await reteriveCoBrandApplicationStatusResponse.Content.ReadAsStringAsync());
            string applicationDecision = reteriveCoBrandApplicationStatusResult.SelectToken("$..applicationDecision").ToString();
            Assert.True(applicationDecision == scenario.SynchronyApplicationStatus, $"Application is not correct {reteriveCoBrandApplicationStatusResult}");

            return synchronyToken;
        }

        private async Task<HttpResponseMessage> RetrieveCoBrandApplicationStatusWithRetry(string request)
        {
            const int maxAttempts = 4;
            HttpResponseMessage response = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                response = GraphQLEndpoint.Book(request, _testConfig.Internal.Bearer);
                if (response.IsGQLResponseSuccess())
                {
                    return response.EnsureGQLResponseSuccess();
                }

                string responseBody = response.ReadResponseAsString();
                bool retryable = responseBody.Contains("Http connector failed", StringComparison.OrdinalIgnoreCase)
                    || responseBody.Contains("Unhandled Exception in retrieve co brand application status", StringComparison.OrdinalIgnoreCase);
                if (!retryable || attempt == maxAttempts)
                {
                    return response.EnsureGQLResponseSuccess();
                }

                response.Dispose();
                await Task.Delay(TimeSpan.FromSeconds(attempt));
            }

            return response.EnsureGQLResponseSuccess();
        }

        public async Task<JToken> GetFinalBasketState(string basketId, string b2cToken)
        {
            var retrieveBasketGQLRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
            HttpResponseMessage retrieveBasketGQLResponse;
            if (b2cToken != null)
                retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest, b2cToken).EnsureGQLResponseSuccess();
            else
                retrieveBasketGQLResponse = GraphQLEndpoint.Book(retrieveBasketGQLRequest).EnsureGQLResponseSuccess();
            return JsonConvert.DeserializeObject<JToken>(await retrieveBasketGQLResponse.Content.ReadAsStringAsync());
        }

        public async Task<bool> HandlePayment(E2EScenario scenario, string basketId, string b2cToken, string selectedPoints, string synchronyToken, JToken afterInsuranceBasketResponse)
        {
            // For CoBrand, payment PRI was already retrieved in HandleCoBrand
            if (!scenario.IsCoBrand)
            {
                HttpResponseMessage basketResponseForPaymentValidation = null;
                JObject basketResponseResult = null;
                double amountToPay = 0.0;

                if (scenario.ValidatePaymentRetreive)
                {
                    string basketRequest = GraphQLRequestBuilder.RetrieveBasketGQLRequest(basketId);
                    if (scenario.UseExplicitSearchOverload || scenario.BookingType == BookingType.Redemption || scenario.BookingType == BookingType.PayWithPoints)
                    {
                        basketResponseForPaymentValidation = GraphQLEndpoint.Book(basketRequest, b2cToken ?? "").EnsureGQLResponseSuccess();
                    }
                    else
                    {
                        basketResponseForPaymentValidation = GraphQLEndpoint.Book(basketRequest).EnsureGQLResponseSuccess();
                    }

                    basketResponseResult = JsonConvert.DeserializeObject<JObject>(await basketResponseForPaymentValidation.Content.ReadAsStringAsync());
                    amountToPay = basketResponseResult.SelectToken("data.retrieveBasket.summary.paymentAmount")?.Value<double>()
                        ?? throw new Exception($"Unable to read basket paymentAmount before retrievePayment call. BasketId={basketId}");
                }

                // First PRI call intentionally uses default amountToPay (0.0 from request builder).
                // Current backend flow creates/initializes payment session on this path for fresh baskets.
                string paymentRetGraphqlRequest = (selectedPoints != null)
                    ? GraphQLRequestBuilder.RetrievePaymentGQLRequest(basketId, selectedPoints)
                    : GraphQLRequestBuilder.RetrievePaymentGQLRequest(basketId);
                string priPrecheckDiagnostic = BuildPriPrecheckDiagnostic(scenario, basketId, amountToPay, basketResponseResult, afterInsuranceBasketResponse, paymentRetGraphqlRequest);
                bool requiresAuthenticatedGraphQlCall = scenario.IsLoggedIn
                    || scenario.UseExplicitSearchOverload
                    || scenario.BookingType == BookingType.Redemption
                    || scenario.BookingType == BookingType.PayWithPoints;

                HttpResponseMessage paymentRetGraphqlResponse;
                if (requiresAuthenticatedGraphQlCall)
                {
                    try
                    {
                        paymentRetGraphqlResponse = await RetrievePaymentReferenceWithRetry(paymentRetGraphqlRequest, b2cToken ?? "");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{priPrecheckDiagnostic}{System.Environment.NewLine}PRI request failed: {ex.Message}", ex);
                    }
                }
                else
                {
                    try
                    {
                        paymentRetGraphqlResponse = await RetrievePaymentReferenceWithRetry(paymentRetGraphqlRequest);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{priPrecheckDiagnostic}{System.Environment.NewLine}PRI request failed: {ex.Message}", ex);
                    }
                }

                if (scenario.ValidatePaymentRetreive)
                {
                    GraphQLAssertion.ValidatePaymentRetreiveGQL(basketResponseForPaymentValidation, paymentRetGraphqlResponse);
                }

                // For PWP, extract the selected points amount from the PRI response
                if (scenario.BookingType == BookingType.PayWithPoints && selectedPoints != null)
                {
                    dynamic priResult = JsonConvert.DeserializeObject<dynamic>(await paymentRetGraphqlResponse.Content.ReadAsStringAsync());
                    double pwpAmount = (double?)priResult?.data?.orderPaymentReferenceId?.summary?.selectedPayWithPointsOption?.amount?.Value ?? 0.0;
                    _testConfig.Book.Cost.SelectedPWPAmount = pwpAmount;
                }

                // Full PWP with rejected insurance - no card payment, skip PRI/HPP
                if (scenario.FullPointsSelection && scenario.Insurance != InsuranceAction.Accept)
                    return false;

                GraphQLAssertion.GetPriGQL(paymentRetGraphqlResponse, _testConfig);
            }

            // HPP
            bool useApiForCardPayment = ShouldUseApiForCardPayment(scenario);
            bool usePlaywrightForHpp = ShouldUsePlaywrightForHpp(scenario);
            if (useApiForCardPayment)
            {
                await ApiHostedPaymentAutomation.SubmitCardAsync(_testConfig);
            }
            else if (scenario.IsCoBrand)
            {
                _testConfig.Book.Order.HppUrl = _testConfig.Book.Order.HppUrl + $"&cbToken={synchronyToken}";

                if (usePlaywrightForHpp)
                {
                    if (scenario.UseSynchronyCardPayment)
                    {
                        await SubmitHppExistingPaymentWithPlaywright(expandCard: false);
                    }
                    else
                    {
                        _testConfig.Book.Payment = new Payment(CardType.Visa);
                        if (scenario.SynchronyApplicationStatus == "APPROVED")
                            await SubmitHppExistingPaymentWithPlaywright(expandCard: true);
                        else
                            await SubmitHppCardDetailsWithPlaywright();
                    }
                }
                else if (scenario.UseSynchronyCardPayment)
                {
                    using var enterHppCardDetails = new NavigateToHostedPaymentPage(_testConfig.Book.Order.HppUrl);
                    enterHppCardDetails.SubmitDetailsForSynchronyCardPayment();
                }
                else
                {
                    _testConfig.Book.Payment = new Payment(CardType.Visa);
                    using var enterHppCardDetails = new NavigateToHostedPaymentPage(_testConfig.Book.Order.HppUrl);
                    if (scenario.SynchronyApplicationStatus == "APPROVED")
                        enterHppCardDetails.SubmitAndVerifyHppCardDetails(_testConfig);
                    else
                        enterHppCardDetails.SubmitHppCardDetails(_testConfig);
                }
            }
            else if (scenario.PaymentMethod == PaymentMethod.PayPal && usePlaywrightForHpp)
            {
                _testConfig.Book.Order.ChildPRI = await SubmitHppPayPalWithPlaywright();
            }
            else if (scenario.PaymentMethod == PaymentMethod.PayPal)
            {
                using var payPalPayment = new NavigateToHostedPaymentPage(_testConfig.Book.Order.HppUrl);
                payPalPayment.SubmitPayPal(_testConfig);
                _testConfig.Book.Order.ChildPRI = payPalPayment.GetChildPri();
            }
            else if (usePlaywrightForHpp)
            {
                await SubmitHppCardDetailsWithPlaywright();
            }
            else if (scenario.UseExplicitSearchOverload || scenario.BookingType == BookingType.Redemption)
            {
                using var enterHppCardDetails = new NavigateToHostedPaymentPage(_testConfig.Book.Order.HppUrl);
                enterHppCardDetails.SubmitHppCardDetails(_testConfig);
            }
            else if (scenario.PaymentCardType.HasValue)
            {
                using var enterHppCardDetails = new NavigateToHostedPaymentPage(_testConfig.Book.Order.HppUrl);
                _testConfig.Book.Payment = new Core.TestDataConfig.Payment(scenario.PaymentCardType.Value);
                enterHppCardDetails.SubmitHppCardDetails(_testConfig);
            }
            else if (scenario.IsMultiCity)
            {
                using var enterHppCardDetails = new NavigateToHostedPaymentPage(_testConfig.Book.Order.HppUrl);
                _testConfig.Book.Payment = new Payment();
                enterHppCardDetails.SubmitHppCardDetails(_testConfig);
            }
            else
            {
                using var enterHppCardDetails = new NavigateToHostedPaymentPage(_testConfig.Book.Order.HppUrl);
                enterHppCardDetails.SubmitHppCardDetails(_testConfig);
            }

            // Temporary UAT mitigation: for logged-in flows, stop after card details.
            // Quick backout is supported via BookUatLoggedInStopAfterCardDetails=false in environment settings.
            if (ShouldStopAfterCardDetailsInUat(scenario))
            {
                return true;
            }

            // Payment status
            string priForStatus, paymentType = null;
            if (scenario.PaymentMethod == PaymentMethod.PayPal)
            {
                priForStatus = _testConfig.Book.Order.ChildPRI;
                paymentType = "PayPal";
            }
            else
            {
                priForStatus = _testConfig.Book.Order.PRI;
            }

            var paymentStatusGraphqlRequest = (paymentType != null)
                ? GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(basketId, priForStatus, paymentType)
                : GraphQLRequestBuilder.PaymentStatusGQLRequestBuild(basketId, priForStatus);

            HttpResponseMessage paymentStatusGraphQLResponse;
            if (b2cToken != null && (scenario.BookingType != BookingType.Revenue || scenario.IsCoBrand))
                paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest, b2cToken ?? _testConfig.Internal.Bearer).EnsureGQLResponseSuccess();
            else
                paymentStatusGraphQLResponse = GraphQLEndpoint.Book(paymentStatusGraphqlRequest).EnsureGQLResponseSuccess();

            var paymentStatusGraphQLResult = JsonConvert.DeserializeObject<dynamic>(await paymentStatusGraphQLResponse.Content.ReadAsStringAsync());
            Assert.True(paymentStatusGraphQLResult.data.bookUpdatePaymentStatus.paymentStatus.Value == "SUCCESS", $"Payment status is not success -- \n \t Request ---:{paymentStatusGraphqlRequest} \n \t Response -- : {paymentStatusGraphQLResult} ");

            // Set basket prices if needed
            if (afterInsuranceBasketResponse != null && scenario.ExpectFraudCheck)
            {
                _testConfig.Book.Cost.TotalPrice = (double)(afterInsuranceBasketResponse.SelectToken("data.retrieveBasket.summary.totalPrice")?.Value<decimal>() ?? 0.0m);
                _testConfig.Book.Cost.FlightPrice = (double)(afterInsuranceBasketResponse.SelectToken("data.retrieveBasket.summary.flightPrice")?.Value<decimal>() ?? 0.0m);
            }

            return false;
        }

        private async Task SubmitHppCardDetailsWithPlaywright()
        {
            bool headless = ConfigReader.Get("BookHppPlaywrightHeadless", true);
            await HostedPaymentAutomation.SubmitCardAsync(_testConfig, usePlaywright: true, headless: headless);
        }

        private async Task<string> SubmitHppPayPalWithPlaywright()
        {
            bool headless = ConfigReader.Get("BookHppPlaywrightHeadless", true);
            return await HostedPaymentAutomation.SubmitPayPalAsync(_testConfig, usePlaywright: true, headless: headless);
        }

        private async Task SubmitHppExistingPaymentWithPlaywright(bool expandCard)
        {
            bool headless = ConfigReader.Get("BookHppPlaywrightHeadless", true);
            await HostedPaymentAutomation.SubmitExistingPaymentAsync(_testConfig, expandCard, headless);
        }

        private static bool ShouldStopAfterCardDetailsInUat(E2EScenario scenario)
        {
            if (!Config.Environment.Is("uat"))
            {
                return false;
            }

            string stopAfterCardDetailsSetting = ConfigReader.Get("BookUatLoggedInStopAfterCardDetails");
            if (!stopAfterCardDetailsSetting.IsNullOrWhiteSpace()
                && bool.TryParse(stopAfterCardDetailsSetting, out bool isEnabled)
                && !isEnabled)
            {
                return false;
            }

            return scenario.BookingType == BookingType.PayWithPoints
                || scenario.BookingType == BookingType.Redemption
                || scenario.IsCoBrand;
        }

        private static bool ShouldUsePlaywrightForHpp(E2EScenario scenario)
        {
            string automationMode = ConfigReader.Get("BookHppAutomationMode");
            if (automationMode.Is("selenium"))
            {
                return false;
            }

            if (automationMode.Is("playwright"))
            {
                // Co-brand remains an explicit scenario pilot until its external application
                // journey is available for end-to-end verification.
                return !scenario.IsCoBrand;
            }

            return scenario.UsePlaywrightForHpp;
        }

        private static bool ShouldUseApiForCardPayment(E2EScenario scenario)
        {
            if (scenario.PaymentMethod != PaymentMethod.Card || scenario.Is3DS || scenario.IsCoBrand)
            {
                return false;
            }

            string paymentMode = ConfigReader.Get("BookCardPaymentMode");
            if (paymentMode.IsNullOrWhiteSpace() || paymentMode.Is("Api"))
            {
                return true;
            }

            if (paymentMode.Is("Hpp"))
            {
                return false;
            }

            throw new InvalidOperationException(
                $"Unsupported BookCardPaymentMode '{paymentMode}'. Use Api or Hpp.");
        }

        private static bool ShouldBypassVRedBalanceRefreshInUat(E2EScenario scenario) => ShouldStopAfterCardDetailsInUat(scenario);

        private string GetRequiredVRedToken(string usage)
        {
            _tokenFixture.AddTokenForVRedFromKeyVault("vred");
            return _tokenFixture.GetRequiredToken("vred", usage);
        }

        private static string BuildPriPrecheckDiagnostic(
            E2EScenario scenario,
            string basketId,
            double amountToPay,
            JObject basketResponseResult,
            JToken afterInsuranceBasketResponse,
            string paymentRetGraphqlRequest)
        {
            JToken summaryToken = basketResponseResult?.SelectToken("data.retrieveBasket.summary")
                ?? afterInsuranceBasketResponse?.SelectToken("data.retrieveBasket.summary");

            decimal basketPaymentAmount = summaryToken?.SelectToken("paymentAmount")?.Value<decimal>() ?? 0m;
            decimal basketTotalPrice = summaryToken?.SelectToken("totalPrice")?.Value<decimal>() ?? 0m;
            decimal basketFlightPrice = summaryToken?.SelectToken("flightPrice")?.Value<decimal>() ?? 0m;
            decimal basketSeatPrice = summaryToken?.SelectToken("totalSeatPrice")?.Value<decimal>() ?? 0m;
            decimal basketInsurancePrice = summaryToken?.SelectToken("insurancePrice")?.Value<decimal>() ?? 0m;
            int adults = summaryToken?.SelectToken("numberOfAdults")?.Value<int>() ?? scenario.Adults;
            int teens = summaryToken?.SelectToken("numberOfYouths")?.Value<int>() ?? scenario.Teens;
            int children = summaryToken?.SelectToken("numberOfChildren")?.Value<int>() ?? scenario.Children;
            int infants = summaryToken?.SelectToken("numberOfInfants")?.Value<int>() ?? scenario.Infants;

            return $"PRI precheck -> Test={scenario.TestName}, BasketId={basketId}, JourneyType={scenario.JourneyType}, FareFamily={scenario.FareFamily}, FlightType={scenario.FlightType}, " +
                   $"Pax(A/T/C/I)={adults}/{teens}/{children}/{infants}, AmountToPay={amountToPay:0.00}, BasketPaymentAmount={basketPaymentAmount:0.00}, BasketTotalPrice={basketTotalPrice:0.00}, " +
                   $"FlightPrice={basketFlightPrice:0.00}, SeatPrice={basketSeatPrice:0.00}, InsurancePrice={basketInsurancePrice:0.00}, " +
                   $"PaymentMethod={scenario.PaymentMethod}, BookingType={scenario.BookingType}, PRIRequest={paymentRetGraphqlRequest}";
        }

        private static async Task<HttpResponseMessage> RetrievePaymentReferenceWithRetry(string paymentRetGraphqlRequest, string b2cToken = "")
        {
            const int maxAttempts = 3;
            const int retryDelayMs = 1000;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                HttpResponseMessage response = string.IsNullOrEmpty(b2cToken)
                    ? GraphQLEndpoint.Book(paymentRetGraphqlRequest)
                    : GraphQLEndpoint.Book(paymentRetGraphqlRequest, b2cToken);

                if (response.IsGQLResponseSuccess())
                {
                    return response.EnsureGQLResponseSuccess();
                }

                string responseBody = response.ReadResponseAsString();
                bool isRetryablePriFailure =
                    responseBody.Contains("Unhandled Exception in create PRI from payment mutation.", StringComparison.OrdinalIgnoreCase) ||
                    responseBody.Contains("Http connector failed", StringComparison.OrdinalIgnoreCase) ||
                    responseBody.Contains("Object reference not set to an instance of an object", StringComparison.OrdinalIgnoreCase);

                if (!isRetryablePriFailure || attempt == maxAttempts)
                {
                    return response.EnsureGQLResponseSuccess();
                }

                // amountToPay=0 intentionally requests a new payment session. Replacing it
                // with the basket amount on retry enters the existing-session server path.
                await Task.Delay(retryDelayMs);
            }

            throw new Exception("Failed to retrieve payment reference after retry attempts.");
        }

        public async Task ValidatePaymentMSPrices(E2EScenario scenario, JToken afterInsuranceBasketResponse)
        {
            HttpResponseMessage paymentMSResponse = MicroserviceEndpoint.GetOrderPayment(_testConfig.Book.Order.PRI).EnsureIsSuccessStatusCode();
            var paymentMSResult = JsonConvert.DeserializeObject<dynamic>(await paymentMSResponse.Content.ReadAsStringAsync());

            double basketTotalPrice = (double)(afterInsuranceBasketResponse.SelectToken("data.retrieveBasket.summary.totalPrice")?.Value<decimal>() ?? 0.0m);
            _testConfig.Book.Cost.TotalPrice = basketTotalPrice;
            _testConfig.Book.Cost.FlightPrice = (double)(afterInsuranceBasketResponse.SelectToken("data.retrieveBasket.summary.flightPrice")?.Value<decimal>() ?? 0.0m);

            double paymentAuthTotalPrice = (double?)paymentMSResult?.paymentDetails?[0]?.amount?.monetaryAmount ?? 0.0;

            double expectedPrice = basketTotalPrice;
            if (scenario.BookingType == BookingType.PayWithPoints && _testConfig.Book.Cost.SelectedPWPAmount > 0)
                expectedPrice = Math.Round(basketTotalPrice - _testConfig.Book.Cost.SelectedPWPAmount, 2);

            Assert.True(expectedPrice == paymentAuthTotalPrice, $"Payment Total price [{paymentAuthTotalPrice}] is not equal to expected price [{expectedPrice}] (Basket Total: {basketTotalPrice}, PWP Amount: {_testConfig.Book.Cost.SelectedPWPAmount})  -- \n \t Payment MS Response ---:{paymentMSResult} \n \t Basket GQL Response -- : {afterInsuranceBasketResponse} ");

            if (scenario.Insurance == InsuranceAction.Accept)
            {
                double flightPlusSeats = _testConfig.Book.Cost.FlightPrice;
                if (scenario.HasPaidSeat)
                {
                    _testConfig.Book.Cost.TotalSeatPrice = (double)(afterInsuranceBasketResponse.SelectToken("data.retrieveBasket.summary.totalSeatPrice")?.Value<decimal>() ?? 0.0m);
                    flightPlusSeats += _testConfig.Book.Cost.TotalSeatPrice;
                }
                double priceForInsuranceDiff = (scenario.BookingType == BookingType.PayWithPoints)
                    ? basketTotalPrice
                    : paymentAuthTotalPrice;
                double insurnaceActualDiff = Math.Round(priceForInsuranceDiff - flightPlusSeats, 2);
                Assert.True(_testConfig.Book.Cost.InsurancePrice == insurnaceActualDiff, $"Insurnace Diff price [{insurnaceActualDiff}] does not match Basket Insurance price [{_testConfig.Book.Cost.InsurancePrice}]  -- \n \t Payment MS Response ---:{paymentMSResult} \n \t Basket GQL Response -- : {afterInsuranceBasketResponse} ");
            }
        }

        public async Task HandleOrderCreate(E2EScenario scenario, string basketId, string b2cToken, string fcMembershipNumber, JToken afterInsuranceBasketResponse, string selectedPoints)
        {
            string preOrderPassengerDiagnostic = BuildPreOrderPassengerDiagnostic(scenario, basketId, afterInsuranceBasketResponse);
            _output?.WriteLine(preOrderPassengerDiagnostic);

            var orderCreateGQLRequest = GraphQLRequestBuilder.OrderCreateGQLRequestBuild(basketId);
            HttpResponseMessage orderCreateGQLResponse;

            if (b2cToken != null && (scenario.BookingType != BookingType.Revenue || scenario.IsCoBrand))
                orderCreateGQLResponse = GraphQLEndpoint.Book(orderCreateGQLRequest, b2cToken ?? _testConfig.Internal.Bearer);
            else
                orderCreateGQLResponse = GraphQLEndpoint.Book(orderCreateGQLRequest);

            if (scenario.ExpectFraudCheck)
            {
                Assert.True(GraphQLAssertion.CheckFraudCheck(orderCreateGQLResponse, out string fraudCheck), fraudCheck);

                // Force create after fraud check
                orderCreateGQLRequest = GraphQLRequestBuilder.OrderCreateGQLRequestBuild(basketId, true);
                orderCreateGQLResponse = GraphQLEndpoint.Book(orderCreateGQLRequest);
            }

            if (orderCreateGQLResponse.ReadResponseAsString().HasAny(AcceptedErrorsArray.Get("PnrSellError")))
                return;

            if (!scenario.IsMultiCity || scenario.ExpectFraudCheck)
            {
                try
                {
                    orderCreateGQLResponse.EnsureGQLResponseSuccess();
                }
                catch (Exception exception) when (TryBuildDeltaFailureBucket(orderCreateGQLResponse, basketId, out string deltaBucketDiagnostic))
                {
                    throw new Exception($"{preOrderPassengerDiagnostic}{System.Environment.NewLine}{deltaBucketDiagnostic}", exception);
                }
            }

            var orderCreateGQLResult = JsonConvert.DeserializeObject<dynamic>(await orderCreateGQLResponse.Content.ReadAsStringAsync());
            ValidateExpectedLegOperatingAirline(scenario, orderCreateGQLResult, "order create");

            if (scenario.BookingType == BookingType.Redemption && !scenario.IsCoBrand)
                Assert.True(GraphQLAssertion.ValidateConfirmationGraphqlResponse(orderCreateGQLRequest, orderCreateGQLResult, out string response, fcMembershipNumber), $"Order Validation Failed:  {response}");
            else if (scenario.IsCoBrand && scenario.BookingType == BookingType.PayWithPoints)
                Assert.True(GraphQLAssertion.ValidateConfirmationGraphqlResponse(orderCreateGQLRequest, orderCreateGQLResult, out string response, fcMembershipNumber), $"Order Validation Failed:  {response}");
            else
                Assert.True(GraphQLAssertion.ValidateConfirmationGraphqlResponse(orderCreateGQLRequest, orderCreateGQLResult, out string response), response);

            // Order Create has returned a valid confirmation. Capture the booking reference now,
            // before optional post-order basket/document checks can fail on downstream services.
            GraphQLHelper.ConfirmationInfo(orderCreateGQLResponse, _testConfig);
            WritePnrDetailForReporting(scenario);

            if (scenario.BookingType == BookingType.Redemption && !scenario.IsCoBrand && scenario.ValidateRedemptionBasket)
            {
                var softAssertOrder = new SoftAssertHelper();
                softAssertOrder.True(GraphQLAssertion.CheckPnrCreated(orderCreateGQLResponse, out string errorMessage), $"Order Validation: {errorMessage} \n\tRequest : \n\t{orderCreateGQLRequest} \n\tResponse : \n\t{await orderCreateGQLResponse.Content.ReadAsStringAsync()} ");

                softAssertOrder.True(GraphQLAssertion.ValidateOrderConfirmationTotalPrice((JToken)orderCreateGQLResult, afterInsuranceBasketResponse, out errorMessage), $"Order Validation: {errorMessage} \n\tOrder Response : \n\t{await orderCreateGQLResponse.Content.ReadAsStringAsync()} ");
                softAssertOrder.True(GraphQLAssertion.ValidateOrderConfirmationMembershipPoints((JToken)orderCreateGQLResult, afterInsuranceBasketResponse, out string message), $"Validation failed:  {message}");

                string pnrValue = _testConfig.Book.Order.PNR;
                softAssertOrder.True(GraphQLAssertion.ValidatePnrOrderDocument(pnrValue, out string info), "Order Validation:  {info}");
                softAssertOrder.AssertAll();
            }
            else
            {
                Assert.True(GraphQLAssertion.CheckPnrCreated(orderCreateGQLResponse, out string errorMessage), $"{errorMessage} \n\tRequest : \n\t{orderCreateGQLRequest} \n\tResponse : \n\t{await orderCreateGQLResponse.Content.ReadAsStringAsync()} ");

            }

            // Preserve the booking reference and lead-passenger details before reporting a
            // voucher contract mismatch. Order Create may have succeeded even when the final
            // pre-order basket and confirmation expose inconsistent voucher state.
            if (scenario.ValidateVoucherConfirmation)
            {
                Assert.True(
                    GraphQLAssertion.ValidateVoucherConfirmationConsistency(
                        (JToken)orderCreateGQLResult,
                        afterInsuranceBasketResponse,
                        scenario.VoucherRedemptionType,
                        scenario.ExpectedAppliedVoucherCount,
                        out string voucherConfirmationMessage),
                    $"{scenario.VoucherRedemptionType} voucher confirmation did not match the final settled basket. {voucherConfirmationMessage}");
            }
        }

        private void WritePnrDetailForReporting(E2EScenario scenario)
        {
            string pnr = _testConfig.Book.Order.PNR;
            string lastName = _testConfig.Book.Order.LeadPassengerLastName;

            if (string.IsNullOrWhiteSpace(pnr))
                return;

            _output?.WriteLine($"Book PNR Detail: Scenario={scenario.TestName} | PNR={pnr} | Last Name={lastName}");
        }

        private static bool TryBuildDeltaFailureBucket(HttpResponseMessage orderCreateGQLResponse, string basketId, out string diagnostic)
        {
            diagnostic = string.Empty;
            string responseBody = orderCreateGQLResponse.ReadResponseAsString();

            if (responseBody.IsNullOrWhiteSpace())
            {
                return false;
            }

            JObject root;
            try
            {
                root = JObject.Parse(responseBody);
            }
            catch
            {
                return false;
            }

            JToken errorsToken = root.SelectToken("errors");
            if (errorsToken is null || errorsToken.Type != JTokenType.Array)
            {
                return false;
            }

            foreach (JToken errorToken in errorsToken.Children())
            {
                JToken extensionsToken = errorToken["extensions"];
                string outerMessage = errorToken["message"]?.ToString() ?? string.Empty;
                string outerReason = extensionsToken?["reason"]?.ToString() ?? string.Empty;
                string nestedResponse = extensionsToken?["response"]?.ToString() ?? string.Empty;
                string combined = $"{outerMessage}\n{outerReason}\n{nestedResponse}";

                if (!combined.Contains("DeltaServices", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string normalizedCombined = NormalizeEscapedJson(combined);
                string errorSource = ExtractFirstMatch(normalizedCombined, "\"errorSource\"\\s*:\\s*\"(?<value>[^\"]+)\"");
                string transactionId = ExtractFirstMatch(normalizedCombined, "transactionId:(?<value>[0-9a-fA-F-]{36})");
                string rorCode = ExtractFirstMatch(normalizedCombined, "\"code\"\\s*:\\s*\"(?<value>ROR[0-9]{4})\"");
                string downstreamCode = ExtractFirstMatch(normalizedCombined, "\"code\"\\s*:\\s*\"(?<value>[0-9]{4})\"");

                string failureKind = combined.Contains("paymentengine", StringComparison.OrdinalIgnoreCase)
                    ? "PaymentEngine"
                    : combined.Contains("INVLD ZIP CDE", StringComparison.OrdinalIgnoreCase)
                        ? "BillingAddress"
                        : "Downstream";

                string bucketSource = errorSource.IsNullOrWhiteSpace() ? "DeltaServices" : errorSource;
                string bucketCode = !downstreamCode.IsNullOrWhiteSpace()
                    ? downstreamCode
                    : !rorCode.IsNullOrWhiteSpace()
                        ? rorCode
                        : "UnknownCode";
                string bucket = $"{bucketSource}.{failureKind}.{bucketCode}";
                string requestBody = orderCreateGQLResponse.RequestMessage?.Content is null
                    ? string.Empty
                    : orderCreateGQLResponse.ReadRequestAsString();

                diagnostic =
                    $"Failure bucket: {bucket}. " +
                    $"This is a Delta downstream failure for reporting. " +
                    $"BasketId={basketId}, TransactionId={transactionId}, Source={bucketSource}, RorCode={rorCode}, DownstreamCode={downstreamCode}.{System.Environment.NewLine}" +
                    $"Request: {requestBody}{System.Environment.NewLine}" +
                    $"Response: {responseBody}";
                return true;
            }

            return false;
        }

        private static string NormalizeEscapedJson(string source)
        {
            string normalized = source;

            for (int index = 0; index < 4; index++)
            {
                string next = normalized.Replace("\\\"", "\"").Replace("\\\\", "\\");
                if (next == normalized)
                {
                    return normalized;
                }

                normalized = next;
            }

            return normalized;
        }

        private static string ExtractFirstMatch(string source, string pattern)
        {
            if (source.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            Match match = Regex.Match(source, pattern, RegexOptions.IgnoreCase);
            return match.Success ? match.Groups["value"].Value : string.Empty;
        }

        public async Task PostOrderValidations(E2EScenario scenario, string basketId, string b2cToken, string fcMembershipNumber, JToken afterInsuranceBasketResponse)
        {
            string pnr = _testConfig.Book.Order.PNR;
            bool isCoreRegressionScenario = IsCoreRegressionScenario(scenario);
            bool shouldValidateOrderDocuments = !isCoreRegressionScenario;

            // Cosmos-backed order document validation is intentionally skipped for core regression scenarios.
            if (shouldValidateOrderDocuments)
            {
                if (!scenario.UseExplicitSearchOverload || scenario.IsCoBrand || !scenario.ValidateRedemptionBasket)
                    Assert.True(GraphQLAssertion.ValidatePnrOrderDocument(pnr, out string info), $"Order Validation:  {info}");

                if (scenario.ValidateInsuranceInOrderDoc)
                    Assert.True(GraphQLAssertion.ValidatePnrInsuranceOrderDocument(_testConfig, out string info2), $"Order Validation:  {info2}");

                if (scenario.ValidateRejectedInsuranceInOrderDoc)
                    Assert.True(GraphQLAssertion.ValidatePnrRejectedInsuranceOrderDocument(_testConfig, out string info3), $"Order Validation:  {info3}");
            }

            // Core regression runs do not need PNR persistence, and their reduced config may omit storage-only fields.
            if (scenario.WriteToAzure && !isCoreRegressionScenario)
                await WriteToAzureStorage.AddPNRToAzureStorageTable(_testConfig);

            string selectedSharedService = System.Environment.GetEnvironmentVariable("SHARED_SERVICE_SIGNOFF_SELECTION") ?? string.Empty;
            bool shouldValidateChildMealRemarks = scenario.ValidateChildMealRemarks &&
                (selectedSharedService.IsNullOrWhiteSpace() ||
                 selectedSharedService.Is("all") ||
                 selectedSharedService.Is("Retail.OrderMgmtMS"));

            if (shouldValidateChildMealRemarks)
                await ValidateChildMealRemarks(scenario, pnr);

            // Loyalty points update
            if (fcMembershipNumber != null && !string.IsNullOrEmpty(fcMembershipNumber) && (scenario.BookingType == BookingType.Redemption || scenario.BookingType == BookingType.PayWithPoints || scenario.IsCoBrand))
            {
                if (scenario.UseRetryHelperForLoyalty)
                {
                    await RetryHelper.RunWithRetryAsync(async () =>
                    {
                        System.Net.Http.HttpResponseMessage vRedResponse = MicroserviceEndpoint.Vred(GetRequiredVRedToken("Book E2E post-order retry VRed balance lookup"), fcMembershipNumber).EnsureIsSuccessStatusCode();
                        dynamic vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());
                        int points = (int)vRedResult.balance.usable.Value;
                        await  LoyaltyAccountUpdates.UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, points);
                    }, 3);
                }
                else
                {
                    System.Net.Http.HttpResponseMessage vRedResponse = MicroserviceEndpoint.Vred(GetRequiredVRedToken("Book E2E post-order VRed balance lookup"), fcMembershipNumber).EnsureIsSuccessStatusCode();
                    dynamic vRedResult = JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());
                    int points = (int)vRedResult.balance.usable.Value;
                    await  LoyaltyAccountUpdates.UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, points);
                }
            }
        }

        private async Task ValidateChildMealRemarks(E2EScenario scenario, string pnr)
        {
            Assert.True(scenario.Children > 0, "Child-meal remark validation requires at least one child passenger.");
            Assert.False(pnr.IsNullOrWhiteSpace(), "Child-meal remark validation requires a created PNR.");

            int expectedChildMeals = scenario.Children * (scenario.JourneyType == JourneyType.Return ? 2 : 1);
            int actualChildMeals = 0;
            Exception lastException = null;
            const int maxAttempts = 7;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    actualChildMeals = RetrievePnrDetails.ReturnAllocatedChildMealCount(pnr);
                    if (actualChildMeals == expectedChildMeals)
                        return;

                    lastException = null;
                }
                catch (Exception exception)
                {
                    lastException = exception;
                }

                if (attempt < maxAttempts)
                {
                    _output?.WriteLine($"Waiting for OrderMgmt child-meal event processing. PNR={pnr} Attempt={attempt}/{maxAttempts} ExpectedCHML={expectedChildMeals} ActualCHML={actualChildMeals} Error={lastException?.Message ?? "none"}");
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }

            Assert.True(
                actualChildMeals == expectedChildMeals,
                $"OrderMgmt child-meal event validation failed. Expected {expectedChildMeals} allocated CHML/HK remarks but found {actualChildMeals}. PNR={pnr}. Last error: {lastException?.Message ?? "none"}");
        }

        private static bool IsCoreRegressionScenario(E2EScenario scenario)
        {
            if (scenario?.Categories == null)
            {
                return false;
            }

            foreach (string category in scenario.Categories)
            {
                if (category.Is(Traits.BookCoreRegression))
                {
                    return true;
                }
            }

            return false;
        }

        private static async Task<string> GenerateLoginB2CTokenFromApi(string fcMembershipNumber, Dictionary<string, string> loyaltyDetails)
        {
            Assert.True(!fcMembershipNumber.IsNullOrWhiteSpace(), "FC membership number is required to generate a B2C token.");

            string email = ResolveLoyaltyEmail(loyaltyDetails);
            string password = ConfigReader.Get("LoyaltyAccountPassword");
            Assert.False(password.IsNullOrWhiteSpace(), "LoyaltyAccountPassword is required for Book GraphQL loyalty sign-in.");
            HttpResponseMessage gqlResponse = MyAccountGraphQL.AccountMemberSignIn(Config.Environment, email, fcMembershipNumber, password).EnsureGQLResponseSuccess();
            string responseBody = await gqlResponse.Content.ReadAsStringAsync();
            JObject signInResponse = JObject.Parse(responseBody);
            string accessToken = signInResponse.SelectToken("data.accountMemberSignIn.accessToken")?.ToString();

            Assert.True(
                !accessToken.IsNullOrWhiteSpace(),
                $"B2C token is null from API sign-in. Environment={Config.Environment}, FcMembershipNumberSupplied={!fcMembershipNumber.IsNullOrWhiteSpace()}, EmailSupplied={!email.IsNullOrWhiteSpace()}, GraphQLErrors={BuildGraphQLErrorSummary(signInResponse)}");

            return accessToken;
        }

        private static string ResolveLoyaltyEmail(Dictionary<string, string> loyaltyDetails)
        {
            if (loyaltyDetails != null && loyaltyDetails.TryGetValue("email", out string email) && !email.IsNullOrWhiteSpace())
            {
                return email;
            }

            return string.Empty;
        }

        private static string BuildAllSectorsSeatSelectionDiagnostic(E2EScenario scenario, string basketId, string originalSectorId, string firstSectorId, CabinName cabinName, HttpResponseMessage retrieveBasketGQLResponse)
        {
            string retrieveBasketBody = retrieveBasketGQLResponse.ReadResponseAsString();

            return "All-sector seat selection failed."
                + $"{System.Environment.NewLine}Test={scenario.TestName}"
                + $"{System.Environment.NewLine}BasketId={basketId}"
                + $"{System.Environment.NewLine}JourneyType={scenario.JourneyType}"
                + $"{System.Environment.NewLine}Route={scenario.Origin}-{scenario.Destination}"
                + $"{System.Environment.NewLine}FareFamily={scenario.FareFamily}"
                + $"{System.Environment.NewLine}FlightType={scenario.FlightType}"
                + $"{System.Environment.NewLine}CabinName={cabinName}"
                + $"{System.Environment.NewLine}SeatProductName={scenario.SeatProductName}"
                + $"{System.Environment.NewLine}ProvidedSectorId={originalSectorId}"
                + $"{System.Environment.NewLine}ResolvedFirstSectorId={firstSectorId}"
                + $"{System.Environment.NewLine}Pax(A/T/C/I)={scenario.Adults}/{scenario.Teens}/{scenario.Children}/{scenario.Infants}"
                + $"{System.Environment.NewLine}RetrieveBasketResponse={TruncateDiagnosticPayload(retrieveBasketBody)}";
        }

        private static string BuildPreOrderPassengerDiagnostic(E2EScenario scenario, string basketId, JToken basketResponse)
        {
            JToken peopleToken = basketResponse?.SelectToken("data.retrieveBasket.people");
            JToken summaryToken = basketResponse?.SelectToken("data.retrieveBasket.summary");
            List<string> passengerTypes = new List<string>();
            List<string> passengerDetails = new List<string>();

            if (peopleToken != null)
            {
                foreach (JToken passengerToken in peopleToken.Children())
                {
                    string passengerType = passengerToken.SelectToken("type")?.ToString() ?? "UNKNOWN";
                    string passengerId = passengerToken.SelectToken("id")?.ToString() ?? "UNKNOWN";
                    string passengerValidation = passengerToken.SelectToken("validation")?.ToString() ?? "UNKNOWN";
                    string profilePassenger = passengerToken.SelectToken("profilePassenger")?.ToString() ?? "null";

                    passengerTypes.Add(passengerType);
                    passengerDetails.Add($"Id={passengerId}:Type={passengerType}:Validation={passengerValidation}:Profile={profilePassenger}");
                }
            }

            string summaryCounts = "Summary(A/T/C/I)="
                + $"{summaryToken?.SelectToken("numberOfAdults")?.ToString() ?? "n/a"}/"
                + $"{summaryToken?.SelectToken("numberOfTeens")?.ToString() ?? "n/a"}/"
                + $"{summaryToken?.SelectToken("numberOfChildren")?.ToString() ?? "n/a"}/"
                + $"{summaryToken?.SelectToken("numberOfInfants")?.ToString() ?? "n/a"}";

            return "Pre-order passenger diagnostic"
                + $"{System.Environment.NewLine}Test={scenario.TestName}"
                + $"{System.Environment.NewLine}BasketId={basketId}"
                + $"{System.Environment.NewLine}ScenarioPax(A/T/C/I)={scenario.Adults}/{scenario.Teens}/{scenario.Children}/{scenario.Infants}"
                + $"{System.Environment.NewLine}{summaryCounts}"
                + $"{System.Environment.NewLine}BasketPeopleCount={passengerTypes.Count}"
                + $"{System.Environment.NewLine}BasketTypeCounts={BuildPassengerTypeCounts(passengerTypes)}"
                + $"{System.Environment.NewLine}BasketPassengers={string.Join(", ", passengerDetails)}";
        }

        private static string BuildPassengerTypeCounts(List<string> passengerTypes)
        {
            if (passengerTypes.Count == 0)
            {
                return "none";
            }

            Dictionary<string, int> passengerTypeCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (string passengerType in passengerTypes)
            {
                if (passengerTypeCounts.ContainsKey(passengerType))
                {
                    passengerTypeCounts[passengerType]++;
                }
                else
                {
                    passengerTypeCounts.Add(passengerType, 1);
                }
            }

            List<string> typeCountSummaries = passengerTypeCounts
                .OrderBy(passengerTypeCount => passengerTypeCount.Key)
                .Select(passengerTypeCount => $"{passengerTypeCount.Key}={passengerTypeCount.Value}")
                .ToList();

            return string.Join(", ", typeCountSummaries);
        }

        private static string BuildGraphQLErrorSummary(JObject response)
        {
            List<string> errorSummaries = response
                .SelectTokens("$.errors[*]")
                .Select(error =>
                {
                    string code = error.SelectToken("extensions.code")?.ToString();
                    string message = error.SelectToken("message")?.ToString();

                    if (!code.IsNullOrWhiteSpace() && !message.IsNullOrWhiteSpace())
                    {
                        return $"{code}: {message}";
                    }

                    return message.IsNullOrWhiteSpace() ? code : message;
                })
                .Where(summary => !summary.IsNullOrWhiteSpace())
                .ToList();

            if (errorSummaries.Count == 0)
            {
                return "No GraphQL errors returned.";
            }

            return TruncateDiagnosticPayload(string.Join("; ", errorSummaries));
        }

        private static string TruncateDiagnosticPayload(string payload)
        {
            const int maxDiagnosticLength = 4000;

            if (payload.IsNullOrWhiteSpace() || payload.Length <= maxDiagnosticLength)
            {
                return payload;
            }

            return payload[..maxDiagnosticLength] + "...";
        }
    }
}

