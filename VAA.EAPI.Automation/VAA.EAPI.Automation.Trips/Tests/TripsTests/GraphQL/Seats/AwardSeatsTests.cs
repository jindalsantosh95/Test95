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
    public class AwardSeatsTests : SeatTestsBase
    {
        public AwardSeatsTests(TokenFixture tokenFixture) : base(tokenFixture)
        {
        }

        [SkippableTheory]
        [Trait("Category", Traits.SelectSeatsAwardBookingRegression)]
        [InlineData(Currency.GBP, FlyingClubMemberTiers.Red)]
        [InlineData(Currency.USD, FlyingClubMemberTiers.Red)]
        public async Task SelectAwardSeat_ReturnsExpectedResult(Currency currency, FlyingClubMemberTiers tier)
        {
            #region - pre-requisite for fetching the PNR from the storage table

            string currencyString = currency.ToString();
            string tierString = tier.ToString();
            string cabinTypeString = CabinType.EconomyClassic.ToString();
            string passengersCount = "1";

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForSeatScenarios(cabinType: cabinTypeString, currency: currencyString, reservationType: "AWARD", tierCode: tierString, passengersCount: passengersCount);

            ValidatePnrDetails(pnrDetails);

            List<string> candidatePnrs = (pnrDetails.TryGetValue("allCandidatePnrs", out string allPnrs) && !string.IsNullOrEmpty(allPnrs))
                ? allPnrs.Split(',').ToList() : new List<string> { pnrDetails["pnr"] };
            List<string> candidateFirstNames = (pnrDetails.TryGetValue("allCandidateFirstNames", out string allFirst) && !string.IsNullOrEmpty(allFirst))
                ? allFirst.Split(new[] { "||" }, StringSplitOptions.None).ToList() : new List<string> { pnrDetails["firstName"] };
            List<string> candidateLastNames = (pnrDetails.TryGetValue("allCandidateLastNames", out string allLast) && !string.IsNullOrEmpty(allLast))
                ? allLast.Split(new[] { "||" }, StringSplitOptions.None).ToList() : new List<string> { pnrDetails["lastName"] };

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            #endregion

            #region - Iterate candidates to find a usable award PNR with a free paid seat

            string[] cabinPriority = { "ECONOMY", "PREMIUM ECONOMY", "UPPER CLASS" };

            dynamic bookingResult = null;
            dynamic seatMapQuery = null;
            dynamic flightPassengerSeatMap = null;
            List<string> passengerIds = new();
            string bookingId = null;
            string seatNumber = null;
            decimal paidSeatAmount = 0;
            string currencyFromSeatmapMutation = null;
            string fareProductNameFromSeatMap = null;
            List<string> skippedPnrs = new();

            for (int candidateIndex = 0; candidateIndex < candidatePnrs.Count; candidateIndex++)
            {
                string candidatePnr = candidatePnrs[candidateIndex];
                string candidateFirst = candidateIndex < candidateFirstNames.Count ? candidateFirstNames[candidateIndex] : firstName;
                string candidateLast = candidateIndex < candidateLastNames.Count ? candidateLastNames[candidateIndex] : lastName;

                HttpResponseMessage bookingResponse = GraphQlEndpoint.Booking(env, "PNR", candidatePnr, candidateFirst, candidateLast).EnsureIsSuccessStatusCode();
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

                string foundSeatNumber = null;
                decimal foundAmount = 0;
                string foundCurrency = null;
                string foundFareProductName = null;

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
                bookingResult = candidateBookingResult;
                bookingId = candidateBookingId;
                seatMapQuery = candidateSeatMapQuery;
                flightPassengerSeatMap = candidateFlightPassengerSeatMap;
                seatNumber = foundSeatNumber;
                paidSeatAmount = foundAmount;
                currencyFromSeatmapMutation = foundCurrency;
                fareProductNameFromSeatMap = foundFareProductName;

                dynamic? candidatePassengers = bookingResult.data.Booking.passengers;
                if (candidatePassengers != null && candidatePassengers.HasValues)
                    foreach (dynamic pax in candidatePassengers)
                        passengerIds.Add((string)pax.passengerId);

                break;
            }

            if (bookingId == null)
            {
                SkipTest($"Test skipped: No usable award PNR found after checking all candidates. Skipped: [{string.Join(", ", skippedPnrs)}]. Filter: {cabinTypeString}/{currencyString}/{tierString}.");
                return;
            }

            #endregion

            #region - Validate booking response

            Assert.True(passengerIds.Count > 0, $"Response does not contain 'passengers'. {debugContext}");
            Assert.True(paidSeatAmount > 0, $"Seat amount should be greater than 0, found: {paidSeatAmount}. {debugContext}");
            Assert.True(!string.IsNullOrEmpty(currencyFromSeatmapMutation), $"Seat currency should not be null or empty. {debugContext}");

            string firstPassengerId = passengerIds.First();

            #endregion

            #region - Validate seat map response

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

            #region - GraphQL call for select seat and validate SelectSeat response

            dynamic firstPassengerSeat = flightPassengerSeatMap.First;
            string passengerIdForSeat = firstPassengerSeat.passengerId;
            string paxSeatOfferId = firstPassengerSeat.seatOfferId;

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

            #endregion

            #region - GraphQL call for seat payment mutation and validate response

            HttpResponseMessage seatPaymentMutationResponse = GraphQlEndpoint.SeatPayment(env, bookingId).EnsureIsSuccessStatusCode();
            dynamic seatPaymentMutationResult = JsonConvert.DeserializeObject<dynamic>(await seatPaymentMutationResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(seatPaymentMutationResult, "errors") || seatPaymentMutationResult.errors == null, $"Expected a valid response from Graphql for seat payment but found {seatPaymentMutationResult}. {debugContext}");

            dynamic? seatPaymentMutation = seatPaymentMutationResult.data.seatPaymentMutation;
            Assert.True(seatPaymentMutation is not null, $"seatPaymentMutation should not be null. {debugContext}");

            string paymentReferenceId = seatPaymentMutation.paymentReferenceId;
            Assert.True(!string.IsNullOrEmpty(paymentReferenceId), $"paymentReferenceId should not be null or empty. {debugContext}");

            string hppUrl = seatPaymentMutation.hppLink;
            Assert.True(!string.IsNullOrEmpty(hppUrl), $"hppUrl should not be null or empty. {debugContext}");

            decimal monetaryAmount = seatPaymentMutation.amount.monetaryAmount;
            Assert.True(paidSeatAmount == monetaryAmount, $"paid amount {paidSeatAmount} should match with the seat payment response amount {monetaryAmount}. {debugContext}");

            string currencyFromSeatpaymentMutationResponse = seatPaymentMutation.amount.baseCurrency;
            Assert.True(currencyFromSeatmapMutation.Equals(currencyFromSeatpaymentMutationResponse), $"currencyFromSeatmapMutation {currencyFromSeatmapMutation} should match with the response {currencyFromSeatpaymentMutationResponse}. {debugContext}");

            #endregion

            #region - Open HPP and submit card payment

            _testConfig = new TestConfig();
            using NavigateToHostedPaymentPage enterHppCardDetails = new(hppUrl);
            ConfigureSeatPaymentCard(currency);
            enterHppCardDetails.SubmitHppCardDetails(_testConfig);

            #endregion

            #region - GraphQL call for seat fulfilment mutation and validate confirmation

            string isPaymentStatus = "SUCCESS";

            HttpResponseMessage seatFulfilmentMutationResponse = GraphQlEndpoint.SeatFulfilmentMutation(env, bookingId, isPaymentStatus).EnsureIsSuccessStatusCode();
            dynamic seatFulfilmentMutationResult = JsonConvert.DeserializeObject<dynamic>(await seatFulfilmentMutationResponse.Content.ReadAsStringAsync());

            string maskedCard = MaskCardNumber(_testConfig.Book.Payment.CardDetails.CardNumber?.ToString());

            Assert.True(!DynamicHelper.HasProperty(seatFulfilmentMutationResult, "errors") || seatFulfilmentMutationResult.errors == null,
                $"Expected a valid response from Graphql for seat fulfilment but found {seatFulfilmentMutationResult}. Card: {_testConfig.Book.Payment.CardDetails.PaymentCardType} {maskedCard}. {debugContext}");

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
            Assert.True(string.Equals(selectedFareProductName, fareProductNameFromSeatMap), $"expected fare product name {fareProductNameFromSeatMap} but found {selectedFareProductName}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(firstPassengerSeatSelection, "isConfirmed"), $"Passenger seat selection should contain isConfirmed property. {debugContext}");
            bool isConfirmed = (bool)firstPassengerSeatSelection.isConfirmed;
            Assert.True(isConfirmed, $"Seat selection should be confirmed. {debugContext}");

            #endregion
        }
    }
}
