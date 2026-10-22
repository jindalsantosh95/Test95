using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL.Seats
{
    [Collection("SeatSharedServicesSignOff")]
    public class FreeSeatsTests : SeatTestsBase
    {
        public FreeSeatsTests(TokenFixture tokenFixture) : base(tokenFixture)
        {
        }

        [SkippableFact]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "SeatMS")]
        [Trait("SharedServiceConsumer", "Trips")]
        public async Task SharedServicesSignOff_FreeSeatSelection() =>
            await SelectFreeSeat_ReturnsExpectedResult(FlightType.VS, CabinType.UpperClass, Currency.GBP, "LHR", "JFK");

        [SkippableTheory]
        [Trait("Category", Traits.SelectFreeSeatsRegression)]
        [InlineData(FlightType.VS, CabinType.UpperClass, Currency.GBP)]
        [InlineData(FlightType.VS, CabinType.UpperClass, Currency.USD)]
        [InlineData(FlightType.DL, CabinType.DeltaMain, Currency.GBP)]
        [InlineData(FlightType.DL, CabinType.DeltaOne, Currency.USD)]
        public async Task SelectFreeSeat_ReturnsExpectedResult(FlightType flightType, CabinType cabinType, Currency currency, string origin = null, string destination = null)
        {
            #region - pre-requisite for fetching the PNR candidates from the storage table

            string flightTypeString = flightType.ToString();
            string cabinTypeString = cabinType.ToString();
            string currencyString = currency.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForSeatScenarios(flightType: flightTypeString, cabinType: cabinTypeString, currency: currencyString, origin: origin, destination: destination);

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

            #region - Iterate candidates to find a usable PNR with a free seat available

            dynamic bookingResult = null;
            dynamic seatMapQuery = null;
            dynamic flightPassengerSeatMap = null;
            List<string> passengerIds = new();
            string bookingId = null;
            string seatNumber = null;
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

                // Find a free seat in the cabin matching the test parameter
                string targetClassType = cabinType switch
                {
                    CabinType.UpperClass => "UPPER CLASS",
                    CabinType.DeltaMain  => "DELTA MAIN",
                    CabinType.DeltaOne   => "DELTA ONE",
                    _                    => cabinTypeString.ToUpper()
                };

                string foundSeatNumber = null;
                foreach (dynamic cabin in candidateSeatMapQuery.seatMap)
                {
                    string classType = ((string)cabin.classType).ToUpper();
                    if (classType == targetClassType)
                    {
                        foreach (dynamic row in cabin.rows)
                        {
                            foreach (dynamic col in row.columns)
                            {
                                if ((string)col.type == "SEAT" && (string)col.availability == "FREE" && col.offers != null && col.offers.HasValues)
                                {
                                    foundSeatNumber = (string)col.seatNumber;
                                    break;
                                }
                            }
                            if (foundSeatNumber != null) break;
                        }
                    }
                    if (foundSeatNumber != null) break;
                }

                if (foundSeatNumber == null)
                {
                    skippedPnrs.Add($"{candidatePnr}(no-free-seat)");
                    continue;
                }

                dynamic? candidatePassengers = candidateBookingResult.data.Booking.passengers;
                if (candidatePassengers == null || !candidatePassengers.HasValues)
                {
                    skippedPnrs.Add($"{candidatePnr}(no-passengers)");
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

                foreach (dynamic pax in candidatePassengers)
                    passengerIds.Add((string)pax.passengerId);

                break;
            }

            if (bookingId == null)
            {
                SkipTest($"Test skipped: No usable PNR found after checking all candidates. Skipped: [{string.Join(", ", skippedPnrs)}]. Filter: {flightTypeString}/{cabinTypeString}/{currencyString}.");
                return;
            }

            #endregion

            #region - Validate booking and seat map response

            string segmentId = "1";

            Assert.True(passengerIds.Count > 0, $"Response does not contain 'passengers'. {debugContext}");
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

            string firstPassengerId = passengerIds.First();

            #endregion

            #region - GraphQL call for select seat and validate SelectSeat response

            dynamic firstPassengerSeat = flightPassengerSeatMap.First;
            string passengerIdForSeat = firstPassengerSeat.passengerId;
            string paxSeatOfferId = firstPassengerSeat.seatOfferId;

            HttpResponseMessage selectSeatResponse = GraphQlEndpoint.SelectSeat(env, bookingId, segmentId, passengerIdForSeat, seatNumber, paxSeatOfferId).EnsureIsSuccessStatusCode();
            dynamic selectSeatResult = JsonConvert.DeserializeObject<dynamic>(await selectSeatResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(selectSeatResult, "errors") || selectSeatResult.errors == null, $"Expected a valid response from Graphql for select seat but found \n {selectSeatResult}. {debugContext}");

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

            #region - GraphQL call for seat fulfilment mutation to confirm the selected seats

            string isPaymentStatus = "SUCCESS";

            HttpResponseMessage seatFulfilmentMutationResponse = GraphQlEndpoint.SeatFulfilmentMutation(env, bookingId, isPaymentStatus).EnsureIsSuccessStatusCode();
            dynamic seatFulfilmentMutationResult = JsonConvert.DeserializeObject<dynamic>(await seatFulfilmentMutationResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(seatFulfilmentMutationResult, "errors") || seatFulfilmentMutationResult.errors == null, $"Expected a valid response from Graphql for seat fulfilment but found {seatFulfilmentMutationResult}. {debugContext}");

            dynamic? seatFulfilmentResult = seatFulfilmentMutationResult.data.seatFulfilmentMutation;
            Assert.True(seatFulfilmentResult is not null, $"seatFulfilmentMutation should not be null. {debugContext}");

            string pnrFromResponse = seatFulfilmentResult.pnrLocator;
            Assert.True(pnr.Equals(pnrFromResponse), $"pnr {pnr} does not match with the PNR in the confirmation mutation result {pnrFromResponse}. {debugContext}");

            Assert.True(seatFulfilmentResult.passengerSeatSelection != null && seatFulfilmentResult.passengerSeatSelection.HasValues, $"passengerSeatSelection should not be null or empty. {debugContext}");

            dynamic firstPassengerSelection = seatFulfilmentResult.passengerSeatSelection.First;

            string passengerIdFromConfirmation = firstPassengerSelection.passengerId;
            Assert.True(firstPassengerId.Equals(passengerIdFromConfirmation), $"passengerId {firstPassengerId} does not match in the confirmation response {passengerIdFromConfirmation}. {debugContext}");

            string seatNumberFromConfirmation = firstPassengerSelection.seatNumber;
            Assert.True(seatNumber.Equals(seatNumberFromConfirmation), $"seatNumber {seatNumber} does not match in the confirmation response {seatNumberFromConfirmation}. {debugContext}");

            #endregion
        }

    }
}
