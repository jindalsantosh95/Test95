using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.Holidays.GraphQL
{
    public class RetrieveHolidayBookingDisneyExtrasTests
    {
        private readonly string _env = Config.Environment;

        [SkippableFact]
        [Trait("Category", Traits.RetrieveHolidayDisneyRegression)]
        public async Task RetrieveHolidayBooking_DisneyExtraItem_MapsToExtraBookingTypeAndContainsDisneyDetails()
        {
            string[] optionStatus = new[] { "FIRM", "DEFINITE", "FULLY PAID" };
            List<string> requiredItems = [HolidayBookingItem.GEN.ToString(), HolidayBookingItem.HTL.ToString(), HolidayBookingItem.FLT.ToString()];
            string departureDate = HolidaysHelper.GenerateFutureDepartureDate();

            Dictionary<string, string> bookingDetails = RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(
                optionStatus: optionStatus,
                requiredItems: requiredItems,
                departureDate: departureDate,
                genericTypeContains: "DisneyTicket");

            TestDataPrerequisiteHelper.ValidateHolidayDetailsOrSkip(bookingDetails);

            string bookingReference = bookingDetails["bookingReference"];
            string dateOfBirth = HolidaysHelper.NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string leadName = bookingDetails["leadName"];
            string surname = HolidaysHelper.ExtractSurname(leadName);
            string genericType = bookingDetails.TryGetValue("genericType", out string value) ? value : string.Empty;
            string debugContext = $"{HolidaysHelper.FormatHolidayBookingContext(bookingReference, leadName, dateOfBirth)}, genericType='{genericType}'";

            Assert.True(
                !string.IsNullOrWhiteSpace(genericType) && genericType.Contains("DisneyTicket", StringComparison.OrdinalIgnoreCase),
                $"Precondition failed: expected storage genericType to contain 'DisneyTicket'. {debugContext}");

            HttpResponseMessage response = GraphQlEndpoint.RetrieveHolidayBooking(_env, "HOLIDAYS", bookingReference, dateOfBirth, surname)
                .EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(responseBody), $"RetrieveHolidayBooking response body was empty. {debugContext}");

            JObject result = JObject.Parse(responseBody);
            Assert.False(DynamicHelper.HasProperty(result, "errors"), $"Expected no errors in retrieveHolidayBooking response. {debugContext}");

            JToken products = result.RequireToken("data.retrieveHolidayBooking.product");
            List<JObject> disneyProducts = products
                .Children<JObject>()
                .Where(p => IsDisneyProduct(p.SelectToken("extraDetails")))
                .ToList();

            Assert.True(disneyProducts.Count > 0, $"Expected at least one Disney extra item in response but none were found. {debugContext}");

            foreach (JObject disneyProduct in disneyProducts)
            {
                string bookingType = disneyProduct.SelectToken("bookingType")?.ToString() ?? string.Empty;
                Assert.True(bookingType.Equals("Extra", StringComparison.OrdinalIgnoreCase),
                    $"Expected Disney product bookingType to be 'Extra' but got '{bookingType}'. {debugContext}");

                JToken extraDetails = disneyProduct.RequireToken("extraDetails");

                Assert.True(IsDisneyProduct(extraDetails), $"Expected Disney product extraDetails to contain Disney indicators (text containing 'disney' or isDisneyTicket==true) but got '{extraDetails}'. {debugContext}");
            }
        }

        private static bool IsDisneyProduct(JToken? extraDetailsToken)
        {
            if (extraDetailsToken == null || extraDetailsToken.Type == JTokenType.Null)
                return false;

            bool containsDisneyText = ExtraDetailsContainDisney(extraDetailsToken);
            bool isDisneyTicket = bool.TryParse(extraDetailsToken.SelectToken("isDisneyTicket")?.ToString(), out bool parsed) && parsed;

            return containsDisneyText || isDisneyTicket;
        }

        private static bool ExtraDetailsContainDisney(JToken extraDetailsToken)
        {
            if (extraDetailsToken.Type == JTokenType.String)
            {
                string? value = extraDetailsToken.Value<string>();
                return !string.IsNullOrWhiteSpace(value) && value.Contains("disney", StringComparison.OrdinalIgnoreCase);
            }

            if (extraDetailsToken is JProperty property)
                return ExtraDetailsContainDisney(property.Value);

            else if (extraDetailsToken is JContainer container)
                return container.Children().Any(ExtraDetailsContainDisney);

            return false;
        }
    }
}
