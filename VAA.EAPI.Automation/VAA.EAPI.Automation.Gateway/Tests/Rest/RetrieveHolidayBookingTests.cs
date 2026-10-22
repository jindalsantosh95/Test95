using System.Globalization;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using RestEndpoint = VAA.EAPI.Automation.Gateway.Requests.Endpoint.Rest;

namespace VAA.EAPI.Automation.Gateway.Tests.Rest
{
    public class RetrieveHolidayBookingTests
    {
        public string env = Config.Environment;
        private const string CruisesCompany = "Virgin Holidays Cruises";
        private const string CancelledBookingStatus = "CANCELLED";

        [Fact]
        public async Task TC01_RetrieveHolidayBooking_ReturnsSuccess()
        {
            string[] optionStatus = new[] { "FIRM", "DEFINITE", "FULLY PAID" };
            string[] requiredItemNames = new[] { "GEN", "HTL", "FLT" };
            string departureDate = GenerateFutureDepartureDate();

            Dictionary<string, string> holidayBookingDetails =
                RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(
                    optionStatus: optionStatus,
                    requiredItems: requiredItemNames,
                    departureDate: departureDate
                );

            if (holidayBookingDetails.TryGetValue("error", out string? error))
            {
                Assert.Fail($"Test case failed: {error}");
            }

            string bookingReference = holidayBookingDetails["bookingReference"];
            string dateOfBirth = NormalizeDateOfBirth(holidayBookingDetails["dateOfBirth"]);
            string surname = ExtractSurname(holidayBookingDetails["leadName"]);

            Assert.True(
                !string.IsNullOrWhiteSpace(bookingReference) &&
                !string.IsNullOrWhiteSpace(dateOfBirth) &&
                !string.IsNullOrWhiteSpace(surname),
                "bookingReference, dateOfBirth, and surname should not be null or empty");

            HttpResponseMessage response = RestEndpoint.RetrieveHolidayBooking(
                env,
                bookingReference: bookingReference,
                dateOfBirth: dateOfBirth,
                surname: surname
            );
            response.EnsureIsSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(responseBody), "RetrieveHolidayBooking response body was empty");
            JObject bookingResponse = JObject.Parse(responseBody);

            // Response contract assertions (based on the sample response shape).
            string status = bookingResponse.RequireToken("status").ToString();
            Assert.False(string.IsNullOrWhiteSpace(status), "status should be populated");

            string returnedId = bookingResponse.RequireToken("id").ToString();
            Assert.True(returnedId == bookingReference,
                $"Expected id to match bookingReference. Expected: {bookingReference} Actual: {returnedId}");

            JToken productToken = bookingResponse.RequireToken("product");
            Assert.NotNull(productToken);

            // The GraphQL payload requests `product { ... }` (object), but some older fixtures/tests
            // assumed an array. Support both shapes to keep the assertions stable across schema drift.
            JObject product = productToken switch
            {
                JObject productObject => productObject,
                JArray productArray when productArray.Count > 0 => SelectFlightProduct(productArray),
                _ => throw new Newtonsoft.Json.JsonException($"Unexpected 'product' shape. Expected object/array but was '{productToken.Type}'.")
            };

            JToken slicesArray = product.RequireToken("slices");
            int slicesCount = slicesArray.Count();
            Assert.True(slicesCount > 0, $"Expected at least 1 slice but found {slicesCount}");

            JToken segmentsArray = product.RequireToken("slices[0].segments");
            int segmentsCount = segmentsArray.Count();
            Assert.True(segmentsCount > 0, $"Expected at least 1 segment but found {segmentsCount}");

            JToken legsArray = product.RequireToken("slices[0].segments[0].legs");
            int legsCount = legsArray.Count();
            Assert.True(legsCount > 0, $"Expected at least 1 leg but found {legsCount}");

            JToken passengersArray = product.RequireToken("passengers");
            int passengersCount = passengersArray.Count();
            Assert.True(passengersCount > 0, $"Expected at least 1 passenger but found {passengersCount}");
        }

        private static JObject SelectFlightProduct(JArray productArray)
        {
            foreach (JToken token in productArray)
            {
                if (token is not JObject productObject)
                {
                    continue;
                }

                if (productObject.TryGetValue("slices", StringComparison.OrdinalIgnoreCase, out JToken? slicesToken) &&
                    slicesToken is JArray slicesArray &&
                    slicesArray.Count > 0)
                {
                    return productObject;
                }
            }

            if (productArray[0] is JObject first)
            {
                return first;
            }

            throw new Newtonsoft.Json.JsonException("Unexpected 'product' array contents. Expected at least one object.");
        }

        private static string ExtractSurname(string leadName)
        {
            if (string.IsNullOrWhiteSpace(leadName))
                return string.Empty;

            string[] parts = leadName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? string.Empty : parts[^1];
        }

        private static string NormalizeDateOfBirth(string dateOfBirth)
        {
            if (string.IsNullOrWhiteSpace(dateOfBirth))
                return string.Empty;

            if (DateTime.TryParseExact(dateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (DateTime.TryParse(dateOfBirth, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fallbackDate))
            {
                return fallbackDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            return dateOfBirth;
        }

        private static string GenerateFutureDepartureDate(int daysInFuture = 7)
        {
            DateTime futureDateBase = DateTime.UtcNow.AddDays(daysInFuture);
            return futureDateBase.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }
    }
}
