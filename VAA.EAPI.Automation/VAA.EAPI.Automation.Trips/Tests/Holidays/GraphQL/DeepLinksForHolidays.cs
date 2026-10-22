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
    public class DeepLinksForHolidays
    {
        private readonly string _env = Config.Environment;
        private static readonly string OcpSubKey = Vault.Default.GetSecretValue("TestingTripsGraphQLSecureSubscriptionKey");

        [SkippableFact]
        [Trait("Category", Traits.DeepLinksHolidaysRegression)]
        [Trait("Category", Traits.RetrieveHolidayBookingRegression)]
        public async Task ManageHoliday_SecureDeepLink_ReturnsExpectedUrl()
        {
            string[] optionStatus = new[] { "FIRM", "DEFINITE", "FULLY PAID" };
            List<string> requiredItems = new()
            {
                HolidayBookingItem.GEN.ToString(),
                HolidayBookingItem.HTL.ToString(),
                HolidayBookingItem.FLT.ToString()
            };
            string departureDate = HolidaysHelper.GenerateFutureDepartureDate();

            Dictionary<string, string> bookingDetails = RetrieveFromStorageTable.RetrieveHolidayBookingDetailsFromStorageTable(
                optionStatus: optionStatus,
                requiredItems: requiredItems,
                departureDate: departureDate);

            ValidateHolidayDetails(bookingDetails);

            string bookingReference = bookingDetails["bookingReference"];
            string dateOfBirth = HolidaysHelper.NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string leadName = bookingDetails["leadName"];
            string surname = HolidaysHelper.ExtractSurname(leadName);
            string debugContext = HolidaysHelper.FormatHolidayBookingContext(bookingReference, leadName, dateOfBirth);

            HttpResponseMessage retrieveHolidayBookingResponse = GraphQlEndpoint.RetrieveHolidayBooking(_env, "HOLIDAYS", bookingReference, dateOfBirth, surname).EnsureGQLResponseSuccess();

            string retrieveHolidayBookingResponseBody = await retrieveHolidayBookingResponse.Content.ReadAsStringAsync();
            Assert.True(!string.IsNullOrWhiteSpace(retrieveHolidayBookingResponseBody), $"RetrieveHolidayBooking response body was empty. {debugContext}");

            JObject retrieveHolidayBookingResult = JObject.Parse(retrieveHolidayBookingResponseBody);
            JToken retrieveHolidayBooking = retrieveHolidayBookingResult.RequireToken("data.retrieveHolidayBooking");
            string tripResourceId = retrieveHolidayBooking.RequireToken("tripResourceId").ToString();
            Assert.True(!string.IsNullOrWhiteSpace(tripResourceId), $"Trip resource id should not be null or empty. {debugContext}");

            string deeplinkOptionString = "ManageHoliday";
            HttpResponseMessage getDeepLinksResponse = GraphQlEndpoint.GetDeepLinksResponse(_env, tripResourceId, deeplinkOptionString).EnsureGQLResponseSuccess();

            string getDeepLinksResponseBody = await getDeepLinksResponse.Content.ReadAsStringAsync();
            Assert.True(!string.IsNullOrWhiteSpace(getDeepLinksResponseBody), $"DeepLinks response body was empty. {debugContext}");

            JObject getDeepLinksResult = JObject.Parse(getDeepLinksResponseBody);
            JToken deepLinkDetails = getDeepLinksResult.RequireToken("data.Deeplink");

            string deeplinkUrl = deepLinkDetails.RequireToken("deeplinkUrl").ToString();
            Assert.True(!string.IsNullOrWhiteSpace(deeplinkUrl), $"DeeplinkUrl should not be null or empty. {debugContext}");
            Assert.True(deeplinkUrl.StartsWith("https://"), $"DeeplinkUrl should start with https://. {debugContext}");

            string deeplinkOptionFromResponse = deepLinkDetails.RequireToken("deeplinkOption").ToString();
            Assert.True(deeplinkOptionString.Equals(deeplinkOptionFromResponse, StringComparison.Ordinal),
                $"Expected deeplinkOption to be {deeplinkOptionString} but got {deeplinkOptionFromResponse}. {debugContext}");

            string cacheId = string.Empty;
            if (deeplinkUrl.Contains("id=", StringComparison.OrdinalIgnoreCase))
            {
                Uri uri = new(deeplinkUrl);
                System.Collections.Specialized.NameValueCollection query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                cacheId = query["id"] ?? string.Empty;
            }

            Assert.True(!string.IsNullOrWhiteSpace(cacheId), $"Cache ID should not be null or empty. {debugContext}");

            HttpResponseMessage getDeepLinksSecureResponse = GraphQlEndpoint
                .GetDeepLinksSecureResponse(_env, cacheId, OcpSubKey)
                .EnsureGQLResponseSuccess();

            string getDeepLinksSecureResponseBody = await getDeepLinksSecureResponse.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(getDeepLinksSecureResponseBody), $"DeepLinks secure response body was empty. {debugContext}");

            JObject getDeepLinksSecureResult = JObject.Parse(getDeepLinksSecureResponseBody);
            JToken endpointDetails = getDeepLinksSecureResult.RequireToken("data.GetDeepLinkForPost.endpoint");

            string endpointUrl = endpointDetails.RequireToken("endpointUrl").ToString();
            Assert.True(!string.IsNullOrWhiteSpace(endpointUrl), $"EndpointUrl should not be null or empty. {debugContext}");
            Assert.True(endpointUrl.StartsWith("https://"), $"EndpointUrl should start with https://. {debugContext}");

            string httpMethod = endpointDetails.RequireToken("http_Method").ToString();
            Assert.True("GET".Equals(httpMethod, StringComparison.Ordinal), $"Expected http_Method to be GET but got {httpMethod}. {debugContext}");
        }

        private static void ValidateHolidayDetails(Dictionary<string, string> bookingDetails)
            => TestDataPrerequisiteHelper.ValidateHolidayDetailsOrSkip(bookingDetails, requireItems: true, requireDepartureDate: true);

    }
}
