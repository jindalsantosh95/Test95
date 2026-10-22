namespace VAA.EAPI.Automation.Trips.Helpers
{
    public static class TestDataPrerequisiteHelper
    {
        public static void Skip(string reason) => Xunit.Skip.If(true, reason);

        public static void ValidatePnrDetailsOrSkip(Dictionary<string, string> pnrDetails)
        {
            if (pnrDetails == null)
            {
                Skip("Test skipped: prerequisite PNR data is not available in storage.");
                return;
            }

            string confirmationNum = pnrDetails.TryGetValue("pnr", out string foundPnr) ? foundPnr ?? string.Empty : string.Empty;
            string givenNames = pnrDetails.TryGetValue("firstName", out string foundFirstName) ? foundFirstName ?? string.Empty : string.Empty;
            string surname = pnrDetails.TryGetValue("lastName", out string foundLastName) ? foundLastName ?? string.Empty : string.Empty;
            string debugContext = RequestHelper.FormatTripsBookingContext(confirmationNum, givenNames, surname);

            if (pnrDetails.TryGetValue("error", out string error) && !string.IsNullOrWhiteSpace(error))
            {
                Skip($"Test skipped: prerequisite PNR data unavailable. {error}. {debugContext}");
                return;
            }

            bool hasUsableData = pnrDetails.TryGetValue("pnr", out string pnr) && !string.IsNullOrWhiteSpace(pnr) &&
                                 pnrDetails.TryGetValue("firstName", out string firstName) && !string.IsNullOrWhiteSpace(firstName) &&
                                 pnrDetails.TryGetValue("lastName", out string lastName) && !string.IsNullOrWhiteSpace(lastName) &&
                                 pnrDetails.TryGetValue("passengersCount", out string passengersCount) &&
                                 int.TryParse(passengersCount, out int paxCount) && paxCount > 0;

            if (!hasUsableData)
            {
                Skip($"Test skipped: prerequisite PNR data is incomplete or invalid in storage. {debugContext}");
            }
        }

        public static void ValidateHolidayDetailsOrSkip(Dictionary<string, string> bookingDetails, bool requireItems = true, bool requireDepartureDate = true)
        {
            if (bookingDetails == null)
            {
                Skip("Test skipped: prerequisite holiday booking data is not available in storage.");
                return;
            }

            string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingDetails);

            if (bookingDetails.TryGetValue("error", out string error) && !string.IsNullOrWhiteSpace(error))
            {
                Skip($"Test skipped: prerequisite holiday booking data unavailable. {error}. {debugContext}");
                return;
            }

            bool hasUsableData = bookingDetails.TryGetValue("bookingReference", out string bookingReference) && !string.IsNullOrWhiteSpace(bookingReference) &&
                                 bookingDetails.TryGetValue("dateOfBirth", out string dateOfBirth) && !string.IsNullOrWhiteSpace(dateOfBirth) &&
                                 bookingDetails.TryGetValue("leadName", out string leadName) && !string.IsNullOrWhiteSpace(leadName);

            if (requireItems)
            {
                hasUsableData = hasUsableData && bookingDetails.TryGetValue("items", out string items) && !string.IsNullOrWhiteSpace(items);
            }

            if (requireDepartureDate)
            {
                hasUsableData = hasUsableData && bookingDetails.TryGetValue("departureDate", out string departureDate) && !string.IsNullOrWhiteSpace(departureDate);
            }

            if (!hasUsableData)
            {
                Skip($"Test skipped: prerequisite holiday booking data is incomplete or invalid in storage. {debugContext}");
            }
        }
    }
}
