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
    public class RetrieveHolidayBookingCarComponentTests
    {
        private readonly string _env = Config.Environment;

        [SkippableFact]
        [Trait("Category", Traits.RetrieveHolidayBookingRegression)]
        public async Task RetrieveHolidayBooking_CarComponent_MapsDatesAndModel()
        {
            string[] optionStatus = new[] { "FIRM", "DEFINITE", "FULLY PAID" };
            List<string> requiredItems = new() { HolidayBookingItem.GEN.ToString(), HolidayBookingItem.HTL.ToString(), HolidayBookingItem.FLT.ToString(), HolidayBookingItem.CAR.ToString() };
            string departureDate = HolidaysHelper.GenerateFutureDepartureDate();

            Dictionary<string, string> bookingDetails = RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(
                optionStatus: optionStatus,
                requiredItems: requiredItems,
                departureDate: departureDate);

            TestDataPrerequisiteHelper.ValidateHolidayDetailsOrSkip(bookingDetails);

            string bookingReference = bookingDetails["bookingReference"];
            string dateOfBirth = HolidaysHelper.NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string surname = HolidaysHelper.ExtractSurname(bookingDetails["leadName"]);
            string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, bookingDetails["leadName"], dateOfBirth);

            HttpResponseMessage response = GraphQlEndpoint
                .RetrieveHolidayBooking(_env, "HOLIDAYS", bookingReference, dateOfBirth, surname)
                .EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(responseBody), $"RetrieveHolidayBooking response body was empty. {debugContext}");

            JObject result = JObject.Parse(responseBody);
            JToken products = result.RequireToken("data.retrieveHolidayBooking.product");

            List<JObject> carProducts = products
                .Children<JObject>()
                .Where(p => "CAR".Equals(p.SelectToken("bookingType")?.ToString(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.True(carProducts.Count > 0, $"Expected at least one product with bookingType='CAR' but none were returned. {debugContext}");

            bool anyStartDateLocalPopulated = false;
            bool anyEndDateLocalPopulated = false;

            foreach (JObject carProduct in carProducts)
            {
                JToken carDetailsToken = carProduct.RequireToken("carDetails");
                JObject? carDetails = carDetailsToken as JObject;
                Assert.True(carDetails != null, $"carDetails is not a JObject. DebugContext: {debugContext}");

                string name = carDetails.RequireToken("name").ToString();
                Assert.False(string.IsNullOrWhiteSpace(name), $"Expected carDetails.name to be populated. {debugContext}");

                string model = carDetails.RequireToken("model").ToString();
                Assert.False(string.IsNullOrWhiteSpace(model), $"Expected carDetails.model to be populated. {debugContext}");

                string? startDateLocal = carDetails.SelectToken("startDateLocal")?.ToString();
                if (!string.IsNullOrWhiteSpace(startDateLocal))
                {
                    anyStartDateLocalPopulated = true;
                    Assert.True(
                        HolidaysHelper.TryParseStandardDateTime(startDateLocal, out _),
                        $"carDetails.startDateLocal was not a valid date/time: '{startDateLocal}'. {debugContext}");
                }

                string? endDateLocal = carDetails.SelectToken("endDateLocal")?.ToString();
                if (!string.IsNullOrWhiteSpace(endDateLocal))
                {
                    anyEndDateLocalPopulated = true;
                    Assert.True(
                        HolidaysHelper.TryParseStandardDateTime(endDateLocal, out _),
                        $"carDetails.endDateLocal was not a valid date/time: '{endDateLocal}'. {debugContext}");
                }
            }
            Assert.True(anyStartDateLocalPopulated, $"Expected at least one CAR product to have carDetails.startDateLocal populated. {debugContext}");
            Assert.True(anyEndDateLocalPopulated, $"Expected at least one CAR product to have carDetails.endDateLocal populated. {debugContext}");
        }
    }
}
