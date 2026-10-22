using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.Holidays.GraphQL
{
    /// <summary>
    /// Mixed-shape <c>TripDetails</c> tests: a single GraphQL call carrying both a
    /// PNR-based lookup (Trips/MobileAppPnrs) AND a Holidays booking-reference
    /// lookup (vHolsBookings). The factory accepts a <see cref="Requests.Factory.TripDetailEntry"/>
    /// list with per-entry nullable fields so a single request can mix the two
    /// identification shapes the GraphQL endpoint supports.
    /// </summary>
    public class TripDetailsWithHolidays
    {
        public string env = Config.Environment;

        [SkippableTheory]
        [Trait("Category", Traits.RetrieveHolidayInTripListRegression)]
        [InlineData(JourneyType.OneWay, ReservationType.REVENUE)]
        public async Task TripDetails_PnrAndHolidayBookingReference_ReturnsBothTrips(JourneyType journeyType, ReservationType reservationType)
        {
            // PNR-based entry â€” current-week PNR from MobileAppPnrs storage table.
            string journeyTypeString = journeyType.ToString();
            string reservationTypeString = reservationType.ToString();

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable
                .RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString);

            if (pnrDetails.TryGetValue("error", out string pnrError) && !string.IsNullOrWhiteSpace(pnrError))
            {
                TestDataPrerequisiteHelper.Skip($"Test skipped: PNR lookup failed. {pnrError}");
                return;
            }

            string pnr = pnrDetails["pnr"];
            string pnrFirstName = pnrDetails["firstName"];
            string pnrLastName = pnrDetails["lastName"];
            string pnrCreatedDate = DateTime.Parse(pnrDetails["timestamp"]).ToString("yyyy-MM-dd");

            Assert.True(
                !string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(pnrFirstName) && !string.IsNullOrEmpty(pnrLastName),
                "PNR, first name and last name must all be present for the PNR entry");

            // Holidays-booking entry â€” active booking with a future departure date.
            // Filters mirror RetrieveHolidayBookingTests.cs:43 (FIRM/DEFINITE/FULLY PAID
            // + HTL+FLT required items) so we land on a fully bookable holiday.
            string futureDeparture = HolidaysHelper.GenerateFutureDepartureDate();
            string[] holidayOptionStatus = new[] { "FIRM", "DEFINITE", "FULLY PAID" };
            List<string> requiredItems = new() { "HTL", "FLT" };

            Dictionary<string, string> holidayDetails = RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(
                optionStatus: holidayOptionStatus,
                requiredItems: requiredItems,
                departureDate: futureDeparture);

            string holidayDebugContext = HolidaysHelper.FormatHolidayBookingContext(holidayDetails);
            if (holidayDetails.TryGetValue("error", out string holidayError) && !string.IsNullOrWhiteSpace(holidayError))
            {
                TestDataPrerequisiteHelper.Skip($"Test skipped: Holiday booking lookup failed. {holidayError}. {holidayDebugContext}");
                return;
            }

            string bookingReference = holidayDetails["bookingReference"];
            string holidayDateOfBirth = HolidaysHelper.NormalizeDateOfBirth(holidayDetails["dateOfBirth"]);
            string holidaySurname = HolidaysHelper.ExtractSurname(holidayDetails["leadName"]);
            holidayDebugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, holidayDetails["leadName"], holidayDateOfBirth);

            Assert.True(
                !string.IsNullOrEmpty(bookingReference) && !string.IsNullOrEmpty(holidayDateOfBirth) && !string.IsNullOrEmpty(holidaySurname),
                $"Booking reference, dateOfBirth and surname (from leadName) must all be present for the Holidays entry. {holidayDebugContext}");

            // Build the mixed-shape TripDetails request.
            List<Requests.Factory.TripDetailEntry> entries = new()
            {
                // PNR-style entry â€” PNR locator + given names + surname (+ createdDate).
                new Requests.Factory.TripDetailEntry
                {
                    PnrLocator = pnr,
                    GivenNames = pnrFirstName,
                    Surname = pnrLastName,
                    CreatedDate = pnrCreatedDate
                },
                // Holidays-style entry â€” bookingReference + dateOfBirth + surname.
                new Requests.Factory.TripDetailEntry
                {
                    BookingReference = bookingReference,
                    DateOfBirth = holidayDateOfBirth,
                    Surname = holidaySurname
                }
            };

            HttpResponseMessage response = GraphQlEndpoint.TripDetails(env, entries).EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(responseBody), $"TripDetails response body was empty. {holidayDebugContext}");
            JObject tripDetailsResult = JObject.Parse(responseBody);

            // The TripDetails query returns an array under data.TripDetails â€” one entry
            // per request element, in request order. We don't rely on the order: locate
            // each returned trip by the echo-back field that's unique to its lookup shape.
            JToken tripDetails = tripDetailsResult.RequireToken("data.TripDetails");
            Assert.True(tripDetails.Count() == 2,
                $"Expected 2 trips in response (one per request entry) but got {tripDetails.Count()}. {holidayDebugContext}");

            // Discovery phase uses SelectToken (a missing match is meaningful â€” it means
            // "this isn't the trip we're looking for"). Once located, all assertions use
            // RequireToken so a missing field surfaces as a JsonException naming the
            // path, not a silent null comparison further down (see AGENTS.md Â§ Design
            // conventions: Response JSON navigation).
            JObject? pnrTrip = tripDetails
                .Cast<JObject>()
                .FirstOrDefault(t => pnr.Is(t.SelectToken("tripSearchRequest.pnrLocator")?.ToString()));
            Assert.True(pnrTrip != null, $"PNR trip entry not found in TripDetails response. {holidayDebugContext}");

            JObject? holidayTrip = tripDetails
                .Cast<JObject>()
                .FirstOrDefault(t => bookingReference.Is(t.SelectToken("tripSearchRequest.bookingReference")?.ToString()));
            Assert.True(holidayTrip != null, $"Holiday trip entry not found in TripDetails response. {holidayDebugContext}");

            // PNR trip echoes back the surname we sent. .Is() is the codebase idiom
            // for case-insensitive string compare (see AGENTS.md Â§ Design conventions).
            string actualPnrSurname = pnrTrip.RequireToken("tripSearchRequest.surname").ToString();
            Assert.True(
                pnrLastName.Is(actualPnrSurname),
                $"PNR entry surname echo mismatch. Expected: {pnrLastName} Actual: {actualPnrSurname}");

            // Holidays trip echoes back the surname and dateOfBirth.
            string actualHolidaySurname = holidayTrip.RequireToken("tripSearchRequest.surname").ToString();
            Assert.True(
                holidaySurname.Is(actualHolidaySurname),
                $"Holiday entry surname echo mismatch. Expected: {holidaySurname} Actual: {actualHolidaySurname}. {holidayDebugContext}");
            string actualHolidayDob = holidayTrip.RequireToken("tripSearchRequest.dateOfBirth").ToString();
            Assert.True(holidayDateOfBirth.Equals(actualHolidayDob, StringComparison.Ordinal), $"Holiday entry dateOfBirth echo mismatch. Expected: {holidayDateOfBirth} Actual: {actualHolidayDob}. {holidayDebugContext}");
        }
    }
}
