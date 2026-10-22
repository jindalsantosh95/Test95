using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.Holidays.GraphQL
{
    public class RetrieveHolidayBookingTests
    {
        public string env = Config.Environment;
        private const string CouldNotAddTripMessage = "We couldn’t add your trip at the moment. Please try again later";
        private const string CouldNotAddTripErrorCode = "MA13";
        private const string TripCannotBeAddedMessage = "This type of trip can't be added in the app just yet. Please continue on our website.";
        private const string TripCannotBeAddedErrorCode = "MA14";
        private const string CancelledBookingStatus = "CANCELLED";
        private static readonly HashSet<string> HiddenExtraElementGroups = new(StringComparer.OrdinalIgnoreCase)
        {
            "FCHIDDEN", "OWA", "RCD", "RETAILBOOK", "ATOLLEVY", "ADVANCE", "LOWDEP", "CCBF", "STBF", "BOOKFEE", "HREPFEES"
        };


        public static IEnumerable<object[]> RequiredItemCombinations()
        {
            yield return new object[] { new[] { HolidayBookingItem.GEN, HolidayBookingItem.HTL, HolidayBookingItem.FLT } };
            yield return new object[] { new[] { HolidayBookingItem.GEN, HolidayBookingItem.HTL, HolidayBookingItem.TRS, HolidayBookingItem.FLT } };
            yield return new object[] { new[] { HolidayBookingItem.GEN, HolidayBookingItem.HTL, HolidayBookingItem.FLT, HolidayBookingItem.CAR } };
        }

        [SkippableTheory]
        [Trait("Category", Traits.RetrieveHolidayBookingRegression)]
        [Trait("Category", GlobalTraits.MobileAppsSanityTests)]
        [MemberData(nameof(RequiredItemCombinations))]
        public async Task RetrieveHolidayBooking_ReturnsExpectedBooking(HolidayBookingItem[] requiredItems)
        {

            #region - pre-requisite for fetching the holidays booking from the storage table

            List<string> requiredItemNames = requiredItems.Select(item => item.ToString()).ToList();

            // Generate departure date greater than today's date in ISO format (2025-04-25T00:00:00Z)
            string departureDate = HolidaysHelper.GenerateFutureDepartureDate();

            string[] optionStatus = new[] { "FIRM", "DEFINITE", "FULLY PAID" };

            Dictionary<string, string> bookingDetails = RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(optionStatus: optionStatus, requiredItems: requiredItemNames, departureDate: departureDate);

            ValidateHolidayDetails(bookingDetails);

            string bookingReference = bookingDetails["bookingReference"];
            string dateOfBirth = HolidaysHelper.NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string leadName = bookingDetails["leadName"];
            string surname = HolidaysHelper.ExtractSurname(leadName);
            string itemsCsv = bookingDetails["items"];
            string bookingStatus = bookingDetails["bookingStatus"];
            string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, leadName, dateOfBirth);

            List<HolidayBookingItem> items = ParseItems(itemsCsv, debugContext);
            Assert.True(items.Count > 0, $"Items list should contain at least one valid value. {debugContext}");
            foreach (HolidayBookingItem requiredItem in requiredItems)
            {
                Assert.True(items.Contains(requiredItem), $"Expected items list to contain {requiredItem} but it did not. {debugContext}");
            }

            #endregion

            #region - Build request payload

            string usingString = "HOLIDAYS";

            HttpResponseMessage response = GraphQlEndpoint.RetrieveHolidayBooking(env, usingString, bookingReference, dateOfBirth, surname)
                .EnsureGQLResponseSuccess();
            JObject HolidayDetailsResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            #endregion

            #region - Validations

            Assert.True(DynamicHelper.HasProperty(HolidayDetailsResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(HolidayDetailsResult["data"], "retrieveHolidayBooking"), $"Response should contain retrieveHolidayBooking property. {debugContext}");

            JObject retrieveHolidayBooking = HolidayDetailsResult["data"]["retrieveHolidayBooking"] as JObject;
            Assert.True(retrieveHolidayBooking != null, $"retrieveHolidayBooking should not be null. {debugContext}");

            Assert.True(!string.IsNullOrEmpty(bookingStatus), $"Booking status from storage should not be empty. {debugContext}");
            Assert.True(retrieveHolidayBooking["status"].ToString().Contains(bookingStatus, StringComparison.OrdinalIgnoreCase), $"Expected booking status to be {bookingStatus} but got {retrieveHolidayBooking["status"].ToString()}. {debugContext}");

            #endregion
        }

        [SkippableFact]
        [Trait("Category", Traits.RetrieveHolidayBookingRegression)]
        [Trait("Category", GlobalTraits.MobileAppsSanityTests)]
        public async Task RetrieveHolidayBooking_DepartureDateOlderThan7Days_ReturnsMA14Error()
        {
            #region - pre-requisite for fetching the holidays booking with past departure date from the storage table

            List<string> requiredItemNames = new[] { HolidayBookingItem.GEN.ToString(), HolidayBookingItem.HTL.ToString(), HolidayBookingItem.FLT.ToString() }.ToList();

            // Generate departure date less than 7 days in the past in ISO format
            string pastDepartureDate = HolidaysHelper.GeneratePastDepartureDate();

            Dictionary<string, string> bookingDetails = RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(requiredItems: requiredItemNames, pastDepartureDate: pastDepartureDate);

            string debugContextFromStorage = HolidaysHelper.FormatHolidayBookingContext(bookingDetails);
            Assert.True(bookingDetails.ContainsKey("bookingReference") && !string.IsNullOrEmpty(bookingDetails["bookingReference"]), $"Booking reference should not be null or empty. {debugContextFromStorage}");

            string bookingReference = bookingDetails["bookingReference"];
            string dateOfBirth = HolidaysHelper.NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string leadName = bookingDetails["leadName"];
            string surname = HolidaysHelper.ExtractSurname(leadName);
            string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, leadName, dateOfBirth);
            string retrievedDepartureDate = bookingDetails["departureDate"];

            Assert.True(!string.IsNullOrEmpty(retrievedDepartureDate), $"Departure date should not be null or empty. {debugContext}");
            Assert.True(retrievedDepartureDate.CompareTo(pastDepartureDate) < 0, $"Expected departure date to be less than {pastDepartureDate} but got {retrievedDepartureDate}. {debugContext}");

            #endregion

            #region - Build request payload and validate error response

            string usingString = "HOLIDAYS";

            HttpResponseMessage response = GraphQlEndpoint.RetrieveHolidayBooking(env, usingString, bookingReference, dateOfBirth, surname);
            JObject result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            #endregion

            #region - Validations

            // Validate that response contains errors array
            Assert.True(DynamicHelper.HasProperty(result, "errors"), $"Response should contain errors array for past departure date. {debugContext}");
            Assert.True(JArray.Parse(result["errors"].ToString()).Count > 0, $"Errors array should not be empty. {debugContext}");

            // Validate error details
            JObject errorObject = (JObject)result["errors"][0];
            Assert.True(errorObject.ContainsKey("message"), $"Error should contain message property. {debugContext}");
            Assert.True(errorObject["message"].ToString().Contains("This type of trip can't be added in the app just yet. Please continue on our website"), $"Error message should indicate - This type of trip can't be added in the app just yet. Please continue on our website. {debugContext}");

            // Validate error code MA14 in extensions
            Assert.True(errorObject.ContainsKey("extensions"), $"Error should contain extensions property. {debugContext}");
            JObject extensions = (JObject)errorObject["extensions"];
            Assert.True(extensions.ContainsKey("code"), $"Extensions should contain code property. {debugContext}");
            Assert.True(extensions["code"].ToString().Equals("MA14", StringComparison.OrdinalIgnoreCase), $"Expected error code MA14 but got {extensions["code"].ToString()}. {debugContext}");

            // Validate that data.retrieveHolidayBooking is null
            Assert.True(DynamicHelper.HasProperty(result, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(result["data"]["retrieveHolidayBooking"] == null || result["data"]["retrieveHolidayBooking"].Type == JTokenType.Null || result["data"]["retrieveHolidayBooking"].ToString() == "null", $"retrieveHolidayBooking should be null for past dates. {debugContext}");

            #endregion
        }

        public static IEnumerable<object[]> InvalidInputScenarios()
        {
            yield return new object[] { "IncorrectBookingReference", CouldNotAddTripMessage, CouldNotAddTripErrorCode };
            yield return new object[] { "IncorrectLastName", CouldNotAddTripMessage, CouldNotAddTripErrorCode };
            yield return new object[] { "IncorrectLeadPaxDob", CouldNotAddTripMessage, CouldNotAddTripErrorCode };
        }

        [SkippableTheory]
        [Trait("Category", Traits.RetrieveHolidayBookingRegression)]
        [MemberData(nameof(InvalidInputScenarios))]
        public async Task RetrieveHolidayBooking_InvalidInputs_ReturnsExpectedError(string scenarioName, string expectedMessage, string expectedCode)
        {
            Dictionary<string, string> bookingDetails = GetValidHolidayBookingDetails();

            string bookingReference = bookingDetails["bookingReference"];
            string dateOfBirth = HolidaysHelper.NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string surname = HolidaysHelper.ExtractSurname(bookingDetails["leadName"]);
            string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, bookingDetails["leadName"], dateOfBirth);

            switch (scenarioName)
            {
                case "IncorrectBookingReference":
                    bookingReference = MakeInvalidBookingReference(bookingReference);
                    debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, bookingDetails["leadName"], dateOfBirth);
                    break;
                case "IncorrectLastName":
                    surname = MakeInvalidSurname(surname);
                    debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, bookingDetails["leadName"], dateOfBirth);
                    break;
                case "IncorrectLeadPaxDob":
                    dateOfBirth = MakeInvalidDateOfBirth(dateOfBirth);
                    debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, bookingDetails["leadName"], dateOfBirth);
                    break;
                default:
                    Assert.True(false, $"Unknown scenarioName '{scenarioName}'. {debugContext}");
                    break;
            }

            string usingString = "HOLIDAYS";

            HttpResponseMessage response = GraphQlEndpoint.RetrieveHolidayBooking(env, usingString, bookingReference, dateOfBirth, surname);
            JObject result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            AssertRetrieveHolidayBookingError(result, expectedMessage, expectedCode, debugContext);
        }

        [SkippableFact]
        [Trait("Category", Traits.RetrieveHolidayBookingRegression)]
        [Trait("Category", GlobalTraits.MobileAppsSanityTests)]
        public async Task RetrieveHolidayBooking_VirginHolidaysCruisesCompany_ReturnsMA14Error()
        {
            string company = "Virgin Holidays Cruises";

            Dictionary<string, string> bookingDetails = RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(
                company: company);

            ValidateHolidayDetails(bookingDetails);

            string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingDetails);
            Assert.True(bookingDetails.ContainsKey("company"), $"Precondition failed: expected booking details to include a 'company' value. {debugContext}");
            Assert.True(bookingDetails["company"].Is(company), $"Precondition failed: expected Company in storage to be '{company}' but got '{bookingDetails["company"]}'. {debugContext}");

            string bookingReference = bookingDetails["bookingReference"];
            string dateOfBirth = HolidaysHelper.NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string surname = HolidaysHelper.ExtractSurname(bookingDetails["leadName"]);
            debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, bookingDetails["leadName"], dateOfBirth);

            string usingString = "HOLIDAYS";

            HttpResponseMessage response = GraphQlEndpoint.RetrieveHolidayBooking(env, usingString, bookingReference, dateOfBirth, surname);
            JObject result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            AssertRetrieveHolidayBookingError(result, TripCannotBeAddedMessage, TripCannotBeAddedErrorCode, debugContext);
        }

        [SkippableFact]
        [Trait("Category", Traits.RetrieveHolidayBookingRegression)]
        public async Task RetrieveHolidayBooking_CancelledBooking_ReturnsMA14()
        {
            Dictionary<string, string> bookingDetails = RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(
                bookingStatus: CancelledBookingStatus);

            ValidateHolidayDetails(bookingDetails);
            string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingDetails);
            Assert.True(bookingDetails.ContainsKey("bookingStatus"), $"BookingStatus should be present in booking details. {debugContext}");

            string bookingReference = bookingDetails["bookingReference"];
            string dateOfBirth = HolidaysHelper.NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string surname = HolidaysHelper.ExtractSurname(bookingDetails["leadName"]);
            string bookingStatus = bookingDetails["bookingStatus"];
            string itemsCsv = bookingDetails["items"];
            debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, bookingDetails["leadName"], dateOfBirth);

            Assert.True(bookingStatus.Is(CancelledBookingStatus), $"Expected BookingStatus in storage to be {CancelledBookingStatus} but got '{bookingStatus}'. {debugContext}");
            Assert.True(!string.IsNullOrEmpty(itemsCsv), $"Cancelled booking should still have items populated in storage. {debugContext}");

            string usingString = "HOLIDAYS";

            HttpResponseMessage response = GraphQlEndpoint.RetrieveHolidayBooking(env, usingString, bookingReference, dateOfBirth, surname);
            JObject result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            AssertRetrieveHolidayBookingError(result, TripCannotBeAddedMessage, TripCannotBeAddedErrorCode, debugContext);
        }

        [SkippableTheory]
        [Trait("Category", Traits.RetrieveHolidayBookingRegression)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task RetrieveHolidayBooking_SurnameAndDobWithLeadingTrailingSpaces_ReturnsSuccess(bool addSpacesToSurname, bool addSpacesToDateOfBirth)
        {
            Dictionary<string, string> bookingDetails = GetValidHolidayBookingDetails();

            string bookingReference = bookingDetails["bookingReference"];
            string normalizedDateOfBirth = HolidaysHelper.NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string leadName = bookingDetails["leadName"];
            string surname = HolidaysHelper.ExtractSurname(leadName);
            string bookingStatus = bookingDetails["bookingStatus"];

            string requestSurname = addSpacesToSurname ? $"  {surname}  " : surname;
            string requestDateOfBirth = addSpacesToDateOfBirth ? $"  {normalizedDateOfBirth}  " : normalizedDateOfBirth;

            string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, leadName, requestDateOfBirth);
            string usingString = "HOLIDAYS";

            HttpResponseMessage response = GraphQlEndpoint.RetrieveHolidayBooking(env, usingString, bookingReference, requestDateOfBirth, requestSurname)
                .EnsureGQLResponseSuccess();
            JObject result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(result, "errors"), $"Expected no errors for surname/dateOfBirth values with surrounding spaces. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(result, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(result["data"], "retrieveHolidayBooking"), $"Response should contain retrieveHolidayBooking property. {debugContext}");

            JObject retrieveHolidayBooking = result["data"]["retrieveHolidayBooking"] as JObject;
            Assert.True(retrieveHolidayBooking != null, $"retrieveHolidayBooking should not be null. {debugContext}");
            Assert.True(!string.IsNullOrEmpty(bookingStatus), $"Booking status from storage should not be empty. {debugContext}");
            Assert.True(retrieveHolidayBooking["status"].ToString().Contains(bookingStatus, StringComparison.OrdinalIgnoreCase),
                $"Expected booking status to contain '{bookingStatus}' but got '{retrieveHolidayBooking["status"]}'. {debugContext}");
        }

        [SkippableFact]
        [Trait("Category", Traits.RetrieveHolidayBookingRegression)]
        public async Task RetrieveHolidayBooking_ShouldNotReturnHiddenExtraElementGroups()
        {
            Dictionary<string, string> bookingDetails = GetValidHolidayBookingDetails();

            string bookingReference = bookingDetails["bookingReference"];
            string dateOfBirth = HolidaysHelper.NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string leadName = bookingDetails["leadName"];
            string surname = HolidaysHelper.ExtractSurname(leadName);
            string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, leadName, dateOfBirth);

            string usingString = "HOLIDAYS";

            HttpResponseMessage response = GraphQlEndpoint.RetrieveHolidayBooking(env, usingString, bookingReference, dateOfBirth, surname)
                .EnsureGQLResponseSuccess();
            JObject result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(result, "errors"), $"Expected no errors in retrieveHolidayBooking response. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(result, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(result["data"], "retrieveHolidayBooking"), $"Response should contain retrieveHolidayBooking property. {debugContext}");

            JToken retrieveHolidayBooking = result["data"]["retrieveHolidayBooking"];
            Assert.True(retrieveHolidayBooking != null && retrieveHolidayBooking.Type != JTokenType.Null, $"retrieveHolidayBooking should not be null. {debugContext}");

            List<string> hiddenExtraViolations = FindHiddenExtraViolations(retrieveHolidayBooking);
            Assert.True(hiddenExtraViolations.Count == 0,
                $"Hidden extras should not be returned in GraphQL response. Violations: {string.Join(" | ", hiddenExtraViolations)}. {debugContext}");
        }

        [SkippableFact]
        [Trait("Category", Traits.RetrieveHolidayBookingRegression)]
        public async Task RetrieveHolidayBooking_AirlineProducts_HaveSequentialSliceIds()
        {
            Dictionary<string, string> bookingDetails = GetValidHolidayBookingDetails();

            string bookingReference = bookingDetails["bookingReference"];
            string dateOfBirth = HolidaysHelper.NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string leadName = bookingDetails["leadName"];
            string surname = HolidaysHelper.ExtractSurname(leadName);
            string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, leadName, dateOfBirth);

            string usingString = "HOLIDAYS";

            HttpResponseMessage response = GraphQlEndpoint.RetrieveHolidayBooking(env, usingString, bookingReference, dateOfBirth, surname)
                .EnsureGQLResponseSuccess();
            JObject result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(result, "errors"), $"Expected no errors in retrieveHolidayBooking response. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(result, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(result["data"], "retrieveHolidayBooking"), $"Response should contain retrieveHolidayBooking property. {debugContext}");

            JObject retrieveHolidayBooking = result["data"]["retrieveHolidayBooking"] as JObject;
            Assert.True(retrieveHolidayBooking != null, $"retrieveHolidayBooking should not be null. {debugContext}");

            JArray productArray = retrieveHolidayBooking["product"] as JArray;
            Assert.True(productArray != null && productArray.HasValues, $"retrieveHolidayBooking.product should not be null or empty. {debugContext}");

            int airlineIndex = 0;
            foreach (JToken product in productArray)
            {
                string bookingType = product["bookingType"]?.ToString() ?? string.Empty;
                if (!bookingType.Equals("AIRLINE", StringComparison.OrdinalIgnoreCase))
                    continue;

                airlineIndex++;

                string actualId = product["slices"]?[0]?["id"]?.ToString() ?? string.Empty;
                string expectedId = airlineIndex.ToString(CultureInfo.InvariantCulture);
                string sortByDate = product["sortByDate"]?.ToString() ?? string.Empty;
                string originCode = product["slices"]?[0]?["origin"]?["code"]?.ToString() ?? string.Empty;
                string destinationCode = product["slices"]?[0]?["destination"]?["code"]?.ToString() ?? string.Empty;

                Assert.True(!string.IsNullOrEmpty(actualId),
                    $"AIRLINE product id should not be null or empty. bookingType={bookingType}, sortByDate={sortByDate}, origin={originCode}, destination={destinationCode}. {debugContext}");

                Assert.True(actualId.Equals(expectedId, StringComparison.Ordinal),
                    $"Expected AIRLINE product id '{expectedId}' but got '{actualId}'. bookingType={bookingType}, sortByDate={sortByDate}, origin={originCode}, destination={destinationCode}. {debugContext}");
            }

            Assert.True(airlineIndex > 0, $"Expected at least one AIRLINE product in retrieveHolidayBooking.product. {debugContext}");
        }

        [SkippableTheory]
        [Trait("Category", Traits.RetrieveHolidayBookingRegression)]
        [MemberData(nameof(RequiredItemCombinations))]
        public async Task RetrieveHolidayBooking_ValidOptionStatuses_DocumentDetailsIs14DaysBeforeDeparture(HolidayBookingItem[] requiredItems)
        {
            string[] preferredOptionStatuses = new[] { "FIRM", "DEFINITE", "FULLY PAID" };
            foreach (string preferredOptionStatus in preferredOptionStatuses)
            {
                bool foundBooking = TryGetValidHolidayBookingDetails(requiredItems, out Dictionary<string, string> bookingDetails, preferredOptionStatus);
                string selectedOptionStatus = preferredOptionStatus;

                if (!foundBooking)
                {
                    foundBooking = TryGetValidHolidayBookingDetails(requiredItems, out bookingDetails);
                    selectedOptionStatus = "fallback-any";
                }

                Assert.True(foundBooking,
                    $"Could not find a valid future holiday booking for requiredItems='{string.Join(",", requiredItems)}' using preferred statuses ({string.Join(",", preferredOptionStatuses)}) or fallback search.");

                ValidateHolidayDetails(bookingDetails);

                string bookingReference = bookingDetails["bookingReference"];
                string dateOfBirth = HolidaysHelper.NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
                string leadName = bookingDetails["leadName"];
                string surname = HolidaysHelper.ExtractSurname(leadName);
                string debugContext = $"{HolidaysHelper.FormatHolidayBookingContext(bookingReference, leadName, dateOfBirth)}, optionStatus='{selectedOptionStatus}', requiredItems='{string.Join(",", requiredItems)}'";

                string usingString = "HOLIDAYS";

                HttpResponseMessage response = GraphQlEndpoint.RetrieveHolidayBooking(env, usingString, bookingReference, dateOfBirth, surname)
                    .EnsureGQLResponseSuccess();
                JObject result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                Assert.True(!DynamicHelper.HasProperty(result, "errors"), $"Expected no errors in retrieveHolidayBooking response. {debugContext}");
                Assert.True(DynamicHelper.HasProperty(result, "data"), $"Response should contain data property. {debugContext}");
                Assert.True(DynamicHelper.HasProperty(result["data"], "retrieveHolidayBooking"), $"Response should contain retrieveHolidayBooking property. {debugContext}");

                JObject retrieveHolidayBooking = result["data"]["retrieveHolidayBooking"] as JObject;
                Assert.True(retrieveHolidayBooking != null, $"retrieveHolidayBooking should not be null. {debugContext}");

                string departureDateRaw = retrieveHolidayBooking.RequireToken("travelDates.departureDate")?.ToString() ?? string.Empty;
                string ticketsDocumentAvailableTimeRaw = retrieveHolidayBooking.RequireToken("documentDetails.ticketsDocumentAvailableTime")?.ToString() ?? string.Empty;

                Assert.True(TryParseBookingDateTime(departureDateRaw, out DateTime departureDate),
                    $"travelDates.departureDate should be a valid datetime but got '{departureDateRaw}'. {debugContext}");
                Assert.True(TryParseBookingDateTime(ticketsDocumentAvailableTimeRaw, out DateTime ticketsDocumentAvailableTime),
                    $"documentDetails.ticketsDocumentAvailableTime should be a valid datetime but got '{ticketsDocumentAvailableTimeRaw}'. {debugContext}");

                DateTime expectedTicketsAvailableDate = departureDate.Date.AddDays(-14);
                Assert.True(ticketsDocumentAvailableTime.Date == expectedTicketsAvailableDate,
                    $"Expected documentDetails.ticketsDocumentAvailableTime to be 14 days before departure. departureDate='{departureDate:yyyy-MM-dd}', expected='{expectedTicketsAvailableDate:yyyy-MM-dd}', actual='{ticketsDocumentAvailableTime:yyyy-MM-dd}'. {debugContext}");

                break;
            }
        }

        private void ValidateHolidayDetails(Dictionary<string, string> bookingDetails)
            => TestDataPrerequisiteHelper.ValidateHolidayDetailsOrSkip(bookingDetails);

        private Dictionary<string, string> GetValidHolidayBookingDetails()
        {
            return GetValidHolidayBookingDetails(new[] { HolidayBookingItem.GEN, HolidayBookingItem.HTL, HolidayBookingItem.FLT }, "FIRM", "DEFINITE", "FULLY PAID");
        }

        private Dictionary<string, string> GetValidHolidayBookingDetails(HolidayBookingItem[] requiredItems, params string[] optionStatus)
        {
            List<string> requiredItemNames = requiredItems.Select(item => item.ToString()).ToList();

            string departureDate = HolidaysHelper.GenerateFutureDepartureDate();
            string[] statuses = optionStatus != null && optionStatus.Length > 0 ? optionStatus : new[] { "FIRM", "DEFINITE", "FULLY PAID" };

            Dictionary<string, string> bookingDetails = null;
            for (int attempt = 1; attempt <= 5; attempt++)
            {
                bookingDetails = RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(optionStatus: statuses, requiredItems: requiredItemNames, departureDate: departureDate);
                ValidateHolidayDetails(bookingDetails);

                if (IsFutureDepartureDate(bookingDetails["departureDate"]))
                    return bookingDetails;
            }

            string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingDetails);
            TestDataPrerequisiteHelper.Skip($"Test skipped: Unable to find a holiday booking with a future departure date after 5 attempts. Last departureDate='{bookingDetails?["departureDate"]}'. {debugContext}");
            return bookingDetails;
        }


        private bool TryGetValidHolidayBookingDetails(HolidayBookingItem[] requiredItems, out Dictionary<string, string> bookingDetails, params string[] optionStatus)
        {
            bookingDetails = null;

            List<string> requiredItemNames = requiredItems.Select(item => item.ToString()).ToList();
            string departureDate = HolidaysHelper.GenerateFutureDepartureDate();

            for (int attempt = 1; attempt <= 5; attempt++)
            {
                Dictionary<string, string> candidate = optionStatus != null && optionStatus.Length > 0
                    ? RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(optionStatus: optionStatus, requiredItems: requiredItemNames, departureDate: departureDate)
                    : RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(requiredItems: requiredItemNames, departureDate: departureDate);

                if (candidate == null || candidate.ContainsKey("error"))
                    continue;

                if (!candidate.ContainsKey("bookingReference") || candidate["bookingReference"].IsNullOrWhiteSpace())
                    continue;

                if (!candidate.ContainsKey("dateOfBirth") || candidate["dateOfBirth"].IsNullOrWhiteSpace())
                    continue;

                if (!candidate.ContainsKey("leadName") || candidate["leadName"].IsNullOrWhiteSpace())
                    continue;

                if (!candidate.ContainsKey("departureDate") || candidate["departureDate"].IsNullOrWhiteSpace())
                    continue;

                if (!IsFutureDepartureDate(candidate["departureDate"]))
                    continue;

                bookingDetails = candidate;
                return true;
            }

            return false;
        }

        private static bool IsFutureDepartureDate(string departureDate)
        {
            return !departureDate.IsNullOrWhiteSpace() && DateTime.TryParse(departureDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime parsed) && parsed >= DateTime.UtcNow;
        }

        private static bool TryParseBookingDateTime(string value, out DateTime parsed)
        {
            parsed = default;

            if (value.IsNullOrWhiteSpace())
                return false;

            string[] supportedFormats =
            {
                "yyyy-MM-ddTHH:mm:ss.fffZ",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd",
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy"
            };

            const DateTimeStyles styles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;

            return DateTime.TryParseExact(value, supportedFormats, CultureInfo.InvariantCulture, styles, out parsed)
                || DateTime.TryParseExact(value, supportedFormats, CultureInfo.GetCultureInfo("en-GB"), styles, out parsed)
                || DateTime.TryParse(value, CultureInfo.InvariantCulture, styles, out parsed)
                || DateTime.TryParse(value, CultureInfo.GetCultureInfo("en-GB"), styles, out parsed);
        }

        private static void AssertRetrieveHolidayBookingError(JObject result, string expectedMessage, string expectedCode, string debugContext)
        {
            Assert.True(DynamicHelper.HasProperty(result, "errors"), $"Response should contain errors array. {debugContext}");
            Assert.True(JArray.Parse(result["errors"].ToString()).Count > 0, $"Errors array should not be empty. {debugContext}");

            JObject errorObject = (JObject)result["errors"][0];
            Assert.True(errorObject.ContainsKey("message"), $"Error should contain message property. {debugContext}");

            string actualMessage = errorObject["message"]?.ToString() ?? string.Empty;
            Assert.True(NormalizeForComparison(actualMessage).Contains(NormalizeForComparison(expectedMessage), StringComparison.OrdinalIgnoreCase),
                $"Expected error message to contain '{expectedMessage}' but got '{actualMessage}'. {debugContext}");

            Assert.True(errorObject.ContainsKey("extensions"), $"Error should contain extensions property. {debugContext}");
            JObject extensions = (JObject)errorObject["extensions"];

            Assert.True(extensions.ContainsKey("code"), $"Extensions should contain code property. {debugContext}");
            Assert.True(extensions["code"]?.ToString().Equals(expectedCode, StringComparison.OrdinalIgnoreCase) == true,
                $"Expected error code {expectedCode} but got {extensions["code"]?.ToString()}. {debugContext}");

            if (extensions.ContainsKey("errorSource"))
            {
                string errorSource = extensions["errorSource"]?.ToString() ?? string.Empty;
                Assert.True(errorSource.Equals("RetrieveHolidayBooking", StringComparison.OrdinalIgnoreCase) || errorSource.Equals("retrieveHolidayBooking", StringComparison.OrdinalIgnoreCase),
                    $"Expected extensions.errorSource to be RetrieveHolidayBooking (or retrieveHolidayBooking) but got '{errorSource}'. {debugContext}");
            }

            if (extensions.ContainsKey("remote") && extensions["remote"] is JObject remote)
            {
                string remoteMessage = remote["message"]?.ToString() ?? string.Empty;
                if (!remoteMessage.IsNullOrWhiteSpace())
                {
                    Assert.True(NormalizeForComparison(remoteMessage).Contains(NormalizeForComparison(expectedMessage), StringComparison.OrdinalIgnoreCase),
                        $"Expected remote.message to contain '{expectedMessage}' but got '{remoteMessage}'. {debugContext}");
                }
            }

            if (extensions.ContainsKey("response") && extensions["response"]?.Type == JTokenType.String)
            {
                string responseText = extensions["response"]?.ToString() ?? string.Empty;
                if (!responseText.IsNullOrWhiteSpace())
                {
                    try
                    {
                        JArray responseArray = JArray.Parse(responseText);
                        if (responseArray.Count > 0 && responseArray[0] is JObject connectorError)
                        {
                            Assert.True(connectorError["code"]?.ToString().Equals(expectedCode, StringComparison.OrdinalIgnoreCase) == true,
                                $"Expected connector response code {expectedCode} but got {connectorError["code"]?.ToString()}. {debugContext}");

                            string connectorMessage = connectorError["message"]?.ToString() ?? string.Empty;
                            Assert.True(NormalizeForComparison(connectorMessage).Contains(NormalizeForComparison(expectedMessage), StringComparison.OrdinalIgnoreCase),
                                $"Expected connector response message to contain '{expectedMessage}' but got '{connectorMessage}'. {debugContext}");
                        }
                    }
                    catch (JsonReaderException)
                    {
                        // Ignore parsing issues - response payload can be non-JSON depending on downstream behavior
                    }
                }
            }

            Assert.True(DynamicHelper.HasProperty(result, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(result["data"]["retrieveHolidayBooking"] == null || result["data"]["retrieveHolidayBooking"].Type == JTokenType.Null || result["data"]["retrieveHolidayBooking"].ToString() == "null", $"retrieveHolidayBooking should be null for invalid input scenarios. {debugContext}");
        }

        private static string NormalizeForComparison(string input) =>
            (input ?? string.Empty).Replace('’', '\'').Replace('“', '"').Replace('”', '"');

        private static string MakeInvalidBookingReference(string bookingReference)
        {
            if (bookingReference.IsNullOrWhiteSpace())
                return "INVALIDREF";

            char last = bookingReference[^1];
            char replacement = char.ToUpperInvariant(last) == 'Z' ? 'Y' : 'Z';
            return bookingReference[..^1] + replacement;
        }

        private static string MakeInvalidSurname(string surname)
        {
            return surname.IsNullOrWhiteSpace() ? "WRONGSURNAME" : surname + "X";
        }

        private static string MakeInvalidDateOfBirth(string dateOfBirth)
        {
            return dateOfBirth.IsNullOrWhiteSpace()
                ? "1900-01-01"
                : DateTime.TryParseExact(dateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed)
                ? parsed.AddDays(1).ToString("yyyy-MM-dd")
                : DateTime.TryParse(dateOfBirth, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fallback)
                ? fallback.AddDays(1).ToString("yyyy-MM-dd")
                : dateOfBirth + "1";
        }

        private static List<HolidayBookingItem> ParseItems(string itemsCsv, string debugContext)
        {
            List<HolidayBookingItem> items = new();

            if (itemsCsv.IsNullOrWhiteSpace())
                return items;

            string normalizedItemsCsv = itemsCsv.Replace(';', ',');
            string[] tokens = normalizedItemsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string token in tokens)
            {
                if (!Enum.TryParse<HolidayBookingItem>(token, true, out HolidayBookingItem item))
                {
                    Assert.True(false, $"Unexpected item '{token}' found in storage table list. Update HolidayBookingItem enum if needed. {debugContext}");
                }
                items.Add(item);
            }

            return items;
        }

        private static List<string> FindHiddenExtraViolations(JToken root)
        {
            List<string> violations = [];

            foreach (JToken node in root.SelectTokens("$..*[?(@.bookingType)]"))
            {
                string bookingType = node["bookingType"]?.ToString() ?? string.Empty;
                if (!bookingType.Equals("EXTRA", StringComparison.OrdinalIgnoreCase))
                    continue;

                string elementGroup = node["extraDetails"]?["elementGroup"]?.ToString() ?? string.Empty;
                if (!elementGroup.IsNullOrWhiteSpace() && HiddenExtraElementGroups.Contains(elementGroup))
                {
                    string extraName = node["extraDetails"]?["name"]?.ToString() ?? string.Empty;
                    violations.Add($"Path={node.Path}, bookingType={bookingType}, elementGroup={elementGroup}, extraName={extraName}");
                }
            }

            return violations;
        }
    }
}
