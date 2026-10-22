using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL.Seats
{
    [Collection("SeatSharedServicesSignOff")]
    public class PaidSeatsUpgradeCabinTests : SeatTestsBase
    {
        public PaidSeatsUpgradeCabinTests(TokenFixture tokenFixture) : base(tokenFixture)
        {
        }

        [SkippableFact(DisplayName = "SharedServicesSignOff_SelectPaidSeat_UpgradeCabin")]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "Retail.PaymentMS")]
        [Trait("SharedService", "SeatMS")]
        [Trait("SharedServiceConsumer", "Trips")]
        public async Task SharedServicesSignOff_SelectPaidSeat_UpgradeCabin() =>
            await ExecutePaidSeatsUpgradeCabinSuccessScenario(FlightType.VS, CabinType.EconomyClassic, Currency.GBP, 1, "LHR", "JFK");

        [SkippableTheory(DisplayName = "SelectPaidSeat_UpgradeCabin_ReturnsExpectedResult_GBP")]
        [Trait("Category", Traits.PaidSeatsUpgradeCabinRegression)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task SelectPaidSeat_UpgradeCabin_ReturnsExpectedResult_Gbp(int runNumber)
        {
            await ExecutePaidSeatsUpgradeCabinSuccessScenario(FlightType.VS, CabinType.EconomyClassic, Currency.GBP, runNumber);
        }

        [SkippableTheory(DisplayName = "SelectPaidSeat_UpgradeCabin_ReturnsExpectedResult_USD")]
        [Trait("Category", Traits.PaidSeatsUpgradeCabinRegression)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task SelectPaidSeat_UpgradeCabin_ReturnsExpectedResult_Usd(int runNumber)
        {
            await ExecutePaidSeatsUpgradeCabinSuccessScenario(FlightType.VS, CabinType.EconomyClassic, Currency.USD, runNumber);
        }

        private async Task ExecutePaidSeatsUpgradeCabinSuccessScenario(FlightType flightType, CabinType cabinType, Currency currency, int runNumber, string origin = null, string destination = null)
        {
            #region - pre-requisite: fetch PNR candidates booked in EconomyClassic (VS)

            string flightTypeString = flightType.ToString();
            string cabinTypeString = cabinType.ToString();
            string currencyString = currency.ToString();
            string passengersCount = "1";

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForSeatScenarios(flightType: flightTypeString, cabinType: cabinTypeString, currency: currencyString, passengersCount: passengersCount, origin: origin, destination: destination);

            ValidatePnrDetails(pnrDetails);

            // Build candidate list — fall back to the single record if allCandidatePnrs is not present
            string[] candidatePnrs = pnrDetails.TryGetValue("allCandidatePnrs", out string rawPnrs) && !string.IsNullOrEmpty(rawPnrs)
                ? rawPnrs.Split(',')
                : new string[] { pnrDetails["pnr"] };

            string[] candidateFirstNames = pnrDetails.TryGetValue("allCandidateFirstNames", out string rawFirstNames) && !string.IsNullOrEmpty(rawFirstNames)
                ? rawFirstNames.Split(new string[] { "||" }, StringSplitOptions.None)
                : new string[] { pnrDetails["firstName"] };

            string[] candidateLastNames = pnrDetails.TryGetValue("allCandidateLastNames", out string rawLastNames) && !string.IsNullOrEmpty(rawLastNames)
                ? rawLastNames.Split(new string[] { "||" }, StringSplitOptions.None)
                : new string[] { pnrDetails["lastName"] };

            #endregion

            #region - iterate candidates to find one whose Booking response has cabinClass.productCode == VSCL

            string bookingId = null;
            string pnr = null;
            string firstName = null;
            string lastName = null;
            string debugContext = null;
            dynamic bookingResult = null;
            List<string> passengerIds = new();

            const int MaxCandidatesToTry = 10;

            for (int c = 0; c < candidatePnrs.Length && c < MaxCandidatesToTry; c++)

            {
                string candidatePnr = candidatePnrs[c]?.Trim() ?? string.Empty;
                string candidateFirstName = c < candidateFirstNames.Length ? candidateFirstNames[c]?.Trim() ?? string.Empty : string.Empty;
                string candidateLastName = c < candidateLastNames.Length ? candidateLastNames[c]?.Trim() ?? string.Empty : string.Empty;

                if (string.IsNullOrEmpty(candidatePnr) || string.IsNullOrEmpty(candidateFirstName) || string.IsNullOrEmpty(candidateLastName))
                    continue;

                HttpResponseMessage candidateResponse = GraphQlEndpoint.Booking(env, "PNR", candidatePnr, candidateFirstName, candidateLastName);

                if (!candidateResponse.IsSuccessStatusCode)

                    continue;

                dynamic candidateResult = JsonConvert.DeserializeObject<dynamic>(await candidateResponse.Content.ReadAsStringAsync());

                if (DynamicHelper.HasProperty(candidateResult, "errors") || !DynamicHelper.HasProperty(candidateResult, "data"))
                    continue;

                if (!DynamicHelper.HasProperty(candidateResult.data, "Booking"))
                    continue;

                // Check that at least one leg under product[].slices[].segments[].legs[] has cabinClass.productCode == VSCL
                bool hasVsclCabin = false;
                dynamic candidateProducts = candidateResult.data.Booking.product;
                if (candidateProducts == null || !candidateProducts.HasValues)
                    continue;

                foreach (dynamic product in candidateProducts)
                {
                    if (!DynamicHelper.HasProperty(product, "slices") || product.slices == null)
                        continue;
                    foreach (dynamic slice in product.slices)
                    {
                        if (!DynamicHelper.HasProperty(slice, "segments") || slice.segments == null)
                            continue;
                        foreach (dynamic seg in slice.segments)
                        {
                            if (!DynamicHelper.HasProperty(seg, "legs") || seg.legs == null)
                                continue;
                            foreach (dynamic leg in seg.legs)
                            {
                                if (DynamicHelper.HasProperty(leg, "cabinClass") && leg.cabinClass != null &&
                                    DynamicHelper.HasProperty(leg.cabinClass, "productCode") &&
                                    (string)leg.cabinClass.productCode == "VSCL")
                                {
                                    hasVsclCabin = true;
                                    break;
                                }
                            }
                            if (hasVsclCabin)
                                break;
                        }
                        if (hasVsclCabin)
                            break;
                    }
                    if (hasVsclCabin)
                        break;
                }

                if (!hasVsclCabin)
                    continue;

                // Found a valid candidate — capture everything needed for the rest of the test
                pnr = candidatePnr;
                firstName = candidateFirstName;
                lastName = candidateLastName;
                debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);
                bookingResult = candidateResult;
                bookingId = (string)candidateResult.data.Booking.id;

                dynamic candidatePassengerList = candidateResult.data.Booking.passengers;
                if (candidatePassengerList != null && candidatePassengerList.HasValues)
                {
                    foreach (dynamic pax in candidatePassengerList)
                    {
                        string passengerId = pax.passengerId;
                        if (!string.IsNullOrEmpty(passengerId))
                            passengerIds.Add(passengerId);
                    }
                }
                break;
            }

            if (string.IsNullOrEmpty(bookingId))
            {
                SkipTest($"Test skipped: No candidate PNR found with cabinClass.productCode == VSCL for flightType={flightTypeString}, cabinType={cabinTypeString}, currency={currencyString}, runNumber={runNumber}");
                return;
            }

            Assert.True(!DynamicHelper.HasProperty(bookingResult, "errors"), $"Expected a valid response from Graphql but found \n {bookingResult}. {debugContext}");
            Assert.True(passengerIds.Count > 0, $"Response does not contain 'passengers'. {debugContext}");

            #endregion

            #region - GraphQL call for seat map for segment 1

            string segmentId = "1";

            HttpResponseMessage getSeatMapResponse = GraphQlEndpoint.GetSeatMap(env, bookingId, segmentId).EnsureIsSuccessStatusCode();
            dynamic getSeatMapResult = JsonConvert.DeserializeObject<dynamic>(await getSeatMapResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(getSeatMapResult, "errors"), $"Expected a valid response from Graphql for segment {segmentId} but found \n {getSeatMapResult}. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(getSeatMapResult, "data"), $"Response for segment {segmentId} does not contain data property. {debugContext}");

            dynamic? dataObj = getSeatMapResult.data;
            Assert.True(dataObj != null, $"Data property should not be null for segment {segmentId}. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(dataObj, "seatMapQuery"), $"Response for segment {segmentId} does not contain seatMapQuery property. {debugContext}");

            dynamic? seatMapQuery = dataObj.seatMapQuery;
            Assert.True(seatMapQuery != null, $"SeatMapQuery property should not be null for segment {segmentId}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(seatMapQuery, "flightPassengerSeatMap"), $"Response for segment {segmentId} does not contain flightPassengerSeatMap property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(seatMapQuery, "seatMap"), $"Response for segment {segmentId} does not contain seatMap property. {debugContext}");

            dynamic? flightPassengerSeatMap = seatMapQuery.flightPassengerSeatMap;
            Assert.True(flightPassengerSeatMap != null && flightPassengerSeatMap.HasValues, $"flightPassengerSeatMap should not be null or empty for segment {segmentId}. {debugContext}");

            dynamic? seatMap = seatMapQuery.seatMap;
            Assert.True(seatMap != null && seatMap.HasValues, $"seatMap should not be null or empty for segment {segmentId}. {debugContext}");

            foreach (dynamic passengerSeat in flightPassengerSeatMap)
            {
                Assert.True(DynamicHelper.HasProperty(passengerSeat, "passengerId"), $"Passenger seat should contain passengerId. {debugContext}");
                Assert.True(DynamicHelper.HasProperty(passengerSeat, "segmentId"), $"Passenger seat should contain segmentId. {debugContext}");
                Assert.True(DynamicHelper.HasProperty(passengerSeat, "firstName"), $"Passenger seat should contain firstName. {debugContext}");
                Assert.True(DynamicHelper.HasProperty(passengerSeat, "lastName"), $"Passenger seat should contain lastName. {debugContext}");
                Assert.True(DynamicHelper.HasProperty(passengerSeat, "seatNumber"), $"Passenger seat should contain seatNumber. {debugContext}");
                Assert.True(DynamicHelper.HasProperty(passengerSeat, "seatOfferId"), $"Passenger seat should contain seatOfferId. {debugContext}");

                string passengerIdFromSeat = passengerSeat.passengerId;
                Assert.True(passengerIds.Contains(passengerIdFromSeat), $"PassengerId {passengerIdFromSeat} from seat map should match booking passengers. {debugContext}");

                string seatOfferIdFromSeat = passengerSeat.seatOfferId;
                Assert.True(!string.IsNullOrEmpty(seatOfferIdFromSeat), $"seatOfferId should not be null or empty. {debugContext}");
            }

            string firstPassengerId = passengerIds.First();

            dynamic? firstPassengerSeat = null;
            foreach (dynamic passengerSeat in flightPassengerSeatMap)
            {
                if (string.Equals((string)passengerSeat.passengerId, firstPassengerId, StringComparison.Ordinal))
                {
                    firstPassengerSeat = passengerSeat;
                    break;
                }
            }

            Assert.True(firstPassengerSeat != null, $"SeatMap response does not contain a passenger seat entry for passengerId {firstPassengerId}. {debugContext}");

            string paxSeatOfferId = firstPassengerSeat.seatOfferId;
            string preAllocatedSeatNumber = firstPassengerSeat.seatNumber?.ToString() ?? string.Empty;

            // Search for a paid seat in any available upgrade cabin - Upper Class (VSUP) preferred, fallback to Premium Economy (VSPE)
            string[] upgradeCabinClassTypes = new string[] { "UPPER CLASS", "PREMIUM ECONOMY" };
            string[] upgradeFareProducts = new string[] { "VSUP", "VSPE" };
            Assert.Equal(upgradeCabinClassTypes.Length, upgradeFareProducts.Length);

            string seatNumber = null;
            decimal paidSeatAmount = 0;
            string currencyFromSeatmapMutation = null;
            string fareProductNameFromSeatMap = null;
            string resolvedCabinClassType = null;

            for (int i = 0; i < upgradeCabinClassTypes.Length; i++)
            {
                string targetClassType = upgradeCabinClassTypes[i];
                string targetFareProduct = upgradeFareProducts[i];

                foreach (dynamic cabin in seatMap)
                {
                    if (((string)cabin.classType).ToUpper() == targetClassType)
                    {
                        foreach (dynamic row in cabin.rows)
                        {
                            foreach (dynamic column in row.columns)
                            {
                                if ((string)column.type == "SEAT" && (string)column.availability == "FREE" && column.offers != null && column.offers.HasValues)
                                {
                                    if (!string.IsNullOrEmpty(preAllocatedSeatNumber) && string.Equals((string)column.seatNumber, preAllocatedSeatNumber, StringComparison.OrdinalIgnoreCase))
                                        continue;

                                    foreach (dynamic offer in column.offers)
                                    {
                                        if (DynamicHelper.HasProperty(offer, "price") && offer.price != null && DynamicHelper.HasProperty(offer, "fareProduct") && offer.fareProduct == targetFareProduct && DynamicHelper.HasProperty(offer.price, "amount") && DynamicHelper.HasProperty(offer.price, "currency") && offer.price.amount != null)
                                        {
                                            decimal amount = (decimal)offer.price.amount;
                                            if (amount > 0)
                                            {
                                                seatNumber = (string)column.seatNumber;
                                                paidSeatAmount = amount;
                                                currencyFromSeatmapMutation = (string)offer.price.currency;
                                                fareProductNameFromSeatMap = (string)offer.fareProductName;
                                                resolvedCabinClassType = targetClassType;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (seatNumber != null)
                                    break;
                            }
                            if (seatNumber != null)
                                break;
                        }
                    }
                    if (seatNumber != null)
                        break;
                }
                if (seatNumber != null)
                    break;
            }

            Assert.True(!string.IsNullOrEmpty(seatNumber), $"No paid seat available in Upper Class or Premium Economy cabin for {pnr}. {debugContext}");
            Assert.True(paidSeatAmount > 0, $"Seat amount should be greater than 0, found: {paidSeatAmount}. {debugContext}");
            Assert.True(!string.IsNullOrEmpty(currencyFromSeatmapMutation), $"Seat currency should not be null or empty, found: {currencyFromSeatmapMutation}. {debugContext}");

            #endregion

            #region - GraphQL call for select seat and validate SelectSeat response

            string passengerIdForSeat = firstPassengerId;

            HttpResponseMessage selectSeatResponse = GraphQlEndpoint.SelectSeat(env, bookingId, segmentId, passengerIdForSeat, seatNumber, paxSeatOfferId).EnsureIsSuccessStatusCode();
            dynamic selectSeatResult = JsonConvert.DeserializeObject<dynamic>(await selectSeatResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(selectSeatResult, "errors"), $"Expected a valid response from Graphql for select seat but found \n {selectSeatResult}. {debugContext}");

            dynamic selectSeatMutation = selectSeatResult.data.selectSeatMutation;
            Assert.NotNull(selectSeatMutation);

            Assert.True(DynamicHelper.HasProperty(selectSeatMutation, "flightPassengerSeatMap"), $"SelectSeat response does not contain flightPassengerSeatMap. {debugContext}");

            dynamic? flightPassengerSelectSeat = selectSeatMutation.flightPassengerSeatMap;
            Assert.True(flightPassengerSelectSeat != null && flightPassengerSelectSeat.HasValues, $"flightPassengerSelectSeat should not be null or empty. {debugContext}");

            dynamic? firstPassengerSelectSeat = flightPassengerSelectSeat.First;
            Assert.True(firstPassengerSelectSeat is not null, $"flightPassengerSelectSeat should not be null or empty. {debugContext}");

            string passengerIdFromResponse = firstPassengerSelectSeat.passengerId;
            Assert.True(firstPassengerId.Equals(passengerIdFromResponse), $"PassengerId {passengerIdFromResponse} does not match the passenger id {firstPassengerId} sent in the request. {debugContext}");

            string seatNumberFromResponse = firstPassengerSelectSeat.seatNumber;
            Assert.True(seatNumber.Equals(seatNumberFromResponse), $"Seat number {seatNumberFromResponse} does not match the seat number {seatNumber} sent in the request. {debugContext}");

            string seatOfferIdFromResponse = firstPassengerSelectSeat.seatOfferId;
            Assert.True(paxSeatOfferId.Equals(seatOfferIdFromResponse), $"Seat offer id {seatOfferIdFromResponse} does not match the seat offer id {paxSeatOfferId} sent in the request. {debugContext}");

            string segmentIdFromResponse = firstPassengerSelectSeat.segmentId;
            Assert.True(segmentId.Equals(segmentIdFromResponse), $"Segment id {segmentIdFromResponse} does not match the segment id {segmentId} sent in the request. {debugContext}");
            Assert.False(string.Equals(preAllocatedSeatNumber, seatNumber, StringComparison.OrdinalIgnoreCase), $"Selected seat {seatNumber} is already pre-allocated for passengerId {firstPassengerId}; expected a different seat. {debugContext}");

            #endregion

            #region - GraphQL call for seat payment mutation and validate seat payment response

            HttpResponseMessage seatPaymentMutationResponse = GraphQlEndpoint.SeatPayment(env, bookingId).EnsureIsSuccessStatusCode();
            dynamic seatPaymentMutationResult = JsonConvert.DeserializeObject<dynamic>(await seatPaymentMutationResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(seatPaymentMutationResult, "errors") || seatPaymentMutationResult.errors == null, $"Expected a valid response from Graphql for seat payment but found {seatPaymentMutationResult}. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(seatPaymentMutationResult, "data"), $"SeatPayment response should contain data property. {debugContext}");

            dynamic? seatPaymentMutation = seatPaymentMutationResult.data.seatPaymentMutation;
            Assert.True(seatPaymentMutation is not null, $"seatPaymentMutation should not be null. {debugContext}");

            string paymentReferenceId = seatPaymentMutation.paymentReferenceId;
            Assert.True(!string.IsNullOrEmpty(paymentReferenceId), $"paymentReferenceId should not be null or empty. {debugContext}");

            string hppUrl = seatPaymentMutation.hppLink;
            Assert.True(!string.IsNullOrEmpty(hppUrl), $"hppUrl should not be null or empty. {debugContext}");

            decimal monetaryAmount = seatPaymentMutation.amount.monetaryAmount;
            Assert.True(paidSeatAmount == monetaryAmount, $"paid amount {paidSeatAmount} should match with the seat payment response amount {monetaryAmount}. {debugContext}");

            string currencyFromSeatpaymentMutationResponse = seatPaymentMutation.amount.baseCurrency;
            Assert.True(currencyFromSeatmapMutation.Equals(currencyFromSeatpaymentMutationResponse), $"currencyFromSeatmapMutation {currencyFromSeatmapMutation} should match with the response currencyFromSeatpaymentMutationResponse {currencyFromSeatpaymentMutationResponse}. {debugContext}");

            #endregion

            #region - Chrome browser call to open the HPP link and make the payment

            _testConfig = new TestConfig();
            _testConfig.Book.Order.HppUrl = hppUrl;
            ConfigureSeatPaymentCard(currency);
            await HostedPaymentAutomation.SubmitCardAsync(
                _testConfig,
                usePlaywright: true,
                expectedTotalPrice: (double)monetaryAmount);

            #endregion

            #region - GraphQL call for seat fulfillment mutation to confirm the selected seats

            string isPaymentStatus = "SUCCESS";
            dynamic seatFulfilmentMutationResult = null;
            string seatFulfilmentRawResponse = string.Empty;
            string seatFulfilmentErrorsSummary = string.Empty;


            await Core.Xunit.RetryHelper.RunWithRetryAsync(async () =>

            {

                HttpResponseMessage seatFulfilmentMutationResponse = GraphQlEndpoint.SeatFulfilmentMutation(env, bookingId, isPaymentStatus).EnsureIsSuccessStatusCode();

                seatFulfilmentRawResponse = await seatFulfilmentMutationResponse.Content.ReadAsStringAsync();
                seatFulfilmentMutationResult = JsonConvert.DeserializeObject<dynamic>(seatFulfilmentRawResponse);
                seatFulfilmentErrorsSummary = DynamicHelper.HasProperty(seatFulfilmentMutationResult, "errors") && seatFulfilmentMutationResult.errors != null
                    ? TruncateForAssertion(seatFulfilmentMutationResult.errors.ToString(), 1200) : "<null>";

                Assert.True(!DynamicHelper.HasProperty(seatFulfilmentMutationResult, "errors") || seatFulfilmentMutationResult.errors == null,

                    $"Seat fulfilment returned GraphQL errors. errors={seatFulfilmentErrorsSummary}; bookingId={bookingId}; pnr={pnr}; {debugContext}");

            }, maxRetries: 3, delay: 2000);

            string cardNumber = _testConfig.Book.Payment.CardDetails.CardNumber?.ToString();
            string maskedCard = MaskCardNumber(cardNumber);

            Assert.True(
                !DynamicHelper.HasProperty(seatFulfilmentMutationResult, "errors") || seatFulfilmentMutationResult.errors == null,
                $"Seat fulfilment returned GraphQL errors. errors={seatFulfilmentErrorsSummary}; cardType={_testConfig.Book.Payment.CardDetails.PaymentCardType}; card={maskedCard}; bookingId={bookingId}; pnr={pnr}; rawResponse={TruncateForAssertion(seatFulfilmentRawResponse, 1500)}; {debugContext}");

            Assert.True(DynamicHelper.HasProperty(seatFulfilmentMutationResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(seatFulfilmentMutationResult.data, "seatFulfilmentMutation"), $"Response should contain seatFulfilmentMutation property. {debugContext}");

            dynamic? seatFulfilmentMutation = seatFulfilmentMutationResult.data.seatFulfilmentMutation;
            Assert.True(seatFulfilmentMutation != null, $"seatFulfilmentMutation should not be null. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(seatFulfilmentMutation, "pnrLocator"), $"seatFulfilmentMutation should contain pnrLocator property. {debugContext}");
            string pnrLocatorFromResponse = seatFulfilmentMutation.pnrLocator;
            Assert.True(pnrLocatorFromResponse.Equals(pnr), $"expected pnr value in seatFulfilmentMutation {pnr} but found {pnrLocatorFromResponse}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(seatFulfilmentMutation, "passengerSeatSelection"), $"seatFulfilmentMutation should contain passengerSeatSelection property. {debugContext}");
            Assert.True(seatFulfilmentMutation.passengerSeatSelection != null && seatFulfilmentMutation.passengerSeatSelection.HasValues, $"passengerSeatSelection should not be null or empty. {debugContext}");

            dynamic? firstPassengerSeatSelection = seatFulfilmentMutation.passengerSeatSelection.First;
            Assert.True(firstPassengerSeatSelection != null, $"First passenger seat selection should not be null. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(firstPassengerSeatSelection, "seatNumber"), $"Passenger seat selection should contain seatNumber property. {debugContext}");
            string confirmedSeatNumber = firstPassengerSeatSelection.seatNumber;
            Assert.True(seatNumber.Equals(confirmedSeatNumber), $"seatNumber {seatNumber} does not match in the confirmation response {confirmedSeatNumber}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(firstPassengerSeatSelection, "price"), $"Passenger seat selection should contain price property. {debugContext}");
            Assert.True(firstPassengerSeatSelection.price != null, $"Price should not be null. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(firstPassengerSeatSelection.price, "amount"), $"Price should contain amount property. {debugContext}");
            decimal confirmedAmount = (decimal)firstPassengerSeatSelection.price.amount;
            Assert.True(confirmedAmount.Equals(paidSeatAmount), $"expected amount in seatFulfilmentMutation {paidSeatAmount} but found {confirmedAmount}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(firstPassengerSeatSelection.price, "currency"), $"Price should contain currency property. {debugContext}");
            string confirmedCurrency = (string)firstPassengerSeatSelection.price.currency;
            Assert.True(confirmedCurrency.Equals(currencyFromSeatmapMutation), $"expected currency in seatFulfilmentMutation {currencyFromSeatmapMutation} but found {confirmedCurrency}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(firstPassengerSeatSelection, "selectedFareProductName"), $"Passenger seat selection should contain selectedFareProductName property. {debugContext}");
            string selectedFareProductName = firstPassengerSeatSelection.selectedFareProductName;
            Assert.True(string.Equals(selectedFareProductName, fareProductNameFromSeatMap), $"expected fare product name as {fareProductNameFromSeatMap} but found {selectedFareProductName}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(firstPassengerSeatSelection, "isConfirmed"), $"Passenger seat selection should contain isConfirmed property. {debugContext}");
            bool isConfirmed = (bool)firstPassengerSeatSelection.isConfirmed;
            Assert.True(isConfirmed, $"Seat selection should be confirmed. Resolved cabin: {resolvedCabinClassType}. {debugContext}");

            #endregion
        }

        private static string TruncateForAssertion(string? value, int maxLength)
        {
            return string.IsNullOrEmpty(value) || value.Length <= maxLength ? value ?? string.Empty : value[..maxLength] + "...(truncated)";
        }
    }
}
