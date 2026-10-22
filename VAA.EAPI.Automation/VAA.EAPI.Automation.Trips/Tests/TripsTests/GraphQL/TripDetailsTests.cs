using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL
{
    public class TripDetailsTests : TripsGraphQlTestsBase
    {
        [SkippableTheory]
        [Trait("Category", Traits.TripDetailsMobileRegression)]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "OrderDataMS")]
        [Trait("SharedServiceConsumer", "Trips")]
        [Trait("TestType", "Contract")]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task RetrieveMultipleTrips_ReturnsTripDetails(JourneyType journeyType, ReservationType reservationType)
        {
            #region - pre-requisite for fetching 2 PNRs from the storage table

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            // Get first PNR
            Dictionary<string, string> pnrDetails1 = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);
            ValidatePnrDetails(pnrDetails1);
            string debugContext1 = RequestHelper.FormatTripsBookingContext(pnrDetails1["pnr"], pnrDetails1["firstName"], pnrDetails1["lastName"]);

            // Get second PNR (different from first)
            Dictionary<string, string> pnrDetails2;
            int retryCount = 0;
            const int maxRetries = 3;
            do
            {
                pnrDetails2 = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);
                ValidatePnrDetails(pnrDetails2);
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    TestDataPrerequisiteHelper.Skip($"Test skipped: Could not find a different PNR after {maxRetries} attempts. Only one PNR may exist in storage for {journeyTypeString}/{reservationTypeString}. {debugContext1}");
                }
            } while (pnrDetails1["pnr"] == pnrDetails2["pnr"]); // Ensure different PNRs

            string debugContext2 = RequestHelper.FormatTripsBookingContext(pnrDetails2["pnr"], pnrDetails2["firstName"], pnrDetails2["lastName"]);
            string debugContext = $"{debugContext1} | {debugContext2}";

            List<string> pnrs = new()
            { pnrDetails1["pnr"], pnrDetails2["pnr"] };
            List<string> firstNames = new()
            { pnrDetails1["firstName"], pnrDetails2["firstName"] };
            List<string> lastNames = new()
            { pnrDetails1["lastName"], pnrDetails2["lastName"] };
            List<string>? createdDates = null;

            #endregion

            #region - GraphQL call for retrieve multiple trips

            HttpResponseMessage response = GraphQlEndpoint.TripDetails(env, pnrs, firstNames, lastNames, createdDates).EnsureIsSuccessStatusCode();
            dynamic TripDetailsResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            #endregion

            #region - Validations

            // Validate data exists
            Assert.True(DynamicHelper.HasProperty(TripDetailsResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(TripDetailsResult.data, "TripDetails"), $"Response should contain TripDetails property. {debugContext}");

            // Validate trip details contains 2 trips
            JToken? tripDetailsToken = TripDetailsResult.data.TripDetails as JToken;
            Assert.NotNull(tripDetailsToken);
            Assert.True(tripDetailsToken!.Type == JTokenType.Array,
                $"Expected data.TripDetails to be an array but received {tripDetailsToken.Type}.");
            JArray tripDetails = (JArray)tripDetailsToken;
            Assert.True(tripDetails.Count == 2, $"Expected 2 trips in response but got {tripDetails.Count}. {debugContext}");

            // Validate both PNRs exist in response (order may vary)
            List<string> returnedPnrs = new();

            foreach (dynamic? trip in tripDetails)
            {
                string pnrFromTrip = trip.product[0]["reference"]["pnrLocator"].ToString();
                returnedPnrs.Add(pnrFromTrip);
            }

            Assert.True(returnedPnrs.Contains(pnrs[0]), $"Expected PNR {pnrs[0]} to be in response but got {string.Join(", ", returnedPnrs)}. {debugContext}");
            Assert.True(returnedPnrs.Contains(pnrs[1]), $"Expected PNR {pnrs[1]} to be in response but got {string.Join(", ", returnedPnrs)}. {debugContext}");

            #endregion
        }

        [SkippableTheory]
        [Trait("Category", Traits.TripDetailsMobileRegression)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task RetrieveMultipleTrips_AllowsPartialFailureForSpecialCharacterLastName(JourneyType journeyType, ReservationType reservationType)
        {
            #region - pre-requisite for fetching 2 PNRs from the storage table

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            // Get first PNR
            Dictionary<string, string> pnrDetails1 = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);
            ValidatePnrDetails(pnrDetails1);

            // Get second PNR (different from first)
            Dictionary<string, string> pnrDetails2;
            int retryCount = 0;
            const int maxRetries = 3;
            do
            {
                pnrDetails2 = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);
                ValidatePnrDetails(pnrDetails2);
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    TestDataPrerequisiteHelper.Skip($"Test skipped: Could not find a different PNR after {maxRetries} attempts. Only one PNR may exist in storage for {journeyTypeString}/{reservationTypeString}");
                }
            } while (pnrDetails1["pnr"] == pnrDetails2["pnr"]); // Ensure different PNRs

            List<string> pnrs = new()
            { pnrDetails1["pnr"], pnrDetails2["pnr"] };
            List<string> firstNames = new()
            { pnrDetails1["firstName"], pnrDetails2["firstName"] };
            List<string> lastNames = new()
            { pnrDetails1["lastName"], pnrDetails2["lastName"] };

            // Inject special characters into the second PNR's last name to simulate special-character scenario
            lastNames[1] = lastNames[1] + "@#$$%^";

            List<string>? createdDates = null;

            #endregion

            #region - GraphQL call for retrieve multiple trips (expect only valid PNR returned)

            HttpResponseMessage response = GraphQlEndpoint.TripDetails(env, pnrs, firstNames, lastNames, createdDates).EnsureIsSuccessStatusCode();
            dynamic TripDetailsResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            #endregion

            #region - Validations

            // Validate data exists and TripDetails is present
            Assert.True(DynamicHelper.HasProperty(TripDetailsResult, "data"), "Response should contain data property");
            Assert.True(DynamicHelper.HasProperty(TripDetailsResult.data, "TripDetails"), "Response should contain TripDetails property");

            JToken? tripDetailsToken = TripDetailsResult.data.TripDetails as JToken;
            Assert.NotNull(tripDetailsToken);
            Assert.True(tripDetailsToken!.Type == JTokenType.Array,
                $"Expected data.TripDetails to be an array but received {tripDetailsToken.Type}: {TripDetailsResult}");
            JArray tripDetails = (JArray)tripDetailsToken;

            // Now both trips should be returned - one valid, one with error status
            Assert.True(tripDetails.Count == 2, $"Expected 2 trips in response but got {tripDetails.Count}");

            // Find the valid trip and the error trip
            dynamic validTrip = null;
            dynamic errorTrip = null;

            foreach (dynamic? trip in tripDetails)
            {
                string tripPnr = trip.tripSearchRequest.pnrLocator.ToString();
                if (tripPnr == pnrs[0])
                    validTrip = trip;
                else if (tripPnr == pnrs[1])
                    errorTrip = trip;
            }

            Assert.NotNull(validTrip);
            Assert.NotNull(errorTrip);

            // Validate the valid trip has OK status
            Assert.True(((string)validTrip.responseStatus.status).Equals("OK", StringComparison.OrdinalIgnoreCase), $"Expected valid trip status to be OK but got {validTrip.responseStatus.status}");

            // Validate the error trip has InternalServerError status and error details
            Assert.True(((string)errorTrip.responseStatus.status).Equals("InternalServerError", StringComparison.OrdinalIgnoreCase), $"Expected error trip status to be InternalServerError but got {errorTrip.responseStatus.status}");
            Assert.NotNull(errorTrip.responseStatus.errors);
            Assert.True(errorTrip.responseStatus.errors.Count > 0, "Error trip should contain errors array");
            Assert.Equal("ROR5037", (string)errorTrip.responseStatus.errors[0].code);

            #endregion
        }

        [SkippableTheory]
        [Trait("Category", Traits.TripDetailsMobileRegression)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        [InlineData(JourneyType.Return, ReservationType.REVENUE)]
        public async Task RetrieveMultipleTrips_AllowsSupportedSpecialCharacterLastName(JourneyType journeyType, ReservationType reservationType)
        {
            #region - pre-requisite for fetching 2 PNRs from the storage table

            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails1 = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);
            ValidatePnrDetails(pnrDetails1);

            Dictionary<string, string> pnrDetails2;
            int retryCount = 0;
            const int maxRetries = 3;
            do
            {
                pnrDetails2 = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);
                ValidatePnrDetails(pnrDetails2);
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    TestDataPrerequisiteHelper.Skip($"Test skipped: Could not find a different PNR after {maxRetries} attempts. Only one PNR may exist in storage for {journeyTypeString}/{reservationTypeString}");
                }
            } while (pnrDetails1["pnr"] == pnrDetails2["pnr"]);

            List<string> pnrs = new()
            { pnrDetails1["pnr"], pnrDetails2["pnr"] };
            List<string> firstNames = new()
            { pnrDetails1["firstName"], pnrDetails2["firstName"] };
            List<string> lastNames = new()
            { pnrDetails1["lastName"], pnrDetails2["lastName"] };

            // Inject allowed special characters into the second PNR's last name
            lastNames[1] = lastNames[1] + "'&";

            List<string> createdDates = new()
            {
                DateTime.Parse(pnrDetails1["timestamp"]).ToString("yyyy-MM-dd"),
                DateTime.Parse(pnrDetails2["timestamp"]).ToString("yyyy-MM-dd")
            };

            #endregion

            #region - GraphQL call for retrieve multiple trips

            HttpResponseMessage response = GraphQlEndpoint.TripDetails(env, pnrs, firstNames, lastNames, createdDates).EnsureIsSuccessStatusCode();
            dynamic TripDetailsResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            #endregion

            #region - Validations

            Assert.True(DynamicHelper.HasProperty(TripDetailsResult, "data"), "Response should contain data property");
            Assert.True(DynamicHelper.HasProperty(TripDetailsResult.data, "TripDetails"), "Response should contain TripDetails property");

            JToken? tripDetailsToken = TripDetailsResult.data.TripDetails as JToken;
            Assert.NotNull(tripDetailsToken);
            Assert.True(tripDetailsToken!.Type == JTokenType.Array,
                $"Expected data.TripDetails to be an array but received {tripDetailsToken.Type}: {TripDetailsResult}");
            JArray tripDetails = (JArray)tripDetailsToken;

            // Both trips should be returned with OK status
            Assert.True(tripDetails.Count == 2, $"Expected 2 trips in response but got {tripDetails.Count}");

            foreach (dynamic? trip in tripDetails)
            {
                Assert.True(((string)trip.responseStatus.status).Equals("OK", StringComparison.OrdinalIgnoreCase), $"Expected trip status to be OK but got {trip.responseStatus.status}");
            }

            #endregion
        }

        [SkippableTheory]
        [Trait("Category", Traits.TripDetailsMobileRegression)]
        [InlineData("VS", null, null)]
        [InlineData("DL", "This flight is with Delta Air Lines", "For full support to view and update your information, please contact the operating airline directly.")]
        public async Task ValidateOtherAirlineOperatedHelpAttributesInTripDetailsResponse(
            string partnerCode,
            string? expectedHelpTitle,
            string? expectedHelpText)
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable
                .RetrievePNRDetailsFromStorageTableForCurrentWeek(
                    flightType: partnerCode);
            ValidatePnrDetails(pnrDetails);

            string pnr = pnrDetails["pnr"];
            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            HttpResponseMessage response = GraphQlEndpoint.TripDetails(env, [pnr], [firstName], [lastName], null).EnsureIsSuccessStatusCode();
            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(DynamicHelper.HasProperty(result, "data"), $"TripDetails response should contain data. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(result.data, "TripDetails"), $"TripDetails response should contain TripDetails. {debugContext}");
            dynamic tripDetails = result.data.TripDetails;
            Assert.NotNull(tripDetails);
            Assert.True(tripDetails.Count > 0, $"Expected at least one trip. {debugContext}");

            bool foundExpectedOperatingCarrier = false;
            foreach (dynamic trip in tripDetails)
            {
                foreach (dynamic product in trip.product)
                {
                    foreach (dynamic slice in product.slices)
                    {
                        foreach (dynamic segment in slice.segments)
                        {
                            string operatingCarrierCode = segment.operatingCarrier?.carrierCode?.ToString() ?? string.Empty;
                            if (!operatingCarrierCode.Equals(partnerCode, StringComparison.OrdinalIgnoreCase))
                                continue;

                            foundExpectedOperatingCarrier = true;
                            Assert.Equal(expectedHelpTitle, (string?)segment.otherAirlineOperatedHelpTitle);
                            Assert.Equal(expectedHelpText, (string?)segment.otherAirlineOperatedHelpText);
                        }
                    }
                }
            }

            Assert.True(foundExpectedOperatingCarrier, $"No {partnerCode}-operated segment was found in the retrieved trip details. {debugContext}");
        }

        [SkippableFact]
        [Trait("Category", Traits.TripDetailsMobileRegression)]
        public async Task ValidatePopsOpensSevenDaysBeforeDepartureInTripDetailsResponse()
        {
            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromOCIPNRsStorageTable();
            ValidatePnrDetails(pnrDetails);

            HttpResponseMessage response = GraphQlEndpoint.TripDetails(
                env,
                [pnrDetails["pnr"]],
                [pnrDetails["firstName"]],
                [pnrDetails["lastName"]],
                null).EnsureIsSuccessStatusCode();
            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(DynamicHelper.HasProperty(result, "data"), "POPS TripDetails response should contain data.");
            Assert.True(DynamicHelper.HasProperty(result.data, "TripDetails"), "POPS TripDetails response should contain TripDetails.");

            dynamic tripDetails = result.data.TripDetails;
            Assert.NotNull(tripDetails);

            bool foundApplicableSegment = false;
            foreach (dynamic trip in tripDetails)
            {
                foreach (dynamic product in trip.product)
                {
                    foreach (dynamic slice in product.slices)
                    {
                        foreach (dynamic segment in slice.segments)
                        {
                            if (!IsVsTomorrowSegment(segment))
                                continue;

                            foundApplicableSegment = true;
                            DateTimeOffset departure = DateTimeOffset.Parse(segment.scheduledDepartureUtc.ToString());
                            Assert.NotNull(segment.popsOpenDateTimeUtc);
                            DateTimeOffset popsOpen = DateTimeOffset.Parse(segment.popsOpenDateTimeUtc.ToString());
                            Assert.Equal(departure.AddDays(-7), popsOpen);
                        }
                    }
                }
            }

            Assert.True(foundApplicableSegment, "No VS-operated flight segment departing tomorrow was found in the OCI TripDetails response.");
        }

        private static bool IsVsTomorrowSegment(dynamic segment)
        {
            string operatingCarrier = segment.operatingCarrier?.carrierCode?.ToString() ?? string.Empty;
            DateTimeOffset departure = DateTimeOffset.Parse(segment.scheduledDepartureUtc.ToString());
            return operatingCarrier.Equals("VS", StringComparison.OrdinalIgnoreCase)
                && departure.UtcDateTime.Date == DateTime.UtcNow.Date.AddDays(1);
        }
    }
}
