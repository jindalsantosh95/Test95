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
    public class PaidSeatsTests : SeatTestsBase
    {
        public PaidSeatsTests(TokenFixture tokenFixture) : base(tokenFixture)
        {
        }

        [SkippableFact(DisplayName = "SharedServicesSignOff_SelectPaidSeat")]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "Retail.PaymentMS")]
        [Trait("SharedService", "SeatMS")]
        [Trait("SharedServiceConsumer", "Trips")]
        public async Task SharedServicesSignOff_SelectPaidSeat() =>
            await ExecutePaidSeatsSuccessScenario(FlightType.VS, CabinType.EconomyClassic, Currency.GBP, 1, "LHR", "JFK");

        [SkippableTheory(DisplayName = "SelectPaidSeat_ReturnsExpectedResult_GBP")]
        [Trait("Category", Traits.SelectPaidSeatsRegression)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task SelectPaidSeat_ReturnsExpectedResult_Gbp(int runNumber)
        {
            await ExecutePaidSeatsSuccessScenario(FlightType.VS, CabinType.EconomyClassic, Currency.GBP, runNumber);
        }

        [SkippableTheory(DisplayName = "SelectPaidSeat_ReturnsExpectedResult_USD")]
        [Trait("Category", Traits.SelectPaidSeatsRegression)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task SelectPaidSeat_ReturnsExpectedResult_Usd(int runNumber)
        {
            await ExecutePaidSeatsSuccessScenario(FlightType.VS, CabinType.EconomyClassic, Currency.USD, runNumber);
        }

        private async Task ExecutePaidSeatsSuccessScenario(FlightType flightType, CabinType cabinType, Currency currency, int runNumber, string originFilter = null, string destinationFilter = null)
        {

            #region - pre-requisite for fetching the PNR from the storage table with conditon of direct flight with VS with selected seats as false.

            string flightTypeString = flightType.ToString();
            string cabinTypeString = cabinType.ToString();
            string currencyString = currency.ToString();
            string passengersCount = "1"; // Assuming we are testing with 1 passenger for simplicity

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForSeatScenarios(flightType: flightTypeString, cabinType: cabinTypeString, currency: currencyString, passengersCount: passengersCount, origin: originFilter, destination: destinationFilter);

            ValidatePnrDetails(pnrDetails);

            // Build candidate list: primary + any extras returned by the storage helper
            List<string> candidatePnrs = (pnrDetails.TryGetValue("allCandidatePnrs", out string allPnrs) && !string.IsNullOrEmpty(allPnrs))
                ? allPnrs.Split(',').ToList() : new List<string> { pnrDetails["pnr"] };
            List<string> candidateFirstNames = (pnrDetails.TryGetValue("allCandidateFirstNames", out string allFirst) && !string.IsNullOrEmpty(allFirst))
                ? allFirst.Split(new[] { "||" }, StringSplitOptions.None).ToList() : new List<string> { pnrDetails["firstName"] };
            List<string> candidateLastNames = (pnrDetails.TryGetValue("allCandidateLastNames", out string allLast) && !string.IsNullOrEmpty(allLast))
                ? allLast.Split(new[] { "||" }, StringSplitOptions.None).ToList() : new List<string> { pnrDetails["lastName"] };

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);
            string origin = pnrDetails["origin"];
            string destination = pnrDetails["destination"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && paxCount > 0, $"PNR, First Name, and Last Name should not be null or empty and Pax Count > 0. runNumber={runNumber}. {debugContext}");

            #endregion

            #region - GraphQL call for retrieve trip to retrieve booking id and passenger ids and count of slices

            string ticketNum = string.Empty; // as we are performing a PNR Search
            string usingString = "PNR";
            string[] cabinPriority = { "ECONOMY", "PREMIUM ECONOMY", "UPPER CLASS" };

            // Working variables — resolved by iterating candidates until one passes both checks
            dynamic BookingResult = null;
            dynamic seatMapQuery = null;
            dynamic flightPassengerSeatMap = null;
            List<string> passengerIds = new();
            string bookingId = null;
            string seatNumber = null;
            decimal paidSeatAmount = 0;
            string currencyFromSeatmapMutation = null;
            string fareProductNameFromSeatMap = null;
            string matchedCabinClassType = null;
            int totalSegmentCount = 0;
            List<string> skippedPnrs = new();

            for (int candidateIndex = 0; candidateIndex < candidatePnrs.Count; candidateIndex++)
            {
                string candidatePnr = candidatePnrs[candidateIndex];
                string candidateFirst = candidateIndex < candidateFirstNames.Count ? candidateFirstNames[candidateIndex] : firstName;
                string candidateLast = candidateIndex < candidateLastNames.Count ? candidateLastNames[candidateIndex] : lastName;

                // --- Booking call ---
                HttpResponseMessage bookingResponse = GraphQlEndpoint.Booking(env, usingString, candidatePnr, candidateFirst, candidateLast).EnsureIsSuccessStatusCode();
                dynamic candidateBookingResult = JsonConvert.DeserializeObject<dynamic>(await bookingResponse.Content.ReadAsStringAsync());

                if (DynamicHelper.HasProperty(candidateBookingResult, "errors") || !DynamicHelper.HasProperty(candidateBookingResult, "data") || !DynamicHelper.HasProperty(candidateBookingResult.data, "Booking"))
                {
                    skippedPnrs.Add($"{candidatePnr}(booking-error)");
                    continue;
                }

                if (IsSeatAlreadyAssigned(candidateBookingResult))
                {
                    skippedPnrs.Add($"{candidatePnr}(seat-already-assigned)");
                    continue;
                }

                // --- SeatMap call ---
                string candidateBookingId = candidateBookingResult.data.Booking.id;
                if (string.IsNullOrEmpty(candidateBookingId))
                {
                    skippedPnrs.Add($"{candidatePnr}(no-booking-id)");
                    continue;
                }

                HttpResponseMessage seatMapResponse = GraphQlEndpoint.GetSeatMap(env, candidateBookingId, "1").EnsureIsSuccessStatusCode();
                dynamic candidateSeatMapResult = JsonConvert.DeserializeObject<dynamic>(await seatMapResponse.Content.ReadAsStringAsync());

                if (DynamicHelper.HasProperty(candidateSeatMapResult, "errors") || !DynamicHelper.HasProperty(candidateSeatMapResult, "data"))
                {
                    skippedPnrs.Add($"{candidatePnr}(seatmap-error)");
                    continue;
                }

                dynamic candidateSeatMapQuery = candidateSeatMapResult.data?.seatMapQuery;
                if (candidateSeatMapQuery == null)
                {
                    skippedPnrs.Add($"{candidatePnr}(no-seatmap)");
                    continue;
                }

                dynamic candidateFlightPassengerSeatMap = candidateSeatMapQuery.flightPassengerSeatMap;
                if (candidateFlightPassengerSeatMap == null || !candidateFlightPassengerSeatMap.HasValues)
                {
                    skippedPnrs.Add($"{candidatePnr}(no-flightPassengerSeatMap)");
                    continue;
                }

                // --- Find a paid seat across cabin priority ---
                string foundSeatNumber = null;
                decimal foundAmount = 0;
                string foundCurrency = null;
                string foundFareProductName = null;
                string foundCabinType = null;

                foreach (string targetCabin in cabinPriority)
                {
                    foreach (dynamic cabin in candidateSeatMapQuery.seatMap)
                    {
                        if (((string)cabin.classType).ToUpper() != targetCabin)
                            continue;
                        foreach (dynamic row in cabin.rows)
                        {
                            foreach (dynamic column in row.columns)
                            {
                                if ((string)column.type == "SEAT" && (string)column.availability == "FREE" && column.offers != null && column.offers.HasValues)
                                {
                                    foreach (dynamic offer in column.offers)
                                    {
                                        if (DynamicHelper.HasProperty(offer, "price") && offer.price != null &&
                                            DynamicHelper.HasProperty(offer, "fareProduct") &&
                                            DynamicHelper.HasProperty(offer.price, "amount") &&
                                            DynamicHelper.HasProperty(offer.price, "currency") &&
                                            offer.price.amount != null)
                                        {
                                            decimal amount = (decimal)offer.price.amount;
                                            if (amount > 0)
                                            {
                                                foundSeatNumber = (string)column.seatNumber;
                                                foundAmount = amount;
                                                foundCurrency = (string)offer.price.currency;
                                                foundFareProductName = (string)offer.fareProductName;
                                                foundCabinType = targetCabin;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (foundSeatNumber != null)
                                    break;
                            }
                            if (foundSeatNumber != null)
                                break;
                        }
                        if (foundSeatNumber != null)
                            break;
                    }
                    if (foundSeatNumber != null)
                        break;
                }

                if (foundSeatNumber == null)
                {
                    skippedPnrs.Add($"{candidatePnr}(no-paid-seat-in-any-cabin)");
                    continue;
                }

                // All checks passed — commit this candidate
                pnr = candidatePnr;
                firstName = candidateFirst;
                lastName = candidateLast;
                debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);
                BookingResult = candidateBookingResult;
                bookingId = candidateBookingId;
                seatMapQuery = candidateSeatMapQuery;
                flightPassengerSeatMap = candidateFlightPassengerSeatMap;
                seatNumber = foundSeatNumber;
                paidSeatAmount = foundAmount;
                currencyFromSeatmapMutation = foundCurrency;
                fareProductNameFromSeatMap = foundFareProductName;
                matchedCabinClassType = foundCabinType;

                dynamic? candidatePassengers = BookingResult.data.Booking.passengers;
                foreach (dynamic pax in candidatePassengers)
                    passengerIds.Add((string)pax.passengerId);

                dynamic? candidateProducts = BookingResult.data.Booking.product;
                dynamic? candidateAirlineProduct = ((IEnumerable<dynamic>)candidateProducts).FirstOrDefault(p => p.bookingType == "AIRLINE");
                if (candidateAirlineProduct?.slices != null)
                    foreach (dynamic slice in candidateAirlineProduct.slices)
                        totalSegmentCount += slice.segments != null ? ((IEnumerable<dynamic>)slice.segments).Count() : 0;

                break;
            }

            if (bookingId == null)
            {
                SkipTest($"Test skipped: No usable PNR found after checking all candidates. Skipped: [{string.Join(", ", skippedPnrs)}]. Filter: {flightTypeString}/{cabinTypeString}/{currencyString}.");
                return;
            }

            Assert.True(passengerIds.Count > 0, $"Response does not contain 'passengers'. {debugContext}");
            Assert.True(paidSeatAmount > 0, $"Seat amount should be greater than 0, found: {paidSeatAmount}. {debugContext}");
            Assert.True(!string.IsNullOrEmpty(currencyFromSeatmapMutation), $"Seat currency should not be null or empty. {debugContext}");

            string firstPassengerId = passengerIds.First();

            #endregion

            #region - Validate seat map response (already fetched above per candidate)

            string segmentId = "1";

            Assert.True(seatMapQuery != null, $"SeatMapQuery should not be null. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(seatMapQuery, "flightPassengerSeatMap"), $"Response does not contain flightPassengerSeatMap. {debugContext}");
            Assert.True(flightPassengerSeatMap != null && flightPassengerSeatMap.HasValues, $"flightPassengerSeatMap should not be null or empty. {debugContext}");

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

            #endregion

            #region - GraphQL call for select seat and Validate SelectSeat response (first pax only in flightPassengerSeatMap)

            // Find the first passenger seat mapping in flightPassengerSeatMap
            dynamic firstPassengerSeat = flightPassengerSeatMap.First;

            // Extract passengerId and seatOfferId
            string passengerIdForSeat = firstPassengerSeat.passengerId;
            string paxSeatOfferId = firstPassengerSeat.seatOfferId;

            // Now you can pass these to the selectSeat mutation
            HttpResponseMessage selectSeatResponse = GraphQlEndpoint.SelectSeat(env, bookingId, segmentId, passengerIdForSeat, seatNumber, paxSeatOfferId).EnsureIsSuccessStatusCode();
            dynamic selectSeatResult = JsonConvert.DeserializeObject<dynamic>(await selectSeatResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(selectSeatResult, "errors"), $"Expected a valid response from Graphql for select seat but found \n {selectSeatResult}. {debugContext}");

            dynamic selectSeatMutation = selectSeatResult.data.selectSeatMutation;
            Assert.NotNull(selectSeatMutation);

            // Ensure flightPassengerSeatMap exists
            Assert.True(DynamicHelper.HasProperty(selectSeatMutation, "flightPassengerSeatMap"), $"SelectSeat response does not contain flightPassengerSeatMap. {debugContext}");

            dynamic? flightPassengerSelectSeat = selectSeatMutation.flightPassengerSeatMap;
            Assert.True(flightPassengerSelectSeat != null && flightPassengerSelectSeat.HasValues, $"flightPassengerSelectSeat should not be null or empty. {debugContext}");

            // Get first passenger mapping
            dynamic? firstPassengerSelectSeat = flightPassengerSelectSeat.First;
            Assert.True(firstPassengerSelectSeat is not null, $"flightPassengerSelectSeat should not be null or empty. {debugContext}");

            // Validate passengerId
            string passengerIdFromResponse = firstPassengerSelectSeat.passengerId;
            Assert.True(firstPassengerId.Equals(passengerIdFromResponse), $"PassengerId {passengerIdFromResponse} does not match the passenger id {firstPassengerId} sent in the request. {debugContext}");

            // Validate seatNumber
            string seatNumberFromResponse = firstPassengerSelectSeat.seatNumber;
            Assert.True(seatNumber.Equals(seatNumberFromResponse), $"Seat number {seatNumberFromResponse} does not match the seat number {seatNumber} sent in the request. {debugContext}");

            // Validate seatOfferId
            string seatOfferIdFromResponse = firstPassengerSelectSeat.seatOfferId;
            Assert.True(paxSeatOfferId.Equals(seatOfferIdFromResponse), $"Seat offer id {seatOfferIdFromResponse} does not match the seat offer id {paxSeatOfferId} sent in the request. {debugContext}");

            // Validate segmentId matches expected
            string segmentIdFromResponse = firstPassengerSelectSeat.segmentId;
            Assert.True(segmentId.Equals(segmentIdFromResponse), $"Segment id {segmentIdFromResponse} does not match the segment id {segmentId} sent in the request. {debugContext}");

            #endregion

            #region - GraphQL call for seat payment mutation and Validate seat payment response

            HttpResponseMessage seatPaymentMutationResponse = GraphQlEndpoint.SeatPayment(env, bookingId).EnsureIsSuccessStatusCode();
            dynamic seatPaymentMutationResult = JsonConvert.DeserializeObject<dynamic>(await seatPaymentMutationResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(seatPaymentMutationResult, "errors") || seatPaymentMutationResult.errors == null, $"Expected a valid response from Graphql for seat confirmation but found {seatPaymentMutationResult}. {debugContext}");

            dynamic? seatPaymentMutation = seatPaymentMutationResult.data.seatPaymentMutation;

            Assert.True(seatPaymentMutation is not null, $"seatConfirmationMutation should not be null. {debugContext}");

            string paymentReferenceId = seatPaymentMutation.paymentReferenceId;

            Assert.True(!string.IsNullOrEmpty(paymentReferenceId), $"paymentReferenceId should not be null or empty. {debugContext}");

            string hppUrl = seatPaymentMutation.hppLink;
            Assert.True(!string.IsNullOrEmpty(hppUrl), $"hppUrl should not be null or empty. {debugContext}");

            decimal monetaryAmount = seatPaymentMutation.amount.monetaryAmount;
            Assert.True(paidSeatAmount == monetaryAmount, $"paid amount {paidSeatAmount} should match with the seat payment response amount {monetaryAmount}. {debugContext}");

            string currencyFromSeatpaymentMutationResponse = seatPaymentMutation.amount.baseCurrency;
            Assert.True(currencyFromSeatmapMutation.Equals(currencyFromSeatpaymentMutationResponse), $"currencyFromSeatmapMutation {currencyFromSeatmapMutation} should match with the response currencyFromSeatpaymentMutationResponse {currencyFromSeatpaymentMutationResponse}. {debugContext}");

            #endregion

            #region - chrome browser call to open the hpp link and make the payment

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

            HttpResponseMessage SeatFulfilmentMutationResponse = GraphQlEndpoint.SeatFulfilmentMutation(env, bookingId, isPaymentStatus).EnsureIsSuccessStatusCode();
            string seatFulfilmentRawResponse = await SeatFulfilmentMutationResponse.Content.ReadAsStringAsync();
            dynamic SeatFulfilmentMutationResult = JsonConvert.DeserializeObject<dynamic>(seatFulfilmentRawResponse);
            string seatFulfilmentErrorsSummary = DynamicHelper.HasProperty(SeatFulfilmentMutationResult, "errors") && SeatFulfilmentMutationResult.errors != null
                ? TruncateForAssertion(SeatFulfilmentMutationResult.errors.ToString(), 1200)
                : "<null>";

            string cardNumber = _testConfig.Book.Payment.CardDetails.CardNumber?.ToString();
            string maskedCard = MaskCardNumber(cardNumber);

            Assert.True(
                !DynamicHelper.HasProperty(SeatFulfilmentMutationResult, "errors") || SeatFulfilmentMutationResult.errors == null,
                $"Seat fulfilment returned GraphQL errors. errors={seatFulfilmentErrorsSummary}; cardType={_testConfig.Book.Payment.CardDetails.PaymentCardType}; card={maskedCard}; bookingId={bookingId}; pnr={pnr}; responseLength={seatFulfilmentRawResponse.Length}; {debugContext}");

            Assert.True(DynamicHelper.HasProperty(SeatFulfilmentMutationResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(SeatFulfilmentMutationResult.data, "seatFulfilmentMutation"), $"Response should contain seatConfirmationMutation property. {debugContext}");

            dynamic? seatFulfilmentMutation = SeatFulfilmentMutationResult.data.seatFulfilmentMutation;
            Assert.True(seatFulfilmentMutation != null, $"seatFulfilmentMutation should not be null. {debugContext}");

            // Validate PNR locator
            Assert.True(DynamicHelper.HasProperty(seatFulfilmentMutation, "pnrLocator"), $"seatFulfilmentMutation should contain pnrLocator property. {debugContext}");
            string pnrLocatorFromResponse = seatFulfilmentMutation.pnrLocator;
            Assert.True(pnrLocatorFromResponse.Equals(pnr), $"expected pnr value in seatFulfilmentMutation {pnr} but found {pnrLocatorFromResponse}. {debugContext}");

            // Validate passenger seat selection
            Assert.True(DynamicHelper.HasProperty(seatFulfilmentMutation, "passengerSeatSelection"), $"seatConfirmationMutation should contain passengerSeatSelection property. {debugContext}");
            Assert.True(seatFulfilmentMutation.passengerSeatSelection != null && seatFulfilmentMutation.passengerSeatSelection.HasValues, $"passengerSeatSelection should not be null or empty. {debugContext}");

            dynamic? firstPassengerSeatSelection = seatFulfilmentMutation.passengerSeatSelection.First;
            Assert.True(firstPassengerSeatSelection != null, $"First passenger seat selection should not be null. {debugContext}");

            // Validate seat number
            Assert.True(DynamicHelper.HasProperty(firstPassengerSeatSelection, "seatNumber"), $"Passenger seat selection should contain seatNumber property. {debugContext}");
            string confirmedSeatNumber = firstPassengerSeatSelection.seatNumber;

            Assert.True(seatNumber.Equals(confirmedSeatNumber), $"seatNumber {seatNumber} doesnot match in the confirmation response {confirmedSeatNumber.ToString()}. {debugContext}");

            // Validate price information
            Assert.True(DynamicHelper.HasProperty(firstPassengerSeatSelection, "price"), $"Passenger seat selection should contain price property. {debugContext}");
            Assert.True(firstPassengerSeatSelection.price != null, $"Price should not be null. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(firstPassengerSeatSelection.price, "amount"), $"Price should contain amount property. {debugContext}");
            decimal confirmedAmount = (decimal)firstPassengerSeatSelection.price.amount;
            Assert.True(confirmedAmount.Equals(paidSeatAmount), $"expected amount in SeatConfirmationMutation {paidSeatAmount} but found {confirmedAmount}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(firstPassengerSeatSelection.price, "currency"), $"Price should contain currency property. {debugContext}");
            string confirmedCurrency = (string)firstPassengerSeatSelection.price.currency;
            Assert.True(confirmedCurrency.Equals(currencyFromSeatmapMutation), $"expected currency in SeatConfirmationMutation {currencyFromSeatmapMutation} but found {confirmedCurrency}. {debugContext}");

            // Validate selected fare product name
            Assert.True(DynamicHelper.HasProperty(firstPassengerSeatSelection, "selectedFareProductName"), $"Passenger seat selection should contain selectedFareProductName property. {debugContext}");
            string selectedFareProductName = firstPassengerSeatSelection.selectedFareProductName;
            Assert.True(string.Equals(selectedFareProductName, fareProductNameFromSeatMap), $"expected fare product name as {fareProductNameFromSeatMap} but found {selectedFareProductName}. {debugContext}");

            // Validate confirmation status
            Assert.True(DynamicHelper.HasProperty(firstPassengerSeatSelection, "isConfirmed"), $"Passenger seat selection should contain isConfirmed property. {debugContext}");
            bool isConfirmed = (bool)firstPassengerSeatSelection.isConfirmed;
            Assert.True(isConfirmed, $"Seat selection should be confirmed. {debugContext}");

            #endregion
        }

        private static string TruncateForAssertion(string? value, int maxLength)
        {
            return string.IsNullOrEmpty(value) || value.Length <= maxLength ? value ?? string.Empty : value[..maxLength] + "...(truncated)";
        }
    }
}
