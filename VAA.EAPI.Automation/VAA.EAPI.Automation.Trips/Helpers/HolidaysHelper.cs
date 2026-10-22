using System.Globalization;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Helpers
{
    public static class HolidaysHelper
    {
        public static string FormatHolidayBookingContext(string bookingReference, string leadName, string dateOfBirth)
        {
            string safeBookingReference = string.IsNullOrWhiteSpace(bookingReference) ? "<empty>" : bookingReference;
            string safeLeadName = string.IsNullOrWhiteSpace(leadName) ? "<empty>" : leadName;
            string safeDateOfBirth = string.IsNullOrWhiteSpace(dateOfBirth) ? "<empty>" : dateOfBirth;

            return $"[HolidayBooking] bookingReference='{safeBookingReference}', leadName='{safeLeadName}', dateOfBirth='{safeDateOfBirth}'";
        }

        public static string FormatHolidayBookingContext(Dictionary<string, string> bookingDetails)
        {
            if (bookingDetails == null)
                return FormatHolidayBookingContext("<null>", "<null>", "<null>");

            string bookingReference = bookingDetails.TryGetValue("bookingReference", out string foundBookingReference) ? (foundBookingReference ?? string.Empty) : string.Empty;
            string leadName = bookingDetails.TryGetValue("leadName", out string foundLeadName) ? (foundLeadName ?? string.Empty) : string.Empty;
            string dateOfBirth = bookingDetails.TryGetValue("dateOfBirth", out string foundDateOfBirth) ? (foundDateOfBirth ?? string.Empty) : string.Empty;

            return FormatHolidayBookingContext(bookingReference, leadName, dateOfBirth);
        }

        public static string ExtractSurname(string leadName)
        {
            if (string.IsNullOrWhiteSpace(leadName))
                return string.Empty;

            string[] parts = leadName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? string.Empty : parts[^1];
        }

        public static string NormalizeDateOfBirth(string dateOfBirth)
        {
            return string.IsNullOrWhiteSpace(dateOfBirth)
                ? string.Empty
                : DateTime.TryParseExact(dateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate)
                ? parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : DateTime.TryParse(dateOfBirth, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fallbackDate)
                ? fallbackDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : dateOfBirth;
        }

        public static string GenerateFutureDepartureDate(int daysInFuture = 7)
        {
            DateTime futureDateBase = DateTime.UtcNow.AddDays(daysInFuture);
            return futureDateBase.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        public static string GeneratePastDepartureDate(int daysInPast = 7)
        {
            DateTime pastDateBase = DateTime.UtcNow.AddDays(-daysInPast);
            return pastDateBase.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        public static void ValidateHolidayBookingDetails(Dictionary<string, string> bookingDetails)
        {
            Assert.NotNull(bookingDetails);
            string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingDetails);

            if (bookingDetails.TryGetValue("error", out string? error) && !string.IsNullOrWhiteSpace(error))
                Assert.Fail($"Test case failed: {error}. {debugContext}");
            Assert.True(bookingDetails.ContainsKey("bookingReference") && !string.IsNullOrWhiteSpace(bookingDetails["bookingReference"]), $"Booking reference should not be null or empty. {debugContext}");
            Assert.True(bookingDetails.ContainsKey("dateOfBirth") && !string.IsNullOrWhiteSpace(bookingDetails["dateOfBirth"]), $"DateOfBirth should not be null or empty. {debugContext}");
            Assert.True(bookingDetails.ContainsKey("leadName") && !string.IsNullOrWhiteSpace(bookingDetails["leadName"]), $"LeadName should not be null or empty. {debugContext}");
            Assert.True(bookingDetails.ContainsKey("items") && !string.IsNullOrWhiteSpace(bookingDetails["items"]), $"Items should not be null or empty. {debugContext}");
            Assert.True(bookingDetails.ContainsKey("departureDate") && !string.IsNullOrWhiteSpace(bookingDetails["departureDate"]), $"DepartureDate should not be null or empty. {debugContext}");
        }

        public static async Task<(JObject Result, string BookingReference, string DebugContext, string TripResourceId)> RetrieveHolidayBookingResultAsync(
            string env,
            string optionStatus,
            string bookingStatus,
            IEnumerable<string>? requiredItems = null)
        {
            List<string> resolvedRequiredItems = requiredItems?.ToList() ??
                new() { HolidayBookingItem.GEN.ToString(), HolidayBookingItem.HTL.ToString(), HolidayBookingItem.FLT.ToString() };
            string departureDate = GenerateFutureDepartureDate();

            Dictionary<string, string> bookingDetails = RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(
                optionStatus: [optionStatus],
                requiredItems: resolvedRequiredItems,
                departureDate: departureDate,
                bookingStatus: bookingStatus);

            TestDataPrerequisiteHelper.ValidateHolidayDetailsOrSkip(bookingDetails);

            string bookingReference = bookingDetails["bookingReference"];
            string dateOfBirth = NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string leadName = bookingDetails["leadName"];
            string surname = ExtractSurname(leadName);
            string debugContext = $"{FormatHolidayBookingContext(bookingReference, leadName, dateOfBirth)}, optionStatus='{optionStatus}', bookingStatus='{bookingStatus}'";

            HttpResponseMessage retrieveHolidayBookingResponse = GraphQlEndpoint
                .RetrieveHolidayBooking(env, "HOLIDAYS", bookingReference, dateOfBirth, surname)
                .EnsureGQLResponseSuccess();

            string retrieveHolidayBookingBody = await retrieveHolidayBookingResponse.Content.ReadAsStringAsync();
            Assert.False(retrieveHolidayBookingBody.IsNullOrWhiteSpace(), $"RetrieveHolidayBooking response body was empty. {debugContext}");

            JObject retrieveHolidayBookingResult = JObject.Parse(retrieveHolidayBookingBody);
            Assert.False(DynamicHelper.HasProperty(retrieveHolidayBookingResult, "errors"), $"Expected no GraphQL errors in RetrieveHolidayBooking response. {debugContext}");

            string tripResourceId = retrieveHolidayBookingResult.RequireToken("data.retrieveHolidayBooking.tripResourceId")?.ToString() ?? string.Empty;
            Assert.False(tripResourceId.IsNullOrWhiteSpace(), $"tripResourceId should not be null or empty. {debugContext}");

            return (retrieveHolidayBookingResult, bookingReference, debugContext, tripResourceId);
        }

        public static void ValidatePaymentSection(JObject retrieveHolidayBooking, string debugContext)
        {
            JToken paymentToken = retrieveHolidayBooking["payment"];
            JObject payment = paymentToken as JObject;
            Assert.True(payment != null, $"retrieveHolidayBooking.payment should be an object. {debugContext}");

            Assert.True(!string.IsNullOrWhiteSpace(payment["currencyCode"]?.ToString()), $"payment.currencyCode should not be null or empty. {debugContext}");

            Assert.True(decimal.TryParse(payment["balanceDue"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _), $"payment.balanceDue should be numeric. Actual='{payment["balanceDue"]}'. {debugContext}");
            Assert.True(decimal.TryParse(payment["deposit"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _), $"payment.deposit should be numeric. Actual='{payment["deposit"]}'. {debugContext}");
            Assert.True(decimal.TryParse(payment["totalPaidAmount"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _), $"payment.totalPaidAmount should be numeric. Actual='{payment["totalPaidAmount"]}'. {debugContext}");
            Assert.True(decimal.TryParse(payment["totalPriceToBePaid"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _), $"payment.totalPriceToBePaid should be numeric. Actual='{payment["totalPriceToBePaid"]}'. {debugContext}");
            Assert.True(int.TryParse(payment["paidPercentage"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _), $"payment.paidPercentage should be an integer. Actual='{payment["paidPercentage"]}'. {debugContext}");

            string dueDate = payment["dueDate"]?.ToString();
            Assert.True(string.IsNullOrWhiteSpace(dueDate) || DateTime.TryParseExact(dueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
                 $"payment.dueDate should be in yyyy-MM-dd format when populated. Actual='{dueDate}'. {debugContext}");

            JToken? pastPaymentsToken = payment["pastPayments"];
            Assert.True(
                pastPaymentsToken == null || pastPaymentsToken.Type == JTokenType.Array,
                $"payment.pastPayments should be an array when populated. {debugContext}");

            if (pastPaymentsToken is not JArray pastPayments)
                return;


            foreach (JToken pastPaymentToken in pastPayments)
            {
                JObject pastPayment = pastPaymentToken as JObject;
                Assert.True(pastPayment != null, $"Each payment.pastPayments item should be an object. {debugContext}");

                Assert.True(!string.IsNullOrWhiteSpace(pastPayment["paymentMethodDescription"]?.ToString()), $"payment.pastPayments.paymentMethodDescription should not be null or empty. {debugContext}");
                Assert.True(!string.IsNullOrWhiteSpace(pastPayment["paymentMethodCode"]?.ToString()), $"payment.pastPayments.paymentMethodCode should not be null or empty. {debugContext}");
                Assert.True(decimal.TryParse(pastPayment["currencyAmount"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _), $"payment.pastPayments.currencyAmount should be numeric. Actual='{pastPayment["currencyAmount"]}'. {debugContext}");
                Assert.True(!string.IsNullOrWhiteSpace(pastPayment["paymentCurrency"]?.ToString()), $"payment.pastPayments.paymentCurrency should not be null or empty. {debugContext}");
                Assert.True(bool.TryParse(pastPayment["realised"]?.ToString(), out _), $"payment.pastPayments.realised should be boolean. Actual='{pastPayment["realised"]}'. {debugContext}");

                string realisedDate = pastPayment["realisedDate"]?.ToString();
                if (!string.IsNullOrWhiteSpace(realisedDate))
                {
                    Assert.True(TryParseStandardDateTime(realisedDate, out _),
                        $"payment.pastPayments.realisedDate should be a valid date/time. Actual='{realisedDate}'. {debugContext}");
                }

                string receiptDate = pastPayment["receiptDate"]?.ToString();
                if (!string.IsNullOrWhiteSpace(receiptDate))
                {
                    Assert.True(TryParseStandardDateTime(receiptDate, out _),
                        $"payment.pastPayments.receiptDate should be a valid date/time. Actual='{receiptDate}'. {debugContext}");
                }
            }
        }

        public static bool TryParseStandardDateTime(string value, out DateTime parsed)
        {
            parsed = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string[] formats = new[]
            {
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-ddTHH:mm:ss.fffZ",
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy H:mm:ss",
                "dd/MM/yyyy HH:mm",
                "dd/MM/yyyy H:mm",
                "M/dd/yyyy h:mm:ss tt",
                "M/d/yyyy h:mm:ss tt",
                "MM/dd/yyyy h:mm:ss tt",
                "MM/dd/yyyy HH:mm:ss"
            };

            if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime exact))
            {
                parsed = exact;
                return true;
            }

            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out parsed);
        }

        public static Dictionary<string, string> RetrieveBookingDetailsForPayment(string optionStatus, string departureDate)
        {
            List<string> preferredItems = new[] { HolidayBookingItem.GEN.ToString(), HolidayBookingItem.HTL.ToString(), HolidayBookingItem.FLT.ToString() }.ToList();
            List<IEnumerable<string>?> requiredItemAttempts = new()
            {
                preferredItems
            };

            requiredItemAttempts.AddRange(preferredItems.Select(item => new[] { item }));
            requiredItemAttempts.Add(null);

            Dictionary<string, string> fallbackResult = new();

            foreach (IEnumerable<string>? requiredItems in requiredItemAttempts)
            {
                Dictionary<string, string> bookingDetails = RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(
                    optionStatus: new[] { optionStatus },
                    requiredItems: requiredItems,
                    departureDate: departureDate);

                fallbackResult = bookingDetails;

                bool hasError = bookingDetails.TryGetValue("error", out string error) && !string.IsNullOrWhiteSpace(error);
                bool hasCoreFields = bookingDetails.TryGetValue("bookingReference", out string bookingReference) && !string.IsNullOrWhiteSpace(bookingReference) &&
                                     bookingDetails.TryGetValue("dateOfBirth", out string dateOfBirth) && !string.IsNullOrWhiteSpace(dateOfBirth) &&
                                     bookingDetails.TryGetValue("leadName", out string leadName) && !string.IsNullOrWhiteSpace(leadName);

                if (!hasError && hasCoreFields)
                {
                    return bookingDetails;
                }
            }

            return fallbackResult;
        }

        public static async Task<(JObject RetrieveHolidayBooking, string DebugContext)> RetrieveHolidayBookingForPaymentAsync(string env, string optionStatus)
        {
            string departureDate = GenerateFutureDepartureDate();
            Dictionary<string, string> bookingDetails = RetrieveBookingDetailsForPayment(optionStatus, departureDate);

            TestDataPrerequisiteHelper.ValidateHolidayDetailsOrSkip(bookingDetails, requireItems: false, requireDepartureDate: true);

            string bookingReference = bookingDetails["bookingReference"];
            string dateOfBirth = NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string leadName = bookingDetails["leadName"];
            string surname = ExtractSurname(leadName);
            string debugContext = $"{FormatHolidayBookingContext(bookingReference, leadName, dateOfBirth)}, optionStatus='{optionStatus}'";

            HttpResponseMessage response = GraphQlEndpoint.RetrieveHolidayBooking(env, "HOLIDAYS", bookingReference, dateOfBirth, surname)
                .EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(responseBody), $"RetrieveHolidayBooking response body was empty. {debugContext}");

            JObject result = JObject.Parse(responseBody);
            Assert.False(DynamicHelper.HasProperty(result, "errors"), $"Expected no errors in retrieveHolidayBooking response. {debugContext}");

            JObject retrieveHolidayBooking = (JObject)result.RequireToken("data.retrieveHolidayBooking");
            return (retrieveHolidayBooking, debugContext);
        }

        public static Dictionary<string, string> RetrieveHolidayBookingDetailsByOptionStatus(IEnumerable<string> optionStatuses, string departureDate, out string selectedOptionStatus)
        {
            selectedOptionStatus = string.Empty;
            Dictionary<string, string> fallbackResult = new();

            foreach (string optionStatus in optionStatuses)
            {
                Dictionary<string, string> bookingDetails = HolidaysHelper.RetrieveBookingDetailsForPayment(optionStatus, departureDate);
                fallbackResult = bookingDetails;

                bool hasError = bookingDetails.TryGetValue("error", out string? error) && !error.IsNullOrWhiteSpace();
                bool hasCoreFields = bookingDetails.TryGetValue("bookingReference", out string? bookingReference) && !bookingReference.IsNullOrWhiteSpace() &&
                                     bookingDetails.TryGetValue("dateOfBirth", out string? dateOfBirth) && !dateOfBirth.IsNullOrWhiteSpace() &&
                                     bookingDetails.TryGetValue("leadName", out string? leadName) && !leadName.IsNullOrWhiteSpace();

                if (!hasError && hasCoreFields)
                {
                    selectedOptionStatus = optionStatus;
                    return bookingDetails;
                }
            }

            selectedOptionStatus = string.Join("/", optionStatuses);
            return fallbackResult;
        }

        public static Dictionary<string, string> ExtractCookiesFromResponse(HttpResponseMessage response)
        {
            Dictionary<string, string> cookies = new(StringComparer.OrdinalIgnoreCase);
            List<string> rawSetCookieHeaders = [];

            if (response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? responseHeaders))
                rawSetCookieHeaders.AddRange(responseHeaders);

            if (response.Content != null && response.Content.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? contentHeaders))
                rawSetCookieHeaders.AddRange(contentHeaders);

            foreach (string rawHeader in rawSetCookieHeaders)
            {
                foreach (string cookiePart in rawHeader.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    int equalsIndex = cookiePart.IndexOf('=');

                    if (equalsIndex <= 0)
                        continue;

                    string cookieName = cookiePart[..equalsIndex].Trim();
                    string cookieValueWithAttributes = cookiePart[(equalsIndex + 1)..].Trim();
                    string cookieValue = cookieValueWithAttributes.Split(';', 2, StringSplitOptions.TrimEntries)[0];

                    if (cookieName.IsNullOrWhiteSpace() || cookieValue.IsNullOrWhiteSpace())
                        continue;

                    if (!cookies.ContainsKey(cookieName))
                        cookies[cookieName] = cookieValue;
                }
            }
            return cookies;
        }
    }
}
